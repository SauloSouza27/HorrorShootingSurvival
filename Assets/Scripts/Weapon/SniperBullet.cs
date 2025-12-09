using UnityEngine;
using System.Collections.Generic;

public class SniperBullet : Bullet
{
    [Header("Base Sniper Settings")]
    [SerializeField] private int basePenetrations = 3;
    [SerializeField] private float baseFalloff = 0.85f;

    [SerializeField] private LayerMask impactLayers; // optional / unused now

    private int   penetrationsRemaining;
    private int   maxPenetrations;
    private float damageFalloff;

    private bool initialized;
    private bool finished;

    // Avoid damaging the same enemy twice due to multiple colliders
    private readonly HashSet<EnemyBase> hitEnemies = new HashSet<EnemyBase>();

    private void OnEnable()
    {
        if (cd != null)
            cd.isTrigger = true;

        finished    = false;
        initialized = false;
        hitEnemies.Clear();
    }

    public override void BulletSetup(
        int bulletDamage1,
        float flyDistance1,
        Player owner,
        int packtier,
        float impactForce1)
    {
        base.BulletSetup(bulletDamage1, flyDistance1, owner, packtier, impactForce1);

        Weapon weapon = owner != null ? owner.weapon.CurrentWeapon() : null;
        int tier      = weapon != null ? weapon.PackAPunchTier : 0;

        switch (tier)
        {
            case 0:
                maxPenetrations = basePenetrations;
                damageFalloff   = baseFalloff;
                break;
            case 1:
                maxPenetrations = basePenetrations + 1;
                damageFalloff   = baseFalloff * 0.9f;
                break;
            case 2:
                maxPenetrations = basePenetrations + 2;
                damageFalloff   = baseFalloff * 0.8f;
                break;
            case 3:
                maxPenetrations = basePenetrations + 3;
                damageFalloff   = baseFalloff * 0.7f;
                break;
            default:
                maxPenetrations = basePenetrations;
                damageFalloff   = baseFalloff;
                break;
        }

        penetrationsRemaining = maxPenetrations;
        initialized           = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized || finished) return;

        // Compute impact point/normal once
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 normal   = -rb.linearVelocity.normalized;

        EnemyBase   enemy      = other.GetComponentInParent<EnemyBase>();
        IDamageable damageable = other.GetComponent<IDamageable>();

        bool hitEnemy = (enemy != null && damageable != null);
        
        //  ENEMY HIT
        if (hitEnemy)
        {
           
            if (!hitEnemies.Add(enemy))
                return;

            
            damageable.TakeDamage(BulletDamage, Owner);

            
            ApplyBulletImpactToEnemy(other, hitPoint);

            
            CreateImpactFx(hitPoint, normal);

            
            penetrationsRemaining--;
            BulletDamage = Mathf.Max(1, Mathf.RoundToInt(BulletDamage * damageFalloff));

            // If no penetrations left, end the bullet here
            if (penetrationsRemaining <= 0)
            {
                EndBullet();
            }

            return;
        }
        
        //  NON-ENEMY
       
        if (!other.isTrigger)
        {
            CreateImpactFx(hitPoint, normal);
            EndBullet();
        }
    }

    private void EndBullet()
    {
        if (finished) return;
        finished = true;

        if (trailRenderer != null)
            trailRenderer.Clear();

        ReturnBulletToPool();
    }
}
