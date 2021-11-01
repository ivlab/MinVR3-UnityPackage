using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEditorInternal;

namespace IVLab.MinVR3
{

    // VRCallback does not need a custom drawer since it inherits from UnityEvent and displays correctly.
    
    // Same for VRCallback<>, looks good as it is.


    [CustomPropertyDrawer(typeof(VRCallbackAny))]
    public class VRCallbackAnyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            string labelText = label.text;

            Rect propRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

            // Pick just one of the internal VRCallback* types to display based on the current value of the datatype
            // Each callback is stored in a member var of the form "m_Callback" + dataTypeName
            SerializedProperty dataTypeNameProp = property.FindPropertyRelative("m_DataTypeName");

            // data type dropdown
            SerializedProperty showDataTypeProp = property.FindPropertyRelative("m_ShowDataTypeInEditor");
            if (showDataTypeProp.boolValue) {
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
                selected = EditorGUI.Popup(propRect, new GUIContent(labelText + " Data Type"), selected, displayNames);
                if (EditorGUI.EndChangeCheck()) {
                    if (selected >= 0) {
                        dataTypeNameProp.stringValue = dataTypeNames[selected];
                    }
                }
                propRect.y += propRect.height;
            }


            string callbackPropName = "m_Callback" + dataTypeNameProp.stringValue;
            SerializedProperty callbackProp = property.FindPropertyRelative(callbackPropName);

            propRect.height = EditorGUI.GetPropertyHeight(callbackProp, true) + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(propRect, callbackProp, new GUIContent(labelText));

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            
            // data type dropdown
            SerializedProperty showDataTypeProp = property.FindPropertyRelative("m_ShowDataTypeInEditor");
            if (showDataTypeProp.boolValue) {
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            // callback
            SerializedProperty dataTypeNameProp = property.FindPropertyRelative("m_DataTypeName");
            string callbackVarName = "m_Callback" + dataTypeNameProp.stringValue;
            SerializedProperty callbackProp = property.FindPropertyRelative(callbackVarName);
            height += EditorGUI.GetPropertyHeight(callbackProp, true) + EditorGUIUtility.standardVerticalSpacing;

            return height;
        }
    }

} // namespace
