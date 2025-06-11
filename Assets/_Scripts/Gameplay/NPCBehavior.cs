using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum NPCRole { Social, Bebedor, Bailador }
public enum NPCMood { Normal, Borracho, Euforico, Peleador }

[Serializable]
public class NPC
{
    public NPCRole role;
    public NPCMood mood;
    public TypeZones assignedZone;
    public bool isProblematic;
    public int molestia;
    public int MaxSatisfaccion = 100;
    public int satisfaccion;
    public float partyMaxTimer;
    public float partyActualTimer;
    public int nroSerMolestado;
    public bool inTransition;
    public NPC(NPCRole role, NPCMood mood)
    {
        this.role = role;
        this.mood = mood;
        isProblematic = false;
        molestia = 10;
        satisfaccion = MaxSatisfaccion - molestia;
        partyMaxTimer = 40;
        partyActualTimer = 0;
        nroSerMolestado = 0;
        inTransition = false;
    }
}

public class NPCBehavior : MonoBehaviour
{
    public NPC npc;
    public static event Action<NPCBehavior> OnNPCRequestDespawn;
    [SerializeField] Transform directionToMove;
    [HideInInspector] public bool canMove = true; // Indicates if the NPC can move
    bool busy = false;
    bool isGoingKicked = false;

    public float moveSpeed = 2f;
    private Coroutine moveCoroutine;

    public Rigidbody2D rb;
    public Collider2D col2D;

    void Start()
    {

    }
    void Update()
    {

    }

