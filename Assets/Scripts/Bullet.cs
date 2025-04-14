using System;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    private Rigidbody rb => GetComponent<Rigidbody>();
    
    private void OnCollisionEnter(Collision collision)
    {
        //rb.constraints = RigidbodyConstraints.FreezeAll;
        Debug.Log("Bullet Hit");
        Destroy(gameObject);
        
    }
}
