using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    private Player player;
    private PlayerControls controls;

    [Header("Aim Settings")] [SerializeField]
    private bool isToggleAim = false; // Whether the aim input toggles or must be held

    [Header("Aim Visual - Laser")] [SerializeField]
    private LineRenderer aimLaser;

    [Header("Aim Info")] [SerializeField] private Transform aim;
    [SerializeField] private LayerMask aimLayerMask;
    private Vector3 lastValidAimPosition;
    public Vector2 mouseAimInput { get; private set; }
    public Vector2 controllerAimInput { get; private set; }

    private bool isAimingToggled = false;

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
            UpdateAimLaser();
        }
    }

    private void UpdateAimLaser()
    {
        SetAimLaserEnabled(player.weapon.WeaponReady());

        if (aimLaser.enabled == false)
            return;

        WeaponModel weaponModel = player.weaponVisuals.CurrentWeaponModel();
        
        weaponModel.transform.LookAt(aim);
        weaponModel.gunPoint.LookAt(aim);
        
        Transform gunPoint = player.weapon.GunPoint();
        Vector3 laserDirection = player.weapon.BulletDirection();

        float laserTipLength = .5f;
        float gunDistance = 14f;

        Vector3 endPoint = gunPoint.position + laserDirection * gunDistance;

        if (Physics.Raycast(gunPoint.position, laserDirection, out RaycastHit hit, gunDistance))
        {
            endPoint = hit.point;
            //laserTipLength = 0;
        }

        aimLaser.SetPosition(0, gunPoint.position);
        aimLaser.SetPosition(1, endPoint);
        aimLaser.SetPosition(2, endPoint + laserDirection * laserTipLength);
    }

    public Vector3 GetAimPosition()
    {
        if (controllerAimInput.sqrMagnitude > 0.01f)
        {

            Vector3 aimPosition =
                transform.position +
                new Vector3(controllerAimInput.x, 0, controllerAimInput.y).normalized * 10f;

            aimPosition.y = transform.position.y + 1;
            lastValidAimPosition = aimPosition;
            return aimPosition;
        }

        if (mouseAimInput != Vector2.zero)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouseAimInput);
            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
            {
                Vector3 aimPosition =
                    new Vector3(hitInfo.point.x, transform.position.y + 1, hitInfo.point.z);

                lastValidAimPosition = aimPosition;
                return aimPosition;
            }
        }

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

        // This is for toggling or holding the aim input
        controls["ActivateAim"].performed += ctx =>
        {
            if (isToggleAim)
            {
                player.ToggleManualAiming();
            }
            else
            {
                player.SetManualAiming(true);
            }
        };

        if (!isToggleAim)
        {
            controls["ActivateAim"].canceled += ctx =>
            {
                player.SetManualAiming(false);
            };
        }

        // Aim position input (mouse/gamepad direction)
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
