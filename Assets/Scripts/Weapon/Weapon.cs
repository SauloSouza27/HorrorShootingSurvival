using UnityEngine;


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
    public WeaponType WeaponType;

    [Header("Shooting specific")] 
    public ShootType shootType;
    public float fireRate = 1; //bullets per second
    private float lastShootTime;
    
    
    [Header("Magazine details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    [Range(1, 5)]
    public float reloadSpeed = 1;
    [Range(1, 5)]
    public float equipSpeed = 1;

    [Header("Spread")] 
    public float baseSpread;
    public float currentSpread = 2;
    private float maxSpread = 3;

    public float spreadIncreaseRate = .15f;

    private float lastSpreadUpdateTime;
    private float spreadCooldown = 1;

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
    
    public bool CanShoot()
    {
        if (HasEnoughBullets() && ReadyToFire())
        {
            bulletsInMagazine--;
            return true;
        }

        return false;
    }

    private bool ReadyToFire()
    {
        if (Time.time > lastShootTime + 1 / fireRate)
        {
            lastShootTime = Time.time;
            return true;
        }
        return false;
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
