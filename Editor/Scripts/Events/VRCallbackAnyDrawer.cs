using UnityEngine;
using UnityEditor;
using System.Linq;

namespace IVLab.MinVR3
{

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

                VREventPrototypeAny any = new VREventPrototypeAny();
                var dataTypeNames = any.AllEventPrototypes.Keys.ToList();
                var dataTypeNamesList = dataTypeNames.ToList();
                int blankIndex = dataTypeNamesList.FindIndex(t => t.Length == 0);
                if (blankIndex >= 0) {
                    dataTypeNamesList[blankIndex] = "(none)";
                }
                var displayNames = dataTypeNamesList
                    .Select(t => new GUIContent(t))
                    .ToArray();

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
