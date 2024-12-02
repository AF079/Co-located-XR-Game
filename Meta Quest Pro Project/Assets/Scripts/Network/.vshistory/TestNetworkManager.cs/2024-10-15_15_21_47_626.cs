using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Meta.XR.ImmersiveDebugger.Manager;
using System;
using UnityEngine.Video;
using Photon.Realtime;
using System.Threading.Tasks;
using TMPro;
using Meta.XR.ImmersiveDebugger;
using UnityEditor;

public class TestNetworkManager : MonoBehaviourPunCallbacks
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
    private List<OVRSpatialAnchor> recievedAnchors = new List<OVRSpatialAnchor>();
    private List<GameObject> sphereList;

    private OVRSpaceUser otherUser;

    private int numberOfBalls = 9;
    private int numberOfBallsLeft = 9;

    private int maxRounds = 2;
    private int currentRound = 1;
    private const string NumUuidsPlayerPref = "numUuids";
    private List<Guid> guidList = new List<Guid>();
    private List<Guid> guidList_test = new List<Guid>();
    public TextMeshProUGUI tmpText;
    private string tmpValue;

    private AnchorLoader anchorLoader;
    
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

    private void OnDestroy()
    {
        foreach(var anchor in spatialAnchors)
        {
            Destroy(anchor.gameObject);
        }
    }

    void Start()
    {
        anchorLoader =  GetComponent<AnchorLoader>();
        StartCoroutine(CheckAndLogPlayerInfo());
        sphereList = new List<GameObject>();

        StartCoroutine(StartTimer());
    }


    private IEnumerator CheckAndLogPlayerInfo()
    {
        while (!PhotonNetwork.InRoom || !PhotonNetwork.IsConnected)
        {
            yield return null; 
        }
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
            yield return new WaitUntil(() => anchor.Created && anchor.Localized);
          
            Task task = SaveAnchor(anchor);
            yield return new WaitUntil(() => task.IsCompleted);

            GameObject sphere = PhotonNetwork.Instantiate(pBall.name, anchor.transform.position, Quaternion.identity);
            //sphere.name = "Sphere" + i;
            Debug.Log("Saved Anchor with uuid: " + anchor.Uuid);
            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            sphere.GetComponent<NetworkedSphere>().color = allColors[i];

            
            sphereList.Add(sphere);
            guidList.Add(anchor.Uuid);
            photonView.RPC("DeactivateSphere", RpcTarget.All, sphere.name);
            BroadcastUuidToOtherUser(anchor.Uuid);
        }

        Task task2 = LoadAnchorByUuid();
        yield return new WaitUntil(() => task2.IsCompleted);
        Debug.Log("Received/Loaded Anchors: " + recievedAnchors.Count + " " + recievedAnchors);
        Debug.Log("Guids: " + guidList.Count + " " + guidList);
    }

    [PunRPC]
    public void DeactivateSphere(string sphereName) //sphereName = Ball 1
    {
        GameObject sphere = GameObject.Find(sphereName);
        if (sphere != null)
        {
            sphere.SetActive(false);
            Debug.Log("Setting active=false!");
        }
        else
        {
            Debug.Log("Cant deactivate sphere");
        }
        if (!PhotonNetwork.IsMasterClient)
        {
            sphereList.Add(sphere);
        }
    }
    [PunRPC]
    private void BroadcastUuidToOtherUser(Guid uuid)
    {
        Debug.Log("broadcasting!");
        guidList_test.Add(uuid);
    }

    public async Task<int> SaveAnchor(OVRSpatialAnchor anchor)
    {
        var result = await anchor.SaveAnchorAsync();
        if (result.Success)
        {
            Debug.Log($"Anchor {anchor.Uuid} saved successfully.");
            spatialAnchors.Add(anchor);
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
        foreach (Guid guid in guidList)
        {
            Debug.Log("GUID: " + guid.ToString());
        }
        List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();
        var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(guidList, _unboundAnchors);
        if (result.Success)
        {
            Debug.Log("Anchors Loaded! " + _unboundAnchors.Count + " " + guidList.Count);
            foreach (var unboundAnchor in result.Value)
            {
                unboundAnchor.LocalizeAsync().ContinueWith((success, anchor) =>
                {
                    if (success)
                    {
                        var spatialAnchor = new GameObject($"Anchor {unboundAnchor.Uuid}").AddComponent<OVRSpatialAnchor>();
                        Debug.Log("trying to bind unbound anchor!");
                        unboundAnchor.BindTo(spatialAnchor);
                        Debug.Log("done binding!");
                        spatialAnchor.transform.position = unboundAnchor.Pose.position;
                        recievedAnchors.Add(spatialAnchor);
                        Debug.Log("Adding to received anchors! " + recievedAnchors.Count);
                    }
                    else
                    {
                        Debug.Log("Localization failed!");
                    }

                }, unboundAnchor);
            }
            return 0;
        }
        return 1;
    }
    private IEnumerator TryLoadAnchors()
    {
        Task task = LoadAnchorByUuid();
        yield return new WaitUntil(() => task.IsCompleted);
    }

    private IEnumerator StartTimer()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient) yield break;

        while (!(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient))
        {
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
    


    [DebugMember(Category = "DebugTest")]
    private void test()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log(guidList + " guid list size: " + guidList.Count + " rec anchors: " + recievedAnchors.Count);

        }
        StartCoroutine(debug());
    }
    // Update is called once per frame
    private bool loaded = false;
    void Update()
    {
        test();
        /*if(guidList.Count == numberOfBalls && !loaded)
        {
            anchorLoader.LoadAnchorByUuid_test(guidList);
            loaded = true;
        }*/

    }
}
