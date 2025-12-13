using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PerkType
{
    Juggernog,     // More health
    SpeedCola,     // Faster reload
    StaminUp,      // Run faster
    QuickRevive,   // Revive faster
    DoubleTap      // Cadência tiro
}

[RequireComponent(typeof(Player))]
public class PlayerStats : MonoBehaviour
{
    private Player player;

    //  Currency
    [SerializeField] private int startingPoints = 500;
    private int currentPoints;

    //  Base stats
    [Header("Base Stats")]
    [SerializeField] private int baseMaxHealth = 100;
    [SerializeField] private float baseReloadSpeedMultiplier = 1f;
    [SerializeField] private float baseRunSpeedMultiplier = 1f;
    [SerializeField] private float baseReviveSpeedMultiplier = 1f;

    //  Active multipliers (affected by perks)
    public int MaxHealth { get; private set; }
    public float ReloadSpeedMultiplier { get; private set; }
    public float RunSpeedMultiplier { get; private set; }
    public float ReviveSpeedMultiplier { get; private set; }
    //  Perk state
    public bool DoubleTapActive { get; private set; }


    //  Perks owned
    private readonly HashSet<PerkType> ownedPerks = new HashSet<PerkType>();

    //  Perks Icons
    [SerializeField] private Sprite juggernogSprite;
    [SerializeField] private Sprite speedColaSprite;
    [SerializeField] private Sprite staminUpSprite;
    [SerializeField] private Sprite quickReviveSprite;
    [SerializeField] private Sprite doubleTapSprite;

    //  Events
    public event Action OnStatsChanged;
    public event Action<int> OnPointsChanged;
    public event Action<PerkType> OnPerkPurchased;

    //  HUD reference
    public ScoreCount scoreCount;
    public Transform perkSlots;
    public GameObject perkIconSlot;
    private int perkCount;
    
    

    private void Awake()
    {
        player = GetComponent<Player>();
        ResetStats();                 // sets MaxHealth / multipliers from base values
        currentPoints = startingPoints;
        OnPointsChanged?.Invoke(currentPoints); // optional: update UI immediately
        updateScoreDisplay();
    }

    private void Start()
    {
        var pi = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (ScoreManager.Instance != null && pi != null)
            ScoreManager.Instance.RegisterPlayer(pi.playerIndex, currentPoints);
    }

    //  Reset stats to base values (called at start or on reset)
    private void ResetStats()
    {
        MaxHealth = baseMaxHealth;
        ReloadSpeedMultiplier = baseReloadSpeedMultiplier;
        RunSpeedMultiplier = baseRunSpeedMultiplier;
        ReviveSpeedMultiplier = baseReviveSpeedMultiplier;
        perkCount = 0;
        DoubleTapActive = false;

        OnStatsChanged?.Invoke();
    }


    //  Currency methods
    public int GetPoints() => currentPoints;

    public bool CanAfford(int cost) => currentPoints >= cost;

    public void AddPoints(int amount)
    {
        if (amount == 0) return;

        currentPoints += amount;
        OnPointsChanged?.Invoke(currentPoints);
        updateScoreDisplay();

        if (scoreCount != null)
            scoreCount.ShowDelta(amount);   // float up
    }

    public bool SpendPoints(int cost)
    {
        if (!CanAfford(cost)) return false;

        currentPoints -= cost;
        OnPointsChanged?.Invoke(currentPoints);
        updateScoreDisplay();

        if (scoreCount != null)
            scoreCount.ShowDelta(-cost);    // float down

        return true;
    }

    public void OverridePoints(int amount)
    {
        amount = Mathf.Max(0, amount);
        int delta = amount - currentPoints;

        currentPoints = amount;
        OnPointsChanged?.Invoke(currentPoints);
        updateScoreDisplay();

        if (scoreCount != null && delta != 0)
            scoreCount.ShowDelta(delta);
    }


    //  Perk purchase logic
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

    // Apply perk effects
    private void ApplyPerk(PerkType perkType)
    {
        GameObject newPerkIcon;
        if (perkCount < 3)
        {
            newPerkIcon = Instantiate(perkIconSlot, perkSlots.GetChild(0), false);
            perkCount++;
        }
        else
        {
            newPerkIcon = Instantiate(perkIconSlot, perkSlots.GetChild(1), false);
            perkCount++;
        }
        switch (perkType)
        {
            case PerkType.Juggernog:
                MaxHealth = Mathf.RoundToInt(baseMaxHealth * 2f); // Double health
                player.health.SetMaxHealth(MaxHealth, healToFull: true);
                newPerkIcon.GetComponent<Image>().sprite = juggernogSprite;
                break;

            case PerkType.SpeedCola:
                ReloadSpeedMultiplier = 0.5f; // 50% reload time
                newPerkIcon.GetComponent<Image>().sprite = speedColaSprite;
                break;

            case PerkType.StaminUp:
                RunSpeedMultiplier = 1.1f; // 10% faster run
                newPerkIcon.GetComponent<Image>().sprite = staminUpSprite;
                break;
            
            case PerkType.QuickRevive:
                ReviveSpeedMultiplier = 0.5f; // 50% faster revive
                newPerkIcon.GetComponent<Image>().sprite = quickReviveSprite;
                break;

            case PerkType.DoubleTap:
                DoubleTapActive = true;
                newPerkIcon.GetComponent<Image>().sprite = doubleTapSprite;

                // Apply Double Tap bonuses to all weapons this player already has
                ApplyDoubleTapToWeapons();
                break;

        }
    }
    private void ApplyDoubleTapToWeapons()
    {
        var weaponController = player.GetComponent<PlayerWeaponController>();
        if (weaponController == null) return;

        weaponController.ApplyDoubleTapToAllWeapons();
    }

    public bool HasDoubleTap() => DoubleTapActive;


    public bool HasPerk(PerkType perkType) => ownedPerks.Contains(perkType);

    private void updateScoreDisplay()
    {
        scoreCount.UpdateScore(currentPoints);
    }
    

}
