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
        Debug.Log("PlayerHitBox TakeDamage" + damage);
        base.TakeDamage(damage);
        
        player.health.ReduceHealth(damage);
    }
    
    
}
