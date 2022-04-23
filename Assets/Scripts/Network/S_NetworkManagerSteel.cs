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
        [Header("Server settings")]
        [Scene] [SerializeField] private List<string> onlineScenes;

        [Header("Game settings")]
        [SerializeField] private float GameTime = 180f;
        [SerializeField] private float PreMatchPlacementTime = 15f;
        [SerializeField] private int InGameWeightMax = 15;
        private float RemainingTime = 0f;
        private bool timerisRunning = false;
        [SerializeField] private SO_UnitsToPlay unitsData;

        [Header("Game process")]
        [SerializeField]
        private List<GameObject> firstPlayerBattleUnits = new List<GameObject>();
        private int firstPlayerWeight = 0;
        private bool firstCanPlace = false;
        private int firstPlayerWins = 0;

        [SerializeField]
        private List<GameObject> secondPlayerBattleUnits = new List<GameObject>();
        private int SecondPlayerWeight = 0;
        private bool secondCanPlace = false;
        private int secondPlayerWins = 0;

        private bool firstPlayerPlacing = true;

        [SerializeField]
        private List<int> firstPlayerUnits = new List<int>();
        
        [SerializeField]
        private List<int> SecondPlayerUnits = new List<int>();

        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;

        public List<S_GamePlayer> InGamePlayers { get; private set; } = new List<S_GamePlayer>();

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

        public override void Start()
        {
            if (onlineScenes.Count == 0) throw new ArgumentNullException("Online scenes count is zero!");

            System.Random random = new System.Random();
            onlineScene = onlineScenes[random.Next(0, onlineScenes.Count)];

#if UNITY_SERVER
            if (autoStartServerBuild)
            {
                StartServer();
            }
#endif
        }

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

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            if(conn.identity != null)
            {
                var player = conn.identity.GetComponent<S_GamePlayer>();
                InGamePlayers.Remove(player);

                //Notify players for debug
               // NotifyPlayersofReadyState();
                //Check game state, if in prematch state = abort match with no results
                //if in match and mathc time > 30 seconds then disconnected player is lost
                //if in match results then write match results in database
            }
            base.OnServerDisconnect(conn);
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

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

                unitObj.GetComponent<S_Unit>().SetData(0, unitsData.UnitsData[unitid].GetMaxHealth(), unitsData.UnitsData[unitid].GetMinDamage(), unitsData.UnitsData[unitid].GetMaxDamage(), unitsData.UnitsData[unitid].GetSizeType());
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

                unitObj.GetComponent<S_Unit>().SetData(1, unitsData.UnitsData[unitid].GetMaxHealth(), unitsData.UnitsData[unitid].GetMinDamage(), unitsData.UnitsData[unitid].GetMaxDamage(), unitsData.UnitsData[unitid].GetSizeType());
                unitObj.name = "SecondPlayerUnit" + secondPlayerBattleUnits.Count;
                InGamePlayers[1].TargetRpcRemoveUnitFromHand(Unitid);
                SecondPlayerWeight += unitsData.UnitsData[unitid].GetWeight();
                SecondPlayerUnits.RemoveAt(Unitid);

                secondPlayerBattleUnits.Add(unitObj);
            }
        }
        //
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
        //
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
                else if(RemainingTime <= 0)
                {
                    //Timer end event
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
                            timerisRunning = true;
                            InGamePlayers[0].UpdateGameDisplayUI(RemainingTime, true, false, true);
                            InGamePlayers[1].UpdateGameDisplayUI(RemainingTime, true, false, true);
                        }
                    }
                    else if (matchState == MatchState.BattleStartingState)
                    {
                        matchState = MatchState.BattleState;

                        RemainingTime = GameTime;
                        timerisRunning = true;

                        InGamePlayers[0].UpdateGameDisplayUI(RemainingTime, true, false, true);
                        InGamePlayers[1].UpdateGameDisplayUI(RemainingTime, true, false, true);

                        foreach (GameObject unit in firstPlayerBattleUnits) unit.GetComponent<S_Unit>().StartBehaviour();
                        foreach (GameObject unit in secondPlayerBattleUnits) unit.GetComponent<S_Unit>().StartBehaviour();
                    }
                    else if(matchState == MatchState.BattleEndingState)
                    {
                       // StartMatch();

                        CheckRoundEnding();
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
        public async void ServerGetPlayerUnitsFromDataBase(NetworkConnection conn, string userToken)
        {
            string _playerUnits = await FirebaseManager.instance.GetCurInventory(userToken);

            int curLength = _playerUnits.Length - 1;

            List<int> curUnitsList = new List<int>();

            for (int i = 0; i < curLength; i += 4)
            {
                string tempStr = "";
                tempStr += _playerUnits[i];
                tempStr += _playerUnits[i + 1];
                curUnitsList.Add(System.Convert.ToInt32(tempStr));
            }

            if (InGamePlayers[0].connectionToClient == conn)
                firstPlayerUnits = curUnitsList.ToList();
            else if (InGamePlayers[1].connectionToClient == conn)
                SecondPlayerUnits = curUnitsList.ToList();
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
            //Check second
            foreach (var unitid in SecondPlayerUnits)
                if (SecondPlayerWeight + unitsData.UnitsData[unitid].GetWeight() <= InGameWeightMax) secondCanPlace = true;

            if (firstCanPlace && !firstPlayerPlacing && !secondCanPlace) firstPlayerPlacing = true;

            List<int> unitsToSend = new List<int>();
            unitsToSend = firstPlayerUnits.ToList();
            InGamePlayers[0].StartPreMatchStep(RemainingTime, true, unitsToSend, !firstCanPlace ? false : firstPlayerPlacing, InGameWeightMax, false);

            firstPlayerPlacing = firstCanPlace ? firstPlayerPlacing : false;
                   

            unitsToSend.Clear();
            unitsToSend = SecondPlayerUnits.ToList();
            InGamePlayers[1].StartPreMatchStep(RemainingTime, true, unitsToSend, !secondCanPlace ? false : !firstPlayerPlacing, InGameWeightMax, false);
            unitsToSend.Clear();

            if (!firstCanPlace && !secondCanPlace)
            {
                matchState = MatchState.BattleStartingState;
                RemainingTime = 3f;
                InGamePlayers[0].UpdateGameDisplayUI(RemainingTime, true, false, true);
                InGamePlayers[1].UpdateGameDisplayUI(RemainingTime, true, false, true);
            }
        }

        public List<GameObject> GetBattlePlayerUnits(int playerid)
        {
            return (playerid == 0) ? firstPlayerBattleUnits : secondPlayerBattleUnits;
        }

        public List<GameObject> GetBattlePlayerUnitsByTeam(int teamid)
        {
            return (teamid == 0) ? firstPlayerBattleUnits : secondPlayerBattleUnits;
        }

        [Server]
        public void RemoveBattleUnit(int teamId, GameObject unit)
        {
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

            if(firstPlayerBattleUnits.Count == 0 || secondPlayerBattleUnits.Count == 0)
            {
                //Someone lost all units in round
                matchState = MatchState.BattleEndingState;
                RemainingTime = 3f;
                timerisRunning = true;
                InGamePlayers[0].UpdateGameDisplayUI(RemainingTime, true, false, true);
                InGamePlayers[1].UpdateGameDisplayUI(RemainingTime, true, false, true);
            }

        }

        [Server]
        public void CheckRoundEnding()
        {
            int firstLeftUnits = firstPlayerBattleUnits.Count;
            int secondLeftUnits = secondPlayerBattleUnits.Count;

            if (firstLeftUnits == 0 && secondLeftUnits == 0)
            {
                //If no units alive from both players (DRAW)
                CheckMatchEndConditions(0);
            }
            else if(firstLeftUnits > 0 && secondLeftUnits == 0)
            {
                DestroyAllUnits();
                firstPlayerWins++;
                CheckMatchEndConditions(-1);
            }
            else
            {
                DestroyAllUnits();
                secondPlayerWins++;
                CheckMatchEndConditions(1);
            }
        }

        [Server]
        public void CheckMatchEndConditions(int unitsDiferrences)
        {
            if(unitsDiferrences == 0) //Draw round
            {
                if (firstPlayerUnits.Count > 0 && SecondPlayerUnits.Count > 0)
                {
                    //Both players still have units in inventory (0 0 next round) (1 0 next round) (0 1 next round) (1 1 next round)

                    //Next round
                    StartMatch();
                }
                else if (firstPlayerUnits.Count > 0 && SecondPlayerUnits.Count == 0)
                {
                    //Second player lost all units in inventory (0 0 first player win) (0 1 draw) (1 0 first player wins) (1 1 first player wins) 
                    if (firstPlayerWins == 0 && secondPlayerWins == 0) AnounceMatchEnding(true, false);
                    else if (firstPlayerWins == 0 && secondPlayerWins == 1) AnounceMatchEnding(false, false);
                    else if (firstPlayerWins == 1 && secondPlayerWins == 0) AnounceMatchEnding(true, false);
                    else AnounceMatchEnding(true, false);
                }
                else
                {
                    //First player lost all units in inventory (0 0 second player win) (0 1 second player win) (1 0 draw) (1 1 second player win) 
                    if (firstPlayerWins == 0 && secondPlayerWins == 0) AnounceMatchEnding(false, true);
                    else if (firstPlayerWins == 0 && secondPlayerWins == 1) AnounceMatchEnding(false, true);
                    else if (firstPlayerWins == 1 && secondPlayerWins == 0) AnounceMatchEnding(false, false);
                    else AnounceMatchEnding(false, true);
                }
            }
            else if(unitsDiferrences == -1) //First player wins round
            {
                if (firstPlayerUnits.Count > 0 && SecondPlayerUnits.Count > 0)
                {
                    //Both players still have units in inventory (1 0 next) (1 1 next) (2 1 first win) 

                    if (firstPlayerWins == 2 && secondPlayerWins == 1) AnounceMatchEnding(true, false);
                    else StartMatch();

                }
                else if (firstPlayerUnits.Count > 0 && SecondPlayerUnits.Count == 0)
                {
                    //Second player lost all units in inventory (1 0 first win) (1 1 first win) (2 1 first win) 
                    if (firstPlayerWins == 1 && secondPlayerWins == 0) AnounceMatchEnding(true, false);
                    else if (firstPlayerWins == 1 && secondPlayerWins == 1) AnounceMatchEnding(true, false);
                    else if (firstPlayerWins == 2 && secondPlayerWins == 1) AnounceMatchEnding(true, false);
                    else throw new Exception("Win condition after the first player wins round and second player don't have inventory is not found!");
                }
                else
                {
                    //First player lost all units in inventory (1 0 draw) (1 1 second win) (2 1 first win) 
                    if (firstPlayerWins == 1 && secondPlayerWins == 0) AnounceMatchEnding(false, false);
                    else if (firstPlayerWins == 1 && secondPlayerWins == 1) AnounceMatchEnding(false, true);
                    else if (firstPlayerWins == 2 && secondPlayerWins == 1) AnounceMatchEnding(true, false);
                    else throw new Exception("Win condition after the first player wins round and first player don't have inventory is not found!");
                }
            }
            else if(unitsDiferrences == 1) //Second player wins round
            {
                if (firstPlayerUnits.Count > 0 && SecondPlayerUnits.Count > 0)
                {
                    //Both players still have units in inventory (0 1 next) (1 1 next) (1 2 second win)

                    if (firstPlayerWins == 1 && secondPlayerWins == 2) AnounceMatchEnding(false, true);
                    else StartMatch();

                }
                else if (firstPlayerUnits.Count > 0 && SecondPlayerUnits.Count == 0)
                {
                    //Second player lost all units in inventory (0 1 draw) (1 1 first win) (1 2 second win) 
                    if (firstPlayerWins == 0 && secondPlayerWins == 01) AnounceMatchEnding(false, false);
                    else if (firstPlayerWins == 1 && secondPlayerWins == 1) AnounceMatchEnding(true, false);
                    else if (firstPlayerWins == 1 && secondPlayerWins == 2) AnounceMatchEnding(false, true);
                    else throw new Exception("Win condition after the second player wins round and second player don't have inventory is not found!");
                }
                else
                {
                    //First player lost all units in inventory (0 1 second win) (1 1 second win) (1 2 second win)
                    if (firstPlayerWins == 0 && secondPlayerWins == 1) AnounceMatchEnding(false, true);
                    else if (firstPlayerWins == 1 && secondPlayerWins == 1) AnounceMatchEnding(false, true);
                    else if (firstPlayerWins == 1 && secondPlayerWins == 2) AnounceMatchEnding(false, true);
                    else throw new Exception("Win condition after the second player wins round and first player don't have inventory is not found!");
                }
            }
            else throw new Exception("Can't check match end conditions Error unitsDif = " + unitsDiferrences);
        }

        [Server]
        public void AnounceMatchEnding(bool firstPlayerWins, bool secondPlayerWins)
        {
            timerisRunning = true;
            RemainingTime = 30f;
            matchState = MatchState.AfterMatchState;

            if (firstPlayerWins == false && secondPlayerWins == false) //Draw
            {            
                InGamePlayers[0].UpdateGameDisplayUIDraw(RemainingTime);
                InGamePlayers[1].UpdateGameDisplayUIDraw(RemainingTime);
            }
            else if(firstPlayerWins == true && secondPlayerWins == false) //First player wins the match
            {
                InGamePlayers[0].UpdateGameDisplayUIWinLose(RemainingTime, true);
                InGamePlayers[1].UpdateGameDisplayUIWinLose(RemainingTime, false);
            }
            else if(secondPlayerWins == false && firstPlayerWins == true) //Second player wins the match
            {
                InGamePlayers[0].UpdateGameDisplayUIWinLose(RemainingTime, false);
                InGamePlayers[1].UpdateGameDisplayUIWinLose(RemainingTime, true);
            }
            else throw new Exception("Incorrect match ending!");
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

    }
}
