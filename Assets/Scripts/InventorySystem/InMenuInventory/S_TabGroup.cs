using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_TabGroup : MonoBehaviour
{
    public List<S_TabButton> tabButtons;

    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;
    public S_TabButton selectedTab;

    private void Start()
    {
        if (selectedTab != null) { // Tab group starts first so we need to do this
            selectedTab.background = selectedTab.GetComponent<Image>();
            OnTabSelected(selectedTab); 
        }
    }
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
        if (selectedTab == null || button != selectedTab)
        button.background.sprite = tabHover;
    }

    public void OnTabExit(S_TabButton button)
    {
        if (selectedTab != null && button != selectedTab)
        button.background.sprite = tabIdle;
    }

    public void OnTabSelected(S_TabButton button)
    {
        TabClose(selectedTab);
        selectedTab = button;
        TabOpen(selectedTab);
    }

    public void TabClose(S_TabButton button)
    {
        button.background.sprite = tabIdle;
        button.linkedPanel.SetActive(false);
    }

    public void TabOpen(S_TabButton button)
    {
        button.background.sprite = tabActive;
        button.linkedPanel.SetActive(true);
    }
}
