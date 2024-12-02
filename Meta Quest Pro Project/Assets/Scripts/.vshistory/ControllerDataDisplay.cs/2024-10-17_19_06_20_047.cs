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
    private Vector3 rec_p1;

    //mc is player a
    //c is player b
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
            if (p1 == Vector3.zero || p2 == Vector3.zero)
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

        photonView_.RPC("sendTransformData", RpcTarget.Others, p12.x,p12.y,p12.z,p1.x,p1.y,p1.z);
    }

    [PunRPC]
    private void sendTransformData(float x12, float y12, float z12,float x1,float y1,float z1)
    {
        rec_p12 = new Vector3(x12,y12,z12);
        rec_p1 = new Vector3(x1,y1,z1);
        Vector3 translation = rec_p1 - p1;
        Camera.main.transform.position += translation;
        Quaternion rotation = Quaternion.FromToRotation(p12, rec_p12);
        Camera.main.transform.rotation *= rotation;
    }
}
