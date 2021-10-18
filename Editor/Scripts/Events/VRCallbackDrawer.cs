using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEditorInternal;

namespace IVLab.MinVR3
{

    [CustomPropertyDrawer(typeof(VRCallback))]
    public class VRCallbackDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // label
            Rect propRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(propRect, label);

            // notifyList
            SerializedProperty notifyListProp = property.FindPropertyRelative("m_NotifyList");
            propRect.x += 15;
            propRect.width -= 15;
            propRect.y += propRect.height + EditorGUIUtility.standardVerticalSpacing;
            propRect.height = position.height - 2 * propRect.height;
            EditorGUI.PropertyField(propRect, notifyListProp);
            
            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // label
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // notifyList
            SerializedProperty notifyListProp = property.FindPropertyRelative("m_NotifyList");
            height += EditorGUI.GetPropertyHeight(notifyListProp, true) + EditorGUIUtility.standardVerticalSpacing;

            return height;
        }
    }


    [CustomPropertyDrawer(typeof(VRCallback<>))]
    public class VRCallbackTemplatedDrawer : VRCallbackDrawer
    {
    }



    [CustomPropertyDrawer(typeof(VRCallbackAny))]
    public class VRCallbackAnyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // label
            Rect propRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(propRect, label);

            // indent
            propRect.x += 15;
            propRect.width -= 15;


            SerializedProperty dataTypeNameProp = property.FindPropertyRelative("m_DataTypeName");

            // data type dropdown
            SerializedProperty showDropdownProp = property.FindPropertyRelative("m_ShowDataTypeDropdown");
            if (showDropdownProp.boolValue) {
                List<string> dataTypeNamesList = new List<string>();
                SerializedProperty iteratorProp = property.serializedObject.GetIterator();
                while (iteratorProp.NextVisible(true)) {
                    if (iteratorProp.name.StartsWith("m_Callback")) {
                        dataTypeNamesList.Add(iteratorProp.name.Substring(10));
                    }
                }
                string[] dataTypeNames = new string[dataTypeNamesList.Count];
                GUIContent[] displayNames = new GUIContent[dataTypeNamesList.Count];
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
                propRect.y += propRect.height + EditorGUIUtility.standardVerticalSpacing;
                selected = EditorGUI.Popup(propRect, new GUIContent("Expected Data Type"), selected, displayNames);
                if (EditorGUI.EndChangeCheck()) {
                    if (selected >= 0) {
                        dataTypeNameProp.stringValue = dataTypeNames[selected];
                    }
                }
            }

            // notifyList
            string callbackVarName = "m_Callback" + dataTypeNameProp.stringValue;
            SerializedProperty callbackProp = property.FindPropertyRelative(callbackVarName);
            SerializedProperty notifyListProp = callbackProp.FindPropertyRelative("m_NotifyList");

            propRect.y += propRect.height + EditorGUIUtility.standardVerticalSpacing;
            propRect.height = EditorGUI.GetPropertyHeight(callbackProp);
            EditorGUI.PropertyField(propRect, notifyListProp);

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // label
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // data type dropdown
            SerializedProperty showDropdownProp = property.FindPropertyRelative("m_ShowDataTypeDropdown");
            if (showDropdownProp.boolValue) {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            // notifyList
            SerializedProperty dataTypeNameProp = property.FindPropertyRelative("m_DataTypeName");
            string callbackVarName = "m_Callback" + dataTypeNameProp.stringValue;
            SerializedProperty callbackProp = property.FindPropertyRelative(callbackVarName);
            SerializedProperty notifyListProp = callbackProp.FindPropertyRelative("m_NotifyList");
            height += EditorGUI.GetPropertyHeight(notifyListProp, true) + EditorGUIUtility.standardVerticalSpacing;

            return height;
        }
    }


} // namespace
