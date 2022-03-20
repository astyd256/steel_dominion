using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_SettingsManager : MonoBehaviour
{
    [SerializeField] private int _musicVolume;
    [SerializeField] private int _effectsVolume;
    [SerializeField] GameObject _settingsPanel;



    public void OpenSettings()
    {
        _settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        _settingsPanel.SetActive(false);
    }


}
