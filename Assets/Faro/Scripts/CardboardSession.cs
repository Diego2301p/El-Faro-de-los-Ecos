using Google.XR.Cardboard;
using UnityEngine;
using UnityEngine.SpatialTracking;
namespace Faro
{
    // Ciclo de Cardboard basado en CardboardStartup, Apache-2.0 (Google).
    public class CardboardSession : MonoBehaviour
    {
        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if UNITY_ANDROID && !UNITY_EDITOR
            var pose = gameObject.AddComponent<TrackedPoseDriver>();
            pose.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice,
                TrackedPoseDriver.TrackedPose.Center);
            pose.trackingType = TrackedPoseDriver.TrackingType.RotationOnly;
            pose.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
#endif
        }
        private void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Api.HasDeviceParams()) Api.ScanDeviceParams();
#endif
        }
        private void Update()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Api.IsGearButtonPressed) Api.ScanDeviceParams();
            if (Api.IsCloseButtonPressed) Application.Quit();
            if (Api.IsTriggerHeldPressed) Api.Recenter();
            if (Api.HasNewDeviceParams()) Api.ReloadDeviceParams();
            Api.UpdateScreenParams();
#endif
        }
    }
}
