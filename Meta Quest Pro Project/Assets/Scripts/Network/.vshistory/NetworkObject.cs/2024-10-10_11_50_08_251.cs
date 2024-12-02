using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    private int playersTouching = 0;

    private GameObject sphere;

    void Start()
    {
        sphere = gameObject; 
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void PlayerTouchedSphere()
    {
        playersTouching++;

        if (playersTouching >= 2)
        {
            sphere.SetActive(false);

            playersTouching = 0;
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void PlayerStoppedTouchingSphere()
    {
        playersTouching = Mathf.Max(0, playersTouching - 1);
    }

    // Detect collisions using OnTriggerEnter and OnTriggerExit
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand")) 
        {
            PlayerTouchedSphere();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            PlayerStoppedTouchingSphere();
        }
    }
}
