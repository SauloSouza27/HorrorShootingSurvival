
using UnityEngine;
using System; 

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [SerializeField] private int currentScore = 0;
    
    private const int BULLET_HIT_POINTS = 10;
    
    private const int KILL_POINTS = 80;

    // Event that can be subscribed to by UI elements to update the score display
    public event Action<int> OnScoreChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddBulletHitPoints()
    {
        currentScore += BULLET_HIT_POINTS;
        Debug.Log($"Bullet hit! Current Score: {currentScore}");
        // Invoke the event to notify subscribers about the score change
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public void AddKillPoints()
    {
        currentScore += KILL_POINTS;
        Debug.Log($"Enemy killed! Current Score: {currentScore}");
        // Invoke the event to notify subscribers about the score change
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        Debug.Log("Score reset to 0.");
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }

    
}
