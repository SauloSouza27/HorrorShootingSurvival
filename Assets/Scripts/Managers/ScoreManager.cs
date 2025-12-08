
using UnityEngine;
using System;
using System.Collections.Generic;


public class ScoreManager : MonoBehaviour
{
    
    public static ScoreManager Instance { get; private set; }
    
    [SerializeField] private int currentScore = 0;

    [SerializeField] private const int BULLET_HIT_POINTS = 10;

    [SerializeField] private const int KILL_POINTS = 80;
    
    private Dictionary<int, int> playerScores = new Dictionary<int, int>(); // replace currentScore

    public event Action<int,int> OnPlayerScoreChanged; // (playerIndex, newScore)

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
    
    public void RegisterPlayer(int playerIndex, int initialScore = 0) // new
    {
        playerScores[playerIndex] = initialScore;
        OnPlayerScoreChanged?.Invoke(playerIndex, initialScore);
    }

    public void AddScoreForPlayer(int playerIndex, int amount) // new
    {
        if (!playerScores.ContainsKey(playerIndex)) playerScores[playerIndex] = 0;
        playerScores[playerIndex] += amount;
        OnPlayerScoreChanged?.Invoke(playerIndex, playerScores[playerIndex]);
    }
    
    public void AddBulletHitPoints(int playerIndex)
    {
        AddScoreForPlayer(playerIndex, 10);
    }

    public void AddKillPoints(int playerIndex)
    {
        AddScoreForPlayer(playerIndex, 80);
    }

    
    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    public int GetCurrentScore()
    {
        return currentScore;
    }

    
}
