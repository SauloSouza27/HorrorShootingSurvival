using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDItens : MonoBehaviour
{
    [SerializeField] private Image _healthBar, _pointsIcon;
    public Image healthBar { get; set; }
    public Image pointsIcon { get; set; }

    [Header("Efeitos Barra de vida")]
    [SerializeField] private Slider sliderHealth;
    [SerializeField] private float speedRedEffect = 1f;
    [SerializeField] private Color colorEffect = Color.red;

    private Color originalColor;

    [Header("Efeitos Barra de vida")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 10f;

    private RectTransform playerStats;
    private HorizontalLayoutGroup horizontalLayout;
    private float previousValue;
    private float shakeTime;
    private Vector3 originalPos;
    private float rateToCheckPosition = 2f, timer;
    private bool isShaking = false;


    private void Awake()
    {
        healthBar = _healthBar;
        pointsIcon = _pointsIcon;
    }
    private void Start()
    {
        playerStats = transform.GetComponent<RectTransform>();
        horizontalLayout = transform.GetComponentInParent<HorizontalLayoutGroup>();
        originalPos = new Vector3(playerStats.position.x, playerStats.position.y, 0f);

        if (originalColor != null) originalColor = _healthBar.color;

        if (playerStats != null) originalPos = new Vector3 (playerStats.position.x, playerStats.position.y, 0f);

        previousValue = sliderHealth.value;

        timer = rateToCheckPosition;
    }

    
    private void Update()
    {
        EfeitoBarraVidaLow();

        EfeitoTomaDano(sliderHealth.value);

        ShakeHudPlayer();

        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            return;
        }
        if(timer <= 0f)
        {
            UpdateOriginalPosition();
            timer = rateToCheckPosition;
        }
    }

    private void EfeitoBarraVidaLow()
    {
        float sliderValue = sliderHealth.value;

        if (sliderValue < 50f)
        {
            _healthBar.color = BlinkRed(originalColor, speedRedEffect);
        }

        else
        {
            _healthBar.color = originalColor;
        }
    }

    private Color BlinkRed(Color baseColor, float speed)
    {
        Color cor = new Color(1f, 1f, 1f, 0.5f);

        // valor senoidal entre 0 e 1
        float t = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;

        // interpola entre a cor original e o vermelho
        return Color.Lerp(cor, colorEffect, t);
    }

    private void EfeitoTomaDano(float newValue)
    {
        // se o valor diminuiu, ativa o shake
        if (newValue < previousValue)
        {
            shakeTime = shakeDuration;
            isShaking = true;
        }

        previousValue = newValue;
    }

    private void ShakeHudPlayer()
    {
        if (shakeTime > 0f)
        {
            if (horizontalLayout.enabled)
            {
                horizontalLayout.enabled = false;
            }

            // aplica shake
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            playerStats.position = originalPos + new Vector3(x, y, 0);
            shakeTime -= Time.deltaTime;

            // quando acabar, volta ao normal
            if (shakeTime <= 0f)
            {
                playerStats.position = originalPos;
                horizontalLayout.enabled = true;
                isShaking = true;
            }
        }
    }

    private void UpdateOriginalPosition()
    {
        originalPos = new Vector3(playerStats.position.x, playerStats.position.y, 0f);
    }
}
