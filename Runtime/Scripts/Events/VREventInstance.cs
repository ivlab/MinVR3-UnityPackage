using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.Serialization;

namespace IVLab.MinVR3
{

    public class VREventInstance : ISerializable
    {
        public VREventInstance(string eventName)
        {
            name = eventName;
        }

        public VREventInstance()
        {
        }

        public string name { get; }

        protected VREventInstance(SerializationInfo info, StreamingContext context)
        {
            name = info.GetString("name");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", name);
        }
    }


    public class VREventInstance<T> : VREventInstance
    {
        public VREventInstance(string eventName, T eventData) : base(eventName)
        {
            data = eventData;
        }

        public VREventInstance() : base()
        {
        }

        public T data { get; }

        protected VREventInstance(SerializationInfo info, StreamingContext context)
        {
            data = (T)info.GetValue("data", typeof(T));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("data", data);
            base.GetObjectData(info, context);
        }
    }

} // namespace
