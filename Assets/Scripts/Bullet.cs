using System;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    private Rigidbody rb => GetComponent<Rigidbody>();
    
    private void OnCollisionEnter(Collision other)
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}
