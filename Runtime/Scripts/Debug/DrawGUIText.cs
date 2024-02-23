using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    /// <summary>
    /// </summary>
    [AddComponentMenu("MinVR/Debug/Draw GUI Text")]
    [ExecuteAlways]
    public class DrawGUIText : MonoBehaviour
    {
        public string m_Text = "Hello World";
        public Color m_TextColor = Color.white;
        public int m_FontSize = 10;
        public TextAnchor m_TextAnchor = TextAnchor.MiddleLeft;
        public Rect m_Position = new Rect(20, 10, 40, 20);

        void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.alignment = m_TextAnchor;
            style.fontSize = m_FontSize;
            style.normal.textColor = m_TextColor;

            GUI.Label(m_Position, m_Text, style);
        }
    }

} // namespace
