using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour
{
    private PlayerInput playerInput; // Reference to PlayerInput
    private InputAction fireAction; // Fire action input
    private Animator animator;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>(); // Get the PlayerInput component
        animator = GetComponentInChildren<Animator>(); // Get the Animator from children
        
        AssignInputEvents(); // Assign the input actions
    }

    private void AssignInputEvents()
    {
        var controls = playerInput.actions; // Get the controls from PlayerInput

        // Fire action
        fireAction = controls["Fire"]; // Bind to Fire action in Input system
        fireAction.performed += ctx => Shoot(); // When performed, call Shoot()
    }

    private void Shoot()
    {
        animator.SetTrigger("Fire"); // Trigger the fire animation
    }
}