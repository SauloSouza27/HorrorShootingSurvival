using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReviveTarget : Interactable
{
    private PlayerHealth downedHealth;
    private Coroutine reviveRoutine;

    [Header("Revive Settings")]
    [SerializeField] private float baseReviveTime = 3f;
    [SerializeField] private float reviveRadius = 1.6f;

    private Collider triggerCol;

    [Header("DEBUG (Gizmos only)")]
    [SerializeField] private bool debugGizmos = true;
    [SerializeField] private Color debugGizmoColor = new Color(0f, 1f, 0.3f, 0.35f);
    private readonly HashSet<Player> playersInRange = new HashSet<Player>();

    // who is reviving now (drives anim + rescuer UI)
    private Player currentRescuer;
    private float currentProgress01;

    public override bool SupportsHighlight => false;

    // Rescuer progress UI
    private ReviveRescuerWorldUI rescuerWorldUI;

    private void Awake()
    {
        // Ensure we have a dedicated trigger collider we can toggle with enable/disable
        triggerCol = GetComponent<Collider>();
        if (triggerCol == null || triggerCol is CharacterController)
            triggerCol = gameObject.AddComponent<SphereCollider>();

        if (triggerCol is SphereCollider sc)
            sc.radius = reviveRadius;

        triggerCol.isTrigger = true;
    }

    private void OnEnable()
    {
        if (triggerCol != null) triggerCol.enabled = true;

        playersInRange.Clear();
        currentRescuer = null;
        currentProgress01 = 0f;
    }

    private void OnDisable()
    {
        if (triggerCol != null) triggerCol.enabled = false;

        // stop rescuer animation & UI if this gets disabled mid-revive
        CleanupReviveState();

        playersInRange.Clear();

        if (reviveRoutine != null)
        {
            StopCoroutine(reviveRoutine);
            reviveRoutine = null;
        }
    }

    public void Init(PlayerHealth health)
    {
        downedHealth = health;
        enabled = false; // off until the player is downed
    }

    public void BeginWaitingForRevive()
    {
        // Hook worldspace UI here if needed (downed side is handled by ReviveDownedWorldUI)
    }

    public override void Interaction(Player rescuer)
    {
        if (!enabled || downedHealth == null || downedHealth.isDead || !downedHealth.isDowned)
            return;

        if (reviveRoutine != null)
            StopCoroutine(reviveRoutine);

        reviveRoutine = StartCoroutine(ReviveProcess(rescuer));
    }

    private IEnumerator ReviveProcess(Player rescuer)
    {
        currentRescuer = rescuer;
        currentProgress01 = 0f;

        // Start rescuer UI (if present)
        rescuerWorldUI = rescuer.GetComponentInChildren<ReviveRescuerWorldUI>(true);
        if (rescuerWorldUI != null)
            rescuerWorldUI.BeginRevive();

        // Start revive animation on rescuer
        SetRescuerReviveAnim(true);

        var stats = rescuer.GetComponent<PlayerStats>();
        float timeMult = stats != null ? Mathf.Max(0.05f, stats.ReviveSpeedMultiplier) : 1f;
        float requiredTime = baseReviveTime * timeMult;

        var rescuerInput = rescuer.GetComponent<PlayerInput>();
        if (rescuerInput == null)
        {
            CleanupReviveState();
            yield break;
        }

        var interactAction = rescuerInput.actions["Interaction"];
        float t = 0f;

        while (t < requiredTime)
        {
            if (!enabled || downedHealth == null || downedHealth.isDead || !downedHealth.isDowned)
            {
                CleanupReviveState();
                yield break;
            }

            float dist = Vector3.Distance(rescuer.transform.position, transform.position);
            if (dist > reviveRadius)
            {
                CleanupReviveState();
                yield break;
            }

            if (interactAction.IsPressed())
            {
                t += Time.deltaTime;
                currentProgress01 = Mathf.Clamp01(t / requiredTime);

                // Update rescuer UI bar
                if (rescuerWorldUI != null)
                    rescuerWorldUI.SetProgress(currentProgress01);
            }
            else
            {
                // Button released -> cancel and reset so we can immediately try again
                CleanupReviveState();
                yield break;
            }

            yield return null;
        }

        // Finished revive
        downedHealth.CompleteRevive();
        currentProgress01 = 1f;

        if (rescuerWorldUI != null)
            rescuerWorldUI.SetProgress(1f);

        CleanupReviveState();
    }

    // Toggle rescuer's "isReviving" animation + weapon visuals
    private void SetRescuerReviveAnim(bool isReviving)
    {
        if (currentRescuer == null) return;

        if (currentRescuer.animator != null)
        {
            if (isReviving)
            {
                currentRescuer.weaponVisuals.ReduceRigWeight();
                currentRescuer.weaponVisuals.SwitchOffAnimationLayer();
                currentRescuer.weaponVisuals.SwitchOffWeaponModels();
                currentRescuer.animator.SetBool("isReviving", true);
            }
            else
            {
                currentRescuer.weaponVisuals.MaximizeRigWeight();
                currentRescuer.weaponVisuals.SwitchOnCurrentWeaponModel();
                currentRescuer.animator.SetBool("isReviving", false);
            }
        }
    }

    private void CleanupReviveState()
    {
        // stop rescuer animation
        SetRescuerReviveAnim(false);

        // stop rescuer UI
        if (rescuerWorldUI != null)
        {
            rescuerWorldUI.EndRevive();
            rescuerWorldUI = null;
        }

        currentRescuer = null;
        reviveRoutine = null;
        currentProgress01 = 0f;
    }

    // ====== Trigger tracking (still useful if you ever want it) ======
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        var p = other.GetComponent<Player>();
        if (p != null) playersInRange.Add(p);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        var p = other.GetComponent<Player>();
        if (p != null) playersInRange.Remove(p);
    }

    // ====== Scene View gizmos only (NOT UI) ======
    private void OnDrawGizmos()
    {
        if (!debugGizmos) return;
        if (!enabled) return;

        Gizmos.color = debugGizmoColor;
        Gizmos.DrawSphere(transform.position, reviveRadius);

        if (currentRescuer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.2f,
                            currentRescuer.transform.position + Vector3.up * 1.2f);
        }
    }
}
