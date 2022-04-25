using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

public class S_CurrentUnitsPanel : MonoBehaviour
{
#if !UNITY_SERVER
    [SerializeField] public int panelWidth;
    [SerializeField] public int panelHeight;
    [SerializeField] private GameObject panelParent;
    [SerializeField] private Transform _inventoryUnitsParent;

    [SerializeField] private List<S_InventoryUnitSlot> slots = new List<S_InventoryUnitSlot>();
    [SerializeField] private List<S_InventoryUnitSlot> previousSlots = new List<S_InventoryUnitSlot>();
    [SerializeField] public int RosterWeight = 0;
    [SerializeField] public int MaxRosterWeight = 30;
    [SerializeField] public bool OverWeight = false;
    [SerializeField] public Color addedColor;

    private int slotscount = 0;

    [SerializeField]
    public GridLayoutGroup glg = null;
    public bool previewActive = false;
    public Color defaultColor;
    public Color WeightUnderColor;
    public Color WeightOverColor;

    public bool placingSlot = false;
    public bool shuffleReady = true;
    private int indexForShuffle = 0;

    [SerializeField] private S_InventoryUnitSlot slotInstance; // Blank
    [SerializeField] public GameObject SaveInventoryButton;

    public void SetSizes()
    {
        foreach(Transform slot in transform)
        {
            slot.GetComponent<S_Draggable>().SetSize();
        }
    }
    public void SaveUnitsPanel()
    {
        SaveInventoryButton.SetActive(false);
        previousSlots = slots.ToList();
        // SAVE INVENTORY CODE:
        string _saveString = "";
        foreach (S_InventoryUnitSlot slot in slots)
        {
            int _unitID = slot.GetUnitData().id;
            int _unitInventoryPosition = int.Parse(slot.name);

            if(_unitID < 10)
            {
                _saveString += "0";
                _saveString += _unitID.ToString();
            }
            else _saveString += _unitID.ToString();

            if (_unitInventoryPosition < 10)
            {
                _saveString += "0";
                _saveString += _unitInventoryPosition.ToString();
            }
            else _saveString += _unitInventoryPosition.ToString();
        }
        Debug.Log("String to save = " + _saveString);
        FirebaseManager.instance.SaveCurInventory(_saveString);
    }

    public async Task RemoveUnitsFromPanel()
    {
        foreach (Transform child in transform)
        {
            AddingSlotPreviewEnd(child.GetComponent<S_InventoryUnitSlot>());
            RemoveUnitFromPanel(child.GetComponent<S_InventoryUnitSlot>());
            
        }
        await Task.Yield();
    }

    public async void ReverseSlots()
    {

        await RemoveUnitsFromPanel();
        
        // NEED FINE ARRANGEMENT OF PREVIOUS SLOTS IN A STRING

        for (int i = 0; i < previousSlots.Count; i++)
        {
            //previousSlots[i].name = i.ToString(); // PSB
            AddingSlotPreviewStart(previousSlots[previousSlots.Count - 1 - i]); 
            AddUnitSLot(previousSlots[i]);
            previousSlots[i].GetComponent<Image>().color = addedColor;
        }

        slots = previousSlots.ToList();

        RosterWeight = 0;
        foreach (S_InventoryUnitSlot slot in slots)
        {
            RosterWeight += slot.GetUnitWeight();
        }
        UpdateRosterWeight();

        int k = 0;
        foreach (Transform child in transform)
        {
            child.name = k.ToString();
            child.GetComponent<S_InventoryUnitSlot>().SetCanDrag(true);
            k++;
        }
    }

