using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WaveSystem : MonoBehaviour
{
    public static WaveSystem instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Main Settings")]
    public int currentWave = 1;

    [Header("Increase Settings (only for speed/attack etc.)")]
    [SerializeField] private float spawn_multiplier = 1.2f;   // still used for max-on-map if you want
    [SerializeField] private float strength_multiplier = 1.1f; // still used as a generic “difficulty” for speed/attack

    [Header("Start Settings")]
    [SerializeField] private int start_summons = 10;
    public int current_summons = 10;
    public int summons_spawned = 0;
    public int current_summons_alive = 0;
    public int current_summons_dead = 0;

    private int current_max_summons_once = 30;
    [SerializeField] private int start_max_summons_once = 30;

    [SerializeField] private float start_strength = 1f;
    [HideInInspector] public float current_strength = 1f;   // used by EnemyBase.multiplier

    [Header("Time Settings")]
    [SerializeField] private float time_between_waves = 30f;
    [SerializeField] private float time_end_last_wave = 30f;
    [SerializeField] private bool waveIsRunning = false;

    [Header("Spawnpoint Settings")]
    public List<Transform> spawnpoints = new List<Transform>();
    public Transform EnemyHolder;

    [Header("Enemy Settings")]
    public List<EnemyBase> enemy_Container = new List<EnemyBase>();
    private readonly List<EnemyBase> avaible_enemys = new List<EnemyBase>();
    private readonly List<EnemyBase> selected_enemys = new List<EnemyBase>();

    [Header("Time Between Summons")]
    public float time_between_summons = 0.1f;
    private float time_last_summon = 0f;
    public bool use_time_between_summons = true;

    [Header("Player Respawn Settings")]
    [SerializeField] private bool respawnDeadPlayersAtNewWave = true;
    [SerializeField] private int baseRespawnPoints = 500; // points at wave 1
    [SerializeField] private int pointsPerWave = 250;     // extra per wave

    [Header("Audio")]
    public AudioClip meleeSpawnSFX;
    [Range(0f, 1f)] public float meleeSpawnVolume = 1f;
    public AudioClip meteorSpawnSFX;
    [Range(0f, 1f)] public float meteorSpawnVolume = 1f;

    [Header("Change Data")]
    public List<WaveData> newWaveData = new List<WaveData>();

    private void Start()
    {
        currentWave = 0;
        current_summons_alive = 0;
        current_summons_dead = 0;
        summons_spawned = 0;

        current_summons = start_summons;
        current_max_summons_once = start_max_summons_once;
        current_strength = start_strength;

        time_end_last_wave = Time.time;
        time_last_summon = Time.time;

        NewWave();
    }

    // ---------- PUBLIC HELPERS FOR OTHER SYSTEMS ----------

    /// <summary>
    /// BO2 zombies health curve, returned as a **multiplier relative to round 1**.
    /// Round 1 reference HP is 150.
    /// </summary>
    public static float GetHealthFactorForWave(int wave)
    {
        if (wave < 1) wave = 1;

        const float hpRound1 = 150f;       // BO2 base
        float hp;

        if (wave <= 9)
        {
            // 150 base, +100 each round until 9
            hp = 150f + 100f * (wave - 1);
        }
        else
        {
            // From round 10: +10% per round, starting from round 9 value (950 HP)
            float hpAt9 = 150f + 100f * (9 - 1); // = 950
            int extraRounds = wave - 9;
            hp = hpAt9 * Mathf.Pow(1.10f, extraRounds);
        }

        // Factor relative to round-1 HP
        return hp / hpRound1;
    }

    /// <summary>
    /// BO2 zombies-per-round formula (approximation of the wiki maths).
    /// Uses:
    /// - special early-round factors for round 1–9
    /// - cubic growth for 10+ as in the wiki formula.
    /// </summary>
    public static int GetZombieCountForWave(int wave, int playerCount)
    {
        if (wave < 1) wave = 1;
        if (playerCount < 1) playerCount = 1;

        bool isSolo = playerCount == 1;

        if (wave < 10)
            return GetZombieCount_PreRound10(wave, playerCount, isSolo);
        else
            return GetZombieCount_Round10Plus(wave, playerCount, isSolo);
    }

    private static float GetEarlyRoundFactor(int wave)
    {
        // Same piecewise factors as the wiki image
        switch (wave)
        {
            case 1: return 0.25f;
            case 2: return 0.30f;
            case 3: return 0.50f;
            case 4: return 0.70f;
            case 5: return 0.90f;
            default: return 1.0f;
        }
    }

    private static int GetZombieCount_PreRound10(int wave, int playerCount, bool isSolo)
    {
        float factor = GetEarlyRoundFactor(wave);
        float roundTerm = Mathf.Max(1f, wave / 5f);

        float baseValue;
        if (isSolo)
        {
            // Solo: 24 + (0.5 × 6 × max(1, Round/5))
            baseValue = 24f + 0.5f * 6f * roundTerm;
        }
        else
        {
            // Co-op: 24 + ((PlayerCount − 1) × 6 × max(1, Round/5))
            baseValue = 24f + (playerCount - 1) * 6f * roundTerm;
        }

        return Mathf.FloorToInt(baseValue * factor);
    }

    private static int GetZombieCount_Round10Plus(int wave, int playerCount, bool isSolo)
    {
        float roundTerm = wave / 5f;
        float baseValue;

        if (isSolo)
        {
            // Solo: 24 + (0.5 × 6 × Round/5 × Round × 0.15)
            baseValue = 24f + (0.5f * 6f * roundTerm * wave * 0.15f);
        }
        else
        {
            // Co-op: 24 + ((PlayerCount − 1) × 6 × Round/5 × Round × 0.15)
            baseValue = 24f + ((playerCount - 1) * 6f * roundTerm * wave * 0.15f);
        }

        return Mathf.FloorToInt(baseValue);
    }

    // ---------- WAVE FLOW ----------

    //Add new zone spawn enemies
    public void AddNewZone(List<Transform> newSpawns)
    {
        List<Transform> currentSpawnPoints = spawnpoints;
        int currentListCount = currentSpawnPoints.Count;
        int newListCount = currentListCount + newSpawns.Count;

        spawnpoints = new List<Transform>(newListCount);

        spawnpoints.AddRange(currentSpawnPoints);
        spawnpoints.AddRange(newSpawns);
    }

    public void NewWave()
    {
        waveIsRunning = false;
        currentWave++;
        time_end_last_wave = Time.time;

        // ▼ Difficulty multiplier (kept only for things like speed/attack)
        if (currentWave > 1)
        {
            current_strength *= strength_multiplier;
        }

        // ▼ BO2-style zombie count per wave
        int playerCount = Mathf.Max(1, PlayerHealth.AllPlayers.Count);
        current_summons = GetZombieCountForWave(currentWave, playerCount);

        // Reset Wave Data
        current_summons_alive = 0;
        summons_spawned = 0;
        current_summons_dead = 0;
        current_max_summons_once = start_max_summons_once; // you can tune this, e.g. 24

        // Generate Available Enemy List
        avaible_enemys.Clear();
        for (int i = 0; i < enemy_Container.Count; i++)
        {
            if (currentWave <= enemy_Container[i].avaible_to_wave &&
                currentWave >= enemy_Container[i].avaible_from_wave)
            {
                avaible_enemys.Add(enemy_Container[i]);
            }
        }

        // Generate Selected Enemy List (which prefab each spawn will use)
        selected_enemys.Clear();
        for (int i = 0; i < current_summons; i++)
        {
            selected_enemys.Add(Get_Random_Enemy());
        }

        CheckNewWaveData();
        HandlePlayerRespawns();
    }

    private void HandlePlayerRespawns()
    {
        if (!respawnDeadPlayersAtNewWave)
            return;

        int minPoints = Mathf.Max(0, baseRespawnPoints + (currentWave - 1) * pointsPerWave);
        PlayerHealth.RespawnAllForNewWave(minPoints);
    }

    public void CheckNewWaveData()
    {
        if (newWaveData.Count > 0)
        {
            if (currentWave == newWaveData[0].change_at_wave)
            {
                if (newWaveData[0].change_spawn_multiplier)
                {
                    spawn_multiplier = newWaveData[0].new_spawn_multiplier;
                }

                if (newWaveData[0].change_strength_multiplier)
                {
                    strength_multiplier = newWaveData[0].new_strength_multiplier;
                }
                newWaveData.RemoveAt(0);
            }
        }
    }

    public EnemyBase Get_Random_Enemy()
    {
        if (avaible_enemys.Count > 0)
        {
            int randomint = Random.Range(0, avaible_enemys.Count);
            return avaible_enemys[randomint];
        }
        return null;
    }

    private void Update()
    {
        WaveLoop();
    }

    public void WaveLoop()
    {
        if (waveIsRunning)
        {
            if (summons_spawned < current_summons)
            {
                if (current_summons_alive < current_max_summons_once)
                {
                    if (!use_time_between_summons)
                    {
                        SpawnEnemy();
                    }
                    else
                    {
                        if (Time.time > time_last_summon + time_between_summons)
                        {
                            SpawnEnemy();
                        }
                    }
                }
            }
            else
            {
                if (current_summons_dead == summons_spawned)
                {
                    NewWave();
                }
            }
        }
        else
        {
            if (Time.time > time_end_last_wave + time_between_waves)
            {
                waveIsRunning = true;
            }
        }
    }

    public void SpawnEnemy()
    {
        Transform spawnpoint;
        int randomspawnint = Random.Range(0, spawnpoints.Count);
        spawnpoint = spawnpoints[randomspawnint];

        // ativa efeito portal
        VisualEffect portal = spawnpoint.GetChild(0).GetComponent<VisualEffect>();

        if (portal != null)
        {
            portal.Play();
        }

        // 🔊 3D spawn sound from that spawnpoint
        if (AudioManager.Instance != null && meleeSpawnSFX != null)
        {
            AudioManager.Instance.PlaySFX3D(
                meleeSpawnSFX,
                spawnpoint.position,
                meleeSpawnVolume,
                spatialBlend: 1f,  // fully 3D
                minDistance: 5f,
                maxDistance: 80f
            );
        }

        EnemyBase newEnemy = Instantiate(
            selected_enemys[summons_spawned],
            spawnpoint.transform.position,
            Quaternion.identity,
            EnemyHolder);

        newEnemy.multiplier = current_strength;
        newEnemy.AdjustEnemyToWave();

        // 🔹 Assign a speed tier based on current round
        newEnemy.SetSpeedTier(GetSpeedTierForCurrentWave());

        current_summons_alive++;
        summons_spawned++;

        time_last_summon = Time.time;
    }
    
    public static int GetZombieHealthForRound(int round)
    {
        if (round < 1) round = 1;

        // Before round 10: start at 150 and add 100 per round
        if (round <= 9)
        {
            // r1 = 150, r2 = 250, ..., r9 = 950
            return 150 + 100 * (round - 1);
        }

        // From round 10 and on: +10% multiplicatively from round-9 health (950)
        const float healthAtNine = 950f;
        int roundsAfterNine = round - 9;
        float health = healthAtNine * Mathf.Pow(1.1f, roundsAfterNine);

        // Examples (to match the wiki):
        // r15 ≈ 1683, r30 ≈ 7030, r50 ≈ 47296, r100 ≈ 5.5M
        return Mathf.RoundToInt(health);
    }

    
    /// <summary>
    /// BO2-style speed distribution:
    /// - Round 1–2: 100% Tier1.
    /// - From round 3 onwards: +10% of zombies become "fast" per round,
    ///   capped at 90% fast. Remaining are Tier1.
    /// - Fast zombies are split into Tier2/3/4 as rounds go up.
    /// - Distribution stops changing after round 20.
    /// </summary>
    private ZombieSpeedTier GetSpeedTierForCurrentWave()
    {
        int wave = Mathf.Clamp(currentWave, 1, 20);

        // Early rounds: everyone shambles
        if (wave <= 2)
            return ZombieSpeedTier.Tier1;

        // Total fraction of zombies in this round that should be faster than Tier1
        // Example: round 3 -> 10%, round 4 -> 20%, ... round 11+ -> 90% (capped).
        float fastFrac = Mathf.Clamp01(0.1f * (wave - 2));   // 0..0.9

        // Split fast zombies across tiers as rounds go up:
        float p2 = 0f, p3 = 0f, p4 = 0f;

        if (wave < 8)
        {
            // Rounds 3–7: only Tier2 exists, so all fast zombies are Tier2
            p2 = fastFrac;
        }
        else if (wave < 14)
        {
            // Rounds 8–13: introduce Tier3, but keep most fast zombies at Tier2
            p3 = fastFrac * 0.3f;      // 30% of fast ones are Tier3
            p2 = fastFrac - p3;        // rest of fast ones are Tier2
        }
        else
        {
            // Rounds 14–20: all three fast tiers exist
            p4 = fastFrac * 0.3f;      // 30% of fast ones are Tier4
            p3 = fastFrac * 0.3f;      // 30% of fast ones are Tier3
            p2 = fastFrac - p3 - p4;   // remaining fast ones are Tier2
        }

        float p1 = 1f - fastFrac;      // slow shamblers

        // Randomly choose tier based on these probabilities.
        float r = UnityEngine.Random.value;

        if (r < p1) return ZombieSpeedTier.Tier1;
        r -= p1;

        if (r < p2) return ZombieSpeedTier.Tier2;
        r -= p2;

        if (r < p3) return ZombieSpeedTier.Tier3;

        return ZombieSpeedTier.Tier4;
    }


}

[System.Serializable]
public class WaveData
{
    [Header("Wave Data")]
    public int change_at_wave = 5;

    [Header("New Data")]
    public bool change_spawn_multiplier = false;
    public float new_spawn_multiplier = 1f;

    [Space]
    public bool change_strength_multiplier = false;
    public float new_strength_multiplier = 1f;
}
