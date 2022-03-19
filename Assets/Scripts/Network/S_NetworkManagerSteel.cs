using Mirror;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror
{
    [AddComponentMenu("")]
    public class S_NetworkManagerSteel : NetworkManager
    {
        [Header("Game settings")]
        public float GameTime = 180f;
        public float PreMatchPlacementTime = 15f;
        public int InGameWeightMax = 15;
        private float RemainingTime = 0f;
        private bool timerisRunning = false;
        

        [Header("Game process")]
        [SerializeField]
        private SO_UnitsToPlay unitsData;
        [SerializeField]
        private List<GameObject> firstPlayerBattleUnits = new List<GameObject>();
        [SerializeField]
        private List<GameObject> secondPlayerBattleUnits = new List<GameObject>();

        [SerializeField]
        //private List<SO_UnitItemData> firstPlayerUnits = new List<SO_UnitItemData>();
        private List<int> firstPlayerUnits = new List<int>();
        private int firstPlayerWeight = 0;
        private bool firstCanPlace = false;
        private int firstPlayerWins = 0;

        [SerializeField]
        //private List<SO_UnitItemData> SecondPlayerUnits = new List<SO_UnitItemData>();
        private List<int> SecondPlayerUnits = new List<int>();
        private int SecondPlayerWeight = 0;
        private bool secondCanPlace = false;
        private int secondPlayerWins = 0;

        private bool firstPlayerPlacing = true;


        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;

        public List<S_GamePlayer> InGamePlayers { get; } = new List<S_GamePlayer>();

        private enum MatchState
        { 
            PlayerWaitingState,
            UnitPlacementState,
            BattleState,
            BattleStartingState,
            BattleEndingState,
            AfterMatchState
        }

        private MatchState matchState = MatchState.PlayerWaitingState;
        


        //Server start, stop, add player, connect client, disconnect client
        public override void OnStartServer()
        {

            spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
        }

        public override void OnStartClient()
        {
            var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");
            foreach (var prefab in spawnablePrefabs)
                NetworkClient.RegisterPrefab(prefab);
        }

        public override void OnClientConnect()//OnClientConnect(NetworkClient.)
        {
            Debug.Log("ClientConnected");
            base.OnClientConnect();

            OnClientConnected?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            Debug.Log("ClientDisonnected");
            base.OnClientDisconnect();

            OnClientDisconnected?.Invoke();
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            if(conn.identity != null)
            {
                var player = conn.identity.GetComponent<S_GamePlayer>();
                InGamePlayers.Remove(player);

                //Notify players for debug
                NotifyPlayersofReadyState();
                //Check game state, if in prematch state = abort match with no results
                //if in match and mathc time > 30 seconds then disconnected player is lost
                //if in match results then write match results in database
            }
            base.OnServerDisconnect(conn);
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            //Debug.Log("AddPlayer");
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);
            var playerpref = player.GetComponent<S_GamePlayer>();

            playerpref.SetupSpawnAreaClientRPC(InGamePlayers.Count);
        }

        public override void OnStopServer()
        {
            //Write results of match?
            InGamePlayers.Clear();
        }
        
        //Lobby functions

        public void NotifyPlayersofReadyState()
        {
           // foreach (var player in InGamePlayers)
           // {
                //player.HandleReadyToStart(IsReadyToStart());
           // }
        }

        private bool IsReadyToStart()
        {
            //Check for number of players to be ready
            //and check for every player to be ready for start

            if (numPlayers < 2) return false;
            
            foreach (var player in InGamePlayers)
                if (!player.IsReady) return false;
            

            return true;
        }

        public void StartMatch()
        {
            if (!IsReadyToStart()) return;

            matchState = MatchState.UnitPlacementState;

            RemainingTime = PreMatchPlacementTime;
            timerisRunning = true;

            firstCanPlace = true;
            secondCanPlace = true;
            firstPlayerPlacing = true;

            firstPlayerBattleUnits.Clear();
            secondPlayerBattleUnits.Clear();

            firstPlayerWeight = 0;
            SecondPlayerWeight = 0;

            List<int> unitsToSend = new List<int>();
            unitsToSend = firstPlayerUnits.ToList();
            InGamePlayers[0].StartPreMatchStep(RemainingTime, true, unitsToSend, true, InGameWeightMax, true);

            unitsToSend.Clear();

            unitsToSend = SecondPlayerUnits.ToList();
            InGamePlayers[1].StartPreMatchStep(RemainingTime, true, unitsToSend, false, InGameWeightMax, true);
            unitsToSend.Clear();

            Debug.Log("Match started! Place phase!");
        }
        //Game functions

        public void PlaceUnit(int playerid,int Unitid, Vector3 spawnplace)
        {
            if (playerid == 0)
            {
                int unitid = firstPlayerUnits[Unitid];
                if (firstPlayerWeight + unitsData.UnitsData[unitid].GetWeight() > InGameWeightMax) return;
                Debug.Log("First player spawn unit = " + Quaternion.identity.eulerAngles);
                GameObject unitObj = Instantiate(unitsData.UnitsData[unitid].prefab, spawnplace, Quaternion.identity);
                NetworkServer.Spawn(unitObj);

                unitObj.GetComponent<S_Unit>().SetData(0, unitsData.UnitsData[unitid].GetMaxHealth(), unitsData.UnitsData[unitid].GetMinDamage(), unitsData.UnitsData[unitid].GetMaxDamage());
                unitObj.name = "FirstPlayerUnit" + firstPlayerBattleUnits.Count;
                InGamePlayers[0].TargetRpcRemoveUnitFromHand(Unitid);
                firstPlayerWeight += unitsData.UnitsData[unitid].GetWeight();
                firstPlayerUnits.RemoveAt(Unitid);

                firstPlayerBattleUnits.Add(unitObj);
            }
            else if (playerid == 1)
            {
                int unitid = SecondPlayerUnits[Unitid];
                if (SecondPlayerWeight + unitsData.UnitsData[unitid].GetWeight() > InGameWeightMax) return;

                Quaternion rot = Quaternion.identity;
                rot.eulerAngles = new Vector3(rot.eulerAngles.x, 180, rot.eulerAngles.z);
                Debug.Log("Second player spawn unit = " + rot.eulerAngles);
                GameObject unitObj = Instantiate(unitsData.UnitsData[unitid].prefab, spawnplace, rot);
                NetworkServer.Spawn(unitObj);

                unitObj.GetComponent<S_Unit>().SetData(1, unitsData.UnitsData[unitid].GetMaxHealth(), unitsData.UnitsData[unitid].GetMinDamage(), unitsData.UnitsData[unitid].GetMaxDamage());
                unitObj.name = "SecondPlayerUnit" + secondPlayerBattleUnits.Count;
                InGamePlayers[1].TargetRpcRemoveUnitFromHand(Unitid);
                SecondPlayerWeight += unitsData.UnitsData[unitid].GetWeight();
                SecondPlayerUnits.RemoveAt(Unitid);

                secondPlayerBattleUnits.Add(unitObj);
            }
        }

        [Server]
        public void ServerPlaceUnit(NetworkConnection conn, int idToPlace, Vector3 placeToSpawn)
        {
            //add validation to place and unit type
            if(InGamePlayers[0].connectionToClient == conn)
                PlaceUnit(0, idToPlace, placeToSpawn);
            else if (InGamePlayers[1].connectionToClient == conn)
                PlaceUnit(1, idToPlace, placeToSpawn);
              
            CalcTurnOrder();
        }

        [Server]
        public void ServerPlaceUnitSelf(int playerid)
        {
            System.Random rand = new System.Random();
            int randId;
            Vector3 spawnPlace = new Vector3(rand.Next(0, 40) - 20, 1, 0);

            if (playerid == 0)
            {
                randId = rand.Next(0,firstPlayerUnits.Count);
                spawnPlace.z = rand.Next(0, 4) - 20;

                PlaceUnit(0, randId, spawnPlace);
            }
            else if (playerid == 1)
            {
                randId = rand.Next(0, SecondPlayerUnits.Count);
                spawnPlace.z = rand.Next(16, 20);

                PlaceUnit(1, randId, spawnPlace);      
            }

            CalcTurnOrder();
        }

        //
        //Timer
        [ServerCallback]
        private void Update()
        {
            if (timerisRunning)
            {
                if (RemainingTime > 0)
                {
                    RemainingTime -= Time.deltaTime;
                }
                else
                {
                    //Timer end event
                    Debug.Log("Timer ended!");
                    RemainingTime = 0f;
                    timerisRunning = false;

                    if(matchState == MatchState.UnitPlacementState)
                    {
                        if (firstPlayerPlacing && firstCanPlace)
                        {
                            if (firstPlayerWeight == 0)
                                ServerPlaceUnitSelf(0);
                            else 
                                passTurnPlayerServer(0);

                        }
                        else if (!firstPlayerPlacing && secondCanPlace)
                        {
                            if (secondCanPlace && SecondPlayerWeight == 0)
                                ServerPlaceUnitSelf(1);
                            else
                             passTurnPlayerServer(1);
                        }
                        else if(!firstPlayerPlacing && !secondCanPlace)
                        {
                            matchState = MatchState.BattleStartingState;
                            RemainingTime = 3f;
                            InGamePlayers[0].UpdateGameDisplayUI(RemainingTime, true, false, false, false);
                            InGamePlayers[1].UpdateGameDisplayUI(RemainingTime, true, false, false, false);
                        }
                    }
                    else if (matchState == MatchState.BattleStartingState)
                    {
                        matchState = MatchState.BattleState;

                        InGamePlayers[0].UpdateGameDisplayUI(GameTime, true, false, false ,false);
                        InGamePlayers[1].UpdateGameDisplayUI(GameTime, true, false, false, false);

                        RemainingTime = GameTime;
                        timerisRunning = true;

                        foreach (GameObject unit in firstPlayerBattleUnits) unit.GetComponent<S_Unit>().StartBehaviour();
                        foreach (GameObject unit in secondPlayerBattleUnits) unit.GetComponent<S_Unit>().StartBehaviour();
                    }
                    else if(matchState == MatchState.BattleEndingState)
                    {
                        StartMatch();
                    }
                    else if(matchState == MatchState.AfterMatchState)
                    {
                        Application.Quit();
                    }
                }
            }
        }

        //Load player Data functions

        [Server]
        public void ServerGetPlayerUnits(NetworkConnection conn, List<int> UnitsIds)
        {
            if(InGamePlayers[0].connectionToClient == conn)
                firstPlayerUnits = UnitsIds.ToList();
            else if(InGamePlayers[1].connectionToClient == conn)
                SecondPlayerUnits = UnitsIds.ToList();
        }

        [Server]
        public void passTurnPlayer(NetworkConnection conn)
        {
            if (InGamePlayers[0].connectionToClient == conn)
                firstPlayerWeight = InGameWeightMax;
            else if (InGamePlayers[1].connectionToClient == conn)
                SecondPlayerWeight = InGameWeightMax;

            CalcTurnOrder();
        }

        [Server]
        public void passTurnPlayerServer(int playerId)
        {
            if (playerId == 0)
                firstPlayerWeight = InGameWeightMax;
            else if (playerId == 1)
                SecondPlayerWeight = InGameWeightMax;

            CalcTurnOrder();
        }

        public void CalcTurnOrder()
        {
            RemainingTime = PreMatchPlacementTime;
            timerisRunning = true;

            firstPlayerPlacing = !firstPlayerPlacing;

            firstCanPlace = false;
            secondCanPlace = false;

            //Check first
            foreach (var unitid in firstPlayerUnits)
                if (firstPlayerWeight + unitsData.UnitsData[unitid].GetWeight() <= InGameWeightMax) firstCanPlace = true;

            List<int> unitsToSend = new List<int>();
            unitsToSend = firstPlayerUnits.ToList();
            InGamePlayers[0].StartPreMatchStep(RemainingTime, true, unitsToSend, !firstCanPlace ? false : firstPlayerPlacing, InGameWeightMax, false);

            firstPlayerPlacing = firstCanPlace ? firstPlayerPlacing : false;
            
            //Check second
            foreach (var unitid in SecondPlayerUnits)
                if (SecondPlayerWeight + unitsData.UnitsData[unitid].GetWeight() <= InGameWeightMax) secondCanPlace = true;

            unitsToSend.Clear();
            unitsToSend = SecondPlayerUnits.ToList();
            InGamePlayers[1].StartPreMatchStep(RemainingTime, true, unitsToSend, !secondCanPlace ? false : !firstPlayerPlacing, InGameWeightMax, false);
            unitsToSend.Clear();

            if (!firstCanPlace && !secondCanPlace)
            {
                matchState = MatchState.BattleStartingState;
                RemainingTime = 3f;
                InGamePlayers[0].UpdateGameDisplayUI(RemainingTime, true, false, false, false);
                InGamePlayers[1].UpdateGameDisplayUI(RemainingTime, true, false, false, false);
            }
        }

        public List<GameObject> GetBattlePlayerUnits(int playerid)
        {
            return (playerid == 0) ? firstPlayerBattleUnits : secondPlayerBattleUnits;
        }

        [Server]
        public void RemoveBattleUnit(int teamId, GameObject unit)
        {
            Debug.Log("Killed = " + unit.name);
            if (teamId == 0)
            {
                firstPlayerBattleUnits.Remove(unit);
                Destroy(unit);
            }
            else if (teamId == 1)
            {
                secondPlayerBattleUnits.Remove(unit);
                Destroy(unit);
            }

            int firstLeftUnits = firstPlayerBattleUnits.Count;
            int secondLeftUnits = secondPlayerBattleUnits.Count;

            if (firstLeftUnits == 0 || secondLeftUnits == 0)
            {
                Debug.Log("Round ended!");
                if (firstLeftUnits == 0) secondPlayerWins++;
                else if (secondLeftUnits == 0) firstPlayerWins++;

                if (secondPlayerWins == 2 || firstPlayerWins == 2)
                {
                    timerisRunning = true;
                    RemainingTime = 30f;
                    matchState = MatchState.AfterMatchState;
                    this.CallWithDelay(DestroyAllUnits, 2.5f);

                    if (secondPlayerWins == 2)
                    {
                        InGamePlayers[0].UpdateGameDisplayUI(RemainingTime, true, false, true, false);
                        InGamePlayers[1].UpdateGameDisplayUI(RemainingTime, true, false, true, true);
                    }
                    else if(firstPlayerWins == 2)
                    {
                        InGamePlayers[0].UpdateGameDisplayUI(RemainingTime, true, false, true, true);
                        InGamePlayers[1].UpdateGameDisplayUI(RemainingTime, true, false, true, false);
                    }
                }
                else
                {
                    Debug.Log("Units clearing!");
                    matchState = MatchState.BattleEndingState;
                    timerisRunning = true;
                    RemainingTime = 3f;
                    InGamePlayers[0].UpdateGameDisplayUI(RemainingTime, true, false, false, false);
                    InGamePlayers[1].UpdateGameDisplayUI(RemainingTime, true, false, false, false);
                    this.CallWithDelay(DestroyAllUnits, 2.5f);
                }
                return;
            }     
            //Check for round ending
        }

        [Server]
        public void DestroyAllUnits()
        {
            List<GameObject> tempUnits = firstPlayerBattleUnits.ToList();

            if (firstPlayerBattleUnits.Count > 0)
                foreach (GameObject unit in tempUnits)
                {
                    GameObject unitobj = unit;
                    firstPlayerBattleUnits.Remove(unit);
                    Destroy(unitobj);
                }

            tempUnits = secondPlayerBattleUnits.ToList();

            if (secondPlayerBattleUnits.Count > 0)
                foreach (GameObject unit in tempUnits)
                {
                    GameObject unitobj = unit;
                    secondPlayerBattleUnits.Remove(unit);
                    Destroy(unitobj);
                }
        }

        //public Transform leftRacketSpawn;
        //public Transform rightRacketSpawn;
        //GameObject ball;

        //public override void OnServerAddPlayer(NetworkConnection conn)
        //{
        //    // add player at correct spawn position
        //    Transform start = numPlayers == 0 ? leftRacketSpawn : rightRacketSpawn;
        //    GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        //    NetworkServer.AddPlayerForConnection(conn, player);

        //    // spawn ball if two players
        //    if (numPlayers == 2)
        //    {
        //        ball = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Ball"));
        //        NetworkServer.Spawn(ball);
        //    }
        //}

        //public override void OnServerDisconnect(NetworkConnection conn)
        //{
        //    // destroy ball
        //    if (ball != null)
        //        NetworkServer.Destroy(ball);

        //    // call base functionality (actually destroys the player)
        //    base.OnServerDisconnect(conn);
        //}
    }
}
