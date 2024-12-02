
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    private Rigidbody rb;
    private float k_energy;
    private float maxEnergy = 1f;
    private float dampingFactor = 0.98f; // Percentage of energy retained per bounce (90%)
    private float baseLatency = 0.5f;
    private Vector3 simulatedPosition;
    private Queue<Vector3> positionQueue = new Queue<Vector3>(); // Queue to simulate delayed packets
    private float packetLossProbability = 0.1f;
    private float jitterIntensity = 0.2f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        k_energy = maxEnergy;
        simulatedPosition = transform.position;
        StartCoroutine(NetworkSimulationCoroutine());

    }

    void Update()
    {
        // Reset energy if the sphere comes to a stop
        

        if (rb.velocity == Vector3.zero)
        {
            k_energy = maxEnergy;
        }
        
    }
    private void FixedUpdate()
    {
        rb.position = Vector3.Lerp(rb.position, simulatedPosition, Time.fixedDeltaTime * 5f);
        Debug.Log(rb.position.y);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Calculate the velocity multiplier based on current kinetic energy
        float x = Mathf.Sqrt(k_energy);
        rb.velocity *= x;

        // Retain a percentage of the energy for the next bounce
        k_energy = Mathf.Clamp(k_energy * dampingFactor, 0f, maxEnergy);

        //Debug.Log($"Velocity Magnitude: {rb.velocity.magnitude}, Kinetic Energy: {k_energy}, Retained Factor: {x}");
    }

    private IEnumerator NetworkSimulationCoroutine()
    {
        while (true)
        {
            // Simulate network latency and jitter
            yield return new WaitForSeconds(baseLatency + Random.Range(-jitterIntensity, jitterIntensity));

            if (Random.value > packetLossProbability) // Simulate packet loss
            {
                // Push current position to the simulated queue (mimicking a network packet)
                positionQueue.Enqueue(rb.position);
            }

            // Simulate delayed application of position updates
            if (positionQueue.Count > 0)
            {
                simulatedPosition = positionQueue.Dequeue();
            }
        }
    }

}
