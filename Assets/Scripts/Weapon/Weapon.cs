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

    [Header("Shooting specific")] 
    public ShootType shootType;
    public int bulletsPerShot;
    public float fireRate = 1; //bullets per second
    private float lastShootTime;

    [Header("Burst fire")] 
    public bool burstAvailable;
    public bool burstActive;
    
    public int burstBulletPerShot;
    public float burstFireRate;
    public float burstFireDelay = .1f;
    
    [Header("Magazine details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    [Range(1, 5)]
    public float reloadSpeed = 1;
    [Range(1, 5)]
    public float equipSpeed = 1;
    [Range(2, 12)] 
    public float bulletDistance = 4f;

    [Header("Spread")] 
    public float baseSpread;
    public float currentSpread = 0;
    public float maxSpread = 3;

    public float spreadIncreaseRate = .15f;

    private float lastSpreadUpdateTime;
    private float spreadCooldown = 1;

    public Weapon(WeaponType weaponType)
    {
        this.fireRate = fireRate;
        this.weaponType = weaponType;
        
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
            burstFireDelay = 0;
            return true;
        }
        
        return burstActive;
    }

    public void ToggleBurst()
    {
        if (burstAvailable == false)
            return;
        
        burstActive = !burstActive;
        
        bulletsPerShot = burstActive ? burstBulletPerShot : 1;
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
