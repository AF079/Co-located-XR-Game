using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class JoinGameManager : MonoBehaviour
{

    public void joinGame()
    {
        PhotonNetwork.SendRate = 25;
        PhotonNetwork.SerializationRate = 15;
        PhotonNetwork.ConnectUsingSettings();
        SceneManager.LoadScene("WaitRoom");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        PhotonNetwork.LoadLevel("Lobby");
        //Debug.Log("Joined Lobby - Now Fetching Available Rooms");

    }
}
