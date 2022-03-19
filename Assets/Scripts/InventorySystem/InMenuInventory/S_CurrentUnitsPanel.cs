using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S_CurrentUnitsPanel : MonoBehaviour
{
    [SerializeField] int panelWidth;
    [SerializeField] int panelHeight;
    [SerializeField] private GameObject panelParent;

    [SerializeField] private List<S_InventoryUnitSlot> slots = new List<S_InventoryUnitSlot>();
    [SerializeField] private int RosterWeight = 0;
    [SerializeField] private int MaxRosterWeight = 30;
    [SerializeField] private bool OverWeight = false;

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
        placingSlot = false;
        foreach(S_InventoryUnitSlot slot in slots)
        {
            RosterWeight += slot.GetUnitWeight();
        }
        UpdateRosterWeight();
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

        //Roster Weight Control:
        RosterWeight += addingSlot.GetUnitWeight();
        UpdateRosterWeight();

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
            Destroy(transform.GetChild(indexForShuffle).gameObject);
            // Roster Weight control
            RosterWeight -= addingSlot.GetUnitWeight();
            UpdateRosterWeight();
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
        SetLayoutGroupSize((panelWidth / slotscount), panelHeight);

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

        int toRemove = int.Parse(slotToRemove.name);
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

    public void SetRosterWeight(int weight)
    {
        RosterWeight = weight;
    }
    public int GetRosterWeight()
    {
        return RosterWeight;
    }

    public bool GetOverWeightBool()
    {
        return OverWeight;
    }
}
