using UnityEngine;
using UnityEngine.PlayerLoop;

public class Bullet : MonoBehaviour
{
    public float impactForce;
    
    [SerializeField] protected GameObject bulletImpactFX;

    protected BoxCollider cd;
    protected Rigidbody rb;
    protected MeshRenderer meshRenderer;
    protected TrailRenderer trailRenderer;
    
    private int packTier; // already set in BulletSetup
    public Player Owner { get; private set; }
    public int BulletDamage { get; protected set; }

    private Vector3 startPosition;
    private float flyDistance;
    private bool bulletDisabled;

    [Header("Trail Materials (per tier)")]
    [SerializeField] private Material baseTrailMat;
    [SerializeField] private Material tier1TrailMat;
    [SerializeField] private Material tier2TrailMat;
    [SerializeField] private Material tier3TrailMat;
    private Light _light = null;

    [Header("Lights color (per tier)")]
    [SerializeField] private Color baseLightColor;
    [SerializeField] private Color tier1LightColor;
    [SerializeField] private Color tier2LightColor;
    [SerializeField] private Color tier3LightColor;

    private Color currentTierColor = Color.white;

    [Header("Audio")]
    [SerializeField] private AudioClip impactSFX;
    [Range(0f, 1f)] public float impactVolume = 1f;

    protected virtual void Awake()
    {
        cd = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
        _light = GetComponentInChildren<Light>();
    }

    public virtual void BulletSetup(int bulletDamage1, float flyDistance1, Player owner, int packTier, float impactForce)
    {
        this.impactForce = impactForce;
        
        bulletDisabled = false;
        cd.enabled = true;
        meshRenderer.enabled = true;

        BulletDamage = bulletDamage1;
        Owner        = owner;
        this.packTier = packTier;

        trailRenderer.time = .04f;
        startPosition = transform.position;
        flyDistance = flyDistance1 + .5f;

        ApplyTierVisuals(owner);
    }

    private void ApplyTierVisuals(Player owner)
    {
        if (owner == null || owner.weapon == null) return;

        Weapon weapon = owner.weapon.CurrentWeapon();
        packTier = weapon != null ? weapon.PackAPunchTier : 0;

        // Decide which material to use for this tier
        Material trailMat = null;
        switch (packTier)
        {
            case 1: trailMat = tier1TrailMat; break;
            case 2: trailMat = tier2TrailMat; break;
            case 3: trailMat = tier3TrailMat; break;
            default: trailMat = baseTrailMat; break;
        }

        // Apply to trail
        if (trailRenderer && trailMat != null)
            trailRenderer.material = trailMat;

        // Read color from that material
        currentTierColor = GetColorFromMaterial(trailMat);

        // Apply same color to bullet mesh emission
        if (meshRenderer && meshRenderer.material.HasProperty("_EmissionColor"))
            meshRenderer.material.SetColor("_EmissionColor", currentTierColor);

        // Decide which color to use for the light in this tier
        Color lightColor = Color.black;
        switch (packTier)
        {
            case 1: lightColor = tier1LightColor; break;
            case 2: lightColor = tier2LightColor; break;
            case 3: lightColor = tier3LightColor; break;
            default: lightColor = baseLightColor; break;
        }

        // Apply to light color
        if (_light && lightColor != null)
            _light.color = lightColor;

    }



    private void FixedUpdate()
    {
        FadeTrail();
        DisableBullet();
        ReturnToPoolIfNeeded();
    }

    private void ReturnToPoolIfNeeded()
    {
        if (trailRenderer.time < 0)
        {
            trailRenderer.Clear();
            ReturnBulletToPool();
        }
    }

    private void DisableBullet()
    {
        if (Vector3.Distance(startPosition, transform.position) > flyDistance && !bulletDisabled)
        {
            cd.enabled = false;
            meshRenderer.enabled = false;
            bulletDisabled = true;
        }
    }

    private void FadeTrail()
    {
        if (Vector3.Distance(startPosition, transform.position) > flyDistance - 1.5f)
            trailRenderer.time -= 2 * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if (collision.gameObject.CompareTag("Enemy"))
        // {
        //     EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
        //     if (enemy != null)
        //     {
        //         enemy.TakeDamage(BulletDamage, Owner);
        //         
        //         Debug.Log("collision check:" + collision);
        //         ApplyBulletImpactToEnemy(collision);
        //     }
        // }
        trailRenderer.Clear();
        CreateImpactFx(collision);
        ObjectPool.instance.ReturnObject(0, gameObject);
        
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        damageable?.TakeDamage(BulletDamage, Owner);
        
        ApplyBulletImpactToEnemy(collision);
    }
    
    private void ApplyBulletImpactToEnemy(Collision collision)
    {
        EnemyBase enemy = collision.gameObject.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            Vector3 force = rb.linearVelocity.normalized * impactForce;
            Rigidbody hitRigidbody = collision.collider.attachedRigidbody;
            enemy.BulletImpact(force, collision.contacts[0].point, hitRigidbody);
        }
    }

    protected void ReturnBulletToPool() => ObjectPool.instance.ReturnObject(0, gameObject);

    private void CreateImpactFx(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];

            // 🔊 3D impact sound at contact point
            if (AudioManager.Instance != null && impactSFX != null)
            {
                AudioManager.Instance.PlaySFX3D(
                    impactSFX,
                    contact.point,
                    impactVolume,
                    spatialBlend: 1f,
                    minDistance: 3f,
                    maxDistance: 30f
                );
            }
            
            GameObject newImpactFx = ObjectPool.instance.GetObject(bulletImpactFX);
            newImpactFx.transform.position = contact.point;

            // 🔥 Apply tier color to all particle systems in the impact fx
            var impact = newImpactFx.GetComponent<ImpactFX>();
            if (impact != null)
            {
                impact.ApplyColor(GetTierColor());
            }

            ObjectPool.instance.ReturnObject(1, newImpactFx);
        }
    }

    protected Color GetTierColor()
    {
        return currentTierColor;
    }
    
    private Color GetColorFromMaterial(Material mat)
    {
        if (mat == null) return Color.white;

        if (mat.HasProperty("_EmissionColor"))
            return mat.GetColor("_EmissionColor");
        if (mat.HasProperty("_BaseColor"))
            return mat.GetColor("_BaseColor");
        if (mat.HasProperty("_Color"))
            return mat.GetColor("_Color");

        return Color.white;
    }

}
