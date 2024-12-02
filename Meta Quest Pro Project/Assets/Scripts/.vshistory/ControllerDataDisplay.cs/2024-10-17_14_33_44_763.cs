using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerDataDisplay : MonoBehaviour
{
 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(DisplayPostion());
    }

    private IEnumerator DisplayPostion()
    {
        //Debug.Log("LEFT POS: " + leftController.transform.position + " RIGHT POS: " + rightController.transform.position);
        Debug.Log("HEAD POS: " + Camera.main.transform.position);
        yield return new WaitForSeconds(5);
    }
}
