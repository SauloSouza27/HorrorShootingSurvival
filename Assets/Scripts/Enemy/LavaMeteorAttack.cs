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

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
    }

    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float AttackDuration => attackDuration;

    public void ExecuteAttack(GameObject target)
    {
        StartCoroutine(AttackCoroutine(target));
    }

    private IEnumerator AttackCoroutine(GameObject target)
    {
        animator.SetBool("isAttacking", true);
        Vector3 targetPosition = target.transform.position;

        if (groundIndicatorVFX != null)
        {
            Destroy(Instantiate(groundIndicatorVFX, targetPosition, Quaternion.identity), damageDelay + 0.5f);
        }

        yield return new WaitForSeconds(damageDelay);
        animator.SetBool("isAttacking", false);

        if (impactVFX != null)
        {
            Destroy(Instantiate(impactVFX, targetPosition, Quaternion.identity), 4f);
        }

        Collider[] hits = Physics.OverlapSphere(targetPosition, damageRadius, damageLayerMask);

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.gameObject.GetComponent<IDamageable>();
            damageable?.TakeDamage(attackDamage);
            {
                //Debug.Log($"Hit {hit.name} for {attackDamage} damage!");
            }
        }
    }
}