using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerData : MonoBehaviour
{
    GameObject leftController;
    GameObject rightController;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator PrintMessageEveryFiveSeconds()
    {
        while (true) 
        {
            
            Debug.Log("LEFT POS: " + leftController.transform.position + " RIGHT POS: " + rightController.transform.position);
            
            yield return new WaitForSeconds(5);
        }
    }
}
