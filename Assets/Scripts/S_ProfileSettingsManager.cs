using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class S_ProfileSettingsManager : MonoBehaviour
{
    [SerializeField] GameObject _profileSettingsPanel;
    [SerializeField] public string _userName;
    [SerializeField] TextMeshProUGUI _userNameTMP;
    [SerializeField] TMP_InputField _userNameInputField;


    public void OpenProfileSettings()
    {
        _profileSettingsPanel.SetActive(true);
    }

    public void CloseProfileSettings()
    {
        _profileSettingsPanel.SetActive(false);
    }

    public void StartEditingName()
    {
        _userNameInputField.interactable = true;
    }

    public void ChangeName()
    {
        _userName = _userNameTMP.text;
        _userNameInputField.interactable = false;
    }
}
