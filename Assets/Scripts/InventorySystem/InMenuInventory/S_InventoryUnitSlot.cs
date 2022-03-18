using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S_InventoryUnitSlot : MonoBehaviour // IT'S JUST A BLANK SHELL WITH ITEM'S NUMBERS ON IT
{
    //[SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI unitNameTMP;
    [SerializeField] private TextMeshProUGUI unitWeightTMP;
    [SerializeField] private Image unitSpriteGUI;
    [SerializeField] private Button slotButton;
    //[SerializeField] private Button slotButton;

    [SerializeField] private bool belongsToUnitsPanel = false;
    [SerializeField] private bool canDrag = true;

    [SerializeField] private SO_UnitItemData unitData;

    private void Start()
    {
        // Initialize BELONG variables
        if (this.transform.parent.CompareTag("InventoryUnits"))
        {

        }
        else if (this.transform.parent.CompareTag("UnitPanel"))
        {
            belongsToUnitsPanel = true;
        }
    }

    public SO_UnitItemData GetUnitData()
    {
        return unitData;
    }

    public void SetBelongsToUnitsPanel(bool b)
    {
        belongsToUnitsPanel = b;
    }

    public bool GetBelongsToUnitsPanel()
    {
        return belongsToUnitsPanel;
    }

    // SET DISPLAYED VALUES
    public void InitSlotVisualisation(int unitWeight, string unitName, Sprite unitSprite, SO_UnitItemData newData)
    {
        unitNameTMP.text = unitName;
        unitWeightTMP.text = unitWeight.ToString();
        unitSpriteGUI.sprite = unitSprite;
        unitData = newData;
        
    }

    // ON PRESS
    public void AssignSlotButtonCallback(System.Action onClickCallback)
    {
        slotButton.onClick.AddListener(() => onClickCallback());
    }

    public bool GetCanDrag()
    {
        return canDrag;
    }

    public void SetCanDrag(bool b)
    {
        canDrag = b;
    }
}
