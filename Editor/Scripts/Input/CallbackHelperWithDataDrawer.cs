using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{
    [CustomPropertyDrawer(typeof(CallbackHelperWithData<>))]
    public class CallbackHelperWithDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            SerializedProperty onVREventProp = property.FindPropertyRelative("onVREvent");
            EditorGUI.PropertyField(position, onVREventProp);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty onVREventProp = property.FindPropertyRelative("onVREvent");

            return EditorGUI.GetPropertyHeight(onVREventProp, true) + EditorGUIUtility.standardVerticalSpacing;
        }
    }

} // namespace
