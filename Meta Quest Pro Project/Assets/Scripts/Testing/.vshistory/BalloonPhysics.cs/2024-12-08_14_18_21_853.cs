using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonPhysics : MonoBehaviour
{
    private Rigidbody rb;
    public float velocity = 2f;
    bool stopMoving = false;
    int direction = 1;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        rb.MovePosition(rb.position + direction * Vector3.one * velocity * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        direction *= -1;
    }
    void OnCollisionExit(Collision collision)
    {
        
    }
}
