using UnityEngine;

public class PlayerHealth : HealthController
{
    private Player player;
    public GameObject defeatScreen;
    public bool isDead { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
    }

    protected void Start()
    {
        // ensure maxHealth matches PlayerStats
        var stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            maxHealth = stats.MaxHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);

            // subscribe to stats changes so HP updates if perk changes max health
            stats.OnStatsChanged += OnPlayerStatsChanged;
        }
    }

    private void OnDestroy()
    {
        var stats = GetComponent<PlayerStats>();
        if (stats != null)
            stats.OnStatsChanged -= OnPlayerStatsChanged;
    }

    private void OnPlayerStatsChanged()
    {
        var stats = GetComponent<PlayerStats>();
        if (stats == null) return;
        SetMaxHealth(stats.MaxHealth, healToFull: false);
    }

    // Expose a method to adjust max health from PlayerStats
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

    public override void ReduceHealth()
    {
        base.ReduceHealth();

        if (ShouldDie())
            Die();
    }

    private void Die()
    {
        isDead = true;
        player.animator.enabled = false;
        player.ragdoll.RagdollActive(true);
        defeatScreen.SetActive(true);
    }
}