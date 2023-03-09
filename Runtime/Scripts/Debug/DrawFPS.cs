using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    /// <summary>
    /// Displays a frames-per-second counter in the top-left of the screen; useful when working with a VR
    /// display that doesn't allow you to easily see Unity's performance data while you are wearing it.
    /// </summary>
    [AddComponentMenu("MinVR/Debug/Draw FPS")]
    public class DrawFPS : MonoBehaviour
    {
        public Color m_TextColor = Color.white;
        public int m_FontSize = 10;
        public Rect m_Position = new Rect(0, 5, 40, 20);

        void OnGUI()
        {
            float fps = 1.0f / Time.smoothDeltaTime;
            string text = string.Format("{0:0.} fps", fps);

            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperRight;
            style.fontSize = m_FontSize;
            style.normal.textColor = m_TextColor;

            GUI.Label(m_Position, text, style);
        }
    }

} // namespace
