using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Note: This version of the TrackedPoseDriver is modified from the original provided with Unity's XR Interaction
    /// Toolkit.  The modified version uses InputActions defined in MinVR.mainInput.
    /// 
    /// The TrackedPoseDriver component applies the current Pose value of a tracked device to the transform of the GameObject.
    /// TrackedPoseDriver can track multiple types of devices including XR HMDs, controllers, and remotes.
    /// </summary>
    [Serializable]
    [AddComponentMenu("MinVR/Tracked Pose Driver")]
    public class TrackedPoseDriver : MonoBehaviour
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
        VRActionReference m_PositionAction;
        public VRActionReference positionAction {
            get { return m_PositionAction; }
            set {
                m_PositionAction = value;
            }
        }

        [SerializeField]
        VRActionReference m_RotationAction;
        public VRActionReference rotationAction {
            get { return m_RotationAction; }
            set {
                m_RotationAction = value;
            }
        }

        Vector3 m_CurrentPosition = Vector3.zero;
        Quaternion m_CurrentRotation = Quaternion.identity;

        public void OnActionTriggered(InputAction.CallbackContext context)
        {
            if (m_PositionAction.Matches(context)) {
                OnPositionUpdate(context);
            }
            else if (m_RotationAction.Matches(context)) {
                OnRotationUpdate(context);
            }
        }

        void OnPositionUpdate(InputAction.CallbackContext context)
        {
            m_CurrentPosition = context.ReadValue<Vector3>();
        }

        void OnRotationUpdate(InputAction.CallbackContext context)
        {
            m_CurrentRotation = context.ReadValue<Quaternion>();
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
            InputSystem.onAfterUpdate += UpdateCallback;
            MinVR.mainInput.actionTriggered.AddListener(OnActionTriggered);
        }

        void OnDisable()
        {
            InputSystem.onAfterUpdate -= UpdateCallback;
            if (MinVR.mainInput != null) {
                MinVR.mainInput.actionTriggered.RemoveListener(OnActionTriggered);
            }
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

        protected void UpdateCallback()
        {
            if (InputState.currentUpdateType == InputUpdateType.BeforeRender)
                OnBeforeRender();
            else
                OnUpdate();
        }

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
    }
}

