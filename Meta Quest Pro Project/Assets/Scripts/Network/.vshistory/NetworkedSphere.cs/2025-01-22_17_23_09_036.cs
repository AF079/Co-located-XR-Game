using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using ExitGames.Client.Photon;
using UnityEngine.UIElements;
using TMPro;

[RequireComponent(typeof(PhotonView), typeof(PhotonTransformViewClassic), typeof(Rigidbody))]

public class NetworkedSphere : MonoBehaviour, IPunOwnershipCallbacks, IPunObservable
{
    private GameObject Logger; //Logger instance, used for storing data in log file

    private PhotonView photonView; //Photon view for this gameobject

    private Dictionary<string, double> userPressTimestamps = new Dictionary<string, double>(); //Dictionary that stores user interactions. One dictionary per sphere
    private Queue<Vector3> positionQueue = new Queue<Vector3>(); //Stores stimulated positions

    private Rigidbody rb;
    private Vector3 targetPosition; //Non-Master Client target position to move sphere in

    //Master Client Latency configuration (in ms). These values can be changed.
    private float chanceOfLatency = 0.5f; //Chance of latency
    private float jitterIntensity = 0.0f; //Simulates variability in the timing of incoming network packets from the server to the client.
    private float baseLatency = 0.02f; //Simulates a fixed delay for incoming packets from the server to the client.
    private Vector3 simulatedPosition; //Simulated position affected by latency

    private const int REQ_NUM_INTERACTIONS = 2; //Number of interactions required for sphere to be popped
    [HideInInspector] public Color color; //Color of sphere

    //Status Flags
    private bool hasLogged = false; //Flag for whether data has been stored in log file
    private bool syncColors = true; //Flag for if colors have been synced (color of spheres should be the same for both players)
    private bool isTouching = false; //Flag for if sphere is being interacted with

    private int numPopped = 0; //Count to keep track of number of spheres popped
    float fixedDeltaTime;
    void Awake()
    {

        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        if (photonView == null)
        {
            Debug.Log("NO PHOTON VIEW!!!!");
        }
        PhotonNetwork.AddCallbackTarget(this);
        
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // Start is called before the first frame update
    void Start()
    {
       /* Vector3 startForce = new Vector3(UnityEngine.Random.Range(-2, 2), 0, UnityEngine.Random.Range(-2, 2)); //Random direction
        rb.AddForce(startForce, ForceMode.Impulse); //Apply random force in x and z direction*/
      
        fixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = 0.15f; //Slow down time
        Logger = GameObject.Find("Logger");
        if (PhotonNetwork.IsMasterClient)
        {
            simulatedPosition = rb.position;
            StartCoroutine(NetworkSimulationCoroutine()); //Simulate latency
        }
        else
        {
            targetPosition = rb.position;
        }
        if (!PhotonNetwork.IsMasterClient)
        {
            NetworkManager.move(transform.position);
        }
    }
    // Update is called once per frame
    void Update()
    {
        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
        if (syncColors && PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient) //If colors are not synced
        {
            photonView.RPC("SyncBallColor", RpcTarget.AllBuffered, new Vector3(color.r, color.g, color.b)); //Sync color of spheres
            syncColors = false;

        }

        if(PhotonNetwork.IsMasterClient && !hasLogged && numPopped >= NetworkManager.numberOfSpheres) //If data has not been logged yet and all balls have been popped
        {
            //Log data
            hasLogged = true;
            LogHandler logger = Logger.GetComponent<LogHandler>();
            string path = logger.GetQuestFilePath(LogHandler.FILE_NAME);
            logger.WriteToFile(path);
        }
    }


    //Sync sphere color 
    [PunRPC]
    void SyncBallColor(Vector3 color)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(color.x, color.y, color.z);

    }
    void FixedUpdate()
    {

        
        /*if (PhotonNetwork.IsMasterClient)
        {
            rb.MovePosition(Vector3.Lerp(rb.position, simulatedPosition, Time.fixedDeltaTime * 5f)); //Move position of sphere to simulated position (MC)

        }
        else
        {
            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * 5f)); //Move position of sphere to target position (NMC)

        }*/

    }

    //Network Simulation
    private IEnumerator NetworkSimulationCoroutine()
    {
        while (true) //Loop forever
        {
            yield return new WaitForSeconds(baseLatency + UnityEngine.Random.Range(-jitterIntensity, jitterIntensity)); //Wait for n (ms). Currently 200ms

            if (UnityEngine.Random.value > chanceOfLatency) { //If should add latency
                positionQueue.Enqueue(rb.position); // Add current position to queue
            }

            // Simulate delayed application of position updates
            if (positionQueue.Count > 0) //If there are latency affected positions
            {
                Vector3 pos = positionQueue.Dequeue(); //remove from queue

                simulatedPosition = new Vector3(pos.x, pos.y, pos.z); //set simulated position
            }
        }
    }
   
