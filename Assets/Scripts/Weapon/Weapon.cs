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

    public bool CanShoot()
    {
        return HasEnoughBullets();
    }

    private bool HasEnoughBullets()
    {
        if (bulletsInMagazine > 0)
        {
            bulletsInMagazine--;
            return true;
        }

        return false;
    }

    public bool canReload()
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
}
