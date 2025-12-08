using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LivingEntity : MonoBehaviour
{
    [Header("Health Settings")]
    public float startHealth = 100;
    public float maxHealth = 100;
    [Header("Tipo Inimigo")]

    [HideInInspector]
    public float currentHealth = 100;

    public void Start()
    {
        SetLivingEntity();
    }

    public virtual void SetLivingEntity()
    {
        currentHealth = startHealth;
    }

    public void Adjust_Health(float ammount)
    {
        if (ammount >= 0)
        {
            if (currentHealth + ammount <= maxHealth)
            {
                currentHealth += ammount;
            }
            else
            {
                currentHealth = maxHealth;
            }
        }
        else
        {
            if (currentHealth - ammount > 0)
            {
                currentHealth -= ammount;
            }
            else
            {
                currentHealth = 0;
                //Die();
            }
        }
    }

    public virtual IEnumerator Die()
    {
        yield return new WaitForSeconds(3f);
    }

    public virtual void DestroyThisGameObject()
    {
        Destroy(gameObject);
    }
}
