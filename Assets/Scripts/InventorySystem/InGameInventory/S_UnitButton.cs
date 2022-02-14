using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_UnitButton : MonoBehaviour
{
    public event Action<int> ClientUnitClicked;
    private Button btn;

    public int unitListid = 0;
    //Recolor button on toggle and save button id to gameplayer

    private void Awake()
    {
        btn = this.GetComponent<Button>();
    }
    public void ClickButton()
    {
        ClientUnitClicked?.Invoke(unitListid);
        ToggleButtonLight(true);
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

}
