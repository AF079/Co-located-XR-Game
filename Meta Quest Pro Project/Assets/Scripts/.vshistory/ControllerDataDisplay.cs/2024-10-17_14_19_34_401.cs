using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerDataDisplay : MonoBehaviour
{
    public GameObject leftController;
    public GameObject rightController;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DisplayPostion());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator DisplayPostion()
    {
        while (true)
        {

            Debug.Log("LEFT POS: " + leftController.transform.position + " RIGHT POS: " + rightController.transform.position);

            yield return new WaitForSeconds(5);
        }
    }
}
