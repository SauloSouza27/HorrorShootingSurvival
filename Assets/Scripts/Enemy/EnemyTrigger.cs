using System;
using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
        damageable?.TakeDamage();
    }
}
