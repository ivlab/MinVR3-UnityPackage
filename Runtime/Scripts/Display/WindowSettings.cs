using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    [AddComponentMenu("MinVR/Display/Window Settings")]
    public class WindowSettings : MonoBehaviour
    {
        [Header("Window Configuration Options")]
        [SerializeField]
        public bool setWindowTitle = true;
        [SerializeField, Tooltip("Set the title for this window")]
        public string windowTitle = "MinVR3 Window";

        [SerializeField]
        public bool setWindowPositionAndSize = true;
        [SerializeField, Tooltip("Upper left x-coordinate of the window, in pixels")]
        public int upperLeftX = 0;
        [SerializeField, Tooltip("Upper left y-coordinate of the window, in pixels")]
        public int upperLeftY = 0;

        [SerializeField, Tooltip("Window width, in pixels")]
        public int width = 1920;
        [SerializeField, Tooltip("Window height, in pixels")]
        public int height = 1080;

        [SerializeField]
        public bool setShowBorders = true;
        [SerializeField, Tooltip("Show the window decorations or not")]
        public bool showWindowBorders = true;

        [SerializeField]
        public bool setFullscreenMode = true;
        [SerializeField, Tooltip("Control the fullscreen mode of the application")]
        public FullScreenMode fullScreenMode;
        [SerializeField, Tooltip("Control the desired refresh rate of the application (0 = default / max)")]
        public int refreshRate = 0;

        [Header("When to Apply Window Configuration")]
        [SerializeField, Tooltip("When in the MonoBehaviour lifecycle to apply the configuration specified above")]
        public ApplyConfigTiming applyConfigTiming = ApplyConfigTiming.OnEnable;

        public enum ApplyConfigTiming
        {
            OnAwake,
            OnEnable,
            Start,
            Update,
            LateUpdate
        }

        void OnAwake()
        {
            if (applyConfigTiming == ApplyConfigTiming.OnAwake)
            {
                ApplyWindowConfig();
            }
        }

        void OnEnable()
        {
            if (applyConfigTiming == ApplyConfigTiming.OnEnable)
            {
                ApplyWindowConfig();
            }
        }

        void Start()
        {
            if (applyConfigTiming == ApplyConfigTiming.Start)
            {
                ApplyWindowConfig();
            }
        }

        void Update()
        {
            if (applyConfigTiming == ApplyConfigTiming.Update)
            {
                ApplyWindowConfig();
            }
        }

        void LateUpdate()
        {
            if (applyConfigTiming == ApplyConfigTiming.LateUpdate)
            {
                ApplyWindowConfig();
            }
        }


        private void ApplyWindowConfig()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            if (setFullscreenMode)
            {
                Screen.SetResolution(width, height, fullScreenMode, refreshRate);
            }

            if (setWindowPositionAndSize)
            {
                // set the window position and size
                WindowUtility.SetPosition(upperLeftX, upperLeftY, width, height);
                Debug.LogFormat("Set window position to {0}, {1}, resolution {2}x{3}", upperLeftX, upperLeftY, width, height);
            }

            if (setShowBorders)
            {
                WindowUtility.ShowWindowBorders(showWindowBorders);
                Debug.Log("Show window borders: " + showWindowBorders);
            }

            if (setWindowTitle)
            {
                WindowUtility.SetWindowTitle(windowTitle);
                Debug.Log("Set window title to `" + windowTitle + "`");
            }
#endif
            
        }
    }
}