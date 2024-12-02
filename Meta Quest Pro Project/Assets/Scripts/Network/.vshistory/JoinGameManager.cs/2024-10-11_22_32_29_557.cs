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
    public void Start()
    {
        PhotonNetwork.SendRate = 25;
        PhotonNetwork.SerializationRate = 15;
        PhotonNetwork.ConnectUsingSettings();

        Core.Initialize();
        Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
    }

    private void GetLoggedInUserCallback(Message msg)
    {
        if (msg.IsError)
        {
            return;
        }
        var isLoggedInUserMessage = msg.Type == Message.MessageType.User_GetLoggedInUser;
        if (!isLoggedInUserMessage)
        {
            return;
        }

        _oculusUsername = msg.GetUser().OculusID;
        _oculusUserId = msg.GetUser().ID;
    }
    public void joinGame()
    {
        
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

    private void CheckForAvailableRooms() //call this function on btn press
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
