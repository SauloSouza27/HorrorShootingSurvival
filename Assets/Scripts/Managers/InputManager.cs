using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    private PlayerControls playerControls;
    public event Action OnOpenClosePauseMenu;

    public InputManager()
    {
        playerControls = new PlayerControls();
        EnableUIInput();

        playerControls.UI.OpenClosePauseMenu.performed += OpenClosePauseMenuPerformed;
    }
    
    private void OpenClosePauseMenuPerformed(InputAction.CallbackContext obj)
    {
        if (SceneManager.GetActiveScene().name != "FirstScene") return;
        OnOpenClosePauseMenu?.Invoke();
    }


    public void EnableUIInput() => playerControls.UI.Enable();
    public void DisableUIInput() => playerControls.UI.Disable();

}
