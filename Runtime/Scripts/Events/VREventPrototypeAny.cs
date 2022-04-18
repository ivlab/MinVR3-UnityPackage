using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventPrototypeAny : IVREventPrototype
    {
        // --- Static Constructors for Convenience ---
        
        static public VREventPrototypeAny Create(string eventName)
        {
            var ep = new VREventPrototypeAny();
            ep.SetEventDataType("");
            ep.SetEventName(eventName);
            return ep;
        }

        static public VREventPrototypeAny Create<T>(string eventName)
        {
            var ep = new VREventPrototypeAny();
            Debug.Assert(ep.SupportsDataType(typeof(T)),
                "VREvents with data type '" + typeof(T).Name + "' are not supported.");
            ep.SetEventDataType(typeof(T));
            ep.SetEventName(eventName);
            return ep;
        }

        // ---

        public VREventPrototypeAny()
        {
            m_DataTypeName = "";
            m_EventName = "";
            m_DataTypeLocked = false;
            InitAllEventPrototypes();
        }

        public IVREventPrototype Clone()
        {
            var ep = new VREventPrototypeAny();
            ep.SetEventDataType(m_DataTypeName);
            ep.SetEventName(m_EventName);
            return ep;
        }

        public bool SupportsDataType(Type dataType)
        {
            string name = "";
            if (dataType != null) {
                name = dataType.Name;
            }
            return m_AllEventPrototypes.ContainsKey(name);
        }

        public bool SupportsDataType(string dataTypeName)
        {
            return m_AllEventPrototypes.ContainsKey(dataTypeName);
        }

        public string GetEventName()
        {
            return m_EventName;
        }

        public void SetEventName(string eventName)
        {
            m_EventName = eventName;
        }

        public string GetEventDataTypeName()
        {
            return m_DataTypeName;
        }

        public void SetEventDataType(Type eventDataType)
        {
            string name = "";
            if (eventDataType != null) {
                name = eventDataType.Name;
            }
            Debug.Assert(m_AllEventPrototypes.ContainsKey(name), "No event prototype available for type " + name);
            m_DataTypeName = name;
        }

        public void SetEventDataType(string eventDataTypeName)
        {
            Debug.Assert(m_AllEventPrototypes.ContainsKey(eventDataTypeName),
                "No event prototype available for type " + eventDataTypeName);
            m_DataTypeName = eventDataTypeName;
        }

        public string GetEventDisplayName()
        {
            return GetEventName() + " (" + GetEventDataTypeName() + ")";
        }

        public bool DataTypeLocked()
        {
            return m_DataTypeLocked;
        }

        public void LockDataType()
        {
            m_DataTypeLocked = true;
        }

        public void UnlockDataType()
        {
            m_DataTypeLocked = false;
        }


        [SerializeField] private string m_EventName;
        [SerializeField] private string m_DataTypeName;
        [SerializeField] private bool m_DataTypeLocked;

        [NonSerialized] private Dictionary<string, IVREventPrototype> m_AllEventPrototypes;


        private void InitAllEventPrototypes()
        {
            m_AllEventPrototypes = new Dictionary<string, IVREventPrototype>();

            m_EventPrototype = new VREventPrototype();
            m_AllEventPrototypes.Add("", m_EventPrototype);

            m_EventPrototypeInt32 = new VREventPrototypeInt();
            m_AllEventPrototypes.Add(typeof(int).Name, m_EventPrototypeInt32);

            m_EventPrototypeSingle = new VREventPrototypeFloat();
            m_AllEventPrototypes.Add(typeof(float).Name, m_EventPrototypeSingle);

            m_EventPrototypeVector2 = new VREventPrototypeVector2();
            m_AllEventPrototypes.Add(typeof(Vector2).Name, m_EventPrototypeVector2);

            m_EventPrototypeVector3 = new VREventPrototypeVector3();
            m_AllEventPrototypes.Add(typeof(Vector3).Name, m_EventPrototypeVector3);

            m_EventPrototypeQuaternion = new VREventPrototypeQuaternion();
            m_AllEventPrototypes.Add(typeof(Quaternion).Name, m_EventPrototypeQuaternion);

            // Location 1 of 2 to edit when adding a new event data type.
        }

        [SerializeField] private VREventPrototype m_EventPrototype;
        [SerializeField] private VREventPrototypeInt m_EventPrototypeInt32;
        [SerializeField] private VREventPrototypeFloat m_EventPrototypeSingle;
        [SerializeField] private VREventPrototypeVector2 m_EventPrototypeVector2;
        [SerializeField] private VREventPrototypeVector3 m_EventPrototypeVector3;
        [SerializeField] private VREventPrototypeQuaternion m_EventPrototypeQuaternion;

        // Location 2 of 2 to edit when adding a new event data type.

        public Dictionary<string, IVREventPrototype> AllEventPrototypes { get => m_AllEventPrototypes; }
    }

} // namespace
