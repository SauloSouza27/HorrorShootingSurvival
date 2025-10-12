using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Button backButton;

    private void Start()
    {
        backButton.onClick.AddListener(CloseSettingsMenu);
    }


    private void CloseSettingsMenu()
    {
        transform.parent.Find("Options").gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

}
