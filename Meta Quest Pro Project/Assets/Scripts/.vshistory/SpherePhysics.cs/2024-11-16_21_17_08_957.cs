using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    private float energy = 1f;
    private float mass = 1f;
    private float energyLoss = 0.99f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        float x = Mathf.Sqrt(2 * mass * energy);
        if(x < 1)
        {
            rb.velocity *= Mathf.Max(1f,x);

        }
        energy *= energyLoss;
        Debug.Log(rb.velocity.magnitude);

    }
}
