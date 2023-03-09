using System;
using UnityEngine.Events;
using System.Runtime.Serialization;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventInt : VREventT<int>
    {
        public VREventInt(string name, int data) : base(name, data)
        {
        }

        public override VREvent Clone()
        {
            return new VREventInt(m_Name, m_Data);
        }

        protected VREventInt(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class VREventPrototypeInt : VREventPrototypeT<int>
    {
        static public VREventPrototypeInt Create(string eventName)
        {
            var ep = new VREventPrototypeInt();
            ep.SetEventName(eventName);
            return ep;
        }

        public override IVREventPrototype Clone()
        {
            return Create(m_EventName);
        }
    }


    [Serializable]
    public class VRCallbackInt : VRCallbackT<int>
    {
        public static VRCallbackInt CreateRuntime(UnityAction<int> callbackFunc)
        {
            var cb = new VRCallbackInt();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

#if UNITY_EDITOR
        public static VRCallbackInt CreateInEditor(UnityAction<int> callbackFunc)
        {
            var cb = new VRCallbackInt();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
#endif
    }


    [Serializable]
    public class VREventCallbackInt : VREventCallbackT<int>
    {
        public static VREventCallbackInt CreateRuntime(string listenForEvent, UnityAction<int> callbackFunc = null)
        {
            return CreateRuntime(VREventPrototypeInt.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackInt CreateRuntime(VREventPrototypeInt listenForEvent, UnityAction<int> callbackFunc = null)
        {
            var cb = new VREventCallbackInt();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

#if UNITY_EDITOR
        public static VREventCallbackInt CreateInEditor(string listenForEvent, UnityAction<int> callbackFunc = null)
        {
            return CreateInEditor(VREventPrototypeInt.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackInt CreateInEditor(VREventPrototypeInt listenForEvent, UnityAction<int> callbackFunc = null)
        {
            var cb = new VREventCallbackInt();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddPersistentListener(callbackFunc);
            }
            return cb;
        }
#endif
    }

} // end namespace
