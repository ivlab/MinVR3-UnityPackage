using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IVLab.MinVR3
{
    /// <summary>
    /// Facade to hide the different strategies for accessing mouse input
    /// depending on whether using the Legacy InputModule or the New Input System.
    /// </summary>
    static public class MouseState
    {
// On Windows, right mouse button seems to be button 1 instead of 2...
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        const int RightMouseButtonNum = 1;
#else
        const int RightMouseButtonNum = 2;
#endif
        

        // position
        static public Vector2 Position()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.position.ReadValue();
#else
            return Input.mousePosition;
#endif
        }


        // left mouse button
        static public bool LeftButtonWasPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        static public bool LeftButtonWasReleasedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.leftButton.wasReleasedThisFrame;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }

        static public bool LeftButtonIsPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.leftButton.isPressed;
#else
            return Input.GetMouseButton(0);
#endif
        }


        // middle mouse button
        static public bool MiddleButtonWasPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.middleButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(1);
#endif
        }

        static public bool MiddleButtonWasReleasedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.middleButton.wasReleasedThisFrame;
#else
            return Input.GetMouseButtonUp(1);
#endif
        }

        static public bool MiddleButtonIsPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.middleButton.isPressed;
#else
            return Input.GetMouseButton(1);
#endif
        }


        // right mouse button
        static public bool RightButtonWasPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.rightButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(RightMouseButtonNum);
#endif
        }

        static public bool RightButtonWasReleasedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.rightButton.wasReleasedThisFrame;
#else
            return Input.GetMouseButtonUp(RightMouseButtonNum);
#endif
        }

        static public bool RightButtonIsPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.rightButton.isPressed;
#else
            return Input.GetMouseButton(RightMouseButtonNum);
#endif
        }
    }

} // end namespace

