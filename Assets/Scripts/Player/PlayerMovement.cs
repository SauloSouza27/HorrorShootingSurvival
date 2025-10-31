using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Player player;
    private CharacterController characterController;
    private Animator animator;
    private PlayerStats stats;

    [Header("Movement Info")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float aimingWalkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float gravityScale = 9.81f;
    [SerializeField] private float turnSpeed;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 5f;          // seconds of sprint time
    [SerializeField] private float staminaRegenRate = 1.5f;  // per second
    [SerializeField] private float staminaDrainRate = 1f;    // per second while running
    [SerializeField] private float staminaCooldown = 1.5f;   // delay after exhaustion

    private float currentStamina;
    private bool isRunning;
    private bool canRun = true;
    private float staminaCooldownTimer;

    private float verticalVelocity;
    private float speed;
    private Vector3 movementDirection;
    private Vector2 moveInput;

    private float baseWalkSpeed;
    private float baseAimingWalkSpeed;
    private float baseRunSpeed;
    private float baseMaxStamina;
    private float baseStaminaRegenRate;
    
    public float StaminaNormalized => currentStamina / maxStamina;
    public bool IsRunning => isRunning;

// Debug toggle
    public bool ShowDebugUI = true;


    private void Start()
    {
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        stats = GetComponent<PlayerStats>();

        // cache base values
        baseWalkSpeed = walkSpeed;
        baseAimingWalkSpeed = aimingWalkSpeed;
        baseRunSpeed = runSpeed;
        baseMaxStamina = maxStamina;
        baseStaminaRegenRate = staminaRegenRate;

        currentStamina = maxStamina;

        if (stats != null)
            stats.OnStatsChanged += ApplySpeedAndStaminaFromStats;

        ApplySpeedAndStaminaFromStats();
        AssignInputEvents();
        
        if (!TryGetComponent(out StaminaDebugUI debugUI))
            gameObject.AddComponent<StaminaDebugUI>();

    }

    private void Update()
    {
        if (player.health.isDead || player.health.isDowned)
            return;

        HandleStamina();
        ApplyMovement();
        UpdateRotation();
        AnimatorControllers();
    }

    private void HandleStamina()
    {
        // stamina logic
        if (isRunning && movementDirection.magnitude > 0 && canRun)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                canRun = false;
                staminaCooldownTimer = staminaCooldown;
                StopRunning();
            }
        }
        else
        {
            if (staminaCooldownTimer > 0f)
            {
                staminaCooldownTimer -= Time.deltaTime;
                if (staminaCooldownTimer <= 0f)
                    canRun = true;
            }
            else if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }

        // debug info
        Debug.DrawRay(transform.position + Vector3.up * 2f, Vector3.right * (currentStamina / maxStamina), Color.green);
    }

    private void StopRunning()
    {
        isRunning = false;
        speed = walkSpeed;
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

    private void UpdateRotation()
    {
        Vector3 lookDir = player.aim.GetAimPosition() - transform.position;
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
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
            if (canRun && currentStamina > 0)
            {
                speed = runSpeed;
                isRunning = true;
            }
        };
        controls["Run"].canceled += ctx =>
        {
            StopRunning();
        };
    }

    private void ApplySpeedAndStaminaFromStats()
    {
        float mult = stats != null ? stats.RunSpeedMultiplier : 1f;
        walkSpeed = baseWalkSpeed * mult;
        aimingWalkSpeed = baseAimingWalkSpeed * mult;
        runSpeed = baseRunSpeed * mult;

        // stamina bonus from StaminUp
        if (stats != null && stats.HasPerk(PerkType.StaminUp))
        {
            maxStamina = baseMaxStamina * 2f;
            staminaRegenRate = baseStaminaRegenRate * 1.5f;
        }
        else
        {
            maxStamina = baseMaxStamina;
            staminaRegenRate = baseStaminaRegenRate;
        }

        speed = isRunning ? runSpeed : walkSpeed;
        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }

    private void OnDestroy()
    {
        if (stats != null)
            stats.OnStatsChanged -= ApplySpeedAndStaminaFromStats;
    }
}
