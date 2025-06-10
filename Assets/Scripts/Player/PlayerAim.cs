using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    private Player player;

    [Header("Aim Settings")] [SerializeField]
    private bool isToggleAim; // If true, aim input acts as a toggle; otherwise, it must be held.

    [Header("Aim Visual - Laser")] [SerializeField]
    private LineRenderer aimLaser; 

    [Header("Aim Info")] [SerializeField] private Transform aim; 
    [SerializeField] private LayerMask aimLayerMask; // Layers that the mouse aiming raycast will interact with.
    private Vector3 lastValidAimPosition; 
    public Vector2 mouseAimInput { get; private set; } 
    public Vector2 controllerAimInput { get; private set; } 

    private bool isAimingToggled = false; // Note: This field is declared but not currently used.

    //[SerializeField] private float sensitivity;

    private void Start()
    {
        //QualitySettings.vSyncCount = 0; // Set vSyncCount to 0 so that using .targetFrameRate is enabled.
        //Application.targetFrameRate = 60;
        player = GetComponent<Player>();

        AssignInputEvents(); 
    }


    private void Update()
    {
        GetAimPosition(); 
        aim.position = lastValidAimPosition; 

        if (player.IsAiming)
        {
            UpdateAimLaser(); // Refresh the aim laser visuals if the player is aiming.
        }
    }

    // Updates the position, direction, and appearance of the aim laser.
    private void UpdateAimLaser()
    {
        // Enable the laser only if the weapon is ready to fire.
        SetAimLaserEnabled(player.weapon.WeaponReady());

        if (aimLaser.enabled == false)
            return; // Skip laser rendering if it's disabled.

        WeaponModel weaponModel = player.weaponVisuals.CurrentWeaponModel();
        
        // Orient the weapon model and its gunpoint towards the aim target.
        weaponModel.transform.LookAt(aim);
        weaponModel.gunPoint.LookAt(aim);
        
        Transform gunPoint = player.weapon.GunPoint(); // Starting point of the laser.
        Vector3 laserDirection = player.weapon.BulletDirection(); 

        float laserTipLength = .5f; // Additional length for the laser "tip" effect.
        float gunDistance = player.weapon.CurrentWeapon().BulletDistance; // Max range of the laser.

        Vector3 endPoint = gunPoint.position + laserDirection * gunDistance; // Default end point if no obstacle is hit.

        // Check if the laser hits an object within its range.
        if (Physics.Raycast(gunPoint.position, laserDirection, out RaycastHit hit, gunDistance))
        {
            endPoint = hit.point; // Set laser endpoint to the hit location.
            //laserTipLength = 0; // Optionally remove tip if it hits something.
        }

        // Configure the LineRenderer points for the laser beam and its tip.
        aimLaser.SetPosition(0, gunPoint.position); // Laser start.
        aimLaser.SetPosition(1, endPoint); // Laser main beam end / tip start.
        aimLaser.SetPosition(2, endPoint + laserDirection * laserTipLength); // Laser tip end.
    }

    // Calculates the world space aim position based on active input (controller or mouse).
    public Vector3 GetAimPosition()
    {
        if (controllerAimInput.sqrMagnitude > 0.01f)
        {
            // Calculate aim position based on controller input direction and a fixed distance.
            Vector3 aimPosition =
                transform.position +
                new Vector3(controllerAimInput.x, 0, controllerAimInput.y).normalized * 10f; // Project 10 units away.

            aimPosition.y = transform.position.y + 1.6f; // Set aim height relative to player.
            lastValidAimPosition = aimPosition;
            return aimPosition;
        }

        // Use mouse input if no significant controller input.
        if (mouseAimInput != Vector2.zero)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouseAimInput);
            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
            {
                // Aim point is on the raycast hit, with Y fixed relative to player.
                Vector3 aimPosition =
                    new Vector3(hitInfo.point.x, transform.position.y + 1.6f, hitInfo.point.z);

                lastValidAimPosition = aimPosition;
                return aimPosition;
            }
        }

        // If no current input provides a new position, return the last valid one.
        return lastValidAimPosition;
    }

   
    public void SetAimLaserEnabled(bool state)
    {
        aimLaser.enabled = state;
        
    }
    public Transform GetAim() => aim; 

    public bool IsToggleAimEnabled() => isToggleAim;
    
    private void AssignInputEvents()
    {
        var playerInput = GetComponent<PlayerInput>();
        var controls = playerInput.actions; 

        // Handle "ActivateAim" input for starting/stopping manual aim.
        controls["ActivateAim"].performed += ctx =>
        {
            if (isToggleAim) // Toggle behavior for aiming.
            {
                player.ToggleManualAiming();
            }
            else // Hold behavior for aiming.
            {
                player.SetManualAiming(true);
            }
        };

        // If not using toggle aim, explicitly stop aiming when the input is released.
        if (!isToggleAim)
        {
            controls["ActivateAim"].canceled += ctx =>
            {
                player.SetManualAiming(false);
            };
        }

        
        controls["Aim"].performed += ctx =>
        {
            
            if (ctx.control.device is Gamepad)
            {
                controllerAimInput = ctx.ReadValue<Vector2>();
                mouseAimInput = Vector2.zero; 
            }
            else 
            {
                mouseAimInput = ctx.ReadValue<Vector2>();
                controllerAimInput = Vector2.zero; 
            }
        };
        
        controls["Aim"].canceled += ctx =>
        {
            controllerAimInput = Vector2.zero;
            mouseAimInput = Vector2.zero;
        };
    }
}