using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSceneHandler : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (AudioManager.Instance == null) return;

        if (scene.name == "MainMenu")
        {
            AudioManager.Instance.SwitchToMenuMusic();
        }
        else if (scene.name == "Gameplay")
        {
            AudioManager.Instance.SwitchToGameplayMusic();
        }
    }
}