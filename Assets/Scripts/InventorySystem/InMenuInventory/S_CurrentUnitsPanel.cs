using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_CurrentUnitsPanel : MonoBehaviour
{
    [SerializeField] int panelWidth;
    [SerializeField] int panelHeight;
    [SerializeField] private GameObject panelParent;
    [SerializeField] private List<S_InventoryUnitSlot> slots = new List<S_InventoryUnitSlot>();

    private Vector2 slotSize;
    [SerializeField]
    public GridLayoutGroup glg = null;
    public bool previewActive = false;
    public Color defaultColor;


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
    }

    public void AddingSlotPreviewStart(S_InventoryUnitSlot addingSlot)
    {
        slots.Add(addingSlot);
        S_InventoryUnitSlot slot = Instantiate(addingSlot, GetComponent<S_CurrentUnitsPanel>().transform); // Copy
        slotSize.x = (panelWidth / slots.Count);
        slotSize.y = panelHeight;
        glg.cellSize = slotSize;

        previewActive = true;
    }

    public void AddingSlotPreviewEnd(S_InventoryUnitSlot addingSlot)
    {
        slots.Remove(addingSlot);
        Destroy(transform.GetChild(slots.Count).gameObject);

        if (slots.Count > 0)
        {
            slotSize.x = (panelWidth / slots.Count);
            slotSize.y = panelHeight;
            glg.cellSize = slotSize;
        }
        previewActive = false;
    }

    public void AddUnitSLot(S_InventoryUnitSlot addedSlot)
    {
        slots.Add(addedSlot);
        addedSlot.SetBelongsToUnitsPanelToTrue();
        S_InventoryUnitSlot slot = Instantiate(addedSlot, GetComponent<S_CurrentUnitsPanel>().transform); // Copy
        slot.GetComponent<Image>().color = defaultColor;
        addedSlot.SetCanDragFalse();
        // Size set:
        slotSize.x = (panelWidth / slots.Count);
        slotSize.y = panelHeight;
        glg.cellSize = slotSize;
    }

    public Vector2 GetSize()
    {
        return slotSize;
    }
}
