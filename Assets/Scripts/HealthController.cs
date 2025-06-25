using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public HealthBar healthBar;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    public virtual void ReduceHealth()
    {
        currentHealth--;
        healthBar.SetHealth(currentHealth);
    }

    public virtual void IncreaseHealth()
    {
        currentHealth++;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
            
        healthBar.SetHealth(currentHealth);
    }
    
    public bool ShouldDie() => currentHealth <= 0;
}
