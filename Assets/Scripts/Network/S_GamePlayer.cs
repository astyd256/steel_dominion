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
        public float cameraMoveForward = 0f;
        [SerializeField]
        public float origZloc;

        private List<GameObject> unitBtns = new List<GameObject>();

        private bool placeState = false;
        private int idToPlace = -1;

        [Header("UI")]
        [SerializeField] private GameObject gameUI = null;
        [SerializeField] private TMP_Text _enemyName = null;
        [SerializeField] private TMP_Text _enemyReady = null;
        [SerializeField] private TMP_Text _enemyLevel = null;
        [SerializeField] private TMP_Text timerText = null;
        [SerializeField] private GameObject unitsInventory = null;
        [SerializeField] private Button passButton = null;

        [Header("Scene")]
        [SerializeField] private Camera playercamera = null;
        [SerializeField] private Camera playercameraUI = null;
        private GameObject spawnArea = null;
        private GameObject _enemySpawnArea = null;


        private bool timerState = false;
        private float timerRemaining = 0f;

        [SyncVar(hook = nameof(HandleDisplayPlayerNameChanged))]
        public string DisplayName = "Loading...";
        [SyncVar(hook = nameof(HandlereadyStatusChanged))]
        public bool IsReady = false;

        public int Level = 1;

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
#if !UNITY_SERVER
        public override void OnStartAuthority()
        {
            //SendPlayerNameToServer

            gameUI.SetActive(true);

            transform.parent = GameObject.Find("CameraRotator").transform;
            origZloc = this.transform.position.z;

            CmdGetPlayerToken(FirebaseManager.instance.GetUserToken());
            CmdSetDisplayNameLevel(FirebaseManager.instance.GetUserName(), FirebaseManager.instance.GetUserXp());

            this.CallWithDelay(CmdReadyUp, 3f);
        }
