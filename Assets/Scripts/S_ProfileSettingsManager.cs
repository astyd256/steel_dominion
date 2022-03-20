using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class S_ProfileSettingsManager : MonoBehaviour
{
    [SerializeField] public string _userName;
    [SerializeField] TextMeshProUGUI _userNameChangeTMP;
    [SerializeField] TextMeshProUGUI _userNameProfileTMP;
    [SerializeField] TMP_InputField _userNameInputField;
    [SerializeField] public GameObject profileSettingsPanel;
    [SerializeField] public GameObject changeNamePanel;
    [SerializeField] public Button clickScreenButton;

    public void ChangeName()
    {
        _userName = _userNameChangeTMP.text;
        _userNameInputField.interactable = false;
        _userNameProfileTMP.text = _userName;

        // Menu close:
        changeNamePanel.SetActive(false);
        clickScreenButton.interactable = true;
        profileSettingsPanel.GetComponent<CanvasGroup>().interactable = true;
    }

    public void CancelNameChange()
    {
        // Menu close:
        changeNamePanel.SetActive(false);
        clickScreenButton.interactable = true;
        profileSettingsPanel.GetComponent<CanvasGroup>().interactable = true;
    }
}
