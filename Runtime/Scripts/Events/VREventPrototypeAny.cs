using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
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
            m_DefineNewPrototypeInEditor = false;
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
            return s_AllEventPrototypes.ContainsKey(name);
        }

        public bool SupportsDataType(string dataTypeName)
        {
            return s_AllEventPrototypes.ContainsKey(dataTypeName);
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
            Debug.Assert(s_AllEventPrototypes.ContainsKey(name), "No event prototype available for type " + name);
            m_DataTypeName = name;
        }

        public void SetEventDataType(string eventDataTypeName)
        {
            Debug.Assert(s_AllEventPrototypes.ContainsKey(eventDataTypeName),
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

        public void SetDefineNewPrototypeInEditor(bool value)
        {
            m_DefineNewPrototypeInEditor = value;
        }


        [SerializeField] private string m_EventName;
        [SerializeField] private string m_DataTypeName;
        [SerializeField] private bool m_DataTypeLocked;

        // if false (default), the editor interface is designed for picking an existing prototype from
        //    the list of event prototypes reported by the event manager.  use this when you want to
        //    be able to select an event that your class should respond to in some way.
        // if true, the editor interface is instead used to define a new event prototype.  use this when
        //    you are creating an event producer and want to define prototypes for the events it produces.
        [SerializeField] private bool m_DefineNewPrototypeInEditor = false;

        [NonSerialized] private static Dictionary<string, IVREventPrototype> s_AllEventPrototypes;


        private void InitAllEventPrototypes()
        {
            if (s_AllEventPrototypes != null)
            {
                return;
            }

            s_AllEventPrototypes = new Dictionary<string, IVREventPrototype>();

            // Get all the VREventPrototype classes from Reflection. This is
            // SLOW, but that's okay because InitAllEventPrototypes is only
            // called infrequently and in-editor. Requirements:
            // - be a class
            // - not be generic
            // - not be this "any" prototype (shouldn't infinitely call this method)
            // - be an IVREventPrototype
            // - not be an IVRCallback
            var assembly = Assembly.GetExecutingAssembly();
            var allProtoTypes = assembly.GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsGenericType &&
                    !t.IsAssignableFrom(this.GetType()) &&
                    typeof(IVREventPrototype).IsAssignableFrom(t) &&
                    !typeof(IVRCallback).IsAssignableFrom(t)
                );

            // Construct an instance of each prototype class
            var protoInstances = allProtoTypes
                .Select((t) => t.GetConstructor(new Type[] {})) // get default constructor
                .Where(c => c != null) // no null constructors
                .Select(c => c.Invoke(new object[] {})) // actually call constructor to get object instance
                .Select(c => c as IVREventPrototype); // convert to prototype

            // Add prototypes for all available data types defined in VREvent
            // (and those that we *actually* have classes for)
            foreach (var kv in VREvent.AvailableDataTypes)
            {
                IVREventPrototype protoInstance = protoInstances.First(p => p?.GetEventDataTypeName() == kv.Key);
                if (protoInstance != null)
                {
                    s_AllEventPrototypes.Add(kv.Key, protoInstance);
                }
            }
        }

        public Dictionary<string, IVREventPrototype> AllEventPrototypes { get => s_AllEventPrototypes; }
    }

} // namespace
