using UnityEngine;
using UnityEngine.PlayerLoop;

public class Bullet : MonoBehaviour
{
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

    private Color currentTierColor = Color.white;
    // [Header("Bullet Emission Colors")]
    // [SerializeField] private Color baseEmission  = Color.white;
    // [SerializeField] private Color tier1Emission = new Color(0.3f, 0.6f, 1f);
    // [SerializeField] private Color tier2Emission = new Color(1f, 0.4f, 0.8f);
    // [SerializeField] private Color tier3Emission = new Color(1f, 0.9f, 0.3f);

    [Header("Audio")]
    public AudioClip shootSFX;
    [Range(0f, 1f)] public float shootVolume = 1f;

    protected virtual void Awake()
    {
        cd = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public virtual void BulletSetup(int bulletDamage1, float flyDistance1, Player owner, int packTier)
    {
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
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            if (enemy != null)
                enemy.TakeDamage(BulletDamage, Owner);
        }

        trailRenderer.Clear();
        CreateImpactFx(collision);
        ObjectPool.instance.ReturnObject(0, gameObject);
    }

    protected void ReturnBulletToPool() => ObjectPool.instance.ReturnObject(0, gameObject);

    private void CreateImpactFx(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            AudioManager.Instance.PlaySFX("BulletImpact", shootVolume);

            ContactPoint contact = collision.contacts[0];

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

    private Color GetTierColor()
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
