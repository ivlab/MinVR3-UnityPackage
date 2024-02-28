
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace IVLab.MinVR3
{
    /// <summary>
    /// Note: This version of the TrackedPoseDriver was orginally based on the one provided with
    /// Unity's XR Interaction Toolkit, but their approach, which includes defining when the update
    /// should occur (onUpdate, beforeRender, or both) is not a great fit with an event-based
    /// system like MinVR3.  This version uses VREvents so the transform's position and rotation
    /// will be updated while responding to VREvents dispatched by the VREventManager, which should
    /// be one of the first scripts run during Update().
    /// </summary>
    [AddComponentMenu("MinVR Interaction/Cursors/Tracked Pose Driver")]
    public class TrackedPoseDriver : MonoBehaviour, IVREventListener
    {
        [Header("Tracking VREvents")]
        [SerializeField]
        VREventPrototypeVector3 m_PositionEvent;
        public VREventPrototypeVector3 positionEvent {
            get { return m_PositionEvent; }
            set {
                m_PositionEvent = value;
            }
        }

        [SerializeField]
        VREventPrototypeQuaternion m_RotationEvent;
        public VREventPrototypeQuaternion rotationEvent {
            get { return m_RotationEvent; }
            set { m_RotationEvent = value; }
        }


        [Header("Optional Calibration Offsets")]
        [SerializeField]
        Vector3 m_CalibrationRotation;
        public Vector3 calibrationRotation {
            get { return m_CalibrationRotation; }
            set { m_CalibrationRotation = value; }
        }

        [SerializeField]
        Vector3 m_CalibrationTranslation;
        public Vector3 calibrationTranslation {
            get { return m_CalibrationTranslation; }
            set { m_CalibrationTranslation = value; }
        }

        public Vector3 GetPositionInRoomSpace()
        {
            return transform.LocalPointToRoomSpace(Vector3.zero);
        }

        public Vector3 GetPositionInWorldSpace()
        {
            return transform.LocalPointToWorldSpace(Vector3.zero);
        }

        public Vector3 GetForwardDirInRoomSpace()
        {
            return Vector3.Normalize(transform.LocalVectorToRoomSpace(Vector3.forward));
        }

        public Vector3 GetForwardDirInWorldSpace()
        {
            return Vector3.Normalize(transform.LocalVectorToWorldSpace(Vector3.forward));
        }

        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_RotationEvent)) {
                transform.localRotation = vrEvent.GetData<Quaternion>() * Quaternion.Euler(m_CalibrationRotation);
            } else if (vrEvent.Matches(m_PositionEvent)) {
                Vector3 rotatedPositionOffset = transform.localRotation * m_CalibrationTranslation;
                transform.localPosition = vrEvent.GetData<Vector3>() + rotatedPositionOffset;
            }
        }

        private void Reset()
        {
            m_PositionEvent = new VREventPrototypeVector3();
            m_RotationEvent = new VREventPrototypeQuaternion();
        }

        protected void OnEnable()
        {
            StartListening();
        }

        protected void OnDisable()
        {
            StopListening();
        }

        public void StartListening()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        public void StopListening()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }
    }

} // end namespace
