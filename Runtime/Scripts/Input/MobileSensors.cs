using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// This script uses the legacy Unity Input Module rather than the "New Input System" because the Unity Remote App
    /// (used for iOS and Android development) does not yet support the New Input System.  It seems that the New Input
    /// System works fine if you actually fully deploy the app, but not with Unity Remote, which is super useful for
    /// developing mobile apps.  So, for now, this uses the Legacy InputModule.  When Unity Remote is updated, we should
    /// update this script as well so we are consistent with all of MinVR using the New Input System.
    ///
    /// Note: You can switch which input system you want to use for your app in Player Settings, and it is possible to
    /// select "Both".  That works well if you want to use the New Input System's better support for XR and the Remote's
    /// better support for touch, for example.
    /// </summary>
    [AddComponentMenu("MinVR/Input/Mobile Sensors (Legacy Unity Input Module)")]
    public class MobileSensors : MonoBehaviour, IPolledInputDevice
    {

        private void Reset()
        {
            m_DeviceIdString = "Mobile/";
            m_GyroscopeAttitudeEventName = "Rotation";
            //m_DeviceOrientationChangedEventName = "DeviceOrientation";
            m_CompassHeadingEventName = "Heading";
            m_AccelerationEventName = "Acceleration";
        }

        private void OnEnable()
        {
            VREngine.instance.eventManager.AddPolledInputDevice(this);
        }

        private void OnDisable()
        {
            VREngine.instance?.eventManager?.RemovePolledInputDevice(this);
        }


        void Start()
        {
            if (m_GyroscopeAttitudeEventName != "") {
                Input.gyro.enabled = true;
            }
            if (m_CompassHeadingEventName != "") {
                Input.compass.enabled = true;
            }
        }


        public void PollForEvents(ref List<VREvent> eventQueue)
        {
            if ((m_GyroscopeAttitudeEventName != "") && (SystemInfo.supportsGyroscope) && (Input.gyro.enabled)) {
                // orig rh sensor reading
                Quaternion q = Input.gyro.attitude;
                // convert to lh coordinates
                q = new Quaternion(q.x, q.y, -q.z, -q.w);
                // rotate to align with Unity's axes conventions
                q = Quaternion.Euler(new Vector3(90, 0, 0)) * q;
                eventQueue.Add(new VREventQuaternion(m_DeviceIdString + m_GyroscopeAttitudeEventName, q));
            }

            //if (m_DeviceOrientationChangedEventName != "") {
            //    if (Input.deviceOrientation != m_LastOrientation) {
            //        eventQueue.Add(new VREvent<DeviceOrientation>(m_DeviceIdString + m_DeviceOrientationChangedEventName, Input.deviceOrientation));
            //        m_LastOrientation = Input.deviceOrientation;
            //    }
            //}

            if ((m_CompassHeadingEventName != "") && (Input.compass.enabled)) {
                if (Input.compass.trueHeading != m_LastHeading) {
                    eventQueue.Add(new VREventFloat(m_DeviceIdString + m_CompassHeadingEventName, Input.compass.magneticHeading));
                    m_LastHeading = Input.compass.magneticHeading;
               }
            }

            if ((m_AccelerationEventName != "") && (SystemInfo.supportsAccelerometer)) {
                // find average over the last frame
                Vector3 acceleration = Vector3.zero;
                foreach (AccelerationEvent accEvent in Input.accelerationEvents) {
                    acceleration += accEvent.acceleration * accEvent.deltaTime;
                }
                // convert to lh
                acceleration.x = -acceleration.x;
                // align with Unity's axes conventions

                // TODO: we probably need to apply a rotation here; can't test right now as my device
                // is not reporting acceleration!

                eventQueue.Add(new VREventVector3(m_DeviceIdString + m_AccelerationEventName, acceleration));
            }

        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();
       
            if (m_GyroscopeAttitudeEventName != "") {
                eventsProduced.Add(VREventPrototypeQuaternion.Create(m_DeviceIdString + m_GyroscopeAttitudeEventName));
            }

            //if (m_DeviceOrientationChangedEventName != "") {
            //    eventsProduced.Add(VREventPrototype<DeviceOrientation>.Create(m_DeviceIdString + m_DeviceOrientationChangedEventName));
            //}

            if (m_CompassHeadingEventName != "") {
                eventsProduced.Add(VREventPrototypeFloat.Create(m_DeviceIdString + m_CompassHeadingEventName));
            }

            if (m_AccelerationEventName != "") {
                eventsProduced.Add(VREventPrototypeVector3.Create(m_DeviceIdString + m_AccelerationEventName));
            }

            return eventsProduced;
        }




        [Tooltip("Prepended to the name of each VREvent produced")]
        [SerializeField] private string m_DeviceIdString = "Mobile/";

        [Tooltip("The name of the VREvent to generate when the gyro attitude changes.")]
        public string m_GyroscopeAttitudeEventName = "";

        //[Tooltip("The name of the VREvent to generate when the discrete device orientation changes.  " +
        //    "(Unknown, Portrait, PortraitUpsideDown, LandscapeLeft, LandscapeRight, FaceUp, FaceDown)")]
        //public string m_DeviceOrientationChangedEventName = "";

        [Tooltip("The name of the VREvent to generate when the compass heading changes.")]
        public string m_CompassHeadingEventName = "";

        [Tooltip("The name of the VREvent to generate for acceleration events (averaged over the past frame).")]
        public string m_AccelerationEventName = "";

        //private DeviceOrientation m_LastOrientation = DeviceOrientation.Unknown;
        private float m_LastHeading = 0.0f;
    }

} // namespace
