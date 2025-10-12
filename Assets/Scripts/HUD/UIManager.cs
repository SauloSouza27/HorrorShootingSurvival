using UnityEngine;

public class UIManager : MonoBehaviour
{
   [Header("Panels")]
   [SerializeField] private GameObject SettingsPanel;
   [SerializeField] private GameObject PausePanel;

   private void Awake()
   {
       SettingsPanel.SetActive(false);
       PausePanel.SetActive(false);
   }

   private void Start()
   {
       GameManager.Instance.InputManager.OnOpenClosePauseMenu += OpenClosePauseMenu;
   }

   public void OpenClosePauseMenu()
   {
       if (PausePanel.activeSelf == false && SettingsPanel.activeSelf == false)
       {
            PausePanel.SetActive(true);
            SettingsPanel.SetActive(false);
       }
       else
       {
            PausePanel.SetActive(false);
            SettingsPanel.SetActive(false);   
       }
   }

    public void OpenSettingsPanel()
    {
        SettingsPanel.SetActive(true);
        PausePanel.SetActive(false);
    }

    public void CloseSettingsPanel()
    {
        PausePanel.SetActive(true);
        SettingsPanel.SetActive(false);
    }
}