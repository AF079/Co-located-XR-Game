using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    private float energy = 1f;
    private float mass = 1f;
    private float energyLoss = 0.05f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        energy = Mathf.Max(0,energy - energyLoss * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        float x = Mathf.Sqrt(2 * mass * energy);
        if(x < 1)
        {
            rb.velocity *= x;

        }
        Debug.Log(rb.velocity.magnitude + " " + energy + " " + x);

    }
}
