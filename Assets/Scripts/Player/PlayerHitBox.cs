using UnityEngine;

public class PlayerHitBox : HitBox
{
    private Player player;

    protected override void Awake()
    {
        base.Awake();
        
        player = GetComponentInParent<Player>();
    }

    public override void TakeDamage()
    {
        base.TakeDamage();
        
        player.health.ReduceHealth();
    }
}
