using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    private float k_energy = 1f;
    private float p_energy = 0f;
    private float mass = 1f;
    private float energyLoss = 0.5f;
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
        if (p_energy == 1)
        {
            k_energy = 1f;
            p_energy = 0f;
        }
        float x = Mathf.Sqrt(2 * mass * k_energy);
        if(x < 1)
        {
            rb.velocity *= x;

        }
        Debug.Log(rb.velocity.magnitude + " K ENERGY " + k_energy + " " + x + " P ENERGY " + p_energy);
        k_energy = Mathf.Max(0, k_energy - energyLoss * Time.deltaTime);
        p_energy += Mathf.Max(1,energyLoss * Time.deltaTime);
    }
}
