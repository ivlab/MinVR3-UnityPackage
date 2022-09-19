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
                GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
                myStyle.richText = true;
                EditorGUI.TextArea(position, infoBoxAttribute.message, myStyle);
            }
        }

        public override float GetHeight()
        {
            InfoBoxAttribute infoBoxAttribute = attribute as InfoBoxAttribute;
            GUIStyle myStyle = GUI.skin.GetStyle("HelpBox");
            myStyle.richText = true;
            if ((infoBoxAttribute != null) && (myStyle != null)) {
                return myStyle.CalcHeight(new GUIContent(infoBoxAttribute.message),
                    EditorGUIUtility.currentViewWidth-60);
            } else {
                return base.GetHeight();
            }
        }
    }

}