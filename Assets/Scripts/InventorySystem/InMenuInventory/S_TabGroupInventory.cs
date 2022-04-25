using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_TabGroupInventory : MonoBehaviour
{
    public List<S_TabButton> tabButtons;

    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;
    public S_TabButton selectedTab;

    public void Subscribe(S_TabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<S_TabButton>();
        }
        tabButtons.Add(button);
    }
    public void OnTabEnter(S_TabButton button)
    {
        ResetTabs();
        if (selectedTab == null || button != selectedTab)
        button.background.sprite = tabHover;
    }

    public void OnTabExit(S_TabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(S_TabButton button)
    {
        selectedTab = button;
        ResetTabs();
        button.background.sprite = tabActive;
    }

    public void ResetTabs()
    {
        foreach (S_TabButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab) { continue; }
            button.background.sprite = tabIdle;
        }
    }
}
