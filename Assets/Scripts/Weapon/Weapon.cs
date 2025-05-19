using UnityEngine;


public enum WeaponType
{
    Pistol,
    Revolver,
    AutoRifle,
    Shotgun,
    Sniper,
}


[System.Serializable] 
public class Weapon
{
    public WeaponType WeaponType;
    
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;

    [Range(1, 5)]
    public float reloadSpeed = 1;
    [Range(1, 5)]
    public float equipSpeed = 1;

    [Space] 
    public float fireRate = 1; //bullets per second
    private float lastShootTime;

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
