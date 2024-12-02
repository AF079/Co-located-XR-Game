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
    public float horizontalRandomness = 0.2f;
    public float energyLossFactor = 0.9f;
    public float pushForceMutliplier = 1.5f;
    public float bounceForce = 2f;

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
    private Vector3[] targetPositionsBuff = new Vector3[1000]; 
    private int curPosIdx = 0;
    private int idx = 0;
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
        prevSentPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
       /* if (Mathf.Abs(rb.transform.position.y) >= 5.0f || Mathf.Abs(rb.transform.position.x) >= 5.0f)
        {
            rb.transform.position = new Vector3(
               UnityEngine.Random.Range(0f, 1f),
               UnityEngine.Random.Range(0f, 1f),
               UnityEngine.Random.Range(0f, 1f)
           );
            rb.velocity = Vector3.zero;
        }*/
        if (syncColors && PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncBallColor", RpcTarget.AllBuffered, new Vector3(color.r, color.g, color.b));//photonView.ViewID,color.r, color.g, color.b, color.a
            syncColors = false;

        }

        if (!PhotonNetwork.IsMasterClient && !photonView.IsMine) //&& photonView.IsMine
        {
            /*if (curPosIdx == 0)
            {
                transform.position = targetPositionsBuff[0];
                curPosIdx = (curPosIdx + 1) % targetPositionsBuff.Length;
            }
            else if(curPosIdx > 0)
            {
                if(transform.position == targetPositionsBuff[curPosIdx])
                {
                    curPosIdx = (curPosIdx + 1) % targetPositionsBuff.Length;
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPositionsBuff[curPosIdx], Time.deltaTime * lerpSpeed);

                }

            }
                    Debug.Log("CUR IDX " + curPosIdx);*/

            rb.position = Vector3.MoveTowards(rb.position, targetPosition, Time.fixedDeltaTime);
                
        }
    }
    /*
     * master client req ownershhip
     * master client (owner) sends transform data of sphere
     * client req ownership 
     * client move sphere
     */
    [PunRPC]
    void SyncBallColor(Vector3 color)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(color.x, color.y, color.z);

    }
 
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (PhotonNetwork.IsMasterClient && !photonView.IsMine)
        {
            //Debug.Log("MASTER CLIENT REQ OWNERSHIP");
            photonView.RequestOwnership();
        }
        if (PhotonNetwork.IsMasterClient)
        {
            if (stream.IsWriting)// && prevSentPos != transform.position
            {
                //Debug.Log("MASTER CLIENT SENDING DATA");
                stream.SendNext(rb.position);
                stream.SendNext(rb.velocity);
                //prevSentPos = transform.position;
            }
        }
        else if(stream.Count > 0 && !PhotonNetwork.IsMasterClient && stream.IsReading)
        {
            if(!photonView.IsMine) {
                //Debug.Log("CLIENT REQ OWNERSHIP");

                photonView.RequestOwnership();
            }
            //Debug.Log("CLIENT MOVING SPHERE");
            try
            {
                targetPosition = (Vector3)stream.ReceiveNext();
                rb.velocity = (Vector3)stream.ReceiveNext();
                float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTimestamp));
                
                Vector3 pos = NetworkManager_test.move(targetPosition);

                targetPosition = new Vector3(pos.x, pos.y, pos.z);
                targetPosition += (rb.velocity * lag);
                //Debug.Log("AFTER " + targetPosition);
                /*targetPositionsBuff[idx] = targetPosition;
                idx = (idx+1) % targetPositionsBuff.Length;
                Debug.Log("IDX " + idx);*/

            }catch(Exception e)
            {
                Debug.Log("ERROR: " + e);
            }
        }
       
    }


    // Called when something first touches the sphere
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("tBall"))
        {
            Debug.Log("Sphere touched by hand!");
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
            Debug.Log("Hand stopped touching sphere.");
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
                    Debug.Log("TIME DIFF: " + timeDiff + " " + myName + " " + name);
                }
                else
                {
                    Debug.Log(name + " is not " + myName);
                }
            }
        }
    }

    [PunRPC]
    void HideBall()
    {
        //DestroyImmediate(gameObject);
        Debug.Log("Deactivating!");
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
