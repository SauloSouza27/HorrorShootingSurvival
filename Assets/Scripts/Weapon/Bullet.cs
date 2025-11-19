using System;
using Unity.VisualScripting; 
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] protected GameObject bulletImpactFX;

    protected BoxCollider cd;
    protected Rigidbody rb;
    protected MeshRenderer meshRenderer;
    protected TrailRenderer trailRenderer;

    public Player Owner { get; private set; }

    private Vector3 startPosition;
    private float flyDistance;
    private bool bulletDisabled;
    public int BulletDamage { get; protected set; }

    [Header("Visual FX")]
    [SerializeField] private Gradient baseTrailColor;
    [SerializeField] private Gradient tier1TrailColor;
    [SerializeField] private Gradient tier2TrailColor;
    [SerializeField] private Gradient tier3TrailColor;

    [SerializeField] private Color baseEmission = Color.white;
    [SerializeField] private Color tier1Emission = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color tier2Emission = new Color(1f, 0.4f, 0.8f);
    [SerializeField] private Color tier3Emission = new Color(1f, 0.9f, 0.3f);
    
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

    // ✅ Extended to apply weapon tier visuals
    public virtual void BulletSetup(int bulletDamage1, float flyDistance1, Player owner)
    {
        bulletDisabled = false;
        cd.enabled = true;
        //meshRenderer.enabled = true;

        BulletDamage = bulletDamage1;
        Owner = owner;

        trailRenderer.time = .04f;
        startPosition = transform.position;
        flyDistance = flyDistance1 + .5f;

        ApplyTierVisuals(owner);
    }

    private void ApplyTierVisuals(Player owner)
    {
        if (owner == null || owner.weapon == null) return;

        Weapon weapon = owner.weapon.CurrentWeapon();
        int tier = weapon != null ? weapon.PackAPunchTier : 0;

        Gradient trailGradient;
        Color emissionColor;

        switch (tier)
        {
            case 1:
                trailGradient = tier1TrailColor;
                emissionColor = tier1Emission;
                break;
            case 2:
                trailGradient = tier2TrailColor;
                emissionColor = tier2Emission;
                break;
            case 3:
                trailGradient = tier3TrailColor;
                emissionColor = tier3Emission;
                break;
            default:
                trailGradient = baseTrailColor;
                emissionColor = baseEmission;
                break;
        }

        if (trailRenderer)
            trailRenderer.colorGradient = trailGradient;

        if (meshRenderer && meshRenderer.material.HasProperty("_EmissionColor"))
            meshRenderer.material.SetColor("_EmissionColor", emissionColor);
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
            AudioManager.Instance.PlaySFX("BulletImpact",  shootVolume);
            ContactPoint contact = collision.contacts[0];
            GameObject newImpactFx = ObjectPool.instance.GetObject(bulletImpactFX);
            newImpactFx.transform.position = contact.point;
            ObjectPool.instance.ReturnObject(1, newImpactFx);
        }
    }
}
