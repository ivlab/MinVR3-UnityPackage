using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace IVLab.MinVR3
{
    
    [CustomPropertyDrawer(typeof(VREventPrototype))]
    public class VREventPrototypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty eventNameProp = property.FindPropertyRelative("m_EventName");
            SerializedProperty dataTypeNameProp = property.FindPropertyRelative("m_DataTypeName");

            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.LabelField(labelRect, label);


            Rect eventNameRect = new Rect(position.x + 15, labelRect.yMax, position.width - 15, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

            // Display the event name property as a regular text box
            //EditorGUI.PropertyField(eventNameRect, eventNameProp);

            // Display the event name property as a dropdown list
            // Get all possible events that match the current data type from all IVREventProducers in the scene
            List<IVREventPrototype> expectedEvents = VREventManager.GetMatchingEventPrototypes(dataTypeNameProp.stringValue);

            Rect helpRect = new Rect(position.x + 15, labelRect.yMax, position.width - 15, 4.0f * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            if (expectedEvents.Count == 0) {
                m_ShowHelp = true;
                EditorGUI.HelpBox(helpRect, "No matching VREventPrototpyes.  You may need to add an IVREventProducer " +
                    "to the scene.  Look under Add Component/MinVR/Input/* for virtual and physical input devices that " +
                    "produce VREvents.", MessageType.Warning);
                eventNameRect.y += helpRect.height;
            }

            string[] eventNames = new string[expectedEvents.Count + 1]; ;
            GUIContent[] displayNames = new GUIContent[expectedEvents.Count + 1];
            eventNames[0] = "";
            displayNames[0] = new GUIContent("(none)");

            int i = 1;
            int selected = 0;
            foreach (var e in expectedEvents) {
                eventNames[i] = e.GetEventName();
                displayNames[i] = new GUIContent(e.GetDisplayName());
                if (e.GetEventName() == eventNameProp.stringValue) {
                    selected = i;
                }
                i++;
            }

            EditorGUI.BeginChangeCheck();
            selected = EditorGUI.Popup(eventNameRect, selected, displayNames);
            if (EditorGUI.EndChangeCheck()) {
                if (selected >= 0) {
                    eventNameProp.stringValue = eventNames[selected];
                }
            }
            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 2.0f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            if (m_ShowHelp) {
                height += 4.0f * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }

        private bool m_ShowHelp = false;
    }


    [CustomPropertyDrawer(typeof(VREventPrototype<>))]
    public class VREventPrototypeTemplatedDrawer : VREventPrototypeDrawer
    {
    }


    [CustomPropertyDrawer(typeof(VREventPrototypeAny))]
    public class VREventPrototypeAnyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty eventNameProp = property.FindPropertyRelative("m_EventName");
            SerializedProperty dataTypeNameProp = property.FindPropertyRelative("m_DataTypeName");

            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.LabelField(labelRect, label);


            Rect eventNameRect = new Rect(position.x + 15, labelRect.yMax, position.width - 15, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

            // Display the event name property as a regular text box
            //EditorGUI.PropertyField(eventNameRect, eventNameProp);

            // Display the event name property as a dropdown list
            // Get all possible events that match the current data type from all IVREventProducers in the scene
            List<IVREventPrototype> expectedEvents = VREventManager.GetAllEventPrototypes();

            Rect helpRect = new Rect(position.x + 15, labelRect.yMax, position.width - 15, 4.0f * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            if (expectedEvents.Count == 0) {
                m_ShowHelp = true;
                EditorGUI.HelpBox(helpRect, "No matching VREventPrototpyes.  You may need to add an IVREventProducer " +
                    "to the scene.  Look under Add Component/MinVR/Input/* for virtual and physical input devices that " +
                    "produce VREvents.", MessageType.Warning);
                eventNameRect.y += helpRect.height;
            }

            string[] eventNames = new string[expectedEvents.Count + 1]; ;
            string[] dataTypeNames = new string[expectedEvents.Count + 1];
            GUIContent[] displayNames = new GUIContent[expectedEvents.Count + 1];
            eventNames[0] = "";
            dataTypeNames[0] = "";
            displayNames[0] = new GUIContent("(none)");

            int i = 1;
            int selected = 0;
            foreach (var e in expectedEvents) {
                eventNames[i] = e.GetEventName();
                dataTypeNames[i] = e.GetDataTypeName();
                displayNames[i] = new GUIContent(e.GetDisplayName());
                if (e.GetEventName() == eventNameProp.stringValue) {
                    selected = i;
                }
                i++;
            }

            EditorGUI.BeginChangeCheck();
            selected = EditorGUI.Popup(eventNameRect, selected, displayNames);
            if (EditorGUI.EndChangeCheck()) {
                if (selected >= 0) {
                    eventNameProp.stringValue = eventNames[selected];
                    dataTypeNameProp.stringValue = dataTypeNames[selected];
                }
            }
            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 2.0f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            if (m_ShowHelp) {
                height += 4.0f * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }

        private bool m_ShowHelp = false;
    }

} // namespace
