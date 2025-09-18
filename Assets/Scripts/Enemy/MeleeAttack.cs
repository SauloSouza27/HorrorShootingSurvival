using UnityEngine;

public class MeleeAttack : MonoBehaviour, IEnemyAttack
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 1f;
    [SerializeField] private int attackDamage = 10;

    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float AttackDuration => attackDuration;

    public void ExecuteAttack(GameObject target)
    {
        Debug.Log(gameObject.name + " performs an on " + target.name);

        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            
        }
    }
}