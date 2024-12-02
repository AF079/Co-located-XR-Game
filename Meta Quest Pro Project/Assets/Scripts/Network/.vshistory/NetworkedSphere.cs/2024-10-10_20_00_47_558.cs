using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using ExitGames.Client.Photon;

[RequireComponent(typeof(PhotonView), typeof(PhotonTransformViewClassic), typeof(Rigidbody))]

public class NetworkedSphere : MonoBehaviour, IPunOwnershipCallbacks
{
    public float horizontalRandomness = 0.2f;
    public float energyLossFactor = 0.9f;
    public float pushForceMutliplier = 5f;
    public float bounceForce = 2f;
    private PhotonView photonView;
    private double prevTime;
    private Dictionary<string, double> userPressTimestamps = new Dictionary<string, double>();
    private Rigidbody rb;
    private bool isTouching = false;
    [HideInInspector] public Color color;
    private Vector3 previousHandPosition;
    private bool syncColors = true;

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

    }

    // Update is called once per frame
    void Update()
    {
        if (rb.transform.position.y <= -4.0f)
        {
            rb.transform.position = new Vector3(
               UnityEngine.Random.Range(0f, 1f),
               UnityEngine.Random.Range(0f, 1f),
               UnityEngine.Random.Range(0f, 1f)
           );
            rb.velocity = Vector3.zero;
        }
        if (syncColors && PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncBallColor", RpcTarget.AllBuffered, new Vector3(color.r, color.g, color.b));//photonView.ViewID,color.r, color.g, color.b, color.a
            syncColors = false;

        }
    }

    [PunRPC]
    void SyncBallColor(Vector3 color)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(color.x, color.y, color.z);

    }


    // Called when something first touches the sphere
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
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
        if (isTouching && other.CompareTag("PlayerHand"))
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
            double curTime = PhotonNetwork.Time;   
            double deltaTime = curTime - prevTime;
            Vector3 currentHandPosition = other.gameObject.transform.position;
            Vector3 handMovement = (currentHandPosition - previousHandPosition) / (float)deltaTime;
            rb.AddForce(handMovement * pushForceMutliplier, ForceMode.Impulse);
            previousHandPosition = currentHandPosition;
        }
    }

    // Called when something stops touching the sphere
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
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
