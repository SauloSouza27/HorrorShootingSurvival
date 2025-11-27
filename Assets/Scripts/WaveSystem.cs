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

    [Header("Increase Settings")]
    [SerializeField] private float spawn_multiplier = 1.2f;
    [SerializeField] private float strength_multiplier = 1.1f;

    [Header("Start Settings")]
    [SerializeField] private int start_summons = 10;
    public int current_summons = 10;
    public int summons_spawned = 0;
    public int current_summons_alive = 0;
    public int current_summons_dead = 0;
    private int current_max_summons_once = 30;
    [SerializeField] private int start_max_summons_once = 30;
    [SerializeField] private float start_strength = 1f;
    private float current_strength = 1f;

    [Header("Time Settings")]
    [SerializeField] private float time_between_waves = 30f;
    [SerializeField] private float time_end_last_wave = 30f;
    [SerializeField] private bool waveIsRunning = false;

    [Header("Spawnpoint Settings")]
    public List<Transform> spawnpoints = new List<Transform>();
    public Transform EnemyHolder;

    [Header("Enemy Settings")]
    public List<EnemyBase> enemy_Container = new List<EnemyBase>();
    private List<EnemyBase> avaible_enemys = new List<EnemyBase>();
    private List<EnemyBase> selected_enemys = new List<EnemyBase>();

    [Header("Time Between Summons")]
    public float time_between_summons = 0.1f;
    private float time_last_summon = 0f;
    public bool use_time_between_summons = true;

    [Header("Change Data")]
    public List<WaveData> newWaveData = new List<WaveData>();
    
    [Header("Audio")] 
    public AudioClip meleeSpawnSFX;
    [Range(0f, 1f)] public float meleeSpawnVolume = 1f;
    public AudioClip meteorSpawnSFX;
    [Range(0f, 1f)] public float meteorSpawnVolume = 1f;

    public void Start()
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

    //Add new zone spawn enemys

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
        // Upgrade Enemys
        if (currentWave > 1)
        {
            current_summons = (int)((float)current_summons * spawn_multiplier);
            current_strength = current_strength * strength_multiplier;
        }
        // Reset Wave Data
        current_summons_alive = 0;
        summons_spawned = 0;
        current_summons_dead = 0;
        // Generate Aviable Enemy List
        avaible_enemys.Clear();
        for (int i = 0; i < enemy_Container.Count; i++)
        {
            if (currentWave <= enemy_Container[i].avaible_to_wave && currentWave >= enemy_Container[i].avaible_from_wave)
            {
                avaible_enemys.Add(enemy_Container[i]);
            }
        }
        // Generate Selected Enemy List
        selected_enemys.Clear();
        for (int i = 0; i < current_summons; i++)
        {
            selected_enemys.Add(Get_Random_Enemy());
        }

        CheckNewWaveData();
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

    public void Update()
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

        EnemyBase NewEnemy = Instantiate(
            selected_enemys[summons_spawned],
            spawnpoint.transform.position,
            Quaternion.identity,
            EnemyHolder);

        NewEnemy.multiplier = current_strength;
        NewEnemy.AdjustEnemyToWave();
        current_summons_alive++;
        summons_spawned++;

        time_last_summon = Time.time;
    }


    public void SkipPause()
    {
        waveIsRunning = true;
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