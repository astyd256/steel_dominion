using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S_InventoryUnitSlot : MonoBehaviour
{
    //[SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI unitNameTMP;
    [SerializeField] private TextMeshProUGUI unitWeightTMP;
    //[SerializeField] private Button slotButton;

    public void InitSlotVisualisation(int unitWeight, string unitName)
    {
        unitNameTMP.text = unitName;
        unitWeightTMP.text = unitWeight.ToString();
    }

    public void AssignSlotButtonCallback(System.Action onClickCallback)
    {
        //slotButton.onClick.AddListener(() => onClickCallback());
    }
}
