using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : HealthController
{
    private Player player;
    public GameObject defeatScreen;

    public bool isDead { get; private set; }
    public bool isDowned { get; private set; }

    [Header("Downed/Revive")]
    [SerializeField] private float bleedoutTime = 25f;
    [SerializeField] private int reviveRestoreHealth = 50;
    private Coroutine bleedoutRoutine;

    private ReviveTarget reviveTarget;

    // ========= TEAM-WIDE TRACKING =========
    private static readonly List<PlayerHealth> allPlayers = new List<PlayerHealth>();
    private static bool matchOver = false;
    // =====================================
    
    public static readonly System.Collections.Generic.List<PlayerHealth> AllPlayers 
        = new System.Collections.Generic.List<PlayerHealth>();

    public bool CanBeTargeted => !isDead && !isDowned;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();

        // register in static list
        if (!allPlayers.Contains(this))
            allPlayers.Add(this);

        // assume new match when first players appear
        matchOver = false;
    }

    protected void Start()
    {
        var stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            maxHealth = stats.MaxHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
            stats.OnStatsChanged += OnPlayerStatsChanged;
        }

        // Ensure a revive target exists but is disabled by default
        reviveTarget = GetComponent<ReviveTarget>();
        if (reviveTarget == null)
            reviveTarget = gameObject.AddComponent<ReviveTarget>();

        reviveTarget.Init(this);
        reviveTarget.enabled = false;
    }

    private void OnDestroy()
    {
        var stats = GetComponent<PlayerStats>();
        if (stats != null)
            stats.OnStatsChanged -= OnPlayerStatsChanged;

        allPlayers.Remove(this);
    }

    private void OnPlayerStatsChanged()
    {
        var stats = GetComponent<PlayerStats>();
        if (stats == null) return;
        SetMaxHealth(stats.MaxHealth, healToFull: false);
    }

    public void SetMaxHealth(int newMax, bool healToFull = false)
    {
        maxHealth = newMax;
        currentHealth = healToFull ? maxHealth : Mathf.Min(currentHealth, maxHealth);
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(currentHealth);
    }

    public override void ReduceHealth(int damage)
    {
        if (isDead || matchOver) return;

        base.ReduceHealth(damage);

        if (ShouldDie())
        {
            if (!isDowned)
                EnterDownedState();
            // else: you could call BleedOut() here if you want re-hits to finish them
        }
    }

    private void EnterDownedState()
    {
        if (isDowned || isDead || matchOver) return;

        isDowned = true;

        // disable active gameplay systems
        player.animator.SetBool("isDowned", true);
        player.animator.enabled = true;          // use a downed anim instead of ragdoll
        player.ragdoll.RagdollActive(false);     // keep kinematic so we can be revived
        player.weapon.SetWeaponReady(false);

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // enable revive target
        reviveTarget.enabled = true;
        reviveTarget.BeginWaitingForRevive();

        // start bleedout timer
        if (bleedoutRoutine != null) StopCoroutine(bleedoutRoutine);
        bleedoutRoutine = StartCoroutine(BleedoutTimer());

        // check if this down caused a team wipe
        CheckForTeamWipe();
    }

    private IEnumerator BleedoutTimer()
    {
        float t = bleedoutTime;
        while (t > 0f && isDowned && !isDead && !matchOver)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        if (isDowned && !isDead && !matchOver)
            BleedOut();
    }

    private void BleedOut()
    {
        isDowned = false;
        Die();
    }

    public void CompleteRevive()
    {
        if (isDead || matchOver) return;

        isDowned = false;

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        player.animator.SetBool("isDowned", false);
        player.ragdoll.RagdollActive(false);
        player.weapon.SetWeaponReady(true);

        reviveTarget.enabled = false;
        if (bleedoutRoutine != null) StopCoroutine(bleedoutRoutine);

        currentHealth = Mathf.Clamp(reviveRestoreHealth, 1, maxHealth);
        healthBar.SetHealth(currentHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        isDowned = false;

        if (reviveTarget != null) reviveTarget.enabled = false;
        if (bleedoutRoutine != null) StopCoroutine(bleedoutRoutine);

        player.animator.enabled = false;
        player.ragdoll.RagdollActive(true);

        CheckForTeamWipe();
        
        // 🔹 NEW: remove this player from camera target group
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.RemoveTarget(transform);
        }
    }

    // ================== TEAM WIPE LOGIC ==================

    private static bool AnyPlayerCanStillFight()
    {
        foreach (var ph in allPlayers)
        {
            if (ph == null) continue;
            if (!ph.isDead && !ph.isDowned)
                return true;
        }
        return false;
    }

    private static void CheckForTeamWipe()
    {
        if (matchOver) return;

        if (AnyPlayerCanStillFight())
            return;

        // No one left standing → game over
        matchOver = true;

        // Show defeat screen from any player that has it
        foreach (var ph in allPlayers)
        {
            if (ph != null && ph.defeatScreen != null)
            {
                ph.defeatScreen.SetActive(true);
                break;
            }
        }

        // Optional:
        // Time.timeScale = 0f;
        // GameManager.Instance.OnMatchEnded();
    }

    // ================== NEW: RESPAWN FOR NEW WAVE ==================

    /// <summary>
    /// Called by WaveSystem at the start of a new wave.
    /// Respawns any player that is dead OR downed, and ensures
    /// they have at least minPoints points.
    /// </summary>
    public static void RespawnAllForNewWave(int minPoints)
    {
        if (matchOver) return; // don't respawn after a team wipe

        foreach (var ph in allPlayers)
        {
            if (ph == null) continue;

            // Only respawn those who are out
            if (!ph.isDead && !ph.isDowned) 
                continue;

            ph.RespawnInternal(minPoints);
        }
    }

    private void RespawnInternal(int minPoints)
    {
        // Stop any downed logic
        if (bleedoutRoutine != null)
        {
            StopCoroutine(bleedoutRoutine);
            bleedoutRoutine = null;
        }

        isDead = false;
        isDowned = false;

        // restore movement
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        // restore visuals
        player.ragdoll.RagdollActive(false);
        player.animator.enabled = true;
        player.animator.SetBool("isDowned", false);

        // disable revive target
        if (reviveTarget != null)
            reviveTarget.enabled = false;

        // full heal
        currentHealth = maxHealth;
        healthBar.SetHealth(currentHealth);

        // allow shooting again
        player.weapon.SetWeaponReady(true);

        // ensure minimum points
        var stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            int current = stats.GetPoints();
            if (current < minPoints)
            {
                int delta = minPoints - current;
                stats.AddPoints(delta);
            }
        }

        // 🔹 NEW: add back to camera target group
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.AddTarget(transform, 1f, 0f);
        }

        Debug.Log($"{name} respawned for wave {WaveSystem.instance.currentWave} with at least {minPoints} points.");
    }
    
    private void OnEnable()
    {
        if (!AllPlayers.Contains(this))
            AllPlayers.Add(this);
    }

    private void OnDisable()
    {
        AllPlayers.Remove(this);
    }

}
