using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{

    [CustomPropertyDrawer(typeof(FSMArcCallback))]
    public class FSMArcCallbackDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float left = position.min.x;
            float top = position.min.y;
            float width = position.size.x;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float padding = EditorGUIUtility.standardVerticalSpacing;


            EditorGUI.BeginProperty(position, label, property);

            // DataType enum
            Rect rect = new Rect(left, top, width, lineHeight);
            SerializedProperty dataTypeProp = property.FindPropertyRelative("callbackDataType");
            EditorGUI.PropertyField(rect, dataTypeProp, new GUIContent("Callback Data Type", "Unity InputActions can hold data (e.g., current mouse position), to pass that data to your callback function, specify the type for the expected data here"));

            // UnityEvent callback, selected dynamically based on the value of dataType
            if (dataTypeProp != null) {
                FSMArcCallback.DataType dataType = (FSMArcCallback.DataType)dataTypeProp.enumValueIndex;

                SerializedProperty cbProp;
                if (dataType == FSMArcCallback.DataType.Bool) {
                    cbProp = property.FindPropertyRelative("callbackBool");
                } else if (dataType == FSMArcCallback.DataType.Int) {
                    cbProp = property.FindPropertyRelative("callbackInt");
                } else if (dataType == FSMArcCallback.DataType.Float) {
                    cbProp = property.FindPropertyRelative("callbackFloat");
                } else if (dataType == FSMArcCallback.DataType.Vector2) {
                    cbProp = property.FindPropertyRelative("callbackVector2");
                } else if (dataType == FSMArcCallback.DataType.Vector3) {
                    cbProp = property.FindPropertyRelative("callbackVector3");
                } else if (dataType == FSMArcCallback.DataType.Quaternion) {
                    cbProp = property.FindPropertyRelative("callbackQuaternion");
                } else if (dataType == FSMArcCallback.DataType.InputActionCallbackContext) {
                    cbProp = property.FindPropertyRelative("callbackContext");
                } else {
                    cbProp = property.FindPropertyRelative("callbackVoid");
                }

                rect.y += rect.height + padding;
                rect.height = EditorGUI.GetPropertyHeight(cbProp, true);
                EditorGUI.PropertyField(rect, cbProp, new GUIContent("On Trigger", "Specify a GameObject and a function to call within that object.  In the list of functions that pops up, pick a function under the 'Dynamic' heading to have the event data passed to the function; or pick a function under 'Static' to ignore the event data and just call the function"));


            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //if (!property.isExpanded) {
            //    return EditorGUIUtility.singleLineHeight;
            //} else {
            float cbHeight = 0;
            SerializedProperty dataTypeProp = property.FindPropertyRelative("callbackDataType");
            if (dataTypeProp != null) {
                FSMArcCallback.DataType dataType = (FSMArcCallback.DataType)dataTypeProp.enumValueIndex;

                SerializedProperty cbProp;
                if (dataType == FSMArcCallback.DataType.Bool) {
                    cbProp = property.FindPropertyRelative("callbackBool");
                } else if (dataType == FSMArcCallback.DataType.Int) {
                    cbProp = property.FindPropertyRelative("callbackInt");
                } else if (dataType == FSMArcCallback.DataType.Float) {
                    cbProp = property.FindPropertyRelative("callbackFloat");
                } else if (dataType == FSMArcCallback.DataType.Vector2) {
                    cbProp = property.FindPropertyRelative("callbackVector2");
                } else if (dataType == FSMArcCallback.DataType.Vector3) {
                    cbProp = property.FindPropertyRelative("callbackVector3");
                } else if (dataType == FSMArcCallback.DataType.Quaternion) {
                    cbProp = property.FindPropertyRelative("callbackQuaternion");
                } else if (dataType == FSMArcCallback.DataType.InputActionCallbackContext) {
                    cbProp = property.FindPropertyRelative("callbackContext");
                } else {
                    cbProp = property.FindPropertyRelative("callbackVoid");
                }
                cbHeight += EditorGUI.GetPropertyHeight(cbProp, true);
            }
            return cbHeight + EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
            //}
        }

    }

} // namespace