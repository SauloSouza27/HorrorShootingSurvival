using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 1f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private Bite bite;
    [SerializeField] private float biteDuration = 0.3f;
    private Animator animator;

    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float AttackDuration => attackDuration;

    private void Awake()
    {
        animator = GetComponentInParent<Animator>();
        if (bite != null)
        {
            bite.gameObject.SetActive(false);
        }
    }
    public void ExecuteAttack(GameObject target)
    {
        StartCoroutine(AttackCoroutine());
    }
    private IEnumerator AttackCoroutine()
    {
        bite.SetAttack(attackDamage);
        animator.SetBool("isAttacking", true);
        bite.gameObject.SetActive(true);

        yield return new WaitForSeconds(biteDuration);

        animator.SetBool("isAttacking", false);
        bite.gameObject.SetActive(false);
    }
}