/*using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    private Rigidbody rb;
    private float k_energy;
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

        float x = Mathf.Sqrt(k_energy * 0.8f);//Mathf.Sqrt(k_energy);
        rb.velocity *= x; 
        k_energy = Mathf.Max(0f, k_energy - energyLoss * Time.deltaTime);
        Debug.Log(rb.velocity.magnitude + " K ENERGY " + k_energy + " x " + x);

    }
}
*/

using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    private Rigidbody rb;
    private float k_energy;
    private float maxEnergy = 1f;
    private float dampingFactor = 1f; // Percentage of energy retained per bounce (90%)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        k_energy = maxEnergy;
    }

    private void Update()
    {
        // Reset energy if the sphere comes to a stop
        if (rb.velocity == Vector3.zero)
        {
            k_energy = maxEnergy;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Calculate the velocity multiplier based on current kinetic energy
        float x = Mathf.Sqrt(k_energy);
        rb.velocity *= x;

        // Retain a percentage of the energy for the next bounce
        k_energy = Mathf.Clamp(k_energy * dampingFactor, 0f, maxEnergy);

        Debug.Log($"Velocity Magnitude: {rb.velocity.magnitude}, Kinetic Energy: {k_energy}, Retained Factor: {x}");
    }
}
