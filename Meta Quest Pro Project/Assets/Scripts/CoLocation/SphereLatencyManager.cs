using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;


public class SphereLatencyManager : NetworkBehaviour
{
    // public GameObject SphereGenerator_obj;
    // private List<GameObject> sphereList;
    // // private List<Vector3> delayedPositions;
    // // Start is called before the first frame update
    // private float baseLatency_ = 0.4f;
    // private float jiter_max = 0.3f;

    // private float jiter_min = 0.3f;
    // private float pktLossProb = 0.3f;

    // public static float PKT_LATENCY = 0f;

    // private Queue<Data> delayQueue = new Queue<Data>();

    // public static Stack<float> packetDelayTimes = new Stack<float>();

    // class InterpData
    // {
    //     public Vector3 delayedPosition;
    //     public float interpolateProgress = 0f;
    //     public bool doneInterpolate = false;
    // }
    // class Data
    // {
    //     public List<InterpData> delayedPositions;
    //     public bool delayDone = false;
    //     public bool dataIsDelayed = false;

    // }

    // public override void Spawned()
    // {
    //     base.Spawned(); // Not required, but keeps compatibility

    //     // delayedPositions = new List<Vector3>();
    //     Debug.Log("Is HOST " + Object.HasStateAuthority);
    //     // if (!Object.HasStateAuthority)
    //     // {
    //     // }
    //     StartCoroutine(WaitForSpheresToGenerate());

    // }

    // private IEnumerator WaitForSpheresToGenerate()
    // {
    //     while (!SphereGenerator.GENERATED)
    //     {
    //         yield return null;
    //     }
    //     sphereList = new List<GameObject>(SphereGenerator_obj.GetComponent<SphereGenerator>().sphereList);
    //     //Debug.Log("NUMBER OF SPEHRES " + sphereList.Count);

    //     //StartCoroutine(SimulateNetwork());

    // }


    // // Update is called once per frame
    // void Update()
    // {

    //     if (delayQueue.Count > 0)
    //     {
    //         Data peek = delayQueue.Peek();
    //         if (peek.delayDone)
    //         {
    //             Debug.Log("Resetting positions " + delayQueue.Count + " " + peek.delayedPositions.Count);
    //             for (int i = 0; i < peek.delayedPositions.Count; i++)
    //             {
    //                 InterpData interpData = peek.delayedPositions[i];

    //                 if (interpData.doneInterpolate) continue;

    //                 interpData.interpolateProgress += 2 * Time.unscaledDeltaTime;

    //                 Vector3 target = interpData.delayedPosition;

    //                 Rigidbody rb = sphereList[i].GetComponent<Rigidbody>();

    //                 Vector3 newPos = Vector3.Lerp(rb.transform.position, target, interpData.interpolateProgress);

    //                 rb.MovePosition(newPos);

    //                 Debug.Log("Moving " + interpData.interpolateProgress);

    //                 if (Vector3.Distance(newPos, target) <= 0.01f)
    //                 {
    //                     interpData.doneInterpolate = true;
    //                     interpData.interpolateProgress = 0f;
    //                 }

    //             }
    //             if (peek.delayedPositions.All(x => x.doneInterpolate))
    //             {
    //                 foreach (var y in peek.delayedPositions)
    //                 {
    //                     y.doneInterpolate = false;
    //                 }
    //                 delayQueue.Dequeue();
    //             }

    //         }
    //     }
    // }
    // /*
    // - Chance of packet loss => [1,2,3,4,5] -> [1,4,5]
    // - Chance of out of order packets -> [1,2,3,4,5] -> [3,2,1,4,5]
    // - Chance of jitter -> L = D + Random(-J,J)
    // - Always delay by min(D,L)

    // - If no new packet has arrived D ms (max tolerable delay for minimum functioning system) 
    //     => Extrapolate position 
    // - If a packet arrives holding position (x,y)
    //     => Lerp(Current, (x,y))

    // */


    // private IEnumerator SimulateNetwork()
    // {
    //     while (true)
    //     {
    //         Debug.Log(delayQueue.Count);
    //         float pktDelayTime = baseLatency_ + Random.Range(-jiter_min, jiter_max);

    //         Data data = new Data();
    //         data.delayedPositions = new List<InterpData>();
    //         data.delayDone = false;
    //         if (Random.value < pktLossProb) // simulate packet loss by delayed positions 
    //         {
    //             data.dataIsDelayed = true;
    //             Debug.Log("LATENCY " + pktDelayTime);

    //             //Debug.Log("Pkt loss");
    //             foreach (GameObject sphere in sphereList)
    //             {
    //                 InterpData id = new InterpData();
    //                 Rigidbody rb = sphere.GetComponent<Rigidbody>();
    //                 id.doneInterpolate = false;
    //                 id.interpolateProgress = 0f;
    //                 id.delayedPosition = rb.position;
    //                 data.delayedPositions.Add(id);
    //             }
    //             delayQueue.Enqueue(data);

    //             Debug.Log("Done adding positions " + delayQueue.Count);
    //         }
    //         //Debug.Log("Waiting");
    //         yield return new WaitForSecondsRealtime(pktDelayTime);

    //         //delayDone = delayedPositions.Count != 0;
    //         if (data.dataIsDelayed)
    //         {
    //             data.delayDone = true;
    //             packetDelayTimes.Push(pktDelayTime);
    //             //Debug.Log("Packet delay times " + packetDelayTimes + " " + packetDelayTimes.Count);
    //         }
    //         //Debug.Log("Done waiting");
    //     }
    // }
}


