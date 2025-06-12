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
        partyMaxTimer = 10;
        partyActualTimer = 0;
        nroSerMolestado = 0;
        inTransition = false;
    }
}

public class NPCBehavior : MonoBehaviour
{
    [HideInInspector]
    public ZonePoint currentZonePoint;
    
    public GameObject target;
    public NPC npc;
    public static event Action<NPCBehavior> OnNPCRequestDespawn;
    [SerializeField] Transform directionToMove;
    [HideInInspector] public bool canMove = true; // Indicates if the NPC can move
    bool busy = false;
    private bool hasDespawned = false;
    [SerializeField] bool isGoingKicked = false;

    public float moveSpeed = 2f;
    private Coroutine moveCoroutine;
    private Coroutine timerCorutine;

    public Rigidbody2D rb;
    public Collider2D col2D;
    public SpriteRenderer actualEye;
    public List<Sprite> eyeList;

    [SerializeField] private float normalRoutineInterval = 5f;
    [SerializeField] private float problematicRoutineInterval = 7f;

    public void AssignZonePoint(ZonePoint zp)
    {
        currentZonePoint = zp;
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
        Transform assignedPoint = GameManager.Instance.AssignZoneToNPC(this);
        if (assignedPoint == null)
        {
            Debug.LogWarning("No se pudo asignar punto a NPC");
            return;
        }
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
        int roll = UnityEngine.Random.Range(0, 100);
        if (roll < 80)
        {
            // 80% rutina normal
            npc.isProblematic = false;
            npc.mood = NPCMood.Normal;
            Debug.Log($"NPC {npc.role} hace su rutina normal");
        }
        else
        {
            // 20% se vuelve problemático
            MarkAsProblematic();
        }
        col2D.isTrigger = false;

        if (!npc.isProblematic)
            Timer();
        
        StartCoroutine(RoutineLoop());
    }

    private IEnumerator RoutineLoop()
    {
        while (true)
        {
            if (npc.isProblematic)
            {
                DoProblematicRoutine();
                yield return new WaitForSeconds(problematicRoutineInterval);
            }
            else
            {
                DoNormalRoutine();
                yield return new WaitForSeconds(normalRoutineInterval);
            }
        }
    }

    public void AddMolestiaPoints()
    {
        npc.molestia += 5;
        npc.satisfaccion = npc.MaxSatisfaccion - npc.molestia;

        if (!npc.isProblematic && npc.nroSerMolestado >= 3)
        {
            inTransition();
        }
    }

    public void AddSerMolestado()
    {
        npc.nroSerMolestado++;
        AddMolestiaPoints();
    }

    private void DoNormalRoutine()
    {
        // Aquí defines la rutina estándar según el role
        switch (npc.role)
        {
            case NPCRole.Social:
                // charlar o moverse a un punto de reunión
                print("hago social");
                break;
            case NPCRole.Bebedor:
                // animación de beber en bar
                print("hago beber");
                break;
            case NPCRole.Bailador:
                // animación de baile en pista
                print("hago bailar");
                break;
        }
    }

    public void DoProblematicRoutine()
    {
        isGoingKicked = true;
        this.gameObject.tag = "Angry";
        actualEye.sprite = eyeList[2];

        switch (npc.mood)
        {
            case NPCMood.Peleador:
                print("hago pelea");
                LaunchAtNearestClient();
                break;
            case NPCMood.Borracho:
                print("hago borracho");
                StaggerInPlace();
                break;
            case NPCMood.Euforico:
                print("hago euforico");
                SpinAndMoveRandomly();
                break;
        }
    }

    private void LaunchAtNearestClient()
    {
        RaycastHit2D[] outHit =  Physics2D.CircleCastAll(this.transform.position,10f,Vector2.zero);

        target = null;
        if(outHit.Length > 0 && outHit != null)
        {
            for (int i = 0; i < outHit.Length; i++)
            {
                if (outHit[i].transform.CompareTag("Clients"))
                {
                    target = outHit[i].transform.gameObject;
                    break;
                }
                target = null;
            }
        }

        if(target != null)
        {
            Debug.Log("Lanzandose pt 2");
            rb.AddForce((target.transform.position - this.transform.position).normalized * 4, ForceMode2D.Impulse);
        }
    }

    private void StaggerInPlace()
    {
        Vector2 randomDir = UnityEngine.Random.insideUnitCircle.normalized;
        rb.AddForce(randomDir * 2f, ForceMode2D.Impulse);
        Debug.Log("Borracho se tambalea");
    }

    private void SpinAndMoveRandomly()
    {
        float angle = UnityEngine.Random.Range(0f, 360f);
        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        rb.AddTorque(10f, ForceMode2D.Impulse);
        rb.AddForce(dir * 3f, ForceMode2D.Impulse);
        Debug.Log("Eufórico gira y explota de energía");
    }
    public void Timer()
    {
        timerCorutine = StartCoroutine(DoTimer());
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
        StartCoroutine(ResumeAfterDelay(2f));
    }

    private IEnumerator ResumeAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;
        canMove = false;
        yield return new WaitForSeconds(delay);

        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        canMove = true;

        ContinueTransition();
    }


    void DespawnMe()
    {
        if (hasDespawned) 
            return;
        hasDespawned = true;
        StopAllCoroutines();
        OnNPCRequestDespawn?.Invoke(this); 
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
        ResetMovement(); // Detiene y luego reanudará con ContinueTransition()
    }

    void ContinueTransition()
    {
        int randomChance = UnityEngine.Random.Range(0, npc.MaxSatisfaccion);

        if (randomChance <= npc.molestia)
        {
            MarkAsProblematic();
        }
        else
        {
            npc.isProblematic = false;
            isGoingKicked = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
            Transform door = GameManager.Instance.GetExitDoor();

            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            isGoingKicked = true;
            moveCoroutine = StartCoroutine(MoveToPoint(door.position, DespawnMe));
        }

        npc.inTransition = false;
    }

    private void MarkAsProblematic()
    {
        npc.isProblematic = true;
        isGoingKicked = true;
        transform.Find("Borde").GetComponent<SpriteRenderer>().color = Color.red;
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
        if (timerCorutine != null)
            StopCoroutine(timerCorutine);
        GameManager.Instance.noProblematicAforo--; 
        GameManager.Instance.problematicAforo++;
        print($"NPC se volvió problemático: {npc.mood}");
        StartCoroutine(RoutineLoop());
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {

        var guard = collision.collider.GetComponent<GuardLaunch>();
        if (guard != null)
        {
            if (!npc.isProblematic)
                ReactToGuardHit(collision);
        }

        if (collision.collider.CompareTag("Door")&& isGoingKicked && currentZonePoint != null)
        {
            if (isGoingKicked) DespawnMe();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        {
            if (collision.transform.CompareTag("Door")&& isGoingKicked && currentZonePoint != null)
            {
                DespawnMe();
            }
        }
    }

    private void ReactToGuardHit(Collision2D collision)
    {
        if (!npc.isProblematic)
            AddSerMolestado();
    }

}
