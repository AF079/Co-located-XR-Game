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
    public GameObject ce;
    public GameObject le;
    public GameObject re;
    public GameObject lh;
    public GameObject rh;

    private List<OVRSpatialAnchor> spatialAnchors = new List<OVRSpatialAnchor>();
    private List<OVRSpatialAnchor> recievedAnchors = new List<OVRSpatialAnchor> ();
    private List<GameObject> sphereList;
    
    private OVRSpaceUser otherUser; 
   
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

        /*if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
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
        OVRManager.CameraDevice

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
        /* Vector3 randomPosition = new Vector3(
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
        yield return new WaitForSeconds(35);
        tmpText.text = "SENDING TRANSFORM DATA";
        photonView_.RPC("sendTransformData", RpcTarget.Others, p2.x, p2.y, p2.z, p1.x, p1.y, p1.z);
    }

    public void sendPosition()
    {
        if (!isWaiting)  // Only start the coroutine if it�s not already running
        {
            StartCoroutine(Wait());
        }
        
    }

    private float normalize(float p, float pMax, float pMin) { 
        return (p-pMin)/(pMax - pMin);
    }

    private float TrilinearInterpolation(float u, float v, float w,float[] points)
    {
        float a = 1 - u;
        float b = 1 - v;
        float c = 1 - w;
        float res = a * b * c * points[0] + 
                    u * b * c * points[1] + 
                    a * v * c * points[2] + 
                    u * v * c * points[3] + 
                    a * b * w * points[4] + 
                    u * b * w * points[5] + 
                    a * v * w * points[6] +
                    u * v * w * points[7];
        
        return res;
    }
/*    private Vector3 getInterpolatedPoint(Vector3 pointToInterpolate)
    {
        Vector3 rp000 = new Vector3(Mathf.Min(rp1.x, rp2.x), Mathf.Min(rp1.y, rp2.y), Mathf.Min(rp1.z, rp2.z));
        Vector3 rp111 = new Vector3(Mathf.Max(rp1.x, rp2.x), Mathf.Max(rp1.y, rp2.y), Mathf.Max(rp1.z, rp2.z));

        Vector3 p000 = new Vector3(Mathf.Min(p1.x, p2.x), Mathf.Min(p1.y, p2.y), Mathf.Min(p1.z, p2.z));
        Vector3 p111 = new Vector3(Mathf.Max(p1.x, p2.x), Mathf.Max(p1.y, p2.y), Mathf.Max(p1.z, p2.z));
        Vector3 p100 = new Vector3(p111.x, p000.y, p000.z);
        Vector3 p010 = new Vector3(p000.x, p111.y, p000.z);
        Vector3 p110 = new Vector3(p111.x, p111.y, p000.z);
        Vector3 p001 = new Vector3(p000.x, p000.y, p111.z);
        Vector3 p101 = new Vector3(p111.x, p000.y, p111.z);
        Vector3 p011 = new Vector3(p000.x, p111.y, p111.z);
        Vector3[] points = { p000, p100, p010, p110, p001, p101, p011, p111 };
        float[] X = points.Select(point => point.x).ToArray();
        float[] Y = points.Select(point => point.y).ToArray();
        float[] Z = points.Select(point => point.z).ToArray();


        float x = pointToInterpolate.x;
        float y = pointToInterpolate.y;
        float z = pointToInterpolate.z;
        float u = normalize(x, rp111.x, rp000.x);
        float v = normalize(y, rp111.y, rp000.y);
        float w = normalize(z, rp111.z, rp000.z);
        
        float newX = TrilinearInterpolation(u, v, w, X);
        float newY = TrilinearInterpolation(u, v, w, Y);
        float newZ = TrilinearInterpolation(u, v, w, Z);
        Vector3 res = new Vector3(newX, newY, newZ);
        Debug.Log("INTERPOLATED NEW POSITION: " + res);
        return res;

    }
*/
    [PunRPC]
    private void sendTransformData(float x2, float y2, float z2, float x1, float y1, float z1)
    {
        tmpText.text = "GOT TRANSFORM DATA";
        rp1 = new Vector3(x1, y1, z1);
        rp2 = new Vector3(x2, y2, z2);
        receivedData = true;
    }

    private void move(Transform t)
    {


        Vector3 translation = rp1 - p1;
        Vector3 newPos = t.position;
        newPos += translation;
        t.position = new Vector3(newPos.x, newPos.y, newPos.z);
        Vector3 dirA = rp2 - rp1;
        Vector3 dirB = p2 - p1;
        Quaternion rotation = Quaternion.FromToRotation(dirB, dirA);
        t.rotation *= rotation;
    }


    private void repositionSpheres()
    {
        GameObject[] spheres = GameObject.FindGameObjectsWithTag("tBall");

        if (spheres.Length == numberOfBalls) {
            Debug.Log("CHANGING POSITIONS");
            tmpText.text = "CHANGING POSITIONS";
            Debug.Log("BEFORE CENTER EYE " + ovrCameraRig.centerEyeAnchor.position + " " + ovrCameraRig.centerEyeAnchor.rotation);

            /*move(le.transform);
            move(re.transform);
            move(ce.transform);
            move(lh.transform);
            move(rh.transform);*/
            move(GetComponent<Camera>().transform);

            /*move(ovrCameraRig.centerEyeAnchor);
            move(ovrCameraRig.leftEyeAnchor);
            move(ovrCameraRig.rightEyeAnchor);
            move(ovrCameraRig.leftControllerAnchor);
            move(ovrCameraRig.rightControllerAnchor);*/

            Debug.Log("AFTER CENTER EYE " + ovrCameraRig.centerEyeAnchor.position + " " + ovrCameraRig.centerEyeAnchor.rotation);

            /*Quaternion rot = Quaternion.FromToRotation(Vector3.forward, p1);
            foreach (GameObject sphere in spheres)
            {
                
                Debug.Log("OLD POSITION: " + sphere.transform.position);
                Vector3 newPos = sphere.transform.position;
                newPos += p1;
                newPos = rot * newPos;
                sphere.transform.position = new Vector3(newPos.x, newPos.y, newPos.z);

                Debug.Log("NEW POSITION: " + sphere.transform.position);
                
            }*/

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
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Count() == 2)
            {
                if (!hasSentData)
                {
                    sendPosition();

                    hasSentData = true;
                }
            }
        }
        if (!hasRepositioned && !PhotonNetwork.IsMasterClient && regP1 && regP2 && receivedData)
        {
            repositionSpheres();
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
