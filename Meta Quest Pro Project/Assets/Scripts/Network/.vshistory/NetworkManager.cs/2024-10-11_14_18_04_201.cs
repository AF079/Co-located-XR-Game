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
    // Start is called before the first frame update
    public PhotonView pBall;
    private List<GameObject> sphereList;
    private List<OVRSpatialAnchor> sphereAnchors;


    private int numberOfBalls = 9;
    private int numberOfBallsLeft = 9;

    private int maxRounds = 2;
    private int currentRound = 1;

    private const string NumUuidsPlayerPref = "numUuids";

    private OVRSpaceUser[] users;

    //Action<OVRSpatialAnchor.UnboundAnchor,bool> onLoadAnchor;

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
        /*  if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
          {
              PhotonNetwork.Instantiate(ballPrefab.name, ballPrefab.transform.position, ballPrefab.transform.rotation);
          }*/
        if (PhotonNetwork.InRoom)
        {
            Player[] players = PhotonNetwork.PlayerList;

            for (int i = 0; i < players.Length; i ++)
            {
                ulong metaId = ulong.Parse(players[i].NickName);
                OVRSpaceUser spaceUser = new OVRSpaceUser(metaId);
                users[i] = spaceUser;
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

            GameObject sphere = PhotonNetwork.Instantiate(pBall.name, randomPosition, Quaternion.identity);
            sphereList.Add(sphere);
            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            sphere.GetComponent<NetworkedSphere>().color = allColors[i];
            Debug.Log(sphereList.Count);

            OVRSpatialAnchor anchor = Instantiate(
                sphere.GetComponent<OVRSpatialAnchor>(), randomPosition, Quaternion.identity);
            StartCoroutine(AnchorCreated(anchor));

            SaveCreatedAnchor(anchor);
        }
    }
    private async void SaveCreatedAnchor(OVRSpatialAnchor anchor)
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
        SaveUuidToPlayerPrefs(anchor.Uuid);
    }

    private void SaveUuidToPlayerPrefs(Guid uuid)
    {
        if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
        }
        int playerNumuuids = PlayerPrefs.GetInt(NumUuidsPlayerPref);
        PlayerPrefs.SetString("uuid"+playerNumuuids,uuid.ToString());
        PlayerPrefs.SetInt(NumUuidsPlayerPref,++playerNumuuids);
    }
    private IEnumerator AnchorCreated(OVRSpatialAnchor anchor)
    {
        while(!anchor.Created && !anchor.Localized)
        {
            yield return new WaitForEndOfFrame();
        }
        Guid anchorGuid = anchor.Uuid;
        sphereAnchors.Add(anchor);
    }
    public async void ShareAnchor(OVRSpatialAnchor anchor, OVRSpaceUser[] users)
    {
        var result = await anchor.ShareAsync(users);
        if (result.IsSuccess())
        {
            //BroadcastUuidToUsers(anchor.Uuid);
            return;
        }
    }

/*    public void LoadAnchorsByUuid()
    {
        if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
        }
        var playerUuidCount = PlayerPrefs.GetInt(NumUuidsPlayerPref);
        if (playerUuidCount == 0)
        {
            return;
        }
        
        var uuids = new Guid[playerUuidCount];
        for(int i = 0; i < playerUuidCount; ++i)
        {
            var uuidKey = "uuid" + i;
            var currentUuid = PlayerPrefs.GetString(uuidKey);
            uuids[i] = new Guid(currentUuid);
        }

        Load(new OVRSpatialAnchor.LoadOptions
        {
            Timeout = 0,
            StorageLocation = OVRSpace.StorageLocation.Cloud,
            Uuids = uuids
        });

    }

    private void Load(OVRSpatialAnchor.LoadOptions options)
    {
        OVRSpatialAnchor.LoadUnboundAnchors(options, sphereAnchors => { 
            if(sphereAnchors == null)
            {
                return;
            }
            foreach(var anchor in sphereAnchors) {
                if (anchor.Localized)
                {
                    onLoadAnchor(anchor, true);
                }else if(!anchor.Localizing) {
                    anchor.Localize(onLoadAnchor);
                }
            
            }
        });
    }

    private void OnLocalized(OVRSpatialAnchor.UnboundAnchor unboundAnchor,bool success)
    {
        if (!success)
        {
            return;
        }
        var pose = unboundAnchor.Pose;
        var spatialAnchor = Instantiate([anchor],pose.position,pose.rotation);
        unboundAnchor.BindTo(spatialAnchor);

    }*/
 
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
