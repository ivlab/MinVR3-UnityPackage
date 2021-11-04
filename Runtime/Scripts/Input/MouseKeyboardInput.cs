using System.Collections.Generic;
using UnityEngine;
using System;


#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace IVLab.MinVR3
{
    /// <summary>
    /// This class translates mouse and keyboard inputs from Unity to VREvents.  It will work with either the
    /// new input system or the legacy input system.  You can name the VREvents whatever you wish, and this can
    /// be a useful way to bind desktop-style input to the same events you expect to recieve when running in VR mode.
    /// </summary>
    [AddComponentMenu("MinVR/Input/Mouse & Keyboard Input")]
    [DefaultExecutionOrder(-998)] // make sure this script runs right before VREngine.cs
    public class MouseKeyboardInput : MonoBehaviour, IVREventProducer
    {
        [Serializable]
        public class KeyToVREventName
        {
            [Tooltip("Code for the keyboard key to listen for.")]
            #if ENABLE_INPUT_SYSTEM
              public Key key;
            #else
              public KeyCode key;
            #endif

            [Tooltip("Base name for the VREvents generated when the key is pressed and released.  The actual event " +
                "names will also include an ' UP' or ' DOWN' suffix as appropriate.")] 
            public string name;
        }

        void Reset()
        {
            m_PointerEventName = "";
            m_LeftBtnEventName = "";
            m_MiddleBtnEventName = "";
            m_RightBtnEventName = "";
            m_KeysToVREventNames = null;        
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> allEvents = new List<IVREventPrototype>();

            if (m_PointerEventName != "") {
                allEvents.Add(VREventPrototypeVector2.Create(m_PointerEventName));
            }
            if (m_LeftBtnEventName != "") {
                allEvents.Add(VREventPrototype.Create(m_LeftBtnEventName + " DOWN"));
                allEvents.Add(VREventPrototype.Create(m_LeftBtnEventName + " UP"));
            }
            if (m_MiddleBtnEventName != "") {
                allEvents.Add(VREventPrototype.Create(m_MiddleBtnEventName + " DOWN"));
                allEvents.Add(VREventPrototype.Create(m_MiddleBtnEventName + " UP"));
            }
            if (m_RightBtnEventName != "") {
                allEvents.Add(VREventPrototype.Create(m_RightBtnEventName + " DOWN"));
                allEvents.Add(VREventPrototype.Create(m_RightBtnEventName + " UP"));
            }

            foreach (KeyToVREventName k in m_KeysToVREventNames) {
                allEvents.Add(VREventPrototype.Create(k.name + " DOWN"));
                allEvents.Add(VREventPrototype.Create(k.name + " UP"));
            }

            return allEvents;
        }


        void Update()
        {

#if ENABLE_INPUT_SYSTEM
            if (m_PointerEventName != "") {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (mousePos != m_MouseLastPos) {
                    VREngine.instance.eventManager.QueueEvent(m_PointerEventName, mousePos);
                    m_MouseLastPos = mousePos;
                }
            }

            if (m_LeftBtnEventName != "") {
                if (Mouse.current.leftButton.wasPressedThisFrame) {
                    VREngine.instance.eventManager.QueueEvent(m_LeftBtnEventName + " DOWN");
                }
                if (Mouse.current.leftButton.wasReleasedThisFrame) {
                    VREngine.instance.eventManager.QueueEvent(m_LeftBtnEventName + " UP");
                }
            }

            if (m_MiddleBtnEventName != "") {
                if (Mouse.current.middleButton.wasPressedThisFrame) {
                    VREngine.instance.eventManager.QueueEvent(m_MiddleBtnEventName + " DOWN");
                }
                if (Mouse.current.middleButton.wasReleasedThisFrame) {
                    VREngine.instance.eventManager.QueueEvent(m_MiddleBtnEventName + " DOWN");
                }
            }

            if (m_RightBtnEventName != "") {
                if (Mouse.current.rightButton.wasPressedThisFrame) {
                    VREngine.instance.eventManager.QueueEvent(m_RightBtnEventName + " DOWN");
                }
                if (Mouse.current.rightButton.wasReleasedThisFrame) {
                    VREngine.instance.eventManager.QueueEvent(m_RightBtnEventName + " UP");
                }
            }


            foreach (KeyToVREventName k in m_KeysToVREventNames) {
                if (Keyboard.current[k.key].wasPressedThisFrame) {
                    VREngine.instance.eventManager.QueueEvent(k.name + " DOWN");
                }
                if (((KeyControl)Keyboard.current[k.key]).wasReleasedThisFrame) {
                    VREngine.instance.eventManager.QueueEvent(k.name + " UP");
                }
            }
#else
            if (m_PointerEventName != "") {
                Vector2 mousePos = Input.mousePosition;
                if (mousePos != m_MouseLastPos) {
                    VREngine.instance.eventManager.QueueEvent(m_PointerEventName, mousePos);
                    m_MouseLastPos = mousePos;
                }
            }

            if (m_LeftBtnEventName != "") {
                if (Input.GetMouseButtonDown(0)) {
                    VREngine.instance.eventManager.QueueEvent(m_LeftBtnEventName + " DOWN");
                }
                if (Input.GetMouseButtonUp(0)) {
                    VREngine.instance.eventManager.QueueEvent(m_LeftBtnEventName + " UP");
                }
            }

            if (m_MiddleBtnEventName != "") {
                if (Input.GetMouseButtonDown(1)) {
                    VREngine.instance.eventManager.QueueEvent(m_MiddleBtnEventName + " DOWN");
                }
                if (Input.GetMouseButtonUp(1)) {
                    VREngine.instance.eventManager.QueueEvent(m_MiddleBtnEventName + " DOWN");
                }
            }

            if (m_RightBtnEventName != "") {
                if (Input.GetMouseButtonDown(2)) {
                    VREngine.instance.eventManager.QueueEvent(m_RightBtnEventName + " DOWN");
                }
                if (Input.GetMouseButtonUp(2)) {
                    VREngine.instance.eventManager.QueueEvent(m_RightBtnEventName + " UP");
                }
            }


            foreach (KeyToVREventName k in m_KeysToVREventNames) {
                if (Input.GetKeyDown(k.key)) {
                    VREngine.instance.eventManager.QueueEvent(k.name + " DOWN");
                }
                if (Input.GetKeyUp(k.key)) {
                    VREngine.instance.eventManager.QueueEvent(k.name + " UP");
                }
            }
#endif
        }


        [Header("Mouse")]
        [Tooltip("Name of the VREvent to generate for mouse pointer movement.")]
        [SerializeField] private string m_PointerEventName;
        [Tooltip("Name of the VREvent to generate for mouse left button up/down.")]
        [SerializeField] private string m_LeftBtnEventName;
        [Tooltip("Name of the VREvent to generate for mouse middle button up/down.")]
        [SerializeField] private string m_MiddleBtnEventName;
        [Tooltip("Name of the VREvent to generate for mouse right button up/down.")]
        [SerializeField] private string m_RightBtnEventName;
        private Vector2 m_MouseLastPos = new Vector2(float.MaxValue, float.MaxValue);

        [Header("Keyboard")]
        [Tooltip("For each named keyboard key, define the name of the VREvent to generate on keyup/keydown.")]
        [SerializeField] private List<KeyToVREventName> m_KeysToVREventNames;

        // for reference -- these are the possible key names used by Unity
        public readonly static string[] KeyNames =
        {
            "backspace",
            "delete",
            "tab",
            "clear",
            "return",
            "pause",
            "escape",
            "space",

            "[0]",
            "[1]",
            "[2]",
            "[3]",
            "[4]",
            "[5]",
            "[6]",
            "[7]",
            "[8]",
            "[9]",
            "[.]",
            "[/]",
            "[*]",
            "[-]",
            "[+]",
            "equals",
            "enter",

            "up",
            "down",
            "right",
            "left",

            "insert",
            "home",
            "end",
            "page up",
            "page down",

            "f1",
            "f2",
            "f3",
            "f4",
            "f5",
            "f6",
            "f7",
            "f8",
            "f9",
            "f10",
            "f11",
            "f12",
            "f13",
            "f14",
            "f15",

            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "-",
            "=",

            "!",
            "@",
            "#",
            "$",
            "%",
            "^",
            "&",
            "*",
            "(",
            ")",
            "_",
            "+",

            "[",
            "]",
            "`",
            "{",
            "}",
            "~",

            ";",
            "'",
            "\\",
            ":",
            "\"",
            "|",

            ",",
            ".",
            "/",
            "<",
            ">",
            "?",

            "a",
            "b",
            "c",
            "d",
            "e",
            "f",
            "g",
            "h",
            "i",
            "j",
            "k",
            "l",
            "m",
            "n",
            "o",
            "p",
            "q",
            "r",
            "s",
            "t",
            "u",
            "v",
            "w",
            "x",
            "y",
            "z",

            "numlock",
            "caps lock",
            "scroll lock",
            "right shift",
            "left shift",
            "right ctrl",
            "left ctrl",
            "right alt",
            "left alt",
            "right cmd",
            "left cmd",
            "right super",
            "left super",
            "alt gr",

            "compose",
            "help",
            "print screen",
            "sys req",
            "break",
            "menu",
            "power",
            "euro",
            "undo",
        };
    }

} // namespace
