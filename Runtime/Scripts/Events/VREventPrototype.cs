using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

namespace IVLab.MinVR3
{

    public interface IVREventPrototype
    {
        public string GetEventName();
        public void SetEventName(string eventName);
        public string GetDataTypeName();
        public string GetDisplayName();
        public bool Matches(VREvent vrEvent);
        public bool Matches(IVRCallback callback);
    }


    [Serializable]
    public class VREventPrototype : IVREventPrototype
    {
        public VREventPrototype()
        {
            m_DataTypeName = "";
            m_EventName = "";
        }

        public VREventPrototype(string eventName)
        {
            m_DataTypeName = "";
            m_EventName = eventName;
        }

        public string eventName {
            get => m_EventName;
            set => m_EventName = value;
        }

        public string dataTypeName {
            get => m_DataTypeName;
        }

        public string GetEventName()
        {
            return m_EventName;
        }

        public void SetEventName(string eventName)
        {
            m_EventName = eventName;
        }

        public string GetDataTypeName()
        {
            return m_DataTypeName;
        }

        public string GetDisplayName()
        {
            return GetEventName() + "(" + GetDataTypeName() + ")";
        }

        public bool Matches(VREvent vrEvent)
        {
            return (m_EventName == vrEvent.name) && (m_DataTypeName == vrEvent.GetDataTypeName());
        }

        public bool Matches(IVRCallback callback)
        {
            return (m_DataTypeName == callback.GetDataTypeName());
        }

        [SerializeField] private string m_EventName;
        [SerializeField] private string m_DataTypeName;
    }


    [Serializable]
    public class VREventPrototype<T> : IVREventPrototype
    {
        public VREventPrototype()
        {
            m_DataTypeName = typeof(T).Name;
            m_EventName = "";
        }

        public VREventPrototype(string eventName)
        {
            m_DataTypeName = typeof(T).Name;
            m_EventName = eventName;
        }

        public string eventName {
            get => m_EventName;
            set => m_EventName = value;
        }

        public string dataTypeName {
            get => m_DataTypeName;
        }


        public string GetEventName()
        {
            return m_EventName;
        }

        public void SetEventName(string eventName)
        {
            m_EventName = eventName;
        }

        public string GetDataTypeName()
        {
            return m_DataTypeName;
        }

        public string GetDisplayName()
        {
            return GetEventName() + "(" + GetDataTypeName() + ")";
        }

        public bool Matches(VREvent vrEvent)
        {
            return (m_EventName == vrEvent.name) && (m_DataTypeName == vrEvent.GetDataTypeName());
        }

        public bool Matches(IVRCallback callback)
        {
            return (m_DataTypeName == callback.GetDataTypeName());
        }

        [SerializeField] private string m_EventName;
        [SerializeField] private string m_DataTypeName;
    }


    [Serializable]
    public class VREventPrototypeAny : IVREventPrototype
    {
        public VREventPrototypeAny()
        {
            m_DataTypeName = "";
            m_EventName = "";
            InitAllEventPrototypes();
        }

        public VREventPrototypeAny(string eventName, Type dataType)
        {
            if (dataType == null) {
                m_DataTypeName = "";
            } else {
                m_DataTypeName = dataType.Name;
            }
            m_EventName = eventName;
            InitAllEventPrototypes();
        }

        public string eventName {
            get => m_EventName;
            set => m_EventName = value;
        }

        public string dataTypeName {
            get => m_DataTypeName;
        }

        public string GetEventName()
        {
            return m_EventName;
        }

        public void SetEventName(string eventName)
        {
            m_EventName = eventName;
        }

        public string GetDataTypeName()
        {
            return m_DataTypeName;
        }

        public void SetDataType(Type eventDataType)
        {
            string name = "";
            if (eventDataType != null) {
                name = eventDataType.Name;
            }
            Debug.Assert(m_AllEventPrototypes.ContainsKey(name), "No event prototype available for type " + name);
            m_DataTypeName = name;
        }

        public void SetDataType(string eventDataTypeName)
        {
            Debug.Assert(m_AllEventPrototypes.ContainsKey(eventDataTypeName),
                "No event prototype available for type " + eventDataTypeName);
            m_DataTypeName = eventDataTypeName;
        }

        public string GetDisplayName()
        {
            return GetEventName() + "(" + GetDataTypeName() + ")";
        }

        public bool Matches(VREvent vrEvent)
        {
            return (m_EventName == vrEvent.name) && (m_DataTypeName == vrEvent.GetDataTypeName());
        }

        public bool Matches(IVRCallback callback)
        {
            return (m_DataTypeName == callback.GetDataTypeName());
        }


        private void InitAllEventPrototypes()
        {
            m_AllEventPrototypes = new Dictionary<string, IVREventPrototype>();

            m_EventPrototype = new VREventPrototype();
            m_AllEventPrototypes.Add("", m_EventPrototype);

            m_EventPrototypeInt = new VREventPrototype<int>();
            m_AllEventPrototypes.Add(typeof(int).Name, m_EventPrototypeInt);

            m_EventPrototypeFloat = new VREventPrototype<float>();
            m_AllEventPrototypes.Add(typeof(float).Name, m_EventPrototypeFloat);

            m_EventPrototypeVector2 = new VREventPrototype<Vector2>();
            m_AllEventPrototypes.Add(typeof(Vector2).Name, m_EventPrototypeVector2);

            m_EventPrototypeVector3 = new VREventPrototype<Vector3>();
            m_AllEventPrototypes.Add(typeof(Vector3).Name, m_EventPrototypeVector3);

            m_EventPrototypeQuaternion = new VREventPrototype<Quaternion>();
            m_AllEventPrototypes.Add(typeof(Quaternion).Name, m_EventPrototypeQuaternion);

            m_EventPrototypeTouch = new VREventPrototype<Touch>();
            m_AllEventPrototypes.Add(typeof(Touch).Name, m_EventPrototypeTouch);
        }


        [SerializeField] private VREventPrototype m_EventPrototype;
        [SerializeField] private VREventPrototype<int> m_EventPrototypeInt;
        [SerializeField] private VREventPrototype<float> m_EventPrototypeFloat;
        [SerializeField] private VREventPrototype<Vector2> m_EventPrototypeVector2;
        [SerializeField] private VREventPrototype<Vector3> m_EventPrototypeVector3;
        [SerializeField] private VREventPrototype<Quaternion> m_EventPrototypeQuaternion;
        [SerializeField] private VREventPrototype<Touch> m_EventPrototypeTouch;
        private Dictionary<string, IVREventPrototype> m_AllEventPrototypes;

        [SerializeField] private string m_EventName;
        [SerializeField] private string m_DataTypeName;
    }

} // namespace
