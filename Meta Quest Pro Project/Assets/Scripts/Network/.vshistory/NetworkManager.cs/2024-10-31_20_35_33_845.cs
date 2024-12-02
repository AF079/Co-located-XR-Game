using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using TMPro;
using System.Linq;

public class NetworkManager : MonoBehaviourPunCallbacks
{

 
    // Start is called before the first frame update
    public PhotonView pBall;
    public OVRSpatialAnchor pAnchor;
    public TextMeshProUGUI p1Txt;
    public TextMeshProUGUI p2Txt;
    public TextMeshProUGUI p3Txt;
    public TextMeshProUGUI p4Txt;
    public TextMeshProUGUI vectorsTxt;
    public TextMeshProUGUI tmpText;
    public GameObject canvas1;
    public GameObject canvas2;

    [SerializeField] private OVRCameraRig ovrCameraRig;
    
    public GameObject origin;
    public GameObject up;
    public GameObject side;
    public GameObject forward;

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
    private Vector3 sideBinA;
    private Vector3 forwardBinA;
    private bool regP1 = false;
    private bool regP2 = false;
    private bool regP3 = false;
    private bool regP4 = false;
    private bool receivedData = false;

    private Vector3 r_originBinA;
    private Vector3 r_upBinA;
    private Vector3 r_sideBinA;
    private Vector3 r_forwardBinA;

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

