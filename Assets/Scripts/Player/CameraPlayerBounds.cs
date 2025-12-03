using UnityEngine;

public class CameraPlayerBounds : MonoBehaviour
{
    [Header("Viewport bounds (0–1)")]
    [SerializeField] private float minViewportX = 0.1f;
    [SerializeField] private float maxViewportX = 0.9f;
    [SerializeField] private float minViewportY = 0.1f;
    [SerializeField] private float maxViewportY = 0.9f;

    [Header("Ground / height")]
    [Tooltip("If false, clamp on a horizontal plane at the player's current Y.")]
    [SerializeField] private bool useFixedHeightPlane = false;
    [SerializeField] private float fixedPlaneY = 0f;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;
        if (PlayerHealth.AllPlayers.Count == 0) return;

        foreach (var ph in PlayerHealth.AllPlayers)
        {
            if (ph == null) continue;
            if (ph.isDead) continue;      // don't drag corpses around
            // if you also don't want to constrain downed players:
            // if (ph.isDowned) continue;

            ClampTransformToViewport(ph.transform);
        }
    }

    private void ClampTransformToViewport(Transform t)
    {
        Vector3 worldPos = t.position;
        Vector3 vp = cam.WorldToViewportPoint(worldPos);

        // If behind camera, ignore (shouldn't really happen in top-down)
        if (vp.z < 0f) return;

        float originalX = vp.x;
        float originalY = vp.y;

        vp.x = Mathf.Clamp(vp.x, minViewportX, maxViewportX);
        vp.y = Mathf.Clamp(vp.y, minViewportY, maxViewportY);

        // If still inside bounds, nothing to do
        if (Mathf.Approximately(originalX, vp.x) &&
            Mathf.Approximately(originalY, vp.y))
            return;

        // Project back to world on a horizontal plane
        float planeY = useFixedHeightPlane ? fixedPlaneY : worldPos.y;
        Plane plane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

        Ray ray = cam.ViewportPointToRay(vp);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 newPos = ray.GetPoint(distance);
            newPos.y = worldPos.y; // keep exact vertical height

            // If there is a CharacterController, move via Move() to avoid CC glitches
            var cc = t.GetComponent<CharacterController>();
            if (cc != null && cc.enabled)
            {
                Vector3 delta = newPos - worldPos;
                cc.Move(delta);
            }
            else
            {
                t.position = newPos;
            }
        }
    }

    // Optional: visualize the safe area in the Scene view
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (Camera.main == null) return;

        var c = Camera.main;
        Vector3 a = c.ViewportToWorldPoint(new Vector3(minViewportX, minViewportY, c.nearClipPlane + 10f));
        Vector3 b = c.ViewportToWorldPoint(new Vector3(maxViewportX, minViewportY, c.nearClipPlane + 10f));
        Vector3 d = c.ViewportToWorldPoint(new Vector3(maxViewportX, maxViewportY, c.nearClipPlane + 10f));
        Vector3 e = c.ViewportToWorldPoint(new Vector3(minViewportX, maxViewportY, c.nearClipPlane + 10f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, d);
        Gizmos.DrawLine(d, e);
        Gizmos.DrawLine(e, a);
    }
#endif
}
