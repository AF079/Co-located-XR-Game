using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon.Realtime;
using System.Threading.Tasks;
using TMPro;

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
    public TextMeshProUGUI tmpText;
    private string tmpValue;
    private PhotonView photonView_;

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
        photonView_ = GetComponent<PhotonView>();
        StartCoroutine(CheckAndLogPlayerInfo());
        sphereList = new List<GameObject>();

        StartCoroutine(StartTimer());
    }

    private IEnumerator EraseCoroutine(OVRSpatialAnchor anchor)
    {
        Task task = EraseAnchor(anchor);
        yield return new WaitUntil(() => task.IsCompleted);
    }
    private async Task<int> EraseAnchor(OVRSpatialAnchor anchor)
    {
        var result = await anchor.EraseAnchorAsync();
        if (result.Success)
        {
            Debug.Log("ANCHOR ERASED!");
            return 0;
        }
        Debug.Log("FAILED TO ERASE ANCHOR");
        return 1;
    }
    private void OnDestroy()
    {
        foreach(OVRSpatialAnchor anchor in spatialAnchors)
        {
            //StartCoroutine(EraseCoroutine(anchor));
            anchor.Erase();
        }
    }

    private IEnumerator CheckAndLogPlayerInfo()
    {
        // Wait until we're in a room and all players are properly connected
        while (!PhotonNetwork.InRoom || !PhotonNetwork.IsConnected || PhotonNetwork.PlayerList.Length < 2)
        {
            yield return null; // Wait for the next frame
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
                if (ulong.TryParse(players[i].NickName, out ulong userId))
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
               1.5f,
               UnityEngine.Random.Range(0f, 1f)
           );
            var go = new GameObject();
            var anchor = go.AddComponent<OVRSpatialAnchor>();
            anchor.transform.position = randomPosition;
            //OVRSpatialAnchor anchor = Instantiate(pAnchor,randomPosition,Quaternion.identity);
            yield return new WaitUntil(() => anchor.Created && anchor.Localized);
            //StartCoroutine(AnchorCreated(anchor));
            /*StartCoroutine(AnchorSaved(anchor));*/
            Task task = SaveAnchor(anchor);
            yield return new WaitUntil(() => task.IsCompleted);

            GameObject sphere = PhotonNetwork.Instantiate(pBall.name,anchor.transform.position, Quaternion.identity);
            //sphere.name = "Sphere" + i;
            Debug.Log("SAVED ANCHOR WITH UUID: " + anchor.Uuid);
            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            sphere.GetComponent<NetworkedSphere>().color = allColors[i];

            sphere.SetActive(false);
            spatialAnchors.Add(anchor);
            sphereList.Add(sphere);
            photonView_.RPC("DeactivateSphere", RpcTarget.Others,sphere.name);

        }
        Debug.Log("SHARING ANCHORS");
        foreach (OVRSpatialAnchor anchor in spatialAnchors)
        {
            Task task = ShareAnchor(anchor);
            yield return new WaitUntil(() => task.IsCompleted);
            Debug.Log("BROADCASTING UUID: " + anchor.Uuid);
            photonView_.RPC("BroadcastUuidToOtherUser", RpcTarget.Others, anchor.Uuid);

        }

    }

   /* private IEnumerator AnchorsShared(List<OVRSpatialAnchor> anchors)
    {

        
    }*/
  /*  private IEnumerator AnchorSaved(OVRSpatialAnchor anchor)
    {
        guidList_test
    }*/
    [PunRPC]
    public void DeactivateSphere(string sphereName) //sphereName = Ball 1
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " IS DEACTIVATING SPHERE!");
        GameObject sphere = GameObject.Find(sphereName);
        if (sphere != null)
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " DEACTIVATE OK");
            sphere.SetActive(false);
        }
        else
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " DEACTIVATE FAILED");
        }
        if(!PhotonNetwork.IsMasterClient)
        {
            sphereList.Add(sphere);
        }
    }
 /*   private IEnumerator AnchorCreated(OVRSpatialAnchor anchor)
    {
        while (!anchor.Created && !anchor.Localized)
        {
            yield return new WaitForEndOfFrame();
        }
        spatialAnchors.Add(anchor);
        Debug.Log("Created Anchor with uuid: " + anchor.Uuid);

    }*/

    public async Task<int> SaveAnchor(OVRSpatialAnchor anchor)
    {
        var result = await anchor.SaveAnchorAsync();
        if (result.Success)
        {
            Debug.Log($"ANCHOR {anchor.Uuid} SAVED SUCCESSFULLY.");
            //SaveUuid(anchor.Uuid);
            return 0;
        }
        else
        {
            Debug.LogError($"ANCHOR {anchor.Uuid} FAILED TO SAVE WITH ERROR {result.Status}");
            return 1;
        }
    }
/*    private void SaveUuid(Guid uuid)
    {
        if (!PlayerPrefs.HasKey(NumUuidsPlayerPref))
        {
            PlayerPrefs.SetInt(NumUuidsPlayerPref, 0);
        }
        int curCount = PlayerPrefs.GetInt(NumUuidsPlayerPref);
        PlayerPrefs.SetString("uuid" + curCount, uuid.ToString());
        PlayerPrefs.SetInt(NumUuidsPlayerPref, ++curCount);
    }*/

    public async Task<int> LoadAnchorByUuid()
    {
        List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(guidList, _unboundAnchors);
        if (result.Success)
        {
            Debug.Log("ANCHORS LOADED: " + _unboundAnchors.Count);
            foreach(var unboundAnchor in result.Value) {
                unboundAnchor.LocalizeAsync().ContinueWith((success,anchor)=> {
                    if (success)
                    {
                        var spatialAnchor = new GameObject($"Anchor {unboundAnchor.Uuid}").AddComponent<OVRSpatialAnchor>();
                        unboundAnchor.BindTo(spatialAnchor);
                        spatialAnchor.transform.position = unboundAnchor.Pose.position;
                        //Instantiate(spatialAnchor, unboundAnchor.Pose.position, unboundAnchor.Pose.rotation); //.Pose.position is obsolete???
                        recievedAnchors.Add(spatialAnchor);

                    }
                    else
                    {
                        Debug.Log("LOCALIZATION FAILED");
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
            
            return 0;
        }
        else
        {
            Debug.Log("ERROR IN SHAREING: " + result.ToString());
            return 1;
        }
    }

    [PunRPC]
    private void BroadcastUuidToOtherUser(string uuid_str)


        Guid uuid = Guid.Parse(uuid_str);
        Debug.Log("ADDING TO GUID LIST");
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
            Debug.Log("Waiting for second player to join!");
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(5);

        Debug.Log("5 seconds passed, conditions met!");
        yield return StartCoroutine(spawnBalls());  
    }
    private IEnumerator debug()
    {
        yield return new WaitForSeconds(2);
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
            OVRSpatialAnchor anchor = recievedAnchors[i];
            sphereList[i].transform.position = anchor.transform.position;
            sphereList[i].SetActive(true);
        }
        photonView_.RPC("NotifyMasterClientIsReady", RpcTarget.MasterClient);
        //maybe add delay here??
        yield break;
    }
    private void test()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log(guidList + " guid list size: " + guidList.Count + " rec anchors: " + recievedAnchors.Count);
            if(recievedAnchors.Count < numberOfBalls && guidList.Count == numberOfBalls)
        {
            StartCoroutine(TryLoadAnchors());
        }
        else
        {
            StartCoroutine(RepositionSpheresToAnchors());
        }
        }
        StartCoroutine(debug());
    }
    // Update is called once per frame
    void Update()
    {
        test();
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
