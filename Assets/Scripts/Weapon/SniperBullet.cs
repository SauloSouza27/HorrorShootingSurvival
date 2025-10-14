using UnityEngine;

public class SniperBullet : Bullet
{
    [Header("Base Sniper Settings")]
    [SerializeField] private int basePenetrations = 3;
    [SerializeField] private float baseFalloff = 0.85f;

    [Header("FX Scaling")]
    [SerializeField] private Gradient trailColorGradient;  // optional colored gradient
    [SerializeField] private Light tracerLight;            // optional small light for glow
    [SerializeField] private float baseTrailWidth = 0.05f;
    [SerializeField] private float baseLightIntensity = 1.5f;
    [SerializeField] private Color baseImpactColor = Color.white;

    private int penetrationsRemaining;
    private bool initialized;

    private int maxPenetrations;
    private float damageFalloff;

    private void OnEnable()
    {
        initialized = true;
        if (cd) cd.isTrigger = true;
    }

    public override void BulletSetup(int bulletDamage1, float flyDistance1, Player owner)
    {
        base.BulletSetup(bulletDamage1, flyDistance1, owner);

        Weapon weapon = owner.weapon.CurrentWeapon();
        int tier = weapon != null ? weapon.PackAPunchTier : 0;

        // ----------------------------
        // 🔹 Scale mechanical behavior
        // ----------------------------
        switch (tier)
        {
            case 0:
                maxPenetrations = basePenetrations;
                damageFalloff = baseFalloff;
                break;
            case 1:
                maxPenetrations = basePenetrations + 1;
                damageFalloff = baseFalloff * 0.9f;
                break;
            case 2:
                maxPenetrations = basePenetrations + 2;
                damageFalloff = baseFalloff * 0.8f;
                break;
            case 3:
                maxPenetrations = basePenetrations + 3;
                damageFalloff = baseFalloff * 0.7f;
                break;
        }

        penetrationsRemaining = maxPenetrations;

        // ----------------------------
        // 🔹 Scale visual intensity
        // ----------------------------
        ApplyVisualUpgrades(tier);
    }

    private void ApplyVisualUpgrades(int tier)
    {
        if (!trailRenderer) return;

        // Increase trail width & color intensity
        float width = baseTrailWidth * (1f + 0.4f * tier);
        trailRenderer.startWidth = width;
        trailRenderer.endWidth = width * 0.5f;

        // Optional gradient color (cool tier glow)
        if (trailColorGradient != null)
        {
            trailRenderer.colorGradient = trailColorGradient;
        }
        else
        {
            Color upgradedColor = Color.Lerp(baseImpactColor, Color.cyan, tier * 0.3f);
            trailRenderer.material.SetColor("_EmissionColor", upgradedColor * (1f + 0.5f * tier));
        }

        // Optional tracer glow
        if (tracerLight)
        {
            tracerLight.intensity = baseLightIntensity * (1f + 0.5f * tier);
            tracerLight.color = Color.Lerp(baseImpactColor, Color.cyan, 0.3f * tier);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized) return;

        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyBase>();
            if (enemy)
            {
                enemy.TakeDamage(BulletDamage, Owner);
                penetrationsRemaining--;
                BulletDamage = Mathf.Max(1, Mathf.RoundToInt(BulletDamage * damageFalloff));
            }

            if (penetrationsRemaining <= 0)
            {
                EndBullet();
                return;
            }
        }
        else if (!other.isTrigger)
        {
            EndBullet();
        }
    }

    private void EndBullet()
    {
        if (trailRenderer) trailRenderer.Clear();
        ReturnToPool();
    }
}
