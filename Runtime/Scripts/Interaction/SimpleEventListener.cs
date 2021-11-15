using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    [AddComponentMenu("MinVR/Interaction/Simple Event Listener")]
    public class SimpleEventListener : MonoBehaviour
    {
        public VREventCallbackAny onVREvent {
            get => m_OnVREvent;
            set => m_OnVREvent = value;
        }

        private void OnEnable()
        {
            m_OnVREvent.StartListening();
        }

        private void OnDisable()
        {
            m_OnVREvent.StopListening();
        }

        [SerializeField] VREventCallbackAny m_OnVREvent;
    }

} // end namespace
