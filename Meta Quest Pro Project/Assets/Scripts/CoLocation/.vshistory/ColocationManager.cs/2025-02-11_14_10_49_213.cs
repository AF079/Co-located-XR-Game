using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using System;
using System.Text;

public class NewBehaviourScript : NetworkBehaviour
{
    [SerializeField] private AlignmentManager alignmentManager;
    private Guid _sharedAnchorGroupId;

    public override void Spawned()
    {
        base.Spawned();
        PrepareColocation();
    }

    private void PrepareColocation()
    {
        if (Object.HasStateAuthority)
        {
            Debug.Log("Colocation: Starting advertisement");
            AdvertiseColocationSession();
        }
        else
        {
            Debug.Log("Colocation: Starting discovery");
            DiscoverNearbySession();
        }
    }
    private async void AdvertiseColocationSession()
    {
        try
        {
            var advertismentData = Encoding.UTF8.GetBytes("SharedSpatialAnchorSession");
            var startAdvertisementResult = await OVRColocationSession.StartAdvertisementAsync(advertismentData);

            if (startAdvertisementResult.Success)
            {
                _sharedAnchorGroupId = startAdvertisementResult.Value;
                Debug.Log($"Colocation: Advertisment started. UUID: {_sharedAnchorGroupId}");
                CreateAndShareAlignmentAnchor();
            }
            else
            {
                Debug.LogError($"Colocation: Advertisment failed with status: {startAdvertisementResult.Status}");
            }
        }catch(Exception e)
        {
            Debug.Log($"Colocation: Error during advertisment: {e.Message}");
        }
    }

    private async void DiscoverNearbySession()
    {
        try
        {
            OVRColocationSession.ColocationSessionDiscovered += OnColocationSessionDiscovered;
            var discoveryResult = await OVRColocationSession.StartDiscoveryAsync();
            if (!discoveryResult.Success)
            {
                Debug.LogError($"Colocation: Discovery failed with status {discoveryResult.Status}");
                return;
            }
            Debug.Log("Colocation: Discovery started successfully.");
        }catch(Exception e)
        {
            Debug.LogError($"Colocation: Error during discovery: {e.Message}");
        }
    }
    private async void CreateAndShareAlignmentAnchor()
    {
        try
        {
            Debug.Log("Colocation: Creating alignment anchor...");
            var anchor = await CreateAnchor(Vector3.zero
                , Quaternion.identity);
            if (anchor == null)
            {
                Debug.LogError("Colocation: Failed to create alignment anchor");
                return;
            }
            if (!anchor.Localized)
            {
                Debug.LogError("Colocation: Anchor is not localized. Cannot proceed with sharing");
                return;
            }
            var saveResult = await anchor.SaveAnchorAsync();
            if (!saveResult.Success)
            {
                Debug.LogError($"Colocation: Failed to save alignment anchor. Error: {saveResult}");
                return;
            }
            Debug.Log($"Colocation: Alignment anchor saved successfully. UUIDP {anchor.Uuid}");
            var shareResult = await OVRSpatialAnchor.ShareAsync(new List<OVRSpatialAnchor
                > { anchor}, _sharedAnchorGroupId);
            if (!shareResult.Success)
            {
                Debug.Log($"Colocation: Failed to share aligment anchor. Error {shareResult}");
                return;
            }
            Debug.Log($"Colocation: Alignment anchor shared sucessfully. Group UUID: {_sharedAnchorGroupId}");


        }catch(Exception e)
        {
            Debug.LogError($"Colocation: Error during creation and shareing: {e.Message}");
        }
    }
}

