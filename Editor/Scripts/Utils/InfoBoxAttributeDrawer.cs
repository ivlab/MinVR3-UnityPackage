using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{
    
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxAttributeDrawer : DecoratorDrawer
    {
        // inspired by: https://forum.unity.com/threads/helpattribute-allows-you-to-use-helpbox-in-the-unity-inspector-window.462768/

        public override void OnGUI(Rect position)
        {
            InfoBoxAttribute infoBoxAttribute = attribute as InfoBoxAttribute;
            if (infoBoxAttribute != null) {
                EditorGUI.HelpBox(position, infoBoxAttribute.message, MessageType.None);
            }
        }

        public override float GetHeight()
        {
            InfoBoxAttribute infoBoxAttribute = attribute as InfoBoxAttribute;
            GUIStyle guiStyle = (GUI.skin != null) ? GUI.skin.GetStyle("helpbox") : null;
            if ((infoBoxAttribute != null) && (guiStyle != null)) {
                return Mathf.Max(40f, guiStyle.CalcHeight(new GUIContent(infoBoxAttribute.message),
                    EditorGUIUtility.currentViewWidth) + 4);
            } else {
                return base.GetHeight();
            }
        }
    }

}