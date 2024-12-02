using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.Threading.Tasks;
using TMPro;
using System.Linq;
using System.Net.NetworkInformation;
using System.Xml;

public class NetworkManager : MonoBehaviourPunCallbacks
{

 
    // Start is called before the first frame update
    public PhotonView pBall;
    public OVRSpatialAnchor pAnchor;
    public TextMeshProUGUI p1Txt;
    public TextMeshProUGUI p2Txt;
    public TextMeshProUGUI tmpText;

    [SerializeField] private OVRCameraRig ovrCameraRig;
    
    public GameObject origin;
    public GameObject up;
    private List<GameObject> sphereList;
    
    private OVRSpaceUser otherUser; 
   
    private int numberOfBalls = 4;
    private int numberOfBallsLeft = 4;

    private int maxRounds = 2;
    private int currentRound = 1;
    private string tmpValue;
    private PhotonView photonView_;

    private Vector3 originBinA;
    private Vector3 upBinA;
    private bool regP1 = false;
    private bool regP2 = false;
    private bool receivedData = false;

    private Vector3 r_originBinA;
    private Vector3 r_upBinA;

    private bool hasRepositioned = false;
    private bool hasSentData = false;
    private bool isWaiting = false;

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
        //ovrCameraRig = GameObject.Find("OVRCameraRig").GetComponent<OVRCameraRig>();
        if(PhotonNetwork.IsMasterClient)
        {
            originBinA = Vector3.zero;
            upBinA = Vector3.zero;
        }
        photonView_ = GetComponent<PhotonView>();
        
        sphereList = new List<GameObject>();

/*        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            Vector3 randomPosition = new Vector3(
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f)
               );
            if (!regP1)
            {
                originBinA = randomPosition;
                p1Txt.text = "P1 Anchor Point: " + originBinA;
                regP1 = true;
            }
            *//*randomPosition = new Vector3(
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f)
               );*//*
            if (!regP2)
            {
                upBinA = randomPosition + Vector3.up;
                p2Txt.text = "P2 Anchor Point: " + upBinA;
                regP2 = true;
            }
        }*/
        StartCoroutine(CheckAndLogPlayerInfo());
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

            Debug.Log("SPAWNED SPHERE " + i + " AT " + sphere.transform.position);

            sphereList.Add(sphere);

        }
        yield return null;

    }

    private IEnumerator StartTimer()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) yield break;

        while (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == 2 && regP1 && regP2))
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
            if (!regP1)
            {
                originBinA = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                p1Txt.text = "P1 ORIGIN Anchor Point: " + originBinA;
                regP1 = true;
            }
            else if (!regP2)
            {
                upBinA = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                p2Txt.text = "P2 UP Anchor Point: " + upBinA;
                regP2 = true;
            }
        }
/*        Vector3 randomPosition = new Vector3(
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f)
               );
        if (!regP1)
        {
            p1 = randomPosition + new Vector3(0.01f, 0.01f, 0.01f);
            p1Txt.text = "P1 Anchor Point: " + p1;
            regP1 = true;
        }
        randomPosition = new Vector3(
               UnityEngine.Random.Range(-2f, 2f),
               UnityEngine.Random.Range(-2f, 2f),
               UnityEngine.Random.Range(-2f, 2f)
           );
        if (!regP2)
        {
            p2 = randomPosition + new Vector3(0.01f, 0.01f, 0.01f);
            p2Txt.text = "P2 Anchor Point: " + p2;
            regP2 = true;
        }
*/

    }
    private IEnumerator Wait()
    {
        isWaiting = true;
        yield return new WaitForSeconds(5);
        tmpText.text = "SENDING TRANSFORM DATA";
        photonView_.RPC("sendTransformData", RpcTarget.Others, upBinA.x, upBinA.y, upBinA.z, originBinA.x, originBinA.y, originBinA.z);
    }

    public void sendPosition()
    {
        if (!isWaiting)  // Only start the coroutine if it�s not already running
        {
            StartCoroutine(Wait());
        }
        
    }
    [PunRPC]
    private void sendTransformData(float x2, float y2, float z2, float x1, float y1, float z1)
    {
        tmpText.text = "GOT TRANSFORM DATA";
        r_originBinA = new Vector3(x1, y1, z1);
        r_upBinA = new Vector3(x2, y2, z2);
        receivedData = true;
    }

    private void move(Transform t)
    {
        Vector3 u = t.transform.position - r_originBinA;
        Vector3 v = r_upBinA - r_originBinA;
        Vector3 v_ = new Vector3(r_originBinA.x, r_originBinA.y + 1, r_originBinA.z) - r_originBinA; // 1 also try r_originBinA.z instead of r_upBinA.z , 2 new Vector3(r_originBinA.x,r_upBinA.y, r_originBinA.z)
        Quaternion rot = Quaternion.FromToRotation(v_, v);
        Vector3 newPos = rot * u;
        t.transform.position = new Vector3(newPos.x, newPos.y,newPos.z); 
    }


    private IEnumerator repositionSpheres()
    {
        isRepositioning = true;
        yield return new WaitForSeconds(5);
        GameObject[] spheres = GameObject.FindGameObjectsWithTag("tBall");

        if (spheres.Length == numberOfBalls) {
            Debug.Log("CHANGING POSITIONS");
            tmpText.text = "CHANGING POSITIONS";

            move(origin.transform);
            move(up.transform);
            foreach (GameObject sphere in spheres)
            {

                Debug.Log("OLD POSITION: " + sphere.transform.position);
                move(sphere.transform);
                Debug.Log("NEW POSITION: " + sphere.transform.position);
            }

            hasRepositioned = true;
        }
           
    }
    private bool isRepositioning = false;
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && !regP1 || !regP2)
        {
            registerPoints();

        }
        else
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Count() == 2)
            {
                if (!hasSentData)
                {
                    sendPosition();

                    hasSentData = true;
                }
            }
        }
        if (!hasRepositioned && !PhotonNetwork.IsMasterClient && receivedData) //&& regP1 && regP2 
        {
            if (!isRepositioning)
            {
                StartCoroutine(repositionSpheres());

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
