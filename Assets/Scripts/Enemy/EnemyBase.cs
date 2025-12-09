using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 4 discrete movement speeds for zombies
public enum ZombieSpeedTier
{
    Tier1 = 0, // slowest
    Tier2 = 1,
    Tier3 = 2,
    Tier4 = 3  // fastest
}

public class EnemyBase : LivingEntity
{
    // ─────────────────────────────
    //  BASIC TYPE / WAVE AVAILABILITY
    // ─────────────────────────────
    [Header("Type & Wave")]
    [Tooltip("If true, this enemy uses ranged-attack behaviour (cooldown handled by animator bool 'isCooldown').")]
    [SerializeField] private bool isRange = false;

    [Tooltip("First wave this enemy type can spawn (inclusive).")]
    public int avaible_from_wave = 0;

    [Tooltip("Last wave this enemy type can spawn (inclusive).")]
    public int avaible_to_wave = 100;

    // ─────────────────────────────
    //  MOVEMENT / ROTATION
    // ─────────────────────────────
    [Header("Movement & Rotation")]
    [Tooltip("Current movement speed (set automatically from speed tier).")]
    [SerializeField] private float speed = 1f;

    [Tooltip("Maximum allowed speed for this enemy (tier speeds are clamped to this).")]
    [SerializeField] private float maxspeed = 5f;

    [Tooltip("How fast the NavMeshAgent can rotate.")]
    [SerializeField] private float rotationSpeed = 1200f;

    // ─────────────────────────────
    //  SPEED TIERS (BO2-STYLE)
    // ─────────────────────────────
    [Header("Speed Tiers (round-based)")]
    [Tooltip("Slow shamblers (round 1 baseline).")]
    [SerializeField] private float tier1Speed = 1.0f;

    [Tooltip("Fast walk / light run.")]
    [SerializeField] private float tier2Speed = 2.0f;

    [Tooltip("Very fast.")]
    [SerializeField] private float tier3Speed = 3.0f;

    [Tooltip("Sprint / super sprinter.")]
    [SerializeField] private float tier4Speed = 4.0f;

    [Tooltip("Current tier assigned by WaveSystem when spawned.")]
    [SerializeField] private ZombieSpeedTier currentSpeedTier = ZombieSpeedTier.Tier1;

    // ─────────────────────────────
    //  LEGACY COMBAT STATS (NOT USED ANYMORE)
    //  Kept to avoid breaking old prefabs / code, but hidden from Inspector.
    // ─────────────────────────────
    [HideInInspector, SerializeField] private float attack = 1f;
    [HideInInspector, SerializeField] private float maxattack = 3f;
    [HideInInspector, SerializeField] private float startAttack = 1f;
    [HideInInspector] public float multiplier = 1f;   // WaveSystem still assigns this, but health now uses BO2 formula.

    // ─────────────────────────────
    //  AI / TARGETING
    // ─────────────────────────────
    [Header("AI Targeting")]
    [Tooltip("How often (seconds) this enemy re-evaluates the closest valid player.")]
    [SerializeField] private float checkInterval = 0.4f;

    private float checkTimer = 0f;
    private Player targetPlayer;
    private IEnemyAttack attackScript;
    public bool isAttacking { get; set; }
    private float cooldownTimer = 0f;

    // ─────────────────────────────
    //  DEATH / FX
    // ─────────────────────────────
    [Header("Death / FX")]
    [Tooltip("Delay before starting dissolve after death.")]
    [SerializeField] private float deadStateTimer = 5f;

    [Tooltip("Delay after dissolve before destroying the GameObject.")]
    [SerializeField] private float timeToDestroyAfterDissolve = 3f;

    // ─────────────────────────────
    //  RUNTIME COMPONENTS / STATE
    // ─────────────────────────────
    private NavMeshAgent agent;
    public bool isDead = false;
    private Animator animator;
    private Ragdoll ragdoll;

