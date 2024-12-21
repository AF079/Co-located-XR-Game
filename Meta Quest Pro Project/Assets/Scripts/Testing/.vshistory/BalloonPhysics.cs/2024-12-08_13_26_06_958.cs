using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonPhysics : MonoBehaviour
{
    public float floatForce = 5f;      // Force to simulate balloon-like movement
    public float maxSpeed = 2f;       // Maximum speed to cap the balloon movement
    public float bounceMultiplier = 2f; // Extra bounce strength on collisions

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Apply a constant upward force to simulate floatiness
        /*if (rb.velocity.y < maxSpeed)
        {
            rb.AddForce(Vector3.up * floatForce, ForceMode.Acceleration);
        }

        // Cap the horizontal speed to keep it floaty
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            rb.velocity = horizontalVelocity.normalized * maxSpeed + new Vector3(0, rb.velocity.y, 0);
        }*/
    }

    void OnCollisionEnter(Collision collision)
    {
        // Boost the bounce effect on collision
        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            Debug.Log(collision.contacts.Length);
            Vector3 bounceDirection = Vector3.Reflect(rb.velocity, contact.normal);

            // Scale the bounce force for exaggerated basketball behavior
            rb.velocity = bounceDirection * bounceMultiplier;
        }
    }
}
