using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerControls controls { get; private set; }
    public PlayerAim aim { get; private set; }
    public PlayerMovement movement { get; private set; }
    public PlayerWeaponController weapon { get; private set; }
    
    public PlayerWeaponVisuals weaponVisuals { get; private set; }
    
    public bool IsAiming { get; private set; } = false;

    private void Awake()
    {
        controls = new PlayerControls();
        aim = GetComponent<PlayerAim>();
        movement = GetComponent<PlayerMovement>();
        weapon = GetComponent<PlayerWeaponController>();
        weaponVisuals = GetComponent<PlayerWeaponVisuals>();
    }
    
    public void SetAiming(bool aiming)
    {
        IsAiming = aiming;
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
