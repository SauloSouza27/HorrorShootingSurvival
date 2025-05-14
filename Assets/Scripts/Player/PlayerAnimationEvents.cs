using System;
using System.Collections;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerWeaponVisuals visualController;
    private PlayerWeaponController weaponController;

    private void Start()
    {
        visualController = GetComponentInParent<PlayerWeaponVisuals>();
        weaponController = GetComponentInParent<PlayerWeaponController>();
    }

    public void ReloadIsOver()
    {
        visualController.MaximizeRigWeight();
        weaponController.CurrentWeapon().ReloadBullets();
    }

    public void ReturnRig()
    {
        visualController.MaximizeRigWeight();
        visualController.MaximizeWeightToLeftHandIK();
    }
    public void WeaponEquipIsOver()
    {
        visualController.SetBusyEquippingWeaponTo(false);
    }

    public void SwitchOnWeaponModel() => visualController.SwitchOnCurrentWeaponModel();
}
