using UnityEngine;

[System.Serializable]
public class PackAPunchMaterialSet
{
    public WeaponType weaponType;
    public Material tier1;
    public Material tier2;
    public Material tier3;
}

public class PackAPunchMachine : Interactable
{
    [Header("Pack-a-Punch Settings")]
    [SerializeField] private int tier1Cost = 5000;
    [SerializeField] private int tier2Cost = 10000;
    [SerializeField] private int tier3Cost = 20000;

    [Header("Global Fallback Materials (optional)")]
    [SerializeField] private Material tier1Material;   // used if no per-weapon override
    [SerializeField] private Material tier2Material;
    [SerializeField] private Material tier3Material;

    [Header("Per-Weapon Materials")]
    [SerializeField] private PackAPunchMaterialSet[] weaponMaterialSets;

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

        if (stats == null)
        {
            Debug.LogWarning("No PlayerStats found on player for Pack-a-Punch.");
            return;
        }

        int currentTier = weapon.PackAPunchTier;

        switch (currentTier)
        {
            case 0:
                TryUpgrade(player, weapon, stats, 1, tier1Cost);
                break;
            case 1:
                TryUpgrade(player, weapon, stats, 2, tier2Cost);
                break;
            case 2:
                TryUpgrade(player, weapon, stats, 3, tier3Cost);
                break;
            case 3:
                Debug.Log("Weapon already max upgraded!");
                break;
        }
    }

    private void TryUpgrade(Player player, Weapon weapon, PlayerStats stats, int newTier, int cost)
    {
        if (!stats.SpendPoints(cost))
        {
            Debug.Log("Not enough points to Pack-a-Punch.");
            return;
        }

        // Upgrade stats (damage, mag size, etc.)
        weapon.Upgrade(newTier);

        // Pick correct material for this weapon & tier
        Material upgradeMat = GetMaterialFor(weapon.weaponType, newTier);
        if (upgradeMat != null)
        {
            ApplyMaterialToCurrentWeaponModel(player, upgradeMat);
        }
        else
        {
            Debug.LogWarning($"No Pack-a-Punch material found for {weapon.weaponType} at tier {newTier}.");
        }

        Debug.Log($"Upgraded {weapon.weaponType} to Pack-a-Punch Tier {newTier}");
    }

    /// <summary>
    /// Returns the correct material based on weapon type and tier.
    /// Falls back to global tier materials if no specific override is found.
    /// </summary>
    private Material GetMaterialFor(WeaponType weaponType, int tier)
    {
        // 1) Try per-weapon overrides
        foreach (var set in weaponMaterialSets)
        {
            if (set != null && set.weaponType == weaponType)
            {
                switch (tier)
                {
                    case 1: return set.tier1;
                    case 2: return set.tier2;
                    case 3: return set.tier3;
                }
            }
        }

        // 2) Fallback to global material for that tier
        switch (tier)
        {
            case 1: return tier1Material;
            case 2: return tier2Material;
            case 3: return tier3Material;
        }

        return null;
    }

    /// <summary>
    /// Applies the given material to ALL MeshRenderers of the current weapon model,
    /// replacing all material slots (full weapon recolor/remat).
    /// </summary>
    private void ApplyMaterialToCurrentWeaponModel(Player player, Material newMat)
    {
        if (newMat == null) return;

        var weaponModel = player.weaponVisuals.CurrentWeaponModel();
        if (weaponModel == null)
        {
            Debug.LogWarning("No WeaponModel found on player to apply Pack-a-Punch material.");
            return;
        }

        // Get all mesh renderers under this weapon model (in case the gun is split into multiple meshes)
        var renderers = weaponModel.GetComponentsInChildren<MeshRenderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning("No MeshRenderer found under WeaponModel.");
            return;
        }

        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;

            // If the mesh uses multiple materials, replace them all
            var mats = renderer.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = newMat;
            }
            renderer.sharedMaterials = mats;
        }
    }
}
