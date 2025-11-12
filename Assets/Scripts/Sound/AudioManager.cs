using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer References")]
    public AudioMixer mainMixer; // Assign your MainAudioMixer
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    [Header("SFX Clips")]
    public List<AudioClip> soundEffects = new List<AudioClip>();
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSFXDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSFXDictionary()
    {
        foreach (var clip in soundEffects)
        {
            if (clip != null)
                sfxDictionary[clip.name] = clip;
        }
    }

    // 🎵 MUSIC METHODS
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    public void SwitchToMenuMusic() => PlayMusic(menuMusic);
    public void SwitchToGameplayMusic() => PlayMusic(gameplayMusic);

    // 🔊 SFX METHODS
    public void PlaySFX(string clipName)
    {
        if (sfxDictionary.TryGetValue(clipName, out var clip))
            sfxSource.PlayOneShot(clip);
        else
            Debug.LogWarning($"SFX '{clipName}' not found in AudioManager.");
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // ⚙️ MIXER VOLUME CONTROL
    public void SetMasterVolume(float value) => SetMixerVolume("MasterVolume", value);
    public void SetMusicVolume(float value) => SetMixerVolume("MusicVolume", value);
    public void SetSFXVolume(float value) => SetMixerVolume("SFXVolume", value);

    private void SetMixerVolume(string parameterName, float value)
    {
        // Convert slider 0–1 to mixer’s decibel scale (–80 dB to 0 dB)
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        mainMixer.SetFloat(parameterName, dB);
    }

    public float GetMixerVolume(string parameterName)
    {
        if (mainMixer.GetFloat(parameterName, out float value))
            return Mathf.Pow(10f, value / 20f);
        return 1f;
    }
}
