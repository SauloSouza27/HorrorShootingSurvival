using UnityEngine;

public class HealthController : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected int maxHealth = 100;
    public int currentHealth;

    public HealthBar healthBar;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    public virtual void ReduceHealth(int damage)
    {
        currentHealth -= damage;
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
