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


    private PhotonView photonView;
    private double prevTime;
    private Dictionary<string, double> userPressTimestamps = new Dictionary<string, double>();
    private Rigidbody rb;
    private bool isTouching = false;
    [HideInInspector] public Color color;
    private bool syncColors = true;
    private Vector3 previousHandPosition;
    public Vector3 prevSentPos;
    private Vector3 targetPosition;
    private Queue<Vector3> positionQueue = new Queue<Vector3>();

    private float k_energy;
    private float dampingFactor = 0.98f; 
    private float maxEnergy = 1f;

    private float packetLossProbability = 0.5f;
    private float jitterIntensity = 0.1f;
    private Vector3 simulatedPosition;
    private float baseLatency = 0.2f;

    private float maxHeight = 3f; // Starting maximum height
    private float newMaxHeight = 3f;
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
        k_energy = maxEnergy;
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
    private Vector3 velocity = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
       
        if (syncColors && PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncBallColor", RpcTarget.AllBuffered, new Vector3(color.r, color.g, color.b));//photonView.ViewID,color.r, color.g, color.b, color.a
            syncColors = false;

        }
        

    }
    


    void FixedUpdate()
    {


        if (PhotonNetwork.IsMasterClient && rb.velocity == Vector3.zero)
        {
            k_energy = maxEnergy;
            newMaxHeight = maxHeight;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3 clampedPosition = new Vector3(rb.position.x, Mathf.Min(rb.position.y, newMaxHeight), rb.position.z);
            rb.position = clampedPosition;

            //rb.position = Vector3.Lerp(rb.position, simulatedPosition, Time.fixedDeltaTime * 5f);
            rb.MovePosition(Vector3.Lerp(rb.position, simulatedPosition, Time.fixedDeltaTime * 5f));

        }
        else
        {
            Vector3 clampedPosition = new Vector3(rb.position.x, Mathf.Min(rb.position.y, newMaxHeight), rb.position.z);
            rb.position = clampedPosition;

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

                simulatedPosition = new Vector3(pos.x, Mathf.Min(pos.y, newMaxHeight), pos.z);
            }
        }
    }
    [PunRPC]
    void SyncBallColor(Vector3 color)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(color.x, color.y, color.z);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (PhotonNetwork.IsMasterClient)
        {
            //doAddLatency = addLatency();

            if (stream.IsWriting)// && prevSentPos != transform.position // && mag >= epsilon

            {
                //Debug.Log("MASTER CLIENT SENDING DATA");
                Vector3 localPosition = rb.position - NetworkManager.originBinA;

                Vector3 upB = (NetworkManager.upBinA - NetworkManager.originBinA).normalized;
                Vector3 forwardB = (NetworkManager.forwardBinA - NetworkManager.originBinA).normalized;
                Vector3 rightB = (NetworkManager.sideBinA - NetworkManager.originBinA).normalized;

                float[,] transformationMatrix = new float[,]{ { rightB.x,   rightB.y,   rightB.z },
                                     { upB.x,      upB.y,      upB.z },
                                     { forwardB.x, forwardB.y, forwardB.z } };

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
                Vector3 v = (Vector3)stream.ReceiveNext();
                rb.velocity = (Vector3)stream.ReceiveNext();
                float lag = 1;// Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTimestamp)) * 0.5f;

                //targetPosition += (rb.velocity * lag);

            }
            catch (Exception e)
            {
                //Debug.Log("ERROR: " + e);
            }
        }

    }


    void OnCollisionEnter(Collision collision)
    {
        if (PhotonNetwork.IsMasterClient && !collision.collider.CompareTag("tBall"))
        {
            float x = Mathf.Sqrt(k_energy);
            rb.velocity *= x;
            k_energy = Mathf.Clamp(k_energy * dampingFactor, 0f, maxEnergy);

        }

    }

    // Called when something first touches the sphere
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("tBall"))
        {
            //Debug.Log("Sphere touched by hand!");
            isTouching = true;
            previousHandPosition = other.gameObject.transform.position;
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
            //Debug.Log("Hand stopped touching sphere.");
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

            if (userPressTimestamps.Count >= 2)
            {
                //photonView.RPC("HideBall", RpcTarget.All);
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
        //DestroyImmediate(gameObject);
       // Debug.Log("Deactivating!");
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