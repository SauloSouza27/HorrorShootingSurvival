using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    public TextMeshProUGUI WaveCountText;
    public TextMeshProUGUI SummonsText;   

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
    }
    public void UpdateUI()
    {
        WaveCountText.text = "Wave " + WaveSystem.instance.currentWave;
        SummonsText.text = "Enemies " + WaveSystem.instance.current_summons + " / " + WaveSystem.instance.current_summons_alive;
    }
}
