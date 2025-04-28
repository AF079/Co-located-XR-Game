

using Fusion;
using UnityEngine;
using Fusion.Statistics;

public class SphereController : NetworkBehaviour
{
    //private Dictionary<string, double> userPressTimestamps = new Dictionary<string, double>();
    private FusionStatisticsSnapshot test_snap_shot;
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

    public float maxSpeed = 100f;
    public float maxAngularSpeed = 100f;
    // public GameObject SphereLatencyManager_obj;
    public GameObject logHandler;
    private Rigidbody rb;
    [Networked] private TickTimer RespawnTimer { get; set; }

    [Networked, OnChangedRender(nameof(OnVisibilityChanged))]
    public bool IsVisible { get; set; }
    private bool hasInteracted = false;

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

    private float waitTime = 10f;
    private float dt = 0;

    private bool shouldRespawn = false;

    private double host_mode_time_stamp = -1;
    string color;
    public override void Spawned()
    {

        base.Spawned(); // Not required, but keeps compatibility
        // latencyManager = SphereLatencyManager_obj.GetComponent<SphereLatencyManager>();
        myName = "user" + UnityEngine.Random.Range(1, 1000);
        Debug.Log("IN SPHERE CONTROLLER " + myName);
        rb = gameObject.GetComponent<Rigidbody>();
        color = GetColorName(gameObject.GetComponent<MeshRenderer>().material.color);
        if (HasStateAuthority && ColocationManager.Instance != null)
        {
            SphereId = ColocationManager.Instance.GetNextSphereId();

        }
        test_snap_shot = new FusionStatisticsSnapshot();
        IsVisible = true;
    }


    void FixedUpdate()
    {
        if (rb == null) return;
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }

        if (rb.angularVelocity.magnitude > maxAngularSpeed)
        {
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, maxAngularSpeed);
        }

        // if (test_snap_shot == null) return;
        // Debug.Log("In-Bandwidth " + test_snap_shot.InBandwidth);
        // Debug.Log("Out-Bandwidth " + test_snap_shot.OutBandwidth);

        // Debug.Log("In-Packets " + test_snap_shot.InPackets);
        // Debug.Log("Out-Bandwidth " + test_snap_shot.OutPackets);

        // Debug.Log("Interpolation Speed " + test_snap_shot.InterpolationSpeed);
        // Debug.Log("Interpolation Offset " + test_snap_shot.InterpolationOffset);

        // Debug.Log("InObjectUpdates " + test_snap_shot.InObjectUpdates);
        // Debug.Log("OutObjectUpdates " + test_snap_shot.OutObjectUpdates);

        // Debug.Log("RTT " + test_snap_shot.RoundTripTime);

        // Debug.Log("WordsReadCount " + test_snap_shot.WordsReadCount);
        // Debug.Log("WordsWrittenCount " + test_snap_shot.WordsWrittenCount);


    }

    public override void FixedUpdateNetwork()
    {
        if (Object == null || Runner.GameMode == GameMode.Shared) return;

        if (Object.HasStateAuthority && shouldRespawn)
        {
            shouldRespawn = false;
            IsVisible = false;
            Respawn();

        }
    }

    // [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, Channel = RpcChannel.Reliable)]

    // public void RPC_PingFromHost(double host_ping_time_stamp)
    // {
    //     if (Object.HasStateAuthority) return;
    //     double owd_h2c = Runner.SimulationTime - host_ping_time_stamp;
    //     //log owd_h2c
    //     RPC_PongFromClient(Runner.SimulationTime);
    // }

    // [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]

    // public void RPC_PongFromClient(double client_pong_time_stamp)
    // {
    //     double owd_c2h = Runner.SimulationTime - client_pong_time_stamp;
    //     //loog owd_h2c
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

            if (Runner.GameMode == GameMode.Shared)
            {
                RequestOwnership();
            }
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (Object == null || Runner == null)
            return;
        if (isTouching && !other.CompareTag("sphere"))
        {
            Debug.Log("INTERACTING!");

            if (Runner.GameMode == GameMode.Shared)
            {
                if (hasBeenPopped) return;
                if (Object.HasStateAuthority)
                {
                    HandleInteraction(Runner.SimulationTime, myName);
                }
                else
                {
                    RequestOwnership();
                }
            }
            else
            {
                if (Runner.IsServer)
                {
                    host_mode_time_stamp = Runner.SimulationTime;
                    Debug.Log("IsHost " + SphereId + " " + Runner.IsServer + " " + color);
                }
                else
                {
                    Debug.Log("IsHost " + SphereId + " " + Runner.IsServer);
                    RPC_SendDataToHost(Runner.SimulationTime);

                }
            }
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    public void RPC_SendDataToHost(double client_timeStamp)
    {

        if (Runner == null || Object == null || hasInteracted) return;

        if (host_mode_time_stamp == -1) return;

        double timeDiff = Mathf.Abs((float)(host_mode_time_stamp - client_timeStamp));
        if (timeDiff <= 2.0f)
        {
            Debug.Log("IsHost " + Runner.IsServer + " " + SphereId + " SendingClientLogSignal " + color);

            RPC_SendClientLogSignal();
            //hasInteracted = true;
            // LogData();
            RPC_DestroySphere();
        }
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_SendClientLogSignal()
    {
        if (Runner == null || Object == null || hasInteracted) return;

        //if (Runner.IsServer) return;

        hasInteracted = true;
        LogData();

    }


    private void HandleInteraction(double timeStamp, string name)
    {

        if (interactionCount == 0)
        {
            interactionList.Set(0, new InteractionEntry
            {
                playerName = name,
                timeStamp = timeStamp
            });
            interactionCount++;
        }
        else
        {
            if (interactionList[0].playerName == name)
            {
                interactionList.Set(0, new InteractionEntry
                {
                    playerName = name,
                    timeStamp = timeStamp
                });
            }
            else
            {

                interactionList.Set(1, new InteractionEntry
                {
                    playerName = name,
                    timeStamp = timeStamp
                });
                interactionCount++;
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

    private void OnTriggerExit(Collider other)
    {
        if (Object == null || Runner == null)
            return;
        if (!other.CompareTag("sphere"))
        {
            isTouching = false;
            hasInteracted = false;
            Debug.Log("STOPPED INTERACTING!");
            host_mode_time_stamp = -1;

        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All, Channel = RpcChannel.Reliable)]
    public void RPC_LogData()
    {
        if (ColocationManager.Instance == null)
            return;

        Debug.Log("Logging, IsHost " + Runner.IsServer + " " + color);
        float timeStamp = 0;
        double rtt = 0;
        int ticks = 0;
        if (Runner != null)
        {
            rtt = Runner.GetPlayerRtt(Runner.LocalPlayer);
            timeStamp = Runner.SimulationTime;
            ticks = Runner.TicksExecuted;
        }
        color = GetColorName(gameObject.GetComponent<MeshRenderer>().material.color);
        LogHandler.LOG += string.Format("{0,-10} | {1,-12} | {2,-14:F3} | {3,-10} | {4,-12}\n",
                                SphereId, timeStamp, rtt, ticks, color);

        logHandler.GetComponent<LogHandler>().SaveText(LogHandler.LOG);
        if (!Runner.IsServer)
        {
            hasInteracted = false;
        }
    }

    public void LogData()
    {
        if (ColocationManager.Instance == null)
            return;

        Debug.Log("Logging, IsHost " + Runner.IsServer + " " + color);
        float timeStamp = 0;
        double rtt = 0;
        int ticks = 0;
        if (Runner != null)
        {
            rtt = Runner.GetPlayerRtt(Runner.LocalPlayer);
            timeStamp = Runner.SimulationTime;
            ticks = Runner.TicksExecuted;
        }
        color = GetColorName(gameObject.GetComponent<MeshRenderer>().material.color);
        LogHandler.LOG += string.Format("{0,-10} | {1,-12} | {2,-14:F3} | {3,-10} | {4,-12}\n",
                                SphereId, timeStamp, rtt, ticks, color);

        logHandler.GetComponent<LogHandler>().SaveText(LogHandler.LOG);
        if (!Runner.IsServer)
        {
            hasInteracted = false;
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
    public void RPC_DestroySphere()
    {
        shouldRespawn = true;

        if (Runner.GameMode != GameMode.Shared) return;
        IsVisible = false;
        Respawn();
    }

    public void Respawn()
    {
        hasInteracted = false;
        interactionList.Clear();
        interactionCount = 0;
        host_mode_time_stamp = -1;
        IsVisible = true;
        hasBeenPopped = false;
        Vector3 randomPos = new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 2f), Random.Range(-1f, 1f));
        Position = randomPos;
        Debug.Log("Set new position of Sphere " + SphereId + " to " + Position);
    }


    private string GetColorName(Color color)
    {
        if (color == Color.red) return "Red";
        if (color == Color.blue) return "Blue";
        if (color == Color.green) return "Green";
        if (color == Color.white) return "White";
        if (color == new Color(1f, 1f, 0f)) return "Yellow";
        if (color == new Color(0f, 1f, 1f)) return "Mint";
        if (color == new Color(0.5f, 0f, 1f)) return "Purple";
        if (color == new Color(1f, 0f, 1f)) return "Pink";
        if (color == new Color(1f, 0.5f, 0f)) return "Orange"; // Manual match
                                                               // fallback
        return "Unknown";
    }


}