    public void LoadSlotsFromString(string unitsString)
    {
        Debug.Log("CurrentUnits to load = " + unitsString);
        int curLength = unitsString.Length - 1;

        List<int> curUnitsList = new List<int>();
        List<int> curUnitsInventoryIDList = new List<int>();

        for (int i = 0; i < curLength; i += 4)
        {
            string tempStr = "";
            tempStr += unitsString[i];
            tempStr += unitsString[i + 1];
            curUnitsList.Add(System.Convert.ToInt32(tempStr));
            tempStr = "";
            tempStr += unitsString[i + 2];
            tempStr += unitsString[i + 3];
            curUnitsInventoryIDList.Add(System.Convert.ToInt32(tempStr));
        }

        foreach (Transform child in transform)
        {
            AddingSlotPreviewEnd(child.GetComponent<S_InventoryUnitSlot>());
            RemoveUnitFromPanel(child.GetComponent<S_InventoryUnitSlot>());
        }

       

        for (int i = 0; i < curUnitsList.Count; i++)
        {
            AddingSlotPreviewStart(_inventoryUnitsParent.GetChild(curUnitsInventoryIDList[curUnitsList.Count - 1 - i]).GetComponent<S_InventoryUnitSlot>());
            AddUnitSLot(_inventoryUnitsParent.GetChild(curUnitsInventoryIDList[i]).GetComponent<S_InventoryUnitSlot>());
            _inventoryUnitsParent.GetChild(curUnitsInventoryIDList[i]).GetComponent<Image>().color = addedColor;
        }

        int j = 0;

        foreach (Transform child in transform)
        {
            child.name = j.ToString();
            child.GetComponent<S_InventoryUnitSlot>().SetCanDrag(true);
            j++;
        }
        // slots = previousSlots.ToList();

        RosterWeight = 0;
        foreach (S_InventoryUnitSlot slot in slots)
        {
            RosterWeight += slot.GetUnitWeight();
        }
        UpdateRosterWeight();

        previousSlots = slots.ToList();
    }

    public void UpdateRosterWeight()
    {
        GameObject.Find("TextRosterSize").GetComponent<TextMeshProUGUI>().text = RosterWeight.ToString() + "/30";  
        if (RosterWeight <= MaxRosterWeight)
        {
            GameObject.Find("TextRosterSize").GetComponent<TextMeshProUGUI>().color = WeightUnderColor;
            OverWeight = false;
        }
        else
        {
            GameObject.Find("TextRosterSize").GetComponent<TextMeshProUGUI>().color = WeightOverColor;
            OverWeight = true;
        }
    }
    public int GetSlotsCount()
    {
        return slotscount;
    }

    private void Awake() // In future it needs to be loaded with saved units on panel
    {
        // Initial slot (0) when 0 slots;

        /*
        slotSize = new Vector2(panelWidth, panelHeight);
        slotNumber = slots.Count;
        slotSize.x = (panelWidth / slots.Count);
        slotSize.y = panelHeight;
        Debug.Log(slotSize);
        glg.cellSize = slotSize;

        foreach (S_InventoryUnitSlot slot in slots) {
            slot.GetComponent<BoxCollider2D>().size = slotSize;
        }
        */
        addedColor = GameObject.Find("DragController").GetComponent<S_DragController>().addedColor;

        panelHeight = Mathf.FloorToInt(GetComponent<RectTransform>().rect.height);
        panelWidth = Mathf.FloorToInt(GetComponent<RectTransform>().rect.width);
        placingSlot = false;
        foreach(S_InventoryUnitSlot slot in slots)
        {
            RosterWeight += slot.GetUnitWeight();
        }
        UpdateRosterWeight();
        previousSlots = slots.ToList();
    }

    private void UpdateColliderSize()
    {
        foreach (Transform childSlot in transform)
        {
            childSlot.GetComponent<BoxCollider2D>().size = glg.cellSize;
        }
    }

    public void AddingSlotPreviewStart(S_InventoryUnitSlot addingSlot)
    {
        slotscount++;
        slotInstance = Instantiate(addingSlot, GetComponent<S_CurrentUnitsPanel>().transform); // Copy
        Destroy(slotInstance.GetComponent<Rigidbody2D>()); // MUSTHAVE

        //Roster Weight Control at Draggable OnTriggerEnter2D

        slotInstance.name = (slotscount-1).ToString(); // Name = ID in panel;

        GetComponent<S_CurrentUnitsPanel>().transform.GetChild(slotscount - 1).SetSiblingIndex(indexForShuffle);

        SetLayoutGroupSize((panelWidth / slotscount), panelHeight);


        UpdateColliderSize();

        previewActive = true;
    }

