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
    public TextMeshProUGUI p1Txt;
    public TextMeshProUGUI p2Txt;
    public TextMeshProUGUI p3Txt;
    public TextMeshProUGUI p4Txt;
    public TextMeshProUGUI tmpText;

    private List<GameObject> sphereList;

    public static int numberOfBalls = 4;
    public static int numberOfBallsLeft = 4;

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

    private int clientActorNumber;
    private bool shouldSetActive = false;
    private bool hasSetActive = false;

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
            photonView_ = GetComponent<PhotonView>();

            sphereList = new List<GameObject>();
        }

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
        StartCoroutine(StartPlayerAlign());
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

                Debug.Log(players[i].NickName + " " + players[i].UserId);
                tmpValue += "NON MASTER CLIENT NAME: " + players[i].NickName;
                clientActorNumber = players[i].ActorNumber;
                break;
                /* if (ulong.TryParse(players[i].NickName, out ulong userId))
                 {

                 }
                 else
                 {
                     Debug.LogError("Failed to parse NickName as a user ID: " + players[i].NickName);
                 }*/
            }
        }

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

    private IEnumerator SpawnBalls()
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

    private IEnumerator StartPlayerAlign()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) yield break;

        while (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == 2 && regP1 && regP2 && regP3 && regP4))
        {
            Debug.Log("Waiting for second player to join!");
            yield return new WaitForSeconds(1f);
        }
        PhotonView clientPhotonView = PhotonView.Find(clientActorNumber);
        if (clientPhotonView != null)
        {
            clientPhotonView.RPC("receiveTransformData", RpcTarget.Others, 
                originBinA.x, originBinA.y, originBinA.z,
                upBinA.x, upBinA.y, upBinA.z,
                sideBinA.x, sideBinA.y, sideBinA.z,
                forwardBinA.x, forwardBinA.y, forwardBinA.z);
        }
  
        yield return new WaitForSeconds(5);

        yield return StartCoroutine(SpawnBalls());
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

    [PunRPC]
    private void receiveSetActiveSignal()
    {
        shouldSetActive = true;
    }
    
    void Update()
    {
        if (PhotonNetwork.IsMasterClient && (!regP1 || !regP2 || !regP3 || !regP4))
        {
            registerPoints();

        }
        if(!hasSetActive && shouldSetActive)
        {
            foreach(GameObject sphere in sphereList)
            {
                sphere.SetActive(true);
            }
            hasSetActive = true;
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