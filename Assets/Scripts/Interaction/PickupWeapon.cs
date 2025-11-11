using UnityEngine;
using System.Collections.Generic;

public class PickupWeapon : Interactable
{
    [Header("Weapon Info")]
    [SerializeField] private Weapon_Data weaponData;
    [SerializeField] private Weapon weapon;
    [SerializeField] private BackupWeaponModel[] models;

    [Header("Purchase Settings")]
    [SerializeField] private int weaponCost = 1000;
    private bool isPurchased = false;

    private PlayerWeaponController weaponController;
    private PlayerStats buyerStats;
    private bool oldWeapon;

    // Range detection
    private readonly HashSet<Player> playersInRange = new HashSet<Player>();
    private bool playerNearby => playersInRange.Count > 0;

    // Debug UI
    [Header("DEBUG UI")]
    [SerializeField] private bool debugUI = true;
    private float screenOffsetY = 1.8f;

    private void Awake()
    {
        // 🧩 Auto-register this pickup to the ObjectPool if placed manually
        if (GetComponent<PooledObject>() == null)
        {
            var pooled = gameObject.AddComponent<PooledObject>();
            if (ObjectPool.instance != null && ObjectPool.instance.weaponPickup != null)
                pooled.originalPrefab = ObjectPool.instance.weaponPickup;
            else
                Debug.LogWarning($"[PickupWeapon] Could not auto-register '{name}' because ObjectPool.instance.weaponPickup is missing.");
        }
    }

    private void Start()
    {
        if (!oldWeapon)
            weapon = new Weapon(weaponData);

        // Pull cost automatically if not set
        if (weaponCost <= 0 && weaponData != null)
            weaponCost = Mathf.RoundToInt(weaponData.weaponPrice);

        UpdateGameObject();
    }

    public void SetupPickupWeapon(Weapon weapon, Transform transform)
    {
        oldWeapon = true;
        this.weapon = weapon;
        weaponData = weapon.WeaponData;
        this.transform.position = transform.position + new Vector3(0, 0.75f, 0);
        isPurchased = true; // dropped weapons are free
    }

    [ContextMenu("UpdateItemModel")]
    public void UpdateGameObject()
    {
        gameObject.name = "PickupWeapon - " + weaponData.weaponType.ToString();
        UpdateItemModel();
    }

    public void UpdateItemModel()
    {
        foreach (BackupWeaponModel model in models)
        {
            model.gameObject.SetActive(false);

            if (model.WeaponType == weaponData.weaponType)
            {
                model.gameObject.SetActive(true);
                UpdateMeshAndMaterial(model.GetComponent<MeshRenderer>());
            }
        }
    }

    public override void Interaction(Player player)
    {
        if (player == null) return;

        buyerStats = player.GetComponent<PlayerStats>();
        weaponController = player.GetComponent<PlayerWeaponController>();
        if (buyerStats == null || weaponController == null) return;

        if (!isPurchased)
        {
            // First-time purchase
            if (buyerStats.CanAfford(weaponCost))
            {
                buyerStats.SpendPoints(weaponCost);
                isPurchased = true;
                Debug.Log($"{player.name} bought {weaponData.weaponType} for {weaponCost} points.");
                weaponController.PickupWeapon(new Weapon(weaponData));

                // Instantly remove from scene
                ObjectPool.instance.ReturnObject(0, gameObject);
            }
            else
            {
                Debug.Log($"{player.name} cannot afford {weaponData.weaponType} ({weaponCost} points).");
            }
        }
        else
        {
            // Normal pickup after purchase
            weaponController.PickupWeapon(weapon);
            ObjectPool.instance.ReturnObject(0, gameObject);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        var player = other.GetComponent<Player>();
        if (player != null) playersInRange.Add(player);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        var player = other.GetComponent<Player>();
        if (player != null) playersInRange.Remove(player);
    }

    // ===========================
    // DEBUG UI (only visible when player is near)
    // ===========================
    private void OnGUI()
    {
        if (!debugUI || !playerNearby) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = transform.position + Vector3.up * screenOffsetY;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0) return;

        screenPos.y = Screen.height - screenPos.y;
        Rect rect = new Rect(screenPos.x - 75, screenPos.y - 45, 150, 40);

        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.Box(rect, GUIContent.none);

        GUI.color = Color.white;
        string weaponName = weaponData != null ? weaponData.weaponType.ToString() : "Weapon";
        string costText = isPurchased ? "Free Pickup" : $"Cost: {weaponCost}";
        string info = isPurchased ? "Press Interact to Pick Up" : "Press Interact to Buy";

        GUI.Label(new Rect(rect.x + 8, rect.y + 5, rect.width - 16, 18), $"{weaponName} - {costText}");
        GUI.Label(new Rect(rect.x + 8, rect.y + 20, rect.width - 16, 16), info);
        GUI.color = Color.white;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f,
            isPurchased ? $"{weaponData.weaponType} (Free Pickup)" : $"{weaponData.weaponType} (${weaponCost})");
    }
#endif
}
