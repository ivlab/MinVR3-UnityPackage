using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public static class TouchscreenState
{
    public static int GetTouchCount(bool forceLegacy)
    {
#if ENABLE_INPUT_SYSTEM
        if (!forceLegacy) {
            return Touchscreen.current.touches.Count;
        } else {
            return Input.touchCount;
        }
#else
            return Input.touchCount;
#endif
    }


    public static int GetTouchID(int touchIndex, bool forceLegacy)
    {
#if ENABLE_INPUT_SYSTEM
        if (!forceLegacy) {
            return Touchscreen.current.touches[touchIndex].touchId.ReadValue();
        } else {
            return Input.touches[touchIndex].fingerId;
        }
#else
            return Input.touches[touchIndex].fingerId;
#endif
    }


    public enum TouchInputPhase
    {
        None,
        Began,
        Moved,
        Ended,
        Canceled,
        Stationary
    }

    public static TouchInputPhase GetTouchPhase(int touchIndex, bool forceLegacy)
    {
#if ENABLE_INPUT_SYSTEM
        if (!forceLegacy) {
            switch (Touchscreen.current.touches[touchIndex].phase.ReadValue()) {
                case UnityEngine.InputSystem.TouchPhase.None: return TouchInputPhase.None;
                case UnityEngine.InputSystem.TouchPhase.Began: return TouchInputPhase.Began;
                case UnityEngine.InputSystem.TouchPhase.Moved: return TouchInputPhase.Moved;
                case UnityEngine.InputSystem.TouchPhase.Ended: return TouchInputPhase.Ended;
                case UnityEngine.InputSystem.TouchPhase.Canceled: return TouchInputPhase.Canceled;
                case UnityEngine.InputSystem.TouchPhase.Stationary: return TouchInputPhase.Stationary;
            }
        } else {
            switch (Input.touches[touchIndex].phase) {
                case UnityEngine.TouchPhase.Began: return TouchInputPhase.Began;
                case UnityEngine.TouchPhase.Moved: return TouchInputPhase.Moved;
                case UnityEngine.TouchPhase.Stationary: return TouchInputPhase.Stationary;
                case UnityEngine.TouchPhase.Ended: return TouchInputPhase.Ended;
                case UnityEngine.TouchPhase.Canceled: return TouchInputPhase.Canceled;
            }
        }
#else
            switch (Input.touches[touchIndex].phase) {
                case UnityEngine.TouchPhase.Began: return TouchInputPhase.Began;
                case UnityEngine.TouchPhase.Moved: return TouchInputPhase.Moved;
                case UnityEngine.TouchPhase.Stationary: return TouchInputPhase.Stationary;
                case UnityEngine.TouchPhase.Ended: return TouchInputPhase.Ended;
                case UnityEngine.TouchPhase.Canceled: return TouchInputPhase.Canceled;
            }
#endif
        return TouchInputPhase.None;
    }


    public static Vector2 GetTouchPosition(int touchIndex, bool forceLegacy)
    {
#if ENABLE_INPUT_SYSTEM
        if (!forceLegacy) {
            return Touchscreen.current.touches[touchIndex].position.ReadValue();
        } else {
            return Input.touches[touchIndex].position;
        }
#else
            return Input.touches[touchIndex].position;
#endif
    }

    public static float GetTouchPressure(int touchIndex, bool forceLegacy)
    {
#if ENABLE_INPUT_SYSTEM
        if (!forceLegacy) {
            return Touchscreen.current.touches[touchIndex].pressure.ReadValue();
        } else {
            return Input.touches[touchIndex].pressure / Input.touches[touchIndex].maximumPossiblePressure;
        }
#else
            return Input.touches[touchIndex].pressure / Input.touches[touchIndex].maximumPossiblePressure;
#endif
    }

}
