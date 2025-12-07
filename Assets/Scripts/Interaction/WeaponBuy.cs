using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponBuy : Interactable
{
    [Header("Weapon Info")]
    [SerializeField] private Weapon_Data weaponData;
    [SerializeField] private WeaponType weaponType;

    [Header("Prices")]
    [SerializeField] private int weaponBuyCost = 1000;
    [SerializeField] private int ammoBuyCost = 500;

    public override bool SupportsHighlight => true;

    // ===== DEBUG UI =====
    [Header("DEBUG UI")]
    [SerializeField] private bool debugUI = true;
    [SerializeField] private Color debugPanelColor = new Color(0f, 0f, 0f, 0.65f);
    [SerializeField] private Color debugTextColor = Color.white;
    [SerializeField] private Color debugAffordableColor = Color.green;
    [SerializeField] private Color debugNotAffordableColor = Color.red;

    private readonly HashSet<Player> playersInRange = new HashSet<Player>();
    // ====================


    public override void Interaction(Player player)
    {
        if (player == null) return;

        var stats = player.GetComponent<PlayerStats>();
        var weaponController = player.GetComponent<PlayerWeaponController>();

        if (stats == null || weaponController == null)
            return;

        // --- Does this player already own this weapon? Then it's an ammo buy ---
        Weapon ownedWeapon = weaponController.WeaponInSlots(weaponType);
        if (ownedWeapon != null)
        {
            TryBuyAmmo(stats, ownedWeapon);
            return;
        }

        // --- Otherwise, buy the weapon ---
        TryBuyWeapon(stats, weaponController);
    }

    private void TryBuyWeapon(PlayerStats stats, PlayerWeaponController weaponController)
    {
        if (!stats.CanAfford(weaponBuyCost))
        {
            Debug.Log("Not enough points to buy weapon.");
            return;
        }

        stats.SpendPoints(weaponBuyCost);

        Weapon newWeapon = new Weapon(weaponData);
        weaponController.PickupWeapon(newWeapon);

        Debug.Log($"Bought new weapon: {weaponData.weaponName}");
    }

    private void TryBuyAmmo(PlayerStats stats, Weapon ownedWeapon)
    {
        if (!stats.CanAfford(ammoBuyCost))
        {
            Debug.Log("Not enough points to buy ammo.");
            return;
        }

        stats.SpendPoints(ammoBuyCost);

        // refill ammo fully (you can tweak this logic)
        ownedWeapon.totalReserveAmmo = ownedWeapon.WeaponData.totalReserveAmmo;
        ownedWeapon.bulletsInMagazine = ownedWeapon.magazineCapacity;

        Debug.Log($"Bought ammo refill for: {ownedWeapon.weaponType}");
    }

    // ================== TRIGGER TRACKING (for debug UI) ==================
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        var player = other.GetComponent<Player>();
        if (player != null)
        {
            playersInRange.Add(player);
            showBuyCanvas(player);
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        var player = other.GetComponent<Player>();
        if (player != null)
        {
            playersInRange.Remove(player);
            hideBuyCanvas(player);
        }
    }
    // =====================================================================

    // ======================= DEBUG WORLDSPACE UI ==========================
    private void OnGUI()
    {
        if (!debugUI) return;
        if (playersInRange.Count == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // World → Screen (slightly above the wall-buy object)
        Vector3 worldPos = transform.position + Vector3.up * 1.8f;
        Vector3 screen = cam.WorldToScreenPoint(worldPos);
        if (screen.z < 0) return; // behind camera

        // Flip Y for GUI coordinates
        screen.y = Screen.height - screen.y;

        // Basic rect layout
        Vector2 size = new Vector2(260f, 60f);
        Rect rect = new Rect(
            screen.x - size.x * 0.5f,
            screen.y - size.y,
            size.x,
            size.y
        );

        // Decide context based on *one* player (e.g. closest)
        Player contextPlayer = GetClosestPlayerTo(worldPos);
        string weaponName = weaponData != null ? weaponData.weaponName : weaponType.ToString();

        bool owns = false;
        bool canAfford = false;
        int cost = 0;
        string actionText = "";

        if (contextPlayer != null)
        {
            var stats = contextPlayer.GetComponent<PlayerStats>();
            var weaponController = contextPlayer.GetComponent<PlayerWeaponController>();

            if (weaponController != null)
                owns = weaponController.WeaponInSlots(weaponType) != null;

            if (owns)
            {
                actionText = "Buy Ammo";
                cost = ammoBuyCost;
            }
            else
            {
                actionText = "Buy Weapon";
                cost = weaponBuyCost;
            }

            if (stats != null)
                canAfford = stats.CanAfford(cost);
        }

        // Panel background
        Color oldColor = GUI.color;
        GUI.color = debugPanelColor;
        GUI.Box(rect, GUIContent.none);
        GUI.color = oldColor;

        // Header line: weapon name
        Rect headerRect = new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 20f);
        GUI.color = debugTextColor;
        GUI.Label(headerRect, weaponName);

        // Action line: Buy Weapon / Buy Ammo + cost
        Rect actionRect = new Rect(rect.x + 8f, rect.y + 24f, rect.width - 16f, 18f);
        GUI.color = canAfford ? debugAffordableColor : debugNotAffordableColor;
        string costLine = $"{actionText} - Cost: {cost}";
        GUI.Label(actionRect, costLine);

        // Hint line
        Rect hintRect = new Rect(rect.x + 8f, rect.y + 40f, rect.width - 16f, 16f);
        GUI.color = debugTextColor;
        GUI.Label(hintRect, "Press Interact to purchase");

        GUI.color = oldColor;
    }

    private Player GetClosestPlayerTo(Vector3 worldPos)
    {
        Player closest = null;
        float minDist = float.MaxValue;

        foreach (var p in playersInRange)
        {
            if (p == null) continue;
            float d = Vector3.Distance(worldPos, p.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = p;
            }
        }

        return closest;
    }
    // =====================================================================

    void showBuyCanvas(Player player)
    {
        var weaponController = player.GetComponent<PlayerWeaponController>();
        

        if (weaponController == null)
            return;
        
        BuyWeaponWorldUI buyWeaponWorldUI = transform.GetComponent<BuyWeaponWorldUI>();
        

        // --- Does this player already own this weapon? 
        Weapon ownedWeapon = weaponController.WeaponInSlots(weaponType);
        if (ownedWeapon != null)
        {
            buyWeaponWorldUI.SetupBuyAmmoCanvas(weaponType.ToString(), ammoBuyCost);
        } 
        else
        {
            buyWeaponWorldUI.SetupBuyWeaponCanvas(weaponType.ToString(), weaponBuyCost);
        }
        buyWeaponWorldUI.ShowUI();
    }

    void hideBuyCanvas(Player player)
    {
        var weaponController = player.GetComponent<PlayerWeaponController>();
        

        if (weaponController == null)
            return;
        
        BuyWeaponWorldUI buyWeaponWorldUI = transform.GetComponent<BuyWeaponWorldUI>();
        buyWeaponWorldUI.HideUI();
    }
}
