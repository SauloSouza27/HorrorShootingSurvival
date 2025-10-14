using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameObject bulletImpactFX;

    // 👇 make these protected so child classes can use them
    protected BoxCollider cd;
    protected Rigidbody rb;
    protected MeshRenderer meshRenderer;
    protected TrailRenderer trailRenderer;

    public Player Owner { get; private set; }

    protected Vector3 startPosition;
    protected float flyDistance;
    protected bool bulletDisabled;

    // 👇 protected property instead of private field
    public int BulletDamage { get; protected set; }

    protected virtual void Awake()
    {
        cd = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    // 👇 virtual so child can extend if needed
    public virtual void BulletSetup(int bulletDamage1, float flyDistance1, Player owner)
    {
        bulletDisabled = false;
        if (cd) cd.enabled = true;
        if (meshRenderer) meshRenderer.enabled = true;

        BulletDamage = bulletDamage1;
        Owner = owner;

        if (trailRenderer) trailRenderer.time = .04f;
        startPosition = transform.position;
        flyDistance = flyDistance1 + .5f;
    }

    protected virtual void FixedUpdate()
    {
        FadeTrail();
        DisableBullet();
        ReturnToPoolIfNeeded();
    }

    protected void ReturnToPoolIfNeeded()
    {
        if (trailRenderer && trailRenderer.time < 0)
        {
            trailRenderer.Clear();
            ReturnToPool();
        }
    }

    protected void DisableBullet()
    {
        if (Vector3.Distance(startPosition, transform.position) > flyDistance && !bulletDisabled)
        {
            if (cd) cd.enabled = false;
            if (meshRenderer) meshRenderer.enabled = false;
            bulletDisabled = true;
        }
    }

    protected void FadeTrail()
    {
        if (!trailRenderer) return;
        if (Vector3.Distance(startPosition, transform.position) > flyDistance - 1.5f)
            trailRenderer.time -= 2 * Time.deltaTime;
    }

    // 👇 virtual so child can replace collision behavior
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(BulletDamage, Owner);
            }
        }

        if (trailRenderer) trailRenderer.Clear();
        CreateImpactFx(collision);
        ReturnToPool();
    }

    // 👇 protected so child can optionally call it
    protected void ReturnToPool() => ObjectPool.instance.ReturnObject(0, gameObject);

    // 👇 protected so child can optionally call a visual impact
    protected void CreateImpactFx(Collision collision)
    {
        if (!bulletImpactFX || collision.contacts.Length == 0) return;

        ContactPoint contact = collision.contacts[0];
        GameObject newImpactFx = ObjectPool.instance.GetObject(bulletImpactFX);
        newImpactFx.transform.position = contact.point;
        ObjectPool.instance.ReturnObject(1, newImpactFx);
    }
}
