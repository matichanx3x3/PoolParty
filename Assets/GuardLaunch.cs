using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardLaunch : MonoBehaviour
{
    [Header("CurrentValues/Stats")]
    [Space]
    [SerializeField, ReadOnly] private float currentForce;
    [Space]
    [SerializeField, ReadOnly] private float currentSpeed;
    [Space]
    [SerializeField, ReadOnly] private Vector3 mouseWorldPosition;
    [ReadOnly] public float currentForceToPush;

    [Header("Launch Force Stats")]
    [SerializeField] private float maxDistanceToApplyMaxForce = 10;
    [SerializeField] private float minDistanceToLaunch = 0.2f;
    [SerializeField] private float maxForceToLaunch = 10;
    [SerializeField] private float friccion;

    [Header("Pushing Stats")]
    

    [Header("States")]
    [SerializeField, ReadOnly] private bool isDragged;

    [Header("IA Related")]
    [SerializeField] private float timeToRestartBehaviour;

    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private GameObject flecha;
    private Vector2 lastVelocity;

    // Start is called before the first frame update
    void Start()
    {
        flecha.SetActive(false);
        rb2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragged)
        {
            RegisterMousePosition();
        }
    }

    void FixedUpdate()
    {
        lastVelocity = rb2D.velocity;
    }

    private void OnMouseDown()
    {
        isDragged = true;
    }

    private void RegisterMousePosition()
    {
        mouseWorldPosition = InputManager.Instance.cameraUsed.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;

        flecha.SetActive(true);
        Vector2 posToLookAt = mouseWorldPosition - this.transform.position;
        float transformRotateZ = MathF.Atan2(posToLookAt.x, posToLookAt.y) * Mathf.Rad2Deg;
        //flecha.transform.rotation = Quaternion.Euler(0, 0, -transformRotateZ);
        flecha.transform.rotation = Quaternion.Euler(0, 0, -transformRotateZ);
    }

    private void OnMouseUp()
    {
        isDragged = false;
        flecha.SetActive(false);

        LaunchGuard();
    }

    private void LaunchGuard()
    {
        Debug.Log(mouseWorldPosition);

        Vector3 finalPosition = this.transform.position - mouseWorldPosition;
        Debug.Log(finalPosition);

        finalPosition.z = 0;

        currentForce = CalculateCurrentForce();

        rb2D.AddForce((finalPosition * currentForce), ForceMode2D.Impulse);
    }

    private float CalculateCurrentForce()
    {
        float distance = Vector3.Distance(this.transform.position,mouseWorldPosition);

        if(distance < minDistanceToLaunch)
        {
            distance = 0;
        }
        if (distance > maxDistanceToApplyMaxForce)
        {
            distance = maxDistanceToApplyMaxForce;
        }
        
        float forceToApply = distance * maxForceToLaunch;

        return forceToApply;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Vector2 normal = collision.GetContact(0).normal;
            Vector2 reflected = Vector2.Reflect(lastVelocity.normalized, normal);
            rb2D.velocity = reflected * lastVelocity.magnitude;
        }


        if (collision.collider.CompareTag("Clients"))
        {
            Rigidbody2D otherRb = collision.collider.GetComponent<Rigidbody2D>();

            if (otherRb != null)
            {
                Vector2 direction = (otherRb.position - rb2D.position).normalized;

                float transferredSpeed = lastVelocity.magnitude * 0.8f; 

                otherRb.velocity = direction * transferredSpeed;

                rb2D.velocity = lastVelocity * 0.2f;
            }
        }
    }
}
