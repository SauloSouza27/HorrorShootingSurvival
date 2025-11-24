using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

public class PlayerWeaponVisuals : MonoBehaviour
{
    private Player player;
    
    private Animator animator;
    
    [SerializeField] private WeaponModel[] weaponModels;
    [SerializeField] private BackupWeaponModel[] backupWeaponModels;
    
    [Header("Rig")] 
    [SerializeField] private float rigWeightIncreaseRate;
    private bool shouldIncrease_RigWeight;
    private Rig rig;
    
    [Header("Left hand IK")]
    [SerializeField] private float leftHandIkWeightIncreaseRate;
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandIK_Target;
    private bool shouldIncrease_LeftHandIKWeight;

    private int animationIndex;
    public bool reload;
    private void Start()
    {
        player = GetComponent<Player>();
        animator = GetComponentInChildren<Animator>();
        rig = GetComponentInChildren<Rig>();
        weaponModels = GetComponentsInChildren<WeaponModel>(true);
        backupWeaponModels = GetComponentsInChildren<BackupWeaponModel>(true);
        
        animationIndex = ((int)CurrentWeaponModel().holdType);
    }

    private void Update()
    {
        UpdateRigWeight();
        UpdateLeftHandIKWeight();

        //if (!player.IsAiming && !reload)
        //{
        //    ReduceRigWeight();
        //    animator.SetLayerWeight(animationIndex, 0.0001f);
        //}
        //else
        //{
        //    animator.SetLayerWeight(animationIndex, 1);
        //}
    }

    public void PlayFireAnimation()
    {
        animator.SetTrigger("Fire");
    }

    public void PlayReloadAnimation()
    {
        float baseReloadSpeed = player.weapon.CurrentWeapon().ReloadSpeed;

        // ⬇️ integrate Speed Cola multiplier
        var stats = player.GetComponent<PlayerStats>();
        float timeMult = stats != null ? Mathf.Max(0.01f, stats.ReloadSpeedMultiplier) : 1f;
        
        // If ReloadSpeedMultiplier is TIME (0.5 = half the time), we invert it for speed:
        float effectiveAnimSpeed = baseReloadSpeed * (1f / timeMult);

        animator.SetFloat("ReloadSpeed", effectiveAnimSpeed);
        animator.SetTrigger("Reload");
        ReduceRigWeight();
        
        // 🔊 3D reload sound
        var weapon = player.weapon.CurrentWeapon();
        if (weapon != null && AudioManager.Instance != null)
        {
            var data = weapon.WeaponData;
            if (data != null && data.reloadSFX != null)
            {
                // Prefer the gun point; fall back to player position
                Vector3 pos = CurrentWeaponModel() != null && CurrentWeaponModel().gunPoint != null
                    ? CurrentWeaponModel().gunPoint.position
                    : transform.position;

                AudioManager.Instance.PlaySFX3D(
                    data.reloadSFX,
                    pos,
                    data.reloadSFXVolume,
                    spatialBlend: 1f,
                    minDistance: 3f,
                    maxDistance: 25f
                );
            }
        }
    }
    
    public void PlayWeaponEquipAnimation()
    {
        //animator.SetLayerWeight(animationIndex, 1f);
        EquipType equipType = CurrentWeaponModel().equipType;
        
        float equipSpeed = player.weapon.CurrentWeapon().EquipSpeed;
        
        leftHandIK.weight = 0;
        ReduceRigWeight();
        animator.SetTrigger("WeaponEquip");
        animator.SetFloat("WeaponEquipType", (float)equipType);
        animator.SetFloat("EquipSpeed", equipSpeed);
    }
    
    public void SwitchOnCurrentWeaponModel()
    {
        animationIndex = ((int)CurrentWeaponModel().holdType);

        SwitchOffWeaponModels();
        SwitchOffBackupWeaponModels();
        
        if(player.weapon.HasOnlyOneWeapon() == false)
            SwitchOnBackupWeaponModel();
        
        SwitchAnimationLayer(animationIndex);
        CurrentWeaponModel().gameObject.SetActive(true);
        AttachLeftHand();
    }
    
    public void SwitchOffWeaponModels()
    {
        for (int i = 0; i < weaponModels.Length; i++)
        {
            weaponModels[i].gameObject.SetActive(false);
        }
    }

    public void SwitchOnBackupWeaponModel()
    {
        WeaponType weaponType = player.weapon.BackupWeapon().weaponType;

        foreach (BackupWeaponModel backupModel in backupWeaponModels)
        {
            if (backupModel.WeaponType == weaponType)
                backupModel.gameObject.SetActive(true);
        }
    }
    
    private void SwitchOffBackupWeaponModels()
    {
        foreach (BackupWeaponModel backupModel in backupWeaponModels)
        {
            backupModel.gameObject.SetActive(false);
        }
    }

    private void SwitchAnimationLayer(int layerIndex)
    {
        
        for (int i = 1; i < animator.layerCount; i++)
        {
            animator.SetLayerWeight(i, 0);
        }
        animator.SetLayerWeight(layerIndex, 1);
    }
    
    public WeaponModel CurrentWeaponModel()
    {
        WeaponModel weaponModel = null;

        WeaponType weaponType = player.weapon.CurrentWeapon().weaponType;

        for (int i = 0; i < weaponModels.Length; i++)
        {
            if (weaponModels[i].weaponType == weaponType)
                weaponModel = weaponModels[i];
        }

        return weaponModel;
    }
    
    public BackupWeaponModel CurrentBackupWeaponModel()
    {
        BackupWeaponModel backupWeaponModel = null;

        WeaponType weaponType = player.weapon.CurrentWeapon().weaponType;

        for (int i = 0; i < backupWeaponModels.Length; i++)
        {
            if (backupWeaponModels[i].WeaponType == weaponType)
                backupWeaponModel = backupWeaponModels[i];
        }

        return backupWeaponModel;
    }


    
    #region Animation Rigging Methods

    private void UpdateLeftHandIKWeight()
    {
        if (!shouldIncrease_LeftHandIKWeight) return;
        leftHandIK.weight += leftHandIkWeightIncreaseRate * Time.deltaTime;
            
        if (leftHandIK.weight >= 1)
            shouldIncrease_LeftHandIKWeight = false;
    }

    private void UpdateRigWeight()
    {
        if (!shouldIncrease_RigWeight) return;
        rig.weight += rigWeightIncreaseRate * Time.deltaTime;
            
        if (rig.weight >= 1)
            shouldIncrease_RigWeight = false;
    }

    private void ReduceRigWeight()
    {
        rig.weight = .15f;
    }
    
    public void MaximizeRigWeight() => shouldIncrease_RigWeight = true;
    public void MaximizeWeightToLeftHandIK() => shouldIncrease_LeftHandIKWeight = true;
    
    private void AttachLeftHand()
    {
        Transform targetTransform = CurrentWeaponModel().holdPoint;
        
        leftHandIK_Target.localPosition = targetTransform.localPosition;
        leftHandIK_Target.localRotation = targetTransform.localRotation;
    }
    
    #endregion
    
}


