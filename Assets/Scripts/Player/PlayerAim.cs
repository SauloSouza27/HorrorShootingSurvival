using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    private Player player;
    
    [Header("Aim Constraints")]
    [SerializeField] private float minAimDistance = 1f;

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
        if (player.health.isDead)
            return;
        
        GetAimPosition(); 
        aim.position = lastValidAimPosition; 
        UpdateGunVisuals(); // Refresh the aim laser visuals if the player is aiming.
    }

    // Updates the position, direction, and appearance of the aim laser.
    private void UpdateGunVisuals()
    {
        if (!player.weapon.WeaponReady())
            return;
        
        WeaponModel weaponModel = player.weaponVisuals.CurrentWeaponModel();

        Vector3 flatAimDir = (aim.position - weaponModel.transform.position);
        flatAimDir.y = 0f;

        if (flatAimDir.sqrMagnitude > 0.001f)
        {
            Quaternion flatRot = Quaternion.LookRotation(flatAimDir);
            weaponModel.transform.rotation =
                Quaternion.Slerp(weaponModel.transform.rotation, flatRot, Time.deltaTime * 15f);
        }

        Vector3 flatGunDir = (aim.position - weaponModel.gunPoint.position);
        flatGunDir.y = 0f;

        if (flatGunDir.sqrMagnitude > 0.001f)
        {
            Quaternion gunRot = Quaternion.LookRotation(flatGunDir);
            weaponModel.gunPoint.rotation = gunRot;
        }
    }

    // Calculates the world space aim position based on active input (controller or mouse).
    public Vector3 GetAimPosition()
    {
        Vector3 aimPosition = lastValidAimPosition;

        if (controllerAimInput.sqrMagnitude > 0.01f)
        {
            aimPosition = transform.position +
                          new Vector3(controllerAimInput.x, 0, controllerAimInput.y).normalized * 10f;
            aimPosition.y = transform.position.y + 1.6f;
        }
        else if (mouseAimInput != Vector2.zero)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouseAimInput);
            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
            {
                aimPosition = new Vector3(hitInfo.point.x, transform.position.y + 1.6f, hitInfo.point.z);
            }
        }

        // ✅ Prevent aim from getting too close to player
        Vector3 flatDir = aimPosition - transform.position;
        flatDir.y = 0f;

        if (flatDir.magnitude < minAimDistance)
        {
            flatDir = flatDir.normalized * minAimDistance;
            aimPosition = transform.position + flatDir;
            aimPosition.y = transform.position.y + 1.6f; 
        }

        lastValidAimPosition = aimPosition;
        return aimPosition;
    }
    
    private void AssignInputEvents()
    {
        var playerInput = GetComponent<PlayerInput>();
        var controls = playerInput.actions; 
        
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