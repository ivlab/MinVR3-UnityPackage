using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(FSMCallback))]
public class FSMCallbackDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty cbProp = property.FindPropertyRelative("callback");
        float cbHeight = EditorGUI.GetPropertyHeight(cbProp, true);
        Rect cbRect = new Rect(position.min.x, position.min.y, position.size.x, cbHeight);
        EditorGUI.PropertyField(cbRect, cbProp, label);

        EditorGUI.EndProperty();

        /*
        EditorGUI.BeginProperty(position, label, property);
        Rect rectFoldout = new Rect(position.min.x, position.min.y, position.size.x, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(rectFoldout, property.isExpanded, label);
        if (property.isExpanded) {
            SerializedProperty cbProp = property.FindPropertyRelative("callback");
            float cbHeight = EditorGUI.GetPropertyHeight(cbProp, true);
            float y = position.min.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Rect cbRect = new Rect(position.min.x, y, position.size.x, cbHeight);
            EditorGUI.PropertyField(cbRect, cbProp, new GUIContent("Callback Function", "Specify a GameObject and a function to call on that GameObject"));
        }
        EditorGUI.EndProperty();
        */
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty cbProp = property.FindPropertyRelative("callback");
        float cbHeight = 0;
        if (cbProp != null) {
            cbHeight = EditorGUI.GetPropertyHeight(cbProp, true);
        }
        return cbHeight + EditorGUIUtility.standardVerticalSpacing;

        /*
        if (!property.isExpanded) {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        } else {
            SerializedProperty cbProp = property.FindPropertyRelative("callback");
            float cbHeight = 0;
            if (cbProp != null) {
                cbHeight = EditorGUI.GetPropertyHeight(cbProp, true);
            }
            return cbHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        */
    }

}
