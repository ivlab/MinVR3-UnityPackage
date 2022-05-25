using System;
using UnityEngine;
using UnityEngine.Events;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventVector4 : VREventT<Vector4>
    {
        public VREventVector4(string name, Vector4 data) : base(name, data)
        {
        }

        public override VREvent Clone()
        {
            return new VREventVector4(m_Name, m_Data);
        }
    }

    [Serializable]
    public class VREventPrototypeVector4 : VREventPrototypeT<Vector4>
    {
        static public VREventPrototypeVector4 Create(string eventName)
        {
            var ep = new VREventPrototypeVector4();
            ep.SetEventName(eventName);
            return ep;
        }

        public override IVREventPrototype Clone()
        {
            return Create(m_EventName);
        }
    }

    [Serializable]
    public class VRCallbackVector4 : VRCallbackT<Vector4>
    {
        public static VRCallbackVector4 CreateRuntime(UnityAction<Vector4> callbackFunc)
        {
            var cb = new VRCallbackVector4();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

#if UNITY_EDITOR
        public static VRCallbackVector4 CreateInEditor(UnityAction<Vector4> callbackFunc)
        {
            var cb = new VRCallbackVector4();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
#endif
    }

        [Serializable]
    public class VREventCallbackVector4 : VREventCallbackT<Vector4>
    {
        public static VREventCallbackVector4 CreateRuntime(string listenForEvent, UnityAction<Vector4> callbackFunc = null)
        {
            return CreateRuntime(VREventPrototypeVector4.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackVector4 CreateRuntime(VREventPrototypeVector4 listenForEvent, UnityAction<Vector4> callbackFunc = null)
        {
            var cb = new VREventCallbackVector4();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

#if UNITY_EDITOR
        public static VREventCallbackVector4 CreateInEditor(string listenForEvent, UnityAction<Vector4> callbackFunc = null)
        {
            return CreateInEditor(VREventPrototypeVector4.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackVector4 CreateInEditor(VREventPrototypeVector4 listenForEvent, UnityAction<Vector4> callbackFunc = null)
        {
            var cb = new VREventCallbackVector4();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddPersistentListener(callbackFunc);
            }
            return cb;
        }
#endif
    }

} // end namespace
