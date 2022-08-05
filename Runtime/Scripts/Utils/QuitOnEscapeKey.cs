using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    public class QuitOnEscapeKey : MonoBehaviour
    {
        void Update()
        {
            if (KeyboardState.KeyWasPressedThisFrame(KeyCode.Escape)) {
                Debug.Log("Escape Key Pressed.  Quitting Applicaiton.");
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #endif
                Application.Quit();
                return;
            }
        }
    }
}
