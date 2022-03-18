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

    private int slotscount = 0;

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

    private void UpdateColliderSize()
    {
        foreach (Transform childSlot in transform)
        {
            childSlot.GetComponent<BoxCollider2D>().size = new Vector2(slotSize.x, slotSize.y);
        }
    }

    public void AddingSlotPreviewStart(S_InventoryUnitSlot addingSlot)
    {
        slotscount++;
        S_InventoryUnitSlot slot = Instantiate(addingSlot, GetComponent<S_CurrentUnitsPanel>().transform); // Copy
        Destroy(slot.GetComponent<Rigidbody2D>()); // MUSTHAVE

        slot.name = (slotscount-1).ToString(); // Name = ID in panel;
        slotSize.x = (panelWidth / slotscount);
        slotSize.y = panelHeight;
        glg.cellSize = slotSize;


        UpdateColliderSize();

        previewActive = true;
    }

    public void AddingSlotPreviewEnd(S_InventoryUnitSlot addingSlot)
    {
        if (previewActive == true) {
            // Destroy Preview
            slotscount--;
            Destroy(transform.GetChild(slotscount).gameObject);
            //
            if (slotscount > 0)
            {
                slotSize.x = (panelWidth / slotscount);
                slotSize.y = panelHeight;
                glg.cellSize = slotSize;
            }
            previewActive = false;

            UpdateColliderSize();

        }
    }

    public void AddUnitSLot(S_InventoryUnitSlot addedSlot) //_LastDragged
    {
        // Destroy Preview ---- NOT NEEDED ANYMORE

        GetComponent<S_CurrentUnitsPanel>().transform.GetChild(transform.childCount - 1).GetComponent<Image>().color = defaultColor;

        //S_InventoryUnitSlot newslot = Instantiate(addedSlot, GetComponent<S_CurrentUnitsPanel>().transform); // Copy
 
        slots.Add(addedSlot);
        addedSlot.SetBelongsToUnitsPanel(true);
        addedSlot.SetCanDrag(false);

        // Size set:
        slotSize.x = (panelWidth / slotscount);
        slotSize.y = panelHeight;
        glg.cellSize = slotSize;

        previewActive = false;
    }

    public void RemoveUnitFromPanel(S_InventoryUnitSlot slotToRemove)
    {
        slotscount--;

        int toRemove = int.Parse(slotToRemove.name);
        int removedID = toRemove; // For ignoring

        slots[toRemove].SetBelongsToUnitsPanel(false);
        slots[toRemove].SetCanDrag(true);
        slots[toRemove].GetComponent<Image>().color = defaultColor;

        slots.Remove(slots[toRemove]);
        Destroy(slotToRemove.gameObject);

        if (slotscount > 0)
        {
            slotSize.x = (panelWidth / slotscount);
            slotSize.y = panelHeight;
            glg.cellSize = slotSize;
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

    public Vector2 GetSize()
    {
        return slotSize;
    }
}
