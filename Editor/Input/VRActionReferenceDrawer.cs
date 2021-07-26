using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

namespace IVLab.MinVR3
{

    [CustomPropertyDrawer(typeof(VRActionReference))]
    public class VRActionReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            if (MinVR.mainInput != null) {
                MinVR.mainInput.RefreshEditorArrays();
            }
            if ((MinVR.mainInput != null) && (MinVR.mainInput.editorActionNames != null) && (MinVR.mainInput.editorActionNames.Count > 0)) {
                string text = label.text;
                string tooltip = label.tooltip;

                // Action Name Dropdown
                SerializedProperty actionProp = property.FindPropertyRelative("inputActionName");
                Rect actionRect = new Rect(position.min.x, position.min.y + 0.5f * EditorGUIUtility.standardVerticalSpacing, position.size.x, EditorGUIUtility.singleLineHeight);
                GUIContent actionLabel = new GUIContent(text + " Action", tooltip);
                int id = MinVR.mainInput.editorActionNames.IndexOf(actionProp.stringValue);                
                id = EditorGUI.IntPopup(actionRect, actionLabel, id, MinVR.mainInput.editorGUIContents, MinVR.mainInput.editorActionIndices);
                if ((id >= 0) && (id < MinVR.mainInput.editorActionNames.Count)) {
                    actionProp.stringValue = MinVR.mainInput.editorActionNames[id];
                }

                // Action Phase Dropdown
                SerializedProperty phaseProp = property.FindPropertyRelative("inputActionPhase");
                Rect phaseRect = new Rect(position.min.x, actionRect.min.y + actionRect.size.y + EditorGUIUtility.standardVerticalSpacing, position.size.x, EditorGUIUtility.singleLineHeight);
                GUIContent phaseLabel = new GUIContent(text + " Phase", tooltip);
                EditorGUI.PropertyField(phaseRect, phaseProp, phaseLabel);

            } else {
                string msg;
                if (MinVR.mainInput == null) {
                    msg = "No InputActions available. Add a VRInput component to the scene.";
                } else {
                    msg = "MinVR.mainInput does not contain any InputActions.";
                }
                EditorGUI.HelpBox(position, msg, MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 2 * EditorGUIUtility.singleLineHeight + 3 * EditorGUIUtility.standardVerticalSpacing;
        }

    }

} // namespace
