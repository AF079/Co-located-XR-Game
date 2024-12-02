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
    void Start()
    {
        if (PhotonNetwork.MasterClient)
        {
            StartCoroutine(DisplayPostion());

        }

    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator DisplayPostion()
    {
        //Debug.Log("LEFT POS: " + leftController.transform.position + " RIGHT POS: " + rightController.transform.position);
        while(true)
        {
            Debug.Log("HEAD POS: " + Camera.main.transform.position);
            yield return new WaitForSeconds(5);

        }
    }

    [PunRPC]
    private void sendTransformData(float x, float y, float z)
    {
        masterClientHeadPos = new Vector3(x, y, z);
    }
}
