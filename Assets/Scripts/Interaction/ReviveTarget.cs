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

    // ====== DEBUG toggles/fields ======
    [Header("DEBUG")]
    [SerializeField] private bool debugUI = true;
    [SerializeField] private Color debugGizmoColor = new Color(0f, 1f, 0.3f, 0.35f);
    private readonly HashSet<Player> playersInRange = new HashSet<Player>();
    public bool AnyPlayerInRange => playersInRange.Count > 0;   // exposed for debug

    private Player currentRescuer;   // who is reviving now (debug + anim)
    private float currentProgress01; // 0..1 progress (debug)
    // ==================================

    public override bool SupportsHighlight => false;
    
    private PlayerWeaponVisuals visualController;

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

        // 🔹 Make sure we turn off revive anim if this gets disabled mid-revive
        SetRescuerReviveAnim(false);

        playersInRange.Clear();
        currentRescuer = null;
        currentProgress01 = 0f;

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

    public void BeginWaitingForRevive() { /* hook worldspace UI if you want */ }

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

        // 🔹 Start revive animation on rescuer
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
            }
            else
            {
                // Button released -> cancel and reset so we can immediately try again
                CleanupReviveState();
                yield break;
            }

            yield return null;
        }

        downedHealth.CompleteRevive();
        currentProgress01 = 1f;
        CleanupReviveState();
    }

    //  Helper: toggle rescuer's "isReviving" bool
    private void SetRescuerReviveAnim(bool isReviving)
    {
        if (currentRescuer == null) return;

        // assumes Player has public Animator animator;
        if (currentRescuer.animator != null)
        {
            if (isReviving)
            {
                Debug.Log("true revive");
                currentRescuer.weaponVisuals.ReduceRigWeight();
                currentRescuer.weaponVisuals.SwitchOffAnimationLayer();
                currentRescuer.weaponVisuals.SwitchOffWeaponModels();
                currentRescuer.animator.SetBool("isReviving", true);
            }
            else
            {
                Debug.Log("false revive");
                currentRescuer.weaponVisuals.MaximizeRigWeight();
                currentRescuer.weaponVisuals.SwitchOnCurrentWeaponModel();
                currentRescuer.animator.SetBool("isReviving", false);
            }
            
            
        }
    }

    private void CleanupReviveState()
    {
        //  Stop the revive animation when revive ends/cancels
        SetRescuerReviveAnim(false);

        currentRescuer = null;
        reviveRoutine = null;
        currentProgress01 = 0f;
    }

    // ====== Trigger tracking just for "in range" debug text ======
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
    // =============================================================

    // ====== DEBUG VISUALS ======
    private void OnDrawGizmos()
    {
        if (!debugUI) return;
        if (!enabled) return;

        Gizmos.color = debugGizmoColor;
        Gizmos.DrawSphere(transform.position, reviveRadius);

        // line to current rescuer
        if (currentRescuer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position + Vector3.up * 1.2f, currentRescuer.transform.position + Vector3.up * 1.2f);
        }
    }

    private void OnGUI()
    {
        if (!debugUI) return;
        if (!enabled) return;

        var cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = transform.position + Vector3.up * 2.0f; // above downed player
        Vector3 screen = cam.WorldToScreenPoint(worldPos);
        if (screen.z < 0) return; // behind camera

        screen.y = Screen.height - screen.y;

        var size = new Vector2(220, 48);
        var rect = new Rect(screen.x - size.x * 0.5f, screen.y - size.y, size.x, size.y);
        var barRect = new Rect(rect.x + 8, rect.yMax - 18, rect.width - 16, 10);

        GUI.color = new Color(0, 0, 0, 0.65f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;

        string header = AnyPlayerInRange ? "Rescuer in range" : "No rescuer nearby";
        GUI.Label(new Rect(rect.x + 8, rect.y + 6, rect.width - 16, 18), header);

        string detail = currentRescuer != null
            ? $"Holding Interact... {Mathf.RoundToInt(currentProgress01 * 100)}%"
            : "Hold Interact to revive";

        GUI.Label(new Rect(rect.x + 8, rect.y + 22, rect.width - 16, 16), detail);

        GUI.color = new Color(1, 1, 1, 0.15f);
        GUI.DrawTexture(barRect, Texture2D.whiteTexture);

        GUI.color = currentRescuer != null ? Color.green : new Color(1f, 1f, 1f, 0.25f);
        var fill = new Rect(barRect.x, barRect.y, barRect.width * Mathf.Clamp01(currentProgress01), barRect.height);
        GUI.DrawTexture(fill, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
    // ============================
}
