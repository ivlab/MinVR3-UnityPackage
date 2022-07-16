using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TUIO;

namespace IVLab.MinVR3
{

    [AddComponentMenu("MinVR/Input/Touch TUIO")]
    public class TouchTuio : MonoBehaviour, IVREventProducer, TuioListener
    {
        private void Reset()
        {
            m_DeviceIdString = "Tuio/";
            m_TuioPort = 3333;
            m_FlipYAxis = true;
            m_MaxTouches = 10;
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

        void Start()
        {
            m_TuioClient = new TuioClient(m_TuioPort);
            m_TuioClient.addTuioListener(this);
            m_TuioClient.connect();
            if (m_TuioClient.isConnected()) {
                Debug.Log("Listening to TUIO messages on port " + m_TuioClient.getPort());
            } else {
                Debug.Log("Unable to connect to TUIO on port " + m_TuioPort);
            }
            m_tuioIdToFinger = new Dictionary<int, int>();
        }

        void Update()
        {
            if ((m_TuioClient != null) && (!m_TuioClient.isConnected())) {
                m_TuioClient.connect();
                if (m_TuioClient.isConnected()) {
                    Debug.Log("Listening to TUIO messages on port " + m_TuioClient.getPort());
                } else {
                    Debug.Log("Unable to connect to TUIO on port " + m_TuioPort);
                }
            }
        }

        void OnDestroy()
        {
            if (m_TuioClient != null) {
                m_TuioClient.removeTuioListener(this);
                m_TuioClient.disconnect();
            }
        }

        int GetFingerID(int tuioID)
        {
            if (m_tuioIdToFinger.ContainsKey(tuioID)) {
                return m_tuioIdToFinger[tuioID];
            } else {
                // find the smallest available finger id, should be a value 0 to the max touches that
                // can be sensed by the hardware
                int fingerId = 0;
                while (m_tuioIdToFinger.ContainsValue(fingerId)) {
                    fingerId++;
                }
                m_tuioIdToFinger.Add(tuioID, fingerId);
                return fingerId;
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


        // ---- Begin Tuio Listener Callback Functions ----

        // CURSORS
        public void addTuioCursor(TuioCursor tcur)
        {
            //Debug.Log("add cur " + tcur.CursorID + " (" + tcur.SessionID + ") " + tcur.X + " " + tcur.Y);
            int fingerId = GetFingerID(tcur.CursorID);
            string baseEventName = GetBaseEventName(fingerId);
            Vector2 pos = new Vector2(tcur.X, tcur.Y);
            if (m_FlipYAxis) {
                pos.y = 1.0f - pos.y;
            }
            VREngine.Instance.eventManager.QueueEvent(new VREventVector2(m_DeviceIdString + baseEventName + "/Down", pos));
        }

        public void updateTuioCursor(TuioCursor tcur)
        {
            //Debug.Log("set cur " + tcur.CursorID + " (" + tcur.SessionID + ") " + tcur.X + " " + tcur.Y + " " + tcur.MotionSpeed + " " + tcur.MotionAccel);
            int fingerId = GetFingerID(tcur.CursorID);
            string baseEventName = GetBaseEventName(fingerId);
            Vector2 pos = new Vector2(tcur.X, tcur.Y);
            if (m_FlipYAxis) {
                pos.y = 1.0f - pos.y;
            }
            VREngine.Instance.eventManager.QueueEvent(new VREventVector2(m_DeviceIdString + baseEventName + "/Position", pos));
        }

        public void removeTuioCursor(TuioCursor tcur)
        {
            //Debug.Log("del cur " + tcur.CursorID + " (" + tcur.SessionID + ")");
            int fingerId = GetFingerID(tcur.CursorID);
            string baseEventName = GetBaseEventName(fingerId);
            Vector2 pos = new Vector2(tcur.X, tcur.Y);
            if (m_FlipYAxis) {
                pos.y = 1.0f - pos.y;
            }
            VREngine.Instance.eventManager.QueueEvent(new VREventVector2(m_DeviceIdString + baseEventName + "/Up", pos));
        }


        // BLOBS NOT YET SUPPORTED
        public void addTuioBlob(TuioBlob tblb)
        {
            Debug.Log("add blb " + tblb.BlobID + " (" + tblb.SessionID + ") " + tblb.X + " " + tblb.Y + " " + tblb.Angle + " " + tblb.Width + " " + tblb.Height + " " + tblb.Area);
            throw new System.NotImplementedException();
        }

        public void updateTuioBlob(TuioBlob tblb)
        {
            Debug.Log("set blb " + tblb.BlobID + " (" + tblb.SessionID + ") " + tblb.X + " " + tblb.Y + " " + tblb.Angle + " " + tblb.Width + " " + tblb.Height + " " + tblb.Area + " " + tblb.MotionSpeed + " " + tblb.RotationSpeed + " " + tblb.MotionAccel + " " + tblb.RotationAccel);
            throw new System.NotImplementedException();
        }

        public void removeTuioBlob(TuioBlob tblb)
        {
            Debug.Log("del blb " + tblb.BlobID + " (" + tblb.SessionID + ")");
            throw new System.NotImplementedException();
        }


        // OBJECTS NOT YET SUPPORTED
        public void addTuioObject(TuioObject tobj)
        {
            Debug.Log("add obj " + tobj.SymbolID + " " + tobj.SessionID + " " + tobj.X + " " + tobj.Y + " " + tobj.Angle);
            throw new System.NotImplementedException();
        }

        public void updateTuioObject(TuioObject tobj)
        {
            Debug.Log("set obj " + tobj.SymbolID + " " + tobj.SessionID + " " + tobj.X + " " + tobj.Y + " " + tobj.Angle + " " + tobj.MotionSpeed + " " + tobj.RotationSpeed + " " + tobj.MotionAccel + " " + tobj.RotationAccel);
            throw new System.NotImplementedException();
        }

        public void removeTuioObject(TuioObject tobj)
        {
            Debug.Log("del obj " + tobj.SymbolID + " " + tobj.SessionID);
            throw new System.NotImplementedException();
        }


        public void refresh(TuioTime frameTime)
        {
            //Debug.Log("refresh "+frameTime.getTotalMilliseconds());
        }


        // ---- End Tuio Listener Callback Functions ----

        [Tooltip("Prepended to the name of each VREvent produced")]
        [SerializeField] private string m_DeviceIdString;

        [Tooltip("Port that the TuioServer implementation is running on (usually 3333).")]
        [SerializeField] private int m_TuioPort;

        [Tooltip("Not sure if all TUIO implementations follow the same convention, but it seems to be the case that " +
            "TUIO reports touches with (0,0) in the top-left corner of the sensor and (1,1) in the bottom right. " +
            "Unity's Viewport coordinate system has (0,0) in the bottom left and (1,1) top right. So, flipping the " +
            "Y-axis (i.e., Ynew = 1.0 - Yold) will make TUIO coordinates match Unity viewport coordinates.")]
        [SerializeField] private bool m_FlipYAxis;

        [Tooltip("Max number of simultaneous touches reported by the hardware or the max number you expect to track (usually 10).")]
        [SerializeField] private int m_MaxTouches;

        [Tooltip("Similar to the way you can assign a custom VREvent name to a keyboard keypress event, you can " +
            "also customize the touch event names.  To remap every possible event, the size of the array should " +
            "equal the max number of simultaneous touches supported by the hardware.  A default name will be used " +
            "if the array does not contain enough entries.  Indices are reused and most systems have no way of " +
            "telling whether the exact same finger was lifted and then placed down again, so indices are consistent " +
            "between the DOWN and UP events, but once an UP event is received there is no guarantee that index 0 " +
            "will be assigned to the same physical finger the next time that finger produces a touch.")]
        public string[] m_BaseEventNames;

        private TuioClient m_TuioClient;

        // maps the uid's returned by multi-touch devices to a "finger" id, where fingerIds range from 0 to the
        // max number of simultaneously supported touches on the system.  Finger IDs are consistent while the
        // touch persists, but IDs are reused for later touches.  So, you'll always get the same ID for Position
        // and Pressure events that come between an UP and DOWN event, but after the DOWN event, there is no
        // guarantee that the next time you see an event with the same finger it will correspond to the user's
        // same finger.  Finger 0 might be their index finger for one touch; and it might be their middle finger
        // for the next touch.
        private Dictionary<int, int> m_tuioIdToFinger;
    }

} // namespace
