using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentManager : MonoBehaviour
{
    private Transform _cameraRigTransform;

    public void Awake() 
    {
        _cameraRigTransform = FindAnyObjectByType<OVRCameraRig>().transform;
    }

    public void AlignUserToAnchor(OVRSpatialAnchor anchor) 
    {
        if (!anchor || !anchor.Localized)
        {
            Debug.LogError("Colocation: Invalid or unlocalized anchor. Cannot align.");
            return;
        }
        
        Debug.Log($"Colocation: Aligning user to anchor {anchor.Uuid}.");

        StartCoroutine(AlignmentCoroutine(anchor));
    }

    private IEnumerator AlignmentCoroutine(OVRSpatialAnchor anchor) 
    {
        var anchorTransform = anchor.transform;

        for (var alignmentCount = 2; alignmentCount > 0; alignmentCount--) 
        {
            _cameraRigTransform.position = Vector3.zero;
            _cameraRigTransform.eulerAngles = Vector3.zero;

            yield return null;

            _cameraRigTransform.position = anchorTransform.InverseTransformPoint(Vector3.zero);
            _cameraRigTransform.eulerAngles = new Vector3(0, -anchorTransform.eulerAngles.y, 0);
            Debug.Log($"Colocation: Aligned camera rig position: {_cameraRigTransform.position}");

            yield return new WaitForEndOfFrame();

        }
    }
}

