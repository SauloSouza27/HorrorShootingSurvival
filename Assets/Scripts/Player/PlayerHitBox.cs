using System.Linq.Expressions;
using UnityEngine;

public class PlayerHitBox : HitBox
{
    private Player player;

    protected override void Awake()
    {
        base.Awake();
        
        player = GetComponentInParent<Player>();
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        
        player.health.ReduceHealth(damage);
    }
    
    
}
