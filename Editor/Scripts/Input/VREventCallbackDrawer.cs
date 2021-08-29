using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace IVLab.MinVR3
{
    [CustomPropertyDrawer(typeof(VREventCallback))]
    public class VREventCallbackDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // LABEL
            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.LabelField(labelRect, label);

            //    LISTEN FOR
            EditorGUI.indentLevel++;
            SerializedProperty listenForProp = property.FindPropertyRelative("m_ListenFor");
            Rect listenForRect = new Rect(position.x, labelRect.yMax, position.width, EditorGUI.GetPropertyHeight(listenForProp, true) + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.PropertyField(listenForRect, listenForProp);
            EditorGUI.indentLevel--;

            //    CALLBACK FUNCTION
            //    Display the appropriate callback given the event's data type
            SerializedProperty dataTypeProp = listenForProp.FindPropertyRelative("m_EventDataType");
            SerializedProperty callbackProp = FindCallbackPropertyRelative(property, dataTypeProp.stringValue);
            if (callbackProp != null) {
                Rect cbRect = new Rect(position.x + 15, listenForRect.yMax, position.width - 15, position.height - listenForRect.yMax);
                EditorGUI.PropertyField(cbRect, callbackProp);
            }

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            // label
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // listen for
            SerializedProperty listenForProp = property.FindPropertyRelative("m_ListenFor");
            height += EditorGUI.GetPropertyHeight(listenForProp, true) + EditorGUIUtility.standardVerticalSpacing;

            // callback
            SerializedProperty dataTypeProp = listenForProp.FindPropertyRelative("m_EventDataType");
            SerializedProperty callbackProp = FindCallbackPropertyRelative(property, dataTypeProp.stringValue);
            if (callbackProp != null) {
                height += EditorGUI.GetPropertyHeight(callbackProp, true);
            }
            height += EditorGUIUtility.standardVerticalSpacing;

            return height;
        }


        public SerializedProperty FindCallbackPropertyRelative(SerializedProperty property, string dataType)
        {
            if (dataType == "") {
                return property.FindPropertyRelative("m_VoidCallback");
            } else if (dataType == typeof(bool).Name) {
                return property.FindPropertyRelative("m_BoolCallback");
            } else if (dataType == typeof(int).Name) {
                return property.FindPropertyRelative("m_IntCallback");
            } else if (dataType == typeof(float).Name) {
                return property.FindPropertyRelative("m_FloatCallback");
            } else if (dataType == typeof(Vector2).Name) {
                return property.FindPropertyRelative("m_Vector2Callback");
            } else if (dataType == typeof(Vector3).Name) {
                return property.FindPropertyRelative("m_Vector3Callback");
            } else if (dataType == typeof(Quaternion).Name) {
                return property.FindPropertyRelative("m_QuaternionCallback");
            } else if (dataType == typeof(Touch).Name) {
                return property.FindPropertyRelative("m_TouchCallback");
            }
            // TODO: add more here as necessary (3/3)


            return null;
        }
    }

} // namespace
