using UnityEngine;

public class ImpactFX : MonoBehaviour
{
    private ParticleSystem[] systems;

    private void Awake()
    {
        systems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void ApplyColor(Color color)
    {
        if (systems == null) return;

        foreach (var ps in systems)
        {
            if (ps == null) continue;

            var main = ps.main;
            var start = main.startColor;

            // Preserve original alpha if there is one
            float a = 1f;
            if (start.mode == ParticleSystemGradientMode.Color ||
                start.mode == ParticleSystemGradientMode.TwoColors)
            {
                a = start.color.a;
            }

            Color final = new Color(color.r, color.g, color.b, a);
            main.startColor = new ParticleSystem.MinMaxGradient(final);
        }
    }
}