using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class S_ProfileSettingsManager : MonoBehaviour
{
#if !UNITY_SERVER
    [SerializeField] public string userName;
    [SerializeField] public TextMeshProUGUI userNameChangeTMP;
    [SerializeField] public TextMeshProUGUI userNameProfileTMP;
    [SerializeField] public TMP_InputField userNameInputField;
    [SerializeField] public GameObject profileSettingsPanel;
    [SerializeField] public GameObject changeNamePanel;
    [SerializeField] public Button clickScreenButton;
    [SerializeField] public TextMeshProUGUI userNameInMenu;

    [SerializeField] public GameObject changeIconButton;
    [SerializeField] public GameObject iconChoicePanel;
    [SerializeField] public Image userIcon;
    [SerializeField] public GameObject userIconInMenu;
    [SerializeField] public GameObject userIconInProfile;
 

    public void setChosenIcon()
    {
        userIcon = EventSystem.current.currentSelectedGameObject.transform.GetChild(0).GetComponent<Image>();
        userIconInMenu.GetComponent<Image>().sprite = userIcon.sprite;
        userIconInProfile.GetComponent<Image>().sprite = userIcon.sprite;
        closeIconChangeMenu();
    }

    public void switchIconChangeMenu()
    {
        if (iconChoicePanel.activeSelf)
        {
            iconChoicePanel.SetActive(false);
        }
        else
        {
            iconChoicePanel.SetActive(true);
        }
    }
    public void closeIconChangeMenu()
    {
        iconChoicePanel?.SetActive(false);
    }

    public void RemoveSpaces()
    {
        userNameInputField.text = userNameInputField.text.Replace(" ", "");
    }

    public void profileNameSet()
    {
        userName = userNameInMenu.text;
        userNameProfileTMP.text = userName;
        userNameInputField.text = userName;
    }

    public async void ChangeName()
    {
        if (userNameInputField.text != "")
        {
            userName = userNameChangeTMP.text;
            userNameProfileTMP.text = userName;
            userNameInMenu.text = userName;
            await FirebaseManager.instance.ChangeUsername(userName);

            // Menu close:
            changeNamePanel.SetActive(false);
            clickScreenButton.interactable = true;
            profileSettingsPanel.GetComponent<CanvasGroup>().interactable = true;
        }
    }

    public void CancelNameChange()
    {
        // Menu close:
        changeNamePanel.SetActive(false);
        clickScreenButton.interactable = true;
        profileSettingsPanel.GetComponent<CanvasGroup>().interactable = true;
    }
#endif
}
