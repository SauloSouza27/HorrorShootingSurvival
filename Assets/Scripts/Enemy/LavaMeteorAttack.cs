using System.Collections;
using UnityEngine;

public class LavaMeteorAttack : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private float attackCooldown = 5f;
    [SerializeField] private float attackDuration = 1.5f;

    [Header("Meteor Effect")]
    [SerializeField] private int attackDamage = 25;
    [SerializeField] private float damageRadius = 3f;
    [SerializeField] private float damageDelay = 1f;
    [SerializeField] private LayerMask damageLayerMask;

    [Header("VFX")]
    [SerializeField] private GameObject groundIndicatorVFX;
    [SerializeField] private GameObject impactVFX;

    private Vector3 lastTargetPosition;
    private float gizmoDrawTimer = 0f;


    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float AttackDuration => attackDuration;

    private void Update()
    {
        if (gizmoDrawTimer > 0)
        {
            gizmoDrawTimer -= Time.deltaTime;
        }
    }

    public void ExecuteAttack(GameObject target)
    {
        StartCoroutine(AttackCoroutine(target));
    }

    private IEnumerator AttackCoroutine(GameObject target)
    {
        Vector3 targetPosition = target.transform.position;

        // --- GIZMO LOGIC ADDED HERE ---
        // Save the position and start the timer to draw the runtime gizmo.
        lastTargetPosition = targetPosition;
        gizmoDrawTimer = damageDelay; // Draw the gizmo for the duration of the telegraph.

        if (groundIndicatorVFX != null)
        {
            Destroy(Instantiate(groundIndicatorVFX, targetPosition, Quaternion.identity), damageDelay + 0.5f);
        }

        yield return new WaitForSeconds(damageDelay);

        if (impactVFX != null)
        {
            Destroy(Instantiate(impactVFX, targetPosition, Quaternion.identity), 4f);
        }

        Collider[] hits = Physics.OverlapSphere(targetPosition, damageRadius, damageLayerMask);
        foreach (var hit in hits)
        {
            IDamageable damageable = hit.gameObject.GetComponent<IDamageable>();
            damageable?.TakeDamage();
            {
                Debug.Log($"Hit {hit.name} for {attackDamage} damage!");
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        // Set the color for the attack range gizmo
        Gizmos.color = Color.red;
        // Draw a wire sphere with the same radius as our attack range
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Set the color for the damage radius gizmo
        Gizmos.color = Color.yellow;
        // Draw a wire sphere for the damage radius
        // NOTE: This shows the damage radius centered on the enemy for reference.
        // The actual attack will be centered on the player's position.
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
    private void OnDrawGizmos()
    {
        // Only draw if the timer is active.
        if (gizmoDrawTimer > 0)
        {
            Gizmos.color = Color.magenta; // Use a distinct color like magenta.
            Gizmos.DrawWireSphere(lastTargetPosition, damageRadius);
        }
    }
}