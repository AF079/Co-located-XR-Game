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

    //Alignment cubes positioned at postive unit vectors O = (0,0,0), x = (1,0,0), y = (0,1,0), z = (0,0,1)
    public GameObject origin;
    public GameObject yUnitVector;
    public GameObject xUnitVector;
    public GameObject zUnitVector;

    //List maintaining list of spheres
    private List<GameObject> sphereList;

    //Variables for handling multiple rounds
    public static int numberOfSpheres = 3;
    private int numberOfSpheresLeft = 3;

    private int maxRounds = 2;
    private int currentRound = 1;
    private string tmpValue;


    //Vector3's to store aligned positions 
    public static Vector3 originBinA;
    public static Vector3 yBinA;
    public static Vector3 xBinA;
    public static Vector3 zBinA;

    //Status flags
    private bool regOrigin = false;
    private bool regY = false;
    private bool regX = false;
    private bool regZ = false;
    private bool receivedData = false;
    private bool spawnedSpheres = false;
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
        while (!PhotonNetwork.InRoom || !PhotonNetwork.IsConnected || PhotonNetwork.PlayerList.Length < 2)
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
        xUnitVector.SetActive(false);
        yUnitVector.SetActive(false);
        zUnitVector.SetActive(false);
        canvas1.SetActive(false);
        canvas2.SetActive(false);

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
        for (var i = 0; i < numberOfSpheres; i++)
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
        spawnedSpheres = true;
        yield return null;

    }

    //Function that sends that first waits until all the points have been registered, then sends the points to NMC
    private IEnumerator SendRegisteredPoints()
    {
        while (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && regOrigin && regX && regY && regZ && PhotonNetwork.PlayerList.Length == 2)) 
        {
            Debug.Log("Waiting for second player to join!");
            yield return new WaitForSeconds(1f);
        }

        //*NOTE* For the future, this wont be needed anymore.
        photonView_.RPC("sendTransformData", RpcTarget.Others,
            originBinA.x, originBinA.y, originBinA.z,
            yBinA.x, yBinA.y, yBinA.z,
            xBinA.x, xBinA.y, xBinA.z,
            zBinA.x, zBinA.y, zBinA.z);
        yield return new WaitForSeconds(5);

        Debug.Log("5 seconds passed, conditions met!");
        yield return StartCoroutine(spawnBalls());
    }

    //Register the points. User preseses right trigger on controller and stores the position of there the controller at the time of pressing.
    private void registerPoints()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            if (!regOrigin) //origin flag
            {
                originBinA = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch); //Register point
                p1Txt.text = "Origin: " + originBinA;
                regOrigin = true; //update flag
            }
            else if (!regY) //y flag
            {
                yBinA = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch); //Register point
                p2Txt.text = "Up(y): " + yBinA;
                regY = true; //update flag
            }
            else if (!regX) //x flag
            {
                xBinA = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch); //Register point
                p3Txt.text = "Side(x): " + xBinA;
                regX = true; //update flag
            }
            else if (!regZ) //z flag
            {
                zBinA = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch); //Register point
                p4Txt.text = "Forward(z): " + zBinA;
                regZ = true; //update flag
            } 
        }


    }

    //Same as the function above except this is for testing, program automatically selects points
    private void test_autoRegisterPoints()
    {
        if (!regOrigin)
        {
            originBinA = Vector3.zero;
            p1Txt.text = "Origin Anchor Point: " + originBinA;
            regOrigin = true;
        }

        if (!regY)
        {
            yBinA = Vector3.up;
            p2Txt.text = "Up(y) Anchor Point: " + yBinA;
            regY = true;
        }


        if (!regX)
        {
            xBinA = -Vector3.right;
            p3Txt.text = "Right(x) Anchor Point: " + xBinA;
            regX = true;
        }


        if (!regZ)
        {
            zBinA = -Vector3.forward;
            p4Txt.text = "Forward(z) Anchor Point: " + zBinA;
            regZ = true;
        }


    }

    //*NOTE* This function will be removed in the future except for disabling the GUI.
    [PunRPC]
    private void sendTransformData(float ox, float oy, float oz,
                                    float ux, float uy, float uz,
                                    float sx, float sy, float sz,
                                    float fx, float fy, float fz)
    {
        tmpText.text = "GOT TRANSFORM DATA";
        originBinA = new Vector3(ox, oy, oz);
        yBinA = new Vector3(ux, uy, uz);
        xBinA = new Vector3(sx, sy, sz);
        zBinA = new Vector3(fx, fy, fz);
        receivedData = true;

        Vector3 yB = (yBinA - originBinA).normalized;
        Vector3 zB = (zBinA - originBinA).normalized;
        Vector3 xB = (xBinA - originBinA).normalized;

        transformationMatrix = new float[,]{ { xB.x,   xB.y,   xB.z },
                                     { yB.x,      yB.y,      yB.z },
                                     { zB.x, zB.y, zB.z } };
        disableGUI();
    }
    
    //Function that multiplies matrix m and vector v
    public static Vector3 MatMul(float[,] m, Vector3 v)
    {

        float x = m[0, 0] * v[0] + m[0, 1] * v[1] + m[0, 2] * v[2];
        float y = m[1, 0] * v[0] + m[1, 1] * v[1] + m[1, 2] * v[2];
        float z = m[2, 0] * v[0] + m[2, 1] * v[1] + m[2, 2] * v[2];

        return new Vector3(x, y, z);
    }


    //*NOTE* This function will be removed in the future.
    public static Vector3 move(Vector3 v)
    {
        Vector3 localPosition = v - originBinA;
        Vector3 rotatedPosition = MatMul(transformationMatrix, localPosition);
        return new Vector3(rotatedPosition.x, rotatedPosition.y, rotatedPosition.z);
    }

    //Simulate latency for Non-Master Client
    private IEnumerator SimulateLatencyForNMC()
    {
        while (!spawnedSpheres) //Wait for spheres to be spawned
        {
            yield return null;

        }
        while (true) //Loop forever
        {
            yield return new WaitForSeconds(0.2f); //Wait for 200ms
            int rand = UnityEngine.Random.Range(0, 101); //Roll 100 sided dice
            if (rand > 50) //If random number larger than 50, then induce latency
            {
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingLag = CLIENT_LATENCY;
                Debug.Log("Adding latency!!");
            }
            else //Random number less than or equal to 50 so no latency
            {
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingLag = 0;

            }
        }
    }
    void Update()
    {

       if (PhotonNetwork.IsMasterClient && (!regOrigin || !regX || !regY || !regZ)) //All points must be registered
        {
            registerPoints(); //There are still points to be registrered so call this function
            //autoRegisterPoints();

        }
        
       //*NOTE* This will be removed in the future
        if (!hasRepositioned && !PhotonNetwork.IsMasterClient && receivedData) 
        {
            origin.transform.position = move(origin.transform.position);
            yUnitVector.transform.position = move(yUnitVector.transform.position);
            xUnitVector.transform.position = move(xUnitVector.transform.position);
            zUnitVector.transform.position = move(zUnitVector.transform.position);
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

