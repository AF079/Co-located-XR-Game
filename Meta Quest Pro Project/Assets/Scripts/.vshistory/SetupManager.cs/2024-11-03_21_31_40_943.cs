using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public GameObject ClientManager;
    public GameObject NetworkManager;
    void Start()
    {
        PhotonNetwork.LocalPlayer.NickName = "toby";
        if (PhotonNetwork.IsMasterClient)
        {
            Instantiate(NetworkManager);
        }
        else
        {
            Instantiate(ClientManager);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
