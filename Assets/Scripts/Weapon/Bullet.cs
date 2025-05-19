using System;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    private Rigidbody rb => GetComponent<Rigidbody>();
    private TrailRenderer tr => GetComponentInChildren<TrailRenderer>();
    
    
    private void OnCollisionEnter(Collision collision)
    {
        tr.Clear();
        ObjectPool.instance.ReturnBullet(gameObject);
    }
    
}
