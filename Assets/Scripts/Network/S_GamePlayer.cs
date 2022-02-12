using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Mirror
{
    public class S_GamePlayer : NetworkBehaviour
    {
        [Header("Inventory")]
        [SerializeField]
        public List<SO_UnitItemData> Units = new List<SO_UnitItemData>();
        [SerializeField]
        public Transform ItemContent;
        [SerializeField]
        public GameObject InventoryItem;
        [SerializeField]
        public SO_UnitsToPlay unitsData;

        [Header("UI")]
        [SerializeField] private GameObject gameUI = null;
        [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[2];
        [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[2];
        [SerializeField] private TMP_Text timerText = null;
        [SerializeField] private Slider timeBar = null;
        // [SerializeField] private Slider timeBar = null;
        [Header("Scene")]
        [SerializeField] private GameObject playercamera = null;
        [SerializeField] private GameObject spawnArea = null;
        

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

        public void AddUnit(SO_UnitItemData unit)
        {
            Units.Add(unit);
        }

        public void RemoveUnit(int index)
        {
            Debug.Log("Index to remove = " + index);
            Units.RemoveAt(index);

            ListUnits();
           // for(int i = index; i<Units.Count;i++)
            //{
                
           // }
        }

        public void ListUnits()
        {
            foreach(Transform unit in ItemContent)
            {
                Destroy(unit.gameObject);
            }

            int i = 0;

            foreach(var unit in Units)
            {
                //Debug.Log("Inventory draw - " + unit.id);
                GameObject obj = Instantiate(InventoryItem, ItemContent);
                var itemName = obj.transform.Find("TMP_Unit").GetComponent<TMP_Text>();
                var itemScript = obj.GetComponent<S_UnitButton>();
                itemName.text = unit.displayName;
                itemScript.id = i;
                itemScript.ClientUnitClicked += RemoveUnit;
                i++;
            }
        }
        public override void OnStartAuthority()
        {
            //SendPlayerNameToServer
            S_PlayerData data = S_SavePlayerData.LoadPlayer();

            CmdSetDisplayName(data.playername);

            List<int> unitsIds = new List<int>();

            SO_UnitsToPlay pUnits = Resources.Load<SO_UnitsToPlay>("Scripts/SO/");
            SO_UnitItemData[] pUnitss = Resources.LoadAll<SO_UnitItemData>("Scripts/SO/");

            Debug.Log("Loading" + pUnitss.Length);
            foreach (int id in data.unitData)
            {
               // Debug.Log("Loaded id = " + id);
                Units.Add(unitsData.UnitsData[id]);
            }
            
            CmdGetUnits(data.unitData);

            gameUI.SetActive(true);

            ListUnits();

            this.CallWithDelay(CmdReadyUp, 3f);
        }

        public override void OnStartServer()
        {
            GameRoom.InGamePlayers.Add(this);
            //Debug.Log("Sent to find area = " + GameRoom.InGamePlayers.Count);
            //SetupSpawnArea(GameRoom.InGamePlayers.Count);
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

        [TargetRpc]
        public void StartPreMatchStep(float newTimerValue, bool startTimer)
        {
            timerRemaining = newTimerValue;
            timerState = startTimer;
            spawnArea.SetActive(true);
        }

        [TargetRpc]
        public void SetupSpawnAreaClientRPC(int areaindex)
        {
            Debug.Log("Find area = " + areaindex);
            if (areaindex == 1)
            {
                spawnArea = GameObject.FindWithTag("FirstSpawnArea");
                Destroy(GameObject.FindWithTag("SecondSpawnArea"));
            }
            else
            {
                spawnArea = GameObject.FindWithTag("SecondSpawnArea");
                Destroy(GameObject.FindWithTag("FirstSpawnArea"));
            }
            spawnArea.SetActive(false);
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

        [Command]
        public void CmdGetUnits(List<int> unitsids)
        {
            Debug.Log("ConnectionToClient - " + connectionToClient);
            Debug.Log("ConnectionToClient id - " + connectionToClient.connectionId);
            //NetworkConnection conn;
            //Debug.Log("Connection id - " + conn.connectionId);
            
            foreach (int id in unitsids)
            {
                Debug.Log("Loaded id = " + id);
               // Units.Add(unitsData.UnitsData[id]);
            }
        }
    }
}
