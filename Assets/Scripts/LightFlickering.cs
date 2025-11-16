using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    [SerializeField] private Light lightScene;
    [SerializeField] private float minIntensity = 0.5f;   // intensidade m�nima da luz
    [SerializeField] private float maxIntensity = 1.5f;   // intensidade m�xima da luz
    [SerializeField] private float flickerSpeed = 0.1f;   // tempo entre varia��es
    [SerializeField] private bool isActive = false;

    private float targetIntensity;
    private float timer;

    void Start()
    {
        targetIntensity = lightScene.intensity;
    }

    void Update()
    {
        if (!isActive) return;

        SetLightFlicker(isActive);
    }

    public void SetLightFlicker(bool active)
    {
        if (active)
        {
            timer += Time.deltaTime;

            // Troca o alvo da intensidade de forma aleat�ria a cada intervalo
            if (timer >= flickerSpeed)
            {
                targetIntensity = Random.Range(minIntensity, maxIntensity);
                timer = 0f;
            }

            // Suaviza a transi��o entre as intensidades
            lightScene.intensity = Mathf.Lerp(lightScene.intensity, targetIntensity, Time.deltaTime * 10f);
        }

        else
        {
            lightScene.intensity = 5f;
        }
    }
}
