using System;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    private BoxCollider cd;
    private Rigidbody rb;
    private MeshRenderer meshRenderer;
    private TrailRenderer trailRenderer;
    
    private TrailRenderer tr => GetComponentInChildren<TrailRenderer>();

    private Vector3 startPosition;
    private float flyDistance;
    private bool bulletDisabled;

    private void Awake()
    {
        cd = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public void BulletSetup(float flyDistance)
    {
        bulletDisabled = false;
        cd.enabled = true;
        meshRenderer.enabled = true;
        
        trailRenderer.time = .1f;
        startPosition = transform.position;
        this.flyDistance = flyDistance + .5f; // remember this
    }

    private void FixedUpdate()
    {
        FadeTrail();
        DisableBullet();
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (trailRenderer.time < 0)
        {
            tr.Clear();
            ObjectPool.instance.ReturnBullet(gameObject);
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
        tr.Clear();
        ObjectPool.instance.ReturnBullet(gameObject);
    }
    
}
