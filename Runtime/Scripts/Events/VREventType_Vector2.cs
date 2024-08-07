using System;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.Serialization;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventVector2 : VREventT<Vector2>
    {
        public VREventVector2(string name, Vector2 data) : base(name, data)
        {
        }

        public override VREvent Clone()
        {
            return new VREventVector2(m_Name, m_Data);
        }

        protected VREventVector2(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public class VREventPrototypeVector2 : VREventPrototypeT<Vector2>
    {
        static public VREventPrototypeVector2 Create(string eventName)
        {
            var ep = new VREventPrototypeVector2();
            ep.SetEventName(eventName);
            return ep;
        }

        public override IVREventPrototype Clone()
        {
            return Create(m_EventName);
        }
    }

    [Serializable]
    public class VRCallbackVector2 : VRCallbackT<Vector2>
    {
        public static VRCallbackVector2 CreateRuntime(UnityAction<Vector2> callbackFunc)
        {
            var cb = new VRCallbackVector2();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

#if UNITY_EDITOR
        public static VRCallbackVector2 CreateInEditor(UnityAction<Vector2> callbackFunc)
        {
            var cb = new VRCallbackVector2();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
#endif
    }

    [Serializable]
    public class VREventCallbackVector2 : VREventCallbackT<Vector2>
    {
        public static VREventCallbackVector2 CreateRuntime(string listenForEvent, UnityAction<Vector2> callbackFunc = null)
        {
            return CreateRuntime(VREventPrototypeVector2.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackVector2 CreateRuntime(VREventPrototypeVector2 listenForEvent, UnityAction<Vector2> callbackFunc = null)
        {
            var cb = new VREventCallbackVector2();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

#if UNITY_EDITOR
        public static VREventCallbackVector2 CreateInEditor(string listenForEvent, UnityAction<Vector2> callbackFunc = null)
        {
            return CreateInEditor(VREventPrototypeVector2.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackVector2 CreateInEditor(VREventPrototypeVector2 listenForEvent, UnityAction<Vector2> callbackFunc = null)
        {
            var cb = new VREventCallbackVector2();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddPersistentListener(callbackFunc);
            }
            return cb;
        }
#endif
    }

} // end namespace
