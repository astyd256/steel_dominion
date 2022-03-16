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
    private int slotNumber;
    private Vector2 slotSize;
    [SerializeField]
    public GridLayoutGroup glg = null;

    private void Awake()
    {
        slotSize = new Vector2(panelWidth, panelHeight);
        slotNumber = slots.Count;
        slotSize.x = (panelWidth / slots.Count);
        slotSize.y = panelHeight;
        Debug.Log(slotSize);
        glg.cellSize = slotSize;

        foreach (S_InventoryUnitSlot slot in slots) {
            slot.GetComponent<BoxCollider2D>().size = slotSize;
        }
    }
}
