using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Linq;
using Fusion.Sockets;
using System;
public class SessionManager : MonoBehaviour,INetworkRunnerCallbacks
{
    private NetworkRunner _networkRunner;

    private void Start()
    {
        _networkRunner = GetComponent<NetworkRunner>();

        // Register the INetworkRunnerCallbacks to receive events
        _networkRunner.AddCallbacks(this);

        // Start a Fusion session in Shared mode
        StartSession(GameMode.Shared);
    }

    // Start the Photon Fusion session
    private async void StartSession(GameMode mode)
    {
        await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "WaitRoom", // Name of the session
            Scene = SceneManager.GetActiveScene().buildIndex, // WaitRoom Scene
            PlayerCount = 2, // Two players needed for the game
            DisableClientSessionCreation = false,
        });

        Debug.Log("Waiting for another player...");
    }

    // This is called automatically when a new player joins the session
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player joined: " + player.PlayerId);

        // Check if two players are in the session
        if (runner.ActivePlayers.Count() == 2)
        {
            // Load the game scene for both players
            LoadGameScene();
        }
    }

    // Load the GameScene when both players have joined the WaitRoom
    private void LoadGameScene()
    {
        Debug.Log("Both players are here, loading the GameScene...");

        _networkRunner.LoadScene(SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/GameScene.unity")), LoadSceneMode.Additive);
    }

    // Other required callback implementations for INetworkRunnerCallbacks (empty for now)
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}