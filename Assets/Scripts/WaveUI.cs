using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    public TextMeshProUGUI WaveCountText;
    public TextMeshProUGUI SummonsText;

    [SerializeField] private Color colorEffect = Color.red;
    [SerializeField] private float speedRedEffect = 1f;
    [SerializeField] private float effectDuration = 5f;

    private float timer;


    private Color originalColor;
    private int originalCurrentWave;
    private bool isBlinking = false;

    private void Start()
    {
        if (originalColor != null) originalColor = WaveCountText.color;
        originalCurrentWave = WaveSystem.instance.currentWave;

        timer = effectDuration;
    }
    // Update is called once per frame
    void Update()
    {
        UpdateUI();
        EfeitoBlinkWaveNumber(WaveSystem.instance.currentWave);
    }
    public void UpdateUI()
    {
        WaveCountText.text = "" + WaveSystem.instance.currentWave;
        SummonsText.text = "Enemies " + WaveSystem.instance.current_summons + " / " + WaveSystem.instance.current_summons_alive;
    }

    private void EfeitoBlinkWaveNumber(int currentWave)
    {
        if (originalCurrentWave != currentWave)
        {
            isBlinking = true;
            timer = effectDuration;
            originalCurrentWave = currentWave;
        }

        if (isBlinking)
        {
            WaveCountText.color = BlinkRed(originalColor, speedRedEffect);
        }

        else
        {
            WaveCountText.color = originalColor;
        }
    }

    private Color BlinkRed(Color baseColor, float speed)
    {
        // valor senoidal entre 0 e 1
        float t = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;

        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            // interpola entre a cor original e o vermelho
            return Color.Lerp(baseColor, colorEffect, t);
        }
        if (timer <= 0f)
        {
            isBlinking = false;
        }

        return originalColor;

    }
}
