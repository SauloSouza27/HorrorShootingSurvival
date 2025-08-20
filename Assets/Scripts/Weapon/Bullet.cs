
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

    // ✅ Add this
    public Player Owner { get; private set; }

    private Vector3 startPosition;
    private float flyDistance;
    private bool bulletDisabled;
    public int bulletDamage { get; private set; }

    private void Awake()
    {
        cd = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    // ✅ Extend to also take "Player owner"
    public void BulletSetup(int bulletDamage1, float flyDistance1, Player owner)
    {
        bulletDisabled = false;
        cd.enabled = true;
        meshRenderer.enabled = true;

        this.bulletDamage = bulletDamage1;
        this.Owner = owner; // 🔥 Store who fired the bullet

        trailRenderer.time = .04f;
        startPosition = transform.position;
        this.flyDistance = flyDistance1 + .5f;
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
            {
                enemy.TakeDamage(bulletDamage, Owner); // ✅ shooter passed here
            }
        }

        trailRenderer.Clear();
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
