using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer References")]
    public AudioMixer mainMixer;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX Mixer Group (for 3D sounds)")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;   // ⬅️ assign in Inspector

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;
    public AudioClip creditsMusic;

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

    // ===== MUSIC (unchanged) =====
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
    public void SwitchToCreditsMusic() => PlayMusic(creditsMusic);

    // ===== 2D SFX (unchanged) =====
    public void PlaySFX(string clipName, float volume = 1f, float pitch = 1f)
    {
        if (sfxDictionary.TryGetValue(clipName, out var clip))
            sfxSource.PlayOneShot(clip, volume);
        else
            Debug.LogWarning($"SFX '{clipName}' not found in AudioManager.");
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // ===== NEW: 3D positional SFX =====
    public void PlaySFX3D(AudioClip clip,
                          Vector3 position,
                          float volume = 1f,
                          float spatialBlend = 1f,
                          float minDistance = 5f,
                          float maxDistance = 40f)
    {
        if (clip == null) return;

        GameObject go = new GameObject("3D SFX - " + clip.name);
        go.transform.position = position;

        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = volume;
        src.spatialBlend = spatialBlend;              // 1 = fully 3D
        src.minDistance = minDistance;
        src.maxDistance = maxDistance;
        src.rolloffMode = AudioRolloffMode.Linear;    // or Logarithmic if you prefer
        src.playOnAwake = false;
        src.loop = false;

        if (sfxMixerGroup != null)
            src.outputAudioMixerGroup = sfxMixerGroup;

        src.Play();
        Destroy(go, clip.length + 0.1f);
    }
}
