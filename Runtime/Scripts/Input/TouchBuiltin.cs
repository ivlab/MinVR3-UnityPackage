using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IVLab.MinVR3
{
    /// <summary>
    /// Converts touch events from Unity's built-in touch system to VREvents.
    /// </summary>
    [AddComponentMenu("MinVR/Input/Touch (Unity Built-in Sensing)")]
    public class TouchBuiltin : MonoBehaviour, IPolledInputDevice
    {

        private void Reset()
        {
            m_DeviceIdString = "Touchscreen/";
            m_BaseEventNames = new[] {
                "Touch 0",
                "Touch 1",
                "Touch 2",
                "Touch 3",
                "Touch 4",
                "Touch 5",
                "Touch 6",
                "Touch 7",
                "Touch 8",
                "Touch 9"
            };
            m_ForceLegacyInput = true;
            m_IncludePressureEvents = true;
        }

        private void OnEnable()
        {
            VREngine.instance.eventManager.AddPolledInputDevice(this);
        }

        private void OnDisable()
        {
            VREngine.instance?.eventManager?.RemovePolledInputDevice(this);
        }


        void Start()
        {
            m_uidToFinger = new Dictionary<int, int>();
        }


        public void PollForEvents(ref List<VREvent> eventQueue)
        {
            int touchCount = TouchscreenState.GetTouchCount(m_ForceLegacyInput);
            if (touchCount > 0) {
                for (int i = 0; i < touchCount; i++) {
                    int uid = TouchscreenState.GetTouchID(i, m_ForceLegacyInput);
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

                    string baseName = "Touch " + fingerId;
                    if (fingerId < m_BaseEventNames.Length) {
                        baseName = m_BaseEventNames[fingerId];
                    }

                    switch (TouchscreenState.GetTouchPhase(i, m_ForceLegacyInput)) {
                        case TouchscreenState.TouchInputPhase.Began:
                            eventQueue.Add(new VREventVector2(m_DeviceIdString + baseName + "/Down", TouchscreenState.GetTouchPosition(i, m_ForceLegacyInput)));
                            eventQueue.Add(new VREventVector2(m_DeviceIdString + baseName + "/Position", TouchscreenState.GetTouchPosition(i, m_ForceLegacyInput)));
                            break;

                        case TouchscreenState.TouchInputPhase.Moved:
                            eventQueue.Add(new VREventVector2(m_DeviceIdString + baseName + "/Position", TouchscreenState.GetTouchPosition(i, m_ForceLegacyInput)));
                            if (m_IncludePressureEvents) {
                                eventQueue.Add(new VREventFloat(m_DeviceIdString + baseName + "/Pressure", TouchscreenState.GetTouchPressure(i, m_ForceLegacyInput)));
                            }
                            break;

                        // Report that a direction has been chosen when the finger is lifted.
                        case TouchscreenState.TouchInputPhase.Ended:
                            eventQueue.Add(new VREventVector2(m_DeviceIdString + baseName + "/Up", TouchscreenState.GetTouchPosition(i, m_ForceLegacyInput)));
                            m_uidToFinger.Remove(uid);
                            break;
                    }
                }
            }
        }


        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();
            for (int i = 0; i < m_BaseEventNames.Length; i++) {
                eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + m_BaseEventNames[i] + "/Down"));
                eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + m_BaseEventNames[i] + "/Position"));
                if (m_IncludePressureEvents) {
                    eventsProduced.Add(VREventPrototypeFloat.Create(m_DeviceIdString + m_BaseEventNames[i] + "/Pressure"));
                }
                eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + m_BaseEventNames[i] + "/Up"));
            }

            return eventsProduced;
        }


        [Tooltip("Prepended to the name of each VREvent produced")]
        [SerializeField] private string m_DeviceIdString;


        [Tooltip("If true, each position update event will be followed immediately by a pressure update event.")]
        public bool m_IncludePressureEvents = true;

        [Tooltip("Similar to the way you can assign a custom VREvent name to a keyboard keypress event, you can " +
            "also customize the touch event names.  To remap every possible event, the size of the array should " +
            "equal the max number of simultaneous touches supported by the hardware.  A default name will be used " +
            "if the array does not contain enough entries.  Indices are reused and most systems have no way of " +
            "telling whether the exact same finger was lifted and then placed down again, so indices are consistent " +
            "between the DOWN and UP events, but once an UP event is received there is no guarantee that index 0 " +
            "will be assigned to the same physical finger the next time that finger produces a touch.")]
        public string[] m_BaseEventNames;


        // maps the uid's returned by multi-touch devices to a "finger" id, where fingerIds range from 0 to the
        // max number of simultaneously supported touches on the system.  Finger IDs are consistent while the
        // touch persists, but IDs are reused for later touches.  So, you'll always get the same ID for Position
        // and Pressure events that come between an UP and DOWN event, but after the DOWN event, there is no
        // guarantee that the next time you see an event with the same finger it will correspond to the user's
        // same finger.  Finger 0 might be their index finger for one touch; and it might be their middle finger
        // for the next touch.
        private Dictionary<int, int> m_uidToFinger;

        [Tooltip("If true, will use the Unity's Legacy InputModule even if the New Input System is available " +
            "this may be needed on mobile devices or other platforms where the new input system is not yet " +
            "fully functional")]
        public bool m_ForceLegacyInput = true;
    }

} // namespace
