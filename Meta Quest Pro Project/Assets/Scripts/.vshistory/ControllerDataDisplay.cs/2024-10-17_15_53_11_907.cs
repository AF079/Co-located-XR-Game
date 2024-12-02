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
        registerPoints();
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

        photonView_.RPC("DeactivateSphere", RpcTarget.Others, null);
    }

    [PunRPC]
    private void sendTransformData(float x, float y, float z)
    {
    
    }
}
