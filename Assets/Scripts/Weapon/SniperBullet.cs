using UnityEngine;

public class SniperBullet : Bullet
{
    [Header("Base Sniper Settings")]
    [SerializeField] private int basePenetrations = 3;
    [SerializeField] private float baseFalloff = 0.85f;
    [SerializeField] private LayerMask impactLayers; // Optional: walls, props, etc.

    private int penetrationsRemaining;
    private bool initialized;

    private int maxPenetrations;
    private float damageFalloff;
    private Vector3 lastPosition;

    private void OnEnable()
    {
        initialized = true;
        if (cd) cd.isTrigger = true;
        lastPosition = transform.position;
    }

    public override void BulletSetup(int bulletDamage1, float flyDistance1, Player owner, int packtier)
    {
        base.BulletSetup(bulletDamage1, flyDistance1, owner, packtier);

        // read shooter's weapon tier (Pack-a-Punch level)
        Weapon weapon = owner.weapon.CurrentWeapon();
        int tier = weapon != null ? weapon.PackAPunchTier : 0;

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
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (!initialized) return;

        // 🔹 Raycast between previous and current position for surface impact
        Vector3 direction = transform.position - lastPosition;
        float distance = direction.magnitude;
        if (distance > 0.001f)
        {
            if (Physics.Raycast(lastPosition, direction.normalized, out RaycastHit hit, distance, impactLayers))
            {
                // Spawn wall/floor impact FX (do not affect enemies)
                SpawnImpactFx(hit.point, hit.normal);
            }
        }

        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized) return;

        if (other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy)
            {
                enemy.TakeDamage(BulletDamage, Owner);
                penetrationsRemaining--;

                // Slight damage reduction per target
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

    private void SpawnImpactFx(Vector3 point, Vector3 normal)
    {
        if (!bulletImpactFX) return;

        GameObject newImpactFx = ObjectPool.instance.GetObject(bulletImpactFX);
        newImpactFx.transform.position = point;
        newImpactFx.transform.rotation = Quaternion.LookRotation(normal);
        var impact = newImpactFx.GetComponent<ImpactFX>();
        if (impact != null)
        {
            impact.ApplyColor(GetTierColor());
        }

        ObjectPool.instance.ReturnObject(1, newImpactFx);
    }

    private void EndBullet()
    {
        if (trailRenderer) trailRenderer.Clear();
        ReturnBulletToPool();
    }
}
