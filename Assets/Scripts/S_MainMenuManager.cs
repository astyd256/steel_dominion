using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_MainMenuManager : MonoBehaviour
{
#if !UNITY_SERVER
    [SerializeField] private S_CurrentUnitsPanel currentUnitsPanel = null;
    [SerializeField] public Color ActiveButtonColor;
    [SerializeField] public Color ButtonColor;
    [SerializeField] public GameObject mainMenuPanel;

    [SerializeField] private S_ProfileSettingsManager profileSettingsManager;
    [SerializeField] private S_SettingsManager settingsManager;
    [SerializeField] private S_InventoryMenuManager inventoryMenuManager;
    [SerializeField] public Button clickScreenButton;

    [SerializeField] private GameObject inventoryPanel;    // Inventory panel
    [SerializeField] private Button inventoryOpenButton;   // Inventory open button
    [SerializeField] private GameObject inventoryUnitsParent; 
    [SerializeField] bool inventoryActive = false;

    [SerializeField] public bool interactive = true;
    [SerializeField] public bool unitsDeckBuildActive = true;
    [SerializeField] public bool ordersDeckBuildActive = false;


    private void Start()
    {
        //userName = FirebaseManager.instance.GetUserName();
        

        profileSettingsManager.userNameInputField.onValueChanged.AddListener(delegate { profileSettingsManager.RemoveSpaces(); });


    }

    public S_CurrentUnitsPanel GetUnitsPanel()
    {
        return currentUnitsPanel;
    }
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

        S_SavePlayerData.SavePlayer(FirebaseManager.instance.GetUserName(), unitsIds);
    }

    public void LoadPlayer()
    {
        // S_SavePlayerData.LoadPlayer();
    }

    //Panel response on inventory button press
    public void SwitchInventory()
    {
        if (!inventoryActive)
        {
            inventoryOpenButton.GetComponent<Image>().color = ActiveButtonColor;
        }
        else
        {
            inventoryOpenButton.GetComponent<Image>().color = ButtonColor;
            //unitInventorySlots.Clear();

            // CODE FOR REVERSING CHANGES BECAUSE SAVE WASNT PRESSED:
            if (currentUnitsPanel.SaveInventoryButton.activeSelf == true)
                currentUnitsPanel.ReverseSlots();

            currentUnitsPanel.SaveInventoryButton.SetActive(false);
        }
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        inventoryActive = !inventoryActive;

        // SlotSize refresh
        if (inventoryActive)
        {
            inventoryUnitsParent.GetComponent<GridLayoutGroup>().cellSize = inventoryMenuManager.GetSlotSize();
        }
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
        if (settingsManager.settingsPanel.activeSelf)
        {
            settingsManager.settingsPanel.SetActive(false);
            interactive = true;
            clickScreenButton.gameObject.SetActive(false);
        }
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
        if (profileSettingsManager.profileSettingsPanel.activeSelf)
        {

            profileSettingsManager.profileSettingsPanel.SetActive(false);

            if (profileSettingsManager.iconChoicePanel.activeSelf)
                profileSettingsManager.iconChoicePanel.SetActive(false);

            interactive = true;
            clickScreenButton.gameObject.SetActive(false);
        }
    }

    public void OpenNameChangeMenu()
    {
        profileSettingsManager.changeNamePanel.SetActive(true);
        // Block everything else with ScreenButton
        clickScreenButton.interactable = false;
        // ProfileMenu block:
        profileSettingsManager.profileSettingsPanel.GetComponent<CanvasGroup>().interactable = false;
    }

    public bool getInventoryActive()
    {
        return inventoryActive;
    }

    public void CloseInventory()
    {
        if (inventoryActive)
        {
            inventoryActive = false;
            inventoryPanel.SetActive(false);
        }
    }

    public void shutEverything()
    {
        CloseSettings();

        CloseInventory();

        CloseProfileSettings();
    }

    public void screenButtonOn()
    {
        interactive = false;
        clickScreenButton.gameObject.SetActive(true);
    }

    public void screenButtonOff()
    {
        interactive = true;
        clickScreenButton.gameObject.SetActive(false);
    }

    public void ClickScreenButtonLogic()
    {
        CloseSettings();

        CloseProfileSettings();
    }

#endif
}
