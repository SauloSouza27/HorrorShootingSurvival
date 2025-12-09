using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ToggleControls : MonoBehaviour
{
    private GameObject controls;
    [SerializeField] private Transform toggleControls;

    private void Awake()
    {
        controls = GameObject.Find("Controls");

        if(controls != null)
        {
            controls.SetActive(gameObject.GetComponent<Toggle>().isOn);
        }

        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            toggleControls.gameObject.SetActive(false);
        }
    }

    public void ShowHideControls()
    {
        if(controls != null)
        {
            if (controls.activeSelf)
            {
                controls.SetActive(false);
            }
            else
            {
                controls.SetActive(true);
            }
        }
    }
}
