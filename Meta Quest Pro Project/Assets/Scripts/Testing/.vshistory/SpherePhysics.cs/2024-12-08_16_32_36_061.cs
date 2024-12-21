
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    private Rigidbody rb;
    private float baseLatency = 0.05f;
    private Vector3 simulatedPosition;
    private Queue<Vector3> positionQueue = new Queue<Vector3>(); // Queue to simulate delayed packets
    private float packetLossProbability = 0.01f;
    private float jitterIntensity = 0.02f;
    private float maxHeight = 3f; // Starting maximum height
    private float newMaxHeight = 3f;
    Vector3 startForce;


    public float horizontalDamping = 0.98f; 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        simulatedPosition = transform.position;
        startForce = new Vector3(UnityEngine.Random.Range(-2, 2), 0, UnityEngine.Random.Range(-2, 2));
        rb.AddForce(startForce, ForceMode.Impulse);
        //StartCoroutine(NetworkSimulationCoroutine());

    }

    void Update()
    {
 
        
    }
    private void FixedUpdate()
    {
       if(rb.velocity.magnitude > 2)
        {
            rb.velocity = rb.velocity.normalized * 1.5f;
        }else if(rb.velocity.magnitude < -2)
        {
            rb.velocity = rb.velocity.normalized * -1.5f;
        }

        /*Vector3 clampedPosition = new Vector3(rb.position.x, Mathf.Min(rb.position.y, newMaxHeight), rb.position.z);
        rb.position = clampedPosition;

        //rb.position = Vector3.Lerp(rb.position, simulatedPosition, Time.fixedDeltaTime * 5f);
        rb.MovePosition(Vector3.Lerp(rb.position, simulatedPosition, Time.fixedDeltaTime * 5f));

        Debug.Log(rb.position.y);*/
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
