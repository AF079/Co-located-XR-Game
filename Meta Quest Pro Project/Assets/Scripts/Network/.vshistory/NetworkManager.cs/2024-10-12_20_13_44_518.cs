using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Meta.XR.ImmersiveDebugger.Manager;
using System;
using UnityEngine.Video;
using Photon.Realtime;

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
     */
    // Start is called before the first frame update
    public PhotonView pBall;
    public OVRSpatialAnchor pAnchor;

    private List<OVRSpatialAnchor> spatialAnchors = new List<OVRSpatialAnchor>();
    private List<GameObject> sphereList;
    private OVRSpaceUser[] users; 
   
    private int numberOfBalls = 9;
    private int numberOfBallsLeft = 9;

    private int maxRounds = 2;
    private int currentRound = 1;

    
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
    private void Awake()
    {
        //onLoadAnchor = OnLocalized;
    }
    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            Player[] players = PhotonNetwork.PlayerList;

            foreach (Player player in players)
            {
                Debug.Log("Player Name: " + player.NickName);
            }
        }

        sphereList = new List<GameObject>();

        StartCoroutine(StartTimer());
        
    }
    private void spawnBalls()
    {
        for (var i = 0; i < numberOfBalls; i++)
        {
            Vector3 randomPosition = new Vector3(
               UnityEngine.Random.Range(0f, 1f),
               UnityEngine.Random.Range(0f, 1f),
               UnityEngine.Random.Range(0f, 1f)
           );

            OVRSpatialAnchor anchor = Instantiate(pAnchor,randomPosition,Quaternion.identity);
            StartCoroutine(AnchorCreated(anchor));
            


            GameObject sphere = PhotonNetwork.Instantiate(pBall.name, 
                anchor.transform.position, Quaternion.identity);
            sphereList.Add(sphere);
            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            sphere.GetComponent<NetworkedSphere>().color = allColors[i];
            Debug.Log(sphereList.Count);
        }
    }

    public async void ShareAnchor(OVRSpatialAnchor anchor, OVRSpaceUser[] users)
    {
        var result = await anchor.ShareAsync(users);
        if (result.IsSuccess())
        {
            //must broadcast uuid to users (anchor.uuid)
            return;
        }
        else
        {
            Debug.Log("ERROR IN SHAREING: " + result.ToString());
        }
    }
    private IEnumerator AnchorCreated(OVRSpatialAnchor anchor)
    {
        while(!anchor.Created && !anchor.Localized)
        {
            yield return new WaitForEndOfFrame();
        }
        spatialAnchors.Add(anchor);
    }

    public async void SaveAnchor(OVRSpatialAnchor anchor)
    {
        var result = await anchor.SaveAnchorAsync();
        if (result.Success)
        {
            Debug.Log($"Anchor {anchor.Uuid} saved successfully.");
        }
        else
        {
            Debug.LogError($"Anchor {anchor.Uuid} failed to save with error {result.Status}");
        }
    }

    private IEnumerator StartTimer()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            yield return new WaitForSeconds(5);

            Debug.Log("5 seconds passed!");
            spawnBalls();
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (currentRound < maxRounds && sphereList.Count == numberOfBalls)
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
        }
    }
}