    public override void SetLivingEntity() { }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.angularSpeed = rotationSpeed;
        }

        attackScript = GetComponent<IEnemyAttack>();
        animator = GetComponent<Animator>();
        ragdoll = GetComponent<Ragdoll>();

        isAttacking = false;

        // Ensure we start with Tier1 speed
        ApplySpeedForCurrentTier();
    }

    private void Update()
    {
        // Ranged enemies use animator "isCooldown" gate
        if (isRange && animator != null && animator.GetBool("isCooldown")) return;
        if (isDead || isAttacking) return;

        // If current target becomes invalid (downed/dead/missing), drop it
        if (targetPlayer != null)
        {
            var ph = targetPlayer.health;
            if (ph == null || !ph.CanBeTargeted)
            {
                targetPlayer = null;
            }
        }

        // Periodically retarget to closest valid player
        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f || targetPlayer == null)
        {
            targetPlayer = GetClosestTargetablePlayer();
            SpeedUPWhenFarFromPlayer();
            checkTimer = checkInterval;
        }

        if (targetPlayer == null)
        {
            StopMoving();
            return;
        }

        if (cooldownTimer > 0 && !isRange)
        {
            cooldownTimer -= Time.deltaTime;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);

        if (distanceToPlayer <= attackScript.AttackRange && cooldownTimer <= 0)
        {
            StartCoroutine(AttackSequence());
        }
        else if (distanceToPlayer <= attackScript.AttackRange)
        {
            StopMoving();
        }
        else
        {
            MoveTowardsPlayer();
        }
    }

    /// <summary>
    /// Called from WaveSystem after multiplier has been set.
    /// Health uses BO2-style curve; damage does NOT scale per round.
    /// Speed is handled separately via speed tiers.
    /// </summary>
    public void AdjustEnemyToWave()
    {
        int round = (WaveSystem.instance != null)
            ? WaveSystem.instance.currentWave
            : 1;

        // HEALTH (BO2 style)
        float baseFormulaHp = WaveSystem.GetZombieHealthForRound(round);
        float typeFactor = startHealth / 150f;   // 150 => exactly BO2 curve
        float finalHp = baseFormulaHp * typeFactor;

        maxHealth = finalHp;
        currentHealth = finalHp;

        // Legacy attack value is kept in sync but not actually used by MeleeAttack
        attack = startAttack;
        Set_Attack(startAttack);

        if (agent != null)
        {
            agent.speed = speed;
        }
    }

    /// <summary>
    /// Called by WaveSystem for each spawned enemy to give it a speed tier.
    /// </summary>
    public void SetSpeedTier(ZombieSpeedTier tier)
    {
        currentSpeedTier = tier;
        ApplySpeedForCurrentTier();
    }

    private void ApplySpeedForCurrentTier()
    {
        float targetSpeed = tier1Speed;

        switch (currentSpeedTier)
        {
            case ZombieSpeedTier.Tier2: targetSpeed = tier2Speed; break;
            case ZombieSpeedTier.Tier3: targetSpeed = tier3Speed; break;
            case ZombieSpeedTier.Tier4: targetSpeed = tier4Speed; break;
        }

        // clamp to maxspeed just in case
        speed = Mathf.Clamp(targetSpeed, 0f, maxspeed);

        if (agent != null)
            agent.speed = speed;
    }

    private void MoveTowardsPlayer()
    {
        if (agent != null && agent.enabled && targetPlayer != null)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPlayer.transform.position);
        }
    }

    private void StopMoving()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
        }
    }

    private IEnumerator AttackSequence()
    {
        if (targetPlayer == null)
            yield break;

        isAttacking = true;
        StopMoving();

        Quaternion lookRotation = Quaternion.LookRotation(targetPlayer.transform.position - transform.position);
        float time = 0;

        while (time < 0.2f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time * 5f);
            time += Time.deltaTime;
            yield return null;
        }

        attackScript.ExecuteAttack(targetPlayer.gameObject);

        if (isRange)
        {
            // Ranged enemies manage their own cooldown via animator "isCooldown"
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(attackScript.AttackDuration);
            cooldownTimer = attackScript.AttackCooldown;
            isAttacking = false;
        }
    }

    // Uses PlayerHealth.AllPlayers for better performance than FindGameObjectsWithTag
    private Player GetClosestTargetablePlayer()
    {
        Player closest = null;
        float minSqrDist = Mathf.Infinity;

        foreach (var ph in PlayerHealth.AllPlayers)
        {
            if (ph == null) continue;
            if (!ph.CanBeTargeted) continue;  // skip dead / downed

            float sqr = (ph.transform.position - transform.position).sqrMagnitude;
            if (sqr < minSqrDist)
            {
                minSqrDist = sqr;
                closest = ph.GetComponent<Player>();
            }
        }

        return closest;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // left empty intentionally
    }

    public void TakeDamage(float damage, Player owner)
    {
        if (isDead) return;

        float newHealth = currentHealth - damage;

        // Award hit points to correct player
        if (owner != null && newHealth > 0)
        {
            var stats = owner.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddPoints(10); // hit reward
            }
        }

        Set_Health(newHealth);

        if (currentHealth <= 0)
        {
            Die(owner); // credit kill
        }
    }

    // These "Adjust" / "Set" methods are mostly legacy now, but still used by BO2 HP logic.
    public void Adjust_Attack(float ammount)
    {
        if (ammount >= 0)
        {
            if (attack + ammount <= maxattack)
            {
                attack += ammount;
            }
            else
            {
                attack = maxattack;
            }
        }
        else
        {
            if (attack - ammount > 0)
            {
                attack -= ammount;
            }
            else
            {
                attack = 0;
            }
        }
    }

    public void Adjust_Speed(float ammount)
    {
        if (ammount >= 0)
        {
            if (speed + ammount <= maxspeed)
            {
                speed += ammount;
            }
            else
            {
                speed = maxspeed;
            }
        }
        else
        {
            if (speed - ammount > 0)
            {
                speed -= ammount;
            }
            else
            {
                speed = 0;
            }
        }

        if (agent != null)
        {
            agent.speed = speed;
        }
    }

    public void Set_Health(float count)
    {
        if (count <= maxHealth)
        {
            if (count >= 0)
            {
                currentHealth = count;
            }
            else
            {
                currentHealth = 0;
                // we already handle death elsewhere
            }
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

    public void Set_Attack(float count)
    {
        if (count <= maxattack)
        {
            if (count >= 0)
            {
                attack = count;
            }
            else
            {
                attack = 0;
            }
        }
        else
        {
            attack = maxattack;
        }
    }

    public void Set_Speed(float count)
    {
        if (count <= maxspeed)
        {
            if (count >= 0)
            {
                speed = count;
            }
            else
            {
                speed = 0;
            }
        }
        else
        {
            speed = maxspeed;
        }

        if (agent != null)
        {
            agent.speed = speed;
        }
    }

    public virtual void BulletImpact(Vector3 force, Vector3 hitPoint, Rigidbody rb)
    {
        if (currentHealth <= 0)
            StartCoroutine(DeathImpactCoroutine(force, hitPoint, rb));
    }

    private IEnumerator DeathImpactCoroutine(Vector3 force, Vector3 hitPoint, Rigidbody rb)
    {
        yield return new WaitForSeconds(0f);
        rb.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
    }

    public void Die(Player killer)
    {
        if (isDead) return;
        isDead = true;
        animator.enabled = false;

        if (agent != null)
        {
            agent.isStopped = true;
            //agent.enabled = false;
        }
        
        if (ragdoll != null)
        {
            ragdoll.RagdollActive(true);
            ragdoll.CollidersActive(false);
        }

        StartCoroutine(Die());

        WaveSystem.instance.current_summons_dead++;
        WaveSystem.instance.current_summons_alive--;

        if (killer != null)
        {
            var stats = killer.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddPoints(100); // kill reward
            }
        }
    }

    public override IEnumerator Die()
    {
        yield return new WaitForSeconds(deadStateTimer);

        DissolveFX[] dissolve;
        Transform childComMaterias = transform.GetChild(0);

        dissolve = new DissolveFX[childComMaterias.childCount - 1];

        for (int i = 0; i < dissolve.Length; i++)
        {
            dissolve[i] = childComMaterias.GetChild(i + 1).GetComponent<DissolveFX>();
        }

        foreach (DissolveFX dFX in dissolve)
        {
            dFX.Dissolve();
        }

        Invoke(nameof(DestroyThisGameObject), timeToDestroyAfterDissolve);
    }

    public override void DestroyThisGameObject()
    {
        Destroy(gameObject);
    }

    public Player GetTargetPlayer()
    {
        return targetPlayer;
    }

    public void SpeedUPWhenFarFromPlayer()
    {
        float actualSpeed = tier1Speed;

        switch (currentSpeedTier)
        {
            case ZombieSpeedTier.Tier2: actualSpeed = tier2Speed; break;
            case ZombieSpeedTier.Tier3: actualSpeed = tier3Speed; break;
            case ZombieSpeedTier.Tier4: actualSpeed = tier4Speed; break;
        }


        float newSpeed = actualSpeed * 2f;

        if (targetPlayer != null && (Vector3.Distance(transform.position, targetPlayer.transform.position) > 20f))
        {
            GetComponent<NavMeshAgent>().speed = newSpeed;
        }
        else
        {
            GetComponent<NavMeshAgent>().speed = actualSpeed;
        }
        
    }
}
