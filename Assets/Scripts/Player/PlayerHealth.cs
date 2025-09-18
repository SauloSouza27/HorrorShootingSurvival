using UnityEngine;
using System.Collections;

public class PlayerHealth : HealthController
{
    private Player player;
    public GameObject defeatScreen;

    public bool isDead { get; private set; }
    public bool isDowned { get; private set; } // ⬅️ new

    [Header("Downed/Revive")]
    [SerializeField] private float bleedoutTime = 25f; // seconds before full death
    [SerializeField] private int reviveRestoreHealth = 50; // hp after revive
    private Coroutine bleedoutRoutine;

    private ReviveTarget reviveTarget; // ⬅️ new (added at runtime or via prefab)

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
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
        reviveTarget.Init(this);              // wire back to us
        reviveTarget.enabled = false;         // off until downed
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

    public void SetMaxHealth(int newMax, bool healToFull = false)
    {
        maxHealth = newMax;
        currentHealth = healToFull ? maxHealth : Mathf.Min(currentHealth, maxHealth);
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(currentHealth);
    }

    public override void ReduceHealth()
    {
        // ignore damage when already dead/downed bleeding out is handled by timer
        if (isDead) return;

        base.ReduceHealth();

        if (ShouldDie())
        {
            if (!isDowned)
                EnterDownedState();
            //else
                //BleedOut(); // took damage while downed or re-hit at 0 (optional)
        }
    }

    private void EnterDownedState()
    {
        isDowned = true;

        // disable active gameplay systems
        player.animator.SetBool("isDowned", true);
        player.animator.enabled = true;                // use a downed anim instead of ragdoll
        player.ragdoll.RagdollActive(false);           // keep kinematic so we can be revived
        player.weapon.SetWeaponReady(false);

        // prevent movement & actions at source (quick approach)
        GetComponent<CharacterController>().enabled = false;

        // enable revive target
        reviveTarget.enabled = true;
        reviveTarget.BeginWaitingForRevive();

        // start bleedout timer
        if (bleedoutRoutine != null) StopCoroutine(bleedoutRoutine);
        bleedoutRoutine = StartCoroutine(BleedoutTimer());
    }

    private IEnumerator BleedoutTimer()
    {
        float t = bleedoutTime;
        while (t > 0f && isDowned && !isDead)
        {
            t -= Time.deltaTime;
            //reviveTarget?.UpdateBleedoutUI(t / bleedoutTime); // optional UI hook
            yield return null;
        }

        if (isDowned && !isDead)
            BleedOut();
    }

    private void BleedOut()
    {
        // fully die (game over handling stays as before)
        isDowned = false;
        Die();
    }

    // called by ReviveTarget when a teammate completes revive
    public void CompleteRevive()
    {
        if (isDead) return;

        isDowned = false;

        // restore systems
        GetComponent<CharacterController>().enabled = true;
        player.animator.SetBool("isDowned", false);
        player.ragdoll.RagdollActive(false);
        player.weapon.SetWeaponReady(true);
        reviveTarget.enabled = false;
        if (bleedoutRoutine != null) StopCoroutine(bleedoutRoutine);

        // restore some health
        currentHealth = Mathf.Clamp(reviveRestoreHealth, 1, maxHealth);
        healthBar.SetHealth(currentHealth);
    }

    private void Die()
    {
        isDead = true;

        // disable revive possibility
        if (reviveTarget != null) reviveTarget.enabled = false;
        if (bleedoutRoutine != null) StopCoroutine(bleedoutRoutine);

        // your old death flow
        player.animator.enabled = false;
        player.ragdoll.RagdollActive(true);
        defeatScreen.SetActive(true);
    }
}
