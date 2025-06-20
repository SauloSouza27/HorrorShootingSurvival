
using System;
using Unity.VisualScripting; 
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject bulletImpactFX;

    private BoxCollider cd;
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private TrailRenderer trailRenderer;
    
    private Player player;

    
    private TrailRenderer tr => GetComponentInChildren<TrailRenderer>();

    private Vector3 startPosition;
    private float flyDistance;
    private bool bulletDisabled;
    public int bulletDamage { get; private set; } // This damage value will be passed to the enemy

    private void Awake()
    {
        player = GetComponent<Player>();
        cd = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public void BulletSetup(int bulletDamage1,float flyDistance1)
    {
        bulletDisabled = false;
        cd.enabled = true;
        meshRenderer.enabled = true;
        this.bulletDamage = bulletDamage1;

        trailRenderer.time = .04f;
        startPosition = transform.position;
        this.flyDistance = flyDistance1 + .5f; // remember this
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
            tr.Clear();
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
        //IDamageable damageable = player.gameObject.GetComponentInChildren<IDamageable>();
        //damageable?.TakeDamage();
        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            
            if (enemy != null)
            {
                enemy.TakeDamage(bulletDamage);
            }
        }
        // Always clear the trail and return the bullet to the pool after a collision
        tr.Clear();
        CreateImpactFx(collision);
        ObjectPool.instance.ReturnObject(0, gameObject);
    }

    private void ReturnBulletToPool() => ObjectPool.instance.ReturnObject(0, gameObject);

    private void CreateImpactFx(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];

            GameObject newImpactFx = ObjectPool.instance.GetObject(bulletImpactFX);
            newImpactFx.transform.position = contact.point;

            ObjectPool.instance.ReturnObject(1, newImpactFx);
        }
    }
}
