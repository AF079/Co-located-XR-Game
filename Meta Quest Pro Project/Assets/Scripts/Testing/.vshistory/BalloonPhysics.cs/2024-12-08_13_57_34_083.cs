using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonPhysics : MonoBehaviour
{
    private Rigidbody rb;
    public float velocity = 2f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.position = velocity * Time.fixedDeltaTime * rb.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }
}
