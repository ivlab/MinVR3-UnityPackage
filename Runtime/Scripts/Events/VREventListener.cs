using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;


namespace IVLab.MinVR3
{
    public interface IVREventListener
    {
        public void Invoke(VREvent vrEvent);
        public bool InvokeIfMatches(VREvent vrEvent);
    }


    [Serializable]
    public class VREventListener : IVREventListener
    {
        public VREventListener()
        {
            m_VREventToListenFor = new VREventPrototype();
            m_OnVREventCallback = new VRCallback();
        }

        public VREventListener(string eventName)
        {
            m_VREventToListenFor = new VREventPrototype(eventName);
            m_OnVREventCallback = new VRCallback();
        }

        public VREventListener(VREventPrototype listenForEvent, UnityAction callbackFunction)
        {
            m_VREventToListenFor = listenForEvent;
            m_OnVREventCallback = new VRCallback(callbackFunction);
        }

        public VREventListener(VREventPrototype listenForEvent, VRCallback onEventCallback)
        {
            m_VREventToListenFor = listenForEvent;
            m_OnVREventCallback = onEventCallback;
        }


        public VREventPrototype vrEventToListenFor {
            get => m_VREventToListenFor;
            set => m_VREventToListenFor = value;
        }

        public VRCallback onVREventCallback {
            get => m_OnVREventCallback;
            set => m_OnVREventCallback = value;
        }

        public void Invoke(VREvent vrEvent)
        {
            m_OnVREventCallback.Invoke(vrEvent);
        }

        public bool InvokeIfMatches(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_VREventToListenFor)) {
                m_OnVREventCallback.Invoke(vrEvent);
                return true;
            } else {
                return false;
            }
        }

        [SerializeField] private VREventPrototype m_VREventToListenFor;
        [SerializeField] private VRCallback m_OnVREventCallback;
    }


    [Serializable]
    public class VREventListener<T> : IVREventListener
    {
        public VREventListener()
        {
            m_VREventToListenFor = new VREventPrototype<T>();
            m_OnVREventCallback = new VRCallback<T>();
        }

        public VREventListener(string eventName)
        {
            m_VREventToListenFor = new VREventPrototype<T>(eventName);
            m_OnVREventCallback = new VRCallback<T>();
        }

        public VREventListener(VREventPrototype<T> listenForEvent, UnityAction<T> callbackFunction)
        {
            m_VREventToListenFor = listenForEvent;
            m_OnVREventCallback = new VRCallback<T>(callbackFunction);
        }

        public VREventListener(VREventPrototype<T> listenForEvent, VRCallback<T> onEventCallback)
        {
            m_VREventToListenFor = listenForEvent;
            m_OnVREventCallback = onEventCallback;
        }



        public VREventPrototype<T> vrEventToListenFor {
            get => m_VREventToListenFor;
            set => m_VREventToListenFor = value;
        }

        public VRCallback<T> onVREventCallback {
            get => m_OnVREventCallback;
            set => m_OnVREventCallback = value;
        }

        public void Invoke(VREvent vrEvent)
        {
            m_OnVREventCallback.Invoke(vrEvent);
        }

        public bool InvokeIfMatches(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_VREventToListenFor)) {
                m_OnVREventCallback.Invoke(vrEvent);
                return true;
            } else {
                return false;
            }
        }

        [SerializeField] private VREventPrototype<T> m_VREventToListenFor;
        [SerializeField] private VRCallback<T> m_OnVREventCallback;
    }


    [Serializable]
    public class VREventListenerAny : IVREventListener
    {
        public VREventListenerAny()
        {
            m_VREventToListenFor = new VREventPrototypeAny();
            m_OnVREventCallback = new VRCallbackAny();
            m_OnVREventCallback.SetShowDataTypeDropdown(false);
        }

        public VREventListenerAny(VREventPrototypeAny listenForEvent, VRCallbackAny onEventCallback)
        {
            m_VREventToListenFor = listenForEvent;
            m_OnVREventCallback = onEventCallback;
            m_OnVREventCallback.SetShowDataTypeDropdown(false);
        }

        // static constructors / factory methods
        public static VREventListenerAny Create(VREventPrototype listenFor)
        {
            var prototype = new VREventPrototypeAny(listenFor.eventName, null);
            var callback = new VRCallbackAny();
            return new VREventListenerAny(prototype, callback);
        }

        public static VREventListenerAny Create<T>(VREventPrototype<T> listenFor)
        {
            var prototype = new VREventPrototypeAny(listenFor.eventName, typeof(T));
            var callback = new VRCallbackAny();
            return new VREventListenerAny(prototype, callback);
        }

        public static VREventListenerAny Create(VREventPrototype listenFor, UnityAction callbackFunction)
        {
            var prototype = new VREventPrototypeAny(listenFor.eventName, null);
            var callback = VRCallbackAny.Create(callbackFunction);
            return new VREventListenerAny(prototype, callback);
        }

        public static VREventListenerAny Create<T>(VREventPrototype<T> listenFor, UnityAction<T> callbackFunction)
        {
            var prototype = new VREventPrototypeAny(listenFor.eventName, typeof(T));
            var callback = VRCallbackAny.Create<T>(callbackFunction);
            return new VREventListenerAny(prototype, callback);
        }

        public VREventPrototypeAny vrEventToListenFor {
            get => m_VREventToListenFor;
            set {
                m_VREventToListenFor = value;
                m_OnVREventCallback.SetDataType(m_VREventToListenFor.dataTypeName);
            }
        }

        public VRCallbackAny onVREventCallback {
            get => m_OnVREventCallback;
            set {
                m_OnVREventCallback = value;
                m_OnVREventCallback.SetShowDataTypeDropdown(false);
                m_VREventToListenFor.SetDataType(m_OnVREventCallback.dataTypeName);
            }
        }

        public void Invoke(VREvent vrEvent)
        {
            m_OnVREventCallback.Invoke(vrEvent);
        }

        public bool InvokeIfMatches(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_VREventToListenFor)) {
                m_OnVREventCallback.Invoke(vrEvent);
                return true;
            } else {
                return false;
            }
        }

        [SerializeField] private VREventPrototypeAny m_VREventToListenFor;
        [SerializeField] private VRCallbackAny m_OnVREventCallback;
    }


} // end namespace
