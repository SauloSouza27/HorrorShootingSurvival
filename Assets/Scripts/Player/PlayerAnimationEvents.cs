using System;
using System.Collections;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerWeaponVisuals visuals;

    private void Start()
    {
        visuals = GetComponentInParent<PlayerWeaponVisuals>();
    }

    public void ReloadIsOver()
    {
        visuals.MaximizeRigWeight();
        
        //refill bullets
    }

    public void ReturnRig()
    {
        visuals.MaximizeRigWeight();
        visuals.MaximizeWeightToLeftHandIK();
    }
    public void WeaponEquipIsOver()
    {
        visuals.SetBusyEquippingWeaponTo(false);
    }
}
