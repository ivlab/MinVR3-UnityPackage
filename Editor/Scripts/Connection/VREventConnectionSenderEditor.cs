using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace IVLab.MinVR3
{
    [CustomEditor(typeof(VREventConnectionSender))]
    public class VREventConnectionSenderEditor : Editor
    {
        public void OnEnable()
        {
            m_SendListProp = serializedObject.FindProperty("m_SendList");
            m_NoSendListProp = serializedObject.FindProperty("m_NoSendList");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            VREventConnectionSender script = (VREventConnectionSender) target;

            // Check to see if the GameObject has the proper script attached
            IVREventConnection conn;
            if (!script.TryGetComponent<IVREventConnection>(out conn))
            {
                var assembly = Assembly.GetAssembly(typeof(IVREventConnection));
                var vrEventConnectionTypes = assembly
                    .GetTypes()
                    .Where(t => t != typeof(IVREventConnection))
                    .Where(t => typeof(IVREventConnection).IsAssignableFrom(t))
                    .Select(t => t.Name);
                var typeListStr = "\n  - " + string.Join("\n  - ", vrEventConnectionTypes);

                EditorGUILayout.HelpBox("Error: the ConnectionVREventProducer must have an IVREventConnection " +
                    "attached to the same object. Please attach one of the following scripts:\n" + typeListStr,
                    MessageType.Error);
                return;
            }


            string sendMsg = "Send List = Send ALL VREvents";
            if (m_SendListProp.arraySize > 0) {
                sendMsg = $"Send List = Send ({m_SendListProp.arraySize}) VREvents";
            }
            m_ShowFoldoutSend = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowFoldoutSend, sendMsg);

            if (m_ShowFoldoutSend) {
                List<int> idxToDelete = new List<int>();

                for (int evtNum = 0; evtNum < m_SendListProp.arraySize; evtNum++) {
                    // Event name and event payload type
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(m_SendListProp.GetArrayElementAtIndex(evtNum));

                    if (GUILayout.Button("-", GUILayout.Width(EditorGUIUtility.singleLineHeight))) {
                        idxToDelete.Add(evtNum);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                foreach (int idx in idxToDelete) {
                    script.sendList.RemoveAt(idx);
                }

                if (GUILayout.Button("+")) {
                    if (script.sendList == null)
                    {
                        script.sendList = new List<VREventPrototypeAny>();
                    }
                    script.sendList.Add(VREventPrototypeAny.Create(""));
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();


            string noSendMsg = $"No Send List = Block ({m_NoSendListProp.arraySize}) VREvents";
            m_ShowFoldoutNoSend = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowFoldoutNoSend, noSendMsg);

            if (m_ShowFoldoutNoSend) {
                List<int> idxToDelete = new List<int>();

                for (int evtNum = 0; evtNum < m_NoSendListProp.arraySize; evtNum++) {
                    // Event name and event payload type
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(m_NoSendListProp.GetArrayElementAtIndex(evtNum));

                    if (GUILayout.Button("-", GUILayout.Width(EditorGUIUtility.singleLineHeight))) {
                        idxToDelete.Add(evtNum);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                foreach (int idx in idxToDelete) {
                    script.noSendList.RemoveAt(idx);
                }

                if (GUILayout.Button("+")) {
                    script.noSendList.Add(VREventPrototypeAny.Create(""));
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();


            serializedObject.ApplyModifiedProperties();
        }


        bool m_ShowFoldoutSend = true;
        bool m_ShowFoldoutNoSend = true;
        SerializedProperty m_SendListProp;
        SerializedProperty m_NoSendListProp;

    }

}