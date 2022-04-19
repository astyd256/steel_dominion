using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class S_ProfileSettingsManager : MonoBehaviour
{
    [SerializeField] public string userName;
    [SerializeField] public TextMeshProUGUI userNameChangeTMP;
    [SerializeField] public TextMeshProUGUI userNameProfileTMP;
    [SerializeField] public TMP_InputField userNameInputField;
    [SerializeField] public GameObject profileSettingsPanel;
    [SerializeField] public GameObject changeNamePanel;
    [SerializeField] public Button clickScreenButton;
    [SerializeField] public TextMeshProUGUI userNameInMenu;
 
    public void RemoveSpaces()
    {
        userNameInputField.text = userNameInputField.text.Replace(" ", "");
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
}
