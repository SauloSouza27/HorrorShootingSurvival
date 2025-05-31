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

    public override void SetLivingEntity()
    {

    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = speed;
        }
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
        if (Player.instance != null)
        {
            float distance = Vector3.Distance(this.transform.position, Player.instance.transform.position);

            if (distance > 0.9f)
            {
                if (agent != null && agent.enabled)
                {
                    agent.SetDestination(Player.instance.transform.position);
                }
            }
            else
            {
                Die();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Bullet bullet = collision.gameObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                Debug.Log($"{gameObject.name} hit by bullet. Damage: {bullet.bulletDamage}. Health before hit: {currentHealth}");

                TakeDamage(bullet.bulletDamage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        Set_Health(currentHealth - damage);
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

    public override void Die()
    {
        base.Die();
        WaveSystem.instance.current_summons_dead++;
        WaveSystem.instance.current_summons_alive--;
    }
}