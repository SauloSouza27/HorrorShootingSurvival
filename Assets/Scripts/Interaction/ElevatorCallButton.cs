using UnityEngine;

/// <summary>
/// Wall/button near a floor that calls the elevator to THIS floor.
/// Charges the interacting player and respects moving/locked state.
/// </summary>
public class ElevatorCallButton : Interactable
{
    public override bool RemoveAfterInteract => false; // reusable

    [Header("References")]
    [SerializeField] private ElevatorPlatform elevator; // assign in inspector

    [Header("Floor")]
    [Tooltip("If true, this button calls the elevator to the TOP floor; otherwise BOTTOM.")]
    [SerializeField] private bool thisIsTopFloorButton = false;

    [Header("Debug UI")]
    [SerializeField] private bool debugUI = true;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2f, 0);

    public override void Interaction(Player player)
    {
        if (elevator == null || player == null) return;

        // If already at this floor or not ready, RequestMoveToFloor will no-op/return false.
        bool ok = elevator.RequestMoveToFloor(player, moveToTop: thisIsTopFloorButton);

        // Optional: play feedback if ok/denied.
        // if (!ok) ...
    }

    // ------------ DEBUG UI ------------
    private void OnGUI()
    {
        if (!debugUI || elevator == null) return;
        var cam = Camera.main; if (!cam) return;

        Vector3 screen = cam.WorldToScreenPoint(transform.position + uiOffset);
        if (screen.z < 0) return;
        screen.y = Screen.height - screen.y;

        var rect = new Rect(screen.x - 140, screen.y - 40, 280, 36);
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;

        string floorName = thisIsTopFloorButton ? "TOP" : "BOTTOM";
        string state = elevator.IsLocked ? "LOCKED" :
                       elevator.IsMoving ? "MOVING" :
                       (thisIsTopFloorButton == elevator.IsAtTop ? "HERE" : "CALLABLE");

        string line = $"Call Elevator ({floorName}) — {state}";
        GUI.Label(new Rect(rect.x + 8, rect.y + 8, rect.width - 16, 20), line);
    }
}
