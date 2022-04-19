using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class S_ProfileSettingsManager : MonoBehaviour
{
    [SerializeField] public string userName;
    [SerializeField] TextMeshProUGUI _userNameChangeTMP;
    [SerializeField] TextMeshProUGUI _userNameProfileTMP;
    [SerializeField] TMP_InputField _userNameInputField;
    [SerializeField] public GameObject profileSettingsPanel;
    [SerializeField] public GameObject changeNamePanel;
    [SerializeField] public Button clickScreenButton;
    [SerializeField] public TextMeshProUGUI _userNameInMenu;
    private void Start()
    {
        LoginInterfaceManager.instance.setPlayerProfileValues();

        userName = FirebaseManager.instance.GetUserName();
        _userNameProfileTMP.text = userName;
        _userNameInputField.text = userName;

        _userNameInputField.onValueChanged.AddListener(delegate { RemoveSpaces(); });
    }
    void RemoveSpaces()
    {
        _userNameInputField.text = _userNameInputField.text.Replace(" ", "");
    }

    public async void ChangeName()
    {
        if (_userNameInputField.text != "")
        {
            userName = _userNameChangeTMP.text;
            _userNameProfileTMP.text = userName;
            _userNameInMenu.text = userName;
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
