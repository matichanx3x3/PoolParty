using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject npcPrefab;
    [SerializeField] List<Transform> spawnPoints;
    [SerializeField] int maxNPCs = 20;
    [SerializeField] List<GameObject> npcs = new List<GameObject>();

    void Awake()
    {
        NPCBehavior.OnNPCRequestDespawn += HandleDespawnRequest;
    }

    void Start()
    {
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allGameObjects)
        {
            if (obj.CompareTag("Consumer"))
            {
                npcs.Add(obj);
            }
        }
        allGameObjects = null;
        InvokeRepeating("SpawnNPC", 0f, 5f); //DEBUG: Spawns an NPC every 5 seconds 
    }


    private void OnDisable()
    {
        NPCBehavior.OnNPCRequestDespawn -= HandleDespawnRequest;
    }

    void HandleDespawnRequest(NPCBehavior npc)
    {
        npcs.Remove(npc.gameObject);
        Destroy(npc.gameObject);
        
    }

    public void SpawnNPC()
    {
        if (npcs.Count < maxNPCs)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)]; //spawnean el npc en el un punto de spawn (puertas)
            GameObject newNPC = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
            npcs.Add(newNPC);
        }
    }
    
}
