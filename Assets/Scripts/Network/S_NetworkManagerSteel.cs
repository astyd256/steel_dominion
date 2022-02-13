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

        public float GameTime = 180f;
        public float PreMatchPlacementTime = 10f;
        private float RemainingTime = 0f;
        private bool timerisRunning = false;

        [Header("Game process")]
        [SerializeField]
        private SO_UnitsToPlay unitsData;
        [SerializeField]
        //private List<SO_UnitItemData> firstPlayerUnits = new List<SO_UnitItemData>();
        private List<int> firstPlayerUnits = new List<int>();
        private List<int> firstPlayerHand = new List<int>();
        private int firstCurrentDeckIndex = 0;
        [SerializeField]
        //private List<SO_UnitItemData> SecondPlayerUnits = new List<SO_UnitItemData>();
        private List<int> SecondPlayerUnits = new List<int>();
        private List<int> SecondPlayerHand = new List<int>();
        private int SecondCurrentDeckIndex = 0;


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
            //Debug.Log(numPlayers);
            //Debug.Log(InGamePlayers.Count);
            RemainingTime = PreMatchPlacementTime;
            timerisRunning = true;

            List<int> unitsToSend = new List<int>();

            unitsToSend.Add(firstPlayerUnits[0]);
            unitsToSend.Add(firstPlayerUnits[1]);
            unitsToSend.Add(firstPlayerUnits[2]);
            firstCurrentDeckIndex = 3;
            InGamePlayers[0].StartPreMatchStep(RemainingTime, true, unitsToSend);
            firstPlayerHand = unitsToSend;
            unitsToSend.Clear();

            unitsToSend.Add(SecondPlayerUnits[0]);
            unitsToSend.Add(SecondPlayerUnits[1]);
            unitsToSend.Add(SecondPlayerUnits[2]);
            SecondCurrentDeckIndex = 3;
            InGamePlayers[1].StartPreMatchStep(RemainingTime, true, unitsToSend);
            SecondPlayerHand = unitsToSend;
            unitsToSend.Clear();

            Debug.Log("Match started!");
        }

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
