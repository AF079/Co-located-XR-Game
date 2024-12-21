using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using TMPro;
using System.Linq;
using ExitGames.Client.Photon;
using Unity.Mathematics;

public class NetworkManager : MonoBehaviourPunCallbacks
{


    //Latency control
    //Lag: delay between sending a packet of data from source to target
    public static int CLIENT_LATENCY = 10; //Simulates fixed delay for incoming packets from server to client

    //GUI control
    public GameObject canvas1;
    public GameObject canvas2;
    public PhotonView pBall;
    public TextMeshProUGUI p1Txt;
    public TextMeshProUGUI p2Txt;
    public TextMeshProUGUI p3Txt;
    public TextMeshProUGUI p4Txt;
    public TextMeshProUGUI vectorsTxt;
    public TextMeshProUGUI tmpText;

    //Photon variables
    private PhotonView photonView_;
    private PhotonPeer photonPeer_;

    //Alignment cubes
    public GameObject origin;
    public GameObject up;
    public GameObject side;
    public GameObject forward;

    //List maintaining list of spheres
    private List<GameObject> sphereList;

    //Variables for handling multiple rounds
    public static int numberOfBalls = 3;
    private int numberOfBallsLeft = 3;

    private int maxRounds = 2;
    private int currentRound = 1;
    private string tmpValue;


    //Vector3's to store aligned positions 
    public static Vector3 originBinA;
    public static Vector3 upBinA;
    public static Vector3 sideBinA;
    public static Vector3 forwardBinA;

    //Status flags
    private bool regP1 = false;
    private bool regP2 = false;
    private bool regP3 = false;
    private bool regP4 = false;
    private bool receivedData = false;
    private bool spawnedBalls = false;
    private bool hasRepositioned = false;
    public static float[,] transformationMatrix;


    //List of color options that the spheres can take on
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
        //Initialisation 
        photonView_ = GetComponent<PhotonView>();
        photonPeer_ = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
        sphereList = new List<GameObject>();


        //Coroutine for ensuring there are two players in the room before continuing
        StartCoroutine(WaitFor2Players());

