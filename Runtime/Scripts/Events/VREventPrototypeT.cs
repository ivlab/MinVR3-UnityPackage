using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventPrototypeT<T> : IVREventPrototype
    {
        // Type-Specific Static Constructors should be implemented in VREvent_<DataTypeName>.cs files.
        // It is recommended to use the type-specific subclasses (i.e., use VRCallbackInt rather than
        // the generic version VRCallback<int>) in your code because Unity's serializer has difficulty
        // correctly serializing and deserializing generic types in some instances.

        public VREventPrototypeT()
        {
            m_DataTypeName = typeof(T).Name;
            m_EventName = "";
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

        public string GetEventDisplayName()
        {
            return GetEventName() + " (" + GetEventDataTypeName() + ")";
        }


        [SerializeField] private string m_EventName;
        [SerializeField] private string m_DataTypeName;
    }

} // namespace
