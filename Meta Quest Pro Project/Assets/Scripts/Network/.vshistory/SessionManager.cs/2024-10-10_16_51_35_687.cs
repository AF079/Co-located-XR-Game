using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class SessionManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
/*
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        int playerCount = lobbyStatistics[0].PlayerCount;
        if (playerCount == 1 && lobbyStatistics[0].RoomCount == 0)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2; 
            PhotonNetwork.CreateRoom("GameRoom", roomOptions);
        }
        else if(playerCount == 1 && lobbyStatistics[0].RoomCount > 0) 
        {
            PhotonNetwork.JoinRoom("GameRoom");

        }
    }*/

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
