using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_MainMenuManager : MonoBehaviour
{
    [SerializeField] private S_CurrentUnitsPanel currentUnitsPanel = null;
    [SerializeField] public Color ActiveButtonColor;
    [SerializeField] public Color ButtonColor;
    [SerializeField] public GameObject mainMenuPanel;

    [SerializeField] private S_ProfileSettingsManager profileSettingsManager;
    [SerializeField] private S_SettingsManager settingsManager;
    [SerializeField] private S_InventoryMenuManager inventoryMenuManager;
    [SerializeField] public Button clickScreenButton;

    [SerializeField] public bool interactive = true;

    public void SavePlayer()
    {
        List<int> unitsIds = new List<int>();

        foreach(Transform unit in currentUnitsPanel.transform)
        {
            int id = unit.gameObject.GetComponent<S_InventoryUnitSlot>().GetUnitData().GetId();
            unitsIds.Add(id);

        }

       // foreach (var unit in inventoryMenuManager.GetUnits())
       // {
       //     unitsIds.Add(unit.id);
       // }

        S_SavePlayerData.SavePlayer("Default", unitsIds);
    }

    public void LoadPlayer()
    {
        // S_SavePlayerData.LoadPlayer();
    }

    // Settings
    public void OpenSettings()
    {
        settingsManager.settingsPanel.SetActive(true);
        interactive = false;
        clickScreenButton.gameObject.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsManager.settingsPanel.SetActive(false);
        interactive = true;
        clickScreenButton.gameObject.SetActive(false);
    }

    // Profile settings
    public void OpenProfileSettings()
    {
        profileSettingsManager.profileSettingsPanel.SetActive(true);
        interactive = false;
        clickScreenButton.gameObject.SetActive(true);
    }

    public void CloseProfileSettings()
    {
        profileSettingsManager.profileSettingsPanel.SetActive(false);
        interactive = true;
        clickScreenButton.gameObject.SetActive(false);
    }

    public void OpenNameChangeMenu()
    {
        profileSettingsManager.changeNamePanel.SetActive(true);
        // Block everything else with ScreenButton
        clickScreenButton.interactable = false;
        // ProfileMenu block:
        profileSettingsManager.profileSettingsPanel.GetComponent<CanvasGroup>().interactable = false;
    }


}
