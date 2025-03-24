using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAim : MonoBehaviour
{
    private Player player;
    private PlayerControls controls;

    [Header("Aim Info")]
    [SerializeField] private Transform aim;
    [SerializeField] private LayerMask aimLayerMask;
    private Vector3 lastValidAimPosition;
    public Vector2 mouseAimInput { get; private set; }
    public Vector2 controllerAimInput { get; private set; }

    private void Start()
    {
        player = GetComponent<Player>();

        AssignInputEvents();
        
    }

    private void Update()
    {
        
        // Update aim visual position
        aim.position = lastValidAimPosition;
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

    private void AssignInputEvents()
    {
        var playerInput = GetComponent<PlayerInput>();
        var controls = playerInput.actions;

        controls["ActivateAim"].performed += ctx =>
        {
            
        };
        
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

        controls["ActivateAim"].canceled += ctx =>
        {
            
        };



    }
}
