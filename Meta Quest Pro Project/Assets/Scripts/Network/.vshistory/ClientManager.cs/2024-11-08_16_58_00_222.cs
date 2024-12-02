using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ClientManager : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI tmpText;
    private PhotonView photonView_;
    private List<GameObject> sphereList;
    private Vector3 r_originBinA;
    private Vector3 r_upBinA;
    private Vector3 r_sideBinA;
    private Vector3 r_forwardBinA;
    private bool receivedData = false;
    public GameObject origin;
    public GameObject up;
    public GameObject side;
    public GameObject forward;
    public GameObject someCube;

    private bool hasRepositioned = false;
    private bool deactivated = false;

    // Start is called before the first frame update
    void Awake()
    {
        
    }
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //Destroy(gameObject);
        }
        if (!PhotonNetwork.IsMasterClient)
        {
            Instantiate(origin);
            Instantiate(up);
            Instantiate(forward);
            Instantiate(side);
            Instantiate(someCube);
            photonView_ = GetComponent<PhotonView>();
            sphereList = new List<GameObject>();

        }
    }
    private Vector3 MatMul(float[,] m1, Vector3 v)
    {

        float x = m1[0, 0] * v[0] + m1[0, 1] * v[1] + m1[0, 2] * v[2];
        float y = m1[1, 0] * v[0] + m1[1, 1] * v[1] + m1[1, 2] * v[2];
        float z = m1[2, 0] * v[0] + m1[2, 1] * v[1] + m1[2, 2] * v[2];

        return new Vector3(x, y, z);
    }
    private Vector3 move(Transform t)

    {
        Debug.Log("OLD POS: " + t.position);
        Vector3 originB = r_originBinA;
        Vector3 upB = (r_upBinA - r_originBinA).normalized;
        Vector3 forwardB = (r_forwardBinA - r_originBinA).normalized;
        Vector3 rightB = (r_sideBinA - r_originBinA).normalized;

        float[,] transormationMatrix = { { rightB.x,   rightB.y,   rightB.z },
                                     { upB.x,      upB.y,      upB.z },
                                     { forwardB.x, forwardB.y, forwardB.z } };

        Vector3 localPosition = t.position - originB;
        Vector3 rotatedPosition = MatMul(transormationMatrix, localPosition);

        //t.position = new Vector3(rotatedPosition.x, rotatedPosition.y, rotatedPosition.z);
        Debug.Log("NEW POS: " + new Vector3(rotatedPosition.x, rotatedPosition.y, rotatedPosition.z));
        return new Vector3(rotatedPosition.x, rotatedPosition.y, rotatedPosition.z);
    }

    [PunRPC]
    public void receiveTransformData(float ox, float oy, float oz,
                                float ux, float uy, float uz,
                                float sx, float sy, float sz,
                                float fx, float fy, float fz)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            tmpText.text = "GOT TRANSFORM DATA";
            Debug.Log("GOT TRANSFORM DATA");
            r_originBinA = new Vector3(ox, oy, oz);
            r_upBinA = new Vector3(ux, uy, uz);
            r_sideBinA = new Vector3(sx, sy, sz);
            r_forwardBinA = new Vector3(fx, fy, fz);
            receivedData = true;

        }
    }


    private IEnumerator moveSpheres()
    {
        hasRepositioned = true;
        yield return new WaitForSeconds(10);

        Debug.Log("CHANGING POSITIONS");
        tmpText.text = "CHANGING POSITIONS";
        origin.transform.position = move(origin.transform);
        up.transform.position = move(up.transform);
        side.transform.position = move(side.transform);
        forward.transform.position = move(forward.transform);
        someCube.transform.position = move(someCube.transform);
        foreach (GameObject sphere in sphereList)
        {
            sphere.SetActive(true);
            Vector3 newPos = move(sphere.transform);
            sphere.GetComponent<NetworkedSphere>().move(newPos);

        }
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) {
            if (!deactivated)
            {
                GameObject[] spheres = GameObject.FindGameObjectsWithTag("tBall");
                if (spheres.Length == NetworkManager.numberOfBalls)
                {
                    foreach (GameObject sphere in spheres)
                    {
                        sphere.SetActive(false);
                        sphereList.Add(sphere);
                    }
                    deactivated = true;
                }
            }
            if (!hasRepositioned && receivedData && deactivated) //&& regP1 && regP2 
            {
                //StartCoroutine(moveSpheres());
                Debug.Log("CHANGING POSITIONS");
                tmpText.text = "CHANGING POSITIONS";
                origin.transform.position = move(origin.transform);
                up.transform.position = move(up.transform);
                side.transform.position = move(side.transform);
                forward.transform.position = move(forward.transform);
                someCube.transform.position = move(someCube.transform);
                foreach (GameObject sphere in sphereList)
                {
                    sphere.SetActive(true);
                    Vector3 newPos = move(sphere.transform);
                    sphere.GetComponent<NetworkedSphere>().move(newPos);

                }

            }
        
        }
    }
}
