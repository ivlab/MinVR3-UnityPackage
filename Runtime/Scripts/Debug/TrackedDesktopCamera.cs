using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3 {

    /// <summary>
    /// Makes a regular desktop camera move about in respone to VREvents so you can control it, for example,
    /// from a HMD simulator.
    /// </summary>
    [AddComponentMenu("MinVR/Debug/Tracked Desktop Camera")]
    public class TrackedDesktopCamera : MonoBehaviour, IVREventReceiver {

        [Tooltip("Name of the VREvent that provides positional updates.")]
        public VREventPrototype<Vector3> m_PositionEvent = new VREventPrototype<Vector3>("");

        [Tooltip("Name of the VREvent that provides rotational updates.")]
        public VREventPrototype<Quaternion> m_RotationEvent = new VREventPrototype<Quaternion>("");
        
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
            if (m_PositionEvent.Matches(vrEvent)) {
                m_Camera.transform.position = (vrEvent as VREvent<Vector3>).data;
            } else if (m_RotationEvent.Matches(vrEvent)) {
                m_Camera.transform.rotation = (vrEvent as VREvent<Quaternion>).data;
            }
        }
    }
}
