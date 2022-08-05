using System;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.Serialization;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventString : VREventT<string>
    {
        public VREventString(string name, String data) : base(name, data)
        {
        }

        public override VREvent Clone()
        {
            return new VREventString(m_Name, m_Data);
        }

        protected VREventString(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class VREventPrototypeString : VREventPrototypeT<string>
    {
        static public VREventPrototypeString Create(string eventName)
        {
            var ep = new VREventPrototypeString();
            ep.SetEventName(eventName);
            return ep;
        }

        public override IVREventPrototype Clone()
        {
            return Create(m_EventName);
        }
    }

    [Serializable]
    public class VRCallbackString : VRCallbackT<string>
    {
        public static VRCallbackString CreateRuntime(UnityAction<string> callbackFunc)
        {
            var cb = new VRCallbackString();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

#if UNITY_EDITOR
        public static VRCallbackString CreateInEditor(UnityAction<string> callbackFunc)
        {
            var cb = new VRCallbackString();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
#endif
    }

        [Serializable]
    public class VREventCallbackString : VREventCallbackT<string>
    {
        public static VREventCallbackString CreateRuntime(string listenForEvent, UnityAction<string> callbackFunc = null)
        {
            return CreateRuntime(VREventPrototypeString.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackString CreateRuntime(VREventPrototypeString listenForEvent, UnityAction<string> callbackFunc = null)
        {
            var cb = new VREventCallbackString();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

#if UNITY_EDITOR
        public static VREventCallbackString CreateInEditor(string listenForEvent, UnityAction<string> callbackFunc = null)
        {
            return CreateInEditor(VREventPrototypeString.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackString CreateInEditor(VREventPrototypeString listenForEvent, UnityAction<string> callbackFunc = null)
        {
            var cb = new VREventCallbackString();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddPersistentListener(callbackFunc);
            }
            return cb;
        }
#endif
    }

} // end namespace
