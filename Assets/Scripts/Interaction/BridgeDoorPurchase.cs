using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeDoorPurchase : Interactable
{
    public override bool RemoveAfterInteract => true;

    [Header("Purchase")]
    [SerializeField] private int cost = 1500;
    public int Cost => cost;

    [Header("Bridge Platform (lift & stay)")]
    [SerializeField] private Transform bridgePlatform;
    [SerializeField] private float liftHeight = 3f;
    [SerializeField] private float liftTime = 1f;

    [Header("Walkability")]
    [SerializeField] private Collider bridgeWalkCollider;

    [Header("Ground Floor Blockers")]
    [SerializeField] private List<GameObject> groundBlockers = new List<GameObject>();

    [Header("SFX (optional)")]
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip buyClip;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip deniedClip;

    [Header("DEBUG Prompt")]
    [SerializeField] private bool debugPriceUI = true;
    [SerializeField] private Vector3 uiWorldOffset = new Vector3(0, 2f, 0);

    private bool opened;
    public bool IsOpened => opened;

    private readonly HashSet<Player> playersInRange = new HashSet<Player>();

    public override void Interaction(Player player)
    {
        if (opened || player == null) return;

        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (!stats.SpendPoints(cost))
        {
            if (audioSrc && deniedClip) audioSrc.PlayOneShot(deniedClip);
            return;
        }

        if (audioSrc && buyClip) audioSrc.PlayOneShot(buyClip);

        StartCoroutine(LiftBridgeRoutine());
    }

    private IEnumerator LiftBridgeRoutine()
    {
        opened = true;
        HighlightActive(false);
        if (audioSrc && openClip) audioSrc.PlayOneShot(openClip);

        if (bridgePlatform)
        {
            Vector3 a = bridgePlatform.position, b = a + Vector3.up * liftHeight;
            float u = 0f;
            while (u < 1f)
            {
                u += Time.deltaTime / Mathf.Max(0.01f, liftTime);
                bridgePlatform.position = Vector3.Lerp(a, b, Mathf.SmoothStep(0,1,u));
                yield return null;
            }
            bridgePlatform.position = b;
        }

        foreach (var go in groundBlockers)
        {
            if (!go) continue;
            foreach (var c in go.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            go.SetActive(false);
        }

        if (bridgeWalkCollider) bridgeWalkCollider.enabled = true;
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

    private void OnGUI()
    {
        if (!debugPriceUI || IsOpened) return;
        if (playersInRange.Count == 0) return;
        var cam = Camera.main; if (!cam) return;

        Player nearest = null; float minD = float.MaxValue;
        foreach (var p in playersInRange)
        {
            if (!p) continue;
            float d = Vector3.Distance(p.transform.position, transform.position);
            if (d < minD) { minD = d; nearest = p; }
        }
        if (!nearest) return;

        var stats = nearest.GetComponent<PlayerStats>();
        int points = stats ? stats.GetPoints() : 0;
        bool canAfford = stats && stats.CanAfford(cost);

        Vector3 screen = cam.WorldToScreenPoint(transform.position + uiWorldOffset);
        if (screen.z < 0) return;
        screen.y = Screen.height - screen.y;

        var rect = new Rect(screen.x - 120, screen.y - 40, 240, 38);
        GUI.color = new Color(0,0,0,0.7f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;

        string line1 = canAfford ? $"Press Interact to buy  ({cost})" : $"Not enough points  ({points}/{cost})";
        GUI.Label(new Rect(rect.x + 8, rect.y + 8, rect.width - 16, 22), line1);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!bridgePlatform) return;
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
        Vector3 a = bridgePlatform.position, b = a + Vector3.up * liftHeight;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireCube(b, new Vector3(1.5f, 0.15f, 3f));
    }
#endif
}
