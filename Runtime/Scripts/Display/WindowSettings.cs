using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    [AddComponentMenu("MinVR/Display/Window Settings")]
    public class WindowSettings : MonoBehaviour
    {
        // GRAPHICS WINDOW SETTINGS:

        [Tooltip("Window title")]
        public string title = "IVLab.MinVR3";

        [Tooltip("If false, remove the border for the window, like for a fullscreen window.")]
        public bool hasBorder = true;

        [Tooltip("X position of the graphics window on screen when not running inside the editor.")]
        public int windowXPos = 0;
        [Tooltip("Y position of the graphics window on screen when not running inside the editor.")]
        public int windowYPos = 0;
        [Tooltip("Width of the graphics window on screen when not running inside the editor.")]
        public int windowWidth = 1024;
        [Tooltip("Height of the graphics window on screen when not running inside the editor.")]
        public int windowHeight = 768;


        private void Start()
        {
            if (!hasBorder) {
                WindowUtils.RemoveBorder();
            }
            WindowUtils.SetPositionAndSize(windowXPos, windowYPos, windowWidth, windowHeight);
            WindowUtils.SetWindowTitle(title);
        }
    }

} // namespace
