using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace IVLab.MinVR3
{
   
    [CustomPropertyDrawer(typeof(VREventCallback), true)]
    public class VREventCallbackDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty prototypeProp = property.FindPropertyRelative("m_EventPrototype");
            SerializedProperty callbackProp = property.FindPropertyRelative("m_VRCallback");

            // label
            Rect propRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.LabelField(propRect, new GUIContent(label.text), EditorStyles.boldLabel);

            // indent
            propRect.x += 5;
            propRect.width -= 5;

            // event prototype
            propRect.y += propRect.height;
            propRect.height = EditorGUI.GetPropertyHeight(prototypeProp) + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(propRect, prototypeProp, new GUIContent("Listen for VREvent"));
            if (EditorGUI.EndChangeCheck()) {
                OnEventPrototypeChanged(prototypeProp, callbackProp);
            }

            // callback
            propRect.y += propRect.height;
            propRect.height = EditorGUI.GetPropertyHeight(callbackProp) + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(propRect, callbackProp, new GUIContent("OnVREvent"));

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // label
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            // event prototype
            SerializedProperty prototypeProp = property.FindPropertyRelative("m_EventPrototype");
            height += EditorGUI.GetPropertyHeight(prototypeProp) + EditorGUIUtility.standardVerticalSpacing;
            // callback
            SerializedProperty callbackProp = property.FindPropertyRelative("m_VRCallback");
            height += EditorGUI.GetPropertyHeight(callbackProp) + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        public virtual void OnEventPrototypeChanged(SerializedProperty prototypeProp, SerializedProperty callbackProp)
        {
        }
    }



    [CustomPropertyDrawer(typeof(VREventCallbackT<>), true)]
    public class VREventCallbackTDrawer : VREventCallbackDrawer
    {
    }


    [CustomPropertyDrawer(typeof(VREventCallbackAny), true)]
    public class VREventCallbackAnyDrawer : VREventCallbackDrawer
    {
        public override void OnEventPrototypeChanged(SerializedProperty prototypeProp, SerializedProperty callbackProp)
        {
            SerializedProperty prototypeDataTypeProp = prototypeProp.FindPropertyRelative("m_DataTypeName");
            SerializedProperty callbackDataTypeProp = callbackProp.FindPropertyRelative("m_DataTypeName");
            callbackDataTypeProp.stringValue = prototypeDataTypeProp.stringValue;
        }
    }

} // namespace
