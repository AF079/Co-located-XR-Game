using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

[RequireComponent(typeof(PhotonView), typeof(PhotonTransformViewClassic), typeof(Rigidbody))]

public class NetworkedSphere : MonoBehaviour, IPunOwnershipCallbacks
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        throw new NotImplementedException();
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        throw new NotImplementedException();
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        throw new NotImplementedException();
    }


}
