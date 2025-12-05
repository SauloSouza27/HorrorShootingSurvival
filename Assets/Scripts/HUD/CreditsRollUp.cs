using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CreditsRollUp : MonoBehaviour
{
    [SerializeField] private float rollSpeed = 2f;

    private void Update()
    {
        RollUP();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            VoltarMenu();
        }
    }

    private void RollUP()
    {
        if(gameObject.transform.position.y < gameObject.GetComponent<RectTransform>().rect.height)
        {
            gameObject.transform.Translate(0, 10 * rollSpeed * Time.deltaTime, 0);
        }
        else
        {
            VoltarMenu();
        }
    }

    private void VoltarMenu()
    {
        SceneManager.LoadScene(0);
    }
}
