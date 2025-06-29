using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ScoreCount : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    public void UpdateScore(int currentScore)
    {
        scoreText.text = "Score: " + currentScore;
        
    }
    
}
