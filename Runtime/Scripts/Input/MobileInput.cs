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
    [AddComponentMenu("MinVR/Input/Mobile Input (Legacy InputModule Only)")]
    [DefaultExecutionOrder(-998)] // make sure this script runs right before VREngine.cs
    public class MobileInput : MonoBehaviour, IVREventProducer
    {

        private void Reset()
        {
            m_BaseEventNames = new[] {
                "Touch/Finger 0",
                "Touch/Finger 1",
                "Touch/Finger 2",
                "Touch/Finger 3",
                "Touch/Finger 4",
                "Touch/Finger 5",
                "Touch/Finger 6",
                "Touch/Finger 7",
                "Touch/Finger 8",
                "Touch/Finger 9"
            };

            m_GyroscopeAttitudeEventName = "Mobile/Rotation";
            m_DeviceOrientationChangedEventName = "Mobile/DeviceOrientation";
            m_CompassHeadingEventName = "Mobile/Heading";
            m_AccelerationEventName = "Mobile/Acceleration";
        }

        void Start()
        {
            m_uidToFinger = new Dictionary<int, int>();

            if (m_GyroscopeAttitudeEventName != "") {
                Input.gyro.enabled = true;
            }
            if (m_CompassHeadingEventName != "") {
                Input.compass.enabled = true;
            }
        }


        void Update()
        {
            // TOUCH 
            if (Input.touchCount > 0) {
                for (int i = 0; i < Input.touchCount; i++) {
                    Touch touch = Input.GetTouch(i);
                    int uid = touch.fingerId;
                    int fingerId;
                    if (m_uidToFinger.ContainsKey(uid)) {
                        fingerId = m_uidToFinger[uid];
                    } else {
                        // find the smallest available finger id, should be a value 0 to the max touches that
                        // can be sensed by the hardware
                        fingerId = 0;
                        while (m_uidToFinger.ContainsValue(fingerId)) {
                            fingerId++;
                        }
                        m_uidToFinger.Add(uid, fingerId);
                    }

                    string baseName = "Mobile/Finger " + fingerId;
                    if (fingerId < m_BaseEventNames.Length) {
                        baseName = m_BaseEventNames[fingerId];
                    }

                    switch (touch.phase) {
                        case TouchPhase.Began:
                            VREngine.instance.eventManager.QueueEvent(baseName + " DOWN", touch.position);
                            VREngine.instance.eventManager.QueueEvent(baseName + "/Position", touch.position);
                            break;

                        case TouchPhase.Moved:
                            VREngine.instance.eventManager.QueueEvent(baseName + "/Position", touch.position);
                            if (m_IncludePressureEvents) {
                                VREngine.instance.eventManager.QueueEvent(baseName + "/Pressure", touch.pressure / touch.maximumPossiblePressure);
                            }
                            break;

                        // Report that a direction has been chosen when the finger is lifted.
                        case TouchPhase.Ended:
                            VREngine.instance.eventManager.QueueEvent(baseName + " UP", touch.position);
                            m_uidToFinger.Remove(uid);
                            break;
                    }
                }
            }




            // SENSORS
            if ((m_GyroscopeAttitudeEventName != "") && (SystemInfo.supportsGyroscope) && (Input.gyro.enabled)) {
                // orig rh sensor reading
                Quaternion q = Input.gyro.attitude;
                // convert to lh coordinates
                q = new Quaternion(q.x, q.y, -q.z, -q.w);
                // rotate to align with Unity's axes conventions
                q = Quaternion.Euler(new Vector3(90, 0, 0)) * q;
                VREngine.instance.eventManager.QueueEvent(m_GyroscopeAttitudeEventName, q);
            }

            if (m_DeviceOrientationChangedEventName != "") {
                if (Input.deviceOrientation != m_LastOrientation) {
                    VREngine.instance.eventManager.QueueEvent(m_DeviceOrientationChangedEventName, Input.deviceOrientation);
                    m_LastOrientation = Input.deviceOrientation;
                }
            }

            if ((m_CompassHeadingEventName != "") && (Input.compass.enabled)) {
                if (Input.compass.trueHeading != m_LastHeading) {
                    VREngine.instance.eventManager.QueueEvent(m_CompassHeadingEventName, Input.compass.magneticHeading);
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

                VREngine.instance.eventManager.QueueEvent(m_AccelerationEventName, acceleration);
            }

        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();
            for (int i = 0; i < m_BaseEventNames.Length; i++) {
                eventsProduced.Add(VREventPrototypeVector2.Create(m_BaseEventNames[i] + " DOWN"));
                eventsProduced.Add(VREventPrototypeVector2.Create(m_BaseEventNames[i] + "/Position"));
                if (m_IncludePressureEvents) {
                    eventsProduced.Add(VREventPrototypeFloat.Create(m_BaseEventNames[i] + "/Pressure"));
                }
                eventsProduced.Add(VREventPrototypeVector2.Create(m_BaseEventNames[i] + " UP"));
            }

            if (m_GyroscopeAttitudeEventName != "") {
                eventsProduced.Add(VREventPrototypeQuaternion.Create(m_GyroscopeAttitudeEventName));
            }

            //if (m_DeviceOrientationChangedEventName != "") {
            //    eventsProduced.Add(VREventPrototype<DeviceOrientation>.Create(m_DeviceOrientationChangedEventName));
            //}

            if (m_CompassHeadingEventName != "") {
                eventsProduced.Add(VREventPrototypeFloat.Create(m_CompassHeadingEventName));
            }

            if (m_AccelerationEventName != "") {
                eventsProduced.Add(VREventPrototypeVector3.Create(m_AccelerationEventName));
            }

            return eventsProduced;
        }


        [Header("Touch")]
        [Tooltip("If true, each position update event will be followed immediately by a pressure update event.")]
        public bool m_IncludePressureEvents = true;

        [Tooltip("Similar to the way you can assign a custom VREvent name to a keyboard keypress event, you can " +
            "also customize the touch event names.  To remap every possible event, the size of the array should " +
            "equal the max number of simultaneous touches supported by the hardware.  A default name will be used " +
            "if the array does not contain enough entries.  Indices are reused and most systems have no way of " +
            "telling whether the exact same finger was lifted and then placed down again, so indices are consistent " +
            "between the DOWN and UP events, but once an UP event is received there is no guarantee that index 0 " +
            "will be assigned to the same physical finger the next time that finger produces a touch.")]
        public string[] m_BaseEventNames;

        

        [Header("Sensors")]
        [Tooltip("The name of the VREvent to generate when the gyro attitude changes.")]
        public string m_GyroscopeAttitudeEventName = "";

        [Tooltip("The name of the VREvent to generate when the discrete device orientation changes.  " +
            "(Unknown, Portrait, PortraitUpsideDown, LandscapeLeft, LandscapeRight, FaceUp, FaceDown)")]
        public string m_DeviceOrientationChangedEventName = "";

        [Tooltip("The name of the VREvent to generate when the compass heading changes.")]
        public string m_CompassHeadingEventName = "";

        [Tooltip("The name of the VREvent to generate for acceleration events (averaged over the past frame).")]
        public string m_AccelerationEventName = "";

        // maps the uid's returned by multi-touch devices to a "finger" id, where fingerIds range from 0 to the
        // max number of simultaneously supported touches on the system.  Finger IDs are consistent while the
        // touch persists, but IDs are reused for later touches.  So, you'll always get the same ID for Position
        // and Pressure events that come between an UP and DOWN event, but after the DOWN event, there is no
        // guarantee that the next time you see an event with the same finger it will correspond to the user's
        // same finger.  Finger 0 might be their index finger for one touch; and it might be their middle finger
        // for the next touch.
        private Dictionary<int, int> m_uidToFinger;

        private DeviceOrientation m_LastOrientation = DeviceOrientation.Unknown;
        private float m_LastHeading = 0.0f;
    }

} // namespace
