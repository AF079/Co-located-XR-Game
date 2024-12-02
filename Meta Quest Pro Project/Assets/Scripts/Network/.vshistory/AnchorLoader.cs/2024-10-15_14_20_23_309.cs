using Oculus.Platform.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorLoader : MonoBehaviour
{
    Action<OVRSpatialAnchor.UnboundAnchor, bool> _onLoadAnchor;

    private void Awake()
    {
        _onLoadAnchor = OnLocalized;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LoadAnchorByUuid_test(List<Guid> guidList, GameObject pAnchor)
    {
        Load(new OVRSpatialAnchor.LoadOptions
        {
            Timeout = 0,
            StorageLocation =
            OVRSpace.StorageLocation.Local,
            Uuids = guidList
        },pAnchor
            );
    }
    private void Load(OVRSpatialAnchor.LoadOptions options, GameObject pAnchor)
    {
        OVRSpatialAnchor.LoadUnboundAnchors(options, anchors =>
        {
            if (anchors == null) return;
            foreach (var anchor in anchors)
            {
                if (anchor.Localized)
                {
                    _onLoadAnchor(anchor, true);
                }
                else if (!anchor.Localizing)
                {
                    anchor.Localize(_onLoadAnchor);
                }
            }
        });
    }
    private void OnLocalized(OVRSpatialAnchor.UnboundAnchor unboundAnchor, bool success,, GameObject pAnchor)
    {
        if (!success) return;

        var pose = unboundAnchor.Pose;
        var spatialAnchor = Instantiate(pAnchor, pose.position, pose.rotation);
        unboundAnchor.BindTo(spatialAnchor);

    }
}
