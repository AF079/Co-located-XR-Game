using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGenerator : MonoBehaviour
{
    public static int numberOfSpheres = 9;
    [HideInInspector]
    public List<GameObject> sphereList;
    public GameObject spherePrefab;

    private Rigidbody rb;
    float fixedDeltaTime;
    float force = 5f;
    float syncTime = 15;

    bool doneSync = false;

    public static bool SCENE_LOADED = false;
    public static bool GENERATED = false;


    public void onSceneLoaded()
    {
        SCENE_LOADED = true;
    }

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

    // Start is called before the first frame update
    void Start()
    {
        fixedDeltaTime = Time.fixedDeltaTime;
        Time.timeScale = 0.09f; //Slow down time
        //Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
        sphereList = new List<GameObject>();
        StartCoroutine(generateSpheres());
        StartCoroutine(waitForSync());
        StartCoroutine(initSpheres());

    }

    // Update is called once per frame
    void Update()
    {

    }
    private IEnumerator generateSpheres()
    {
        //Iterate over the number of spheres to spawn
        for (var i = 0; i < numberOfSpheres; i++)
        {
            //Random offset vector to add to the position of the spheres, x= [-1,1], y = [1,2], z = [-1,1]
            Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(1f, 2f), UnityEngine.Random.Range(-1f, 1f));
            //Spawn the sphere at offset vector
            GameObject sphere = Instantiate(spherePrefab, randomPos, Quaternion.identity);

            //Assign color
            sphere.GetComponent<MeshRenderer>().material.color = allColors[i];
            // sphere.GetComponent<NetworkedSphere>().color = allColors[i];
            sphere.GetComponent<Renderer>().enabled = false;

            Debug.Log("SPAWNED SPHERE " + i + " AT " + sphere.transform.position);
            //Add sphere to list
            sphereList.Add(sphere);
        }

        yield return null;

    }

    private IEnumerator waitForSync()
    {
        yield return new WaitForSecondsRealtime(syncTime);
        doneSync = true;
    }

    private IEnumerator initSpheres()
    {
        Debug.Log("Waiting for sync...");
        while (!doneSync) yield return null;
        foreach (var sphere in sphereList)
        {
            sphere.GetComponent<Renderer>().enabled = true;
            rb = sphere.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 startForce = new Vector3(UnityEngine.Random.Range(-force, force), 0, UnityEngine.Random.Range(-force, force)); //Random direction
            rb.AddForce(startForce, ForceMode.Impulse); //Apply random force in x and z direction
        }
        Debug.Log("Done initializing spheres");
        GENERATED = true;
    }
}
