using UnityEngine;
using UnityEngine.Serialization;


public enum WeaponType
{
    Pistol,
    Revolver,
    AutoRifle,
    Shotgun,
    Sniper,
}

public enum ShootType
{
    Single,
    Auto
}


[System.Serializable] 
public class Weapon
{
    public WeaponType weaponType;
    public Sprite weaponIcon;
    
    public int bulletDamage;

    [Header("Shooting specific")] 
    public ShootType shootType;
    public int BulletsPerShot { get; private set; }
    public float fireRate = 1; //bullets per second
    private float lastShootTime;

    [Header("Burst fire")] 
    private bool burstAvailable;
    public bool burstActive;
    
    private int burstBulletPerShot;
    //public float burstFireRate;
    public float BurstFireDelay { get; private set; }
    
    [Header("Magazine details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;
    
    public float ReloadSpeed { get; private set; }
    public float EquipSpeed { get; private set; }
    public float BulletDistance { get; private set; }

    [Header("Spread")] 
    private float baseSpread;
    private float maxSpread = 3;
    private float currentSpread = 0;
    
    private float spreadIncreaseRate = .15f;

    private float lastSpreadUpdateTime;
    private float spreadCooldown = 1;
    
    public Weapon_Data WeaponData {get; private set;} // serves as default weapon data

    public Weapon(Weapon_Data weaponData)
    {
        weaponIcon = weaponData.weaponIcon;
        bulletDamage = weaponData.bulletDamage;
        bulletsInMagazine = weaponData.bulletsInMagazine;
        magazineCapacity = weaponData.magazineCapacity;
        totalReserveAmmo = weaponData.totalReserveAmmo;
        
        fireRate = weaponData.fireRate;
        weaponType = weaponData.weaponType;
        
        BulletsPerShot = weaponData.bulletsPerShot;
        shootType = weaponData.shootType;
        
        baseSpread = weaponData.baseSpread;
        maxSpread = weaponData.maxSpread;
        spreadIncreaseRate = weaponData.spreadIncreaseRate;
        
        ReloadSpeed = weaponData.reloadSpeed;
        EquipSpeed = weaponData.equipSpeed;
        BulletDistance = weaponData.bulletDistance;
        
        burstAvailable = weaponData.burstAvailable;
        burstActive = weaponData.burstActive;
        burstBulletPerShot = weaponData.burstBulletPerShot;
        BurstFireDelay = weaponData.burstFireDelay;

        this.WeaponData = weaponData;
    }

    #region Spread methods
    
    public Vector3 ApplySpread(Vector3 originalDirection)
    {
        UpdateSpread();
        
        float randomizedValue = Random.Range(-currentSpread, currentSpread);
        
        Quaternion spreadRotation = Quaternion.Euler(randomizedValue, randomizedValue, randomizedValue);
        
        return spreadRotation * originalDirection;
    }

    private void UpdateSpread()
    {
        if (Time.time > lastSpreadUpdateTime + spreadCooldown)
            currentSpread = baseSpread;
        else
            IncreaseSpread();
        
        lastSpreadUpdateTime = Time.time;
    }

    private void IncreaseSpread()
    {
        currentSpread = Mathf.Clamp(currentSpread + spreadIncreaseRate, baseSpread, maxSpread);
    }
    
    #endregion
    
    #region Burst methods

    public bool BurstActivated()
    {
        if (weaponType == WeaponType.Shotgun)
        {
            BurstFireDelay = 0;
            return true;
        }
        
        return burstActive;
    }

    public void ToggleBurst()
    {
        if (burstAvailable == false)
            return;
        
        burstActive = !burstActive;
        
        BulletsPerShot = burstActive ? burstBulletPerShot : 1;
    }
    
    
    #endregion

    public bool CanShoot()
    {
        if (HasEnoughBullets() && ReadyToFire())
        {
            if(weaponType == WeaponType.Shotgun)
                bulletsInMagazine--;
            return true;
        }
        return false;
    }
    

    private bool ReadyToFire()
    {
        if (!(Time.time > lastShootTime + 1 / fireRate)) 
            return false;
        
        lastShootTime = Time.time;
        return true;
    }

    #region Reload methods
    private bool HasEnoughBullets()
    {
        if (bulletsInMagazine > 0)
        {
            return true;
        }

        return false;
    }

    public bool CanReload()
    {
        if (bulletsInMagazine == magazineCapacity)
            return false;
        
        if (totalReserveAmmo > 0)
        {
            return true;
        }

        return false;
    }

    public void ReloadBullets()
    {
        totalReserveAmmo += bulletsInMagazine; // return bullets in magazine to total
        
        int bulletsToReload = magazineCapacity;
        
        if (bulletsToReload > totalReserveAmmo)
            bulletsToReload = totalReserveAmmo;
        
        totalReserveAmmo -= bulletsToReload;
        bulletsInMagazine = bulletsToReload;
        
        if (totalReserveAmmo < 0)
            totalReserveAmmo = 0;
    }
    
    #endregion
}
