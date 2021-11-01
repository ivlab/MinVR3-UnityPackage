using System;
using UnityEngine.Events;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventPrototypeFloat : VREventPrototype<float>
    {
        static public VREventPrototypeFloat Create(string eventName)
        {
            var ep = new VREventPrototypeFloat();
            ep.SetEventName(eventName);
            return ep;
        }
    }

    [Serializable]
    public class VRCallbackFloat : VRCallback<float>
    {
        public static VRCallbackFloat CreateRuntime(UnityAction<float> callbackFunc)
        {
            var cb = new VRCallbackFloat();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

        public static VRCallbackFloat CreateInEditor(UnityAction<float> callbackFunc)
        {
            var cb = new VRCallbackFloat();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
    }

    [Serializable]
    public class VREventCallbackFloat : VREventCallback<float>
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
    }


} // end namespace
