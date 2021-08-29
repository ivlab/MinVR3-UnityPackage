using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace IVLab.MinVR3
{

    public class VREventInstance
    {
        public VREventInstance(string eventName)
        {
            name = eventName;
        }
        public string name { get; }
    }


    public class VREventInstance<T> : VREventInstance
    {
        public VREventInstance(string eventName, T eventData) : base(eventName)
        {
            data = eventData;
        }
        public T data { get; }
    }

} // namespace
