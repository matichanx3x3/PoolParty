using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TypeZones
{
    SocialZone,
    BarZone,
    DanceZone,
}

[Serializable]public class ZonePoint
{
    public int maxCustomer;
    public int actualCustomer;
    public bool isFull;
    public Transform point;
    
}

[Serializable]public class ZonesCap
{
    public TypeZones typeZones;
    public int Capacity;

    public int MaxCap;

    public int noProblematicConsumer;
    public int problematicConsumer;

    public bool isZoneFull;
    public List<ZonePoint> zonePoints;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<ZonesCap> capZones;
    [SerializeField] GameObject npcPrefab;
    [SerializeField] List<Transform> spawnPoints; //Puntos de instancia
    [SerializeField] List<Transform> doorPoints;
    [SerializeField] int maxAforo = 20;
    public int currentAforo;

    public List<GameObject> normalConsumers = new List<GameObject>();
    [SerializeField] List<GameObject> problematicConsumers = new List<GameObject>();

    [SerializeField] private float spawnInterval;
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
                return capZones[0].Capacity;
            case TypeZones.BarZone:
                return capZones[1].Capacity;
            case TypeZones.DanceZone:
                return capZones[2].Capacity;
            default:
                return 0;
        }
    }

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnNPC();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void RefreshAforo()
    {
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
        // resta en uno de la zona que era el npc, tambien restando si este era problematico o no de la zona.

        var zoneType = npc.npc.assignedZone;
        var zoneCap = capZones.First(z => z.typeZones == zoneType);

        zoneCap.Capacity--;
        if (npc.npc.isProblematic)
            zoneCap.problematicConsumer--;
        else
            zoneCap.noProblematicConsumer--;

        currentAforo--;

        Destroy(npc.gameObject);

        RefreshAforo();
    }

    public void SpawnNPC()
    {
        if (currentAforo >= maxAforo) return;
        var randomNumberForSpawn = UnityEngine.Random.Range(0, doorPoints.Count);
        var spawn = spawnPoints[randomNumberForSpawn];
        var door = doorPoints[randomNumberForSpawn];

        var go = Instantiate(npcPrefab, spawn.position, Quaternion.identity);
        var npcB = go.GetComponent<NPCBehavior>();

        npcB.BeignSpawned(door.transform);
    }

   public Transform AssignZoneToNPC(NPCBehavior npc)
    {
        //zona según su rol
        TypeZones zoneType = npc.npc.assignedZone;

        // Buscar el objeto ZonesCap para esa zona
        ZonesCap zoneCap = capZones.Find(z => z.typeZones == zoneType);
        if (zoneCap == null)
        {
            Debug.LogError($"No existe ZonesCap para {zoneType}");
            return null;
        }

        // find ZonePoint que no esté lleno
        ZonePoint freePoint = zoneCap.zonePoints.Find(zp => !zp.isFull);
        if (freePoint == null)
        {
            Debug.LogWarning($"Zona {zoneType} está completa (todos los puntos llenos)");
            return null;
        }

    
        freePoint.actualCustomer++;
        //si alcanza el máximo -> lleno
        if (freePoint.actualCustomer >= freePoint.maxCustomer)
            freePoint.isFull = true;

        zoneCap.Capacity = zoneCap.zonePoints.Sum(zp => zp.actualCustomer);
        
        if (zoneCap.Capacity >= zoneCap.MaxCap)
            zoneCap.isZoneFull = true;

        if (npc.npc.isProblematic)
            zoneCap.problematicConsumer++;
        else
            zoneCap.noProblematicConsumer++;

        return freePoint.point;
    }
    

    void GameOver()
    {
        //game over si el aforo actual es menos al 20% del maximo o cuando la cantidad de problematicos sea del 50% del aforo maximo del lugar
    }
}
