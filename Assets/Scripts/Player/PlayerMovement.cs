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
    [SerializeField] private float turnSpeed;
    private float verticalVelocity;
    private float speed;
    private Vector3 movementDirection;
    private Vector2 moveInput;
    private bool isRunning;
    
    private readonly int idleToWalkBlendTreeHash = Animator.StringToHash("idleToWalk");

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
        UpdateRotation();
        AnimatorControllers();
    }

    private void AnimatorControllers()
    {
        if (movementDirection.magnitude == 0)
        {
            animator.SetFloat(idleToWalkBlendTreeHash, 0, .1f, Time.deltaTime );
        }
        else
        {
            animator.SetFloat(idleToWalkBlendTreeHash, 1, .1f, Time.deltaTime );
        }
        
        animator.SetBool("isAiming", player.IsAiming);
        
        float xVelocity = Vector3.Dot(movementDirection.normalized, transform.right);
        float zVelocity = Vector3.Dot(movementDirection.normalized, transform.forward);

        animator.SetFloat("xVelocity", xVelocity, .1f, Time.deltaTime);
        animator.SetFloat("zVelocity", zVelocity, .1f, Time.deltaTime);

        bool playRunAnimation = isRunning && movementDirection.magnitude > 0;
        animator.SetBool("isRunning", playRunAnimation);
    }

    private void UpdateRotation()
    {
        if (player.IsAiming)
        {
            Vector3 lookingDirection = player.aim.GetAimPosition() - transform.position;
            lookingDirection.y = 0f;
            if (lookingDirection != Vector3.zero)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(lookingDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * turnSpeed);
            }
        }
        else if (movementDirection.sqrMagnitude > 0.01f)
        {
            movementDirection.y = 0f;
            Quaternion desiredRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * turnSpeed);
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
