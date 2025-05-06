using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

public class PlayerWeaponVisuals : MonoBehaviour
{
    private Player player;
    
    private Animator animator;
    private bool isEquippingWeapon;
    
    [SerializeField] private WeaponModel[] weaponModels;
    
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
    }

    private void Update()
    {
        CheckWeaponSwitch();
        
        UpdateRigWeight();
        UpdateLeftHandIKWeight();
    }

    public WeaponModel CurrentWeaponModel()
    {
        WeaponModel weaponModel = null;

        WeaponType weaponType = player.weapon.CurrentWeapon().WeaponType;

        for (int i = 0; i < weaponModels.Length; i++)
        {
            if (weaponModels[i].weaponType == weaponType)
                weaponModel = weaponModels[i];
        }

        return weaponModel;
    }

    public void PlayReloadAnimation()
    {
        if (isEquippingWeapon)
            return;
        
        animator.SetTrigger("Reload");
        ReduceRigWeight();
    }

    private void UpdateLeftHandIKWeight()
    {
        if (shouldIncrease_LeftHandIKWeight)
        {
            leftHandIK.weight += leftHandIkWeightIncreaseRate * Time.deltaTime;
            
            if (leftHandIK.weight >= 1)
                shouldIncrease_LeftHandIKWeight = false;
        }
    }

    private void UpdateRigWeight()
    {
        if (shouldIncrease_RigWeight)
        {
            rig.weight += rigWeightIncreaseRate * Time.deltaTime;
            
            if (rig.weight >= 1)
                shouldIncrease_RigWeight = false;
        }
    }

    private void ReduceRigWeight()
    {
        rig.weight = .15f;
    }

    private void PlayWeaponEquipAnimation(EquipType equipType)
    {
        leftHandIK.weight = 0;
        ReduceRigWeight();
        animator.SetFloat("WeaponEquipType", (float)equipType);
        animator.SetTrigger("WeaponEquip");

        SetBusyEquippingWeaponTo(true);
    }

    public void SetBusyEquippingWeaponTo(bool busyEquipping)
    {
        isEquippingWeapon = busyEquipping;
        animator.SetBool("isEquippingWeapon", isEquippingWeapon);
    }


    public void MaximizeRigWeight() => shouldIncrease_RigWeight = true;
    public void MaximizeWeightToLeftHandIK() => shouldIncrease_LeftHandIKWeight = true;
    

    private void SwitchOn()
    {
        SwitchOffWeaponModels();
        CurrentWeaponModel().gameObject.SetActive(true);
        
        AttachLeftHand();
    }

    private void SwitchOffWeaponModels()
    {
        for (int i = 0; i < weaponModels.Length; i++)
        {
            weaponModels[i].gameObject.SetActive(false);
        }
    }

    private void AttachLeftHand()
    {
        Transform targetTransform = CurrentWeaponModel().holdPoint;
        
        leftHandIK_Target.localPosition = targetTransform.localPosition;
        leftHandIK_Target.localRotation = targetTransform.localRotation;
    }

    private void SwitchAnimationLayer(int layerIndex)
    {
        
        for (int i = 1; i < animator.layerCount; i++)
        {
            animator.SetLayerWeight(i, 0);
        }
        animator.SetLayerWeight(layerIndex, 1);
    }
    
    private void CheckWeaponSwitch()
    {
        if (!player.IsAiming)
        {
            SwitchAnimationLayer(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchOn();
            SwitchAnimationLayer(1);
            PlayWeaponEquipAnimation(EquipType.SideEquip);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchOn();
            SwitchAnimationLayer(1);
            PlayWeaponEquipAnimation(EquipType.SideEquip);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchOn();
            SwitchAnimationLayer(1);
            PlayWeaponEquipAnimation(EquipType.BackEquip);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchOn();
            SwitchAnimationLayer(2);
            PlayWeaponEquipAnimation(EquipType.BackEquip);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SwitchOn();
            SwitchAnimationLayer(3);
            PlayWeaponEquipAnimation(EquipType.BackEquip);
        }
    }
}


