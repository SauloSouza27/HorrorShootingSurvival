using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Weapon System/Weapon Data")]
public class Weapon_Data : ScriptableObject
{
    public string weaponName;

    public float weaponPrice;
    public Sprite weaponIcon;

    [Header("Bullet info")]
    public int bulletDamage;
    
    [Header("Normal Fire")]
    public WeaponType weaponType;
    public ShootType shootType;
    public int bulletsPerShot = 1;
    public float fireRate;

    [Header("Burst Fire")] 
    public bool burstAvailable;
    public bool burstActive;
    
    public int burstBulletPerShot;
    //public float burstFireRate;
    public float burstFireDelay = .1f;
    
    [Header("Magazine details")]
    public int bulletsInMagazine;
    public int magazineCapacity;
    public int totalReserveAmmo;
    
    [Header("Spread")] 
    public float baseSpread;
    public float maxSpread = 3;
    public float spreadIncreaseRate = .15f;

    [Header("Stats")] 
    [Range(1, 5)] public float reloadSpeed = 1;
    [Range(1, 5)] public float equipSpeed = 1;
    [Range(4, 24)] public float bulletDistance = 4;
    
    [Header("Bullet Impact Force")] 
    public float impactForce = 1;
    
    [Header("Audio")]
    public AudioClip fireSFX;
    [Range(0f, 1f)] public float fireSFXVolume = 1f;
    public AudioClip reloadSFX;
    [Range(0f, 1f)] public float reloadSFXVolume = 1f;
    
}
