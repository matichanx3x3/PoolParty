using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingBehaviour : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb2D;
    private Vector2 lastVelocity;

    // Start is called before the first frame update
    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    // Update is called once per frame
    void Update()
    {

    }
    void FixedUpdate()
    {
        lastVelocity = rb2D.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Vector2 normal = collision.GetContact(0).normal;
            Vector2 reflected = Vector2.Reflect(lastVelocity.normalized, normal);
            rb2D.velocity = reflected * lastVelocity.magnitude;
        }
    }
}