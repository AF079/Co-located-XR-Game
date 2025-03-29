using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SphereController : NetworkBehaviour
{
    private Dictionary<string, double> userPressTimestamps = new Dictionary<string, double>();
    string myName;
    bool isTouching = false;
    int REQ_NUM_INTERACTIONS = 2;

    void Start()
    {
        myName = "user" + Random.Range(1, 1000);
        Debug.Log("IN SPHERE CONTROLLER " + myName);
    }

    // public override void Spawned()
    // {
    //     base.Spawned(); // Not required, but keeps compatibility

    // }

    public void RequestOwnership()
    {
        if (Object == null || Runner == null)
            return;
        if (Object != null && !Object.HasStateAuthority)
        {
            Object.RequestStateAuthority();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Object == null || Runner == null)
            return;
        //When user touches the sphere 
        if (!other.CompareTag("sphere"))
        {
            isTouching = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (Object == null || Runner == null)
            return;
        if (isTouching && !other.CompareTag("sphere"))
        {
            Debug.Log("INTERACTING!");
            if (Object.HasStateAuthority)
            {
                double timeStamp = Runner.SimulationTime;

                RPC_updateUserPressTimestamps(myName, timeStamp);
            }
            else
            {
                RequestOwnership();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Object == null || Runner == null)
            return;
        if (!other.CompareTag("sphere"))
        {
            isTouching = false;
            Debug.Log("STOPPED INTERACTING!");

        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_updateUserPressTimestamps(string name, double timeStamp)
    {

        if (userPressTimestamps.ContainsKey(name))
        {
            userPressTimestamps[name] = timeStamp;
        }
        //otherwise add the name and timestamp
        else
        {
            userPressTimestamps.Add(name, timeStamp);
        }
        //If there are two interactions: {(name1,time1),(name2,time2)}
        if (userPressTimestamps.Count >= REQ_NUM_INTERACTIONS)
        {
            //Only one user should disable the sphere. The user who did not initiate the RPC call will disable the sphere.
            if (name != myName)
            {
                double timeDiff = Mathf.Abs((float)(userPressTimestamps[myName] - userPressTimestamps[name]));
                if (timeDiff <= 1) //on second time diff
                {
                    RPC_DestroySphere();
                }

            }
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void RPC_DestroySphere()
    {
        gameObject.SetActive(false);
    }
}

