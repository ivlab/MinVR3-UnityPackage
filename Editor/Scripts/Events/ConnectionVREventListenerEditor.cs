using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace IVLab.MinVR3
{
    [CustomEditor(typeof(ConnectionVREventListener))]
    public class ConnectionVREventListenerEditor : Editor
    {
        int _typeChoice = 0;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ConnectionVREventListener script = (ConnectionVREventListener) target;
            VREventPrototypeAny any = new VREventPrototypeAny();

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

                EditorGUILayout.HelpBox(
                    "Error: the ConnectionVREventProducer must have an IVREventConnection attached to the same object!\n" +
                    "Please attach one of the following scripts:\n" + typeListStr,
                    MessageType.Error
                );
                return;
            }

            EditorGUILayout.HelpBox(
                "You must identify what types of VREvents you expect to receive along this connection. " +
                "For example, if your connection is a web browser, you may receive a Vector2 event called " +
                "'Cursor2D/Position', which would be defined in the browser JavaScript. Check out the NetworkedEvents sample for more info.",
                MessageType.None
            );

            int numExpectedEvents = Mathf.Min(script.EventNames.Count, script.EventTypes.Count);
            if (numExpectedEvents == 0)
            {
                EditorGUILayout.HelpBox("No events defined: add events you expect to receive from this connection.", MessageType.Warning);
                EditorGUILayout.HelpBox("Add a new event to forward by pressing the + button:", MessageType.Info);
            }
            else
            {
                EditorGUILayout.BeginFoldoutHeaderGroup(true, "VREvents produced by this connection");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Event Name");
                EditorGUILayout.LabelField("Event Type");
                EditorGUILayout.EndHorizontal();
            }

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