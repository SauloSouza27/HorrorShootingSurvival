using UnityEngine;

public class SniperBullet : Bullet
{
    [Header("Sniper Settings")]
    [SerializeField] private int maxPenetrations = 3;      // enemies we can pierce
    [SerializeField] private float damageFalloff = 0.85f;  // per-hit multiplier

    private int penetrationsRemaining;
    private bool initialized;

    // NOTE: no override here; Unity messages aren't virtual
    private void OnEnable()
    {
        penetrationsRemaining = maxPenetrations;
        initialized = true;

        // Ensure collider is trigger for piercing
        if (cd) cd.isTrigger = true;
    }

    // Use trigger to pass through enemies
    private void OnTriggerEnter(Collider other)
    {
        if (!initialized) return;

        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(BulletDamage, Owner);
                penetrationsRemaining--;

                // reduce damage for next target
                BulletDamage = Mathf.Max(1, Mathf.RoundToInt(BulletDamage * damageFalloff));

                if (penetrationsRemaining <= 0)
                {
                    Finish();
                    return;
                }
            }
        }
        else if (!other.isTrigger)
        {
            // Hit a solid surface; end
            Finish();
        }
    }

    private void Finish()
    {
        if (trailRenderer) trailRenderer.Clear();
        ReturnToPool();
    }
}