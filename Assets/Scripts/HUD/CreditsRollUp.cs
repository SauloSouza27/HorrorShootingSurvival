using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CreditsRollUp : MonoBehaviour
{
    [SerializeField] private float rollSpeed1 = 2f, rollSpeed2 = 1f;
    [SerializeField] private RectTransform controlerRoll;
    [SerializeField] private Transform assetsCenario;

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
        if(controlerRoll != null)
        {
            controlerRoll.Translate(0, 10 * rollSpeed1 * Time.deltaTime, 0);

            assetsCenario.Translate(0, 0.1f * rollSpeed2 * Time.deltaTime, 0);
        }
    }

    private void VoltarMenu()
    {
        SceneManager.LoadScene(0);
    }
}
