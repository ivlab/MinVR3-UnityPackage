using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3 {

    /// <summary>
    /// Makes a regular desktop camera move about in respone to VREvents so you can control it, for example,
    /// from a HMD simulator.
    /// </summary>
    [AddComponentMenu("MinVR/Interaction/Tracked Desktop Camera")]
    public class TrackedDesktopCamera : MonoBehaviour, IVREventReceiver {

        [Tooltip("Name of the VREvent that provides positional updates.")]
        public VREventReference m_PositionEvent = new VREventReference("", "Vector3", true);

        [Tooltip("Name of the VREvent that provides rotational updates.")]
        public VREventReference m_RotationEvent = new VREventReference("", "Quaternion", true);

        [Tooltip("The camera to apply the tracking updates to.  Defaults to Main Camera.")]
        public Camera m_Camera;

        void OnEnable() {
            if (m_Camera == null) {
                m_Camera = Camera.main;
            }
            VREngine.main.eventManager.AddEventReceiver(this);
        }

        void OnDisable()
        {
            VREngine.main.eventManager.RemoveEventReceiver(this);
        }

        public void OnVREvent(VREventInstance vrEvent)
        {
            if (vrEvent.name == m_PositionEvent.name) {
                m_Camera.transform.position = (vrEvent as VREventInstance<Vector3>).data;
            } else if (vrEvent.name == m_RotationEvent.name) {
                m_Camera.transform.rotation = (vrEvent as VREventInstance<Quaternion>).data;
            }
        }
    }
}
