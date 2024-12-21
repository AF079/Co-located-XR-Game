
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    private Rigidbody rb;
    private float k_energy;
    private float maxEnergy = 1f;
    private float dampingFactor = 0.98f; // Percentage of energy retained per bounce (90%)
    private float baseLatency = 0.05f;
    private Vector3 simulatedPosition;
    private Queue<Vector3> positionQueue = new Queue<Vector3>(); // Queue to simulate delayed packets
    private float packetLossProbability = 0.01f;
    private float jitterIntensity = 0.02f;
    private float maxHeight = 3f; // Starting maximum height
    private float newMaxHeight = 3f;
    float constantVelocityMagnitude = 2;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        k_energy = maxEnergy;
        simulatedPosition = transform.position;
        //StartCoroutine(NetworkSimulationCoroutine());

    }

    void Update()
    {
        // Reset energy if the sphere comes to a stop
        

        if (rb.velocity == Vector3.zero)
        {
            k_energy = maxEnergy;
            newMaxHeight = maxHeight;
        
        }
        
    }
    private void FixedUpdate()
    {
        /*if (rb.velocity != Vector3.zero)
        {
           rb.velocity = rb.velocity.normalized * constantVelocityMagnitude;
        }*/

        rb.velocity = rb.velocity.normalized
        Debug.Log(rb.velocity.normalized * constantVelocityMagnitude + " " + rb.velocity.normalized);

        /*Vector3 clampedPosition = new Vector3(rb.position.x, Mathf.Min(rb.position.y, newMaxHeight), rb.position.z);
        rb.position = clampedPosition;

        //rb.position = Vector3.Lerp(rb.position, simulatedPosition, Time.fixedDeltaTime * 5f);
        rb.MovePosition(Vector3.Lerp(rb.position, simulatedPosition, Time.fixedDeltaTime * 5f));

        Debug.Log(rb.position.y);*/
    }

    void OnCollisionEnter(Collision collision)
    {
        // Calculate the velocity multiplier based on current kinetic energy
        /*float x = Mathf.Sqrt(k_energy);
        rb.velocity *= x;

        // Retain a percentage of the energy for the next bounce
        k_energy = Mathf.Clamp(k_energy * dampingFactor, 0f, maxEnergy);

        newMaxHeight = Mathf.Max(0f, newMaxHeight * dampingFactor);*/
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
                Vector3 pos = positionQueue.Dequeue();

                simulatedPosition = new Vector3(pos.x, Mathf.Min(pos.y, newMaxHeight), pos.z);
            }
        }
    }

}
