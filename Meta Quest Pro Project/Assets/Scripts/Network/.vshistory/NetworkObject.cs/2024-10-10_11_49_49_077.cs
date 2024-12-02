using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    // Start is called before the first frame update
    private int playersTouching = 0;

    // Reference to the sphere's GameObject
    private GameObject sphere;

    // Start is called before the first frame update
    void Start()
    {
        sphere = gameObject; // Assume this script is attached to the sphere
    }

    // Called when a player touches the sphere (collision detected)
    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void PlayerTouchedSphere()
    {
        // Increment the number of players touching
        playersTouching++;

        // If two players are touching the sphere at the same time
        if (playersTouching >= 2)
        {
            // Make the sphere disappear
            sphere.SetActive(false);

            // Reset the players touching count
            playersTouching = 0;
        }
    }

    // Called when a player stops touching the sphere (collision ends)
    [Rpc(sources: RpcSources.All, targets: RpcTargets.All)]
    public void PlayerStoppedTouchingSphere()
    {
        // Decrement the number of players touching
        playersTouching = Mathf.Max(0, playersTouching - 1);
    }

    // Detect collisions using OnTriggerEnter and OnTriggerExit
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand")) // Assuming the player's hands have a "PlayerHand" tag
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
