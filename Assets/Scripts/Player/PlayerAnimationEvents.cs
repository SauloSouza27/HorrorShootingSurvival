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
        visualController.reload = false;
        weaponController.CurrentWeapon().ReloadBullets();

        weaponController.SetWeaponReady(true);
        weaponController.UpdateHUD();
    }

    public void ReturnRig()
    {
        visualController.MaximizeRigWeight();
        visualController.MaximizeWeightToLeftHandIK();
    }
    public void WeaponEquipIsOver()
    {
        visualController.reload = false;
        weaponController.SetWeaponReady(true);
    }

    public void FireIsOver()
    {
        visualController.reload = false;
    }

    public void SwitchOnWeaponModel() => visualController.SwitchOnCurrentWeaponModel();
}
