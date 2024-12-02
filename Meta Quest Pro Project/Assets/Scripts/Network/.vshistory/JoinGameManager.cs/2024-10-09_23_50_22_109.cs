using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class JoinGameManager : MonoBehaviour
{
    public void joinGame()
    {
        _networkRunner.LoadScene(SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/WaitRoom.unity")), LoadSceneMode.Additive);

    }
}
