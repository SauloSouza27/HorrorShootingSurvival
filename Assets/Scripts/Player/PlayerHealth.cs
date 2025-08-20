using UnityEngine;

public class PlayerHealth: HealthController
{
    private Player player;
    public GameObject defeatScreen;
    
    public bool isDead { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }

    protected void Start() // ⬆️ add override
    {

        var stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            maxHealth = stats.MaxHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);

            stats.OnStatsChanged += OnPlayerStatsChanged; // ⬆️ subscribe
        }
    }

    private void OnDestroy()
    {
        var stats = GetComponent<PlayerStats>();
        if (stats != null)
            stats.OnStatsChanged -= OnPlayerStatsChanged; // ⬆️ unsubscribe
    }

    private void OnPlayerStatsChanged()
    {
        var stats = GetComponent<PlayerStats>();
        if (stats == null) return;
        SetMaxHealth(stats.MaxHealth, healToFull: false);
    }

// ⬆️ new method
    public void SetMaxHealth(int newMax, bool healToFull = false)
    {
        maxHealth = newMax;
        if (healToFull)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Min(currentHealth, maxHealth);

        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(currentHealth);
    }
}
