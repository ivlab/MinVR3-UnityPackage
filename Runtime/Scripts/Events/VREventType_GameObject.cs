using System;
using UnityEngine;
using UnityEngine.Events;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventGameObject : VREventT<GameObject>
    {
        public VREventGameObject(string name, GameObject data) : base(name, data)
        {
        }

        public override bool IsClusterSafe()
        {
            return false;
        }

        public override VREvent Clone()
        {
            return new VREventGameObject(m_Name, m_Data);
        }
    }


    [Serializable]
    public class VREventPrototypeGameObject : VREventPrototypeT<GameObject>
    {
        static public VREventPrototypeGameObject Create(string eventName)
        {
            var ep = new VREventPrototypeGameObject();
            ep.SetEventName(eventName);
            return ep;
        }

        public override IVREventPrototype Clone()
        {
            return Create(m_EventName);
        }
    }

    [Serializable]
    public class VRCallbackGameObject : VRCallbackT<GameObject>
    {
        public static VRCallbackGameObject CreateRuntime(UnityAction<GameObject> callbackFunc)
        {
            var cb = new VRCallbackGameObject();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

#if UNITY_EDITOR
        public static VRCallbackGameObject CreateInEditor(UnityAction<GameObject> callbackFunc)
        {
            var cb = new VRCallbackGameObject();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
#endif
    }

    [Serializable]
    public class VREventCallbackGameObject : VREventCallbackT<GameObject>
    {
        public static VREventCallbackGameObject CreateRuntime(string listenForEvent, UnityAction<GameObject> callbackFunc = null)
        {
            return CreateRuntime(VREventPrototypeGameObject.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackGameObject CreateRuntime(VREventPrototypeGameObject listenForEvent, UnityAction<GameObject> callbackFunc = null)
        {
            var cb = new VREventCallbackGameObject();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

#if UNITY_EDITOR
        public static VREventCallbackGameObject CreateInEditor(string listenForEvent, UnityAction<GameObject> callbackFunc = null)
        {
            return CreateInEditor(VREventPrototypeGameObject.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallbackGameObject CreateInEditor(VREventPrototypeGameObject listenForEvent, UnityAction<GameObject> callbackFunc = null)
        {
            var cb = new VREventCallbackGameObject();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddPersistentListener(callbackFunc);
            }
            return cb;
        }
#endif
    }

} // end namespace
