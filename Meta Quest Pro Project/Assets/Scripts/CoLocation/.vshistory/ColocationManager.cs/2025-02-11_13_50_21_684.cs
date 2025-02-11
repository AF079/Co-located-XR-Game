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
}
