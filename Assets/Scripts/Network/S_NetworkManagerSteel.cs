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
        //private List<SO_UnitItemData> firstPlayerUnits = new List<SO_UnitItemData>();
        private List<int> firstPlayerUnits = new List<int>();
        private int firstPlayerWeight = 0;
        private bool firstCanPlace = false;

        [SerializeField]
        //private List<SO_UnitItemData> SecondPlayerUnits = new List<SO_UnitItemData>();
        private List<int> SecondPlayerUnits = new List<int>();
        private int SecondPlayerWeight = 0;
        private bool secondCanPlace = false;

        private bool placingPhase = false;
        private bool firstPlayerPlacing = true;


        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;

        public List<S_GamePlayer> InGamePlayers { get; } = new List<S_GamePlayer>();

        //Server start, stop, add player, connect client, disconnect client
        public override void OnStartServer()
        {

            spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
            Debug.Log("sp = " + spawnPrefabs.Count);
        }

        public override void OnStartClient()
        {
            var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");
            Debug.Log("sp = " + spawnPrefabs.Count);
            foreach (var prefab in spawnablePrefabs)
            {
                NetworkClient.RegisterPrefab(prefab);
            }
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
            Debug.Log("ServerConnect");
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
            Debug.Log(InGamePlayers.Count);
            if (numPlayers < 2) return false;
            
            foreach (var player in InGamePlayers)
            {
                Debug.Log(player.IsReady);
                if (!player.IsReady) return false;
            }

            return true;
        }

        public void StartMatch()
        {
            if (!IsReadyToStart()) return;
            Debug.Log("Starting");
            placingPhase = true;
            RemainingTime = PreMatchPlacementTime;
            timerisRunning = true;

            firstCanPlace = true;
            secondCanPlace = true;

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

        [Server]
        public void ServerPlaceUnit(NetworkConnection conn, int idToPlace, Vector3 placeToSpawn)
        {
            //add validation to place and unit type
            if(InGamePlayers[0].connectionToClient == conn)
            {
                int unitid = firstPlayerUnits[idToPlace];
                if (firstPlayerWeight + unitsData.UnitsData[unitid].GetWeight() > InGameWeightMax) return;
                //Debug.Log("Unit id in hand = " + unitid);
                GameObject unitObj = Instantiate(unitsData.UnitsData[unitid].prefab, placeToSpawn, Quaternion.identity);
                NetworkServer.Spawn(unitObj);
                InGamePlayers[0].TargetRpcRemoveUnitFromHand(idToPlace);
                firstPlayerWeight += unitsData.UnitsData[unitid].GetWeight();
                firstPlayerUnits.RemoveAt(idToPlace);
            }
            else if (InGamePlayers[1].connectionToClient == conn)
            {
                int unitid = SecondPlayerUnits[idToPlace];
                if (SecondPlayerWeight + unitsData.UnitsData[unitid].GetWeight() > InGameWeightMax) return;
                //Debug.Log("Unit id in hand = " + unitid);
                GameObject unitObj = Instantiate(unitsData.UnitsData[unitid].prefab, placeToSpawn, Quaternion.identity);
                NetworkServer.Spawn(unitObj);
                InGamePlayers[1].TargetRpcRemoveUnitFromHand(idToPlace);
                SecondPlayerWeight += unitsData.UnitsData[unitid].GetWeight();
                SecondPlayerUnits.RemoveAt(idToPlace);
            }

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

                int unitid = firstPlayerUnits[randId];
                if (firstPlayerWeight + unitsData.UnitsData[unitid].GetWeight() > InGameWeightMax) return;
                //Debug.Log("Unit id in hand = " + unitid);
                GameObject unitObj = Instantiate(unitsData.UnitsData[unitid].prefab, spawnPlace, Quaternion.identity);
                NetworkServer.Spawn(unitObj);
                InGamePlayers[0].TargetRpcRemoveUnitFromHand(randId);
                firstPlayerWeight += unitsData.UnitsData[unitid].GetWeight();
                firstPlayerUnits.RemoveAt(randId);
            }
            else if (playerid == 1)
            {
                randId = rand.Next(0, SecondPlayerUnits.Count);
                spawnPlace.z = rand.Next(16, 20);

                int unitid = SecondPlayerUnits[randId];
                if (SecondPlayerWeight + unitsData.UnitsData[unitid].GetWeight() > InGameWeightMax) return;
                //Debug.Log("Unit id in hand = " + unitid);
                GameObject unitObj = Instantiate(unitsData.UnitsData[unitid].prefab, spawnPlace, Quaternion.identity);
                NetworkServer.Spawn(unitObj);
                InGamePlayers[1].TargetRpcRemoveUnitFromHand(randId);
                SecondPlayerWeight += unitsData.UnitsData[unitid].GetWeight();
                SecondPlayerUnits.RemoveAt(randId);
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

                    if(placingPhase)
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
                        else
                        {
                            placingPhase = false;

                            InGamePlayers[0].UpdateGameDisplayUI(GameTime, true, false);
                            InGamePlayers[1].UpdateGameDisplayUI(GameTime, true, false);

                            RemainingTime = GameTime;
                            timerisRunning = true;
                        }
                    }
                }
            }
        }

        //Load player Data functions

        [Server]
        public void ServerGetPlayerUnits(NetworkConnection conn, List<int> UnitsIds)
        {
            if(InGamePlayers[0].connectionToClient == conn)
            {
                firstPlayerUnits = UnitsIds;
                //foreach(int id in UnitsIds)
                //{
                //    firstPlayerUnits.Add(unitsData.UnitsData[id]);
                    //Debug.Log("Loaded first id = " + id);
                //}  
            }
            else if(InGamePlayers[1].connectionToClient == conn)
            {
                SecondPlayerUnits = UnitsIds;
                //foreach (int id in UnitsIds)
                //{
                //   SecondPlayerUnits.Add(unitsData.UnitsData[id]);
                //Debug.Log("Loaded second id = " + id);
                //}
            }

            

            //foreach (int id in data.unitData)
            // {
            // Debug.Log("Loaded id = " + id);
            //   Units.Add(unitsData.UnitsData[id]);
            // }
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
                RemainingTime = 3f;
                InGamePlayers[0].UpdateGameDisplayUI(RemainingTime, true, false);
                InGamePlayers[1].UpdateGameDisplayUI(RemainingTime, true, false);
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
