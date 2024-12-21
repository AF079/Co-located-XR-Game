using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonPhysics : MonoBehaviour
{
    private Rigidbody rb;
    public float velocity = 2f;
    bool stopMoving = false;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (stopMoving)
            rb.MovePosition(rb.position + Vector3.down * velocity * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        stopMoving = true;
    }
}
