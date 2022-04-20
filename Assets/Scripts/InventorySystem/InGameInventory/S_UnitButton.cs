using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S_UnitButton : MonoBehaviour
{
#if !UNITY_SERVER
    public event Action<int> ClientUnitClicked;
    private Button btn;
    [SerializeField] private Image unitSpriteGUI;
    [SerializeField] private TextMeshProUGUI unitNameTMP;
    [SerializeField] private TextMeshProUGUI unitWeightTMP;

    private string unitName;

    public int unitListid = 0;
    public int unitWeight = 0;
    //Recolor button on toggle and save button id to gameplayer

    private void Awake()
    {
        btn = this.GetComponent<Button>();
    }

    public void SetData(SO_UnitItemData unit)
    {
        // Data:
        unitName = unit.displayName;
        unitWeight = unit.GetWeight();
        //Visualization
        unitNameTMP.text = unitName;
        unitWeightTMP.text = unitWeight.ToString();
        unitSpriteGUI.sprite = unit.unitSprite;
    }
    public void ClickButton()
    {
        ToggleButtonLight(true);
        ClientUnitClicked?.Invoke(unitListid);
        
        //Destroy(gameObject);
    }

    public void ToggleButtonLight(bool selected)
    {
        if(selected)
        {
            var colors = btn.colors;
            colors.normalColor = Color.green;
            colors.highlightedColor = Color.green;
            colors.selectedColor = Color.green;
            btn.colors = colors;
        }
        else
        {
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            btn.colors = colors;
        }
    }
#endif
}