        //Master client send registered points coroutine
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SendRegisteredPoints());

        }
        //Non master client begin latency simulation coroutine
        else
        {
            StartCoroutine(SimulateLatencyForNMC());
        }

    }

  
    private IEnumerator WaitFor2Players()
    {
        // Wait until we're in a room and all players are properly connected
        while (!PhotonNetwork.InRoom || !PhotonNetwork.IsConnected)// || PhotonNetwork.PlayerList.Length < 2
        {
            yield return null;
        }

        // Check if there are exactly two players in the room
        if (PhotonNetwork.IsMasterClient)
        {
            tmpText.text = tmpValue + " Second player joined";
        }
        else
        {
            tmpText.text = tmpValue + " Joined master client room";
        }

    }
    //Function to disable GUI elements when the game starts
    private void disableGUI()
    {

        origin.SetActive(false);
        side.SetActive(false);
        up.SetActive(false);
        forward.SetActive(false);
        //canvas1.SetActive(false);
        //canvas2.SetActive(false);

    }


    /*
    Coroutine to handle the spawning (instantiation) of spheres.
    Each sphere spawns in a random position within a random range, color is assigned to the sphere, and populating sphereList
    */
    private IEnumerator spawnBalls()
    {
        //Disable GUI
        disableGUI();
        //Iterate over the number of spheres to spawn
        for (var i = 0; i < numberOfBalls; i++)
        {
            //Random offset vector to add to the position of the spheres, x= [-1,1], y = [1,2], z = [-1,1]
            Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(1f, 2f), UnityEngine.Random.Range(-1f, 1f));
            //Spawn the sphere at (0,1,0) + offset vector
            GameObject sphere = PhotonNetwork.Instantiate(pBall.name, Vector3.up + randomOffset, Quaternion.identity);

            //Assign color
            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            sphere.GetComponent<NetworkedSphere>().color = allColors[i];

            Debug.Log("SPAWNED SPHERE " + i + " AT " + sphere.transform.position);
            //Add sphere to list
            sphereList.Add(sphere);

        }
        spawnedBalls = true;
        yield return null;

    }

    //Function that sends that first waits until all the points have been registered, then sends the points to NMC
    private IEnumerator SendRegisteredPoints()
    {
        while (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && regP1 && regP2 && regP3 && regP4)) // && PhotonNetwork.PlayerList.Length == 2
        {
            Debug.Log("Waiting for second player to join!");
            yield return new WaitForSeconds(1f);
        }

        //Not needed
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

    private void autoRegisterPoints()
    {
        if (!regP1)
        {
            originBinA = Vector3.zero;
            p1Txt.text = "Origin Anchor Point: " + originBinA;
            regP1 = true;
        }

        if (!regP2)
        {
            upBinA = Vector3.up;
            p2Txt.text = "Up(y) Anchor Point: " + upBinA;
            regP2 = true;
        }


        if (!regP3)
        {
            sideBinA = -Vector3.right;
            p3Txt.text = "Right(x) Anchor Point: " + sideBinA;
            regP3 = true;
        }


        if (!regP4)
        {
            forwardBinA = -Vector3.forward;
            p4Txt.text = "Forward(z) Anchor Point: " + forwardBinA;
            regP4 = true;
        }


    }

    [PunRPC]
    private void sendTransformData(float ox, float oy, float oz,
                                    float ux, float uy, float uz,
                                    float sx, float sy, float sz,
                                    float fx, float fy, float fz)
    {
        tmpText.text = "GOT TRANSFORM DATA";
        originBinA = new Vector3(ox, oy, oz);
        upBinA = new Vector3(ux, uy, uz);
        sideBinA = new Vector3(sx, sy, sz);
        forwardBinA = new Vector3(fx, fy, fz);
        receivedData = true;

        Vector3 upB = (upBinA - originBinA).normalized;
        Vector3 forwardB = (forwardBinA - originBinA).normalized;
        Vector3 rightB = (sideBinA - originBinA).normalized;

        transformationMatrix = new float[,]{ { rightB.x,   rightB.y,   rightB.z },
                                     { upB.x,      upB.y,      upB.z },
                                     { forwardB.x, forwardB.y, forwardB.z } };
        disableGUI();
    }

    public static Vector3 MatMul(float[,] m1, Vector3 v)
    {

        float x = m1[0, 0] * v[0] + m1[0, 1] * v[1] + m1[0, 2] * v[2];
        float y = m1[1, 0] * v[0] + m1[1, 1] * v[1] + m1[1, 2] * v[2];
        float z = m1[2, 0] * v[0] + m1[2, 1] * v[1] + m1[2, 2] * v[2];

        return new Vector3(x, y, z);
    }



    public static Vector3 move(Vector3 v)
    {
        Vector3 localPosition = v - originBinA;
        Vector3 rotatedPosition = MatMul(transformationMatrix, localPosition);
        return new Vector3(rotatedPosition.x, rotatedPosition.y, rotatedPosition.z);
    }

    private IEnumerator SimulateLatencyForNMC()
    {
        while (!spawnedBalls)
        {
            yield return null;

        }
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            int rand = UnityEngine.Random.Range(0, 101);
            if (rand > 50)
            {
                /* PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.IncomingJitter = IN_JITTER;
                 PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingJitter = OUT_JITTER;*/
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingLag = CLIENT_LATENCY;
                //PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.IncomingLag = IN_LOSS_CHANCE;
                Debug.Log("Adding latency!!");
            }
            else
            {
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingLag = 0;

            }
        }
    }
    void Update()
    {

        tmpText.text = LogHandler.hasSaved;

        if (PhotonNetwork.IsMasterClient && (!regP1 || !regP2 || !regP3 || !regP4))
        {
            //registerPoints();
            autoRegisterPoints();

        }

        if (!hasRepositioned && !PhotonNetwork.IsMasterClient && receivedData) //&& regP1 && regP2 
        {
            origin.transform.position = move(origin.transform.position);
            up.transform.position = move(up.transform.position);
            side.transform.position = move(side.transform.position);
            forward.transform.position = move(forward.transform.position);
            hasRepositioned = true;

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

