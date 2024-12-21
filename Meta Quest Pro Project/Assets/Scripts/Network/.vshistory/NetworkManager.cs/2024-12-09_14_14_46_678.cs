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


    // Start is called before the first frame update
    public GameObject canvas1;
    public GameObject canvas2; 
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

    private int numberOfBalls = 9;
    private int numberOfBallsLeft = 9;

    private int maxRounds = 2;
    private int currentRound = 1;
    private string tmpValue;
    private PhotonView photonView_;
    private PhotonPeer photonPeer_;
    public static Vector3 originBinA;
    public static Vector3 upBinA;
    public static Vector3 sideBinA;
    public static Vector3 forwardBinA;
    private bool regP1 = false;
    private bool regP2 = false;
    private bool regP3 = false;
    private bool regP4 = false;
    private bool receivedData = false;
    private bool spawnedBalls = false;
/*
    public static Vector3 r_originBinA;
    public static Vector3 r_upBinA;
    public static Vector3 r_sideBinA;
    public static Vector3 r_forwardBinA;*/

    private bool hasRepositioned = false;


    public static float[,] transformationMatrix;


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
        if (PhotonNetwork.IsMasterClient)
        {
            originBinA = Vector3.zero;
            upBinA = Vector3.zero;
        }
        photonView_ = GetComponent<PhotonView>();
        photonPeer_ = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
        sphereList = new List<GameObject>();

        StartCoroutine(CheckAndLogPlayerInfo());
        StartCoroutine(StartTimer());
        StartCoroutine(SimulateLatencyForNMC());
        //StartCoroutine(SimulateLatencyForMC());

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
    private void disableTUI()
    {

        origin.SetActive(false);
        side.SetActive(false);
        up.SetActive(false);
        forward.SetActive(false);
        canvas1.SetActive(false);
        canvas2.SetActive(false);
        
    }
    private IEnumerator spawnBalls()
    {
        disableTUI();
        for (var i = 0; i < numberOfBalls; i++)
        {

            Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(1f, 2f), UnityEngine.Random.Range(-1f, 1f));

            GameObject sphere = PhotonNetwork.Instantiate(pBall.name, Vector3.up + randomOffset, Quaternion.identity);

            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            sphere.GetComponent<NetworkedSphere>().color = allColors[i];

            Debug.Log("SPAWNED SPHERE " + i + " AT " + sphere.transform.position);

            sphereList.Add(sphere);

        }
        spawnedBalls = true;
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
/*        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
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
        }*/

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
        disableTUI();
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
        //Debug.Log("BEFORE " + v);
        Vector3 localPosition = v - originBinA;
        Vector3 rotatedPosition = MatMul(transformationMatrix, localPosition);
        //Debug.Log("AFTER " + rotatedPosition);
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
               PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.IncomingJitter = 5; // 20ms jitter
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingJitter = 5; // 20ms jitter
                photonPeer_.NetworkSimulationSettings.OutgoingLag = 10;
                photonPeer_.NetworkSimulationSettings.IncomingLag = 10;
                Debug.Log("Adding latency!!");
            }
            else
            {
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.IncomingJitter = 0; // 20ms jitter
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingJitter = 0; // 20ms jitter
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.IncomingLossPercentage =0; // 5% packet loss
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.NetworkSimulationSettings.OutgoingLossPercentage = 0; // 5% packet loss*/
                photonPeer_.NetworkSimulationSettings.IncomingLag = 0;
                photonPeer_.NetworkSimulationSettings.OutgoingLag = 0;

            }
        }
    }
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && (!regP1 || !regP2 || !regP3 || !regP4))
        {
            registerPoints();

        }

        if (!hasRepositioned && !PhotonNetwork.IsMasterClient && receivedData) //&& regP1 && regP2 
        {
            origin.transform.position = move(origin.transform.position);
            up.transform.position = move(up.transform.position);
            side.transform.position = move(side.transform.position);
            forward.transform.position = move(forward.transform.position);
            someCube.transform.position = move(someCube.transform.position);
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

