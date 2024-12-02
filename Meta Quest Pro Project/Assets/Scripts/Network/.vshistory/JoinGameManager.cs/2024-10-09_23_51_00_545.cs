using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class JoinGameManager : MonoBehaviour
{
    private NetworkRunner _networkRunner
    public void Start()
    {
        _networkRunner = GetComponent<NetworkRunner>();
    }
    public void joinGame()
    {
        _networkRunner.LoadScene(SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/WaitRoom.unity")), LoadSceneMode.Additive);

    }
}
