using UnityEngine;

public class StaminaDebugUI : MonoBehaviour
{
    private PlayerMovement movement;
    private Camera cam;

    [SerializeField] private Vector3 offset = new(0f, -1f, 0f);

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        cam = Camera.main;
    }

    private void OnGUI()
    {
        if (movement == null || cam == null) return;
        if (!movement.ShowDebugUI) return;

        if (movement.StaminaNormalized >= 1f && !movement.IsRunning)
            return; // hide when full and idle

        Vector3 worldPos = transform.position + offset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0f) return;

        float barWidth = 100f;
        float barHeight = 12f;
        float normalized = movement.StaminaNormalized;

        float x = screenPos.x - barWidth / 2f;
        float y = Screen.height - screenPos.y;

        // Background (semi-transparent black)
        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(new Rect(x - 2f, y - 2f, barWidth + 4f, barHeight + 4f), Texture2D.whiteTexture);

        // Fill color:
        // ✅ Yellow normally
        // 🔴 Red if empty and in cooldown
        if (normalized > 0f)
            GUI.color = Color.yellow;
        else
            GUI.color = Color.red; // empty stamina warning!

        GUI.DrawTexture(new Rect(x, y, barWidth * normalized, barHeight), Texture2D.whiteTexture);

        // Label
        //GUI.color = Color.white;
        //GUI.Label(new Rect(x, y - 16f, barWidth, 20f), "Stamina");
    }

}