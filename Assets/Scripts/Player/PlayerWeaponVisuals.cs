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

    private void Start()
    {
        player = GetComponent<Player>();
        animator = GetComponentInChildren<Animator>();
        rig = GetComponentInChildren<Rig>();
        weaponModels = GetComponentsInChildren<WeaponModel>(true);
        backupWeaponModels = GetComponentsInChildren<BackupWeaponModel>(true);
    }

    private void Update()
    {
        UpdateRigWeight();
        UpdateLeftHandIKWeight();

        if (!player.IsAiming)
        {
            ReduceRigWeight();
        }
    }

    public void PlayFireAnimation() => animator.SetTrigger("Fire");
    
    public void PlayReloadAnimation()
    {
        float reloadSpeed = player.weapon.CurrentWeapon().ReloadSpeed;
        
        animator.SetFloat("ReloadSpeed", reloadSpeed);
        animator.SetTrigger("Reload");
        ReduceRigWeight();
    }
    
    public void PlayWeaponEquipAnimation()
    {
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
        int animationIndex = ((int)CurrentWeaponModel().holdType);

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


