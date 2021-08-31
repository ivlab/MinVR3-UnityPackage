using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace IVLab.MinVR3 {
    /// <summary>
    /// A virtual input device that pipes input from Unity's New Input System
    /// to MinVR, creating new VREvents when Unity InputActions are performed.
    /// </summary>
    [AddComponentMenu("MinVR/Input/UnityInputActions (Virtual Input Device)")]
    public class UnityInputActions : MonoBehaviour, IVREventProducer
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
            m_InputActionAssets = new List<InputActionAsset>();
        }


        protected void OnEnable()
        {
            if (Application.IsPlaying(this)) {
                VREngine.instance.eventManager.AddEventProducer(this);
                EnableUnityInput();
            }
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
                VREngine.instance?.eventManager?.RemoveEventProducer(this);
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
                    return name + " DOWN";
                } else if (phase == InputActionPhase.Canceled) {
                    return name + " UP";
                }
            } else if (phase == InputActionPhase.Performed) {
                return name;
            }
            return "";
        }


        private void InternalOnActionTriggered(InputAction.CallbackContext context)
        {
            string eventName = GetEventName(context.action, context.phase);
            if (eventName == "") {
                return;
            }

            string expectedDataType = context.action.expectedControlType;
            if ((expectedDataType == "") || (expectedDataType == "Button")) {
                VREngine.instance.eventManager.QueueEvent(eventName);
            } else if (expectedDataType == typeof(int).Name) {
                VREngine.instance.eventManager.QueueEvent(eventName, context.ReadValue<int>());
            } else if (expectedDataType == typeof(float).Name) {
                VREngine.instance.eventManager.QueueEvent(eventName, context.ReadValue<float>());
            } else if (expectedDataType == typeof(Vector2).Name) {
                VREngine.instance.eventManager.QueueEvent(eventName, context.ReadValue<Vector2>());
            } else if (expectedDataType == typeof(Vector3).Name) {
                VREngine.instance.eventManager.QueueEvent(eventName, context.ReadValue<Vector3>());
            } else if (expectedDataType == typeof(Quaternion).Name) {
                VREngine.instance.eventManager.QueueEvent(eventName, context.ReadValue<Quaternion>());
            } else if (expectedDataType == typeof(Touch).Name) {
                VREngine.instance.eventManager.QueueEvent(eventName, context.ReadValue<Touch>());
            } else {
                Debug.Log($"Not queueing event '{eventName}', which has an unrecognized expected data type = '" + expectedDataType + "'");
            }            
        }


        public Dictionary<string, string> GetEventNamesAndTypes()
        {
            Dictionary<string, string> eventsProduced = new Dictionary<string, string>();
            foreach (var actionAsset in m_InputActionAssets) {
                if (actionAsset != null) {
                    foreach (var actionMap in actionAsset.actionMaps) {
                        foreach (var action in actionMap) {
                            if (action.type == InputActionType.Button) {
                                eventsProduced.Add(GetEventName(action, InputActionPhase.Started), "");
                                eventsProduced.Add(GetEventName(action, InputActionPhase.Canceled), "");
                            } else {
                                if (action.expectedControlType == "") {
                                    eventsProduced.Add(GetEventName(action, InputActionPhase.Performed), "");
                                } else if (action.expectedControlType == "Integer") {
                                    eventsProduced.Add(GetEventName(action, InputActionPhase.Performed), typeof(int).Name);
                                } else if ((action.expectedControlType == "Analog") || (action.expectedControlType == "Axis")) {
                                    eventsProduced.Add(GetEventName(action, InputActionPhase.Performed), typeof(float).Name);
                                } else if ((action.expectedControlType == "Vector2") || (action.expectedControlType == "Stick") || (action.expectedControlType == "Dpad")) {
                                    eventsProduced.Add(GetEventName(action, InputActionPhase.Performed), typeof(Vector2).Name);
                                } else if (action.expectedControlType == "Vector3") {
                                    eventsProduced.Add(GetEventName(action, InputActionPhase.Performed), typeof(Vector3).Name);
                                } else if (action.expectedControlType == "Quaternion") {
                                    eventsProduced.Add(GetEventName(action, InputActionPhase.Performed), typeof(Quaternion).Name);
                                } else if (action.expectedControlType == "Touch") {
                                    eventsProduced.Add(GetEventName(action, InputActionPhase.Performed), typeof(Touch).Name);
                                } else {
                                    Debug.Log($"Unity Action '{action.name}' has an unrecognized expected control type of '" + action.expectedControlType + "'");
                                }
                            }
                        }
                    }
                }
            }
            return eventsProduced;
        }

        [Tooltip("One or more Unity InputActionAssets that provide user input to the MinVRInput system")]
        [SerializeField] private List<InputActionAsset> m_InputActionAssets;

    }

} // namespace