using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    public static event Action<NPCBehavior> OnNPCRequestDespawn;
    [SerializeField] List<Transform> directions = new List<Transform>();
    [HideInInspector] public bool canMove = true; // Indicates if the NPC can move
    bool busy = false;

    void Start()
    {

    }
    void Update()
    {
        if (!canMove)
        {
            return;
        }
        if (!busy)
        {
            StartCoroutine(MoveRandomly());
        }
    }

    IEnumerator MoveRandomly()
    {
        busy = true;

        Transform target = GetRandomDirection();
        if (target == null)
        {
            busy = false;
            yield break;
        }

        while ((target.position - transform.position).sqrMagnitude > 0.1f)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Vector3 rayOrigin = transform.position + direction * 0.8f;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, 0.5f);
            Debug.DrawRay(rayOrigin, direction * 1.4f, Color.red, 0.1f);

            if (hit.collider != null && !hit.collider.CompareTag("Door"))
            {
                Debug.Log("Blocked by: " + hit.collider.name);
                busy = false;
                yield break; // Detenemos el movimiento porque hay un obstáculo
            }

            transform.position += direction * Time.deltaTime * 3f;
            yield return null; // Esperar al siguiente frame
        }

        Debug.Log("Llegó al destino, esperando...");
        yield return new WaitForSeconds(5f);
        busy = false;
    }

    private Transform GetRandomDirection()
    {
        if (directions.Count == 0)
        {
            return null;
        }
        int randomIndex = UnityEngine.Random.Range(0, directions.Count);
        Debug.Log("Direction chosen");
        return directions[randomIndex];
    }
    public void ResetMovement()
    {
        StartCoroutine(ResetMovementCoroutine());
    }

    private IEnumerator ResetMovementCoroutine()
    {
        yield return new WaitForSeconds(20f); 
        canMove = true;
        busy = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Door"))
        {
            DespawnMe();
        }
    }

    void DespawnMe()
    {
        OnNPCRequestDespawn?.Invoke(this); // Lanza el evento
    }

}
