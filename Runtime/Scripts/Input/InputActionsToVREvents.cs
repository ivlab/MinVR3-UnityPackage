// This functionality is only available in projects using Unity's New Input System
#if ENABLE_INPUT_SYSTEM

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace IVLab.MinVR3 {

    /// <summary>
    /// A virtual input device that pipes input from Unity's New Input System
    /// to MinVR, creating new VREvents when Unity InputActions are performed.
    /// Unity's "New Input System" is still in a preview state, and it does not currently work for capturing touch input
    /// using the Unity Remote App.  Use the TouchInput class if you need to do that now while we wait for them to
    /// upgrade Unity Remote.
    /// </summary>
    [AddComponentMenu("MinVR/Input/Unity InputActions To VREvents")]
    public class InputActionsToVREvents : MonoBehaviour, IVREventProducer
    {
        /// <summary>
        /// All ActionMaps in these assets are automatically enabled.  These serve as the input sources for
        /// all MinVR inputs.
        /// </summary>
        public List<InputActionAsset> inputActionAssets {
            get => m_InputActionAssets;
            set => m_InputActionAssets = value;
        }

        /// <summary>
        /// Called automatically from MonoBehaviour.OnEnable(); can also call manually together with DisableInput().
        /// </summary>
        public void EnableUnityInput()
        {
            if (m_InputActionAssets != null) {
                foreach (var actionAsset in m_InputActionAssets) {
                    if (actionAsset != null) {
                        foreach (var actionMap in actionAsset.actionMaps) {
                            actionMap.Enable();
                            actionMap.actionTriggered += InternalOnActionTriggered;
                        }
                    }
                }
            }
        }


        void Reset()
        {
            m_DeviceIdString = "InputActions/";
            m_InputActionAssets = new List<InputActionAsset>();
            m_TouchEventNames = new[] {
                "Touch/Finger 0",
                "Touch/Finger 1",
                "Touch/Finger 2",
                "Touch/Finger 3",
                "Touch/Finger 4",
                "Touch/Finger 5",
                "Touch/Finger 6",
                "Touch/Finger 7",
                "Touch/Finger 8",
                "Touch/Finger 9",
          };
        }


        protected void OnEnable()
        {
            if (Application.IsPlaying(this)) {
                EnableUnityInput();
            }
            m_uidToFinger = new Dictionary<int, int>();
        }


        /// <summary>
        /// Called automatically from MonoBehaviour.OnEnable(); can also call manually together with EnableInput().
        /// </summary>
        public void DisableUnityInput()
        {
            if (m_InputActionAssets != null) {
                foreach (var actionAsset in m_InputActionAssets) {
                    if (actionAsset != null) {
                        foreach (var actionMap in actionAsset.actionMaps) {
                            actionMap.Disable();
                            actionMap.actionTriggered -= InternalOnActionTriggered;
                        }
                    }
                }
            }
        }


        protected void OnDisable()
        {
            if (Application.IsPlaying(this)) {
                DisableUnityInput();
            }
        }


        public string GetEventName(InputAction action, InputActionPhase phase)
        {
            string name = action.name;
            if ((action.actionMap != null) && (action.actionMap.name != "")) {
                name = action.actionMap.name + "/" + name;
            }

            if (action.type == InputActionType.Button) {
                if (phase == InputActionPhase.Started) {
                    return name + "/Down";
                } else if (phase == InputActionPhase.Canceled) {
                    return name + "/Up";
                }
            } else if (phase == InputActionPhase.Performed) {
                return name;
            }
            return "";
        }


        private void InternalOnActionTriggered(InputAction.CallbackContext context)
        {
            string expectedDataType = context.action.expectedControlType;

            // Touch requires some processing so handle it differently
            if (expectedDataType == typeof(Touch).Name) {
                string eventName = context.action.name;
                if ((context.action.actionMap != null) && (context.action.actionMap.name != "")) {
                    eventName = context.action.actionMap.name + "/" + eventName;
                }

                // BUG: This looks like a bug in Unity -- the expectedDataType is "Touch" but the
                // actual data carried with the event is of type "TouchStruct".  Our code works, but
                // if they ever fix this, it will break our code ;(
                TouchState touch = context.ReadValue<TouchState>();
                int uid = touch.touchId;
                int fingerId;
                if (m_uidToFinger.ContainsKey(uid)) {
                    fingerId = m_uidToFinger[uid];
                } else {
                    // find the smallest available finger id, should be a value 0 to the max touches that
                    // can be sensed by the hardware
                    fingerId = 0;
                    while (m_uidToFinger.ContainsValue(fingerId)) {
                        fingerId++;
                    }
                    m_uidToFinger.Add(uid, fingerId);
                }

                string baseName = "Touch/Finger " + fingerId;
                if (fingerId < m_TouchEventNames.Length) {
                    baseName = m_TouchEventNames[fingerId];
                }

                switch (touch.phase) {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        VREngine.instance.eventManager.QueueEvent(new VREvent(m_DeviceIdString + baseName + "/Down"));
                        VREngine.instance.eventManager.QueueEvent(new VREventVector2(m_DeviceIdString + baseName + "/Position", touch.position));
                        break;

                    // Determine direction by comparing the current touch position with the initial one.
                    case UnityEngine.InputSystem.TouchPhase.Moved:
                        VREngine.instance.eventManager.QueueEvent(new VREventVector2(m_DeviceIdString + baseName + "/Position", touch.position));
                        if (m_IncludePressureEvents) {
                            VREngine.instance.eventManager.QueueEvent(new VREventFloat(m_DeviceIdString + baseName + "/Pressure", touch.pressure));
                        }
                        break;

                    // Report that a direction has been chosen when the finger is lifted.
                    case UnityEngine.InputSystem.TouchPhase.Ended:
                        VREngine.instance.eventManager.QueueEvent(new VREvent(m_DeviceIdString + baseName + "/Up"));
                        m_uidToFinger.Remove(uid);
                        break;
                }
            }

            // Not a touch event
            else {
                string eventName = GetEventName(context.action, context.phase);
                if (eventName == "") {
                    return;
                }

                if ((expectedDataType == "") || (expectedDataType == "Button")) {
                    VREngine.instance.eventManager.QueueEvent(new VREvent(m_DeviceIdString + eventName));
                } else if (expectedDataType == typeof(int).Name) {
                    VREngine.instance.eventManager.QueueEvent(new VREventInt(m_DeviceIdString + eventName, context.ReadValue<int>()));
                } else if (expectedDataType == typeof(float).Name) {
                    VREngine.instance.eventManager.QueueEvent(new VREventFloat(m_DeviceIdString + eventName, context.ReadValue<float>()));
                } else if (expectedDataType == typeof(Vector2).Name) {
                    VREngine.instance.eventManager.QueueEvent(new VREventVector2(m_DeviceIdString + eventName, context.ReadValue<Vector2>()));
                } else if (expectedDataType == typeof(Vector3).Name) {
                    VREngine.instance.eventManager.QueueEvent(new VREventVector3(m_DeviceIdString + eventName, context.ReadValue<Vector3>()));
                } else if (expectedDataType == typeof(Quaternion).Name) {
                    VREngine.instance.eventManager.QueueEvent(new VREventQuaternion(m_DeviceIdString + eventName, context.ReadValue<Quaternion>()));
                } else {
                    Debug.Log($"Not queueing event '{eventName}', which has an unrecognized expected data type = '" + expectedDataType + "'");
                }
            }
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            bool expectingTouchEvents = false;

            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();
            foreach (var actionAsset in m_InputActionAssets) {
                if (actionAsset != null) {
                    foreach (var actionMap in actionAsset.actionMaps) {
                        foreach (var action in actionMap) {
                            if (action.type == InputActionType.Button) {
                                eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + GetEventName(action, InputActionPhase.Started)));
                                eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + GetEventName(action, InputActionPhase.Canceled)));
                            } else {
                                if (action.expectedControlType == "") {
                                    eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + GetEventName(action, InputActionPhase.Performed)));
                                } else if (action.expectedControlType == "Integer") {
                                    eventsProduced.Add(VREventPrototypeInt.Create(m_DeviceIdString + GetEventName(action, InputActionPhase.Performed)));
                                } else if ((action.expectedControlType == "Analog") || (action.expectedControlType == "Axis")) {
                                    eventsProduced.Add(VREventPrototypeFloat.Create(m_DeviceIdString + GetEventName(action, InputActionPhase.Performed)));
                                } else if ((action.expectedControlType == "Vector2") || (action.expectedControlType == "Stick") || (action.expectedControlType == "Dpad")) {
                                    eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + GetEventName(action, InputActionPhase.Performed)));
                                } else if (action.expectedControlType == "Vector3") {
                                    eventsProduced.Add(VREventPrototypeVector3.Create(m_DeviceIdString + GetEventName(action, InputActionPhase.Performed)));
                                } else if (action.expectedControlType == "Quaternion") {
                                    eventsProduced.Add(VREventPrototypeQuaternion.Create(m_DeviceIdString + GetEventName(action, InputActionPhase.Performed)));
                                } else if (action.expectedControlType == "Touch") {
                                    expectingTouchEvents = true;
                                } else {
                                    Debug.Log($"Unity Action '{action.name}' has an unrecognized expected control type of '" + action.expectedControlType + "'");
                                }
                            }
                        }
                    }
                }
            }

            // The class uses its own logic to generate these per-finger touch events.  They do not show up in the
            // InputActions maps, but we want to add them to the list of possible events if any of the InputActions are
            // 
            if (expectingTouchEvents) {
                for (int i = 0; i < m_TouchEventNames.Length; i++) {
                    eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + m_TouchEventNames[i] + "/Down"));
                    eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + m_TouchEventNames[i] + "/Position"));
                    if (m_IncludePressureEvents) {
                        eventsProduced.Add(VREventPrototypeFloat.Create(m_DeviceIdString + m_TouchEventNames[i] + "/Pressure"));
                    }
                    eventsProduced.Add(VREventPrototype.Create(m_DeviceIdString + m_TouchEventNames[i] + "/Up"));
                }
            }

            return eventsProduced;
        }


        [Tooltip("Prepended to the name of each VREvent produced")]
        [SerializeField] private string m_DeviceIdString = "InputActions/";

        [Header("Input Source(s)")]
        [Tooltip("One or more Unity InputActionAssets that provide user input to the MinVRInput system")]
        [SerializeField] private List<InputActionAsset> m_InputActionAssets;

        [Header("Multi-touch Event Handling")]
        [Tooltip("Similar to the way you can assign a custom VREvent name to a keyboard keypress event, you can " +
            "also customize the touch event names.  To remap every possible event, the size of the array should " +
            "equal the max number of simultaneous touches supported by the hardware.  A default name will be used " +
            "if the array does not contain enough entries.  Indices are reused and most systems have no way of " +
            "telling whether the exact same finger was lifted and then placed down again, so indices are consistent " +
            "between the DOWN and UP events, but once an UP event is received there is no guarantee that index 0 " +
            "will be assigned to the same physical finger the next time that finger produces a touch.")]
        [SerializeField] private string[] m_TouchEventNames;

        [Tooltip("If true, each touch position update event will be followed immediately by a pressure update event.")]
        [SerializeField] private bool m_IncludePressureEvents = true;

        // maps the uid's returned by multi-touch devices to a "finger" id, where fingerIds range from 0 to the
        // max number of simultaneously supported touches on the system.  Finger IDs are consistent while the
        // touch persists, but IDs are reused for later touches.  So, you'll always get the same ID for Position
        // and Pressure events that come between an UP and DOWN event, but after the DOWN event, there is no
        // guarantee that the next time you see an event with the same finger it will correspond to the user's
        // same finger.  Finger 0 might be their index finger for one touch; and it might be their middle finger
        // for the next touch.
        private Dictionary<int, int> m_uidToFinger;

    }

} // namespace

#endif
