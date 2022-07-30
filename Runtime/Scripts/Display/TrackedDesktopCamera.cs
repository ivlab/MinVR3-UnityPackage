using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3 {

    /// <summary>
    /// Makes a regular desktop camera move about in respone to VREvents so you can control it, for example,
    /// from a HMD simulator.
    /// </summary>
    [AddComponentMenu("MinVR/Display/Tracked Desktop Camera")]
    public class TrackedDesktopCamera : MonoBehaviour, IVREventListener {

        public VREventPrototypeVector3 positionEvent {
            get { return m_PositionEvent; }
            set {
                m_PositionEvent = value;
            }
        }

        public VREventPrototypeQuaternion rotationEvent {
            get { return m_RotationEvent; }
            set { m_RotationEvent = value; }
        }

        public Camera trackedCamera {
            get { return m_Camera; }
            set { m_Camera = value; }
        }


        void Start()
        {
            if (m_Camera == null) {
                m_Camera = GetComponentInChildren<Camera>();
                if (m_Camera == null) {
                    m_Camera = Camera.main;
                }
            }
        }


        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_PositionEvent)) {
                m_Camera.transform.position = vrEvent.GetData<Vector3>();
            } else if (vrEvent.Matches(m_RotationEvent)) {
                m_Camera.transform.rotation = vrEvent.GetData<Quaternion>();
            }
        }


        void OnEnable() {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        void OnDisable()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }


        public void StartListening()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        public void StopListening()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }


        [Tooltip("Name of the VREvent that provides positional updates.")]
        [SerializeField] private VREventPrototypeVector3 m_PositionEvent;
        [Tooltip("Name of the VREvent that provides rotational updates.")]
        [SerializeField] private VREventPrototypeQuaternion m_RotationEvent;
        [Tooltip("The camera to apply the tracking updates to.  Defaults to Main Camera.")]
        [SerializeField] private Camera m_Camera;
    }
}
