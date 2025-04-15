using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour
{
    private Player player;

    [SerializeField] private Weapon currentWeapon;
    
    private PlayerInput playerInput; // Reference to PlayerInput
    private InputAction fireAction; // Fire action input
    private Animator animator;
    
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Transform gunPoint;
    
    private const float REFERENCE_BULLET_SPEED = 50;

    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform aim;

    [Header("Inventory")] 
    [SerializeField] private List<Weapon> weaponSlots;

    private void Start()
    {
        player = GetComponent<Player>();
        
        playerInput = GetComponent<PlayerInput>(); 
        animator = GetComponentInChildren<Animator>(); 
        
        AssignInputEvents(); 
        
        currentWeapon.ammo = currentWeapon.maxAmmo;
    }

    private void EquipWeapon(int i)
    {
        currentWeapon = weaponSlots[i];
    }

    private void AssignInputEvents()
    {
        var playerInput = GetComponent<PlayerInput>();
        var controls = playerInput.actions; 
        
        
        // Fire action
        fireAction = controls["Fire"]; 
        
        controls["Fire"].performed += ctx =>
        {
            player.SetAiming(true);
            Shoot();
        };
        
        // controls["Fire"].canceled += ctx =>
        // {
        //     player.SetAiming(false);
        // };

        controls["EquipSlot - 1"].performed += ctx => EquipWeapon(0);
        controls["EquipSlot - 2"].performed += ctx => EquipWeapon(1);
    }

    private void Shoot()
    {
        if (!player.IsAiming) return; // Only allow shooting when aiming

        if (currentWeapon.ammo <= 0)
        {
            Debug.Log("no more bullets");
            return;
        }
        
        currentWeapon.ammo--;
        
        GameObject newBullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.LookRotation(gunPoint.forward));
        
        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();

        rbNewBullet.mass = REFERENCE_BULLET_SPEED / bulletSpeed;
        rbNewBullet.linearVelocity = BulletDirection() * bulletSpeed;
    
        Destroy(newBullet, 10);
        animator.SetTrigger("Fire");
    }

    public Vector3 BulletDirection()
    {
        //Transform aim = player.aim.GetAim();
        
        Vector3 direction = (aim.position - gunPoint.position).normalized;
        
        direction.y = 0;
        
        weaponHolder.LookAt(aim);
        gunPoint.LookAt(aim);
        
        return direction;
    }

    public Transform GunPoint() => gunPoint;

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(weaponHolder.position, weaponHolder.position + weaponHolder.forward * 25);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gunPoint.position, gunPoint.position + BulletDirection() * 25);
    }
}