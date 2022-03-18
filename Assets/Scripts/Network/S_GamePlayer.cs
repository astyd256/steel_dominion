using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        [SerializeField]
        private TMP_Text weightText = null;

        public int maxWeight = 0;
        public int currentWeight = 0;

        private List<GameObject> unitBtns = new List<GameObject>();

        private bool placeState = false;
        private int idToPlace = -1;

        [Header("UI")]
        [SerializeField] private GameObject gameUI = null;
        [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[2];
        [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[2];
        [SerializeField] private TMP_Text timerText = null;
        [SerializeField] private GameObject unitsInventory = null;
        [SerializeField] private Button passButton = null;

        [Header("Scene")]
        [SerializeField] private Camera playercamera = null;
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
        //Network functions

        public override void OnStartAuthority()
        {
            //SendPlayerNameToServer
            
            gameUI.SetActive(true);

            S_PlayerData data = S_SavePlayerData.LoadPlayer();

            CmdSetDisplayName(data.playername);
            CmdGetUnits(data.unitData, netId);
  
            ListUnits();

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
                playercamera.gameObject.SetActive(true);
            }

            GameRoom.InGamePlayers.Add(this);

            UpdateDisplay();
        }

        public override void OnStopClient()
        {
            GameRoom.InGamePlayers.Remove(this);

            UpdateDisplay();
        }

        //

        public void AddUnit(SO_UnitItemData unit)
        {
            Units.Add(unit);
        }

        public void ToggleToPlaceUnit(int index)
        {
            if (idToPlace != -1) unitBtns[idToPlace].GetComponent<S_UnitButton>().ToggleButtonLight(false);

            if(idToPlace == index)
            {
                unitBtns[idToPlace].GetComponent<S_UnitButton>().ToggleButtonLight(false);
                placeState = false;
                idToPlace = -1;
                placeState = !placeState;
                return;
            }

            placeState = true;
            idToPlace = index;
        }

        public void ListUnits()
        {
            idToPlace = -1;
            placeState = false;

            unitBtns.Clear();

            foreach(Transform unit in ItemContent)
            {
                Destroy(unit.gameObject);
            }

            int i = 0;

            foreach(var unit in Units)
            {
                //Debug.Log("Inventory draw - " + unit.id);
                GameObject obj = Instantiate(InventoryItem, ItemContent);
                unitBtns.Add(obj);
                var itemName = obj.transform.Find("TMP_Unit").GetComponent<TMP_Text>();
                var itemScript = obj.GetComponent<S_UnitButton>();
                itemName.text = unit.displayName;
                itemScript.unitListid = i;
                itemScript.unitWeight = unit.GetWeight();
                itemScript.ClientUnitClicked += ToggleToPlaceUnit;
                i++;

                if (currentWeight + unit.GetWeight() > maxWeight)
                    obj.GetComponent<Button>().interactable = false;
                
            }
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

            if(Input.GetMouseButtonDown(0) && placeState)
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                
                var ray = playercamera.ScreenPointToRay(Input.mousePosition);

                LayerMask mask = LayerMask.GetMask("OnlyRaycast");
                if(Physics.Raycast(ray, out RaycastHit hit, mask))
                {
                    placeState = false;

                    if(currentWeight + Units[idToPlace].GetWeight() <= maxWeight) CmdPlaceUnit(idToPlace, hit.point);
                    //Debug.Log("Place id = " + idToPlace + "To vector3 = " + hit.point);
                    //unitBtns.RemoveAt(idToPlace);
                    //ListUnits();
                }

                //Ray ray;
                //ray.GetType
                // ray = playercamera.ScreenPointToRay(Input.mousePosition);
            }
        }

        [TargetRpc]
        public void TargetRpcRemoveUnitFromHand(int idToRemove)
        {
            currentWeight += Units[idToRemove].GetWeight();

            weightText.text = currentWeight.ToString() + "/" + maxWeight.ToString();

            Units.RemoveAt(idToRemove);

            ListUnits();
        }
        [TargetRpc]
        public void UpdateGameDisplayUI(float newValue, bool startTimer, bool showInventoryUI)
        {
            timerRemaining = newValue;
            timerState = startTimer;

            unitsInventory.SetActive(showInventoryUI);

            if(currentWeight > 0) passButton.gameObject.SetActive(showInventoryUI);
            else passButton.gameObject.SetActive(false);
        }

        [TargetRpc]
        public void StartPreMatchStep(float newTimerValue, bool startTimer, List<int> startUnits, bool CanPlace, int maxWeightToPlace, bool resetWeight)
        {
            timerRemaining = newTimerValue;
            timerState = startTimer;

            maxWeight = maxWeightToPlace;

            if (resetWeight) currentWeight = 0;

            if (CanPlace)
            {
                spawnArea.SetActive(true);
                unitsInventory.SetActive(true);

                if (currentWeight > 0) passButton.gameObject.SetActive(true);
                else passButton.gameObject.SetActive(false);
            }
            else
            {
                spawnArea.SetActive(false);
                unitsInventory.SetActive(false);
                passButton.gameObject.SetActive(false);
            }

            Units.Clear();

            foreach (int id in startUnits)
            {
                Units.Add(unitsData.UnitsData[id]);
            }

            ListUnits();
        }

        [TargetRpc]
        public void SetupSpawnAreaClientRPC(int areaindex)
        {
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

        [TargetRpc]
        public void TargetRpcGetUnitsToHand(List<int> unitsIds)
        {
            foreach (int id in unitsIds)
            {
                Units.Add(unitsData.UnitsData[id]);
            }

            ListUnits();
        }

        public void btnPass()
        {
            spawnArea.SetActive(false);
            unitsInventory.SetActive(false);
            passButton.gameObject.SetActive(false);
            passTurns();
        }

        [Command]
        private void passTurns()
        {
            GameRoom.passTurnPlayer(connectionToClient);
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
        public void CmdGetUnits(List<int> unitsids, uint id)
        {
            GameRoom.ServerGetPlayerUnits(connectionToClient, unitsids);
        }

        [Command]
        public void CmdPlaceUnit(int idToPlace, Vector3 place)
        {
            GameRoom.ServerPlaceUnit(connectionToClient, idToPlace, place);
        }
    }
}
