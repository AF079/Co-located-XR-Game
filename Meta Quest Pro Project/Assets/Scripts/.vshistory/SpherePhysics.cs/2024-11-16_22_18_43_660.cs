using UnityEngine;

public class SpherePhysics : MonoBehaviour
{
    private Rigidbody rb;
    private float k_energy; // Kinetic energy
    private float p_energy; // Potential energy
    private float mass = 1f;
    private float energyLoss = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Initialize energy only once
        k_energy = 1f; // Starting kinetic energy
        p_energy = 0f; // Starting potential energy
    }

    void OnCollisionEnter(Collision collision)
    {
        // Swap energy states if potential energy is fully converted
       if(rb.velocity.magnitude <= 0.2f)
        {
            rb.velocity = Vector3.zero;
            k_energy = 0f;
        }
       if(k_energy <= 0f)
        {
            k_energy = 1f;
        }

        // Calculate velocity from kinetic energy
        float x = Mathf.Sqrt(2 * mass * k_energy);
        if (x < 1)
        {
            rb.velocity *= x; // Adjust velocity if kinetic energy is low
        }

        // Update energies
        k_energy = Mathf.Max(0f, k_energy - energyLoss * Time.deltaTime);
        //p_energy = Mathf.Min(1f, p_energy + energyLoss * Time.deltaTime);

        Debug.Log(rb.velocity.magnitude + " K ENERGY " + k_energy + " " + x);
    }
}
