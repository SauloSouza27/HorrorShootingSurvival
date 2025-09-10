using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorOpenMode { DisableBlockers, Slide, Rotate }

public class DoorPurchase : Interactable
{
    public override bool RemoveAfterInteract => true;

    [Header("Purchase")]
    [SerializeField] private int cost = 750;
    public int Cost => cost;

    [Header("Open Behaviour")]
    [SerializeField] private DoorOpenMode openMode = DoorOpenMode.DisableBlockers;
    [SerializeField] private List<GameObject> blockers = new List<GameObject>();
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 slideOffset = new Vector3(0, 0, 2f);
    [SerializeField] private float slideTime = 0.6f;
    [SerializeField] private Vector3 rotateAngles = new Vector3(0f, 90f, 0f);
    [SerializeField] private float rotateTime = 0.5f;

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

    // track who is in range to show the prompt
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

        StartCoroutine(OpenSequence());
    }

    private IEnumerator OpenSequence()
    {
        opened = true;
        HighlightActive(false);
        if (audioSrc && openClip) audioSrc.PlayOneShot(openClip);

        switch (openMode)
        {
            case DoorOpenMode.DisableBlockers:
                DisableBlockers();
                break;
            case DoorOpenMode.Slide:
                if (doorTransform) yield return SlideRoutine(doorTransform, slideOffset, slideTime);
                DisableBlockers();
                break;
            case DoorOpenMode.Rotate:
                if (doorTransform) yield return RotateRoutine(doorTransform, rotateAngles, rotateTime);
                DisableBlockers();
                break;
        }
    }

    private void DisableBlockers()
    {
        foreach (var go in blockers)
        {
            if (!go) continue;
            foreach (var col in go.GetComponentsInChildren<Collider>(true)) col.enabled = false;
            go.SetActive(false);
        }
    }

    private IEnumerator SlideRoutine(Transform t, Vector3 offset, float duration)
    {
        Vector3 a = t.position, b = a + offset;
        float u = 0f;
        while (u < 1f) { u += Time.deltaTime / Mathf.Max(0.01f, duration); t.position = Vector3.Lerp(a, b, Mathf.SmoothStep(0,1,u)); yield return null; }
        t.position = b;
    }

    private IEnumerator RotateRoutine(Transform t, Vector3 angles, float duration)
    {
        Quaternion a = t.rotation, b = a * Quaternion.Euler(angles);
        float u = 0f;
        while (u < 1f) { u += Time.deltaTime / Mathf.Max(0.01f, duration); t.rotation = Quaternion.Slerp(a, b, Mathf.SmoothStep(0,1,u)); yield return null; }
        t.rotation = b;
    }

    // --- Interactable proximity tracking (to show prompt) ---
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

        // choose the nearest player (so text reflects *their* points)
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
}
