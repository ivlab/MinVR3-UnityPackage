#if XR_INTERACTION_TOOLKIT_PRESENT || BUILDING_DOCS

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace IVLab.MinVR3
{
    [AddComponentMenu("XR/XR Controller (MinVR-based)", 11)]
    public class MinVRBasedController : XRBaseController, IVREventListener
    {

        [Tooltip("Controller position event.")]
        public VREventPrototypeVector3 m_PositionEvent = new VREventPrototypeVector3();

        [Tooltip("Controller rotation event.")]
        public VREventPrototypeQuaternion m_RotationEvent = new VREventPrototypeQuaternion();

        [Tooltip("Button down event to use for Canvas UI interaction.")]
        public VREventPrototype m_UIButtonDownEvent = new VREventPrototype();

        [Tooltip("Button down event to use for Canvas UI interaction.")]
        public VREventPrototype m_UIButtonUpEvent = new VREventPrototype();


        protected override void OnEnable()
        {
            base.OnEnable();
            StartListening();
        }


        protected override void OnDisable()
        {
            base.OnDisable();
            StopListening();
        }


        /// <inheritdoc />
        protected override void UpdateTrackingInput(XRControllerState controllerState)
        {
            base.UpdateTrackingInput(controllerState);
            if (controllerState == null)
                return;

            bool trackingPosition = (m_PositionEvent.GetEventName() != "");
            bool trackingRotation = (m_RotationEvent.GetEventName() != "");

            // Update inputTrackingState
            controllerState.inputTrackingState = InputTrackingState.None;
            if (trackingPosition) {
                controllerState.inputTrackingState = controllerState.inputTrackingState | InputTrackingState.Position;
            }
            if (trackingRotation) {
                controllerState.inputTrackingState = controllerState.inputTrackingState | InputTrackingState.Rotation;
            }

            // Update position
            if (trackingPosition) {
                controllerState.position = m_Position;
            }

            // Update rotation
            if (trackingRotation) {
                controllerState.rotation = m_Rotation;
            }
        }


        /// <inheritdoc />
        protected override void UpdateInput(XRControllerState controllerState)
        {
            base.UpdateInput(controllerState);
            if (controllerState == null)
                return;

            controllerState.ResetFrameDependentStates();
            controllerState.uiPressInteractionState.SetFrameState(m_UIBtnDown);
        }

        public void OnVREvent(VREvent vrEvent)
        {
            if (enabled) {
                if (vrEvent.Matches(m_PositionEvent)) {
                    m_Position = vrEvent.GetData<Vector3>();
                } else if (vrEvent.Matches(m_RotationEvent)) {
                    m_Rotation = vrEvent.GetData<Quaternion>();
                } else if (vrEvent.Matches(m_UIButtonDownEvent)) {
                    m_UIBtnDown = true;
                } else if (vrEvent.Matches(m_UIButtonUpEvent)) {
                    m_UIBtnDown = false;
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

        bool m_UIBtnDown = false;
        Vector3 m_Position = new Vector3();
        Quaternion m_Rotation = new Quaternion();
    }
}

#endif
