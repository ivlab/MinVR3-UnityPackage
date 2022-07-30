using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{

    [CustomEditor(typeof(VRConfigMask))]
    public class VRConfigMaskEditor : Editor
    {

        void OnEnable()
        {
            m_EnabledConfigsListProp = serializedObject.FindProperty("m_EnabledConfigsList");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            VRConfig[] availableConfigs = Resources.FindObjectsOfTypeAll<VRConfig>() as VRConfig[];

            string[] displayNames = new string[availableConfigs.Length];
            bool[] enabled = new bool[availableConfigs.Length];
            for (int i = 0; i < availableConfigs.Length; i++) {
                displayNames[i] = availableConfigs[i].name;
                enabled[i] = false;
                int j = 0;
                while ((!enabled[i]) && (j < m_EnabledConfigsListProp.arraySize)) {
                    SerializedProperty configObjProp = m_EnabledConfigsListProp.GetArrayElementAtIndex(j);
                    if (availableConfigs[i] == configObjProp.objectReferenceValue) {
                        enabled[i] = true;
                    }
                    j++;
                }
            }

            EditorGUILayout.HelpBox("Check the VRConfigs for which this GameObject should be active.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < displayNames.Length; i++) {
                enabled[i] = EditorGUILayout.Toggle(new GUIContent(displayNames[i]), enabled[i]);
            }
            if (EditorGUI.EndChangeCheck()) {
                // easiest approach to updating this is probably to just clear and rebuild the array
                m_EnabledConfigsListProp.ClearArray();

                for (int i = 0; i < availableConfigs.Length; i++) {
                    if (enabled[i]) {
                        m_EnabledConfigsListProp.InsertArrayElementAtIndex(m_EnabledConfigsListProp.arraySize);
                        SerializedProperty newConfigObj = m_EnabledConfigsListProp.GetArrayElementAtIndex(m_EnabledConfigsListProp.arraySize - 1);
                        newConfigObj.objectReferenceValue = availableConfigs[i];
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty m_EnabledConfigsListProp;
    }

} // namespace
