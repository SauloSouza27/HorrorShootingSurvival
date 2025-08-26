using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class ReviveTarget : Interactable
{
    private PlayerHealth downedHealth;
    private Coroutine reviveRoutine;

    [Header("Revive Settings")]
    [SerializeField] private float baseReviveTime = 3f; // seconds (before perks)
    [SerializeField] private float reviveRadius = 1.6f; // cancels if rescuer leaves

    // Optional UI hook
    public System.Action<float> OnReviveProgress; // 0..1
    public System.Action<float> OnBleedoutRatio;  // 0..1

    public override bool RequiresPlayer => true;

    public void Init(PlayerHealth health)
    {
        downedHealth = health;

        // Find a non-CharacterController collider we can use as trigger
        Collider triggerCol = null;
        var cols = GetComponents<Collider>();
        foreach (var c in cols)
        {
            if (!(c is CharacterController))
            {
                triggerCol = c;
                break;
            }
        }

        // If none found, add a dedicated trigger collider
        if (triggerCol == null)
        {
            var sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = reviveRadius;       // your serialized radius
            triggerCol = sphere;
        }

        // Make sure this trigger collider is set to trigger
        triggerCol.isTrigger = true;
    }

    public void BeginWaitingForRevive()
    {
        // show worldspace prompt if you have one
        // e.g., enable a canvas above the player
    }

    public void UpdateBleedoutUI(float ratio)
    {
        OnBleedoutRatio?.Invoke(ratio);
        // or drive a UI element here
    }

    public override void Interaction(Player rescuer)
    {
        if (!enabled || downedHealth == null || downedHealth.isDead || !downedHealth.isDowned)
            return;

        if (reviveRoutine != null) StopCoroutine(reviveRoutine);
        reviveRoutine = StartCoroutine(ReviveProcess(rescuer));
    }

    private IEnumerator ReviveProcess(Player rescuer)
    {
        // compute required hold time using rescuer's perk
        var stats = rescuer.GetComponent<PlayerStats>();
        float timeMult = stats != null ? Mathf.Max(0.05f, stats.ReviveSpeedMultiplier) : 1f;
        float requiredTime = baseReviveTime * timeMult;

        // we’ll check rescuer input each frame to simulate “hold to revive”
        var rescuerInput = rescuer.GetComponent<PlayerInput>();
        if (rescuerInput == null) yield break;

        var actions = rescuerInput.actions;
        var interactAction = actions["Interaction"]; // must exist in your input map

        float t = 0f;

        while (t < requiredTime)
        {
            // cancel if target no longer downed or died
            if (!enabled || downedHealth == null || downedHealth.isDead || !downedHealth.isDowned)
                yield break;

            // cancel if rescuer moved too far
            float dist = Vector3.Distance(rescuer.transform.position, transform.position);
            if (dist > reviveRadius) yield break;

            // must be holding Interaction
            // (performed remains true while held with Button interaction type)
            if (interactAction.IsPressed())
            {
                t += Time.deltaTime;
                OnReviveProgress?.Invoke(t / requiredTime); // 0..1 UI
            }
            else
            {
                // Not held → cancel (you could also decay instead of cancel)
                yield break;
            }

            yield return null;
        }

        // success
        downedHealth.CompleteRevive();
        OnReviveProgress?.Invoke(1f);
    }
}
