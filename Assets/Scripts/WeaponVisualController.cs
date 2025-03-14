using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class WeaponVisualController : MonoBehaviour
{
    private Animator animator;
    
    [SerializeField] private Transform[] gunTransforms;
    
    [SerializeField] private Transform pistol;
    [SerializeField] private Transform revolver;
    [SerializeField] private Transform autoRifle;
    [SerializeField] private Transform shotgun;
    [SerializeField] private Transform sniper;

    private Transform currentGun;

    [Header("Rig")] 
    [SerializeField] private float rigIncreaseStep;
    private bool rigShouldBeIncreased;
    
    [Header("Left hand IK")]
    [SerializeField] private Transform leftHand;
    

    private Rig rig;

    private void Start()
    {
        SwitchOn(pistol);

        animator = GetComponentInChildren<Animator>();
        rig = GetComponentInChildren<Rig>();
    }

    private void Update()
    {
        CheckWeaponSwitch();

        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("Reload");
            PauseRig();
        }

        if (rigShouldBeIncreased)
        {
            rig.weight += rigIncreaseStep * Time.deltaTime;
            
            if (rig.weight >= 1)
                rigShouldBeIncreased = false;
        }
    }

    private void PauseRig()
    {
        rig.weight = .15f;
    }

    private void PlayWeaponEquipAnimation(EquipType equipType)
    {
        PauseRig();
        animator.SetFloat("WeaponEquipType", (float)equipType);
        animator.SetTrigger("WeaponEquip");
    }
    
    public void ReturnRigWeightToOne() => rigShouldBeIncreased = true;

    

    private void SwitchOn(Transform gunTransform)
    {
        SwitchOffGuns();
        gunTransform.gameObject.SetActive(true);
        currentGun = gunTransform;
        
        AttachLeftHand();
    }

    private void SwitchOffGuns()
    {
        for (int i = 0; i < gunTransforms.Length; i++)
        {
            gunTransforms[i].gameObject.SetActive(false);
        }
    }

    private void AttachLeftHand()
    {
        Transform targetTransform = currentGun.GetComponentInChildren<LeftHandTargetTransform>().transform;
        
        leftHand.localPosition = targetTransform.localPosition;
        leftHand.localRotation = targetTransform.localRotation;
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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchOn(pistol);
            SwitchAnimationLayer(1);
            PlayWeaponEquipAnimation(EquipType.SideEquip);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchOn(revolver);
            SwitchAnimationLayer(1);
            PlayWeaponEquipAnimation(EquipType.SideEquip);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchOn(autoRifle);
            SwitchAnimationLayer(1);
            PlayWeaponEquipAnimation(EquipType.BackEquip);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchOn(shotgun);
            SwitchAnimationLayer(2);
            PlayWeaponEquipAnimation(EquipType.BackEquip);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SwitchOn(sniper);
            SwitchAnimationLayer(3);
            PlayWeaponEquipAnimation(EquipType.BackEquip);
        }
    }
}

public enum EquipType
{
    SideEquip,
    BackEquip
};
