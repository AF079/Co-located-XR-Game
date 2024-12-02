using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using TMPro;
using System.Linq;

public class NetworkManager_test : MonoBehaviourPunCallbacks
{


    // Start is called before the first frame update
    public PhotonView pBall;
    public TextMeshProUGUI p1Txt;
    public TextMeshProUGUI p2Txt;
    public TextMeshProUGUI p3Txt;
    public TextMeshProUGUI p4Txt;
    public TextMeshProUGUI vectorsTxt;
    public TextMeshProUGUI tmpText;

    public GameObject someCube;

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
        if (PhotonNetwork.IsMasterClient)
        {
            originBinA = Vector3.zero;
            upBinA = Vector3.zero;
        }
        photonView_ = GetComponent<PhotonView>();

        sphereList = new List<GameObject>();

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            Vector3 randomPosition = new Vector3(
                   UnityEngine.Random.Range(0.05f, 2f),
                   UnityEngine.Random.Range(0.05f, 2f),
                   UnityEngine.Random.Range(0.05f, 2f)
               );
            if (!regP1)
            {
                originBinA = randomPosition;
                p1Txt.text = "Origin Anchor Point: " + originBinA;
                regP1 = true;
            }
            randomPosition = new Vector3(
                   UnityEngine.Random.Range(0.05f, 2f),
                   UnityEngine.Random.Range(0.05f, 2f),
                   UnityEngine.Random.Range(0.05f, 2f)
               );
            if (!regP2)
            {
                upBinA = randomPosition + Vector3.up;
                p2Txt.text = "Up(y) Anchor Point: " + upBinA;
                regP2 = true;
            }
            randomPosition = new Vector3(
                   UnityEngine.Random.Range(0.05f, 2f),
                   UnityEngine.Random.Range(0.05f, 2f),
                   UnityEngine.Random.Range(0.05f, 2f)
               );
            if (!regP3)
            {
                sideBinA = randomPosition + Vector3.right;
                p3Txt.text = "Right(x) Anchor Point: " + sideBinA;
                regP3 = true;
            }
            randomPosition = new Vector3(
                   UnityEngine.Random.Range(0.05f, 2f),
                   UnityEngine.Random.Range(0.05f, 2f),
                   UnityEngine.Random.Range(0.05f, 2f)
               );
            if (!regP4)
            {
                forwardBinA = randomPosition + Vector3.forward;
                p4Txt.text = "Forward(z) Anchor Point: " + forwardBinA;
                regP4 = true;
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
        while (PhotonNetwork.PlayerList.Length == 2 && (PhotonNetwork.PlayerList[0].NickName.Length == 0 || PhotonNetwork.PlayerList[1].NickName.Length == 0))
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

                if (ulong.TryParse(players[i].NickName, out ulong userId))
                {
                    Debug.Log(players[i].NickName + " " + players[i].UserId);
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


            GameObject sphere = PhotonNetwork.Instantiate(pBall.name, randomPosition, Quaternion.identity);
            //PhotonView pv = sphere.GetComponent<PhotonView>();

            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            sphere.GetComponent<NetworkedSphere>().color = allColors[i];

            Debug.Log("SPAWNED SPHERE " + i + " AT " + sphere.transform.position);

            sphereList.Add(sphere);
            //sphere.SetActive(false);

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
        photonView_.RPC("sendTransformData", RpcTarget.Others,
            originBinA.x, originBinA.y, originBinA.z,
            upBinA.x, upBinA.y, upBinA.z,
            sideBinA.x, sideBinA.y, sideBinA.z,
            forwardBinA.x, forwardBinA.y, forwardBinA.z);
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
            sideBinA.x, sideBinA.y, sideBinA.z,
            forwardBinA.x, forwardBinA.y, forwardBinA.z);
    }

