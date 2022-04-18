using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace IVLab.MinVR3
{
    [CustomEditor(typeof(ConnectionVREventProducer))]
    public class ConnectionVREventProducerEditor : Editor
    {
        int _typeChoice = 0;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ConnectionVREventProducer script = (ConnectionVREventProducer) target;
            VREventPrototypeAny any = new VREventPrototypeAny();

            EditorGUILayout.BeginFoldoutHeaderGroup(true, "VREvents produced by this connection");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Event Name");
            EditorGUILayout.LabelField("Event Type");
            EditorGUILayout.EndHorizontal();

            int numExpectedEvents = Mathf.Min(script.EventNames.Count, script.EventTypes.Count);
            var eventDataTypes = any.AllEventPrototypes.Keys.ToList();
            List<int> idxToDelete = new List<int>();

            for (int evtNum = 0; evtNum < numExpectedEvents; evtNum++)
            {
                // Event name and event payload type
                EditorGUILayout.BeginHorizontal();

                string newEventName = EditorGUILayout.TextField(script.EventNames[evtNum]);
                script.EventNames[evtNum] = newEventName;

                int typeIndex = eventDataTypes.FindIndex(e => e == script.EventTypes[evtNum]);
                typeIndex = Mathf.Clamp(typeIndex, 0, eventDataTypes.Count);

                int newTypeIndex = EditorGUILayout.Popup(typeIndex, eventDataTypes.ToArray());
                script.EventTypes[evtNum] = eventDataTypes[newTypeIndex];

                if (GUILayout.Button("-"))
                {
                    idxToDelete.Add(evtNum);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+"))
            {
                script.EventNames.Add("Event/Name/Here");
                script.EventTypes.Add(null);
            }

            foreach (int idx in idxToDelete)
            {
                script.EventNames.RemoveAt(idx);
                script.EventTypes.RemoveAt(idx);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            serializedObject.ApplyModifiedProperties();
        }
    }
}