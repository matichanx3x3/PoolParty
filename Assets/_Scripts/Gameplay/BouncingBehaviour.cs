using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncingBehaviour : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private float forceTrasmited = 0.7f;
    [SerializeField] private float bounceLoseness = 0.6f;
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
            rb2D.velocity = reflected * lastVelocity.magnitude * bounceLoseness;

            SoundManager.Instance.PlayBump(0);
        }

        if (collision.collider.CompareTag("Clients"))
        {
            SoundManager.Instance.PlayBump(1);

            Rigidbody2D otherRb = collision.collider.GetComponent<Rigidbody2D>();

            if (otherRb != null)
            {
                Vector2 direction = (otherRb.position - rb2D.position).normalized;

                float transferredSpeed = lastVelocity.magnitude * forceTrasmited;

                otherRb.velocity = direction * transferredSpeed;

                rb2D.velocity = lastVelocity * 0.3f;

                SoundManager.Instance.PlayBump(1);
            }
        }

        if (collision.collider.CompareTag("Angry"))
        {
            SoundManager.Instance.PlayBump(2);

            Rigidbody2D otherRb = collision.collider.GetComponent<Rigidbody2D>();

            if (otherRb != null)
            {
                Vector2 direction = (otherRb.position - rb2D.position).normalized;

                float transferredSpeed = lastVelocity.magnitude * forceTrasmited;

                otherRb.velocity = direction * transferredSpeed;

                rb2D.velocity = lastVelocity * 0.3f;

                SoundManager.Instance.PlayBump(2);
            }
        }
    }
}