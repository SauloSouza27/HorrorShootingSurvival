using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 
using UnityEngine.AI; 

public class EnemyBase : LivingEntity
{
    [Header("Enemy Settings")]
    [SerializeField] private float speed = 1;
    [SerializeField] private float rotationSpeed = 1200;
    [SerializeField] private float attack = 1;
    [SerializeField] private float maxspeed = 5;
    [SerializeField] private float startSpeed = 1;
    [SerializeField] private float maxattack = 3;
    [SerializeField] private float startAttack = 1;
    public float multiplier = 1;

    [Header("Wave Data")]
    public int avaible_from_wave = 0;
    public int avaible_to_wave = 100;

    private NavMeshAgent agent;

    private bool isDead = false;

    private GameObject targetPlayer;
    private float checkInterval = 0.5f;
    private float checkTimer = 0f;

    private IEnemyAttack attackScript; 
    private bool isAttacking = false;
    private float cooldownTimer = 0f;

    private Animator animator;
    private Ragdoll ragdoll;
    [SerializeField] private float deadStateTimer = 5f;
    
    

    public override void SetLivingEntity()
    {
     
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = speed;
            agent.angularSpeed = rotationSpeed;
        }

        attackScript = GetComponent<IEnemyAttack>();
        animator = GetComponent<Animator>();
        ragdoll = GetComponent<Ragdoll>();
    }

    public void AdjustEnemyToWave()
    {
        currentHealth = startHealth;
        attack = startAttack;
        speed = startSpeed;

        Set_Health(startHealth * multiplier);
        Set_Attack(startAttack * multiplier);
        Set_Speed(startSpeed * multiplier);

        if (agent != null)
        {
            agent.speed = speed;
        }
    }

    public void Update()
    {
        if (isDead) 
            deadStateTimer -= Time.deltaTime;
        
        if (isDead || isAttacking) return; 

        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            targetPlayer = GetClosestPlayer();
            checkTimer = checkInterval;
        }

        if (targetPlayer == null)
        {
            StopMoving();
            return;
        }

        if (cooldownTimer > 0)
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
        if (agent != null && agent.enabled)
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

        attackScript.ExecuteAttack(targetPlayer);

        yield return new WaitForSeconds(attackScript.AttackDuration);

        cooldownTimer = attackScript.AttackCooldown;

        isAttacking = false;
    }
    private GameObject GetClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return null;

        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = player;
            }
        }

        return closest;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        
    }


    public void TakeDamage(float damage, Player owner)
    {
        if (isDead) return;

        float newHealth = currentHealth - damage;

        // ✅ Award hit points to correct player
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
                Die(); 
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

    public void Die(Player killer)
    {
        if (isDead) return;
        isDead = true;
        animator.enabled = false;
        agent.isStopped = true;
        ragdoll.RagdollActive(true);
        
        if(deadStateTimer < 0)
            base.Die();

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
    
    public override void Die()
    {
        Die(null); // default if no killer info
    }
    


    //IDamageable damageable = targetPlayer.gameObject.GetComponent<IDamageable>();
    //damageable?.TakeDamage();
}
