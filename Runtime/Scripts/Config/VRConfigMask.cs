using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    public class VRConfigMask : MonoBehaviour
    {
        public bool IsEnabledForConfig(VRConfig config)
        {
            if (config == null) return false;
            return m_EnabledConfigsList.Contains(config);
        }


        public bool IsEnabledForConfig(string configName)
        {
            if (configName == "") return false;
            foreach (var c in m_EnabledConfigsList) {
                if (c.name == configName) {
                    return true;
                }
            }
            return false;
        }


        private void Reset()
        {
            m_EnabledConfigsList = new List<VRConfig>();
        }

        [SerializeField] private List<VRConfig> m_EnabledConfigsList;
    }

} // end namespace
