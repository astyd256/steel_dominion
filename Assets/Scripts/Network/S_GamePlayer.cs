//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Mirror
{
    public class S_GamePlayer : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject gameUI = null;
        [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[2];
        [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[2];
        [SerializeField] private TMP_Text timerText = null;
        [SerializeField] private Slider timeBar = null;

        [SerializeField] private GameObject playercamera = null;

        private bool timerState = false;
        private float timerRemaining = 0f;

        [SyncVar(hook = nameof(HandleDisplayPlayerNameChanged))]
        public string DisplayName = "Loading...";
        [SyncVar(hook = nameof(HandlereadyStatusChanged))]
        public bool IsReady = false;

        //
        private S_NetworkManagerSteel gameroom;

        private S_NetworkManagerSteel GameRoom
        {
            get
            {
                if (gameroom != null) { return gameroom; }
                return gameroom = NetworkManager.singleton as S_NetworkManagerSteel;
            }
        }
        //

        public override void OnStartAuthority()
        {
            //SendPlayerNameToServer
            CmdSetDisplayName(S_SavePlayerData.LoadPlayer().playername);

            gameUI.SetActive(true);

            this.CallWithDelay(CmdReadyUp, 3f);
        }
        public override void OnStartServer()
        {
            GameRoom.InGamePlayers.Add(this);
        }

        public override void OnStopServer()
        {
            GameRoom.InGamePlayers.Remove(this);
        }
        public override void OnStartClient()
        {
            if (hasAuthority)
            {
                playercamera.SetActive(true);
            }

            GameRoom.InGamePlayers.Add(this);
            Debug.Log("Client start");
            UpdateDisplay();
        }

        public override void OnStopClient() //
        {
            GameRoom.InGamePlayers.Remove(this);

            UpdateDisplay();
        }

        public void HandlereadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
        public void HandleDisplayPlayerNameChanged(string oldValue, string newValue) => UpdateDisplay();

        private void UpdateDisplay()
        {
            //find the local player to update ui
            if(!hasAuthority)
            {
                foreach(var player in GameRoom.InGamePlayers)
                {
                    if(player.hasAuthority)
                    {
                        player.UpdateDisplay();
                        break;
                    }
                }
                return;
            }
            //Can be optimized to one loop
            for(int i = 0; i < playerNameTexts.Length; i++)
            {
                playerNameTexts[i].text = "Waiting...";
                playerReadyTexts[i].text = string.Empty;
            }

            for(int i = 0; i<GameRoom.InGamePlayers.Count;i++)
            {
                playerNameTexts[i].text = GameRoom.InGamePlayers[i].DisplayName;
                playerReadyTexts[i].text = GameRoom.InGamePlayers[i].IsReady ?
                    "<color=green>Ready</color>" :
                    "<color=red>Not Ready</color>";
            }
        }

        public void UpdateTimer (float timeToDisplay)
        {
            timeToDisplay += 1;
            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        [ClientCallback]
        void Update()
        {
            if(timerState)
            {
                if(timerRemaining > 0)
                {
                    timerRemaining -= Time.deltaTime;
                    UpdateTimer(timerRemaining);
                }
                else
                {
                    //Timer end event
                    timerRemaining = 0f;
                    timerState = false;
                }
            }
        }

        [ClientRpc]
        public void UpdateGameDisplay(float newValue, bool startTimer)
        {
            timerRemaining = newValue;
            timerState = startTimer;
        }

        [Command]
        private void CmdSetDisplayName(string displayName)
        {
            DisplayName = displayName;
        }

        [Command]
        public void CmdReadyUp()
        {
            IsReady = true;

            GameRoom.StartMatch();
            //GameRoom.NotifyPlayersofReadyState();
        }
    }
}
