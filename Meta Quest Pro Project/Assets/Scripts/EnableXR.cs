using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;

namespace Dorkbots.XR
{
    /// <summary>
    /// This fixes issues with XR tools (Steam VR, Oculus Meta XR Simulator, ect.) not launching when in play mode. Attach to a game object in a scene using XR.
    ///
    /// Unity 2022.3 appears to not always dispose of the loader in XRGeneralSettings.Instance.Manager. This results in the console runtime error "Failed to set DeveloperMode on Start."
    /// This should only be an issue in the editor, so this script stops the XR loader and then restarts it.
    /// This script doesn't always activate the loader, maybe due to order of Awake calls, but it works most of time. Exiting Play mode and starting it again usually results in the XR loader launching...
    ///
    /// This script is based on the solution discussed here -> https://www.anton.website/enable-unity-xr-in-runtime/
    /// </summary>
    public class EnableXR : MonoBehaviour
    {
#if UNITY_EDITOR
        private void Awake()
        {
            StartCoroutine(EnableXRCoroutine());
        }

        private void OnDestroy()
        {
            DisableXR();
        }

        private IEnumerator EnableXRCoroutine()
        {
            while (XRGeneralSettings.Instance == null)
            {
                yield return null;
            }

            // Make sure the XR is disabled and properly disposed. It can happen that there is an activeLoader left
            // from the previous run.
            if (XRGeneralSettings.Instance.Manager.activeLoader || XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                DisableXR();
                // Wait for the next frame, just in case
                yield return null;
            }

            // Make sure we don't have an active loader already
            if (!XRGeneralSettings.Instance.Manager.activeLoader)
            {
                yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
            }

            // Make sure we have an active loader, and the manager is initialized
            if (XRGeneralSettings.Instance.Manager.activeLoader && XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            }
        }

        /// <summary>
        /// Disables XR
        /// </summary>
        private void DisableXR()
        {
            // Make sure there is something to de-initialize
            if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                XRGeneralSettings.Instance.Manager.StopSubsystems();
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
        }
#endif
    }
}



/*using System.Collections;
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
}*/