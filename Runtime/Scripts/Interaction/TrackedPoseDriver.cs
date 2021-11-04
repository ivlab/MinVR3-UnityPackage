
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
    [AddComponentMenu("MinVR/Interaction/Tracked Pose Driver")]
    public class TrackedPoseDriver : MonoBehaviour, IVREventListener
    {
        public enum TrackingType
        {
            RotationAndPosition,
            RotationOnly,
            PositionOnly
        }


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

        Vector3 m_CurrentPosition = Vector3.zero;
        Quaternion m_CurrentRotation = Quaternion.identity;


        [SerializeField]
        Quaternion m_CalibrationRotation;
        public Quaternion calibrationRotation {
            get { return m_CalibrationRotation; }
            set { m_CalibrationRotation = value; }
        }

        [SerializeField]
        Vector3 m_CalibrationTranslation;
        public Vector3 calibrationTranslation {
            get { return m_CalibrationTranslation; }
            set { m_CalibrationTranslation = value; }
        }

        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_PositionEvent)) {
                VREvent<Vector3> posUpdateEvent = vrEvent as VREvent<Vector3>;
                m_CurrentPosition = posUpdateEvent.data;// + m_CalibrationTranslation;
            }
            if (vrEvent.Matches(m_RotationEvent)) {
                VREvent<Quaternion> rotUpdateEvent = vrEvent as VREvent<Quaternion>;
                m_CurrentRotation = rotUpdateEvent.data;// * m_CalibrationRotation;
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
            VREngine.instance.eventManager.AddEventReceiver(this);
        }

        public void StopListening()
        {
            VREngine.instance?.eventManager?.RemoveEventReceiver(this);
        }
    }

} // end namespace
