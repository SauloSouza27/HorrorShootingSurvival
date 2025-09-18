using UnityEngine;
public interface IEnemyAttack
{
    float AttackRange { get; }
    float AttackCooldown { get; }
    float AttackDuration { get; }
    void ExecuteAttack(GameObject target);
}