using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
public class JoinGameManager : MonoBehaviourPunCallbacks
{

    public void joinGame()
    {
        PhotonNetwork.SendRate = 25;
        PhotonNetwork.SerializationRate = 15;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        PhotonNetwork.LoadLevel("WaitRoom");
        heckForAvailableRooms();

    }

    private void CheckForAvailableRooms()
    {
        if (PhotonNetwork.CountOfRooms == 0)
        {
            Debug.Log("No rooms available, creating a new room...");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2;
            PhotonNetwork.CreateRoom("GameRoom", roomOptions);
        }
        else
        {
            Debug.Log("Rooms available, joining the existing room...");
            PhotonNetwork.JoinRoom("GameRoom");
        }
    }
}
