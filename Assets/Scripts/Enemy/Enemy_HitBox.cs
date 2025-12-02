using UnityEngine;

public class Enemy_HitBox : HitBox
{
    private EnemyBase enemy;

    protected override void Awake()
    {
        base.Awake();

        enemy = GetComponentInParent<EnemyBase>();
    }

    public override void TakeDamage(int damage, Player player)
    {
        //int newDamage = Mathf.RoundToInt(damage * damageMultiplier);

        enemy.TakeDamage(damage, player);
    }
}
