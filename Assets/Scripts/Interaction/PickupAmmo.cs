using UnityEngine;

public class PickupAmmo : Interactable
{
    public override void Interaction(Player player)
    {
        Debug.Log("Added ammo to weapon");
    }
}
