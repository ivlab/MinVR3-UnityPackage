using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventCallbackAny : IVREventPrototype, IVRCallback, IVREventListener
    {
        // --- Static Constructors for Convenience ---

        public static VREventCallbackAny CreateRuntime(string listenForEvent, UnityAction callbackFunc = null)
        {
            var cb = new VREventCallbackAny();
            cb.SetEventDataType("");
            cb.SetEventName(listenForEvent);
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

        public static VREventCallbackAny CreateRuntime(VREventPrototype listenForEvent, UnityAction callbackFunc = null)
        {
            return CreateRuntime(listenForEvent.GetEventName(), callbackFunc);
        }

#if UNITY_EDITOR
        public static VREventCallbackAny CreateInEditor(string listenForEvent, UnityAction callbackFunc = null)
        {
            var cb = new VREventCallbackAny();
            cb.SetEventDataType("");
            cb.SetEventName(listenForEvent);
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

        public static VREventCallbackAny CreateInEditor(VREventPrototype listenForEvent, UnityAction callbackFunc = null)
        {
            return CreateInEditor(listenForEvent.GetEventName(), callbackFunc);
        }
#endif


        public static VREventCallbackAny CreateRuntime<T>(string listenForEvent, UnityAction<T> callbackFunc = null)
        {
            var cb = new VREventCallbackAny();
            Debug.Assert(cb.SupportsDataType(typeof(T)),
                "VREvents with data type '" + typeof(T).Name + "' are not supported.");
            cb.SetEventDataType(typeof(T));
            cb.SetEventName(listenForEvent);
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

        public static VREventCallbackAny CreateRuntime<T>(VREventPrototypeT<T> listenForEvent, UnityAction<T> callbackFunc = null)
        {
            return CreateRuntime(listenForEvent.GetEventName(), callbackFunc);
        }


#if UNITY_EDITOR
        public static VREventCallbackAny CreateInEditor<T>(string listenForEvent, UnityAction<T> callbackFunc = null)
        {
            var cb = new VREventCallbackAny();
            Debug.Assert(cb.SupportsDataType(typeof(T)),
                "VREvents with data type '" + typeof(T).Name + "' are not supported.");
            cb.SetEventDataType(typeof(T));
            cb.SetEventName(listenForEvent);
            if (callbackFunc != null) {
                cb.AddPersistentListener(callbackFunc);
            }
            return cb;
        }

        public static VREventCallbackAny CreateInEditor<T>(VREventPrototypeT<T> listenForEvent, UnityAction<T> callbackFunc = null)
        {
            return CreateRuntime(listenForEvent.GetEventName(), callbackFunc);
        }
#endif

        // ---


        public VREventCallbackAny()
        {
            m_EventPrototype = new VREventPrototypeAny();
            m_VRCallback = new VRCallbackAny();
            m_VRCallback.SetShowDataTypeInEditor(false);
        }


        public bool SupportsDataType(Type dataType)
        {
            return m_EventPrototype.SupportsDataType(dataType) && m_VRCallback.SupportsDataType(dataType);
        }

        public bool SupportsDataType(string dataTypeName)
        {
            return m_EventPrototype.SupportsDataType(dataTypeName) && m_VRCallback.SupportsDataType(dataTypeName);
        }

        // --- FORWARD THESE CALLS TO THE VREVENTPROTOTYPE ---

        public void SetEventName(string eventName)
        {
            m_EventPrototype.SetEventName(eventName);
        }

        public string GetEventName()
        {
            return m_EventPrototype.GetEventName();
        }

        public string GetEventDataTypeName()
        {
            return m_EventPrototype.GetEventDataTypeName();
        }

        public string GetEventDisplayName()
        {
            return m_EventPrototype.GetEventDisplayName();
        }

        public void SetEventDataType(Type eventDataType)
        {
            m_EventPrototype.SetEventDataType(eventDataType);
            m_VRCallback.SetEventDataType(eventDataType);
        }

        public void SetEventDataType(string eventDataTypeName)
        {
            m_EventPrototype.SetEventDataType(eventDataTypeName);
            m_VRCallback.SetEventDataType(eventDataTypeName);
        }


        // --- FORWARD THESE CALLS TO THE VRCALLBACK ---

        public void AddRuntimeListener(UnityAction listener)
        {
            m_VRCallback.AddRuntimeListener(listener);
        }

        public void RemoveRuntimeListener(UnityAction listener)
        {
            m_VRCallback.RemoveRuntimeListener(listener);
        }

        public void AddRuntimeListener<T>(UnityAction<T> listener)
        {
            m_VRCallback.AddRuntimeListener(listener);
        }

        public void RemoveRuntimeListener<T>(UnityAction<T> listener)
        {
            m_VRCallback.RemoveRuntimeListener(listener);
        }


#if UNITY_EDITOR
        public void AddPersistentListener(UnityAction listener)
        {
            m_VRCallback.AddPersistentListener(listener);
        }

        public void RemovePersistentListener(UnityAction listener)
        {
            m_VRCallback.RemovePersistentListener(listener);
        }

        public void AddPersistentListener<T>(UnityAction<T> listener)
        {
            m_VRCallback.AddPersistentListener(listener);
        }

        public void RemovePersistentListener<T>(UnityAction<T> listener)
        {
            m_VRCallback.RemovePersistentListener(listener);
        }
#endif

        public void InvokeWithVREvent(VREvent vrEvent)
        {
            m_VRCallback.InvokeWithVREvent(vrEvent);
        }


        // --- IMPLEMENT IVREVENTLISTENER ---

        public void StartListening()
        {
            if (Application.isPlaying) {
                VREngine.instance.eventManager.AddEventListener(this);
            }
        }

        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_EventPrototype)) {
                InvokeWithVREvent(vrEvent);
            }
        }

        public void StopListening()
        {
            if (Application.isPlaying) {
                VREngine.instance?.eventManager?.RemoveEventListener(this);
            }
        }

        [SerializeField] private VRCallbackAny m_VRCallback;
        [SerializeField] private VREventPrototypeAny m_EventPrototype;
    }

} // end namespace
