using System;
using System.Collections.Generic;
using UnityEngine;

public enum PerkType
{
    Juggernog,     // More health
    SpeedCola,     // Faster reload
    StaminUp,      // Run faster
    QuickRevive    // Revive faster
}

[RequireComponent(typeof(Player))]
public class PlayerStats : MonoBehaviour
{
    private Player player;

    // 🔹 Currency
    [SerializeField] private int startingPoints = 500;
    [SerializeField] private int currentPoints;

    // 🔹 Base stats
    [Header("Base Stats")]
    [SerializeField] private int baseMaxHealth = 100;
    [SerializeField] private float baseReloadSpeedMultiplier = 1f;
    [SerializeField] private float baseRunSpeedMultiplier = 1f;
    [SerializeField] private float baseReviveSpeedMultiplier = 1f;

    // 🔹 Active multipliers (affected by perks)
    public int MaxHealth { get; private set; }
    public float ReloadSpeedMultiplier { get; private set; }
    public float RunSpeedMultiplier { get; private set; }
    public float ReviveSpeedMultiplier { get; private set; }

    // 🔹 Perks owned
    private readonly HashSet<PerkType> ownedPerks = new HashSet<PerkType>();

    // 🔹 Events
    public event Action OnStatsChanged;
    public event Action<int> OnPointsChanged;
    public event Action<PerkType> OnPerkPurchased;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Start()
    {
        currentPoints = startingPoints;

        // Reset stats to base values at start
        ResetStats();

        // Register player with ScoreManager
        ScoreManager.Instance.RegisterPlayer(player.GetComponent<UnityEngine.InputSystem.PlayerInput>().playerIndex, startingPoints);
    }

    // 🔹 Reset stats to base values (called at start or on reset)
    private void ResetStats()
    {
        MaxHealth = baseMaxHealth;
        ReloadSpeedMultiplier = baseReloadSpeedMultiplier;
        RunSpeedMultiplier = baseRunSpeedMultiplier;
        ReviveSpeedMultiplier = baseReviveSpeedMultiplier;

        OnStatsChanged?.Invoke();
    }

    // 🔹 Currency methods
    public int GetPoints() => currentPoints;

    public bool CanAfford(int cost) => currentPoints >= cost;

    public void AddPoints(int amount)
    {
        currentPoints += amount;
        OnPointsChanged?.Invoke(currentPoints);
    }

    public bool SpendPoints(int cost)
    {
        if (!CanAfford(cost)) return false;

        currentPoints -= cost;
        OnPointsChanged?.Invoke(currentPoints);
        return true;
    }

    // 🔹 Perk purchase logic
    public bool PurchasePerk(PerkType perkType, int cost)
    {
        if (ownedPerks.Contains(perkType))
            return false; // Already owned

        if (!SpendPoints(cost))
            return false; // Can't afford

        ownedPerks.Add(perkType);
        ApplyPerk(perkType);

        OnPerkPurchased?.Invoke(perkType);
        OnStatsChanged?.Invoke();

        return true;
    }

    // 🔹 Apply perk effects
    private void ApplyPerk(PerkType perkType)
    {
        switch (perkType)
        {
            case PerkType.Juggernog:
                MaxHealth = Mathf.RoundToInt(baseMaxHealth * 2f); // Double health
                player.health.SetMaxHealth(MaxHealth, healToFull: true);
                break;

            case PerkType.SpeedCola:
                ReloadSpeedMultiplier = 0.5f; // 50% reload time
                break;

            case PerkType.StaminUp:
                RunSpeedMultiplier = 1.5f; // 50% faster run
                break;

            case PerkType.QuickRevive:
                ReviveSpeedMultiplier = 0.5f; // 50% faster revive
                break;
        }
    }

    public bool HasPerk(PerkType perkType) => ownedPerks.Contains(perkType);
}
