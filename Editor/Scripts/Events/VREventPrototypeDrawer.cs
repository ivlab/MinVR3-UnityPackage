using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace IVLab.MinVR3
{
    
    [CustomPropertyDrawer(typeof(VREventPrototype), true)]
    public class VREventPrototypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty eventNameProp = property.FindPropertyRelative("m_EventName");
            SerializedProperty dataTypeNameProp = property.FindPropertyRelative("m_DataTypeName");

            SerializedProperty defineNewPrototypeInEditorProp =
                property.FindPropertyRelative("m_DefineNewPrototypeInEditor");

            if ((defineNewPrototypeInEditorProp != null) && (defineNewPrototypeInEditorProp.boolValue)) {
                // Allow defining a new event prototype, like for creating a virtual input device

                string labelText = label.text;

                // data type dropdown
                Rect propRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

                VREventPrototypeAny any = new VREventPrototypeAny();
                var dataTypeNames = any.AllEventPrototypes.Keys.ToList();
                var dataTypeNamesList = dataTypeNames.ToList();
                int blankIndex = dataTypeNamesList.FindIndex(t => t.Length == 0);
                if (blankIndex >= 0) {
                    dataTypeNamesList[blankIndex] = "(none)";
                }
                var displayNames = dataTypeNamesList
                    .Select(t => new GUIContent(t))
                    .ToArray();

                int selected = -1;
                for (int i = 0; i < dataTypeNamesList.Count; i++) {
                    dataTypeNames[i] = dataTypeNamesList[i];
                    if (dataTypeNamesList[i] == "") {
                        displayNames[i] = new GUIContent("(none)");
                    } else {
                        displayNames[i] = new GUIContent(dataTypeNamesList[i]);
                    }
                    if (dataTypeNames[i] == dataTypeNameProp.stringValue) {
                        selected = i;
                    }
                }
                EditorGUI.BeginChangeCheck();
                selected = EditorGUI.Popup(propRect, new GUIContent(labelText + " Data Type"), selected, displayNames);
                if (EditorGUI.EndChangeCheck()) {
                    if (selected >= 0) {
                        SetDataType(property, dataTypeNames[selected]);
                    }
                }
                propRect.y += propRect.height;

                // event name field
                EditorGUI.PropertyField(propRect, eventNameProp, new GUIContent(labelText + " Event Name"));

            } else {
                // Pick for an existing event prototype

                List<IVREventPrototype> expectedEvents = GetExpectedEventPrototypes(property);

                Rect eventNameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

                // If there are no expectedEvents, then show a help msg
                Rect helpRect = new Rect(position.x, position.y, position.width, 4.0f * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                if (expectedEvents.Count == 0) {
                    EditorGUI.HelpBox(helpRect, "No matching VREventPrototpyes.  You may need to add an IVREventProducer " +
                        "to the scene.  Look under Add Component/MinVR/Input/* for virtual and physical input devices that " +
                        "produce VREvents.", MessageType.Warning);
                    eventNameRect.y += helpRect.height;
                }

                // Display the event name property as a dropdown list
                // Build the content for the dropdown list
                string[] eventNames = new string[expectedEvents.Count + 1];
                string[] dataTypeNames = new string[expectedEvents.Count + 1];
                GUIContent[] displayNames = new GUIContent[expectedEvents.Count + 1];
                eventNames[0] = "";
                dataTypeNames[0] = "";
                displayNames[0] = new GUIContent("(none)");

                int i = 1;
                int selected = 0;
                foreach (var e in expectedEvents) {
                    eventNames[i] = e.GetEventName();
                    dataTypeNames[i] = e.GetEventDataTypeName();
                    displayNames[i] = new GUIContent(e.GetEventDisplayName());
                    if (e.GetEventName() == eventNameProp.stringValue) {
                        selected = i;
                    }
                    i++;
                }

                if ((eventNameProp.stringValue != "") && (selected == 0)) {
                    Debug.LogWarning("Possible Bug: A VREventPrototpye references an event named '" +
                        eventNameProp.stringValue + "' but this event does not appear to be generated by any " +
                        "IVRInputDevices in the scene.");
                }

                EditorGUI.BeginChangeCheck();
                selected = EditorGUI.Popup(eventNameRect, label, selected, displayNames);
                if (EditorGUI.EndChangeCheck()) {
                    if (selected >= 0) {
                        eventNameProp.stringValue = eventNames[selected];
                        SetDataType(property, dataTypeNames[selected]);
                    }
                }
                EditorGUI.EndProperty();
            }
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty defineNewPrototypeInEditorProp =
                property.FindPropertyRelative("m_DefineNewPrototypeInEditor");

            float height = 1.0f * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            if ((defineNewPrototypeInEditorProp != null) && (defineNewPrototypeInEditorProp.boolValue)) {
                return height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            } else if (GetExpectedEventPrototypes(property).Count == 0) {
                height += 4.0f * EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }

        protected virtual List<IVREventPrototype> GetExpectedEventPrototypes(SerializedProperty property)
        {
            // Get all possible events that match the current data type from all IVRInputDevices in the scene
            SerializedProperty dataTypeNameProp = property.FindPropertyRelative("m_DataTypeName");
            return VREventManager.GetMatchingEventPrototypes(dataTypeNameProp.stringValue);
        }

        protected virtual void SetDataType(SerializedProperty property, string value)
        {
            // data type should not change for VREventPrototype and VREventPrototype<>
        }

    }


    // VREventPrototype<> can be dispalyed using the exact same property drawer
    [CustomPropertyDrawer(typeof(VREventPrototypeT<>), true)]
    public class VREventPrototypeTemplatedDrawer : VREventPrototypeDrawer
    {
    }


    // VREventPrototypeAny is almost the same; the only changes are that the list of possible events can include events
    // with different data types and that the data type can be changed IF the data type is not locked.
    [CustomPropertyDrawer(typeof(VREventPrototypeAny), true)]
    public class VREventPrototypeAnyDrawer : VREventPrototypeDrawer
    {
        protected override List<IVREventPrototype> GetExpectedEventPrototypes(SerializedProperty property)
        {
            SerializedProperty dataTypeLockedProp = property.FindPropertyRelative("m_DataTypeLocked");

            if (dataTypeLockedProp.boolValue) {
                // Get all possible events that match the current data type from all IVRInputDevices in the scene
                SerializedProperty dataTypeNameProp = property.FindPropertyRelative("m_DataTypeName");
                return VREventManager.GetMatchingEventPrototypes(dataTypeNameProp.stringValue);
            } else {
                // Get all possible events that match the current data type from all IVRInputDevices in the scene
                return VREventManager.GetAllEventPrototypes();
            }
        }

        protected override void SetDataType(SerializedProperty property, string value)
        {
            SerializedProperty dataTypeLockedProp = property.FindPropertyRelative("m_DataTypeLocked");
            if (!dataTypeLockedProp.boolValue) {
                SerializedProperty dataTypeNameProp = property.FindPropertyRelative("m_DataTypeName");
                dataTypeNameProp.stringValue = value;
            }
        }
    }

} // namespace
