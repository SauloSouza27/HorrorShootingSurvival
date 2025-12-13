using TMPro;
using UnityEngine;

public class ScoreCount : MonoBehaviour
{
    [Header("Main score")]
    public TextMeshProUGUI scoreText;

    [Header("Delta popup")]
    [SerializeField] private TextMeshProUGUI deltaText;
    [SerializeField] private float floatDistance = 40f;   // how far it moves
    [SerializeField] private float floatDuration = 0.6f;  // how long it lasts
    [SerializeField] private Color gainColor = new Color(0.3f, 1f, 0.3f, 1f);
    [SerializeField] private Color lossColor = new Color(1f, 0.3f, 0.3f, 1f);

    private RectTransform deltaRect;
    private Vector2 deltaStartAnchoredPos;
    private Coroutine popupRoutine;

    private void Awake()
    {
        if (deltaText != null)
        {
            deltaRect = deltaText.rectTransform;
            deltaStartAnchoredPos = deltaRect.anchoredPosition;
            deltaText.gameObject.SetActive(false);
        }
    }

    public void UpdateScore(int currentScore)
    {
        if (scoreText != null)
            scoreText.text = currentScore.ToString();
    }

    /// <summary>
    /// Shows a small floating "+X" or "-X" near the score.
    /// Positive values float up, negative float down.
    /// </summary>
    public void ShowDelta(int delta)
    {
        if (deltaText == null || delta == 0)
            return;

        if (popupRoutine != null)
            StopCoroutine(popupRoutine);

        popupRoutine = StartCoroutine(DeltaRoutine(delta));
    }

    private System.Collections.IEnumerator DeltaRoutine(int delta)
    {
        deltaText.gameObject.SetActive(true);
        deltaRect.anchoredPosition = deltaStartAnchoredPos;

        // Text & color
        string sign = delta > 0 ? "+" : "-";
        deltaText.text = sign + Mathf.Abs(delta).ToString();
        deltaText.color = delta > 0 ? gainColor : lossColor;

        float dir = delta > 0 ? 1f : -1f;
        float t = 0f;

        Color startColor = deltaText.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (t < floatDuration)
        {
            t += Time.deltaTime;
            float normalized = t / floatDuration;

            float yOffset = dir * floatDistance * normalized;
            deltaRect.anchoredPosition = deltaStartAnchoredPos + new Vector2(0f, yOffset);

            deltaText.color = Color.Lerp(startColor, endColor, normalized);

            yield return null;
        }

        deltaText.gameObject.SetActive(false);
    }
}
