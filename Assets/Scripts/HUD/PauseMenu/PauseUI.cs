using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour
{
   [SerializeField] private Button continueButton;
   [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitToMenuButton;
    [SerializeField] private Button closeSettingsButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(ClosePauseMenu);
        settingsButton.onClick.AddListener(OpenSettingsMenu);
        quitToMenuButton.onClick.AddListener(GoToMainMenu);
        closeSettingsButton.onClick.AddListener(CloseSettingsMenu);

    }

   private void ClosePauseMenu()
   {
        GameManager.Instance.UIManager.OpenClosePauseMenu();
   }

    private void OpenSettingsMenu()
    {
        GameManager.Instance.UIManager.OpenSettingsPanel();
    }
   
    private void CloseSettingsMenu()
    {
        GameManager.Instance.UIManager.CloseSettingsPanel();
    }

   private void GoToMainMenu()
   {
       SceneManager.LoadScene("MainMenu");
   }
}
