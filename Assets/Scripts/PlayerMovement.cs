using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Player player;
    private PlayerControls controls;
    private CharacterController characterController;
    private Animator animator;

    [Header("Movement Info")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float gravityScale = 9.81f;
    private float verticalVelocity;
    private float speed;
    private Vector3 movementDirection;
    private bool isRunning;

    [Header("Aim Info")]
    [SerializeField] private Transform aim;
    [SerializeField] private LayerMask aimLayerMask;
    private Vector3 aimDirection;
    private Vector2 moveInput;
    private Vector2 controllerAimInput;
    private Vector2 mouseAimInput;

    private void Start()
    {
        player = GetComponent<Player>();

        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        speed = walkSpeed;

        AssignInputEvents();
    }

    private void Update()
    {
        ApplyMovement();
        AimTowardsTarget();
        AnimatorControllers();
    }

    private void AnimatorControllers()
    {
        float xVelocity = Vector3.Dot(movementDirection.normalized, transform.right);
        float zVelocity = Vector3.Dot(movementDirection.normalized, transform.forward);

        animator.SetFloat("xVelocity", xVelocity, .1f, Time.deltaTime);
        animator.SetFloat("zVelocity", zVelocity, .1f, Time.deltaTime);

        bool playRunAnimation = isRunning && movementDirection.magnitude > 0;
        animator.SetBool("isRunning", playRunAnimation);
    }

    private void AimTowardsTarget()
    {
        if (controllerAimInput != Vector2.zero)
        {
            Vector3 controllerAimDirection = new Vector3(controllerAimInput.x, 0, controllerAimInput.y);
            if (controllerAimDirection.sqrMagnitude > 0.01f)
            {
                aimDirection = controllerAimDirection.normalized;
                aim.position = transform.position + aimDirection * 10f;
                transform.forward = aimDirection;
            }
        }
        else if (mouseAimInput != Vector2.zero)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouseAimInput);

            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
            {
                aimDirection = hitInfo.point - transform.position;
                aimDirection.y = 0f;
                aimDirection.Normalize();

                transform.forward = aimDirection;

                aim.position = new Vector3(hitInfo.point.x, transform.position.y + 1, hitInfo.point.z);
            }
        }
    }

    private void ApplyMovement()
    {
        movementDirection = new Vector3(moveInput.x, 0, moveInput.y);
        ApplyGravity();

        if (movementDirection.magnitude > 0)
        {
            characterController.Move(movementDirection * (Time.deltaTime * speed));
        }
    }

    private void ApplyGravity()
    {
        if (!characterController.isGrounded)
        {
            verticalVelocity -= gravityScale * Time.deltaTime;
            movementDirection.y = verticalVelocity;
        }
        else
        {
            verticalVelocity = -.5f;
        }
    }

    private void AssignInputEvents()
    {
        var playerInput = GetComponent<PlayerInput>();
        var controls = playerInput.actions;

        controls["Movement"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls["Movement"].canceled += ctx => moveInput = Vector2.zero;

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

        // Run input action
        controls["Run"].performed += ctx =>
        {
            speed = runSpeed;
            isRunning = true;
        };
        controls["Run"].canceled += ctx =>
        {
            speed = walkSpeed;
            isRunning = false;
        };
    }
}