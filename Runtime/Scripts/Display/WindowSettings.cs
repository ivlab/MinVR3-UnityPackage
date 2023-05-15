using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    [AddComponentMenu("MinVR/Display/Window Settings")]
    public class WindowSettings : MonoBehaviour
    {
        [Header("Window Configuration Options")]
        [SerializeField, Tooltip("Set the title for this window")]
        public string windowTitle = "MinVR3 Window";

        [SerializeField, Tooltip("Upper left x-coordinate of the window, in pixels")]
        public int upperLeftX = 0;
        [SerializeField, Tooltip("Upper left y-coordinate of the window, in pixels")]
        public int upperLeftY = 0;

        [SerializeField, Tooltip("Window width, in pixels")]
        public int width = 1920;
        [SerializeField, Tooltip("Window height, in pixels")]
        public int height = 1080;

        [SerializeField, Tooltip("Show the window decorations or not")]
        public bool showWindowBorders = true;

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
            // set the window position and size
            WindowUtility.SetPosition(upperLeftX, upperLeftY, width, height);
            Debug.LogFormat("Set window position to {0}, {1}, resolution {2}x{3}", upperLeftX, upperLeftY, width, height);

            WindowUtility.ShowWindowBorders(showWindowBorders);
            Debug.Log("Show window borders: " + showWindowBorders);

            WindowUtility.SetWindowTitle(windowTitle);
            Debug.Log("Set window title to `" + windowTitle + "`");
#endif
            
        }
    }
}