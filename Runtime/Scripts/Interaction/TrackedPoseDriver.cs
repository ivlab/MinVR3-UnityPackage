
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
#endif

namespace IVLab.MinVR3
{
    /// <summary>
    /// Note: This version of the TrackedPoseDriver is modified from the original provided with Unity's XR Interaction
    /// Toolkit.  The modified version adds:
    /// 1. It listens for VREvents rather than Unity Actions
    /// 2. It includes base rotation and translation amounts that can be used, for example, to calibrate a tracker
    ///    attached to a physical prop.
    /// 3. It works with both the new input system and the old input system
    /// 
    /// The TrackedPoseDriver component applies the current Pose value of a tracked device to the transform of the GameObject.
    /// TrackedPoseDriver can track multiple types of devices including XR HMDs, controllers, and remotes.
    /// </summary>
    [AddComponentMenu("MinVR Interaction/Cursors/Tracked Pose Driver")]
    public class TrackedPoseDriver : MonoBehaviour, IVREventListener
    {
        public enum TrackingType
        {
            RotationAndPosition,
            RotationOnly,
            PositionOnly
        }

        [Header("Tracking")]
        [SerializeField]
        TrackingType m_TrackingType;

        /// <summary>
        /// The tracking type being used by the tracked pose driver
        /// </summary>
        public TrackingType trackingType {
            get { return m_TrackingType; }
            set { m_TrackingType = value; }
        }

        public enum UpdateType
        {
            UpdateAndBeforeRender,
            Update,
            BeforeRender,
        }


        [SerializeField]
        UpdateType m_UpdateType = UpdateType.UpdateAndBeforeRender;
        /// <summary>
        /// The update type being used by the tracked pose driver
        /// </summary>
        public UpdateType updateType {
            get { return m_UpdateType; }
            set { m_UpdateType = value; }
        }

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


        [Header("Calibration")]
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


        Vector3 m_CurrentPosition;
        Quaternion m_CurrentRotation;



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
            if (vrEvent.Matches(m_RotationEvent))
            {
                m_CurrentRotation = vrEvent.GetData<Quaternion>() * Quaternion.Euler(m_CalibrationRotation);
            }

            if (vrEvent.Matches(m_PositionEvent)) {
                Vector3 rotatedPositionOffset = m_CurrentRotation * m_CalibrationTranslation;
                m_CurrentPosition = vrEvent.GetData<Vector3>() + rotatedPositionOffset;
            }
        }


        private void Reset()
        {
            m_PositionEvent = new VREventPrototypeVector3();
            m_RotationEvent = new VREventPrototypeQuaternion();
        }

        protected virtual void Awake()
        {
#if UNITY_INPUT_SYSTEM_ENABLE_VR && ENABLE_VR
            if (HasStereoCamera())
            {
                UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(GetComponent<Camera>(), true);
            }
#endif
        }

        private void Start()
        {
            // sets current to whatever is stored in the transform so the transform can be used to set
            // a default pos/rot for the tracker.
            m_CurrentPosition = transform.localPosition;
            m_CurrentRotation = transform.localRotation;
        }

        protected void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem.onAfterUpdate += UpdateCallback;
#endif
            StartListening();
        }

        void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem.onAfterUpdate -= UpdateCallback;
#endif
            StopListening();
        }

        protected virtual void OnDestroy()
        {
#if UNITY_INPUT_SYSTEM_ENABLE_VR && ENABLE_VR
            if (HasStereoCamera())
            {
                UnityEngine.XR.XRDevice.DisableAutoXRCameraTracking(GetComponent<Camera>(), false);
            }
#endif
        }

#if ENABLE_INPUT_SYSTEM
        protected void UpdateCallback()
        {
            if (InputState.currentUpdateType == InputUpdateType.BeforeRender)
                OnBeforeRender();
            else
                OnUpdate();
        }
#else
        protected void Update()
        {
            OnUpdate();
        }

        protected void LateUpdate()
        {
            OnBeforeRender();
        }
#endif

        protected virtual void OnUpdate()
        {
            if (m_UpdateType == UpdateType.Update ||
                m_UpdateType == UpdateType.UpdateAndBeforeRender) {
                PerformUpdate();
            }
        }

        protected virtual void OnBeforeRender()
        {
            if (m_UpdateType == UpdateType.BeforeRender ||
                m_UpdateType == UpdateType.UpdateAndBeforeRender) {
                PerformUpdate();
            }
        }

        protected virtual void SetLocalTransform(Vector3 newPosition, Quaternion newRotation)
        {
            if (m_TrackingType == TrackingType.RotationAndPosition ||
                m_TrackingType == TrackingType.RotationOnly) {
                transform.localRotation = newRotation;
            }

            if (m_TrackingType == TrackingType.RotationAndPosition ||
                m_TrackingType == TrackingType.PositionOnly) {
                transform.localPosition = newPosition;
            }
        }

        private bool HasStereoCamera()
        {
            var camera = GetComponent<Camera>();
            return camera != null && camera.stereoEnabled;
        }

        protected virtual void PerformUpdate()
        {
            SetLocalTransform(m_CurrentPosition, m_CurrentRotation);
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