    // Llamado desde GameManager justo tras Instantiate
    public void BeignSpawned(Transform doorTransform)
    {

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveToPoint(doorTransform.position, OnArrivedAtDoor));
    }

    private IEnumerator MoveToPoint(Vector3 targetPos, Action onArrive)
    {
        // Mientras no estemos cerca del target, movemos
        while ((transform.position - targetPos).sqrMagnitude > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }
        // invocamos callback
        onArrive?.Invoke();
    }

    // Callback cuando termina de llegar a la puerta
    private void OnArrivedAtDoor()
    {
        // Aquí pides la zona disponible al GameManager
        GenerateNewConsumer();
    }

    private void RequestZoneFromManager()
    {
        // Asigna el punto donde irá en la zona
        Transform assignedPoint = GameManager.Instance.AssignZoneToNPC(this);
        if (assignedPoint == null)
        {
            Debug.LogWarning("No se pudo asignar punto a NPC");
            return;
        }

        GameManager.Instance.normalConsumers.Add(this.gameObject);

        GoToAsignedPoint(assignedPoint);
    }

    public void GoToAsignedPoint(Transform pointToGoTransform)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveToPoint(pointToGoTransform.position, DoRoutine));
    }

    public void DoRoutine()
    {
        //rutinas de si es social, bebedor o bailador. todos siempre con el mood NORMAL
        print("Hago mi rutina designada " + npc.role);
        col2D.isTrigger = false;
        Timer();
    }

    public void AddMolestiaPoints()
    {
        npc.molestia += 5;
    }

    public void AddSerMolestado()
    {
        npc.nroSerMolestado++;
        AddMolestiaPoints();
    }

    public void DoProblematicRoutine()
    {
        //si el npc se ha convertido en un problematico, se le asigna su rutina modificada de Borracho, Euforico, Problematico.
    }

    public void Timer()
    {
        StartCoroutine(DoTimer());
    }

    private IEnumerator DoTimer()
    {
        while (npc.partyActualTimer < npc.partyMaxTimer)
        {
            npc.partyActualTimer += Time.deltaTime;
            yield return null;
        }
        // Aquí ya llegó al máximo
        inTransition();
    }

    public void ResetMovement()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Door"))
        {
            switch (isGoingKicked)
            {
                case true:
                    DespawnMe();
                    break;
                case false:
                    GenerateNewConsumer();
                    break;
            }
        }

    }

    void DespawnMe()
    {
        OnNPCRequestDespawn?.Invoke(this); // Lanza el evento
    }
    void GenerateNewConsumer()
    {
        NPCRole desiredRole = (NPCRole)UnityEngine.Random.Range(0, 3);
        print("Desired role: " + desiredRole);

        // my rol?
        var (finalRole, zoneType) = VerifyZoneAvailability(desiredRole);

        // new data
        var data = new NPC(finalRole, NPCMood.Normal);
        data.assignedZone = zoneType;
        npc = data;

        RequestZoneFromManager();

    }

    private (NPCRole role, TypeZones zone) VerifyZoneAvailability(NPCRole desiredRole)
    {
        // rol -> zona, diccionario temporal
        var roleToZone = new Dictionary<NPCRole, TypeZones>
        {
            { NPCRole.Social, TypeZones.SocialZone },
            { NPCRole.Bebedor, TypeZones.BarZone },
            { NPCRole.Bailador, TypeZones.DanceZone }
        };

        //setea roles en orden, primero el deseado
        var roles = new List<NPCRole> { desiredRole };
        roles.AddRange(Enum.GetValues(typeof(NPCRole))
                           .Cast<NPCRole>()
                           .Where(r => r != desiredRole && (r == NPCRole.Social || r == NPCRole.Bebedor || r == NPCRole.Bailador)));

        // search for zona con hueco.
        foreach (var role in roles)
        {
            var zone = roleToZone[role];
            var cap = GameManager.Instance.capZones.Find(zc => zc.typeZones == zone);
            if (cap != null && cap.Capacity < cap.MaxCap)
            {
                return (role, zone);
            }
        }

        return (desiredRole, roleToZone[desiredRole]);
    }

    void inTransition()
    {
        npc.inTransition = true;
        int randomChance = UnityEngine.Random.Range(0, npc.MaxSatisfaccion);

        if (randomChance <= npc.molestia)
        {
            npc.isProblematic = true;

            switch (npc.role)
            {
                case NPCRole.Social:
                    npc.mood = NPCMood.Peleador;
                    break;
                case NPCRole.Bebedor:
                    npc.mood = NPCMood.Borracho;
                    break;
                case NPCRole.Bailador:
                    npc.mood = NPCMood.Euforico;
                    break;
            }

            print($"NPC se volvió problemático: {npc.mood}");

            DoProblematicRoutine();
        }
        else
        {
            Transform door = GameManager.Instance.GetExitDoor();
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveToPoint(door.position, DespawnMe));
        }
    }

     private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1) Si choca con el guardia (que impulsa usando física):
        var guard = collision.collider.GetComponent<GuardLaunch>();
        if (guard != null)
        {
            ReactToGuardHit(collision);
        }

        // 2) Si es la puerta al salir:
        if (collision.collider.CompareTag("Door"))
        {
            if (isGoingKicked) DespawnMe();
            else GenerateNewConsumer();
        }
    }

    private void ReactToGuardHit(Collision2D collision)
    {
        // --- LA FÍSICA ya la aplica el GuardLaunch (con AddForce/velocity) ---

        // 1) Aumentar molestia en un 5% de MaxSatisfaccion
        int delta = Mathf.CeilToInt(npc.MaxSatisfaccion * 0.05f);
        npc.molestia += delta;

        // 2) Aumentar número de veces molestado
        npc.nroSerMolestado++;

        // 3) Recalcular satisfaccion
        npc.satisfaccion = Mathf.Max(0, npc.MaxSatisfaccion - npc.molestia);

        // 4) (Opcional) Si baja de cierto umbral, pasarlo a problemático
        if (!npc.isProblematic && npc.satisfaccion <= npc.MaxSatisfaccion * 0.5f)
        {
            npc.isProblematic = true;
            npc.mood = NPCMood.Borracho; // o el que prefieras
            // Lanza animación, corrutina de rutina problemática, etc.
            DoProblematicRoutine();
        }

        // 5) Feedback visual o sonoro
        // e.g. animator.SetTrigger("HitByGuard");
    }

}
