using System;
using UnityEngine;
using UnityEngine.Events;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventPrototypeVector2 : VREventPrototype<Vector2>
    {
        static public VREventPrototypeVector2 Create(string eventName)
        {
            var ep = new VREventPrototypeVector2();
            ep.SetEventName(eventName);
            return ep;
        }
    }

    [Serializable]
    public class VRCallbackVector2 : VRCallback<Vector2>
    {
        public static VRCallbackVector2 CreateRuntime(UnityAction<Vector2> callbackFunc)
        {
            var cb = new VRCallbackVector2();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

        public static VRCallbackVector2 CreateInEditor(UnityAction<Vector2> callbackFunc)
        {
            var cb = new VRCallbackVector2();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
    }

    [Serializable]
    public class VREventCallbackVector2 : VREventCallback<Vector2>
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
    }

} // end namespace
