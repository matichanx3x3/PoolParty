using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTest : MonoBehaviour
{
    Vector2[] directions = new Vector2[]

{
    new Vector2(0, 1),   // Arriba
    new Vector2(0, -1),  // Abajo
    new Vector2(1, 0),   // Derecha
    new Vector2(-1, 0)   // Izquierda
};
    [SerializeField] float speed = 3f;
    [HideInInspector] public bool canMove = true;
    private bool ismoving;
    private Vector2 movingDirection;

    void Start()
    {

    }

    void Update()
    {
        if (!canMove)
        {
            return;
        }

        if (!ismoving)
        {
            movingDirection = GetDirection(); //Pila una dirección aleatoria
        }
        else if (!Physics2D.Raycast(transform.position, movingDirection, 1f))
        {
            transform.Translate(movingDirection * Time.deltaTime * speed); //si no hay colisión, mueve al segurata en esa dirección
        }
        else
        {
            Debug.Log("Blocked in direction: " + movingDirection); //Si hay colisión, no se mueve y se reinicia el movimiento
            ResetMovement();
            return;
        }
    }

    Vector2 GetDirection()
    {
        ismoving = true;
        int randomDirection = Random.Range(0, directions.Length);
        Vector2 direction = directions[randomDirection];
        Debug.Log("Moving in direction: " + direction);
        return direction;
    }

    public void ResetMovement()
    {
        StartCoroutine(ResetMovementCoroutine()); //Resetea el movimiento después de unos segundos para que resulte más orgánico
    }

    IEnumerator ResetMovementCoroutine()
    {
        yield return new WaitForSeconds(1.5f); // Espera 1 segundo antes de reiniciar el movimiento
        ismoving = false;
        canMove = true;
    }

    public void StopCoroutines()
    {
        StopAllCoroutines(); // Detiene todas las corutinas activas
        ismoving = false; // Reinicia el estado de movimiento
        canMove = false; // Desactiva el movimiento
    }
    
    
}
