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
    }

    public void sendPosition()
    {
        Vector3 myHeadPosition = Camera.main.transform.position;
        Debug.Log("HEAD POS: " + Camera.main.transform.position);
        photonView_.RPC("DeactivateSphere", RpcTarget.Others, myHeadPosition.x, myHeadPosition.y, myHeadPosition.z);
    }

 /*   private IEnumerator DisplayPostion()
    {
        //Debug.Log("LEFT POS: " + leftController.transform.position + " RIGHT POS: " + rightController.transform.position);
        while(true)
        {
            
            yield return new WaitForSeconds(5);

        }
    }*/

    [PunRPC]
    private void sendTransformData(float x, float y, float z)
    {
        Vector3 myHeadPosition = Camera.main.transform.position;
        masterClientHeadPos = new Vector3(x, y, z);
        Vector3 offSet = masterClientHeadPos - myHeadPosition
        Debug.Log("OFFSET: " + offSet);
    }
}