        /*if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            Vector3 randomPosition = new Vector3(
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f)
               );
            if (!regP1)
            {
                originBinA = randomPosition;
                p1Txt.text = "Origin Anchor Point: " + originBinA;
                regP1 = true;
            }
            randomPosition = new Vector3(
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f)
               );
            if (!regP2)
            {
                upBinA = randomPosition + Vector3.up;
                p2Txt.text = "Up(y) Anchor Point: " + upBinA;
                regP2 = true;
            }
            randomPosition = new Vector3(
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f)
               );
            if (!regP3)
            {
                sideBinA = randomPosition + Vector3.right;
                p3Txt.text = "Right(x) Anchor Point: " + sideBinA;
                regP3 = true;
            }
            randomPosition = new Vector3(
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f),
                   UnityEngine.Random.Range(-2f, 2f)
               );
            if (!regP4)
            {
                forwardBinA = randomPosition + Vector3.forward;
                p4Txt.text = "Forward(z) Anchor Point: " + forwardBinA;
                regP4 = true;
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

        while (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == 2 && regP1 && regP2 && regP3 && regP4))
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
                p1Txt.text = "Origin: " + originBinA;
                regP1 = true;
            }
            else if (!regP2)
            {
                upBinA = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                p2Txt.text = "Up(y): " + upBinA;
                regP2 = true;
            }
            else if (!regP3)
            {
                sideBinA = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                p3Txt.text = "Side(x): " + sideBinA;
                regP3 = true;
            }
            else if (!regP4)
            {
                forwardBinA = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                p4Txt.text = "Forward(z): " + forwardBinA;
                regP4 = true;
            }
        }


    }
    private IEnumerator Wait()
    {
        isWaiting = true;
        yield return new WaitForSeconds(5);
        tmpText.text = "SENDING TRANSFORM DATA";
        photonView_.RPC("sendTransformData", RpcTarget.Others,
            originBinA.x, originBinA.y, originBinA.z, 
            upBinA.x, upBinA.y, upBinA.z,
            sideBinA.x,sideBinA.y,sideBinA.z,
            forwardBinA.x,forwardBinA.y,forwardBinA.z);
    }

    public void sendPosition()
    {
        if (!isWaiting)  // Only start the coroutine if it�s not already running
        {
            StartCoroutine(Wait());
        }
        
    }
    [PunRPC]
    private void sendTransformData( float ox,float oy, float oz, 
                                    float ux,float uy, float uz,
                                    float sx,float sy, float sz,
                                    float fx, float fy, float fz)
    {
        tmpText.text = "GOT TRANSFORM DATA";
        r_originBinA = new Vector3(ox,oy,oz);
        r_upBinA = new Vector3(ux,uy,uz);
        r_sideBinA = new Vector3(sx,sy,sz);
        r_forwardBinA = new Vector3(fx,fy,fz); 
        receivedData = true;
    }

    private float[,] Rot_x(float theta)
    {
        float cos_ = Mathf.Cos(theta);
        float sin_ = Mathf.Sin(theta); 
        float[,] mat = { { 1,0,0},{0,cos_,-sin_} ,{0,sin_,cos_} };

        return mat;
    }
    private float[,] Rot_y(float theta)
    {
        float cos_ = Mathf.Cos(theta);
        float sin_ = Mathf.Sin(theta);
        float[,] mat = { { cos_,0,sin_}, { 0,1,0 }, { -sin_,0,cos_} };

        return mat;
    }
    private float[,] Rot_z(float theta)
    {
        float cos_ = Mathf.Cos(theta);
        float sin_ = Mathf.Sin(theta);
        float[,] mat = { { cos_,-sin_,0}, { sin_,cos_,0 }, {0,0,1} };

        return mat;
    }
    private Vector3 MatMul(float[,] m1, Vector3 v)
    {
        // Ensure m1 is 3x3 and m2 is 3x1
        if (m1.GetLength(0) != 3 || m1.GetLength(1) != 3)
        {
            throw new System.ArgumentException("Invalid matrix dimensions. m1 should be 3x3 and m2 should be 3x1.");
        }

        // Multiply m1 (3x3) by m2 (3x1) to get a 3x1 result
        float x = m1[0, 0] * v[0] + m1[0, 1] * v[1] + m1[0, 2] * v[2];
        float y = m1[1, 0] * v[0] + m1[1, 1] * v[1] + m1[1, 2] * v[2];
        float z = m1[2, 0] * v[0] + m1[2, 1] * v[1] + m1[2, 2] * v[2];

        // Return the result as a Vector3
        return new Vector3(x, y, z);
    }
    /*    private void move(Transform t)
        {
            Vector3 u = t.position - r_originBinA;
            Vector3 vu = (r_upBinA - r_originBinA).normalized;


            float theta = Vector3.Angle(Vector3.up,new Vector3(0,vu.y,vu.z));
            float alpha = Vector3.Angle(Vector3.up, new Vector3(vu.x,0,vu.z));
            float gamma = Vector3.Angle(Vector3.up,new Vector3(vu.x,vu.y,0));

            float[,] rot_x = Rot_x(theta);
            float[,] rot_y = Rot_y(alpha);
            float[,] rot_z = Rot_z(gamma);

            Vector3 newPos = MatMul(rot_z,MatMul(rot_y,MatMul(rot_x, u)));
            t.position = new Vector3(newPos.x, newPos.y, newPos.z);

        }*/
    private void Move(Transform t)
    {
        // Step 1: Translate to make t relative to B's origin in A's space
        Vector3 u = t.position - r_originBinA;

        // Step 2: Define the up and forward directions for B in A’s space
        Vector3 vu = (r_upBinA - r_originBinA).normalized;       // Up vector for B in A’s space
        Vector3 vf = (r_forwardBinA - r_originBinA).normalized;  // Forward vector for B in A’s space

        // Step 3: Create a rotation that aligns A’s axes with B’s using LookRotation
        Quaternion rotationToAlign = Quaternion.LookRotation(vf, vu);

        // Step 4: Apply the rotation and then translate to B’s origin
        Vector3 newPos = rotationToAlign * u + r_originBinA;

        // Step 5: Update the transform position in B’s aligned space
        t.position = new Vector3(newPos.x, newPos.y, newPos.z);
    }
    /*Vector3 vs = (r_sideBinA - r_originBinA).normalized;
    Vector3 vf = (r_forwardBinA - r_originBinA).normalized;*/
    /*    Vector3 u = t.position + r_originBinA;
            Vector3 vu = (r_upBinA - r_originBinA).normalized;
            Vector3 vs = (r_sideBinA - r_originBinA).normalized;
            Vector3 vf = (r_forwardBinA - r_originBinA).normalized;

            vectorsTxt.text = vu + " : " + vs + " : " + vf;

            Quaternion rot_y = Quaternion.FromToRotation(Vector3.up, vu);
            Quaternion rot_x = Quaternion.FromToRotation(Vector3.right, vs);
            Quaternion rot_z = Quaternion.FromToRotation(Vector3.forward, vf);

            Vector3 newPos = rot_z * rot_y * rot_x * u;
            t.position = new Vector3(newPos.x, newPos.y, newPos.z);*/


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
            move(side.transform);
            move(forward.transform);
            move(canvas1.transform);
            move(canvas2.transform);
            foreach (GameObject sphere in spheres)
            {

                //Debug.Log("OLD POSITION: " + sphere.transform.position);
                move(sphere.transform);
                //Debug.Log("NEW POSITION: " + sphere.transform.position);
            }

            hasRepositioned = true;
        }
           
    }
    private bool isRepositioning = false;
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && (!regP1 || !regP2 || !regP3 || !regP4))
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
