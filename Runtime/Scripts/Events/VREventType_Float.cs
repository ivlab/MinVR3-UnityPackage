using System;
using UnityEngine.Events;
using System.Runtime.Serialization;

namespace IVLab.MinVR3
{
    [Serializable]
    public class VREventFloat : VREventT<float>, ISerializable
    {
        public VREventFloat(string name, float data) : base(name, data)
        {
        }

        public override VREvent Clone()
        {
            return new VREventFloat(m_Name, m_Data);
        }

        protected VREventFloat(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class VREventPrototypeFloat : VREventPrototypeT<float>
    {
        static public VREventPrototypeFloat Create(string eventName)
        {
            var ep = new VREventPrototypeFloat();
            ep.SetEventName(eventName);
            return ep;
        }

        public override IVREventPrototype Clone()
        {
            return Create(m_EventName);
        }
    }

    [Serializable]
    public class VRCallbackFloat : VRCallbackT<float>
    {
        public static VRCallbackFloat CreateRuntime(UnityAction<float> callbackFunc)
        {
            var cb = new VRCallbackFloat();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

#if UNITY_EDITOR
        public static VRCallbackFloat CreateInEditor(UnityAction<float> callbackFunc)
        {
            var cb = new VRCallbackFloat();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
#endif
    }

    [Serializable]
    public class VREventCallbackFloat : VREventCallbackT<float>
    {
        public static VREventCallbackFloat CreateRuntime(string listenForEvent, UnityAction<float> callbackFunc = null)
        {
            return CreateRuntime(VREventPrototypeFloat.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackFloat CreateRuntime(VREventPrototypeFloat listenForEvent, UnityAction<float> callbackFunc = null)
        {
            var cb = new VREventCallbackFloat();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

#if UNITY_EDITOR
        public static VREventCallbackFloat CreateInEditor(string listenForEvent, UnityAction<float> callbackFunc = null)
        {
            return CreateInEditor(VREventPrototypeFloat.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackFloat CreateInEditor(VREventPrototypeFloat listenForEvent, UnityAction<float> callbackFunc = null)
        {
            var cb = new VREventCallbackFloat();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddPersistentListener(callbackFunc);
            }
            return cb;
        }
#endif
    }


} // end namespace
