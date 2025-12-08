using UnityEngine;
using System.Collections.Generic;

public class Bite : MonoBehaviour
{
    [SerializeField] private LayerMask damageLayerMask;

    private int currentDamage;

    // We track ROOT transforms here so multiple colliders on the same character
    // only count as ONE hit per bite.
    private readonly HashSet<Transform> hitRoots = new HashSet<Transform>();

    public void SetAttack(int damage)
    {
        currentDamage = damage;
    }

    private void OnEnable()
    {
        // New bite window -> reset list of things we already damaged
        hitRoots.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((damageLayerMask.value & (1 << other.gameObject.layer)) == 0)
            return;
        
        Transform root = other.transform.root != null
            ? other.transform.root
            : other.transform;

        // If we've already hit this root this bite, ignore
        if (hitRoots.Contains(root))
            return;

        // Find IDamageable somewhere in this root hierarchy
        IDamageable damageable = root.GetComponentInChildren<IDamageable>();
        if (damageable == null)
            return;

        // Apply damage ONCE for this character
        damageable.TakeDamage(currentDamage);

        // Remember that we already hit this root this bite
        hitRoots.Add(root);
    }
}