using UnityEngine;
using UnityEngine.UI;

public class ReviveDownedWorldUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image reviveIcon;
    [SerializeField] private Camera uiCamera;

    [Header("Color Over Bleedout Time")]
    [SerializeField] private Color startColor = Color.yellow;
    [SerializeField] private Color endColor = Color.red;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.0f, 0f);

    private PlayerHealth downedHealth;

    private void Awake()
    {
        if (downedHealth == null)
            downedHealth = GetComponentInParent<PlayerHealth>();

        if (uiCamera == null)
            uiCamera = Camera.main;

        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);
    }

    private void OnEnable()
    {
        if (canvas == null)
        {
            Debug.LogError("ReviveDownedWorldUI: Canvas not assigned.", this);
            return;
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = uiCamera != null ? uiCamera : Camera.main;
        canvas.gameObject.SetActive(false); // will be toggled only when downed
    }

    private void LateUpdate()
    {
        if (canvas == null || downedHealth == null)
            return;

        // Only show when player is downed
        if (!downedHealth.isDowned)
        {
            if (canvas.gameObject.activeSelf)
                canvas.gameObject.SetActive(false);
            return;
        }

        if (!canvas.gameObject.activeSelf)
            canvas.gameObject.SetActive(true);

        // Position above the player
        Vector3 targetPos = downedHealth.transform.position + worldOffset;
        canvas.transform.position = targetPos;

        // Face the camera
        if (uiCamera == null)
            uiCamera = Camera.main;

        if (uiCamera != null)
        {
            var toCam = canvas.transform.position - uiCamera.transform.position;
            canvas.transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
        }

        // Color: 0 → just downed (yellow), 1 → about to die (red)
        if (reviveIcon != null)
        {
            float t = 1f - Mathf.Clamp01(downedHealth.BleedoutRemaining01);
            //Debug.Log("reviveIcon: " + downedHealth.BleedoutRemaining01);
            reviveIcon.color = Color.Lerp(startColor, endColor, t);
        }
    }
}
