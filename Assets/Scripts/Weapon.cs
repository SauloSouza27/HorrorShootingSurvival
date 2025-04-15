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
    
    public int ammo;
    public int maxAmmo;


}
