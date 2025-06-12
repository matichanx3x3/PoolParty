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


[Serializable]
public class ZonePoint
{
    public int maxCustomer;
    public int actualCustomer;
    public bool isFull;
    public Transform point;
    public void ReleaseCustomer()
    {
        if (actualCustomer > 0)
            actualCustomer--;
        if (isFull && actualCustomer < maxCustomer)
            isFull = false;
    }
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
    public void RecalculateCapacity()
    {
        Capacity = zonePoints.Sum(zp => zp.actualCustomer);
        isZoneFull = Capacity >= MaxCap;
    }
}

public class GameManager : MonoBehaviour
{
    public GameObject optionsUI;
    public GameObject t1;
    public GameObject t2;
    public GameObject t3;
    public GameObject t4;
    public GameObject tutorialCanva;
    public GameObject mainOptions;
    public GameObject musicOptions;
    public bool isPaused = false;
    public static GameManager Instance; 
    public List<GameObject> CustomerPrefabs;
    public List<ZonesCap> capZones;
    //[SerializeField] GameObject npcPrefab;
    //GameOverUI
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] List<Transform> spawnPoints; //Puntos de instancia
    [SerializeField] List<Transform> doorPoints;
    [SerializeField] int maxAforo = 20;
    public int currentAforo;
    public int noProblematicAforo;
    public int problematicAforo;

    [SerializeField] private float spawnInterval;
    [SerializeField] private GameObject[] cameras = new GameObject[1];
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
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OpenOptionsMenu(!isPaused);
        }
    }

    public void OpenOptionsMenu(bool value)
    {
        ResetTutorial();
        isPaused = value;
        TogglePause(isPaused);
        optionsUI.SetActive(value);
    }

    private void ResetTutorial()
    {
        t1.SetActive(true);
        t2.SetActive(false);
        t3.SetActive(false);
        t4.SetActive(false);
        tutorialCanva.SetActive(false);
        mainOptions.SetActive(true);
        musicOptions.SetActive(false);

    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            GameOver();
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

    void HandleDespawnRequest(NPCBehavior npc)
    {
        // resta en uno de la zona que era el npc, tambien restando si este era problematico o no de la zona.
        var zp = npc.currentZonePoint;
        if (zp != null)
        {
            zp.ReleaseCustomer();
            var zc = capZones.First(z => z.typeZones == npc.npc.assignedZone);
            zc.RecalculateCapacity();
            if (npc.npc.isProblematic) zc.problematicConsumer--;
            else                       zc.noProblematicConsumer--;
        }

        if (npc.npc.isProblematic) problematicAforo--;
        else                       noProblematicAforo--;
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
        var randomIndex = UnityEngine.Random.Range(0, CustomerPrefabs.Count);
        var go = Instantiate(CustomerPrefabs[randomIndex], spawn.position, Quaternion.identity);
        var npcB = go.GetComponent<NPCBehavior>();

        npcB.BeignSpawned(door.transform);
    }

   public Transform AssignZoneToNPC(NPCBehavior npc)
    {
        TypeZones preferredZone = npc.npc.assignedZone;

        // primero la preferida, luego el resto
        var orderedZones = new List<ZonesCap>();
        var preferredCap = capZones.Find(zc => zc.typeZones == preferredZone);
        if (preferredCap != null) orderedZones.Add(preferredCap);
        orderedZones.AddRange(capZones.Where(zc => zc.typeZones != preferredZone));

        // Intentar asignar en orden
        foreach (var zoneCap in orderedZones)
        {
            
            var availablePoints = zoneCap.zonePoints.Where(zp => !zp.isFull).ToList();
            if (availablePoints.Count == 0)
                continue; 

            var freePoint = availablePoints[UnityEngine.Random.Range(0, availablePoints.Count)];

            if (zoneCap.typeZones != preferredZone)
                npc.npc.assignedZone = zoneCap.typeZones;

            freePoint.actualCustomer++;
            if (freePoint.actualCustomer >= freePoint.maxCustomer)
                freePoint.isFull = true;

            zoneCap.Capacity = zoneCap.zonePoints.Sum(zp => zp.actualCustomer);
            zoneCap.isZoneFull = zoneCap.Capacity >= zoneCap.MaxCap;

            currentAforo++;

            // 2) Incrementa el contador global según su estado actual
            if (npc.npc.isProblematic)
                problematicAforo++;
            else
                noProblematicAforo++;

            // 3) Y solo aquí actualizas zoneCap.noProblematicConsumer o .problematicConsumer
            if (npc.npc.isProblematic)
                zoneCap.problematicConsumer++;
            else
                zoneCap.noProblematicConsumer++;

            npc.AssignZonePoint(freePoint);
            
            return freePoint.point;
        }
        Debug.LogWarning("No hay espacio en ninguna zona disponible para el NPC");
        //HandleDespawnRequest(npc);
        return null;
    }

    public Transform GetExitDoor()
    {
        int randomIndex = UnityEngine.Random.Range(0, doorPoints.Count);
        return doorPoints[randomIndex];
    }


    public void CameraSwap(GameObject camera1, GameObject camera2)
    {
        Debug.Log("Cambiando cámara");
        camera1.SetActive(!camera1.activeSelf);
        camera2.SetActive(!camera2.activeSelf);
    }
    public void TogglePause(bool isPaused)
    {
        if (isPaused)
            PauseGame();
        else
            ResumeGame();
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        Debug.Log("Juego pausado");
    }

    // Método para reanudar el juego
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Juego reanudado");
    }

     private void GameOver()
    {
        bool tooManyProblem =  problematicAforo >= Mathf.CeilToInt(maxAforo * 0.5f);

        if (!tooManyProblem)
            return; // aún no es Game Over

        PauseGame();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Debug.Log("GameOver");
        StopCoroutine("SpawnLoop");
        
    }
}
