using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace IVLab.MinVR3
{
    [CustomEditor(typeof(VREventConnectionReceiver))]
    public class VREventConnectionReceiverEditor : Editor
    {
        public void OnEnable()
        {
            m_EventPrototypesProp = serializedObject.FindProperty("m_EventPrototypes");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            VREventConnectionReceiver script = (VREventConnectionReceiver) target;

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
                    "attached to the same object!\nPlease attach one of the following scripts:\n" +
                    typeListStr, MessageType.Error);
                return;
            }


            EditorGUILayout.HelpBox("The list below should define an appropriate VREventPrototpye for each VREvent " +
                    "you expect to receive over the connection.  Note: If a new prototype would duplicate one " +
                    "already defined elsewhere in the scene, it is not technically necessary to redefine it here; " +
                    "however, there is no harm in redefining it (as long as the data types match), and this " +
                    "may sometimes be useful to make it extra clear what events are expected via the connection. " +
                    "If a VREvent with no pre-defined prototype is received, it will still be correctly received " +
                    "and processed by the event manager at runtime.  However, a prototype is needed if you want " +
                    "to access the event in the Unity Editor, for example, if you want to be able to select it " +
                    "from a drop-down menu in a VREventListener.",
                    MessageType.None);

            string msg = $"Connection defines ({m_EventPrototypesProp.arraySize}) new VREventPrototpyes"; 


            m_ShowFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowFoldout, msg);

            if (m_ShowFoldout) {

                if (m_EventPrototypesProp.arraySize > 0) {
                    EditorGUILayout.LabelField("Event Name", "Event Data Type");
                }
                List<int> idxToDelete = new List<int>();

                for (int evtNum = 0; evtNum < m_EventPrototypesProp.arraySize; evtNum++) {
                    // Event name and event payload type
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.PropertyField(m_EventPrototypesProp.GetArrayElementAtIndex(evtNum));

                    if (GUILayout.Button("-", GUILayout.Width(EditorGUIUtility.singleLineHeight))) {
                        idxToDelete.Add(evtNum);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                foreach (int idx in idxToDelete) {
                    script.eventPrototypes.RemoveAt(idx);
                }


                if (GUILayout.Button("+")) {
                    string eventName = VREventManager.GetUniqueEventPrototypeName("MyDevice/MyEvent");
                    VREventPrototypeAny newProto = VREventPrototypeAny.Create(eventName);
                    newProto.SetDefineNewPrototypeInEditor(true);
                    if (script.eventPrototypes == null)
                    {
                        script.eventPrototypes = new List<VREventPrototypeAny>();
                    }
                    script.eventPrototypes.Add(newProto);
                }

            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            

            serializedObject.ApplyModifiedProperties();
        }


        bool m_ShowFoldout = true;
        SerializedProperty m_EventPrototypesProp;
    }
}