using System.Collections.Generic;
using UnityEngine;

public class UnlockNewZoneSpawn : MonoBehaviour
{
    public static UnlockNewZoneSpawn instance;

    private List<Transform> newSpawnPoints = new List<Transform>();
    [SerializeField] private Transform[] zones;

    private void Awake()
    {
        instance = this;

        zones = new Transform[gameObject.transform.childCount];

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            zones[i] = gameObject.transform.GetChild(i);
        }

        for (int i = 1; i < zones.Length; i++)
        {
            zones[i].gameObject.SetActive(false);
        }
    }

    public void EnableZone(int zoneNumber)
    {
        if (zones[4].gameObject.activeSelf && zoneNumber == 4)
        {
            zoneNumber = 3;
        }

        Transform zoneToActivate = zones[zoneNumber];
        zoneToActivate.gameObject.SetActive(true);

        newSpawnPoints = new List<Transform>(zoneToActivate.childCount);

        for (int i = 0;  i < zoneToActivate.childCount ; i++)
        {
            newSpawnPoints.Add(zoneToActivate.GetChild(i));
        }

        WaveSystem.instance.AddNewZone(newSpawnPoints);
    }
}
