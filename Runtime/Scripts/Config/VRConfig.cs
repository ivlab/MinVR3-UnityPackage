using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Attach to game objects you wish to act as VRConfig objects.
    /// </summary>
    public class VRConfig : MonoBehaviour
    {
        [Tooltip("MinVR Config Files that are specific to this VRConfig.  These are only parsed when this VRConfig is active, and the values override any defaults that appear in the config files loaded from the VRConfigManager attached to the UnityEngine Game Object.")]
        [SerializeField] private List<TextAsset> m_ConfigFiles;

        public void ParseConfigFiles()
        {
            foreach (var cf in m_ConfigFiles) {
                ConfigVal.ParseConfigFile(cf);
            }
        }

#if UNITY_EDITOR
        public void AddConfigFile(TextAsset configFile)
        {
            if (m_ConfigFiles == null) {
                m_ConfigFiles = new List<TextAsset>();
            }
            m_ConfigFiles.Add(configFile);
        }
#endif
    }

}
