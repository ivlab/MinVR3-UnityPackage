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

        [Tooltip("Name of the VREvent that provides positional updates.")]
        public VREventPrototypeVector3 m_PositionEvent;

        [Tooltip("Name of the VREvent that provides rotational updates.")]
        public VREventPrototypeQuaternion m_RotationEvent;
        
        [Tooltip("The camera to apply the tracking updates to.  Defaults to Main Camera.")]
        public Camera m_Camera;

        void OnEnable() {
            if (m_Camera == null) {
                m_Camera = Camera.main;
            }
            VREngine.instance.eventManager.AddEventReceiver(this);
        }

        void OnDisable()
        {
            VREngine.instance?.eventManager?.RemoveEventReceiver(this);
        }

        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_PositionEvent)) {
                m_Camera.transform.position = (vrEvent as VREvent<Vector3>).data;
            } else if (vrEvent.Matches(m_RotationEvent)) {
                m_Camera.transform.rotation = (vrEvent as VREvent<Quaternion>).data;
            }
        }

        public bool IsListening()
        {
            return m_Listening;
        }

        public void StartListening()
        {
            VREngine.instance.eventManager.AddEventReceiver(this);
            m_Listening = true;
        }

        public void StopListening()
        {
            VREngine.instance?.eventManager?.RemoveEventReceiver(this);
            m_Listening = false;
        }

        private bool m_Listening = false;
    }
}
