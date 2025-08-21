using UnityEngine;

public class PerkMachine : Interactable
{
    public override bool RequiresPlayer => true;
    [SerializeField] private PerkType perkType;
    [SerializeField] private int cost = 3000;

    public override void Interaction(Player player)
    {
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (stats.PurchasePerk(perkType, cost))
            Debug.Log($"Player bought {perkType} for {cost} points!");
        else
            Debug.Log("Purchase failed (not enough points or already owned)");
    }
}