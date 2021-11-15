using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.Serialization;

namespace IVLab.MinVR3
{

    public class VREventT<T> : VREvent
    {
        protected VREventT(string eventName, T eventData) : base(eventName)
        {
            m_Data = eventData;
        }

        protected VREventT() : base()
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

        protected VREventT(SerializationInfo info, StreamingContext context) : base(info, context)
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
