using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace IVLab.MinVR3
{
    [CustomEditor(typeof(VRConfigManager))]
    public class VRConfigManagerEditor : Editor
    {

        public void OnEnable()
        {
            m_StartupVRConfigProp = serializedObject.FindProperty("m_StartupVRConfig");
            m_DefaultConfigFilesProp = serializedObject.FindProperty("m_DefaultConfigFiles");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            VRConfig[] availableConfigs = Resources.FindObjectsOfTypeAll<VRConfig>() as VRConfig[];


            string[] displayNames = new string[availableConfigs.Length];
            int selected = -1;
            for (int i = 0; i < availableConfigs.Length; i++) {
                displayNames[i] = availableConfigs[i].name;
                if (m_StartupVRConfigProp.objectReferenceValue == availableConfigs[i]) {
                    selected = i;
                }
            }

            GUIContent label = new GUIContent("Startup VRConfig", "On application startup, this VRConfig object is enabled and all other VRConfig objects are disabled.  A default startup config should be set here, but this can be overridden using command line arguments so that the same execuable can run in multiple different vrconfig modes.");

            EditorGUI.BeginChangeCheck();
            selected = EditorGUILayout.Popup(label, selected, displayNames);
            if (EditorGUI.EndChangeCheck()) {
                if (selected >= 0) {
                    Debug.Log("Selected: " + availableConfigs[selected].name);
                    m_StartupVRConfigProp.objectReferenceValue = availableConfigs[selected];


                    foreach (var cfg in availableConfigs) {
                        Debug.Log(cfg.gameObject.name + " " + (cfg == availableConfigs[selected]) + " " + availableConfigs[selected].name);
                        // dfk 2/19/25: this check seems to not always work--i'm seeing it fail with
                        // VRConfig_VRSimulator and VRConfig_Quest where VRConfig_Quest is a prefab.  Perhaps
                        // the problem is that one is a prefab. Regardless, for command-line selection of
                        // VRConfigs to work, they must have a unique name.  So, it seems ok to change the
                        // check to use the name instead, and that seems to work fine.
                        //cfg.gameObject.SetActive(cfg == availableConfigs[selected]);

                        cfg.gameObject.SetActive(cfg.name == availableConfigs[selected].name);
                    }

                    VRConfigMask[] objectsWithConfigMask = Resources.FindObjectsOfTypeAll<VRConfigMask>();
                    foreach (var cfgMask in objectsWithConfigMask) {
                        cfgMask.gameObject.SetActive(cfgMask.IsEnabledForConfig(availableConfigs[selected]));
                    }
                }
            }

            EditorGUILayout.PropertyField(m_DefaultConfigFilesProp);

            if (GUILayout.Button("Go to Startup VRConfig GameObject"))
            {
                Selection.activeGameObject = availableConfigs[selected].gameObject;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty m_StartupVRConfigProp;
        private SerializedProperty m_DefaultConfigFilesProp;

    }

} // namespace
