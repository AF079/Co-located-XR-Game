using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.Threading.Tasks;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    /*
     * create anchor by instantiating object that has OVRSA Component
     * Save anchor using OVRSpatialAnchor.SaveAnchorAsyn(). Wait for finish
     * Share anchor with users using OVRSpatialAnchor.ShareAsync()
     * Broadcast anchors UUID. Can use RPC??
     * Each player can load anchor by its UUID using OVRSpatialAnchor.LoadUnboundSharedAnchorsAsync()
     * Done.
     * Additions:
     * users are represented by and OVRSpaceUser
     * NOTE: consider multiple attempts to perform sharing/loading, not just a single one.
     */
    // Start is called before the first frame update
    public PhotonView pBall;
    public OVRSpatialAnchor pAnchor;
    public TextMeshProUGUI p1Txt;
    public TextMeshProUGUI p2Txt;
    public TextMeshProUGUI tmpText;
    public GameObject PlayerRig;

    private List<OVRSpatialAnchor> spatialAnchors = new List<OVRSpatialAnchor>();
    private List<OVRSpatialAnchor> recievedAnchors = new List<OVRSpatialAnchor> ();
    private List<GameObject> sphereList;
    
    private OVRSpaceUser otherUser; 
   
    private int numberOfBalls = 2;
    private int numberOfBallsLeft = 2;

    private int maxRounds = 2;
    private int currentRound = 1;
    private string tmpValue;
    private PhotonView photonView_;

    private Vector3 p1;
    private Vector3 p2;
    private Vector3 p12;
    private Vector3 rec_p12;
    private Vector3 rec_p1;

    private bool hasSentData = false;

    [HideInInspector]
    public static Color[] allColors = new Color[]
    {
            new Color(1f,1f,1f), //white
            new Color(1f,0f,0f), //red
            new Color(1f,0.5f,0f), //orange
            new Color(1f,1f,0f), //yellow
            new Color(0f,1f,0f), //green
            new Color(0f,1f,1f), //aqua
            new Color(0f,0f,1f), //blue
            new Color(0.5f,0f,1f), //purple
            new Color(1f,0f,1f) //pink

    };

    void Start()
    {
        p1 = Vector3.zero;
        p2 = Vector3.zero;
        photonView_ = GetComponent<PhotonView>();
        StartCoroutine(CheckAndLogPlayerInfo());
        sphereList = new List<GameObject>();

        StartCoroutine(StartTimer());
    }


    private IEnumerator CheckAndLogPlayerInfo()
    {
        // Wait until we're in a room and all players are properly connected
        while (!PhotonNetwork.InRoom || !PhotonNetwork.IsConnected || PhotonNetwork.PlayerList.Length < 2)
        {
            yield return null; // Wait for the next frame
        }
        while(PhotonNetwork.PlayerList.Length == 2 && (PhotonNetwork.PlayerList[0].NickName.Length == 0 || PhotonNetwork.PlayerList[1].NickName.Length == 0))
        {
            yield return null;
        }
        
        Player[] players = PhotonNetwork.PlayerList;

        
        // Check if there are exactly two players in the room
        if (players.Length == 2 && PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < players.Length; i++)
            {
                // Skip the local player
                Debug.Log("NICKNAME i: " + players[i].NickName + " " + players[i].UserId);
                if (players[i].NickName == PhotonNetwork.LocalPlayer.NickName) continue;

                // Try parsing the other player's nickname to extract the userId
                if (ulong.TryParse(players[i].NickName,out ulong userId))
                {
                    
                    Debug.Log(players[i].NickName + " " + players[i].UserId);
                    otherUser = new OVRSpaceUser(userId);
                    tmpValue += "NON MAST CLIENT NAME: " + players[i].NickName;
                    break;
                }
                else
                {
                    Debug.LogError("Failed to parse NickName as a user ID: " + players[i].NickName);
                }
            }
        }

        // Ensure the master client and local player info are properly logged
        if (!string.IsNullOrEmpty(PhotonNetwork.MasterClient.NickName))
        {
            Debug.Log("USER INFO OF MASTER CLIENT: " + PhotonNetwork.MasterClient.NickName + " " + PhotonNetwork.MasterClient.UserId);
      
        }
        else
        {
            Debug.LogWarning("MasterClient NickName is empty or null.");
        }

        if (!string.IsNullOrEmpty(PhotonNetwork.LocalPlayer.NickName))
        {
            Debug.Log("USER INFO OF This CLIENT: " + PhotonNetwork.LocalPlayer.NickName + " " + PhotonNetwork.LocalPlayer.UserId);
        }
        else
        {
            Debug.LogWarning("LocalPlayer NickName is empty or null.");
        }
        tmpText.text = tmpValue + " USER INFO OF MASTER CLIENT: " + PhotonNetwork.MasterClient.NickName;
    }

    private IEnumerator spawnBalls()
    {
        for (var i = 0; i < numberOfBalls; i++)
        {
            Vector3 randomPosition = new Vector3(
               UnityEngine.Random.Range(0f, 1f),
               UnityEngine.Random.Range(0f, 1f),
               UnityEngine.Random.Range(0f, 1f)
           );
          
            GameObject sphere = PhotonNetwork.Instantiate(pBall.name,randomPosition, Quaternion.identity);
           
            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            sphere.GetComponent<NetworkedSphere>().color = allColors[i];

            sphereList.Add(sphere);

        }
        yield return null;

    }

    private IEnumerator StartTimer()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) yield break;

        while (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == 2))
        {
            Debug.Log("Waiting for second player to join!");
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(5);

        Debug.Log("5 seconds passed, conditions met!");
        yield return StartCoroutine(spawnBalls());  
    }

    private void registerPoints()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            if (p1 == Vector3.zero)
            {
                p1 = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                p1Txt.text = "P1 Anchor Point: " + p1;
            }
            else if (p2 == Vector3.zero)
            {
                p2 = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                p2Txt.text = "P2 Anchor Point: " + p2;
            }
        }

    }

    public void sendPosition()
    {
        p12 = p2 - p1;

        photonView_.RPC("sendTransformData", RpcTarget.Others, p12.x, p12.y, p12.z, p1.x, p1.y, p1.z);
    }

    [PunRPC]
    private void sendTransformData(float x12, float y12, float z12, float x1, float y1, float z1)
    {
        Debug.Log("BEFORE POS: " + PlayerRig.transform.position + " ROT: " + PlayerRig.transform.rotation);

        rec_p12 = new Vector3(x12, y12, z12);
        rec_p1 = new Vector3(x1, y1, z1);
        Vector3 translation = rec_p1 - p1;
        PlayerRig.transform.position += translation;
        Quaternion rotation = Quaternion.FromToRotation(p12, rec_p12);
        PlayerRig.transform.rotation *= rotation;
        Debug.Log("AFTER POS: " + PlayerRig.transform.position + " ROT: " +  PlayerRig.transform.rotation);
    }

    void Update()
    {
        if (p1 == Vector3.zero || p2 == Vector3.zero)
        {
            registerPoints();

        }
        else
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
            {
                if (!hasSentData)
                {
                    sendPosition();
                    
                    hasSentData = true;
                }
            }
            
        }
        


        /*        if (PhotonNetwork.IsMasterClient && currentRound < maxRounds && sphereList.Count == numberOfBalls)
                {
                    for (int i = 0; i < sphereList.Count; i++)
                    {
                        bool isActive = sphereList[i].activeSelf;
                        if (!isActive)
                        {
                            numberOfBallsLeft--;
                        }
                    }
                    //Debug.Log("Number of Balls left " + numberOfBallsLeft);
                    if (numberOfBallsLeft == 0)
                    {
                        StartCoroutine(StartTimer());
                        currentRound++;
                        numberOfBallsLeft = numberOfBalls;
                    }
                    else
                    {
                        numberOfBallsLeft = numberOfBalls;
                    }
                }*/
    }
}
