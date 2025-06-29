using System;
using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
    
    
    private void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
        damageable?.TakeDamage();
    }
}
