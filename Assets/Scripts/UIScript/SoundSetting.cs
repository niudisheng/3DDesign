using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SoundSetting : MonoBehaviour
{
    public Slider MusicVolume;
    public Slider EffectVolume;

    public void Start()
    {
        MusicVolume.onValueChanged.AddListener(OnMusicVolumeChanged);
        EffectVolume.onValueChanged.AddListener(OnEffectVolumeChanged);
    }
    private void OnMusicVolumeChanged(float value)
    {
        SoundManager.Instance.SetMusicVolume(value);
    }
    private void OnEffectVolumeChanged(float value)
    {
        SoundManager.Instance.SetEffectVolume(value);
    }
}