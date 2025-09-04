using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorOpenMode { DisableBlockers, Slide, Rotate }

public class DoorPurchase : Interactable
{
    public override bool RemoveAfterInteract => true; // one-shot after open

    [Header("Purchase")]
    [SerializeField] private int cost = 750;

    [Header("Open Behaviour")]
    [SerializeField] private DoorOpenMode openMode = DoorOpenMode.DisableBlockers;

    [Tooltip("Objects to disable when purchased (colliders/meshes).")]
    [SerializeField] private List<GameObject> blockers = new List<GameObject>();

    [Tooltip("If Slide/Rotate, the target to move/rotate (e.g., the door mesh root).")]
    [SerializeField] private Transform doorTransform;

    [Header("Slide Settings")]
    [SerializeField] private Vector3 slideOffset = new Vector3(0, 0, 2f);
    [SerializeField] private float slideTime = 0.6f;

    [Header("Rotate Settings")]
    [SerializeField] private Vector3 rotateAngles = new Vector3(0f, 90f, 0f);
    [SerializeField] private float rotateTime = 0.5f;

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
            // not enough points
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

        // Optional open sfx
        if (audioSrc && openClip) audioSrc.PlayOneShot(openClip);

        switch (openMode)
        {
            case DoorOpenMode.DisableBlockers:
                DisableBlockers();
                break;

            case DoorOpenMode.Slide:
                if (doorTransform != null)
                    yield return SlideRoutine(doorTransform, slideOffset, slideTime);
                DisableBlockers(); // usually also disable colliders after the motion
                break;

            case DoorOpenMode.Rotate:
                if (doorTransform != null)
                    yield return RotateRoutine(doorTransform, rotateAngles, rotateTime);
                DisableBlockers();
                break;
        }

        // Optionally destroy or pool this interactable object
        // Destroy(gameObject);
    }

    private void DisableBlockers()
    {
        foreach (var go in blockers)
        {
            if (go == null) continue;
            // Disable colliders
            foreach (var col in go.GetComponentsInChildren<Collider>(true))
                col.enabled = false;
            // Optionally hide renderers or the entire GO
            // foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            //     r.enabled = false;
            go.SetActive(false);
        }
    }

    private IEnumerator SlideRoutine(Transform t, Vector3 offset, float duration)
    {
        Vector3 start = t.position;
        Vector3 end = start + offset;
        float tNorm = 0f;
        while (tNorm < 1f)
        {
            tNorm += Time.deltaTime / Mathf.Max(0.01f, duration);
            t.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, tNorm));
            yield return null;
        }
        t.position = end;
    }

    private IEnumerator RotateRoutine(Transform t, Vector3 angles, float duration)
    {
        Quaternion start = t.rotation;
        Quaternion end = start * Quaternion.Euler(angles);
        float tNorm = 0f;
        while (tNorm < 1f)
        {
            tNorm += Time.deltaTime / Mathf.Max(0.01f, duration);
            t.rotation = Quaternion.Slerp(start, end, Mathf.SmoothStep(0, 1, tNorm));
            yield return null;
        }
        t.rotation = end;
    }
}
