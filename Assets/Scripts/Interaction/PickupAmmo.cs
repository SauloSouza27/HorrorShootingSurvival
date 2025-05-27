using UnityEngine;

public class PickupAmmo : Interactable
{
    public override void Interaction()
    {
        Debug.Log("Added ammo to weapon");
    }
}
