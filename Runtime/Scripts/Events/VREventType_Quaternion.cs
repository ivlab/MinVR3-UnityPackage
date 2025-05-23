using System;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.Serialization;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventQuaternion : VREventT<Quaternion>
    {
        public VREventQuaternion(string name, Quaternion data) : base(name, data)
        {
        }

        public override VREvent Clone()
        {
            return new VREventQuaternion(m_Name, m_Data);
        }

        protected VREventQuaternion(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }


    [Serializable]
    public class VREventPrototypeQuaternion : VREventPrototypeT<Quaternion>
    {
        static public VREventPrototypeQuaternion Create(string eventName)
        {
            var ep = new VREventPrototypeQuaternion();
            ep.SetEventName(eventName);
            return ep;
        }

        public override IVREventPrototype Clone()
        {
            return Create(m_EventName);
        }
    }

    [Serializable]
    public class VRCallbackQuaternion : VRCallbackT<Quaternion>
    {
        public static VRCallbackQuaternion CreateRuntime(UnityAction<Quaternion> callbackFunc)
        {
            var cb = new VRCallbackQuaternion();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

#if UNITY_EDITOR
        public static VRCallbackQuaternion CreateInEditor(UnityAction<Quaternion> callbackFunc)
        {
            var cb = new VRCallbackQuaternion();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
#endif
    }

    [Serializable]
    public class VREventCallbackQuaternion : VREventCallbackT<Quaternion>
    {
        public static VREventCallbackQuaternion CreateRuntime(string listenForEvent, UnityAction<Quaternion> callbackFunc = null)
        {
            return CreateRuntime(VREventPrototypeQuaternion.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackQuaternion CreateRuntime(VREventPrototypeQuaternion listenForEvent, UnityAction<Quaternion> callbackFunc = null)
        {
            var cb = new VREventCallbackQuaternion();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

#if UNITY_EDITOR
        public static VREventCallbackQuaternion CreateInEditor(string listenForEvent, UnityAction<Quaternion> callbackFunc = null)
        {
            return CreateInEditor(VREventPrototypeQuaternion.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackQuaternion CreateInEditor(VREventPrototypeQuaternion listenForEvent, UnityAction<Quaternion> callbackFunc = null)
        {
            var cb = new VREventCallbackQuaternion();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddPersistentListener(callbackFunc);
            }
            return cb;
        }
#endif
    }

} // end namespace
