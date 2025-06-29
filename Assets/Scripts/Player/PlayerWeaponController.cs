/*
* This C# script manages the player's weapons, including equipping,
* shooting, reloading, picking up, and dropping weapons.
* It now includes logic for cycling through weapons using a single
* controller button, in addition to existing keyboard slot selection.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine; // Removed Unity.VisualScripting as it wasn't explicitly used and might not be needed.
using UnityEngine.InputSystem; // Essential for Unity's new Input System

public class PlayerWeaponController : MonoBehaviour
{
    private Player player;

    [SerializeField] private Weapon_Data defaultWeaponData;
    [SerializeField] private Weapon currentWeapon;
    private bool weaponReady;
    private bool isShooting;

    private PlayerInput playerInput; // Reference to PlayerInput
    private InputAction fireAction; // Fire action input

    // Keep track of the currently equipped weapon's index for cycling
    private int currentWeaponSlotIndex;

    [Header("Bullet details")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;

    private const float REFERENCE_BULLET_SPEED = 50;

    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform aim;

    [Header("Inventory")]
    [SerializeField] private List<Weapon> weaponSlots;

    [SerializeField] private GameObject weaponPickupPrefab;

    private const int MaxSlots = 2; // Assuming MaxSlots refers to the size of weaponSlots

    [SerializeField] public AmmoCount ammoCount; // Referência à HUD

    [SerializeField] public Image weaponSprite;

    private void Start()
    {
        player = GetComponent<Player>();

        playerInput = GetComponent<PlayerInput>();
        AssignInputEvents();

        Invoke(nameof(EquipStartingWeapon), .1f);

        // Ensure currentWeapon is not null before accessing its properties
        if (currentWeapon != null)
        {
            currentWeapon.bulletsInMagazine = currentWeapon.totalReserveAmmo; // This line seems to initialize ammo incorrectly; usually, it's magazineCapacity
        }
        UpdateHUD();
        UpdateWeaponSprite();
    }

    private void Update()
    {
        if (isShooting)
        {
            StartCoroutine(HandleShootWithAutoAim());
        }
        if (currentWeapon != null && currentWeapon.CanReload() && WeaponReady() && currentWeapon.bulletsInMagazine == 0)
        {
            Reload();
        }
    }

    private void EquipStartingWeapon()
    {
        // Ensure weaponSlots has at least one slot
        if (weaponSlots.Count == 0)
        {
            weaponSlots.Add(new Weapon(defaultWeaponData));
        }
        else
        {
            weaponSlots[0] = new Weapon(defaultWeaponData);
        }

        EquipWeapon(0); // Equip the first weapon (default weapon)
    }

    #region Slots management - Pickup/Equip/DropWeapon/ReadyWeapon
    /// <summary>
    /// Equips a weapon from a specific slot index.
    /// Updates the current weapon and its index.
    /// </summary>
    /// <param name="slotIndex">The index of the weapon slot to equip.</param>
    private void EquipWeapon(int slotIndex)
    {
        // Basic validation: ensure the slotIndex is valid
        if (slotIndex < 0 || slotIndex >= weaponSlots.Count)
        {
            Debug.LogWarning($"Attempted to equip weapon from invalid slot index: {slotIndex}. Weapon slots count: {weaponSlots.Count}");
            return;
        }

        SetWeaponReady(false); // Make sure weapon is not ready during equip animation

        currentWeapon = weaponSlots[slotIndex];
        currentWeaponSlotIndex = slotIndex; // Update the current weapon index

        player.weaponVisuals.PlayWeaponEquipAnimation(); // Play equip animation
        UpdateHUD(); // Update HUD to reflect new weapon's ammo
        UpdateWeaponSprite(); // Update HUD to show new weapon's icon
    }

    public void PickupWeapon(Weapon newWeapon)
    {
        if (WeaponInSlots(newWeapon.weaponType) != null)
        {
            WeaponInSlots(newWeapon.weaponType).totalReserveAmmo += newWeapon.bulletsInMagazine;
            return;
        }

        if (weaponSlots.Count >= MaxSlots)
        {
            // Drop current weapon and replace it
            int weaponIndexToReplace = currentWeaponSlotIndex; // Use the current index to drop and replace

            player.weaponVisuals.SwitchOffWeaponModels(); // Hide current weapon visual
            weaponSlots[weaponIndexToReplace] = newWeapon; // Replace the weapon in the slot

            CreateWeaponOnTheGround(); // Drop the old weapon

            EquipWeapon(weaponIndexToReplace); // Equip the newly picked up weapon
            return;
        }

        // Add new weapon to an empty slot
        weaponSlots.Add(newWeapon);
        player.weaponVisuals.SwitchOnBackupWeaponModel(); // Assuming this enables visual for the new weapon
    }

    private void DropWeapon()
    {
        if (HasOnlyOneWeapon())
            return; // Cannot drop the last weapon

        CreateWeaponOnTheGround(); // Spawn the weapon pickup prefab

        // Remove the current weapon from the list
        weaponSlots.Remove(currentWeapon);

        // Equip the first available weapon (or default to 0 if list changes size)
        // Ensure there's still a weapon to equip, otherwise equip default if needed.
        if (weaponSlots.Count > 0)
        {
            EquipWeapon(0); // Equip the first weapon in the remaining list
        }
        else
        {
            // If dropping leads to no weapons, equip the starting weapon again (or handle empty slot)
            EquipStartingWeapon();
        }
    }

    private void CreateWeaponOnTheGround()
    {
        GameObject droppedWeapon = ObjectPool.instance.GetObject(weaponPickupPrefab);
        droppedWeapon.transform.position = transform.position + transform.forward * 1.5f; // Place slightly in front of player
        droppedWeapon.GetComponent<PickupWeapon>()?.SetupPickupWeapon(currentWeapon, transform);
    }

    public void SetWeaponReady(bool ready) => weaponReady = ready;
    public bool WeaponReady() => weaponReady;

    #endregion

    private IEnumerator BurstFire()
    {
        SetWeaponReady(false);

        for (int i = 1; i <= currentWeapon.BulletsPerShot; i++)
        {
            FireSingleBullet();

            yield return new WaitForSeconds(currentWeapon.BurstFireDelay);

            if (i >= currentWeapon.BulletsPerShot)
                SetWeaponReady(true);
        }
    }

    private void Shoot()
    {
        if (!WeaponReady() || !currentWeapon.CanShoot() || !player.IsAiming) return;

        player.weaponVisuals.PlayFireAnimation();

        if (currentWeapon.shootType == ShootType.Single)
        {
            isShooting = false;
        }

        if (currentWeapon.BurstActivated())
        {
            StartCoroutine(BurstFire());
            UpdateHUD();
            return;
        }

        FireSingleBullet();
        UpdateHUD();
    }

    private void FireSingleBullet()
    {
        if (currentWeapon.weaponType != WeaponType.Shotgun)
            currentWeapon.bulletsInMagazine--;

        GameObject newBullet = ObjectPool.instance.GetObject(bulletPrefab);

        newBullet.transform.position = GunPoint().position;
        newBullet.transform.rotation = Quaternion.LookRotation(GunPoint().forward);

        Rigidbody rbNewBullet = newBullet.GetComponent<Rigidbody>();

        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        // Assuming BulletSetup signature is (float damage, float distance)
        bulletScript.BulletSetup(currentWeapon.bulletDamage, currentWeapon.BulletDistance);

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
        Vector3 direction = (aim.position - GunPoint().position).normalized;
        direction.y = 0; // Keep direction horizontal in a top-down shooter
        return direction;
    }

    public bool HasOnlyOneWeapon() => weaponSlots.Count <= 1;

    public Weapon WeaponInSlots(WeaponType weaponType)
    {
        foreach (Weapon weapon in weaponSlots)
        {
            if (weapon.weaponType == weaponType)
                return weapon;
        }
        return null;
    }

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


    #region Input Events

    private void AssignInputEvents()
    {
        var playerInput = GetComponent<PlayerInput>();
        var controls = playerInput.actions;

        controls["Fire"].performed += ctx => isShooting = true;
        controls["Fire"].canceled += ctx => isShooting = false;

        // Existing keyboard input for slot selection
        controls["EquipSlot - 1"].performed += ctx => EquipWeapon(0);
        controls["EquipSlot - 2"].performed += ctx =>
        {
            if (weaponSlots.Count > 1) // Only allow equipping slot 2 if it exists
            {
                EquipWeapon(1);
            }
        };
        controls["Drop Current Weapon"].performed += ctx => DropWeapon();

        controls["Reload"].performed += ctx =>
        {
            if (currentWeapon != null && currentWeapon.CanReload() && WeaponReady())
            {
                Reload();
            }
        };

        // NEW: Controller Weapon Swap Input
        // You will need to add an Action named "SwapWeaponController" in your Input Action Asset
        // and bind it to the desired controller button (e.g., Gamepad Left Shoulder or Face Button)

        controls["Swap Weapon"].performed += ctx => SwapWeaponController();


    }

    /// <summary>
    /// Cycles through available weapon slots when the controller swap button is pressed.
    /// </summary>
    private void SwapWeaponController()
    {
        if (weaponSlots.Count <= 1)
        {
            // No need to swap if there's only one or no weapon
            Debug.Log("Only one weapon or no weapons available. Cannot swap.");
            return;
        }

        // Increment the index, and wrap around if it exceeds the list count
        currentWeaponSlotIndex = (currentWeaponSlotIndex + 1) % weaponSlots.Count;

        // Equip the weapon at the new index
        EquipWeapon(currentWeaponSlotIndex);
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
    //Atualiza a HUD com a quantidade de munição
    public void UpdateHUD()
    {
        if (ammoCount != null && currentWeapon != null)
        {
            ammoCount.UpdateAmmo(currentWeapon.bulletsInMagazine, currentWeapon.magazineCapacity, currentWeapon.totalReserveAmmo);
        }
        else if (ammoCount != null && currentWeapon == null)
        {
            ammoCount.UpdateAmmo(0, 0, 0);
        }
    }

    public void UpdateWeaponSprite()
    {
        if (currentWeapon.weaponIcon != null && currentWeapon != null)
        {
            weaponSprite.sprite = currentWeapon.weaponIcon;
        } 
    }

    #endregion
}
