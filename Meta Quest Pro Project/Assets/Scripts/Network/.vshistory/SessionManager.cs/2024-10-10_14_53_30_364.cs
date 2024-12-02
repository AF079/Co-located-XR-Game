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
        

        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined the room successfully: " + PhotonNetwork.CurrentRoom.Name);

        // Optionally load the game scene
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            // Load game scene only when there are 2 players
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

}
