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
