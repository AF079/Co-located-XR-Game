using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class SphereLatencyManager : NetworkBehaviour
{
    public GameObject SphereGenerator_obj;
    private List<GameObject> sphereList;
    private List<Vector3> delayedPositions;
    // Start is called before the first frame update
    private float baseLatency_ = 0.4f; //400
    private float jiter_max = 0.2f; //200

    private float jiter_min = 0.25f; //250ms
    private float pktLossProb = 0.5f; //5% , if random value > pktLossProb
    private bool delayDone = false;
    private Queue<List<Vector3>> delayQueue = new Queue<List<Vector3>>();
    private Queue<List<Vector3>> velocityQueue = new Queue<List<Vector3>>();

    public override void Spawned()
    {
        base.Spawned(); // Not required, but keeps compatibility

        delayedPositions = new List<Vector3>();
        Debug.Log("Is HOST " + Object.HasStateAuthority);
        if (Object.HasStateAuthority)
        {
            //StartCoroutine(WaitForSpheresToGenerate());
        }

    }

    private IEnumerator WaitForSpheresToGenerate()
    {
        while (!SphereGenerator.GENERATED)
        {
            yield return null;
        }
        sphereList = new List<GameObject>(SphereGenerator_obj.GetComponent<SphereGenerator>().sphereList);
        //Debug.Log("NUMBER OF SPEHRES " + sphereList.Count);

        StartCoroutine(SimulateNetwork());

    }


    // Update is called once per frame
    void FixedUpdate()
    {

        if (delayDone && delayQueue.Count > 0)
        {
            Debug.Log("Resetting positions");
            for (int i = 0; i < delayQueue.Peek().Count; i++)
            {
                Vector3 pos = sphereList[i].GetComponent<Rigidbody>().position;
                Vector3 velocity = velocityQueue.Peek()[i];
                //sphereList[i].GetComponent<Rigidbody>().velocity = velocity;
                sphereList[i].GetComponent<Rigidbody>().position = Vector3.Slerp(pos, delayQueue.Peek()[i], 5 * Time.fixedDeltaTime);
            }
            delayQueue.Dequeue();
            velocityQueue.Dequeue();

        }
    }

    private IEnumerator SimulateNetwork()
    {
        while (true)
        {
            float pktDelayTime = baseLatency_ + Random.Range(-jiter_min, jiter_max);
            delayedPositions = new List<Vector3>();
            List<Vector3> delayedVelocity = new List<Vector3>();
            if (Random.value < pktLossProb) // simulate packet loss by delayed positions 
            {
                Debug.Log("LATENCY " + pktDelayTime);
                delayDone = false;
                //Debug.Log("Pkt loss");
                foreach (GameObject sphere in sphereList)
                {
                    Rigidbody rb = sphere.GetComponent<Rigidbody>();
                    delayedPositions.Add(rb.position);
                    delayedVelocity.Add(rb.velocity);
                }
                delayQueue.Enqueue(delayedPositions);
                velocityQueue.Enqueue(delayedVelocity);

                Debug.Log("Done adding positions " + delayQueue.Count);
            }
            //Debug.Log("Waiting");
            yield return new WaitForSecondsRealtime(pktDelayTime);
            delayDone = delayedPositions.Count != 0;

            //Debug.Log("Done waiting");
        }
    }
}