#endif
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
                playercameraUI.gameObject.SetActive(true);
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
#if !UNITY_SERVER
        public void ToggleToPlaceUnit(int index)
        {
            if (idToPlace != -1) unitBtns[idToPlace].GetComponent<S_UnitButton>().ToggleButtonLight(false);

            if (idToPlace == index)
            {
                unitBtns[idToPlace].GetComponent<S_UnitButton>().ToggleButtonLight(false);
                idToPlace = -1;
                placeState = false;
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

            foreach (Transform unit in ItemContent)
            {
                Destroy(unit.gameObject);
            }

            int i = 0;

            foreach (var unit in Units)
            {
                //Debug.Log("Inventory draw - " + unit.id);
                GameObject obj = Instantiate(InventoryItem, ItemContent);
                unitBtns.Add(obj);
                var itemScript = obj.GetComponent<S_UnitButton>();
                // Weight, name, sprite
                itemScript.SetData(unit);

                itemScript.unitListid = i;
                itemScript.ClientUnitClicked += ToggleToPlaceUnit;
                i++;

                if (currentWeight + unit.GetWeight() > maxWeight)
                    obj.GetComponent<Button>().interactable = false;

            }
        }
#endif
        public void HandlereadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
        public void HandleDisplayPlayerNameChanged(string oldValue, string newValue) => UpdateDisplay();

        private void UpdateDisplay()
        {
            //find the local player to update ui
            if (!hasAuthority)
            {
                foreach (var player in GameRoom.InGamePlayers)
                {
                    if (player.hasAuthority)
                    {
                        player.UpdateDisplay();
                        break;
                    }
                }
                return;
            }
            //Can be optimized to one loop

            _enemyName.text = "Waiting...";
            _enemyReady.text = "Loading...";
            _enemyLevel.text = "Level 0";

            foreach (S_GamePlayer player in GameRoom.InGamePlayers)
            {
                if (player.netId != this.netId)
                {
                    _enemyName.text = (player.DisplayName == "") ? "Unknown" : player.DisplayName;
                    _enemyReady.text = player.IsReady ?
                        "<color=green>Ready</color>" :
                        "<color=red>Not Ready</color>";
                    _enemyLevel.text = "Level " + player.Level.ToString();
                    break;
                }
            }
        }

        public void UpdateTimer(float timeToDisplay)
        {
            timeToDisplay += 1;
            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        [ClientCallback]
        void Update()
        {
            if (timerState)
                if (timerRemaining > 0)
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

            if (Input.GetMouseButtonDown(0) && placeState)
            {

                if (EventSystem.current.IsPointerOverGameObject()) return;

                Ray ray = playercamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, 500f, 1 << 6))
                {
                    placeState = false;

                    if (currentWeight + Units[idToPlace].GetWeight() <= maxWeight) CmdPlaceUnit(idToPlace, hit.point);
                    //Debug.Log("Place id = " + idToPlace + "To vector3 = " + hit.point);
                    //unitBtns.RemoveAt(idToPlace);
                    //ListUnits();
                }

                //Ray ray;
                //ray.GetType
                // ray = playercamera.ScreenPointToRay(Input.mousePosition);
            }

            if (Input.GetMouseButton(0) && !placeState)
            {
                if (netId == 1) cameraMoveForward = Mathf.Clamp(cameraMoveForward + Input.GetAxis("Mouse Y"), -10f, 40f);
                else cameraMoveForward = Mathf.Clamp(cameraMoveForward + Input.GetAxis("Mouse Y"), -40f, 10f);

                transform.localPosition = new Vector3(0f, this.transform.position.y, origZloc + cameraMoveForward);

                GameObject camera = GameObject.Find("CameraRotator");
                camera.transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));


                // this.transform.position.z = origZloc + cameraMoveForward;
                // camera

            }
        }

        [TargetRpc]
        public void TargetRpcRemoveUnitFromHand(int idToRemove)
        {
#if !UNITY_SERVER
            currentWeight += Units[idToRemove].GetWeight();

            weightText.text = currentWeight.ToString() + "/" + maxWeight.ToString();

            Units.RemoveAt(idToRemove);

            ListUnits();
#endif
        }
        [TargetRpc]
        public void UpdateGameDisplayUI(float newValue, bool startTimer, bool showInventoryUI, bool hideEnemyArea)
        {
            timerRemaining = newValue;
            timerState = startTimer;

            unitsInventory.SetActive(showInventoryUI);
            weightText.gameObject.SetActive(showInventoryUI);

            if (currentWeight > 0) passButton.gameObject.SetActive(showInventoryUI);
            else passButton.gameObject.SetActive(false);

            _enemySpawnArea.SetActive(!hideEnemyArea);

        }

        [TargetRpc]
        public void UpdateGameDisplayUIWinLose(float newValue, bool win)
        {
            timerRemaining = newValue;
            timerState = true;

            unitsInventory.SetActive(false);
            _enemySpawnArea.SetActive(false);
            passButton.gameObject.SetActive(false);

            weightText.gameObject.SetActive(true);
            weightText.text = (win) ? "You win!" : "You lose!";
        }

        [TargetRpc]
        public void UpdateGameDisplayUIDraw(float newValue)
        {
            timerRemaining = newValue;
            timerState = true;

            unitsInventory.SetActive(false);
            _enemySpawnArea.SetActive(false);
            passButton.gameObject.SetActive(false);

            weightText.gameObject.SetActive(true);
            weightText.text = "Draw !";
        }

        [TargetRpc]
        public void StartPreMatchStep(float newTimerValue, bool startTimer, List<int> startUnits, bool CanPlace, int maxWeightToPlace, bool resetWeight)
        {
#if !UNITY_SERVER
            timerRemaining = newTimerValue;
            timerState = startTimer;

            maxWeight = maxWeightToPlace;

            if (resetWeight)
            {
                currentWeight = 0;
                weightText.text = "0/" + maxWeight.ToString();
            }

            if (CanPlace)
            {
                weightText.gameObject.SetActive(true);
                spawnArea.SetActive(true);
                _enemySpawnArea.SetActive(false);
                unitsInventory.SetActive(true);

                if (currentWeight > 0) passButton.gameObject.SetActive(true);
                else passButton.gameObject.SetActive(false);
            }
            else
            {
                weightText.gameObject.SetActive(false);
                spawnArea.SetActive(false);
                _enemySpawnArea.SetActive(true);
                unitsInventory.SetActive(false);
                passButton.gameObject.SetActive(false);
            }

            Units.Clear();

            foreach (int id in startUnits)
            {
                Units.Add(unitsData.UnitsData[id]);
            }

            ListUnits();
#endif
        }

        [TargetRpc]
        public void SetupSpawnAreaClientRPC(int areaindex)
        {
            if (areaindex == 1)
            {
                spawnArea = GameObject.FindWithTag("FirstSpawnArea");
                _enemySpawnArea = GameObject.FindWithTag("SecondSpawnArea");
            }
            else
            {
                spawnArea = GameObject.FindWithTag("SecondSpawnArea");
                _enemySpawnArea = GameObject.FindWithTag("FirstSpawnArea");
            }
            spawnArea.SetActive(false);

            _enemySpawnArea.transform.GetChild(0).GetComponent<Image>().color = Color.red;
            _enemySpawnArea.SetActive(false);
        }

        [TargetRpc]
        public void TargetRpcGetUnitsToHand(List<int> unitsIds)
        {
#if !UNITY_SERVER
            foreach (int id in unitsIds)
            {
                Units.Add(unitsData.UnitsData[id]);
            }

            ListUnits();
#endif
        }

        public void btnPass()
        {
            spawnArea.SetActive(false);
            weightText.gameObject.SetActive(false);
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
        private void CmdSetDisplayNameLevel(string displayName, int exp)
        {
            Level = (int)Mathf.Floor(Mathf.Sqrt(exp / 20) + 1);
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
        public void CmdGetPlayerToken(string playerToken)
        {
//#if UNITY_SERVER
           GameRoom.ServerGetPlayerUnitsFromDataBase(connectionToClient, playerToken);
//#endif
        }

        [Command]
        public void CmdPlaceUnit(int idToPlace, Vector3 place)
        {
            GameRoom.ServerPlaceUnit(connectionToClient, idToPlace, place);
        }
    }
}
