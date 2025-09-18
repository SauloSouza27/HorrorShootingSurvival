using UnityEngine;

public class MeleeAttack : MonoBehaviour, IEnemyAttack
{
    [Header("Melee Attack Settings")]
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _attackCooldown = 2f;
    [SerializeField] private float _attackDuration = 1f;
    [SerializeField] private int _attackDamage = 10;

    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public float AttackDuration => _attackDuration;

    public void ExecuteAttack(GameObject target)
    {
        Debug.Log(gameObject.name + " performs an on " + target.name);

        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            
        }
    }
}