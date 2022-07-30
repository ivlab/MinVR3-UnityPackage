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
                    //Debug.Log("Selected: " + availableConfigs[selected].name);
                    m_StartupVRConfigProp.objectReferenceValue = availableConfigs[selected];


                    foreach (var cfg in availableConfigs) {
                        cfg.gameObject.SetActive(cfg == availableConfigs[selected]);
                    }

                    VRConfigMask[] objectsWithConfigMask = Resources.FindObjectsOfTypeAll<VRConfigMask>();
                    foreach (var cfgMask in objectsWithConfigMask) {
                        cfgMask.gameObject.SetActive(cfgMask.IsEnabledForConfig(availableConfigs[selected]));
                    }
                }
            }

            EditorGUILayout.PropertyField(m_DefaultConfigFilesProp);

            serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty m_StartupVRConfigProp;
        private SerializedProperty m_DefaultConfigFilesProp;

    }

} // namespace
