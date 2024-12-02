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

    private float k_energy;
    private float dampingFactor = 0.98f; 
    private float maxEnergy = 1f;


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
        prevSentPos = transform.position;

    }

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
        }

        if (!PhotonNetwork.IsMasterClient)
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
        }

    }
 
/*    public IEnumerator simulateLatency()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Adding latency for MC");
            Vector3 originalVelocity = rb.velocity;
            rb.velocity *= 0.002f;
            yield return new WaitForSeconds(1.5f);
            rb.velocity = originalVelocity;
        }
    }*/

    [PunRPC]
    void SyncBallColor(Vector3 color)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(color.x, color.y, color.z);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (PhotonNetwork.IsMasterClient)
        {
            Vector3 diff = prevSentPos - rb.position;
            float mag = diff.magnitude;
            float epsilon = 0.001f;
            Debug.Log("Diff size " + mag);

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

                prevSentPos = rb.position;
            
            }
        }
        else if (!PhotonNetwork.IsMasterClient && stream.IsReading)
        {
            try
            {

                targetPosition = (Vector3)stream.ReceiveNext();
                rb.velocity = (Vector3)stream.ReceiveNext();
                float lag = 1;// Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTimestamp)) * 0.5f;

                targetPosition += (rb.velocity * lag);

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

                /*if(userPressTimestamps.Count <= 1)
                {
                    double curTime = PhotonNetwork.Time;
                    double deltaTime = curTime - prevTime;
                    Vector3 currentHandPosition = other.gameObject.transform.position;
                    Vector3 handMovement = (currentHandPosition - previousHandPosition) / Time.deltaTime;
                    rb.AddForce(handMovement * pushForceMutliplier, ForceMode.Impulse);
                    previousHandPosition = currentHandPosition;
                }*/
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
