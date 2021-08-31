using UnityEngine;
using System.Linq;

namespace IVLab.MinVR3 {

    [RequireComponent(typeof(VREventManager))]
    [AddComponentMenu("")] // don't list in the component menu
    [DefaultExecutionOrder(-500)] // make sure this script runs before others
    public class VREngine : OnDemandMonoBehaviourSingleton<VREngine>
    {
        public VREventManager eventManager {
            get {
                if (m_EventManager == null) {
                    m_EventManager = GetComponent<VREventManager>();
                }
                return m_EventManager;
            }
        }
        
        // cached access to other components attached to the gameobject
        private VREventManager m_EventManager;
    }

} // namespace
