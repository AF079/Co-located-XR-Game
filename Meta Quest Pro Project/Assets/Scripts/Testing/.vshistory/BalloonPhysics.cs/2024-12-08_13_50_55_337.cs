using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonPhysics : MonoBehaviour
{
    public float floatForce = 5f;          // Upward force to simulate buoyancy
    public float movementForce = 2f;      // Random horizontal movement force
    public float bounceMultiplier = 1.1f; // Multiplier for bounce velocity to maintain energy

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Apply a constant upward force to simulate buoyancy
        ApplyBuoyancy();

        // Apply a small random force for movement
        ApplyRandomHorizontalForce();
    }

    void OnCollisionEnter(Collision collision)
    {
        // When the sphere collides with a surface, amplify its velocity slightly to avoid energy loss
        Vector3 velocity = rb.velocity;

        // Reflect the velocity and multiply it slightly
        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;

            // Calculate the reflected velocity
            Vector3 reflectedVelocity = Vector3.Reflect(velocity, normal) * bounceMultiplier;

            // Apply the new velocity to maintain constant energy
            rb.velocity = reflectedVelocity;
        }
    }

    void ApplyBuoyancy()
    {
        // Apply a constant upward force
        rb.AddForce(Vector3.up * floatForce, ForceMode.Force);
    }

    void ApplyRandomHorizontalForce()
    {
        // Random horizontal direction
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        // Apply a small random force to the sphere
        rb.AddForce(randomDirection * movementForce, ForceMode.Force);
    }
}

