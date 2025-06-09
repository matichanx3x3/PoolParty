using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
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
    [SerializeField, Range(0,1)] private float forceTrasmited = 0.7f;

    [Header("States")]
    [SerializeField, ReadOnly] private bool isDragged;

    [Header("IA Related")]
    [SerializeField] private float timeToRestartBehaviour;

    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private GameObject flecha;
    private Vector2 lastVelocity;

    // Start is called before the first frame update
    void Awake()
    {
        flecha.SetActive(false);
        rb2D = GetComponent<Rigidbody2D>();
        rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  
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

        float scale = CalculateCurrentForce() * maxForceToLaunch / 50 ;

        if(scale <= 2)
        {
            scale = 2;
        }

        flecha.transform.localScale = new Vector3 (flecha.transform.localScale.x,scale, flecha.transform.localScale.z);

        flecha.SetActive(true);

        //ROTATION-----------------------------------------------------------------------
        Vector2 posToLookAt = mouseWorldPosition - this.transform.position;
        float transformRotateZ = MathF.Atan2(posToLookAt.x, posToLookAt.y) * Mathf.Rad2Deg;
        //flecha.transform.rotation = Quaternion.Euler(0, 0, -transformRotateZ);
        flecha.transform.rotation = Quaternion.Euler(0, 0, -(transformRotateZ + 180f));
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

        Debug.Log(finalPosition.normalized * currentForce);

        rb2D.AddForce((finalPosition.normalized * currentForce), ForceMode2D.Impulse);
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

                float transferredSpeed = lastVelocity.magnitude * forceTrasmited; 

                otherRb.velocity = direction * transferredSpeed;

                rb2D.velocity = lastVelocity * 0.3f;
            }
        }
    }
}
