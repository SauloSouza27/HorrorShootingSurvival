using UnityEngine;

public class UIScoreDisplay : MonoBehaviour
{
    public TMPro.TextMeshProUGUI scoreText; // Assign in Inspector
    void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
        }
    }
    void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        }
    }

    void Start()
    {
        // Initial update when the UI element starts
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay(ScoreManager.Instance.GetCurrentScore());
        }
    }
    
    void UpdateScoreDisplay(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore}";
        }
    }
}
