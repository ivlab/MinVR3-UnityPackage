using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace IVLab.MinVR3
{

    [CustomPropertyDrawer(typeof(VREventListener))]
    public class VREventListenerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty listenForProp = property.FindPropertyRelative("m_VREventToListenFor");
            SerializedProperty callbackProp = property.FindPropertyRelative("m_OnVREventCallback");

            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.LabelField(labelRect, label);

            Rect listenForRect = new Rect(position.x, labelRect.yMax, position.width, EditorGUI.GetPropertyHeight(listenForProp, true) + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.PropertyField(listenForRect, listenForProp);

            Rect callbackRect = new Rect(position.x, listenForRect.yMax, position.width, EditorGUI.GetPropertyHeight(callbackProp, true) + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.PropertyField(callbackRect, callbackProp);

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty listenForProp = property.FindPropertyRelative("m_VREventToListenFor");
            height += EditorGUI.GetPropertyHeight(listenForProp, true) + EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty callbackProp = property.FindPropertyRelative("m_OnVREventCallback");
            height += EditorGUI.GetPropertyHeight(callbackProp, true) + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

    }


    [CustomPropertyDrawer(typeof(VREventListener<>))]
    public class VREventListenerTemplatedDrawer : VREventListenerDrawer
    {
    }



    [CustomPropertyDrawer(typeof(VREventListenerAny))]
    public class VREventListenerAnyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty listenForProp = property.FindPropertyRelative("m_VREventToListenFor");
            SerializedProperty callbackProp = property.FindPropertyRelative("m_OnVREventCallback");

            Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.LabelField(labelRect, label);

            Rect listenForRect = new Rect(position.x, labelRect.yMax, position.width, EditorGUI.GetPropertyHeight(listenForProp, true) + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(listenForRect, listenForProp);
            if (EditorGUI.EndChangeCheck()) {
                SerializedProperty prototypeDataTypeNameProp = listenForProp.FindPropertyRelative("m_DataTypeName");
                SerializedProperty callbackDataTypeNameProp = callbackProp.FindPropertyRelative("m_DataTypeName");
                callbackDataTypeNameProp.stringValue = prototypeDataTypeNameProp.stringValue;
            }

            Rect callbackRect = new Rect(position.x, listenForRect.yMax, position.width, EditorGUI.GetPropertyHeight(callbackProp, true) + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.PropertyField(callbackRect, callbackProp);

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty listenForProp = property.FindPropertyRelative("m_VREventToListenFor");
            height += EditorGUI.GetPropertyHeight(listenForProp, true) + EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty callbackProp = property.FindPropertyRelative("m_OnVREventCallback");
            height += EditorGUI.GetPropertyHeight(callbackProp, true) + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

    }

} // namespace
