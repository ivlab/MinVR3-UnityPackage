using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3 {

    public class MinVR
    {
        /// <summary>
        /// Cached access to the first enabled VRInput component found in the scene.
        /// </summary>
        static public VRInput mainInput {
            get {
                // if main is already set and it is still enabled, just return it
                if ((m_MainInput != null) && (m_MainInput.enabled)) {
                    return m_MainInput;
                }
                // otherwise, search for it and return the first match found
                m_MainInput = null;
                VRInput[] allVRInputs = GameObject.FindObjectsOfType<VRInput>();
                foreach (VRInput vrInput in allVRInputs) {
                    if (vrInput.enabled) {
                        return vrInput;
                    }
                }
                return null;
            }
            set {
                m_MainInput = value;
            }
        }
        static private VRInput m_MainInput;
        
    }

} // namespace
