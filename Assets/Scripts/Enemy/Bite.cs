using UnityEngine;
using System.Collections.Generic;

public class Bite : MonoBehaviour
{
    [SerializeField] private LayerMask damageLayerMask; 

    private int currentDamage;
    private List<Collider> hitObjects = new List<Collider>(); 

    public void SetAttack(int damage)
    {
        currentDamage = damage;
    }

    private void OnEnable()
    {
        hitObjects.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hitObjects.Contains(other))
        {
            return;
        }


        if ((damageLayerMask.value & (1 << other.gameObject.layer)) > 0)
        {
            IDamageable damageable = other.gameObject.GetComponent<IDamageable>();
            damageable?.TakeDamage(currentDamage);
            {
                
                hitObjects.Add(other);
            }
        }
    }
}