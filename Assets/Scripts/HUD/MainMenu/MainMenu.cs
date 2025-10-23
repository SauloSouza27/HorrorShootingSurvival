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


    private void OnEnable()
    {
        playButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettingsMenu);
        quitButton.onClick.AddListener(QuitGame);
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
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void OpenSettingsMenu()
    {
        optionsPanel.SetActive(false);
        settingsPanel.SetActive(true);
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
