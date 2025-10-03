using UnityEngine;

public class PackAPunchMachine : Interactable
{
    [Header("Pack-a-Punch Settings")]
    [SerializeField] private int tier1Cost = 5000;
    [SerializeField] private int tier2Cost = 10000;
    [SerializeField] private int tier3Cost = 20000;

    [Header("Visual Upgrades")]
    [SerializeField] private Material tier1Material;
    [SerializeField] private Material tier2Material;
    [SerializeField] private Material tier3Material;

    public override void Interaction(Player player)
    {
        var weaponController = player.GetComponent<PlayerWeaponController>();
        if (weaponController == null || weaponController.CurrentWeapon() == null)
        {
            Debug.Log("No weapon equipped to upgrade.");
            return;
        }

        Weapon weapon = weaponController.CurrentWeapon();
        PlayerStats stats = player.GetComponent<PlayerStats>();

        int currentTier = weapon.PackAPunchTier;

        switch (currentTier)
        {
            case 0:
                TryUpgrade(player, weapon, stats, 1, tier1Cost, tier1Material);
                break;
            case 1:
                TryUpgrade(player, weapon, stats, 2, tier2Cost, tier2Material);
                break;
            case 2:
                TryUpgrade(player, weapon, stats, 3, tier3Cost, tier3Material);
                break;
            case 3:
                Debug.Log("Weapon already max upgraded!");
                break;
        }
    }

    private void TryUpgrade(Player player, Weapon weapon, PlayerStats stats, int newTier, int cost, Material newMat)
    {
        if (!stats.SpendPoints(cost))
        {
            Debug.Log("Not enough points to Pack-a-Punch.");
            return;
        }

        weapon.Upgrade(newTier);

        var weaponModel = player.weaponVisuals.CurrentWeaponModel();
        if (weaponModel != null)
        {
            var renderer = weaponModel.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
                renderer.material = newMat;
        }

        Debug.Log($"Upgraded {weapon.weaponType} to Pack-a-Punch Tier {newTier}");
    }
}
