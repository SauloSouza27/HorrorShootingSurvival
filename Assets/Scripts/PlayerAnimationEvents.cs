using System;
using System.Collections;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private WeaponVisualController visualController;

    private void Start()
    {
        visualController = GetComponentInParent<WeaponVisualController>();
    }

    public void ReloadIsOver()
    {
        visualController.ReturnRigWeightToOne();
        
        //refill bullets
    }

    public void WeaponEquipIsOver()
    {
        visualController.ReturnRigWeightToOne();
    }
}
