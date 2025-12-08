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
    [Header("Enemy Settings")]
    [SerializeField] private float speed = 1;
    [SerializeField] private float rotationSpeed = 1200;
    [SerializeField] private float attack = 1;
    [SerializeField] private float maxspeed = 5;
    //[SerializeField] private float startSpeed = 1;
    [SerializeField] private float maxattack = 3;
    [SerializeField] private float startAttack = 1;
    public float multiplier = 1;   // still used for health scaling if you want
    [SerializeField] private bool isRange = false;

    [Header("Wave Data")]
    public int avaible_from_wave = 0;
    public int avaible_to_wave = 100;

    [Header("Speed Tiers (round-based)")]
    [Tooltip("Slow shamblers (round 1 baseline).")]
    [SerializeField] private float tier1Speed = 1.0f;
    [Tooltip("Fast walk / light run.")]
    [SerializeField] private float tier2Speed = 2.0f;
    [Tooltip("Very fast.")]
    [SerializeField] private float tier3Speed = 3.0f;
    [Tooltip("Sprint / super sprinter.")]
    [SerializeField] private float tier4Speed = 4.0f;

    [SerializeField] private ZombieSpeedTier currentSpeedTier = ZombieSpeedTier.Tier1;

    private NavMeshAgent agent;

    private bool isDead = false;

    // 🔹 Now store Player instead of GameObject
    private Player targetPlayer;
    private float checkInterval = 0.4f;
    private float checkTimer = 0f;

    private IEnemyAttack attackScript;
    public bool isAttacking { get; set; }
    private float cooldownTimer = 0f;

    private Animator animator;
    private Ragdoll ragdoll;
    [SerializeField] private float deadStateTimer = 5f;
    [SerializeField] private float timeToDestroyAfterDissolve = 3f;
    
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

    /// <summary>
    /// Called from WaveSystem after multiplier has been set.
    /// Health may scale with multiplier, damage does NOT scale with round.
    /// Speed is handled separately via speed tiers.
    /// </summary>
    public void AdjustEnemyToWave()
    {
        // What round are we on?
        int round = (WaveSystem.instance != null) 
            ? WaveSystem.instance.currentWave 
            : 1;

        // --- HEALTH (BO2 style) ---
        // Base BO2 formula gives 150 HP at round 1 for a "normal" zombie.
        // We use startHealth as a type-multiplier, so:
        //   startHealth = 150 → exactly the BO2 curve
        //   startHealth = 300 → exactly 2x the BO2 curve, etc.
        float baseFormulaHp = WaveSystem.GetZombieHealthForRound(round);
        float typeFactor     = startHealth / 150f;
        float finalHp        = baseFormulaHp * typeFactor;

        maxHealth     = finalHp;
        currentHealth = finalHp;
        
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

    private void Update()
    {
        if (isRange == true && animator.GetBool("isCooldown")) return;
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
            checkTimer = checkInterval;
        }

        if (targetPlayer == null)
        {
            StopMoving();
            return;
        }

        if (cooldownTimer > 0 && isRange == false)
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
            agent.isStopped = true;

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
        if (targetPlayer != null)
        {
            return targetPlayer;
        }
        else return null;
    }
}
