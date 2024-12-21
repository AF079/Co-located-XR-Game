using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SetupManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public GameObject ClientManager;
    public GameObject NetworkManager;
    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    void Start()
    {
/*        PhotonNetwork.LocalPlayer.NickName = GenerateRandomString(10);
        Debug.Log(PhotonNetwork.LocalPlayer.ActorNumber);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(NetworkManager.name,Vector3.zero,Quaternion.identity);
        }
        else
        {
            PhotonNetwork.Instantiate(ClientManager.name, Vector3.zero, Quaternion.identity);
        }*/
    }
    public static string GenerateRandomString(int length)
    {
        StringBuilder result = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            result.Append(chars[Random.Range(0, chars.Length)]);
        }
        return result.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
