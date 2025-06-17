using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public virtual void ReduceHealth()
    {
        currentHealth--;
    }

    public virtual void IncreaseHealth()
    {
        currentHealth++;
        
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }
    
    public bool ShouldDie() => currentHealth <= 0;
}
