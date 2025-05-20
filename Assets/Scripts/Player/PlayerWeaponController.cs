using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour
{
    private Player player;

    [SerializeField] 
    private Weapon currentWeapon;
    private bool weaponReady;
    private bool isShooting;
    
    private PlayerInput playerInput; // Reference to PlayerInput
    private InputAction fireAction; // Fire action input
    
    [Header("Bullet details")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    
    private const float REFERENCE_BULLET_SPEED = 50;

    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform aim;

    [Header("Inventory")] 
    [SerializeField] private List<Weapon> weaponSlots;

    private const int MaxSlots = 2;

    private void Start()
    {
        player = GetComponent<Player>();
        
        playerInput = GetComponent<PlayerInput>(); 
        AssignInputEvents(); 
        
        Invoke(nameof(EquipStartingWeapon), .1f);
        
    }

    private void Update()
    {
        if (isShooting)
        {
            StartCoroutine(HandleShootWithAutoAim());
        }
        
        
    }

    private void EquipStartingWeapon() => EquipWeapon(0);
    
    #region Slots managment - Pickup/Equip/DropWeapon/ReadyWeapon
    private void EquipWeapon(int i)
    {
        SetWeaponReady(false);
        
        currentWeapon = weaponSlots[i];
        player.weaponVisuals.PlayWeaponEquipAnimation();
    }

    public void PickupWeapon(Weapon newWeapon)
    {
        if (weaponSlots.Count >= MaxSlots)
        {
            Debug.Log("No slots avaiable");
            return;
        }
        
        weaponSlots.Add(newWeapon);
        player.weaponVisuals.SwitchOnBackupWeaponModel();
    }

    private void DropWeapon()
    {
        if (HasOnlyOneWeapon())
            return;

        weaponSlots.Remove(currentWeapon);
        EquipWeapon(0);
    }
    
    public void SetWeaponReady(bool ready) => weaponReady = ready;
    public bool WeaponReady() => weaponReady;
    
    #endregion

    private IEnumerator BurstFire()
    {
        SetWeaponReady(false);
        
        for (int i = 1; i <= currentWeapon.bulletsPerShot; i++)
        {
            FireSingleBullet();
            
            yield return new WaitForSeconds(currentWeapon.burstFireDelay);
            
            if (i >= currentWeapon.bulletsPerShot)
                SetWeaponReady(true);
        }
    }
    
    private void Shoot()
    {
        if(!WeaponReady() || !currentWeapon.CanShoot() || !player.IsAiming) return;

        player.weaponVisuals.PlayFireAnimation();
        
        if (currentWeapon.shootType == ShootType.Single)
        {
            isShooting = false;
        }

        if (currentWeapon.BurstActivated())
        {
            StartCoroutine(BurstFire());
            return;
        }
        
        FireSingleBullet();
    }

    private void FireSingleBullet()
    {
        if(currentWeapon.weaponType != WeaponType.Shotgun)
            currentWeapon.bulletsInMagazine--;
        
        GameObject newBullet = ObjectPool.instance.GetBullet();
            
        newBullet.transform.position = GunPoint().position; 
        newBullet.transform.rotation = Quaternion.LookRotation(GunPoint().forward);
        
        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();

        Vector3 bulletsDirection = currentWeapon.ApplySpread(BulletDirection());
        
        rbNewBullet.mass = REFERENCE_BULLET_SPEED / bulletSpeed;
        rbNewBullet.linearVelocity = bulletsDirection * bulletSpeed;
    }

    private void Reload()
    {
        SetWeaponReady(false);
        player.weaponVisuals.PlayReloadAnimation();
    }
    
    public Vector3 BulletDirection()
    {
        //Transform aim = player.aim.GetAim();
        
        Vector3 direction = (aim.position - GunPoint().position).normalized;
        
        direction.y = 0;
        
        return direction;
    }

    public bool HasOnlyOneWeapon() => weaponSlots.Count <= 1;

    public Weapon CurrentWeapon() => currentWeapon;

    public Weapon BackupWeapon()
    {
        foreach (Weapon weapon in weaponSlots)
        {
            if (weapon != currentWeapon)
                return weapon;
        }
        
        return null;
    }
    
    public Transform GunPoint() => player.weaponVisuals.CurrentWeaponModel().gunPoint;

    // private void OnDrawGizmos()
    // {
    //     Gizmos.DrawLine(weaponHolder.position, weaponHolder.position + weaponHolder.forward * 25);
    //     
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawLine(GunPoint().position, GunPoint().position + BulletDirection() * 25);
    // }
    
    #region Input Events
    
    private void AssignInputEvents()
    {
        var playerInput = GetComponent<PlayerInput>();
        var controls = playerInput.actions; 
        
        //controls["Fire"].performed += ctx => StartCoroutine(HandleShootWithAutoAim());

        controls["Fire"].performed += ctx => isShooting = true;

        controls["Fire"].canceled += ctx => isShooting = false;
        
        controls["EquipSlot - 1"].performed += ctx => EquipWeapon(0);
        controls["EquipSlot - 2"].performed += ctx => EquipWeapon(1);
        controls["Drop Current Weapon"].performed += ctx => DropWeapon();
        
        controls["Reload"].performed += ctx =>
        {
            if (currentWeapon.CanReload() && WeaponReady())
            {
                Reload();
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