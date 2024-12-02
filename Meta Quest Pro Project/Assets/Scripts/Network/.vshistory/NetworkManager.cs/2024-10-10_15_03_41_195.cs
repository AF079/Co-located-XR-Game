using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public PhotonView ballPrefab;
    private List<GameObject> sphereList;


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
    void Start()
    {
        /*  if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
          {
              PhotonNetwork.Instantiate(ballPrefab.name, ballPrefab.transform.position, ballPrefab.transform.rotation);
          }*/
        sphereList = new List<GameObject>();

        StartCoroutine(StartTimer());
    }
    private void spawnBalls()
    {
        for (var i = 0; i < numberOfBalls; i++)
        {
            Vector3 randomPosition = new Vector3(
               Random.Range(0f, 1f),
               Random.Range(0f, 1f),
               Random.Range(0f, 1f)
           );

            GameObject sphere = PhotonNetwork.Instantiate(ballPrefab.name, randomPosition, Quaternion.identity);
            sphereList.Add(sphere);
            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            sphere.GetComponent<NetworkedSphere>().color = allColors[i];
            Debug.Log(sphereList.Count);
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
