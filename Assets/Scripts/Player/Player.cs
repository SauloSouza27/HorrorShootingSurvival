using System;
using UnityEngine;
public enum AimSource
{
    None,
    Manual,     // From toggle/hold input
    Shoot       // Temporarily from shooting
}
public class Player : MonoBehaviour
{
    public PlayerControls controls { get; private set; }
    public PlayerAim aim { get; private set; }
    public PlayerMovement movement { get; private set; }
    public PlayerWeaponController weapon { get; private set; }
    
    public PlayerWeaponVisuals weaponVisuals { get; private set; }
    
    private bool isManuallyAiming = false;
    private bool isAutoAiming = false;
    public bool IsAiming => isManuallyAiming || isAutoAiming;

    private void Awake()
    {
        controls = new PlayerControls();
        aim = GetComponent<PlayerAim>();
        movement = GetComponent<PlayerMovement>();
        weapon = GetComponent<PlayerWeaponController>();
        weaponVisuals = GetComponent<PlayerWeaponVisuals>();
    }
    
    public void SetManualAiming(bool aiming)
    {
        isManuallyAiming = aiming;
        aim?.SetAimLaserEnabled(IsAiming);
    }

    public void ToggleManualAiming()
    {
        isManuallyAiming = !isManuallyAiming;
        aim?.SetAimLaserEnabled(IsAiming);
    }

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
