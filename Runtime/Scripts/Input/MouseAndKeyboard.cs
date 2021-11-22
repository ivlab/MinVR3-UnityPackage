using System.Collections.Generic;
using UnityEngine;
using System;

namespace IVLab.MinVR3
{
    /// <summary>
    /// This class translates mouse and keyboard inputs from Unity to VREvents.  It will work with either the
    /// new input system or the legacy input system.  You can name the VREvents whatever you wish, and this can
    /// be a useful way to bind desktop-style input to the same events you expect to recieve when running in VR mode.
    /// </summary>
    [AddComponentMenu("MinVR/Input/Mouse & Keyboard")]
    public class MouseAndKeyboard : MonoBehaviour, IPolledInputDevice
    {
        [Serializable]
        public class KeyToVREventName
        {
            public KeyToVREventName(KeyCode keyCode, string baseEventName)
            {
                key = keyCode;
                name = baseEventName;
            }

            [Tooltip("Code for the keyboard key to listen for.")]
            public KeyCode key;

            [Tooltip("Base name for the VREvents generated when the key is pressed and released.  The actual event " +
                "names will also include an ' UP' or ' DOWN' suffix as appropriate.")] 
            public string name;
        }

        private void OnEnable()
        {
            VREngine.instance.eventManager.AddPolledInputDevice(this);
        }

        private void OnDisable()
        {
            VREngine.instance?.eventManager?.RemovePolledInputDevice(this);
        }

        void Reset()
        {
            m_PointerEventName = "Mouse/Position";
            m_LeftBtnEventName = "Mouse/Left";
            m_MiddleBtnEventName = "Mouse/Middle";
            m_RightBtnEventName = "Mouse/Right";

            // default to adding just the most commonly used keys.  use the editor to add more if needed
            // or to remove unused keys if you want to optimize the code
            m_KeysToVREventNames = new List<KeyToVREventName>();
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Space, "Keyboard/Space"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.UpArrow, "Keyboard/UpArrow"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.DownArrow, "Keyboard/DownArrow"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.LeftArrow, "Keyboard/LeftArrow"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.RightArrow, "Keyboard/RightArrow"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.W, "Keyboard/W"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.A, "Keyboard/A"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.S, "Keyboard/S"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.D, "Keyboard/D"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.X, "Keyboard/X"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Y, "Keyboard/Y"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Z, "Keyboard/Z"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.LeftShift, "Keyboard/LeftShift"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Alpha0, "Keyboard/0"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Alpha1, "Keyboard/1"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Alpha2, "Keyboard/2"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Alpha3, "Keyboard/3"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Alpha4, "Keyboard/4"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Alpha5, "Keyboard/5"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Alpha6, "Keyboard/6"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Alpha7, "Keyboard/7"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Alpha8, "Keyboard/8"));
            m_KeysToVREventNames.Add(new KeyToVREventName(KeyCode.Alpha9, "Keyboard/9"));
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> allEvents = new List<IVREventPrototype>();

            if (m_PointerEventName != "") {
                allEvents.Add(VREventPrototypeVector2.Create(m_PointerEventName));
            }
            if (m_LeftBtnEventName != "") {
                allEvents.Add(VREventPrototype.Create(m_LeftBtnEventName + "/Down"));
                allEvents.Add(VREventPrototype.Create(m_LeftBtnEventName + "/Up"));
            }
            if (m_MiddleBtnEventName != "") {
                allEvents.Add(VREventPrototype.Create(m_MiddleBtnEventName + "/Down"));
                allEvents.Add(VREventPrototype.Create(m_MiddleBtnEventName + "/Up"));
            }
            if (m_RightBtnEventName != "") {
                allEvents.Add(VREventPrototype.Create(m_RightBtnEventName + "/Down"));
                allEvents.Add(VREventPrototype.Create(m_RightBtnEventName + "/Up"));
            }

            foreach (KeyToVREventName k in m_KeysToVREventNames) {
                allEvents.Add(VREventPrototype.Create(k.name + "/Down"));
                allEvents.Add(VREventPrototype.Create(k.name + "/Up"));
            }

            return allEvents;
        }


        public void PollForEvents(ref List<VREvent> eventQueue)
        {
            if (m_PointerEventName != "") {
                Vector2 mousePos = MouseState.Position();
                if (mousePos != m_MouseLastPos) {
                    eventQueue.Add(new VREventVector2(m_PointerEventName, mousePos));
                    m_MouseLastPos = mousePos;
                }
            }

            if (m_LeftBtnEventName != "") {
                if (MouseState.LeftButtonWasPressedThisFrame()) {
                    eventQueue.Add(new VREvent(m_LeftBtnEventName + "/Down"));
                }
                if (MouseState.LeftButtonWasReleasedThisFrame()) {
                    eventQueue.Add(new VREvent(m_LeftBtnEventName + "/Up"));
                }
            }

            if (m_MiddleBtnEventName != "") {
                if (MouseState.MiddleButtonWasPressedThisFrame()) {
                    eventQueue.Add(new VREvent(m_MiddleBtnEventName + "/Down"));
                }
                if (MouseState.MiddleButtonWasReleasedThisFrame()) {
                    eventQueue.Add(new VREvent(m_MiddleBtnEventName + "/Down"));
                }
            }

            if (m_RightBtnEventName != "") {
                if (MouseState.RightButtonWasPressedThisFrame()) {
                    eventQueue.Add(new VREvent(m_RightBtnEventName + "/Down"));
                }
                if (MouseState.RightButtonWasReleasedThisFrame()) {
                    eventQueue.Add(new VREvent(m_RightBtnEventName + "/Up"));
                }
            }


            foreach (KeyToVREventName k in m_KeysToVREventNames) {
                if (KeyboardState.KeyWasPressedThisFrame(k.key)) {
                    eventQueue.Add(new VREvent(k.name + "/Down"));
                }
                if (KeyboardState.KeyWasReleasedThisFrame(k.key)) {
                    eventQueue.Add(new VREvent(k.name + "/Up"));
                }
            }
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

    }

} // namespace
