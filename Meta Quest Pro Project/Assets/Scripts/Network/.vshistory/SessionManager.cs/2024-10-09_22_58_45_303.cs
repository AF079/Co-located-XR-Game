using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
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
            Scene = SceneManager.GetActiveScene(),
            PlayerCount = 2,
            
        });
        Debug.Log("Started Game in mode: " + mode);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
