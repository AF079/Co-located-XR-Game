using System.Collections;
using UnityEngine.XR.Management;

public static class EnableXR
{
    public static IEnumerator EnableXRCoroutine()
    {
        // Make sure the XR is disabled and properly disposed. It can happen that there is an activeLoader left
        // from the previous run.
        if (XRGeneralSettings.Instance.Manager.activeLoader || XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            DisableXR();
            yield return null;
        }

        // Enable XR
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (!XRGeneralSettings.Instance.Manager.activeLoader || !XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            // Something went wrong, XR is not enabled
            yield break;
        }

        XRGeneralSettings.Instance.Manager.StartSubsystems();
        yield return null;

        // Not that OVRBody and OVRFaceExpressions components will not enable themselves automatically.
        // You will have to do that manually
        OVRPlugin.StartBodyTracking();
        OVRPlugin.StartFaceTracking();
    }

    public static void DisableXR()
    {
        if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            OVRPlugin.StopBodyTracking();
            OVRPlugin.StopFaceTracking();

            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }
}