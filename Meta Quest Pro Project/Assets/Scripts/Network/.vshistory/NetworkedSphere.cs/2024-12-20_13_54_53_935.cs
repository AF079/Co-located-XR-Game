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

    //Master Client Latency configuration
    private float packetLossProbability = 0.5f; //Packet Loss
    private float jitterIntensity = 0.0f; //Simulates variability in the timing of incoming network packets from the server to the client.
    private float baseLatency = 0.02f; //Simulates a fixed delay (lag) for incoming packets from the server to the client.
    private Vector3 simulatedPosition;

    private const int REQ_NUM_INTERACTIONS = 2;
    private float maxHeight = 2f; 
    private float newMaxHeight = 2f;
    private double prevTime;
    [HideInInspector] public Color color;

    private bool hasLogged = false;
    private bool syncColors = true;
    private bool isTouching = false;
    private int numPopped = 0;

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
        Vector3 startForce = new Vector3(UnityEngine.Random.Range(-2, 2), 0, UnityEngine.Random.Range(-2, 2));
        rb.AddForce(startForce, ForceMode.Impulse);
        fixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = 0.15f;
        Logger = GameObject.Find("Logger");
        if (PhotonNetwork.IsMasterClient)
        {
            simulatedPosition = rb.position;
            StartCoroutine(NetworkSimulationCoroutine());
        }
        else
        {
            targetPosition = rb.position;
        }
    }
    // Update is called once per frame
    void Update()
    {
        Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
        if (syncColors && PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncBallColor", RpcTarget.AllBuffered, new Vector3(color.r, color.g, color.b));//photonView.ViewID,color.r, color.g, color.b, color.a
            syncColors = false;

        }

        if(PhotonNetwork.IsMasterClient && !hasLogged && numPopped >= NetworkManager.numberOfBalls)
        {
            hasLogged = true;
            LogHandler logger = Logger.GetComponent<LogHandler>();
            string path = logger.GetQuestFilePath(LogHandler.FILE_NAME);
            logger.WriteToFile(path);
            LogHandler.hasSaved = "HAS SAVED = OK";

        }
    }


    [PunRPC]
    void SyncBallColor(Vector3 color)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(color.x, color.y, color.z);

    }
    void FixedUpdate()
    {

        
        if (PhotonNetwork.IsMasterClient)
        {
           // Vector3 clampedPosition = new Vector3(rb.position.x, Mathf.Min(rb.position.y,maxHeight), rb.position.z);
            //rb.position = clampedPosition;

            rb.MovePosition(Vector3.Lerp(rb.position, simulatedPosition, Time.fixedDeltaTime * 5f));

        }
        else
        {
            //Vector3 clampedPosition = new Vector3(rb.position.x, rb.position.y, rb.position.z);
            //rb.position = clampedPosition;


            rb.MovePosition(Vector3.Lerp(rb.position, targetPosition, Time.fixedDeltaTime * 5f));

        }

    }

    private IEnumerator NetworkSimulationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(baseLatency + UnityEngine.Random.Range(-jitterIntensity, jitterIntensity));

            if (UnityEngine.Random.value > packetLossProbability) 
            {
                positionQueue.Enqueue(rb.position);
            }

            // Simulate delayed application of position updates
            if (positionQueue.Count > 0)
            {
                Vector3 pos = positionQueue.Dequeue();

                simulatedPosition = new Vector3(pos.x, pos.y, pos.z);//Mathf.Min(pos.y, newMaxHeight)
            }
        }
    }
   

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (PhotonNetwork.IsMasterClient)
        {

            if (stream.IsWriting)

            {
                Vector3 localPosition = rb.position - NetworkManager.originBinA;

                Vector3 yB = (NetworkManager.yBinA - NetworkManager.originBinA).normalized;
                Vector3 zB = (NetworkManager.zBinA - NetworkManager.originBinA).normalized;
                Vector3 xB = (NetworkManager.xBinA - NetworkManager.originBinA).normalized;

                float[,] transformationMatrix = new float[,]{ { xB.x,   xB.y,   xB.z },
                                     { yB.x,      yB.y,      yB.z },
                                     { zB.x, zB.y, zB.z } };

                Vector3 rotatedPosition = NetworkManager.MatMul(transformationMatrix, localPosition);
                    
                stream.SendNext(rotatedPosition);
                stream.SendNext(NetworkManager.MatMul(transformationMatrix, rb.velocity - NetworkManager.originBinA));

            }
        }
        else if (!PhotonNetwork.IsMasterClient && stream.IsReading)
        {
            try
            {

                targetPosition = (Vector3)stream.ReceiveNext();
                Vector3 receivedVelocity = (Vector3)stream.ReceiveNext();

                //rb.velocity = receivedVelocity;

                /*float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTimestamp)) * 0.5f;
                targetPosition += (receivedVelocity * lag);*/

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
        if (!other.CompareTag("tBall"))
        {
           
            isTouching = true;
            prevTime = PhotonNetwork.Time;
        }
    }

    // Called while something continues touching the sphere
    private void OnTriggerStay(Collider other)
    {
        if (isTouching && !other.CompareTag("tBall"))
        {
            if (photonView != null)
            {
                if (photonView.IsMine)
                {
                    double timeStamp = PhotonNetwork.Time;
                    string myName = PhotonNetwork.LocalPlayer.NickName;
                    photonView.RPC("updateUserPressTimestamps", RpcTarget.All, myName, timeStamp);
                    
                }
                else
                {
                    photonView.RequestOwnership();
                }
            }
        }
    }

    // Called when something stops touching the sphere
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("tBall"))
        {
           
            isTouching = false;

        }
    }

    [PunRPC]
    void updateUserPressTimestamps(string name, double timeStamp)
    {

        if (photonView != null)
        {
            string myName = PhotonNetwork.LocalPlayer.NickName;

            if (userPressTimestamps.ContainsKey(name))
            {
                userPressTimestamps[name] = timeStamp;
            }
            else
            {
                userPressTimestamps.Add(name, timeStamp);
            }

            if (userPressTimestamps.Count >= REQ_NUM_INTERACTIONS)
            {
                
                if (name != myName)
                {
                    double timeDiff = Mathf.Abs((float)(userPressTimestamps[myName] - userPressTimestamps[name]));
                    if (timeDiff <= 1) //on second time diff
                    {
                        photonView.RPC("HideBall", RpcTarget.All);
                    }
                    //Debug.Log("TIME DIFF: " + timeDiff + " " + myName + " " + name);
                }
                else
                {
                    //Debug.Log(name + " is not " + myName);
                }
            }
        }
    }

    [PunRPC]
    void HideBall()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //LogHandler logger = Logger.GetComponent<LogHandler>();
            LogHandler.DATA += "Ball Popped at " + PhotonNetwork.Time + "\n";
            numPopped++;
        }
        
        gameObject.SetActive(false);
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        if (photonView != null)
        {
            if (targetView != photonView)
            {
                return;
            }
            photonView.TransferOwnership(requestingPlayer);

        }
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        Debug.Log("OwnershipTransfered");

    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        Debug.Log("OwnerTransferFailed");
    }


}

/*if (!PhotonNetwork.IsMasterClient)
{
    Vector3 direction = (targetPosition - rb.position).normalized;
    float distance = Vector3.Distance(rb.position, targetPosition);

    if (distance > 0.01f) // Ignore small movements
    {
        rb.velocity = 0.01f* direction * distance / Time.fixedDeltaTime;
    }
    else
    {
        rb.velocity = Vector3.zero;
    }
}*/

    //<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" tools:node="remove" />
  //  < uses - permission android: name = "android.permission.READ_EXTERNAL_STORAGE" tools: node = "remove" />