using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Meta.XR.ImmersiveDebugger.Manager;
using System;
using UnityEngine.Video;
using Photon.Realtime;
using System.Threading.Tasks;

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
     * NOTE: consider multiple attempts to perform sharing/loading, not just a single one.
     */
    // Start is called before the first frame update
    public PhotonView pBall;
    public OVRSpatialAnchor pAnchor;

    private List<OVRSpatialAnchor> spatialAnchors = new List<OVRSpatialAnchor>();
    private List<OVRSpatialAnchor> recievedAnchors = new List<OVRSpatialAnchor> ();
    private List<GameObject> sphereList;
    
    private OVRSpaceUser otherUser; 
   
    private int numberOfBalls = 9;
    private int numberOfBallsLeft = 9;

    private int maxRounds = 2;
    private int currentRound = 1;
    private const string NumUuidsPlayerPref = "numUuids";
    private List<Guid> guidList = new List<Guid>();
    
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

        if (PhotonNetwork.InRoom && PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            Player[] players = PhotonNetwork.PlayerList;
            for(int i = 0; i < players.Length; i++) 
            {
                if (players[i].NickName == PhotonNetwork.LocalPlayer.NickName) continue;
                ulong userId = ulong.Parse(players[i].NickName);
                Debug.Log(players[i].NickName + " " + players[i].UserId);
                otherUser = new OVRSpaceUser(userId);
                break;
            }
        }

        sphereList = new List<GameObject>();

        StartCoroutine(StartTimer());
        
    }
    private async void spawnBalls()
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

            Debug.Log("Created Anchor with uuid: " + anchor.Uuid);
            
            GameObject sphere = PhotonNetwork.Instantiate(pBall.name,anchor.transform.position, Quaternion.identity);
            sphere.tag = "Sphere " + anchor.Uuid;
            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            sphere.GetComponent<NetworkedSphere>().color = allColors[i];
            //sphere.SetActive(false); //This should by global not local change
            photonView.RPC("DeactivateSphere", RpcTarget.All,sphere.tag);

            sphereList.Add(sphere);

            await SaveAnchor(anchor);
        }
        foreach(OVRSpatialAnchor anchor in spatialAnchors)
        {
            await ShareAnchor(anchor);
            
        }
    }
    [PunRPC]
    public void DeactivateSphere(string sphereTag)
    {
        GameObject sphere = GameObject.FindWithTag(sphereTag);
        if (sphere != null)
        {
            sphere.SetActive(false); 
        }
    }
    private IEnumerator AnchorCreated(OVRSpatialAnchor anchor)
    {
        while (!anchor.Created && !anchor.Localized)
        {
            yield return new WaitForEndOfFrame();
        }
        spatialAnchors.Add(anchor);
    }
 
    public async Task<int> SaveAnchor(OVRSpatialAnchor anchor)
    {
        var result = await anchor.SaveAnchorAsync();
        if (result.Success)
        {
            Debug.Log($"Anchor {anchor.Uuid} saved successfully.");
            SaveUuid(anchor.Uuid);
            return 0;
        }
        else
        {
            Debug.LogError($"Anchor {anchor.Uuid} failed to save with error {result.Status}");
            return 1;
        }
        
    }
    private void SaveUuid(Guid uuid)
    {
        if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
        }
        int curCount = PlayerPrefs.GetInt(NumUuidsPlayerPref);
        PlayerPrefs.SetString("uuid" + curCount, uuid.ToString());
        PlayerPrefs.SetInt(NumUuidsPlayerPref, ++curCount);
    }

    public async Task<int> LoadAnchorByUuid()
    {
        List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(guidList, _unboundAnchors);
        if (result.Success)
        {
            Debug.Log("Anchors Loaded!");
            foreach(var unboundAnchor in result.Value) {
                unboundAnchor.LocalizeAsync().ContinueWith((success,anchor)=> {
                    if (success)
                    {
                        var spatialAnchor = new GameObject($"Anchor {unboundAnchor.Uuid}").AddComponent<OVRSpatialAnchor>();
                        unboundAnchor.BindTo(spatialAnchor);
                        Instantiate(spatialAnchor, unboundAnchor.Pose.position, unboundAnchor.Pose.rotation); //.Pose.position is obsolete???
                        recievedAnchors.Add(spatialAnchor);

                    }
                    else
                    {
                        Debug.Log("Localization failed!");
                    }
                
                },unboundAnchor);
            }
            return 0;
        }
        return 1;
    }
    public async Task<int> ShareAnchor(OVRSpatialAnchor anchor)
    {
        var result = await anchor.ShareAsync(otherUser);
        if (result.IsSuccess())
        {
            photonView.RPC("BroadcastUuidToOtherUser", RpcTarget.Others, anchor.Uuid);
            return 0;
        }
        else
        {
            Debug.Log("ERROR IN SHAREING: " + result.ToString());
            return 1;
        }
    }

    [PunRPC]
    private void BroadcastUuidToOtherUser(Guid uuid)
    {
        guidList.Add(uuid);
    }

    private IEnumerator TryLoadAnchors()
    {
        Task task = LoadAnchorByUuid();
        yield return new WaitUntil(() => task.IsCompleted);
    }

    private IEnumerator StartTimer()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) yield break;

        while (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.PlayerList.Length == 2))
        {
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(5);

        Debug.Log("5 seconds passed, conditions met!");
        spawnBalls();  
    }
    [PunRPC]
    private void NotifyMasterClientIsReady()
    {
        foreach (GameObject sphere in sphereList)
        {
            sphere.SetActive(true);
        }
    }

    private IEnumerator RepositionSpheresToAnchors()
    {
        for(int i = 0; i < guidList.Count; i++)
        {
            Guid uuid = guidList[i];
            GameObject sphere = GameObject.FindGameObjectWithTag("Sphere "+uuid);
            OVRSpatialAnchor anchor = recievedAnchors[i];  
            sphere.SetActive(true);
            sphere.transform.position = anchor.transform.position;
        }
        photonView.RPC("NotifyMasterClientIsReady", RpcTarget.MasterClient);
        //maybe add delay here??
        yield break;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if(recievedAnchors.Count < numberOfBalls && guidList.Count == numberOfBalls)
            {
                StartCoroutine(TryLoadAnchors());
            }
            else
            {
                StartCoroutine(RepositionSpheresToAnchors());
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
