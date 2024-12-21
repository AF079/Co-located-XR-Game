using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonPhysics : MonoBehaviour
{
    public float floatForce = 5f;      // Force to simulate balloon-like movement
    public float maxSpeed = 2f;       // Maximum speed to cap the balloon movement
    public float bounceMultiplier = 2f; // Extra bounce strength on collisions
    private Vector3 bounceDirection;
    private Rigidbody rb;
    private int maxHeight = 3f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 velocity = rb.velocity;

        // Compute the drag force magnitude using Unity's drag model
        float dragForceMagnitude = rb.mass * rb.drag * velocity.magnitude;

        // Direction of the drag force is opposite to the velocity
        Vector3 dragForce = -velocity.normalized * dragForceMagnitude;

        rb.AddForce(2 * dragForce.magnitude * bounceDirection * Time.fixedDeltaTime, ForceMode.Force);

    }
    void OnCollisionEnter(Collision collision)
    {
        // Boost the bounce effect on collision
        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];

            bounceDirection = Vector3.Reflect(rb.velocity.normalized, contact.normal);

       
            }

        }
    }

