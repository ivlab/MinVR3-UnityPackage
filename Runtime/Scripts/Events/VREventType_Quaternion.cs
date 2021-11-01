using System;
using UnityEngine;
using UnityEngine.Events;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventPrototypeQuaternion : VREventPrototype<Quaternion>
    {
        static public VREventPrototypeQuaternion Create(string eventName)
        {
            var ep = new VREventPrototypeQuaternion();
            ep.SetEventName(eventName);
            return ep;
        }
    }

    [Serializable]
    public class VRCallbackQuaternion : VRCallback<Quaternion>
    {
        public static VRCallbackQuaternion CreateRuntime(UnityAction<Quaternion> callbackFunc)
        {
            var cb = new VRCallbackQuaternion();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

        public static VRCallbackQuaternion CreateInEditor(UnityAction<Quaternion> callbackFunc)
        {
            var cb = new VRCallbackQuaternion();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
    }

    [Serializable]
    public class VREventCallbackQuaternion : VREventCallback<Quaternion>
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
    }

} // end namespace