    public void AddingSlotPreviewEnd(S_InventoryUnitSlot addingSlot)
    {
        if (previewActive == true) {
            // Destroy Preview
            slotscount--;
            Destroy(transform.GetChild(indexForShuffle).gameObject); // Bug here on inventory close
            // Roster Weight control is at TriggerEnter2D
            //
            if (slotscount > 0)
            {
                SetLayoutGroupSize((panelWidth / slotscount), panelHeight);
            }
            previewActive = false;
            shuffleReady = true;

            UpdateColliderSize();

        }
    }

    public void AddUnitSLot(S_InventoryUnitSlot addedSlot) //_LastDragged
    {
        if (slots.Count == 0) indexForShuffle = 0;

        GetComponent<S_CurrentUnitsPanel>().transform.GetChild(indexForShuffle).GetComponent<Image>().color = defaultColor;

        //S_InventoryUnitSlot newslot = Instantiate(addedSlot, GetComponent<S_CurrentUnitsPanel>().transform); // Copy
 
        slots.Add(addedSlot);
        addedSlot.SetBelongsToUnitsPanel(true);
        addedSlot.SetCanDrag(false);

        // Size set:
        if (slotscount > 0)
        {
            SetLayoutGroupSize((panelWidth / slotscount), panelHeight);
        }

        previewActive = false;
        shuffleReady = true;
    }

    public void ChangePlace(S_InventoryUnitSlot slotOriginal) // _lastDragged
    {
    }
    public void AddingSlotPreviewShuffle() // OnTriggerEnter2D
    {
        //Debug.Log(shuffleReady);
        if (shuffleReady == true)
        {
            // Change preview hierarchy place
            if (transform.childCount > 0)
            {
                //int indexMax = transform.childCount - 1;
                //System.Random random = new System.Random();
                //indexForShuffle = random.Next(indexMax+2);
                indexForShuffle = transform.childCount;

                /*
                for (int i = 0; i < indexMax; i++)
                {
                    GetComponent<S_CurrentUnitsPanel>().transform.GetChild(i).name = GetComponent<S_CurrentUnitsPanel>().transform.GetChild(i).GetSiblingIndex().ToString();
                }
                */
            }

            shuffleReady = false;
        }
    }

    public void ShuffleFromWithinPreviewStart(S_InventoryUnitSlot shuffleSLot)
    {
        // For but deletion
        shuffleReady = true;
    }

    public void ShuffleFromWithinPreviewEnd(S_InventoryUnitSlot shuffleSlot)
    {

        shuffleReady = true;
    }

    public void RemoveUnitFromPanel(S_InventoryUnitSlot slotToRemove)
    {
        slotscount--;

        int toRemove = int.Parse(slotToRemove.name); // 
        // Names change after deletion of 1st element, from 0 to last
        int removedID = toRemove; // For ignoring

        slots[toRemove].SetBelongsToUnitsPanel(false);
        slots[toRemove].SetCanDrag(true);
        slots[toRemove].GetComponent<Image>().color = defaultColor;

        slots.Remove(slots[toRemove]);
        Destroy(slotToRemove.gameObject);

        if (slotscount > 0)
        {
            SetLayoutGroupSize((panelWidth / slotscount), panelHeight);
        }

        toRemove = 0;

        // Names from 0 to last in panel for slots
        
        foreach(Transform slot in transform)
        {
            if (int.Parse(slot.gameObject.name) == removedID) continue; // Ignore
            slot.gameObject.name = toRemove.ToString();
            toRemove++;
        }
        UpdateColliderSize();

    }

    public void SetLayoutGroupSize(int x, int y)
    {
        glg.cellSize = new Vector2(x, y);
    }


    public bool GetPlacingSlotBool()
    {
        return placingSlot;
    }

    public void SetPlacingSlotBool(bool b)
    {
        placingSlot = b;
    }
    public void SetShuffleReady(bool b)
    {
        shuffleReady = b;
    }

    public bool GetShuffleReady()
    {
        return shuffleReady;
    }
#endif
}
