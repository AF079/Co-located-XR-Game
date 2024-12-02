using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.Threading.Tasks;
using TMPro;
using Fusion;

public class ControllerDataDisplay : MonoBehaviour
{
    private Vector3 masterClientHeadPos;
    private PhotonView photonView_;

    private Vector3 p1;
    private Vector3 p2;
    private Vector3 p12;
    private Vector3 rec_p12;
    void Start()
    {
        photonView_ = GetComponent<PhotonView>();
/*        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DisplayPostion());

        }*/

    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient) {
            if (p1 == null || p2 == null)
            {
                registerPoints();

            }
            else
            {
                sendPosition();
            }
        }
    }

    private void registerPoints()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            if(p1 == null)
            {
                p1 = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            }else if(p2 == null)
            {
                p2 = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            }
        }
    }

    public void sendPosition()
    {
        p12 = p2 - p1;

        photonView_.RPC("DeactivateSphere", RpcTarget.Others, p12.x,p12.y,p12.z);
    }

    [PunRPC]
    private void sendTransformData(float x, float y, float z)
    {
        rec_p12 = new Vector3(x,y,z);
        Quaternion rotation = Quaternion.FromToRotation(p12, rec_p12);
    }
}
