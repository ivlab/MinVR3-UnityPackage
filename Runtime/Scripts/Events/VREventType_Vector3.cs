using System;
using UnityEngine;
using UnityEngine.Events;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventVector3 : VREventT<Vector3>
    {
        public VREventVector3(string name, Vector3 data) : base(name, data)
        {
        }
    }

    [Serializable]
    public class VREventPrototypeVector3 : VREventPrototypeT<Vector3>
    {
        static public VREventPrototypeVector3 Create(string eventName)
        {
            var ep = new VREventPrototypeVector3();
            ep.SetEventName(eventName);
            return ep;
        }
    }

    [Serializable]
    public class VRCallbackVector3 : VRCallbackT<Vector3>
    {
        public static VRCallbackVector3 CreateRuntime(UnityAction<Vector3> callbackFunc)
        {
            var cb = new VRCallbackVector3();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

#if UNITY_EDITOR
        public static VRCallbackVector3 CreateInEditor(UnityAction<Vector3> callbackFunc)
        {
            var cb = new VRCallbackVector3();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
#endif
    }

        [Serializable]
    public class VREventCallbackVector3 : VREventCallbackT<Vector3>
    {
        public static VREventCallbackVector3 CreateRuntime(string listenForEvent, UnityAction<Vector3> callbackFunc = null)
        {
            return CreateRuntime(VREventPrototypeVector3.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackVector3 CreateRuntime(VREventPrototypeVector3 listenForEvent, UnityAction<Vector3> callbackFunc = null)
        {
            var cb = new VREventCallbackVector3();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

#if UNITY_EDITOR
        public static VREventCallbackVector3 CreateInEditor(string listenForEvent, UnityAction<Vector3> callbackFunc = null)
        {
            return CreateInEditor(VREventPrototypeVector3.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackVector3 CreateInEditor(VREventPrototypeVector3 listenForEvent, UnityAction<Vector3> callbackFunc = null)
        {
            var cb = new VREventCallbackVector3();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddPersistentListener(callbackFunc);
            }
            return cb;
        }
#endif
    }

} // end namespace
