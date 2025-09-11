using System.Collections;
using UnityEngine;

/// <summary>
/// Buyable elevator platform (players stand on and press Interact).
/// Toggles between bottom/top points. Charges the interacting player.
/// Locks for a cooldown if the **same player** uses it twice consecutively.
/// </summary>
public class ElevatorPlatform : Interactable
{
    public override bool RemoveAfterInteract => false; // reusable

    [Header("Cost & Timing")]
    [SerializeField] private int costPerRide = 500;
    [SerializeField] private float travelTime = 2.0f;
    [SerializeField] private float lockCooldown = 6.0f; // seconds locked after abuse

    [Header("Points")]
    [Tooltip("Bottom world point the platform moves to.")]
    [SerializeField] private Transform bottomPoint;
    [Tooltip("Top world point the platform moves to.")]
    [SerializeField] private Transform topPoint;

    [Header("Platform")]
    [Tooltip("The moving platform (usually this.transform).")]
    [SerializeField] private Transform platform;

    [Header("Debug UI")]
    [SerializeField] private bool debugUI = true;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2f, 0);

    // state
    public bool IsMoving { get; private set; }
    public bool IsLocked { get; private set; }
    public bool IsAtTop { get; private set; } // false = bottom, true = top
    private float cooldownTimer;

    // abuse lock (same player twice consecutively)
    private int lastUserPlayerIndex = -1;
    private int consecutiveUsesByLastUser = 0;
    private const int MaxConsecutiveUses = 2;

    private void Reset()
    {
        platform = transform;
    }

    private void Start()
    {
        if (platform == null) platform = transform;

        // Initialize position to the nearest point at start
        float dTop = Vector3.Distance(platform.position, topPoint ? topPoint.position : platform.position);
        float dBottom = Vector3.Distance(platform.position, bottomPoint ? bottomPoint.position : platform.position);
        IsAtTop = (dTop < dBottom);
    }

    private void Update()
    {
        if (IsLocked)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                IsLocked = false;
                consecutiveUsesByLastUser = 0; // reset abuse counter on unlock
                lastUserPlayerIndex = -1;
            }
        }
    }

    /// <summary>
    /// Toggle move (up if at bottom, down if at top).
    /// </summary>
    public override void Interaction(Player player)
    {
        if (player == null) return;
        if (IsMoving || IsLocked) return;

        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (!stats.SpendPoints(costPerRide))
        {
            // optional: play denied SFX here
            return;
        }

        int pIndex = player.GetComponent<UnityEngine.InputSystem.PlayerInput>()?.playerIndex ?? -1;
        StartCoroutine(MoveToOtherFloor(pIndex));
    }

    /// <summary>
    /// External request (from a call button) to move to a specific floor.
    /// Charges the caller. If already at desired floor, does nothing (but still blocks if moving/locked).
    /// </summary>
    public bool RequestMoveToFloor(Player caller, bool moveToTop)
    {
        if (caller == null) return false;
        if (IsMoving || IsLocked) return false;
        if (moveToTop == IsAtTop) return false; // already there

        var stats = caller.GetComponent<PlayerStats>();
        if (stats == null) return false;

        if (!stats.SpendPoints(costPerRide))
            return false;

        int pIndex = caller.GetComponent<UnityEngine.InputSystem.PlayerInput>()?.playerIndex ?? -1;
        StartCoroutine(MoveTo(moveToTop, pIndex));
        return true;
    }

    private IEnumerator MoveToOtherFloor(int playerIndex)
    {
        bool moveToTop = !IsAtTop;
        yield return MoveTo(moveToTop, playerIndex);
    }

    private IEnumerator MoveTo(bool moveToTop, int playerIndex)
    {
        IsMoving = true;

        Vector3 start = platform.position;
        Vector3 end = (moveToTop ? topPoint.position : bottomPoint.position);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, travelTime);
            platform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        platform.position = end;
        IsAtTop = moveToTop;
        IsMoving = false;

        // Abuse detection: same player twice consecutively → lock
        if (playerIndex == lastUserPlayerIndex)
        {
            consecutiveUsesByLastUser++;
        }
        else
        {
            lastUserPlayerIndex = playerIndex;
            consecutiveUsesByLastUser = 1;
        }

        if (consecutiveUsesByLastUser >= MaxConsecutiveUses)
        {
            LockElevator();
        }
    }

    private void LockElevator()
    {
        IsLocked = true;
        cooldownTimer = lockCooldown;
        // optional: SFX/UI feedback here
    }

    // ------------ DEBUG UI ------------
    private void OnGUI()
    {
        if (!debugUI) return;
        var cam = Camera.main; if (!cam) return;

        Vector3 screen = cam.WorldToScreenPoint(platform.position + uiOffset);
        if (screen.z < 0) return;
        screen.y = Screen.height - screen.y;

        var rect = new Rect(screen.x - 140, screen.y - 52, 280, 48);
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;

        string line1 = IsLocked
            ? $"ELEVATOR LOCKED ({cooldownTimer:0.0}s)"
            : (IsMoving ? "Moving..." : $"Press Interact to ride ({costPerRide})");

        string line2 = $"Pos: {(IsAtTop ? "Top" : "Bottom")}   Next: {(IsAtTop ? "Down" : "Up")}";

        GUI.Label(new Rect(rect.x + 8, rect.y + 6, rect.width - 16, 18), line1);
        GUI.Label(new Rect(rect.x + 8, rect.y + 24, rect.width - 16, 18), line2);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!platform || !bottomPoint || !topPoint) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(bottomPoint.position, topPoint.position);
        Gizmos.DrawWireCube(bottomPoint.position, new Vector3(1.5f, 0.1f, 1.5f));
        Gizmos.DrawWireCube(topPoint.position, new Vector3(1.5f, 0.1f, 1.5f));
    }
#endif
}
