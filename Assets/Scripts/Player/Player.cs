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
    
    // Track if player is aiming manually (toggle/hold) or auto (temporarily from shooting)
    private bool isManuallyAiming = false;
    private bool isAutoAiming = false;
    
    // Returns true if player is currently aiming by any means
    public bool IsAiming => isManuallyAiming || isAutoAiming;
    
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
    }
    

    // Sets manual aiming state and enables/disables aim laser accordingly
    public void SetManualAiming(bool aiming)
    {
        isManuallyAiming = aiming;
        aim?.SetAimLaserEnabled(IsAiming);
    }

    // Toggles manual aiming on/off and updates aim laser state
    public void ToggleManualAiming()
    {
        isManuallyAiming = !isManuallyAiming;
        aim?.SetAimLaserEnabled(IsAiming);
    }

    // Sets automatic aiming state (e.g. when shooting) and updates aim laser
    public void SetAutoAiming(bool aiming)
    {
        isAutoAiming = aiming;
        aim?.SetAimLaserEnabled(IsAiming);
    }

    public bool IsManuallyAiming() => isManuallyAiming;

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
    
}
