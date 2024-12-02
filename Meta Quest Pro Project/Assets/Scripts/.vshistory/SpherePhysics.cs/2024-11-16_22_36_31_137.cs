using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    private Rigidbody rb;
    private float k_energy;
    private float mass;
    private float energyLoss = 1.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        k_energy = 1f;
        Debug.Log(rb.velocity.magnitude);
    }

    private void Update()
    {
        //Debug.Log(rb.velocity.magnitude + " K ENERGY " + k_energy);

        if(rb.velocity == Vector3.zero)
        {
            k_energy = 1f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {

        float x = Mathf.Exp(-k_energy);//Mathf.Sqrt(k_energy);
        rb.velocity *= x; 
        k_energy = Mathf.Max(0f, k_energy - energyLoss * Time.deltaTime);

    }
}
