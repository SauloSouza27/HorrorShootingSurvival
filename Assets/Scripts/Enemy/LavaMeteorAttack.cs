using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LavaMeteorAttack : MonoBehaviour, IEnemyAttack
{
    [Header("Attack Settings")]
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

    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float AttackDuration => attackDuration;

    private Animator animator;
    private NavMeshAgent agent;
    private EnemyBase enemy;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
        agent    = GetComponentInParent<NavMeshAgent>();
        enemy    = GetComponentInParent<EnemyBase>();
    }

    public void ExecuteAttack(GameObject target)
    {
        if (target == null) return;
        StartCoroutine(AttackCoroutine(target));
    }

    private IEnumerator AttackCoroutine(GameObject target)
    {
        animator.SetBool("isCooldown", false);
        animator.SetBool("isAttacking", true);

        Vector3 targetPosition = target.transform.position;

        if (groundIndicatorVFX != null)
        {
            Destroy(Instantiate(groundIndicatorVFX, targetPosition, Quaternion.identity), damageDelay);
        }

        // Wait for animation timing
        yield return new WaitForSeconds(attackDuration - 0.2f);

        animator.SetBool("isCooldown", true);
        animator.SetBool("isAttacking", false);
        StartCoroutine(CooldownTimerAnimation());

        if (impactVFX != null)
        {
            Destroy(Instantiate(impactVFX, targetPosition, Quaternion.identity), 4f);
        }

        //  Single-hit-per-character logic
        Collider[] hits = Physics.OverlapSphere(targetPosition, damageRadius, damageLayerMask);
        var damagedRoots = new HashSet<Transform>();

        foreach (var hit in hits)
        {
            if (hit == null) continue;

            Transform root = hit.transform.root;
            if (damagedRoots.Contains(root))
                continue;

            IDamageable damageable = root.GetComponent<IDamageable>();
            if (damageable == null)
                damageable = hit.GetComponentInParent<IDamageable>();

            if (damageable == null)
                continue;

            damageable.TakeDamage(attackDamage);
            damagedRoots.Add(root);
        }

        yield return null;
    }

    private IEnumerator CooldownTimerAnimation()
    {
        if (enemy.isDead) 
            yield return null;
        
        if (agent != null)
            agent.isStopped = true;

        yield return new WaitForSeconds(attackCooldown);

        animator.SetBool("isCooldown", false);

        if (agent != null)
            agent.isStopped = false;

        if (enemy != null)
            enemy.isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
