using UnityEngine;
using UnityEngine.UI;

public class ToggleControls : MonoBehaviour
{
    private GameObject controls;

    private void Awake()
    {
        controls = GameObject.Find("Controls");

        if(controls != null)
        {
            controls.SetActive(gameObject.GetComponent<Toggle>().isOn);
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
