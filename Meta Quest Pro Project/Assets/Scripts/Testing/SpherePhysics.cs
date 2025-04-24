
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpherePhysics : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 acceleration;
    public float gravity = -9.81f;
    public float restitution = 0.965f;
    public float mass = 1.0f;

    void Start()
    {
        AddForce(new Vector3(Random.Range(-50, 50), 0, Random.Range(-50, 50)));
    }
    private void Update()
    {
        float dt = Time.deltaTime;

        // Apply gravity
        acceleration += new Vector3(0f, gravity, 0f);

        velocity += acceleration * dt;

        transform.position += velocity * dt;

        acceleration = Vector3.zero;

    }

    private void OnCollisionEnter(Collision collision)
    {
        ResolveCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        ResolveCollision(collision);
    }

    // private void ResolveCollision(Collision collision)
    // {
    //     ContactPoint contact = collision.contacts[0];

    //     Vector3 normal = contact.normal;
    //     float incomingSpeed = velocity.magnitude;

    //     // Perfect energy-preserving reflection
    //     velocity = Vector3.Reflect(velocity, normal).normalized * incomingSpeed;

    //     // Use ComputePenetration to fix overlap
    //     // Collider thisCol = GetComponent<Collider>();
    //     // Collider otherCol = collision.collider;

    //     // if (Physics.ComputePenetration(
    //     //     thisCol, transform.position, transform.rotation,
    //     //     otherCol, otherCol.transform.position, otherCol.transform.rotation,
    //     //     out Vector3 direction, out float distance))
    //     // {
    //     //     // Push object out of penetration
    //     //     transform.position += direction * (distance + 0.001f); // Add epsilon to prevent re-penetration
    //     // }
    // }

    private void ResolveCollision(Collision collision)
    {
        Rigidbody otherRb = collision.rigidbody;
        if (otherRb == null)
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 n = contact.normal;
            float incomingSpeed = velocity.magnitude;

            // Perfect energy-preserving reflection
            velocity = Vector3.Reflect(velocity, n);
            return;
        }

        SpherePhysics other = otherRb.GetComponent<SpherePhysics>();
        if (other == null) return;

        // Positions and velocities
        Vector3 posA = transform.position;
        Vector3 posB = other.transform.position;

        Vector3 velA = velocity;
        Vector3 velB = other.velocity;

        // Normalized direction from B to A
        Vector3 normal = (posA - posB).normalized;

        // Relative velocity
        Vector3 relVel = velA - velB;

        // Project velocity onto the normal
        float velAlongNormal = Vector3.Dot(relVel, normal);
        if (velAlongNormal > 0f) return; // They are moving apart

        // Coefficient of restitution (min of both)
        float e = Mathf.Min(restitution, other.restitution);

        // Impulse scalar
        float j = -(1 + e) * velAlongNormal / 2f; // equal mass simplification

        Vector3 impulse = j * normal;

        // Apply impulse to both
        velocity -= impulse * 0.1f;
        other.velocity += impulse * 0.1f;
    }

    public void AddForce(Vector3 force)
    {
        acceleration += force / mass;
    }
}