using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum AimSource
{
    None,
    Manual,     // Aiming controlled by player input (toggle or hold)
    Shoot       // Temporary aiming triggered by shooting action
}

public class Player : MonoBehaviour
{
     // Singleton instance for easy global access
    
    // Core player component references
    public PlayerControls controls { get; private set; }
    public PlayerAim aim { get; private set; }
    public PlayerMovement movement { get; private set; }
    public PlayerWeaponController weapon { get; private set; }
    public PlayerWeaponVisuals weaponVisuals { get; private set; }
    public PlayerInteraction interaction { get; private set; }
    public Ragdoll ragdoll { get; private set; }
    public PlayerHealth health { get; private set; }
    public Animator animator { get; private set; }

    //HUD
    public Transform playerPerksSlots;
    
    
    private void Awake()
    {
        controls = new PlayerControls();
        
        animator = GetComponentInChildren<Animator>();
        ragdoll = GetComponent<Ragdoll>();
        health = GetComponent<PlayerHealth>();
        aim = GetComponent<PlayerAim>();
        movement = GetComponent<PlayerMovement>();
        weapon = GetComponent<PlayerWeaponController>();
        weaponVisuals = GetComponent<PlayerWeaponVisuals>();
        interaction = GetComponent<PlayerInteraction>();
        //playerPerksSlots = GetComponent<>();
    }
    

    

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
    
}
