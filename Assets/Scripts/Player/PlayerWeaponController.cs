using System;
using System.Collections;
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
    
    [Header("Bullet details")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Transform gunPoint;
    
    private const float REFERENCE_BULLET_SPEED = 50;

    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform aim;

    [Header("Inventory")] 
    [SerializeField] private List<Weapon> weaponSlots;
    private int maxSlots = 2;

    private void Start()
    {
        player = GetComponent<Player>();
        
        playerInput = GetComponent<PlayerInput>(); 
        animator = GetComponentInChildren<Animator>(); 
        
        AssignInputEvents(); 
        
        currentWeapon.bulletsInMagazine = currentWeapon.totalReserveAmmo;
    }
    
    #region Slots managment - Pickup/Equip/DropWeapon
    private void EquipWeapon(int i)
    {
        currentWeapon = weaponSlots[i];

        player.weaponVisuals.SwitchOffWeaponModels();
        player.weaponVisuals.PlayWeaponEquipAnimation();
    }

    public void PickupWeapon(Weapon newWeapon)
    {
        if (weaponSlots.Count >= maxSlots)
        {
            Debug.Log("No slots avaiable");
            return;
        }
        weaponSlots.Add(newWeapon);
    }

    private void DropWeapon()
    {
        if (weaponSlots.Count <= 1)
            return;

        weaponSlots.Remove(currentWeapon);

        currentWeapon = weaponSlots[0];
    }
    
    #endregion

    private void Shoot()
    {
        if (currentWeapon.CanShoot() == false)
            return;
        
        if (!player.IsAiming) return; // Only allow shooting when aiming
        
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

    public Weapon CurrentWeapon() => currentWeapon;

    public Transform GunPoint() => gunPoint;

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(weaponHolder.position, weaponHolder.position + weaponHolder.forward * 25);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gunPoint.position, gunPoint.position + BulletDirection() * 25);
    }
    
    #region Input Events
    
    private void AssignInputEvents()
    {
        var playerInput = GetComponent<PlayerInput>();
        var controls = playerInput.actions; 
        
        
        // Fire action
        fireAction = controls["Fire"]; 
        
        controls["Fire"].performed += ctx =>
        {
            StartCoroutine(HandleShootWithAutoAim());
        };
        
        

        controls["EquipSlot - 1"].performed += ctx => EquipWeapon(0);
        controls["EquipSlot - 2"].performed += ctx => EquipWeapon(1);
        controls["Drop Current Weapon"].performed += ctx => DropWeapon();

        controls["Reload"].performed += ctx =>
        {
            if (currentWeapon.canReload())
            {
                player.weaponVisuals.PlayReloadAnimation();
            }
        };
    }
    
    private IEnumerator HandleShootWithAutoAim()
    {
        bool wasAlreadyAiming = player.IsAiming;

        if (!wasAlreadyAiming)
            player.SetAutoAiming(true);

        Shoot();

        yield return new WaitForSeconds(0.1f); // allow aiming state to apply for this frame

        if (!wasAlreadyAiming)
            player.SetAutoAiming(false);
    }
    
    #endregion
}