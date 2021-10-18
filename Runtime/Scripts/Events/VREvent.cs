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
                (GetDataTypeName() == eventPrototype.GetDataTypeName());
        }

        public bool Matches(IVRCallback callback)
        {
            return (GetDataTypeName() == callback.GetDataTypeName());
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


    public class VREvent<T> : VREvent
    {
        public VREvent(string eventName, T eventData) : base(eventName)
        {
            m_Data = eventData;
        }

        public VREvent() : base()
        {
        }

        public T data {
            get => m_Data;
        }

        public override string GetDataTypeName()
        {
            return typeof(T).Name;
        }

        public T GetData()
        {
            return m_Data;
        }

        protected VREvent(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            m_Data = (T)info.GetValue("data", typeof(T));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("data", m_Data);
            base.GetObjectData(info, context);
        }

        private T m_Data;
    }

} // namespace
