using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    /// <summary>
    /// The VRConfigManager should not be created directly.  Instead, use the one attached to VREngine.
    /// </summary>
    [DisallowMultipleComponent]
    public class VRConfigManager : MonoBehaviour
    {
        public string startupConfigName {
            get {
                if (m_StartupVRConfig != null) {
                    return m_StartupVRConfig.name;
                }
                return "";
            }
        }

        public VRConfig startupConfig {
            get { return m_StartupVRConfig; }
            set { m_StartupVRConfig = value; }
        }


        /// <summary>
        /// Enables and disables Game Objects based on the VRConfig that is starting up.  This impacts GameObjects with
        /// VRConfig or VRConfigMask components attached.  All GOs with VRConfig components are disabled EXCEPT for
        /// the one that has the startup VRConfig.  For GOs that have VRConfigMask components attached, they are
        /// enabled if the mask includes a checkmark next to the startup VRConfig and disabled otherwise.
        /// </summary>
        public void EnableStartupVRConfigAndDisableOthers()
        {
            VRConfig[] availableConfigs = GetAvailableConfigs();

            string msg = $"VRConfigManager: {availableConfigs.Length} available VRConfigs:\n";
            foreach (var cfg in availableConfigs) {
                string startup = cfg == m_StartupVRConfig ? "*" : " ";
                msg += $"  [{startup}] {cfg.gameObject.name}\n";
            }
            Debug.Log(msg);

            if (m_StartupVRConfig == null) {
                if (availableConfigs.Length == 1) {
                    // only one config in the scene, set it as the startup config
                    m_StartupVRConfig = availableConfigs[0];
                } else if (availableConfigs.Length > 1) {
                    // use the first one, remind user to set it
                    m_StartupVRConfig = availableConfigs[0];
                    Debug.LogWarning("VRConfigs are available in the scene, but none of them is set as the startup config.  Please go to VREngine > VRConfigManager and select the VRConfig to start.");
                } else {
                    Debug.LogWarning("The scene does not contain any VRConfigs.");
                }
            }

            if (m_StartupVRConfig != null) {
                Debug.Log($"MinVR Starting with VRConfig: {m_StartupVRConfig.name}");
            }

            foreach (var cfg in availableConfigs) {
                cfg.gameObject.SetActive(cfg == m_StartupVRConfig);
            }

            VRConfigMask[] objectsWithConfigMask = Resources.FindObjectsOfTypeAll<VRConfigMask>();
            foreach (var cfgMask in objectsWithConfigMask) {
                cfgMask.gameObject.SetActive(cfgMask.IsEnabledForConfig(m_StartupVRConfig));
            }            
        }


        public void ParseConfigFiles()
        {
            if (m_DefaultConfigFiles != null) {
                foreach (var cf in m_DefaultConfigFiles) {
                    ConfigVal.ParseConfigFile(cf);
                }
            }

            if (m_StartupVRConfig != null) {
                m_StartupVRConfig.ParseConfigFiles();
            }
        }

        public VRConfig[] GetAvailableConfigs()
        {
            return Resources.FindObjectsOfTypeAll<VRConfig>();
        }

        public VRConfig GetConfigByName(string configName)
        {
            VRConfig[] availableConfigs = GetAvailableConfigs();
            foreach (var cfg in availableConfigs) {
                if (cfg.name == configName) {
                    return cfg;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        public void AddConfigFile(TextAsset configFile)
        {
            if (m_DefaultConfigFiles == null) {
                m_DefaultConfigFiles = new List<TextAsset>();
            }
            m_DefaultConfigFiles.Add(configFile);
        }
#endif

        private void Reset()
        {
            m_StartupVRConfig = null;
        }

        [SerializeField] private VRConfig m_StartupVRConfig;

        [Tooltip("Values in these files act as defaults that can be overwritten in config files included in teh Startup VRConfig object.")]
        [SerializeField] private List<TextAsset> m_DefaultConfigFiles;


    }

} // end namespace
