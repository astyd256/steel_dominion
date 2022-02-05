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

        public static event Action OnClientConnected;
        public static event Action OnClientDisconnected;

        public List<S_GamePlayer> InGamePlayers { get; } = new List<S_GamePlayer>();

        public override void OnStartServer()
        {
            spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
        }

        public override void OnStartClient()
        {
            var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

            foreach(var prefab in spawnablePrefabs)
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

            foreach(var player in InGamePlayers)
            {
                if (!player.IsReady) return false;
            }

            return true;
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            Debug.Log("AddPlayer");
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

            // instantiating a "Player" prefab gives it the name "Player(clone)"
            // => appending the connectionId is WAY more useful for debugging!
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);
        }

        public override void OnStopServer()
        {
            //Write results of match?
            InGamePlayers.Clear();
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
