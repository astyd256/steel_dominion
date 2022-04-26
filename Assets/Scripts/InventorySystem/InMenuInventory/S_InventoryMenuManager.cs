using System.Collections.Generic;
using UnityEngine;

public class S_InventoryMenuManager : MonoBehaviour
{
    // All units player has in inventory
    [SerializeField] private SO_UnitsToPlay unitsData = null;
    [SerializeField] private List<SO_UnitItemData> InventoryUnits = new List<SO_UnitItemData>();

    [SerializeField] private List<S_InventoryUnitSlot> unitInventorySlots = new List<S_InventoryUnitSlot>();
    [SerializeField] private S_InventoryUnitSlot unitInventorySlotPrefab; // SLOT PREFAB
    [SerializeField] private Transform inventoryContainer; // Container for inventory
    [SerializeField] public Color ActiveButtonColor;
    [SerializeField] public Color ButtonColor;

    [SerializeField] private Vector2 slotSize;


    //[SerializeField] private Dictionary<SO_UnitItemData, S_InventoryUnitSlot> UnitToSlotMap = new Dictionary<SO_UnitItemData, S_InventoryUnitSlot>();
    
    private bool itemMenuOpened = false;

    public void LoadUnitsFromString(string inventoryString)
    {
        // Delete previous
        ClearInventory();

        Debug.Log("Inventory to load = " + inventoryString);
        int curLength = inventoryString.Length - 1;

        List<int> curItemsListID = new List<int>();

        for (int i = 0; i < curLength; i += 2)
        {
            string tempStr = "";
            tempStr += inventoryString[i];
            tempStr += inventoryString[i + 1];
            curItemsListID.Add(System.Convert.ToInt32(tempStr));
        }

        for (int i = 0; i < curItemsListID.Count; i++)
        {
            InventoryUnits.Add(unitsData.UnitsData[curItemsListID[i]]);
        }
        InitInventory();
    }

    // INVENTORY
    public void AddUnitsToPanel()
    {
    }

    public void RemoveUnitsFromPanel()
    {

    }

    // On open inventory
    public void InitInventory()
    {
        int _slotID = 0;
        // INITIALIZE INVENTORY EXISTING BUTTONS
        foreach (SO_UnitItemData unit in InventoryUnits)
        {
            CreateSlot(unit, _slotID);
            _slotID++;
        }
    }
    public void ClearInventory()
    {
        InventoryUnits.Clear();
        foreach (Transform slot in inventoryContainer.transform)
        {
            Destroy(slot.gameObject);
        }
        unitInventorySlots.Clear();
    }

    //Sending unit's data to slot visualizer to create a proper slot image
    public void CreateSlot(SO_UnitItemData unit, int slotID)
    {
        S_InventoryUnitSlot slot = Instantiate(unitInventorySlotPrefab, inventoryContainer);
        slot.InitSlotVisualisation(unit.GetWeight(), unit.GetName(), unit.GetSprite(), unit, slotSize);
        slot.AssignSlotButtonCallback(() => OpenItemMenu(slot));
        slot.name = slotID.ToString();
        unitInventorySlots.Add(slot);
        
    }

    // Method for items
    public void OpenItemMenu(S_InventoryUnitSlot slot)
    {
        if (itemMenuOpened == false)
        {
            //slot.GetComponent<Image>().color = ActiveButtonColor;
        }
        else
        {
        }
    }

    public Vector2 GetSlotSize()
    {
        return slotSize;
    }

    public List<SO_UnitItemData> GetUnits()
    {
        return InventoryUnits;
    }

    public void Start()
    {
        //InitInventory();
    }

    ////////////////////////////////////
}
