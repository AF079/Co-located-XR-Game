using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using System;

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
}
