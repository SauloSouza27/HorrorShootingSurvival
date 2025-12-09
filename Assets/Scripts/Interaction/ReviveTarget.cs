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

    private Player currentRescuer;
    private float currentProgress01;

    public override bool SupportsHighlight => false;

    private ReviveRescuerWorldUI rescuerWorldUI;

    private void Awake()
    {
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
        enabled = false;
    }

    public void BeginWaitingForRevive()
    {
        // hook UI if needed (downed side handled elsewhere)
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

        // 🔹 Pause bleedout while revive is in progress
        if (downedHealth != null)
            downedHealth.SetBeingRevived(true);

        rescuerWorldUI = rescuer.GetComponentInChildren<ReviveRescuerWorldUI>(true);
        if (rescuerWorldUI != null)
            rescuerWorldUI.BeginRevive();

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

                if (rescuerWorldUI != null)
                    rescuerWorldUI.SetProgress(currentProgress01);
            }
            else
            {
                // Button released → cancel revive → resume bleedout
                CleanupReviveState();
                yield break;
            }

            yield return null;
        }

        // succeed
        downedHealth.CompleteRevive();
        currentProgress01 = 1f;

        if (rescuerWorldUI != null)
            rescuerWorldUI.SetProgress(1f);

        CleanupReviveState();
    }

    private void SetRescuerReviveAnim(bool isReviving)
    {
        if (currentRescuer == null) return;
        var cc = currentRescuer.GetComponent<CharacterController>();
        if (currentRescuer.animator != null)
        {
            if (isReviving)
            {
                if (cc != null) cc.enabled = false;
                currentRescuer.weaponVisuals.ReduceRigWeight();
                currentRescuer.weaponVisuals.SwitchOffAnimationLayer();
                currentRescuer.weaponVisuals.SwitchOffWeaponModels();
                currentRescuer.animator.SetBool("isReviving", true);
            }
            else
            {
                if (cc != null) cc.enabled = true;
                currentRescuer.animator.SetBool("isReviving", false);
                currentRescuer.weaponVisuals.MaximizeRigWeight();
                currentRescuer.weaponVisuals.SwitchOnCurrentWeaponModel();
            }
        }
    }

    private void CleanupReviveState()
    {
        // resume bleedout if still downed
        if (downedHealth != null)
            downedHealth.SetBeingRevived(false);

        SetRescuerReviveAnim(false);

        if (rescuerWorldUI != null)
        {
            rescuerWorldUI.EndRevive();
            rescuerWorldUI = null;
        }

        currentRescuer = null;
        reviveRoutine = null;
        currentProgress01 = 0f;
    }

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
