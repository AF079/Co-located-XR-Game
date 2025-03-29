
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpherePhysics : MonoBehaviour
{

    public GameObject B;
    public float speed = 2.0f; // Adjust speed
    void Start()
    {

    }


    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, B.transform.position, speed * Time.unscaledDeltaTime);
    }
}