    //Send and receive callback function. Master client sends updated positions using transformation matrix to Non-Master Client.
    //Non-Master Client updates position based on what it receives
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (PhotonNetwork.IsMasterClient)
        {

            if (stream.IsWriting) //Only Master Client can write over channel

            {
                Vector3 localPosition = rb.position - NetworkManager.originBinA; //Express the translated sphere position in the master client coordinate system by subtracting the registered origin

                //Express the unit vectors relative to the registered origin. Then normalize to keep them as unit vectors. These are the basis vectors.
                //Remember any v' = a*xB + b*yB + c*zB
                Vector3 yB = (NetworkManager.yBinA - NetworkManager.originBinA).normalized; 
                Vector3 zB = (NetworkManager.zBinA - NetworkManager.originBinA).normalized;
                Vector3 xB = (NetworkManager.xBinA - NetworkManager.originBinA).normalized;

                //Construct transformation matrix
                float[,] transformationMatrix = new float[,]{ { xB.x,   xB.y,   xB.z },
                                     { yB.x,      yB.y,      yB.z },
                                     { zB.x, zB.y, zB.z } };

                //Transform the translated sphere position by doing M*v (matrix * vector)
                Vector3 transformedPosition = NetworkManager.MatMul(transformationMatrix, localPosition);
                
                //Send position over photon network
                stream.SendNext(transformedPosition);
                //Send translated and transformed velocity
                //Commented out not currently using
                //stream.SendNext(NetworkManager.MatMul(transformationMatrix, rb.velocity - NetworkManager.originBinA));

            }
        }
        else if (!PhotonNetwork.IsMasterClient && stream.IsReading) //Only Non-Master Client can receive on channel
        {
            try
            {

                targetPosition = (Vector3)stream.ReceiveNext(); //Set target position to postition sent by master client

                //Commented out not currently using
                //Vector3 receivedVelocity = (Vector3)stream.ReceiveNext(); //Set received velocity
            }
            catch (Exception e)
            {
                //Debug.Log("ERROR: " + e);
            }
        }

    }


    // Called when something first touches the sphere
    private void OnTriggerEnter(Collider other)
    {
        //When user touches the sphere 
        if (!other.CompareTag("tBall"))
        {
           
            isTouching = true;
        }
    }

    // Called while something continues touching the sphere
    private void OnTriggerStay(Collider other)
    {
        if (isTouching && !other.CompareTag("tBall"))
        {
            if (photonView != null)
            {
                if (photonView.IsMine) //if sphere belongs interacting client
                {
                    //record time stamp
                    double timeStamp = PhotonNetwork.Time;
                    //store name of user
                    string myName = PhotonNetwork.LocalPlayer.NickName;
                    photonView.RPC("updateUserPressTimestamps", RpcTarget.All, myName, timeStamp);
                    
                }
                else 
                {
                    photonView.RequestOwnership(); //request ownership if the sphere does not belong to the interacting player
                }
            }
        }
    }

    // Called when something stops touching the sphere
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("tBall"))
        {
           
            isTouching = false; //reset flag

        }
    }

    //RPC function that updates both the local dictionary for master client and non master client
    [PunRPC]
    void updateUserPressTimestamps(string name, double timeStamp)
    {

        if (photonView != null)
        {
            string myName = PhotonNetwork.LocalPlayer.NickName; //This is the name of the user thats interacting with the sphere

            //If the dictionary has already registered this name before, then update the timestamp
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
                        photonView.RPC("HideBall", RpcTarget.All); //Disable sphere
                    }
                    
                }
            }
        }
    }

    //Disable sphere RPC
    [PunRPC]
    void HideBall()
    {
        if (PhotonNetwork.IsMasterClient) //Only master client will generate log data
        {
            LogHandler.DATA += "Ball Popped at " + PhotonNetwork.Time + "\n"; //Data containing time the sphere was poppped at
            numPopped++;
        }
        
        gameObject.SetActive(false); //Disable sphere
    }

    //Callback to request ownership of a photonview
    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        if (photonView != null)
        {
            if (targetView != photonView)
            {
                return;
            }
            photonView.TransferOwnership(requestingPlayer); //Transfer ownership to the requesting player

        }
    }

    //Empty Callbacks not used

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        Debug.Log("OwnershipTransfered");

    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        Debug.Log("OwnerTransferFailed");
    }


}
