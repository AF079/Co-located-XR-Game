using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Oculus.Platform;
using Oculus.Platform.Models;
public class JoinGameManager : MonoBehaviourPunCallbacks
{
    private ulong metaId;
    private void Awake()
    {
        Core.Initialize();
        Users.GetLoggedInUser().OnComplete(OnLoggedInUser);
    }
    private void OnLoggedInUser(Message<User> msg)
    {
        if (msg.IsError)
        {
            Debug.LogError("Failed to get logged in user: " + msg.GetError().Message);
            return;
        }

        ulong userID = msg.Data.ID;
        Debug.Log("Logged in User ID: " + userID);
        metaId = userID;

    }
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
        //PhotonNetwork.LoadLevel("WaitRoom");
        CheckForAvailableRooms();

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

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined the room successfully: " + PhotonNetwork.CurrentRoom.Name);

        PhotonNetwork.LoadLevel("GameScene");

    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join Room Failed: " + message);
    }
}