    public void sendPosition()
    {
        if (!isWaiting)  // Only start the coroutine if it s not already running
        {
            StartCoroutine(Wait());
        }

    }
    [PunRPC]
    private void sendTransformData(float ox, float oy, float oz,
                                    float ux, float uy, float uz,
                                    float sx, float sy, float sz,
                                    float fx, float fy, float fz)
    {
        tmpText.text = "GOT TRANSFORM DATA";
        r_originBinA = new Vector3(ox, oy, oz);
        r_upBinA = new Vector3(ux, uy, uz);
        r_sideBinA = new Vector3(sx, sy, sz);
        r_forwardBinA = new Vector3(fx, fy, fz);
        receivedData = true;
    }
    [PunRPC]
    private void setActiveSignal()
    {
        shouldSetActive = true;
    }

    private Vector3 MatMul(float[,] m1, Vector3 v)
    {

        float x = m1[0, 0] * v[0] + m1[0, 1] * v[1] + m1[0, 2] * v[2];
        float y = m1[1, 0] * v[0] + m1[1, 1] * v[1] + m1[1, 2] * v[2];
        float z = m1[2, 0] * v[0] + m1[2, 1] * v[1] + m1[2, 2] * v[2];

        return new Vector3(x, y, z);
    }

    private Vector3 move(Transform t)

    {

        Vector3 originB = r_originBinA;
        Vector3 upB = (r_upBinA - r_originBinA).normalized;
        Vector3 forwardB = (r_forwardBinA - r_originBinA).normalized;
        Vector3 rightB = (r_sideBinA - r_originBinA).normalized;

        float[,] transormationMatrix = { { rightB.x,   rightB.y,   rightB.z },
                                     { upB.x,      upB.y,      upB.z },
                                     { forwardB.x, forwardB.y, forwardB.z } };

        Vector3 localPosition = t.position - originB;
        Vector3 rotatedPosition = MatMul(transormationMatrix, localPosition);

        //t.position = new Vector3(rotatedPosition.x, rotatedPosition.y, rotatedPosition.z);
        return new Vector3(rotatedPosition.x, rotatedPosition.y, rotatedPosition.z);
    }


    private IEnumerator repositionSpheres()
    {
        isRepositioning = true;
        yield return new WaitForSeconds(5);

        GameObject[] spheres = GameObject.FindGameObjectsWithTag("tBall");

        if (spheres.Length == numberOfBalls)
        {
            Debug.Log("CHANGING POSITIONS");
            tmpText.text = "CHANGING POSITIONS";

            origin.transform.position = move(origin.transform);
            up.transform.position = move(up.transform);
            side.transform.position = move(side.transform);
            forward.transform.position = move(forward.transform);
            someCube.transform.position = move(someCube.transform);
            foreach (GameObject sphere in spheres)
            {
                Vector3 newPos = move(sphere.transform);
                sphere.GetComponent<NetworkedSphere>().move(newPos);

            }

            hasRepositioned = true;
        }


    }
    private bool isRepositioning = false;
    private bool deactivated = false;
    private List<GameObject> r_spheres = new List<GameObject>();
    private bool shouldSetActive = false;
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && (!regP1 || !regP2 || !regP3 || !regP4))
        {
            registerPoints();

        }
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Count() == 2)
        {
            if (!hasSentData)
            {
                sendPosition();

                hasSentData = true;
            }
        }
        if (!hasRepositioned && !PhotonNetwork.IsMasterClient && receivedData) //&& regP1 && regP2 
        {
            if (!isRepositioning)
            {
//                StartCoroutine(repositionSpheres());
                isRepositioning = true;
                //yield return new WaitForSeconds(5);

                GameObject[] spheres = GameObject.FindGameObjectsWithTag("tBall");

                if (spheres.Length == numberOfBalls)
                {
                    Debug.Log("CHANGING POSITIONS");
                    tmpText.text = "CHANGING POSITIONS";

                    origin.transform.position = move(origin.transform);
                    up.transform.position = move(up.transform);
                    side.transform.position = move(side.transform);
                    forward.transform.position = move(forward.transform);
                    someCube.transform.position = move(someCube.transform);
                    foreach (GameObject sphere in spheres)
                    {
                        sphere.GetComponent<NetworkedSphere>().reqOwnership();
                    }
                    foreach (GameObject sphere in spheres)
                    {
                        Vector3 newPos = move(sphere.transform);
                        sphere.GetComponent<NetworkedSphere>().move(newPos);
                    }
                    hasRepositioned = true;
                }
                else
                {
                    isRepositioning = false;
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

