#if XR_INTERACTION_TOOLKIT_PRESENT || BUILDING_DOCS

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace IVLab.MinVR3
{

    public class MinVRController : MonoBehaviour, IVREventListener
    {
        public string m_DeviceName;
        public string m_CommonUsages;

        [Tooltip("Controller position event.")]
        public VREventPrototypeVector3 m_PositionEvent = new VREventPrototypeVector3();

        [Tooltip("Controller rotation event.")]
        public VREventPrototypeQuaternion m_RotationEvent = new VREventPrototypeQuaternion();

        [Tooltip("Primary button down event.")]
        public VREventPrototype m_PrimaryButtonDownEvent = new VREventPrototype();

        [Tooltip("Primary button up event.")]
        public VREventPrototype m_PrimaryButtonUpEvent = new VREventPrototype();


        [Tooltip("Secondary button down event.")]
        public VREventPrototype m_SecondaryButtonDownEvent = new VREventPrototype();

        [Tooltip("Secondary button up event.")]
        public VREventPrototype m_SecondaryButtonUpEvent = new VREventPrototype();


        [Tooltip("Primary 1D axis event (1D analog like a trigger, grip).")]
        public VREventPrototypeFloat m_PrimaryAxisEvent = new VREventPrototypeFloat();

        [Tooltip("Primary 2D axis event (2D axis like a joystick or trackpad.")]
        public VREventPrototypeVector2 m_Primary2DAxisEvent = new VREventPrototypeVector2();


        private void Reset()
        {
            m_DeviceName = "MinVRLeftHand";
            m_CommonUsages = $"{CommonUsages.LeftHand}";
        }

        protected virtual void Awake()
        {
            m_ControllerState.Reset();
        }

        protected virtual void OnEnable()
        {
            m_ControllerDevice = InputSystem.AddDevice<MinVRControllerDevice>(m_DeviceName);
            if (m_ControllerDevice != null) {
                InputSystem.SetDeviceUsage(m_ControllerDevice, m_CommonUsages);
            } else {
                Debug.LogError($"Failed to create {nameof(m_ControllerDevice)} for {m_CommonUsages}.", this);
            }
        }


        protected virtual void OnDisable()
        {
            if (m_ControllerDevice != null && m_ControllerDevice.added)
                InputSystem.RemoveDevice(m_ControllerDevice);
        }

        protected virtual void Update()
        {
            if ((m_ControllerDevice != null) && (m_StateChanged)) {
                InputState.Change(m_ControllerDevice, m_ControllerState);
                m_StateChanged = false;
            }
        }

        public void OnVREvent(VREvent vrEvent)
        {
            if (enabled) {
                if (vrEvent.Matches(m_PositionEvent)) {
                    m_ControllerState.devicePosition = vrEvent.GetData<Vector3>();
                    m_StateChanged = true;
                } else if (vrEvent.Matches(m_RotationEvent)) {
                    m_ControllerState.deviceRotation = vrEvent.GetData<Quaternion>();
                    m_StateChanged = true;
                } else if (vrEvent.Matches(m_PrimaryButtonDownEvent)) {
                    m_ControllerState.WithButton(MinVRControllerButton.PrimaryButton, true);
                    m_StateChanged = true;
                } else if (vrEvent.Matches(m_PrimaryButtonUpEvent)) {
                    m_ControllerState.WithButton(MinVRControllerButton.PrimaryButton, false);
                    m_StateChanged = true;
                } else if (vrEvent.Matches(m_SecondaryButtonDownEvent)) {
                    m_ControllerState.WithButton(MinVRControllerButton.SecondaryButton, true);
                    m_StateChanged = true;
                } else if (vrEvent.Matches(m_SecondaryButtonUpEvent)) {
                    m_ControllerState.WithButton(MinVRControllerButton.SecondaryButton, false);
                    m_StateChanged = true;
                } else if (vrEvent.Matches(m_PrimaryAxisEvent)) {
                    m_ControllerState.primaryAxis = vrEvent.GetData<float>();
                    m_StateChanged = true;
                } else if (vrEvent.Matches(m_Primary2DAxisEvent)) {
                    m_ControllerState.primary2DAxis = vrEvent.GetData<Vector2>();
                    m_StateChanged = true;
                }
            }
        }

        public void StartListening()
        {
            VREngine.Instance.eventManager.AddEventListener(this, VREventManager.DefaultListenerPriority - 1);
        }

        public void StopListening()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }

        bool m_StateChanged = false;
        MinVRControllerState m_ControllerState;
        MinVRControllerDevice m_ControllerDevice;
    }
}

#endif
