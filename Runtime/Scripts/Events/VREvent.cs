using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.Serialization;

namespace IVLab.MinVR3
{
    public class VREvent : ISerializable
    {
        public VREvent(string eventName)
        {
            m_Name = eventName;
        }

        public VREvent()
        {
        }

        public string name {
            get => m_Name;
        }

        public string GetName()
        {
            return m_Name;
        }

        public virtual string GetDataTypeName()
        {
            return "";
        }

        public bool Matches(IVREventPrototype eventPrototype)
        {
            return (GetName() == eventPrototype.GetEventName()) &&
                (GetDataTypeName() == eventPrototype.GetEventDataTypeName());
        }

        protected VREvent(SerializationInfo info, StreamingContext context)
        {
            m_Name = info.GetString("name");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", m_Name);
        }

        private string m_Name;
    }

} // namespace
