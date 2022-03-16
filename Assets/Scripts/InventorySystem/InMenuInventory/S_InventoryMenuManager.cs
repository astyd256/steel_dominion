using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S_InventoryMenuManager : MonoBehaviour
{
    [SerializeField] private List<SO_UnitItemData> Units = new List<SO_UnitItemData>();
    [SerializeField] private List<S_InventoryUnitSlot> unitInventorySlots = new List<S_InventoryUnitSlot>();
    [SerializeField] private S_InventoryUnitSlot unitInventorySlotPrefab; // SLOT PREFAB
    [SerializeField] private Transform inventoryContainer; // Container for inventory
    [SerializeField] private GameObject inventoryPanel;    // Inventory panel
    [SerializeField] private Button inventoryOpenButton;   // Inventory open button
    [SerializeField] private Color ActiveButtonColor;
    [SerializeField] private Color ButtonColor;
    [SerializeField] private bool inventoryActive = false;


    [SerializeField] private Dictionary<SO_UnitItemData, S_InventoryUnitSlot> UnitToSlotMap = new Dictionary<SO_UnitItemData, S_InventoryUnitSlot>();
    
    private bool itemMenuOpened = false;

    public static Transform Clear(Transform transform)
    {
        foreach(Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        return transform;
    }

    public void SavePlayer()
    {
        List<int> unitsIds = new List<int>();

        foreach(var unit in Units)
        {
            unitsIds.Add(unit.id);
        }

        S_SavePlayerData.SavePlayer("Default", unitsIds);
    }

    public void LoadPlayer()
    {
       // S_SavePlayerData.LoadPlayer();
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
        // INITIALIZE INVENTORY EXISTING BUTTONS
        foreach (SO_UnitItemData unit in Units)
        {
            CreateSlot(unit);
        }
    }

    //Sending unit's data to slot visualizer to create a proper slot image
    public void CreateSlot(SO_UnitItemData unit)
    {
        S_InventoryUnitSlot slot = Instantiate(unitInventorySlotPrefab, inventoryContainer);
        slot.InitSlotVisualisation(unit.GetWeight(), unit.GetName(), unit.GetSprite());
        slot.AssignSlotButtonCallback(() => OpenItemMenu(slot));
        unitInventorySlots.Add(slot);
        
    }

    public void OpenItemMenu(S_InventoryUnitSlot slot)
    {
        if (itemMenuOpened == false)
        {
            slot.GetComponent<Image>().color = ActiveButtonColor;
        }
        else
        {
        }
    }

    //Panel response on inventory button press
    public void SwitchInventory()
    {
        if (!inventoryActive)
        {
            inventoryOpenButton.GetComponent<Image>().color = ActiveButtonColor;
        }
        else
        {
            inventoryOpenButton.GetComponent<Image>().color = ButtonColor;
            //unitInventorySlots.Clear();
        }
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        inventoryActive = !inventoryActive;
    }





    public void Start()
    {
        InitInventory();
    }

    ////////////////////////////////////
}
