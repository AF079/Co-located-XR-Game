using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class JoinGameManager : MonoBehaviour
{
    public void joinGame()
    {
        SceneManager.LoadScene("WaitRoom");
    }
}
