using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Linq;
public class SessionManager : MonoBehaviour
{
    private NetworkRunner netRunner;
    // Start is called before the first frame update
    void Start()
    {
        netRunner = GetComponent<NetworkRunner>();
        StartGame(GameMode.Shared);
    }

    private async void StartGame(GameMode mode)
    {
        await netRunner.StartGame(new StartGameArgs() { 
            GameMode = mode,
            SessionName = "MyRoom",
            Scene = "WaitRoom",
            PlayerCount = 2,
            
        });
        Debug.Log("Started Game in mode: " + mode);

    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player Joined: " + player.PlayerId);
        if(runner.ActivePlayers.Count() == 2)
        {
            LoadGameScene();
        }
        private void LoadGameScene()
        {
            Debug.Log("Both players have joined, loading game scene");
            await _networkRunner.SetActiveScene("CollabGameScene");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
