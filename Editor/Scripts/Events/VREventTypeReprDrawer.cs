using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace IVLab.MinVR3
{
    
    [CustomPropertyDrawer(typeof(VREventTypeRepr), true)]
    public class VREventTypeReprDrawer : PropertyDrawer
    {
        private int typeIndex = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty typeProp = property.FindPropertyRelative("eventType");

            Rect eventNameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

            VREventPrototypeAny any = new VREventPrototypeAny();
            var eventDataTypes = any.AllEventPrototypes.Keys.ToList();
            var eventDataTypesDisplay = eventDataTypes.ToList();
            int blankIndex = eventDataTypesDisplay.FindIndex(t => t.Length == 0);
            if (blankIndex >= 0)
            {
                eventDataTypesDisplay[blankIndex] = "[None]";
            }
            var guiDisplay = eventDataTypesDisplay
                .Select(t => new GUIContent(t))
                .ToArray();

            EditorGUI.BeginChangeCheck();
            int newTypeIndex = EditorGUI.Popup(eventNameRect, label, typeIndex, guiDisplay);
            if (EditorGUI.EndChangeCheck()) {
                if (newTypeIndex >= 0) {
                    typeIndex = newTypeIndex;
                    typeProp.stringValue = eventDataTypes[typeIndex];
                }
            }
            EditorGUI.EndProperty();
        }
    }
} // namespace