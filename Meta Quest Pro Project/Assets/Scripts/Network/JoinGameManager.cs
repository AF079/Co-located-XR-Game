using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Oculus.Platform;
using Oculus.Platform.Models;
using System.Text;
using ExitGames.Client.Photon;
public class JoinGameManager : MonoBehaviourPunCallbacks
{

    private void Start()
    {
        joinGame();
    }
    public void joinGame()
    {
        try
        {
            PhotonNetwork.SendRate = 25; //Set send rate
            PhotonNetwork.SerializationRate = 15; //Set serialization rate: Rate at which data gets sampled into packets before sending


            PhotonNetwork.ConnectUsingSettings(); //Connect to photon servers

            PhotonNetwork.NetworkingClient.LoadBalancingPeer.IsSimulationEnabled = true; //Enable network simulation
        }
        catch (UnityException e)
        {
            Debug.LogErrorFormat("Platform failed to initialize due to exception: %s.", e.Message);
            UnityEngine.Application.Quit();
        }
    }

    //Callback for when user has connected to master server
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(); //Join lobby
    }
    //Callback for when user has joined a lobby
    public override void OnJoinedLobby()
    {
        CheckForAvailableRooms(); //Check room status

    }

    //Function to check if rooms are availble or if one should be created
    private void CheckForAvailableRooms()
    {
        if (PhotonNetwork.CountOfRooms == 0) //No rooms
        {
            //create a room
            Debug.Log("No rooms available, creating a new room...");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2;
            PhotonNetwork.CreateRoom("GameRoom", roomOptions); //Set name of room to GameRoom
        }
        else //room availble
        {
            Debug.Log("Rooms available, joining the existing room...");
            PhotonNetwork.JoinRoom("GameRoom"); //join room with name GameRoom
        }
    }

    //When room has been joined load the GameScene
    public override void OnJoinedRoom()
    {

        Debug.Log("Joined the room successfully: " + PhotonNetwork.CurrentRoom.Name);

        PhotonNetwork.LoadLevel("GameScene");

    }

    //Unused Callback
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join Room Failed: " + message);
    }
}
