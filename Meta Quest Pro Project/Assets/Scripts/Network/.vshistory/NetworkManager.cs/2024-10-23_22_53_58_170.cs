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

    /*
     * client picks random points for spheres
     * sends them to master client
     * spawns balls
     */

 
    // Start is called before the first frame update
    public PhotonView pBall;
    public OVRSpatialAnchor pAnchor;
    public TextMeshProUGUI p1Txt;
    public TextMeshProUGUI p2Txt;
    public TextMeshProUGUI tmpText;

    public GameObject cube;
    private List<GameObject> sphereList;
    
   
    private int numberOfBalls = 4;
    private int numberOfBallsLeft = 4;

    private int maxRounds = 2;
    private int currentRound = 1;
    private string tmpValue;
    private PhotonView photonView_;

    private Vector3 p1;
    private Vector3 p2;
    private bool regP1 = false;
    private bool regP2 = false;
    private bool receivedData = false;

    private Vector3 rp1;
    private Vector3 rp2;

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
        p1 = Vector3.zero;
        p2 = Vector3.zero;
        photonView_ = GetComponent<PhotonView>();
        
        sphereList = new List<GameObject>();

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            Vector3 randomPosition = new Vector3(
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
        }
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

                Debug.Log(players[i].NickName + " " + players[i].UserId);
                tmpValue += "NON MAST CLIENT NAME: " + players[i].NickName;
                break;
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
                p1 = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                p1Txt.text = "P1 Anchor Point: " + p1;
                regP1 = true;
            }
            else if (!regP2)
            {
                p2 = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                p2Txt.text = "P2 Anchor Point: " + p2;
                regP2 = true;
            }
        }
        Vector3 randomPosition = new Vector3(
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


    }
    private IEnumerator Wait()
    {
        isWaiting = true;
        yield return new WaitForSeconds(5);
        tmpText.text = "SENDING TRANSFORM DATA";
        photonView_.RPC("sendTransformData", RpcTarget.MasterClient, p2.x, p2.y, p2.z, p1.x, p1.y, p1.z);
        for(int i = 0; i < numberOfBalls; i++)
        {
            Vector3 randomPosition = new Vector3(
               UnityEngine.Random.Range(0f, 1f),
               UnityEngine.Random.Range(0f, 1f),
               UnityEngine.Random.Range(0f, 1f)
           );
            photonView_.RPC("sendSphereCoords",RpcTarget.MasterClient,randomPosition.x,randomPosition.y,randomPosition.z);
        }
    }

    [PunRPC]
    private void sendSphereCoords(float x, float y, float z)
    {
        int i = sphereList.Count();
        Vector3 pos = new Vector3(x, y, z);
        //spawn spheres at aligned position
        Vector3 translation = rp1 - p1;
        pos += translation;
        Vector3 dirA = rp2 - rp1;
        Vector3 dirB = p2 - p1;
        Quaternion rotation = Quaternion.FromToRotation(dirB, dirA); //maybe apply rotation to position and not rotation of object

        GameObject sphere = PhotonNetwork.Instantiate(pBall.name, pos, rotation);
        
        sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
        sphere.GetComponent<NetworkedSphere>().color = allColors[i];

        sphereList.Add(sphere);
        if(sphereList.Count() == numberOfBalls)
        {
            receivedData = true;
        }
    }

    public void sendWorldPosition()
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
        rp1 = new Vector3(x1, y1, z1);
        rp2 = new Vector3(x2, y2, z2);
        //receivedData = true;
    }

    private void move(out Vector3 pos, out Quaternion rot)
    {
        Vector3 translation = rp1 - p1;
        pos += translation;
        Vector3 dirA = rp2 - rp1;
        Vector3 dirB = p2 - p1;
        Quaternion rotation = Quaternion.FromToRotation(dirB, dirA);
        rot *= rotation;
    }


    private void repositionSpheres()
    {
        GameObject[] spheres = GameObject.FindGameObjectsWithTag("tBall");

        if (spheres.Length == numberOfBalls) {
            Debug.Log("CHANGING POSITIONS");
            tmpText.text = "CHANGING POSITIONS";

            move(cube.transform);
            foreach (GameObject sphere in spheres)
            {
                Debug.Log("OLD POSITION: " + sphere.transform.position);
                move(sphere.transform);
                Debug.Log("NEW POSITION: " + sphere.transform.position);

            }

            hasRepositioned = true;
        }
           
    }

    void Update()
    {
        if (!regP1 || !regP2)
        {
            registerPoints();

        }
        else
        {
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Count() == 2)
            {
                if (!hasSentData)
                {
                    sendWorldPosition();

                    hasSentData = true;
                }
            }
        }
     /*   if (!hasRepositioned && !PhotonNetwork.IsMasterClient && regP1 && regP2 && receivedData)
        {
            repositionSpheres();
        }*/
        


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
