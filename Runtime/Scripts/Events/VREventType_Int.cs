using System;
using UnityEngine.Events;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventPrototypeInt : VREventPrototype<int>
    {
        static public VREventPrototypeInt Create(string eventName)
        {
            var ep = new VREventPrototypeInt();
            ep.SetEventName(eventName);
            return ep;
        }
    }


    [Serializable]
    public class VRCallbackInt : VRCallback<int>
    {
        public static VRCallbackInt CreateRuntime(UnityAction<int> callbackFunc)
        {
            var cb = new VRCallbackInt();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

        public static VRCallbackInt CreateInEditor(UnityAction<int> callbackFunc)
        {
            var cb = new VRCallbackInt();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
    }


    [Serializable]
    public class VREventCallbackInt : VREventCallback<int>
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
    }

} // end namespace
