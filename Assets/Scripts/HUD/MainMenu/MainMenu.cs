using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject optionsPanel;
    [Header("Menu UI properties")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button closeSettings;
    [SerializeField] private Button creditsButton;


    private void OnEnable()
    {
        playButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
            StartGame();
        });
        settingsButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
            OpenSettingsMenu();
        });
        closeSettings.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
            CloseSettingsMenu();
        });
        quitButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
            QuitGame();
        });
        creditsButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
            OpenCredits();
        });
    }

    private void OpenCredits()
    {
        AudioManager.Instance.SwitchToCreditsMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

    private void StartGame()
    {
        // Reset timescale just in case game was paused
        Time.timeScale = 1;

        // Clear singletons (to avoid “ghost” objects blocking logic)
        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }
        
        AudioManager.Instance.PlaySFX("ButtonClick");
        AudioManager.Instance.SwitchToGameplayMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OpenSettingsMenu()
    {
        optionsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void CloseSettingsMenu()
    {
        optionsPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else   
        Application.Quit();
#endif
    }

}
