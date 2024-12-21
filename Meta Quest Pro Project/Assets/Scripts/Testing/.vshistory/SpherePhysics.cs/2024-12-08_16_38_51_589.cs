
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    private Rigidbody rb;
    Vector3 startForce;
    public float speed = 5f; // Fixed speed of the ball
    private Vector3 direction; // Current direction of the bal

    public float horizontalDamping = 0.98f; 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startForce = new Vector3(UnityEngine.Random.Range(-2, 2), 0, UnityEngine.Random.Range(-2, 2));

        direction = GetRandomDirection();
    }

    void Update()
    {
        // Move the ball at a constant speed
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Reflect the ball's direction based on the collision normal
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal;
        direction = Vector3.Reflect(direction, normal).normalized;
    }

    private Vector3 GetRandomDirection()
    {
        // Generate a random initial direction (avoiding vertical or horizontal)
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        float z = Random.Range(-1f, 1f);
        return new Vector3(x, y, z).normalized; // Normalized to maintain constant speed
    }
   

}
