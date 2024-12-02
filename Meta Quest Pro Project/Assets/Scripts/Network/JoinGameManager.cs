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
    private string _oculusUsername;
    private string _oculusUserId;

    

    public void Start()
    {
        _oculusUsername = "";
        _oculusUserId = "";

        try
        {
            Core.AsyncInitialize().OnComplete(InitializeCallback);
            Debug.Log("Oculus Core Initialized");
            
            PhotonNetwork.SendRate = 25;
            PhotonNetwork.SerializationRate = 15;
            

            PhotonNetwork.ConnectUsingSettings();

            PhotonNetwork.NetworkingClient.LoadBalancingPeer.IsSimulationEnabled = true;
/*
            // Configure artificial latency and jitter
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.IncomingLag = 500; // 100ms delay for incoming messages
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingLag = 500; // 100ms delay for outgoing messages
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.IncomingJitter = 500; // 20ms jitter
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingJitter = 500; // 20ms jitter
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.IncomingLossPercentage = 100; // 5% packet loss
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingLossPercentage = 100; // 5% packet loss*/
        }
        catch (UnityException e)
        {
            Debug.LogErrorFormat("Platform failed to initialize due to exception: %s.", e.Message);
            UnityEngine.Application.Quit();
        }


        /*Core.Initialize();
        Debug.Log("Oculus Core Initialized");*/
        //Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);

    }

    void Awake()
    {
        
        
    }

    
    void InitializeCallback(Message msg)
    {
        if (msg.IsError)
        {
            var err = msg.GetError();
            Debug.Log("Platform failed to initialize due to exception: " + err);
            //UnityEngine.Application.Quit();
        }
        else
        {
            Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
        }
    }

    void EntitlementCallback(Message msg)
    {
        if (msg.IsError)
        { 
            var err = msg.GetError();
            Debug.Log("Entitlement check failed: " + " " + msg.GetError().ToString());
            //UnityEngine.Application.Quit();
            //PhotonNetwork.LocalPlayer.NickName = GenerateRandomString(10);
        }
        else
        {
            Debug.Log("You are entitled to use this app.");
            Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
        }
    }

    private void GetLoggedInUserCallback(Message msg)
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
                _oculusUserId = msg.GetUser().ID.ToString();
            }
        }


        Debug.Log("Your name and id are: " + _oculusUsername + " " + _oculusUserId);
        PhotonNetwork.LocalPlayer.NickName = _oculusUserId;//_oculusUsername;
    }

    public void joinGame() // when join game button is pressed 
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
