// This functionality is only available in projects using Unity's New Input System
#if ENABLE_INPUT_SYSTEM || BUILDING_DOCS

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IVLab.MinVR3
{

    /// <summary>
    /// Uses Unity's XR system to create events for a common VR system with a tracked head and
    /// tracked left/right controllers.
    /// </summary>
    [AddComponentMenu("MinVR/Input/Unity XR (Head, LeftHand, RightHand)")]
    public class UnityXR : MonoBehaviour, IVREventProducer, IPolledInputDevice
    {
        void Reset()
        {
            m_DeviceIdString = "UnityXR/";
        }

        protected void OnEnable()
        {
            VREngine.Instance.eventManager.AddPolledInputDevice(this);
        }

        protected void OnDisable()
        {
            VREngine.Instance?.eventManager?.RemovePolledInputDevice(this);
        }

        void Start()
        {
            m_LastLeftTriggerButton = false;
            m_LastLeftGripButton = false;
            m_LastLeftPrimaryButton = false;
            m_LastLeftSecondaryButton = false;
            m_LastLeftTrigger = 0.0f;
            m_LastLeftGrip = 0.0f;
            m_LastLeftPrimary2DAxis = new Vector2();
            m_LastLeftSecondary2DAxis = new Vector2();
            m_LastLeftPosition = new Vector3();
            m_LastLeftRotation = Quaternion.identity;

            m_LastRightTriggerButton = false;
            m_LastRightGripButton = false;
            m_LastRightPrimaryButton = false;
            m_LastRightSecondaryButton = false;
            m_LastRightTrigger = 0.0f;
            m_LastRightGrip = 0.0f;
            m_LastRightPrimary2DAxis = new Vector2();
            m_LastRightSecondary2DAxis = new Vector2();
            m_LastRightPosition = new Vector3();
            m_LastRightRotation = Quaternion.identity;

            m_LastHeadPosition = new Vector3();
            m_LastHeadRotation = Quaternion.identity;
        }


        void CheckButton(UnityEngine.XR.InputDevice device, UnityEngine.XR.InputFeatureUsage<bool> buttonUsage, string eventName, ref bool lastButtonState, ref List<VREvent> eventQueue)
        {
            bool btnState = false;
            if (device.TryGetFeatureValue(buttonUsage, out btnState)) {
                if (btnState != lastButtonState) {
                    string stateStr = btnState ? "/Down" : "/Up";
                    eventQueue.Add(new VREvent(eventName + stateStr));
                    lastButtonState = btnState;
                }
            }
        }

        void CheckAnalog(UnityEngine.XR.InputDevice device, UnityEngine.XR.InputFeatureUsage<float> usage, string eventName, ref float lastValue, ref List<VREvent> eventQueue)
        {
            float value = 0.0f;
            if (device.TryGetFeatureValue(usage, out value)) {
                if (value != lastValue) {
                    eventQueue.Add(new VREventFloat(eventName, value));
                    lastValue = value;
                }
            }
        }

        void Check2DAxis(UnityEngine.XR.InputDevice device, UnityEngine.XR.InputFeatureUsage<Vector2> usage, string eventName, ref Vector2 lastValue, ref List<VREvent> eventQueue)
        {
            Vector2 value;
            if (device.TryGetFeatureValue(usage, out value)) {
                if (value != lastValue) {
                    eventQueue.Add(new VREventVector2(eventName, value));
                    lastValue = value;
                }
            }
        }

        void CheckVector3(UnityEngine.XR.InputDevice device, UnityEngine.XR.InputFeatureUsage<Vector3> usage, string eventName, ref Vector3 lastValue, ref List<VREvent> eventQueue)
        {
            Vector3 value;
            if (device.TryGetFeatureValue(usage, out value)) {
                if (value != lastValue) {
                    eventQueue.Add(new VREventVector3(eventName, value));
                    lastValue = value;
                }
            }
        }

        void CheckQuaternion(UnityEngine.XR.InputDevice device, UnityEngine.XR.InputFeatureUsage<Quaternion> usage, string eventName, ref Quaternion lastValue, ref List<VREvent> eventQueue)
        {
            Quaternion value;
            if (device.TryGetFeatureValue(usage, out value)) {
                if (value != lastValue) {
                    eventQueue.Add(new VREventQuaternion(eventName, value));
                    lastValue = value;
                }
            }
        }

        public void PollForEvents(ref List<VREvent> eventQueue)
        {
            UnityEngine.XR.InputDevice lHandDev;
            var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);
            if (leftHandDevices.Count > 0) {
                lHandDev = leftHandDevices[0];
                Debug.Log(string.Format("UnityXR Left Hand Device '{0}' has characteristics '{1}'", lHandDev.name, lHandDev.characteristics.ToString()));
                CheckButton(lHandDev, UnityEngine.XR.CommonUsages.triggerButton, m_DeviceIdString + "LeftHand/Trigger", ref m_LastLeftTriggerButton, ref eventQueue);
                CheckButton(lHandDev, UnityEngine.XR.CommonUsages.gripButton, m_DeviceIdString + "LeftHand/Grip", ref m_LastLeftGripButton, ref eventQueue);
                CheckButton(lHandDev, UnityEngine.XR.CommonUsages.primaryButton, m_DeviceIdString + "LeftHand/PrimaryButton", ref m_LastLeftPrimaryButton, ref eventQueue);
                CheckButton(lHandDev, UnityEngine.XR.CommonUsages.secondaryButton, m_DeviceIdString + "LeftHand/SecondaryButton", ref m_LastLeftSecondaryButton, ref eventQueue);
                CheckAnalog(lHandDev, UnityEngine.XR.CommonUsages.trigger, m_DeviceIdString + "LeftHand/Trigger/Value", ref m_LastLeftTrigger, ref eventQueue);
                CheckAnalog(lHandDev, UnityEngine.XR.CommonUsages.grip, m_DeviceIdString + "LeftHand/Grip/Value", ref m_LastLeftGrip, ref eventQueue);
                Check2DAxis(lHandDev, UnityEngine.XR.CommonUsages.primary2DAxis, m_DeviceIdString + "LeftHand/Primary2DAxis/Value", ref m_LastLeftPrimary2DAxis, ref eventQueue);
                Check2DAxis(lHandDev, UnityEngine.XR.CommonUsages.secondary2DAxis, m_DeviceIdString + "LeftHand/Secondary2DAxis/Value", ref m_LastLeftSecondary2DAxis, ref eventQueue);
                CheckVector3(lHandDev, UnityEngine.XR.CommonUsages.devicePosition, m_DeviceIdString + "LeftHand/Position", ref m_LastLeftPosition, ref eventQueue);
                CheckQuaternion(lHandDev, UnityEngine.XR.CommonUsages.deviceRotation, m_DeviceIdString + "LeftHand/Rotation", ref m_LastLeftRotation, ref eventQueue);
            }

            UnityEngine.XR.InputDevice rHandDev;
            var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
            if (rightHandDevices.Count > 0) {
                rHandDev = rightHandDevices[0];
                Debug.Log(string.Format("UnityXR Right Hand Device '{0}' has characteristics '{1}'", rHandDev.name, rHandDev.characteristics.ToString()));
                CheckButton(rHandDev, UnityEngine.XR.CommonUsages.triggerButton, m_DeviceIdString + "RightHand/Trigger", ref m_LastRightTriggerButton, ref eventQueue);
                CheckButton(rHandDev, UnityEngine.XR.CommonUsages.gripButton, m_DeviceIdString + "RightHand/Grip", ref m_LastRightGripButton, ref eventQueue);
                CheckButton(rHandDev, UnityEngine.XR.CommonUsages.primaryButton, m_DeviceIdString + "RightHand/PrimaryButton", ref m_LastRightPrimaryButton, ref eventQueue);
                CheckButton(rHandDev, UnityEngine.XR.CommonUsages.secondaryButton, m_DeviceIdString + "RightHand/SecondaryButton", ref m_LastRightSecondaryButton, ref eventQueue);
                CheckAnalog(rHandDev, UnityEngine.XR.CommonUsages.trigger, m_DeviceIdString + "RightHand/Trigger/Value", ref m_LastRightTrigger, ref eventQueue);
                CheckAnalog(rHandDev, UnityEngine.XR.CommonUsages.grip, m_DeviceIdString + "RightHand/Grip/Value", ref m_LastRightGrip, ref eventQueue);
                Check2DAxis(rHandDev, UnityEngine.XR.CommonUsages.primary2DAxis, m_DeviceIdString + "RightHand/Primary2DAxis/Value", ref m_LastRightPrimary2DAxis, ref eventQueue);
                Check2DAxis(rHandDev, UnityEngine.XR.CommonUsages.secondary2DAxis, m_DeviceIdString + "RightHand/Secondary2DAxis/Value", ref m_LastRightSecondary2DAxis, ref eventQueue);
                CheckVector3(rHandDev, UnityEngine.XR.CommonUsages.devicePosition, m_DeviceIdString + "RightHand/Position", ref m_LastRightPosition, ref eventQueue);
                CheckQuaternion(rHandDev, UnityEngine.XR.CommonUsages.deviceRotation, m_DeviceIdString + "RightHand/Rotation", ref m_LastRightRotation, ref eventQueue);
            }

            UnityEngine.XR.InputDevice headDev;
            var headDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.CenterEye, headDevices);
            if (headDevices.Count > 0) {
                headDev = headDevices[0];
                Debug.Log(string.Format("UnityXR Head Device '{0}' has characteristics '{1}'", headDev.name, headDev.characteristics.ToString()));
                CheckVector3(headDev, UnityEngine.XR.CommonUsages.centerEyePosition, m_DeviceIdString + "Head/Position", ref m_LastHeadPosition, ref eventQueue);
                CheckQuaternion(headDev, UnityEngine.XR.CommonUsages.centerEyeRotation, m_DeviceIdString + "Head/Rotation", ref m_LastHeadRotation, ref eventQueue);
            }
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();

            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "LeftHand/Trigger/Down"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "LeftHand/Trigger/Up"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "LeftHand/Grip/Down"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "LeftHand/Grip/Up"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "LeftHand/PrimaryButton/Down"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "LeftHand/PrimaryButton/Up"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "LeftHand/SecondaryButton/Down"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "LeftHand/SecondaryButton/Up"));
            eventsProduced.Add(VREventPrototypeFloat.Create(m_DeviceIdString + "LeftHand/Trigger/Value"));
            eventsProduced.Add(VREventPrototypeFloat.Create(m_DeviceIdString + "LeftHand/Grip/Value"));
            eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + "LeftHand/Primary2DAxis/Value"));
            eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + "LeftHand/Secondary2DAxis/Value"));
            eventsProduced.Add(VREventPrototypeVector3.Create(m_DeviceIdString + "LeftHand/Position"));
            eventsProduced.Add(VREventPrototypeQuaternion.Create(m_DeviceIdString + "LeftHand/Rotation"));

            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "RightHand/Trigger/Down"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "RightHand/Trigger/Up"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "RightHand/Grip/Down"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "RightHand/Grip/Up"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "RightHand/PrimaryButton/Down"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "RightHand/PrimaryButton/Up"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "RightHand/SecondaryButton/Down"));
            eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + "RightHand/SecondaryButton/Up"));
            eventsProduced.Add(VREventPrototypeFloat.Create(m_DeviceIdString + "RightHand/Trigger/Value"));
            eventsProduced.Add(VREventPrototypeFloat.Create(m_DeviceIdString + "RightHand/Grip/Value"));
            eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + "RightHand/Primary2DAxis/Value"));
            eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + "RightHand/Secondary2DAxis/Value"));
            eventsProduced.Add(VREventPrototypeVector3.Create(m_DeviceIdString + "RightHand/Position"));
            eventsProduced.Add(VREventPrototypeQuaternion.Create(m_DeviceIdString + "RightHand/Rotation"));

            eventsProduced.Add(VREventPrototypeVector3.Create(m_DeviceIdString + "Head/Position"));
            eventsProduced.Add(VREventPrototypeQuaternion.Create(m_DeviceIdString + "Head/Rotation"));

            return eventsProduced;
        }


        [Tooltip("Prepended to the name of each VREvent produced")]
        [SerializeField] private string m_DeviceIdString = "UnityXR/";

        // RUNTIME STATE INFO
        private bool m_LastLeftTriggerButton;
        private bool m_LastLeftGripButton;
        private bool m_LastLeftPrimaryButton;
        private bool m_LastLeftSecondaryButton;
        private float m_LastLeftTrigger;
        private float m_LastLeftGrip;
        private Vector2 m_LastLeftPrimary2DAxis;
        private Vector2 m_LastLeftSecondary2DAxis;
        private Vector3 m_LastLeftPosition;
        private Quaternion m_LastLeftRotation;

        private bool m_LastRightTriggerButton;
        private bool m_LastRightGripButton;
        private bool m_LastRightPrimaryButton;
        private bool m_LastRightSecondaryButton;
        private float m_LastRightTrigger;
        private float m_LastRightGrip;
        private Vector2 m_LastRightPrimary2DAxis;
        private Vector2 m_LastRightSecondary2DAxis;
        private Vector3 m_LastRightPosition;
        private Quaternion m_LastRightRotation;

        private Vector3 m_LastHeadPosition;
        private Quaternion m_LastHeadRotation;
    }


} // namespace

#endif
