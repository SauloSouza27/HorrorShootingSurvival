using System;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private Weapon_Data weaponData;
    
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<PlayerWeaponController>()?.PickupWeapon(weaponData);
    }
}
