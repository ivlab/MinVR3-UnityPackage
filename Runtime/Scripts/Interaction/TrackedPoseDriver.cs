using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Events;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Note: This version of the TrackedPoseDriver is modified from the original provided with Unity's XR Interaction
    /// Toolkit.  The modified version uses listens for VREvents.
    /// 
    /// The TrackedPoseDriver component applies the current Pose value of a tracked device to the transform of the GameObject.
    /// TrackedPoseDriver can track multiple types of devices including XR HMDs, controllers, and remotes.
    /// </summary>
    [AddComponentMenu("MinVR/Interaction/Tracked Pose Driver")]
    public class TrackedPoseDriver : MonoBehaviour, IVREventReceiver
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
        VREventPrototype<Vector3> m_PositionEvent;
        public VREventPrototype<Vector3> positionEvent {
            get { return m_PositionEvent; }
            set {
                m_PositionEvent = value;
            }
        }

        [SerializeField]
        VREventPrototype<Quaternion> m_RotationEvent;
        public VREventPrototype<Quaternion> rotationEvent {
            get { return m_RotationEvent; }
            set {
                m_RotationEvent = value;
            }
        }

        Vector3 m_CurrentPosition = Vector3.zero;
        Quaternion m_CurrentRotation = Quaternion.identity;


        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_PositionEvent)) {
                VREvent<Vector3> posUpdateEvent = vrEvent as VREvent<Vector3>;
                m_CurrentPosition = posUpdateEvent.data;
            }
            if (vrEvent.Matches(m_RotationEvent)) {
                VREvent<Quaternion> rotUpdateEvent = vrEvent as VREvent<Quaternion>;
                m_CurrentRotation = rotUpdateEvent.data;
            }
        }


        private void Reset()
        {
            m_PositionEvent = new VREventPrototype<Vector3>();
            m_RotationEvent = new VREventPrototype<Quaternion>();
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
            VREngine.instance.eventManager.AddEventReceiver(this);
        }

        void OnDisable()
        {
            InputSystem.onAfterUpdate -= UpdateCallback;
            VREngine.instance?.eventManager?.RemoveEventReceiver(this);
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

