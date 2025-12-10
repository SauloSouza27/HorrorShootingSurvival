using UnityEngine;
using UnityEngine.UI;

public class CheatMenu : MonoBehaviour
{
    [Header("Cheat Toggles")]
    [SerializeField] private Toggle infiniteLifeToggle;
    [SerializeField] private Toggle add10kToggle;

    private void Awake()
    {
        // Make sure toggles start in a consistent state
        if (infiniteLifeToggle != null)
        {
            infiniteLifeToggle.isOn = PlayerHealth.InfiniteHealthCheat;
            infiniteLifeToggle.onValueChanged.AddListener(OnInfiniteLifeToggled);
        }

        if (add10kToggle != null)
        {
            add10kToggle.isOn = false; // behaves like a "button"
            add10kToggle.onValueChanged.AddListener(OnAdd10kToggled);
        }
    }

    private void OnDestroy()
    {
        if (infiniteLifeToggle != null)
            infiniteLifeToggle.onValueChanged.RemoveListener(OnInfiniteLifeToggled);

        if (add10kToggle != null)
            add10kToggle.onValueChanged.RemoveListener(OnAdd10kToggled);
    }

    private void OnInfiniteLifeToggled(bool isOn)
    {
        PlayerHealth.InfiniteHealthCheat = isOn;
    }

    private void OnAdd10kToggled(bool isOn)
    {
        // We treat this toggle like a "button": when turned ON, give 10k and turn it back OFF.
        if (!isOn) return;

        foreach (var ph in PlayerHealth.AllPlayers)
        {
            if (ph == null) continue;

            var stats = ph.GetComponent<PlayerStats>();
            if (stats != null)
                stats.AddPoints(10000);
        }

        // Reset toggle so you can press it again
        add10kToggle.isOn = false;
    }
}