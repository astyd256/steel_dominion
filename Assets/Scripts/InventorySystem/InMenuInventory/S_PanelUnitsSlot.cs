using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S_PanelUnitsSlot : MonoBehaviour
{
    //[SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI unitNameTMP;
    [SerializeField] private TextMeshProUGUI unitWeightTMP;
    [SerializeField] private Image unitSpriteGUI;
    //[SerializeField] private Button slotButton;

    // SET DISPLAYED VALUES
    public void InitSlotVisualisation(int unitWeight, string unitName, Sprite unitSprite)
    {
        unitNameTMP.text = unitName;
        unitWeightTMP.text = unitWeight.ToString();
        unitSpriteGUI.sprite = unitSprite;

    }

    // ON PRESS
    public void AssignSlotButtonCallback(System.Action onClickCallback)
    {
        //slotButton.onClick.AddListener(() => onClickCallback());
    }
}
