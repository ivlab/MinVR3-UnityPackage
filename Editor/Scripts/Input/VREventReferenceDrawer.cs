using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace IVLab.MinVR3
{
    [CustomPropertyDrawer(typeof(VREventReference))]
    public class VREventReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Show a dropdown list of possible events; the event selected defines the event
            // data type, which in turn tells us which callback to display.
            SerializedProperty nameProp = property.FindPropertyRelative("m_EventName");
            SerializedProperty dataTypeProp = property.FindPropertyRelative("m_EventDataType");
            SerializedProperty lockedProp = property.FindPropertyRelative("m_LockDataType");


            // Get all possible events from all IVREventProducers in the scene
            Dictionary<string, string> expectedEvents = VREventManager.GetAllEventNamesAndTypes();
            string[] eventNames = new string[expectedEvents.Count];
            string[] dataTypes = new string[expectedEvents.Count];
            GUIContent[] displayNames = new GUIContent[expectedEvents.Count];
            int i = 0;
            int selected = -1;
            foreach (string eName in expectedEvents.Keys) {
                // if the data type is not locked, then add every possible event to the list
                // if it is locked, then only add events that match the datatype
                if ((!lockedProp.boolValue) || (dataTypeProp.stringValue == expectedEvents[eName])) {
                    eventNames[i] = eName;
                    dataTypes[i] = expectedEvents[eName];
                    displayNames[i] = new GUIContent(VREventManager.EventDisplayName(eventNames[i], dataTypes[i]));
                    if (eName == nameProp.stringValue) {
                        selected = i;
                    }
                    i++;
                }
            }
            if (i != expectedEvents.Count) {
                Array.Resize(ref eventNames, i);
                Array.Resize(ref dataTypes, i);
                Array.Resize(ref displayNames, i);
            }

            m_ShowHelp = displayNames.Length == 0;
            Rect helpRect = new Rect(position.x + 15, position.y, position.width - 15, 4.0f * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            if (m_ShowHelp) {
                EditorGUI.HelpBox(helpRect, "The dropdown list of available VREvents is empty.  You probably need to add an IVREventProducer to the scene.  Look under Add Component/MinVR/Input/* for virtual and physical input devices that produce VREvents.", MessageType.Warning);
            }

            // Display the list as a dropdown; selecting an item here sets both the eventname and
            // the eventdatatype
            float y = position.y;
            if (m_ShowHelp) {
                y = helpRect.yMax;
            }
            Rect listRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.BeginChangeCheck();
            selected = EditorGUI.Popup(listRect, label, selected, displayNames);
            if (EditorGUI.EndChangeCheck()) {
                if (selected != -1) {
                    nameProp.stringValue = eventNames[selected];
                    dataTypeProp.stringValue = dataTypes[selected];
                }
            }

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            if (m_ShowHelp) {
                height += 4.0f * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        private bool m_ShowHelp = false;
    }

} // namespace
