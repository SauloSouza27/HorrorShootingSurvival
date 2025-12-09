using UnityEngine.InputSystem;
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

    // ─────────────────────────────────────────
    //  WAVE PROGRESS (RUNTIME)
    // ─────────────────────────────────────────
    [Header("Wave Progress (Runtime)")]
    [Tooltip("Current wave/round index (starts at 1).")]
    public int currentWave = 1;

    [Tooltip("Total zombies that should spawn this wave (BO2 formula).")]
    public int current_summons = 10;

    [Tooltip("How many zombies have been spawned so far this wave.")]
    public int summons_spawned = 0;

    [Tooltip("How many zombies are currently alive on the map.")]
    public int current_summons_alive = 0;

    [Tooltip("How many zombies from this wave have died.")]
    public int current_summons_dead = 0;

    // Max zombies alive on the map at once
    private int current_max_summons_once = 30;

    // ─────────────────────────────────────────
    //  DIFFICULTY / SCALING
    // ─────────────────────────────────────────
    [Header("Difficulty Scaling")]
    [Tooltip("Legacy spawn multiplier (no longer used with BO2 count formula).")]
    [HideInInspector]
    [SerializeField] private float spawn_multiplier = 1.2f;

    [Tooltip("Multiplicative strength factor applied each new wave, used by enemies as a generic difficulty multiplier (e.g. speed/attack if you want).")]
    [SerializeField] private float strength_multiplier = 1.1f;

    [Tooltip("Base strength multiplier for wave 1.")]
    [SerializeField] private float start_strength = 1f;

    [Tooltip("Current strength multiplier for this wave (used by EnemyBase.multiplier).")]
    [HideInInspector] public float current_strength = 1f;

    // ─────────────────────────────────────────
    //  STARTING VALUES (LEGACY / INIT)
    // ─────────────────────────────────────────
    [Header("Legacy Start Settings")]
    [Tooltip("Legacy initial summons (overridden by BO2 formula after NewWave).")]
    [HideInInspector]
    [SerializeField] private int start_summons = 10;

    [Tooltip("Maximum number of zombies allowed on the map at once (per wave).")]
    [SerializeField] private int start_max_summons_once = 30;

    // ─────────────────────────────────────────
    //  TIME SETTINGS
    // ─────────────────────────────────────────
    [Header("Time Settings")]
    [Tooltip("Delay between the end of a wave and the start of the next one.")]
    [SerializeField] private float time_between_waves = 30f;

    [Tooltip("Internal timestamp of when the last wave ended.")]
    [SerializeField] private float time_end_last_wave = 30f;

    [Tooltip("Whether the wave is currently spawning/active.")]
    [SerializeField] private bool waveIsRunning = false;

    [Header("Time Between Summons")]
    [Tooltip("Delay between individual zombie spawns (if use_time_between_summons is true).")]
    public float time_between_summons = 0.1f;

    private float time_last_summon = 0f;

    [Tooltip("If true, zombies spawn with a delay between each summon. If false, they spawn as fast as allowed by the cap.")]
    public bool use_time_between_summons = true;

    // ─────────────────────────────────────────
    //  SPAWNPOINTS / ENEMY PREFABS
    // ─────────────────────────────────────────
    [Header("Spawnpoint Settings")]
    [Tooltip("All enemy spawnpoints (with portal VFX as first child).")]
    public List<Transform> spawnpoints = new List<Transform>();

    [Tooltip("Parent transform under which all enemies will be instantiated.")]
    public Transform EnemyHolder;

    [Header("Enemy Settings")]
    [Tooltip("All potential enemy types (prefabs) this WaveSystem can use.")]
    public List<EnemyBase> enemy_Container = new List<EnemyBase>();

    // internal lists
    private readonly List<EnemyBase> avaible_enemys = new List<EnemyBase>();
    private readonly List<EnemyBase> selected_enemys = new List<EnemyBase>();

    // ─────────────────────────────────────────
    //  PLAYER RESPAWN
    // ─────────────────────────────────────────
    [Header("Player Respawn Settings")]
    [Tooltip("If true, dead/downed players are respawned at the start of a new wave with some minimum points.")]
    [SerializeField] private bool respawnDeadPlayersAtNewWave = true;

    [Tooltip("Points a respawned player should have at wave 1.")]
    [SerializeField] private int baseRespawnPoints = 500;

    [Tooltip("Extra points granted per wave when respawning (base + waveIndex * pointsPerWave).")]
    [SerializeField] private int pointsPerWave = 250;

    // ─────────────────────────────────────────
    //  AUDIO
    // ─────────────────────────────────────────
    [Header("Audio")]
    [Tooltip("Spawn SFX for standard melee enemies (played in 3D from the spawn point).")]
    public AudioClip meleeSpawnSFX;

    [Range(0f, 1f)]
    public float meleeSpawnVolume = 1f;

    [Tooltip("Legacy meteor spawn sound (not used yet).")]
    [HideInInspector]
    public AudioClip meteorSpawnSFX;

    [HideInInspector]
    [Range(0f, 1f)]
    public float meteorSpawnVolume = 1f;

    // ─────────────────────────────────────────
    //  DATA-DRIVEN TUNING (OPTIONAL)
    // ─────────────────────────────────────────
    [Header("Wave Tuning Data (Optional)")]
    [Tooltip("Optional controlled changes at certain waves (mostly legacy; spawn multiplier is not used with BO2 wave formula).")]
    public List<WaveData> newWaveData = new List<WaveData>();

    // ─────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────
    private void Start()
    {
        currentWave = 0;
        current_summons_alive = 0;
        current_summons_dead = 0;
        summons_spawned = 0;

        current_summons = start_summons; // immediately overridden by NewWave with BO2 formula
        current_max_summons_once = start_max_summons_once;
        current_strength = start_strength;

        time_end_last_wave = Time.time;
        time_last_summon = Time.time;

        NewWave();
    }

    private void Update()
    {
        WaveLoop();
    }

    // ─────────────────────────────────────────
    //  BO2 HEALTH / COUNT HELPERS (STATIC)
    // ─────────────────────────────────────────

    /// <summary>
    /// BO2 zombies health curve for a given round.
    /// Uses the wiki rule:
    /// - Round 1–9: 150 base, +100 per round (r1=150, r9=950)
    /// - From round 10: +10% per round starting from 950 at round 9.
    /// </summary>
    public static int GetZombieHealthForRound(int round)
    {
        if (round < 1) round = 1;

        // Before round 10: start at 150 and add 100 per round
        if (round <= 9)
        {
            return 150 + 100 * (round - 1);
        }

        // From round 10 and on: +10% multiplicatively from round-9 health (950)
        const float healthAtNine = 950f;
        int roundsAfterNine = round - 9;
        float health = healthAtNine * Mathf.Pow(1.1f, roundsAfterNine);

        return Mathf.RoundToInt(health);
    }

    /// <summary>
    /// Health factor relative to round-1 HP (150).
    /// </summary>
    public static float GetHealthFactorForWave(int wave)
    {
        if (wave < 1) wave = 1;
        const float hpRound1 = 150f;
        float hp = GetZombieHealthForRound(wave);
        return hp / hpRound1;
    }

    /// <summary>
    /// BO2 zombies-per-round formula (approximation of the wiki maths).
    /// Uses special early factors for rounds 1–9, then cubic growth from 10+.
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

    // ─────────────────────────────────────────
    //  SPAWNING & WAVE FLOW
    // ─────────────────────────────────────────

    // Add new zone spawn enemies
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

        // Difficulty multiplier (kept only for things like speed/attack)
        if (currentWave > 1)
        {
            current_strength *= strength_multiplier;
        }

        // BO2-style zombie count per wave
        int playerCount = Mathf.Max(1, PlayerHealth.AllPlayers.Count);
        current_summons = GetZombieCountForWave(currentWave, playerCount);

        // Reset Wave Data
        current_summons_alive = 0;
        summons_spawned = 0;
        current_summons_dead = 0;
        current_max_summons_once = start_max_summons_once;

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

            // Snapshot so we don't modify the list while iterating
           var playersSnapshot = new List<PlayerHealth>(PlayerHealth.AllPlayers);

            // Positions of players who are still alive (anchors for respawn)
            List<Vector3> alivePositions = new List<Vector3>();

            // Data needed to recreate dead players
            List<(int playerIndex, InputDevice device, string scheme)> slotsToRespawn =
            new List<(int, InputDevice, string)>();

        foreach (var ph in playersSnapshot)
        {
            if (ph == null) continue;

            var pi = ph.GetComponent<PlayerInput>();
            if (pi == null) continue;

            if (!ph.isDead)
            {
                // This player survived the wave → use as potential respawn anchor
                alivePositions.Add(ph.transform.position);
                continue;
            }

            // ── DEAD PLAYER: capture info and destroy it ──
            InputDevice device = (pi.devices.Count > 0) ? pi.devices[0] : null;
            string scheme = pi.currentControlScheme;

            slotsToRespawn.Add((pi.playerIndex, device, scheme));

            // If you have a HUD link component, clean up its HUD instance
            var hudLink = pi.GetComponent<PlayerHUDLink>();
            if (hudLink != null && hudLink.hudRoot != null)
            {
                Destroy(hudLink.hudRoot);
                hudLink.hudRoot = null;
            }

            // Destroy the old player object – OnDestroy will unregister its PlayerHealth
            Destroy(pi.gameObject);
        }

        if (slotsToRespawn.Count == 0)
            return;

        // Fallback: if somehow no one is alive, use a spawnpoint or origin
        if (alivePositions.Count == 0)
        {
            if (spawnpoints.Count > 0)
                alivePositions.Add(spawnpoints[0].position);
            else
                alivePositions.Add(Vector3.zero);
        }

        var pim = FindObjectOfType<PlayerInputManager>();
        if (pim == null)
        {
            Debug.LogWarning("WaveSystem: no PlayerInputManager found for respawn.");
            return;
        }

        float respawnRadius = 2.0f; // distance around the alive player

        foreach (var slot in slotsToRespawn)
        {
            // Re-join the player
            PlayerInput newPI = pim.JoinPlayer(
                slot.playerIndex,
                -1,
                slot.scheme,
                slot.device
            );

            if (newPI == null) continue;

            // ── Place them near a random alive player ──
            Vector3 anchor = alivePositions[Random.Range(0, alivePositions.Count)];
            Vector2 circle = Random.insideUnitCircle * respawnRadius;
            Vector3 spawnPos = anchor + new Vector3(circle.x, 0f, circle.y);

            newPI.transform.position = spawnPos;

            // Ensure they have at least minPoints
            var stats = newPI.GetComponent<PlayerStats>();
            if (stats != null)
            {
                int current = stats.GetPoints();
                if (current < minPoints)
                {
                    int delta = minPoints - current;
                    stats.AddPoints(delta);
                }
            }
        }
    }



    public void CheckNewWaveData()
    {
        if (newWaveData.Count > 0)
        {
            if (currentWave == newWaveData[0].change_at_wave)
            {
                // NOTE: spawn_multiplier is legacy with BO2 formula,
                // but kept so old data doesn't break.
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

        // portal VFX as first child
        VisualEffect portal = spawnpoint.GetChild(0).GetComponent<VisualEffect>();
        if (portal != null)
        {
            portal.Play();
        }

        // 3D spawn sound from that spawnpoint
        if (AudioManager.Instance != null && meleeSpawnSFX != null)
        {
            AudioManager.Instance.PlaySFX3D(
                meleeSpawnSFX,
                spawnpoint.position,
                meleeSpawnVolume,
                spatialBlend: 1f,
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

        // Assign a speed tier based on current round
        newEnemy.SetSpeedTier(GetSpeedTierForCurrentWave());

        current_summons_alive++;
        summons_spawned++;

        time_last_summon = Time.time;
    }

    public void SkipPause()
    {
        waveIsRunning = true;
    }

    /// <summary>
    /// BO2-style speed distribution for a single zombie:
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
    [Tooltip("At which wave this data change takes effect.")]
    public int change_at_wave = 5;

    [Header("New Data")]
    [Tooltip("Legacy: changes spawn multiplier (not used when using BO2 wave count formula).")]
    public bool change_spawn_multiplier = false;
    public float new_spawn_multiplier = 1f;

    [Space]
    [Tooltip("Whether to change strength multiplier at this wave.")]
    public bool change_strength_multiplier = false;
    public float new_strength_multiplier = 1f;
}
