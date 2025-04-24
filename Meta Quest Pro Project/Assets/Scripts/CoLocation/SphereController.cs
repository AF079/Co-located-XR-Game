using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;
using UnityEditor;
using Fusion.Sockets;
using UnityEngine.Pool;
using Unity.VisualScripting;

public class SphereController : NetworkBehaviour
{
    //private Dictionary<string, double> userPressTimestamps = new Dictionary<string, double>();

    public struct InteractionEntry : INetworkStruct
    {
        [Networked] public NetworkString<_16> playerName { get; set; }
        [Networked] public double timeStamp { get; set; }
    }
    [Networked, Capacity(2)]
    private NetworkArray<InteractionEntry> interactionList { get; }
    [Networked] private int interactionCount { get; set; }
    [Networked] private bool hasBeenPopped { get; set; }

    [Networked] public int SphereId { get; set; }

    // [Networked, OnChangedRender(nameof(OnVisibilityChanged))]
    // public bool IsVisible { get; set; }

    string myName;
    bool isTouching = false;
    int REQ_NUM_INTERACTIONS = 2;

    public float maxSpeed = 5f;
    public float maxAngularSpeed = 10f;
    // public GameObject SphereLatencyManager_obj;
    public GameObject logHandler;
    private Rigidbody rb;
    [Networked] private TickTimer RespawnTimer { get; set; }
    private const string ConfigPath = "NetworkProject.Config";

    [Networked, OnChangedRender(nameof(OnVisibilityChanged))]
    public bool IsVisible { get; set; }

    private void OnVisibilityChanged()
    {
        gameObject.SetActive(IsVisible);
    }


    [Networked, OnChangedRender(nameof(OnPositionChanged))]
    public Vector3 Position { get; set; }

    private void OnPositionChanged()
    {
        transform.position = Position;
    }


    private bool shouldRespawn = false;
    private float waitTime = 10f;
    private float dt = 0;
    public override void Spawned()
    {

        base.Spawned(); // Not required, but keeps compatibility
        // latencyManager = SphereLatencyManager_obj.GetComponent<SphereLatencyManager>();
        myName = "user" + Random.Range(1, 1000);
        Debug.Log("IN SPHERE CONTROLLER " + myName);
        rb = gameObject.GetComponent<Rigidbody>();
        if (HasStateAuthority && ColocationManager.Instance != null)
        {
            SphereId = ColocationManager.Instance.GetNextSphereId();

        }
        IsVisible = true;

        // if (!Runner.IsServer)
        // {

        //     var config = NetworkProjectConfig.Global;
        //     if (config == null)
        //     {
        //         Debug.LogError($"NetworkProjectConfig not found at Resources/{ConfigPath}");
        //         return;
        //     }

        //     config.NetworkConditions.Enabled = true;
        //     config.NetworkConditions.DelayMin = 0.05f;
        //     config.NetworkConditions.DelayMax = 0.15f;
        //     config.NetworkConditions.AdditionalJitter = 0.20f;

        //     config.NetworkConditions.LossChanceMin = 0.01f;
        //     config.NetworkConditions.LossChanceMax = 0.20f;
        //     config.NetworkConditions.AdditionalLoss = 0.05f;

        // }
    }


    // void FixedUpdate()
    // {
    //     if (rb.velocity.magnitude > maxSpeed)
    //     {
    //         rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    //     }

    //     if (rb.angularVelocity.magnitude > maxAngularSpeed)
    //     {
    //         rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngularSpeed);
    //     }
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


            RequestOwnership();
        }
    }



    private void OnTriggerStay(Collider other)
    {
        if (Object == null || Runner == null)
            return;
        if (isTouching && !other.CompareTag("sphere"))
        {
            Debug.Log("INTERACTING!");
            if (hasBeenPopped) return;

            if (Object.HasStateAuthority)
            {
                double timeStamp = Runner.SimulationTime;

                if (interactionCount == 0)
                {
                    interactionList.Set(0, new InteractionEntry
                    {
                        playerName = myName,
                        timeStamp = timeStamp
                    });
                    interactionCount++;
                }
                else
                {
                    if (interactionList[0].playerName == myName)
                    {
                        interactionList.Set(0, new InteractionEntry
                        {
                            playerName = myName,
                            timeStamp = timeStamp
                        });
                    }
                    else
                    {

                        interactionList.Set(1, new InteractionEntry
                        {
                            playerName = myName,
                            timeStamp = timeStamp
                        });

                        double timeDiff = Mathf.Abs((float)(interactionList[1].timeStamp - interactionList[0].timeStamp));
                        if (timeDiff <= 1f) //on second time diff
                        {
                            hasBeenPopped = true;
                            RPC_LogData();
                            RPC_DestroySphere();
                        }
                    }
                }
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


    // private void OnVisibilityChanged()
    // {
    //     gameObject.SetActive(IsVisible);
    // }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_LogData()
    {
        if (ColocationManager.Instance != null)
        {

            //RPC_BroadcastPop(SphereId);

            float timeStamp;
            double rtt;
            if (Runner != null)
            {
                rtt = Runner.GetPlayerRtt(Runner.LocalPlayer);
                timeStamp = Runner.SimulationTime;

            }
            else
            {
                rtt = 0;
                timeStamp = 0;
            }
            LogHandler.LOG += string.Format("{0,-10} | {1,-12} | {2,-14:F3}\n",
                                             SphereId, timeStamp, rtt);

            logHandler.GetComponent<LogHandler>().SaveText(LogHandler.LOG);

        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    public void RPC_DestroySphere()
    {

        Debug.Log("Has State Auhority: " + Object.HasStateAuthority);
        Debug.Log("Diactivating Sphere: " + SphereId);
        //gameObject.SetActive(false);
        IsVisible = false;
        Debug.Log("Deactivated Sphere: " + SphereId);

        shouldRespawn = true;
        Debug.Log("ShouldRespawn: " + shouldRespawn);
        Respawn();

    }

    public void Respawn()
    {
        Vector3 randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 2f), Random.Range(-1f, 1f));
        Position = randomPos;
        IsVisible = true;
        hasBeenPopped = false;
        interactionList.Clear();
        interactionCount = 0;
        Debug.Log("Set new position of Sphere " + SphereId + " to " + Position);
    }

}


