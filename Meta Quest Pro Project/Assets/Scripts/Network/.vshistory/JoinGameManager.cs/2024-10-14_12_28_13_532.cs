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
    private string _oculusUsername;
    private ulong _oculusUserId;
 
    public void Start()
    {

        PhotonNetwork.SendRate = 25;
        PhotonNetwork.SerializationRate = 15;
        PhotonNetwork.ConnectUsingSettings();
        Core.Initialize();
        Debug.Log("Oculus Core Initialized");
        //Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
        
    }

    public void joinGame() // when join game button is pressed 
    {
       
    }

    /*private void GetLoggedInUserCallback(Message msg)
    {
        if (msg.IsError)
        {
            Debug.LogError("GetLoggedInUser error: " + msg.GetError() + "; Error code: " + msg.GetError().Code);
        }
        else
        {
            Debug.Log("GetLoggedInUser success! " + msg + "; message type: " + msg.Type);
            if (msg.Type == Message.MessageType.User_GetLoggedInUser)
            {
                Debug.Log("GetLoggedInUser user: " + msg.GetUser().OculusID);
                _oculusUsername = msg.GetUser().OculusID;
                _oculusUserId = msg.GetUser().ID;
            }
        }

        
        Debug.Log("Your name and id are: " +  _oculusUsername + " " + _oculusUserId);
        PhotonNetwork.LocalPlayer.NickName = _oculusUsername;
        
    }*/
 

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

        PhotonNetwork.LoadLevel("GameScene");//GameScene

    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join Room Failed: " + message);
    }
}
