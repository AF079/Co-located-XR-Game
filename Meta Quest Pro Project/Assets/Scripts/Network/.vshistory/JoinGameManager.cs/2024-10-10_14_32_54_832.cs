using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class JoinGameManager : MonoBehaviour
{
    void Start()
    {
        PhotonNetwork.SendRate = 25;
        PhotonNetwork.SerializationRate = 15;
        PhotonNetwork.ConnectUsingSettings();

    }

    public void joinGame()
    {
        SceneManager.LoadScene("WaitRoom");
    }
}
