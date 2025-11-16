using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSlider : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;


    public void SetMasterVolume(float value)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }

    public void SetMusicVolume(float value)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }

    public void SetSFXVolume(float value)
    {
        mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }


 

}
