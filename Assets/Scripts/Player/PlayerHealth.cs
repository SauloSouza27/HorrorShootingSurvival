using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : HealthController
{
    //  CHEATS
    public static bool InfiniteHealthCheat = false;
    
    private Player player;
    public GameObject defeatScreen;

    public bool isDead { get; private set; }
    public bool isDowned { get; private set; }

    private PlayerStats stats;  

    [Header("Downed/Revive")]
    [SerializeField] private float bleedoutTime = 25f;
    [SerializeField] private int reviveRestoreHealth = 50;
    private Coroutine bleedoutRoutine;

    // normalized remaining bleedout time (1 → just downed, 0 → about to bleed out)
    private float bleedoutRemaining01 = 0f;
    public float BleedoutRemaining01 => bleedoutRemaining01;

    // NEW: pause bleedout while being revived
    private bool isBeingRevived = false;
    public bool IsBeingRevived => isBeingRevived;

    public void SetBeingRevived(bool value)
    {
        isBeingRevived = value;
    }

    [Header("Regeneration")]
    [SerializeField] private float baseRegenDelay = 6f;      // time without damage before regen starts
    [SerializeField] private float regenFullTime = 3f;       // seconds to go from 0 → full
    private float lastDamageTime = -999f;
    private float regenAccumulator = 0f;                     // fractional HP buffer

    private ReviveTarget reviveTarget;
    
    [Header("Hit Feedback")]
    [SerializeField] private AudioClip hitSFX;
    [Range(0f, 1f)] [SerializeField] private float hitVolume = 1f;
    [SerializeField] private float hitSFXMinDistance = 4f;
    [SerializeField] private float hitSFXMaxDistance = 40f;
    [SerializeField] private string hitAnimTrigger = "Hit";

    // ========= TEAM-WIDE TRACKING =========
    private static readonly List<PlayerHealth> allPlayers = new List<PlayerHealth>();
    private static bool matchOver = false;
    // =====================================

    public static readonly System.Collections.Generic.List<PlayerHealth> AllPlayers 
        = new System.Collections.Generic.List<PlayerHealth>();

    public bool CanBeTargeted => !isDead && !isDowned;
    
    private PlayerWeaponVisuals visualController;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
        visualController = GetComponentInParent<PlayerWeaponVisuals>();

        if (!allPlayers.Contains(this))
            allPlayers.Add(this);

        matchOver = false;
    }

    protected void Start()
    {
        stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            maxHealth = stats.MaxHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            healthBar.SetMaxHealth(maxHealth);
            healthBar.SetHealth(currentHealth);
            stats.OnStatsChanged += OnPlayerStatsChanged;
        }

        reviveTarget = GetComponent<ReviveTarget>();
        if (reviveTarget == null)
            reviveTarget = gameObject.AddComponent<ReviveTarget>();

        reviveTarget.Init(this);
        reviveTarget.enabled = false;
    }

    private void OnDestroy()
    {
        if (stats != null)
            stats.OnStatsChanged -= OnPlayerStatsChanged;

        allPlayers.Remove(this);
    }

    private void OnPlayerStatsChanged()
    {
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

    private void PlayHitFeedback()
    {
        if (player != null && player.animator != null && !string.IsNullOrEmpty(hitAnimTrigger))
        {
            player.animator.SetTrigger(hitAnimTrigger);
        }

        if (AudioManager.Instance != null && hitSFX != null)
        {
            Vector3 pos = transform.position + Vector3.up * 1.5f;
            AudioManager.Instance.PlaySFX3D(
                hitSFX,
                pos,
                hitVolume,
                spatialBlend: 1f,
                minDistance: hitSFXMinDistance,
                maxDistance: hitSFXMaxDistance
            );
        }
    }

    public override void ReduceHealth(int damage)
    {
        if (InfiniteHealthCheat) return;      
        if (isDead || matchOver) return;
        if (damage <= 0) return;

        base.ReduceHealth(damage);

        lastDamageTime = Time.time;

        bool shouldDie = ShouldDie();

        if (!shouldDie && !isDowned)
            PlayHitFeedback();

        if (shouldDie)
        {
            if (!isDowned)
                EnterDownedState();
        }
    }


    private void EnterDownedState()
    {
        if (isDowned || isDead || matchOver) return;

        isDowned = true;
        isBeingRevived = false;
        bleedoutRemaining01 = 1f; // just started bleeding out

        visualController.ReduceRigWeight();
        visualController.SwitchOffAnimationLayer();
        visualController.SwitchOffWeaponModels();
        player.animator.SetBool("isDowned", true);
        player.animator.enabled = true;
        player.ragdoll.RagdollActive(false);
        player.weapon.SetWeaponReady(false);

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        reviveTarget.enabled = true;
        reviveTarget.BeginWaitingForRevive();

        if (bleedoutRoutine != null) StopCoroutine(bleedoutRoutine);
        bleedoutRoutine = StartCoroutine(BleedoutTimer());

        CheckForTeamWipe();
    }

    private IEnumerator BleedoutTimer()
    {
        float t = bleedoutTime;
        bleedoutRemaining01 = 1f;

        // While downed and not dead and match still going
        while (t > 0f && isDowned && !isDead && !matchOver)
        {
            // ⬇️ ONLY tick down while not being revived
            if (!isBeingRevived)
            {
                t -= Time.deltaTime;
                bleedoutRemaining01 = Mathf.Clamp01(t / bleedoutTime);
            }

            yield return null;
        }

        // timer finished or interrupted
        if (t <= 0f)
            bleedoutRemaining01 = 0f;

        // only bleed out if timer actually ran out and we’re still downed
        if (t <= 0f && isDowned && !isDead && !matchOver)
            BleedOut();
    }

    private void BleedOut()
    {
        isDowned = false;
        isBeingRevived = false;
        bleedoutRemaining01 = 0f;
        Die();
    }

    public void CompleteRevive()
    {
        if (isDead || matchOver) return;

        isDowned = false;
        isBeingRevived = false;

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;
        player.animator.SetBool("isDowned", false);
        visualController.MaximizeRigWeight();
        visualController.SwitchOnCurrentWeaponModel();
        visualController.SwitchOnCurrentWeaponModel();
        player.ragdoll.RagdollActive(false);
        player.weapon.SetWeaponReady(true);

        reviveTarget.enabled = false;
        if (bleedoutRoutine != null) StopCoroutine(bleedoutRoutine);

        currentHealth = Mathf.Clamp(reviveRestoreHealth, 1, maxHealth);
        healthBar.SetHealth(currentHealth);

        lastDamageTime = Time.time;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        isDowned = false;
        isBeingRevived = false;

        if (reviveTarget != null) reviveTarget.enabled = false;
        if (bleedoutRoutine != null) StopCoroutine(bleedoutRoutine);

        player.animator.enabled = false;
        player.ragdoll.RagdollActive(true);

        CheckForTeamWipe();
        
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

        matchOver = true;

        foreach (var ph in allPlayers)
        {
            if (ph != null && ph.defeatScreen != null)
            {
                ph.defeatScreen.SetActive(true);
                break;
            }
        }
    }

    // ================== RESPAWN FOR NEW WAVE ==================
    public static void RespawnAllForNewWave(int minPoints)
    {
        if (matchOver) return;

        foreach (var ph in allPlayers)
        {
            if (ph == null) continue;
            if (!ph.isDead) 
                continue;

            ph.RespawnInternal(minPoints);
        }
    }

    private void RespawnInternal(int minPoints)
    {
        if (bleedoutRoutine != null)
        {
            StopCoroutine(bleedoutRoutine);
            bleedoutRoutine = null;
        }

        isDead = false;
        isDowned = false;
        isBeingRevived = false;
        bleedoutRemaining01 = 0f;

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = true;

        player.ragdoll.RagdollActive(false);
        player.animator.enabled = true;
        player.animator.SetBool("isDowned", false);

        if (reviveTarget != null)
            reviveTarget.enabled = false;

        currentHealth = maxHealth;
        healthBar.SetHealth(currentHealth);

        player.weapon.SetWeaponReady(true);

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

        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.AddTarget(transform, 1f, 0f);
        }

        Debug.Log($"{name} respawned for wave {WaveSystem.instance.currentWave} with at least {minPoints} points.");
    }
    
    private void Update()
    {
        HandleHealthRegeneration();
    }

    private void HandleHealthRegeneration()
    {
        if (isDead || isDowned || matchOver) return;
        if (currentHealth >= maxHealth) return;

        float delay = baseRegenDelay;

        if (stats != null && stats.HasPerk(PerkType.QuickRevive))
        {
            delay *= 0.5f;
        }

        if (Time.time < lastDamageTime + delay)
        {
            regenAccumulator = 0f;
            return;
        }

        float hpPerSecond = maxHealth / Mathf.Max(0.01f, regenFullTime);

        regenAccumulator += hpPerSecond * Time.deltaTime;

        int heal = Mathf.FloorToInt(regenAccumulator);
        if (heal <= 0) return;

        regenAccumulator -= heal;

        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + heal);

        if (currentHealth != oldHealth)
        {
            healthBar.SetHealth(currentHealth);
        }
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
