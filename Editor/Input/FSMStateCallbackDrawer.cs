using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{

    [CustomPropertyDrawer(typeof(FSMStateCallback))]
    public class FSMStateCallbackDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty cbProp = property.FindPropertyRelative("callback");
            float cbHeight = EditorGUI.GetPropertyHeight(cbProp, true);
            Rect cbRect = new Rect(position.min.x, position.min.y, position.size.x, cbHeight);
            EditorGUI.PropertyField(cbRect, cbProp, label);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty cbProp = property.FindPropertyRelative("callback");
            float cbHeight = 0;
            if (cbProp != null) {
                cbHeight = EditorGUI.GetPropertyHeight(cbProp, true);
            }
            return cbHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }

} // namespace
