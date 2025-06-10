using System;
using System.Collections.Generic;
using UnityEngine;

public enum TypeZones
{
    SocialZone,
    BarZone,
    DanceZone,
}

[Serializable]public class ZonesCap
{
    public TypeZones typeZones;
    public int Capacity;
    public List<Transform> SocialPoints;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<ZonesCap> capZones;
    [SerializeField] GameObject npcPrefab;
    [SerializeField] List<Transform> spawnPoints; //Puntos de instancia
    [SerializeField] int maxAforo = 20;
    int currentAforo;

    [SerializeField] List<GameObject> normalConsumers = new List<GameObject>();
    [SerializeField] List<GameObject> problematicConsumers = new List<GameObject>();

    void Awake()
    {
        Instance = this;

        NPCBehavior.OnNPCRequestDespawn += HandleDespawnRequest;
    }
    public int GetZoneCapacity(TypeZones type)
    {
        switch (type)
        {
            case TypeZones.SocialZone:
                return capZones[2].Capacity;
            case TypeZones.BarZone:
                return capZones[3].Capacity;
            case TypeZones.DanceZone:
                return capZones[4].Capacity;
            default:
                return 0;
        }
    }

    void Start()
    {
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allGameObjects)
        {
            if (obj.CompareTag("Consumer"))
            {
                normalConsumers.Add(obj);
            }
        }
        allGameObjects = null;

    }

    void Update()
    {
        if (normalConsumers.Count < maxAforo)
        {
            SpawnNPC();
        }
    }

    private void RefreshAforo()
    {
        currentAforo = normalConsumers.Count + problematicConsumers.Count;
        Debug.Log("Current Aforo: " + currentAforo);
        if (currentAforo < maxAforo * 0.2f)
        {
            GameOver();
        }
    }


    private void OnDisable()
    {
        NPCBehavior.OnNPCRequestDespawn -= HandleDespawnRequest;
    }

    void HandleDespawnRequest(NPCBehavior npc)
    {
        if (normalConsumers.Contains(npc.gameObject))
        {
            normalConsumers.Remove(npc.gameObject);
        }
        else if (problematicConsumers.Contains(npc.gameObject))
        {
            problematicConsumers.Remove(npc.gameObject);
        }
        Destroy(npc.gameObject);
    }

    public void SpawnNPC()
    {
        if (currentAforo < maxAforo)
        {
            Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)]; //spawnean el npc en el un punto de spawn (puertas)
            GameObject newNPC = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
            normalConsumers.Add(newNPC);
        }
    }

    //game over si el aforo actual es menos al 20% del maximo
    void GameOver()
    {
        // Time.timeScale = 0f;
    }
}
