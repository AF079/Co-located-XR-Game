using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    private Rigidbody rb;
    private float k_energy;
    private float mass;
    private float energyLoss = 1.5f;
    private float maxEnergy = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        k_energy = maxEnergy;
        Debug.Log(rb.velocity.magnitude);
    }

    private void Update()
    {
        //

        if(rb.velocity == Vector3.zero)
        {
            k_energy = maxEnergy;
        }
    }

    void OnCollisionEnter(Collision collision)
    {

        float x = Mathf.Exp(1-k_energy);//Mathf.Sqrt(k_energy);
        rb.velocity *= x; 
        k_energy = Mathf.Max(0f, k_energy - energyLoss * Time.deltaTime);
        Debug.Log(rb.velocity.magnitude + " K ENERGY " + k_energy);

    }
}
