using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    /// <summary>
    /// Listens for a single specific VREvent and calls the specified callback function(s) whenever the event is received.
    /// </summary>
    [AddComponentMenu("MinVR/Interaction/VREvent Listener")]
    public class VREventListener : MonoBehaviour, IVREventReceiver
    {
        private void Reset()
        {
            m_Callback = new VREventCallback();
        }

        private void OnEnable()
        {
            VREngine.instance.eventManager.AddEventReceiver(this);
        }

        private void OnDisable()
        {
            VREngine.instance.eventManager?.RemoveEventReceiver(this);
        }

        public void OnVREvent(VREventInstance e)
        {
            if (e.name == m_Callback.listeningFor.name) {
                m_Callback.Invoke(e);
            }
        }

        [SerializeField] private VREventCallback m_Callback;
    }

} // namespace
