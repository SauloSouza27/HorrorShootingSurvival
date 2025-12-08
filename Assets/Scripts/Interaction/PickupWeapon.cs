using UnityEngine;
using System.Collections.Generic;

public class PickupWeapon : Interactable
{
    [Header("Weapon Info")]
    [SerializeField] private Weapon_Data weaponData;
    [SerializeField] private Weapon weapon;
    [SerializeField] private BackupWeaponModel[] models;

    // If false, Start() will create a new Weapon from weaponData.
    // If true, this was spawned from an existing Weapon instance (dropped weapon).
    private bool oldWeapon;

    // --- DEBUG UI ---
    [Header("DEBUG UI")]
    [SerializeField] private bool debugUI = true;
    [SerializeField] private float screenOffsetY = 1.8f;

    private readonly HashSet<Player> playersInRange = new HashSet<Player>();
    // ----------------
    
    public override bool SupportsHighlight => true;

    private void Start()
    {
        // For manually placed pickups (if you ever use them again)
        if (!oldWeapon && weaponData != null)
        {
            weapon = new Weapon(weaponData);
        }

        UpdateGameObject();
    }

    /// <summary>
    /// Called when a weapon is dropped from a player.
    /// </summary>
    public void SetupPickupWeapon(Weapon weapon, Transform fromTransform)
    {
        oldWeapon = true;

        this.weapon = weapon;
        weaponData = weapon.WeaponData;

        // Position slightly above the ground at drop point
        transform.position = fromTransform.position + new Vector3(0f, 0.75f, -2f);

        UpdateGameObject();
    }

    [ContextMenu("UpdateItemModel")]
    public void UpdateGameObject()
    {
        if (weaponData == null)
            return;

        gameObject.name = "PickupWeapon - " + weaponData.weaponType;
        UpdateItemModel();
    }

    private void UpdateItemModel()
    {
        if (models == null || weaponData == null)
            return;

        foreach (BackupWeaponModel model in models)
        {
            if (model == null) continue;

            bool shouldBeActive = model.WeaponType == weaponData.weaponType;
            model.gameObject.SetActive(shouldBeActive);

            if (shouldBeActive)
            {
                var mr = model.GetComponent<MeshRenderer>();
                if (mr != null)
                    UpdateMeshAndMaterial(mr);
            }
        }
    }

    public override void Interaction(Player player)
    {
        if (player == null || weapon == null)
            return;

        var weaponController = player.GetComponent<PlayerWeaponController>();
        if (weaponController == null)
            return;

        // Give weapon to the interacting player
        weaponController.PickupWeapon(weapon);

        // Return this pickup to the pool (or destroy if no pool)
        if (ObjectPool.instance != null)
        {
            ObjectPool.instance.ReturnObject(0f, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===== Trigger tracking for debug UI =====
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        var player = other.GetComponent<Player>();
        if (player != null)
            playersInRange.Add(player);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        var player = other.GetComponent<Player>();
        if (player != null)
            playersInRange.Remove(player);
    }
    // =========================================

    // ===== Simple debug world-space UI =====
    private void OnGUI()
    {
        if (!debugUI) return;
        if (playersInRange.Count == 0) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = transform.position + Vector3.up * screenOffsetY;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0) return; // behind camera

        screenPos.y = Screen.height - screenPos.y;

        Rect rect = new Rect(screenPos.x - 80f, screenPos.y - 40f, 160f, 38f);

        // Background
        Color old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;

        string weaponName = weaponData != null
            ? weaponData.weaponType.ToString()
            : "Weapon";

        GUI.Label(
            new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 18f),
            weaponName
        );
        GUI.Label(
            new Rect(rect.x + 8f, rect.y + 20f, rect.width - 16f, 16f),
            "Press Interact to pick up"
        );

        GUI.color = old;
    }
    // =======================================

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (weaponData == null) return;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.5f,
            $"Pickup: {weaponData.weaponType}"
        );
    }
#endif
}
