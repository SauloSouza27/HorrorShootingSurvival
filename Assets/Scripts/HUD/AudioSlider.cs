using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;

    [SerializeField] private Slider slider;
    [SerializeField] private VolumeType volumeType;
    float sliderValue = 1f;
    
    void Start()
    {
        if(PlayerPrefs.GetFloat("MasterVolume") != 0.0f )
        {
            switch(volumeType)
            {
                case VolumeType.MasterVolume:
                sliderValue = PlayerPrefs.GetFloat("MasterVolume");
                mixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue)*20);
                slider.value = sliderValue;
                break;
                case VolumeType.MusicVolume:
                sliderValue = PlayerPrefs.GetFloat("MusicVolume");
                mixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue)*20);
                slider.value = sliderValue;
                break;
                case VolumeType.SFXVolume:
                sliderValue = PlayerPrefs.GetFloat("SFXVolume");
                mixer.SetFloat("SFXVolume", Mathf.Log10(sliderValue)*20);
                slider.value = sliderValue;
                break;
            }
        }
        else
        {
            mixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue)*20);
            mixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue)*20);
            mixer.SetFloat("SFXVolume", Mathf.Log10(sliderValue)*20);
        }
    }

    public void SetMasterVolume(float value)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    public enum VolumeType
    {
        MasterVolume,
        MusicVolume,
        SFXVolume
    }
 

}
