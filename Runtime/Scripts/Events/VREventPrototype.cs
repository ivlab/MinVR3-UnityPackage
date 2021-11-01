using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventPrototype : IVREventPrototype
    {
        // --- Static Constructor for Convenience ---

        static public VREventPrototype Create(string eventName)
        {
            var ep = new VREventPrototype();
            ep.SetEventName(eventName);
            return ep;
        }

        // ---


        public VREventPrototype()
        {
            m_DataTypeName = "";
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
