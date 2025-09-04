using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeDoorPurchase : Interactable
{
    public override bool RemoveAfterInteract => true; // one-shot

    [Header("Purchase")]
    [SerializeField] private int cost = 1500;

    [Header("Bridge Settings")]
    [Tooltip("The platform/door object that will lift up to become a bridge.")]
    [SerializeField] private Transform bridgePlatform;

    [Tooltip("How high to lift the platform in world space (Y-axis).")]
    [SerializeField] private float liftHeight = 3f;

    [SerializeField] private float liftTime = 1f;

    [Header("Ground Floor Blockers To Clear")]
    [SerializeField] private List<GameObject> groundBlockers = new List<GameObject>();

    [Header("Walkability")]
    [SerializeField] private Collider bridgeCollider;        // collider for walking across
    [SerializeField] private bool enableBridgeColliderOnOpen = true;

    [Header("SFX (optional)")]
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip buyClip;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip deniedClip;

    private bool opened;

    public override void Interaction(Player player)
    {
        if (opened) return;

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

        // 1) Lift the bridge straight up
        if (bridgePlatform != null)
        {
            Vector3 start = bridgePlatform.position;
            Vector3 end = start + Vector3.up * liftHeight;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, liftTime);
                bridgePlatform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }

            bridgePlatform.position = end;
        }

        // 2) Clear ground-level blockers
        foreach (var go in groundBlockers)
        {
            if (go == null) continue;
            foreach (var c in go.GetComponentsInChildren<Collider>(true))
                c.enabled = false;
            go.SetActive(false);
        }

        // 3) Enable the bridge collider for walking
        if (bridgeCollider != null)
            bridgeCollider.enabled = enableBridgeColliderOnOpen;
    }
}
