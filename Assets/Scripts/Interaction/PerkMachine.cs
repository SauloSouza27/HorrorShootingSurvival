using System.Collections.Generic;
using UnityEngine;

public class PerkMachine : Interactable
{
    public override bool RemoveAfterInteract => false;

    [Header("Perk Data")]
    [SerializeField] private PerkType perkType;
    [SerializeField] private int cost = 2000;

    [Header("Debug UI")]
    [SerializeField] private bool debugUI = true;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2f, 0);
    
    [SerializeField] private AudioClip failSFX;
    [Range(0f, 1f)] [SerializeField] private float failVolume = 1f;
    [SerializeField] private AudioClip upgradeSFX;
    [Range(0f, 1f)] [SerializeField] private float upgradeVolume = 1f;
    

    private readonly HashSet<Player> playersInRange = new HashSet<Player>();

    public override void Interaction(Player player)
    {
        if (player == null) return;
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (stats.PurchasePerk(perkType, cost))
        {
            Play3D(upgradeSFX, upgradeVolume);
            Debug.Log($"Player {player.name} bought {perkType}");
        }
        else
        {
            Play3D(failSFX, failVolume);
            Debug.Log($"Player {player.name} failed to buy {perkType}");
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        var p = other.GetComponent<Player>();
        if (p != null) playersInRange.Add(p);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        var p = other.GetComponent<Player>();
        if (p != null) playersInRange.Remove(p);
    }

    private void OnGUI()
    {
        if (!debugUI) return;
        if (playersInRange.Count == 0) return;

        var cam = Camera.main; if (!cam) return;

        // pick nearest player
        Player nearest = null; float minD = float.MaxValue;
        foreach (var p in playersInRange)
        {
            if (!p) continue;
            float d = Vector3.Distance(p.transform.position, transform.position);
            if (d < minD) { minD = d; nearest = p; }
        }
        if (!nearest) return;

        var stats = nearest.GetComponent<PlayerStats>();
        int points = stats ? stats.GetPoints() : 0;
        bool canAfford = stats && stats.CanAfford(cost);

        Vector3 screen = cam.WorldToScreenPoint(transform.position + uiOffset);
        if (screen.z < 0) return;
        screen.y = Screen.height - screen.y;

        var rect = new Rect(screen.x - 140, screen.y - 60, 280, 54);
        GUI.color = new Color(0,0,0,0.75f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;

        string name = GetPerkName(perkType);
        string desc = GetPerkDescription(perkType);
        string costLine = canAfford ? $"Cost: {cost}" : $"Not enough ({points}/{cost})";

        GUI.Label(new Rect(rect.x + 8, rect.y + 6, rect.width - 16, 18), $"{name}");
        GUI.Label(new Rect(rect.x + 8, rect.y + 24, rect.width - 16, 18), desc);
        GUI.Label(new Rect(rect.x + 8, rect.y + 42, rect.width - 16, 18), costLine);
    }

    public string GetPerkName(PerkType type)
    {
        switch (type)
        {
            case PerkType.Juggernog: return "Stone Blood";
            case PerkType.SpeedCola: return "Quick Hands";
            case PerkType.StaminUp: return "Endless Sprint";
            case PerkType.QuickRevive: return "Clutch Saver";
            case PerkType.DoubleTap: return "Overclock";
            default: return type.ToString();
        }
    }

    public string GetPerkDescription(PerkType type)
    {
        switch (type)
        {
            case PerkType.Juggernog: return "Increases max health";
            case PerkType.SpeedCola: return "Reloads faster";
            case PerkType.StaminUp: return "Increases run speed";
            case PerkType.QuickRevive: return "Revive allies faster";                
            case PerkType.DoubleTap: return "Increases fire rate";
            default: return "Perk effect.";
        }
    }
    
    private void Play3D(AudioClip clip, float volume)
    {
        if (clip == null || AudioManager.Instance == null) return;

        AudioManager.Instance.PlaySFX3D(
            clip,
            transform.position,
            volume,
            spatialBlend: 1f,
            minDistance: 4f,
            maxDistance: 40f
        );
    }

    public PerkType GetPerkType()
    {
        return perkType;
    }

    public int GetPerkPrice()
    {
        return cost;
    }
    
    
}
