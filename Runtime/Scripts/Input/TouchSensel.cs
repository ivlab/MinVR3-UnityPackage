using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sensel;

namespace IVLab.MinVR3
{
    [AddComponentMenu("MinVR/Input/Touch Sensel")]
    public class TouchSensel : MonoBehaviour, IVREventProducer
    {
        private void Reset()
        {
            m_DeviceIdString = "Sensel/";
            m_MaxTouches = 10;
            m_NormalizeTouchCoords = true;
            m_FlipYAxis = true;
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
        }

        void Awake()
        {
            SenselDeviceList list = new SenselDeviceList();
            list.num_devices = 0;
            Sensel.Sensel.senselGetDeviceList(ref list);
            if (list.num_devices != 0)
            {
                Sensel.Sensel.senselOpenDeviceByID(ref _handle, list.devices[0].idx);
            }
            if (_handle != IntPtr.Zero)
            {
                Sensel.Sensel.senselGetSensorInfo(_handle, ref _sensor_info);
                string serialNum = System.Text.Encoding.Default.GetString(list.devices[0].serial_num).Replace("\0", String.Empty);
                Debug.Log($"Sensel Device: {serialNum} -- Width: {_sensor_info.width}mm, Height: {_sensor_info.height}mm");
                Sensel.Sensel.senselSetFrameContent(_handle, 0x04); // FRAME_CONTENT_CONTACTS_MASK
                Sensel.Sensel.senselAllocateFrameData(_handle, _frame);
                Sensel.Sensel.senselStartScanning(_handle);

                activeContacts = new Dictionary<int, SenselContact>();
                previousContacts = new Dictionary<int, SenselContact>();
            }
        }

        void Start()
        {
        }

        void Update()
        {
            if (_handle != IntPtr.Zero)
            {
                Int32 num_frames = 0;
                Sensel.Sensel.senselReadSensor(_handle);
                Sensel.Sensel.senselGetNumAvailableFrames(_handle, ref num_frames);

                for (int f = 0; f < num_frames; ++f)
                {
                    Sensel.Sensel.senselGetFrame(_handle, _frame);

                    activeContacts = new Dictionary<int, SenselContact>();
                    for (int contactNum = 0; contactNum < _frame.n_contacts; contactNum++)
                    {
                        int contactId = _frame.contacts[contactNum].id;
                        activeContacts[contactId] = _frame.contacts[contactNum];
                    }

                    int maxContactId = 0;
                    if (activeContacts.Count > 0 && previousContacts.Count > 0)
                        maxContactId = Math.Max(activeContacts.Keys.Max(), previousContacts.Keys.Max());
                    else if (activeContacts.Count > 0)
                        maxContactId = activeContacts.Keys.Max();
                    else if (previousContacts.Count > 0)
                        maxContactId = previousContacts.Keys.Max();

                    for (int contactId = 0; contactId < maxContactId + 1; contactId++)
                    {
                        string touchEvent = null;
                        Vector2 pos = Vector2.zero;
                        if (activeContacts.ContainsKey(contactId) && previousContacts.ContainsKey(contactId))
                        {
                            touchEvent = "Position";
                            pos = new Vector2(activeContacts[contactId].x_pos, activeContacts[contactId].y_pos);
                        }
                        else if (activeContacts.ContainsKey(contactId) && !previousContacts.ContainsKey(contactId))
                        {
                            touchEvent = "Down";
                            pos = new Vector2(activeContacts[contactId].x_pos, activeContacts[contactId].y_pos);
                        }
                        else if (!activeContacts.ContainsKey(contactId) && previousContacts.ContainsKey(contactId))
                        {
                            touchEvent = "Up";
                            pos = new Vector2(previousContacts[contactId].x_pos, previousContacts[contactId].y_pos);
                        }

                        if (touchEvent != null)
                        {
                            float maxY = 1.0f;
                            if (m_NormalizeTouchCoords)
                            {
                                pos.x /= _sensor_info.width;
                                pos.y /= _sensor_info.height;
                            }
                            else
                            {
                                maxY = _sensor_info.height;
                            }

                            if (m_FlipYAxis)
                                pos.y = maxY - pos.y;

                            string baseEventName = GetBaseEventName(contactId);
                            string minVrString = m_DeviceIdString + baseEventName + "/" + touchEvent;
                            VREngine.instance.eventManager.QueueEvent(new VREventVector2(minVrString, pos));
                        }
                    }

                    previousContacts = activeContacts;
                }
            }
        }

        void OnDestroy()
        {
            if( _handle != IntPtr.Zero ) {
                Sensel.Sensel.senselStopScanning( _handle );
                Sensel.Sensel.senselClose( _handle );
            }
        }

        string GetBaseEventName(int fingerId)
        {
            string baseName = "Touch " + fingerId;
            if (fingerId < m_BaseEventNames.Length) {
                baseName = m_BaseEventNames[fingerId];
            }
            return baseName;
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();
            for (int i = 0; i < m_MaxTouches; i++) {
                eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + m_BaseEventNames[i] + "/Down"));
                eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + m_BaseEventNames[i] + "/Position"));
                eventsProduced.Add(VREventPrototypeVector2.Create(m_DeviceIdString + m_BaseEventNames[i] + "/Up"));
            }
            return eventsProduced;
        }

        [Tooltip("Prepended to the name of each VREvent produced")]
        [SerializeField] private string m_DeviceIdString;

        [Tooltip("Max number of simultaneous touches reported by the hardware or the max number you expect to track (usually 10).")]
        [SerializeField] private int m_MaxTouches;

        [Tooltip("The Sensel reports touches with (0,0) in the top-left corner of the sensor and (1,1) in the bottom right. " +
            "Unity's Viewport coordinate system has (0,0) in the bottom left and (1,1) top right. So, flipping the " +
            "Y-axis (i.e., Ynew = 1.0 - Yold) will make Sensel coordinates match Unity viewport coordinates.")]
        [SerializeField] private bool m_FlipYAxis;

        [Tooltip("Sensel reports touches in real-world mm. Uncheck this box to" +
        "use 0-1 normalized coordinates instead (top left is 0, 0)")]
        [SerializeField] private bool m_NormalizeTouchCoords;

        [Tooltip("Similar to the way you can assign a custom VREvent name to a keyboard keypress event, you can " +
            "also customize the touch event names.  To remap every possible event, the size of the array should " +
            "equal the max number of simultaneous touches supported by the hardware.  A default name will be used " +
            "if the array does not contain enough entries.  Indices are reused and most systems have no way of " +
            "telling whether the exact same finger was lifted and then placed down again, so indices are consistent " +
            "between the DOWN and UP events, but once an UP event is received there is no guarantee that index 0 " +
            "will be assigned to the same physical finger the next time that finger produces a touch.")]
        public string[] m_BaseEventNames;

        // Sensel stuff
        IntPtr _handle = IntPtr.Zero;
        SenselSensorInfo _sensor_info;
        SenselFrame _frame = new SenselFrame();
        Dictionary<int, SenselContact> activeContacts;
        Dictionary<int, SenselContact> previousContacts;
    }

} // namespace
