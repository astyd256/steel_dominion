using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class S_SettingsManager : MonoBehaviour
{
    [SerializeField] private float _musicVolume;
    [SerializeField] private float _effectsVolume;

    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _effectsSlider;

    [SerializeField] public GameObject settingsPanel;


    public void MusicVolumeChange()
    {
        float newValue = _musicSlider.value;
        if (newValue >= 0 && newValue <= 100)
        {
            _musicVolume = newValue;
        }
    }
    public void EffectsVolumeChange()
    {
        float newValue = _effectsSlider.value;
        if (newValue >= 0 && newValue <= 100)
        {
            _effectsVolume = newValue;
        }
    }



}
