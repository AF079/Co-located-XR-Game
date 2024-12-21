using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonPhysics : MonoBehaviour
{
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        // Calculate the velocity multiplier based on current kinetic energy
        float x = Mathf.Sqrt(k_energy);
        rb.velocity *= x;

        // Retain a percentage of the energy for the next bounce
        k_energy = Mathf.Clamp(k_energy * dampingFactor, 0f, maxEnergy);

        newMaxHeight = Mathf.Max(0f, newMaxHeight * dampingFactor);
    }
}
