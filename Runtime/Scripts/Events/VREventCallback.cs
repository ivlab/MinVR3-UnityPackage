using UnityEngine;
using UnityEngine.Events;
using System;


namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventCallback : IVREventPrototype, IVRCallback, IVREventListener
    {
        // --- Static Constructors for Convenience ---

        public static VREventCallback CreateRuntime(string listenForEvent, UnityAction callbackFunc = null)
        {
            return CreateRuntime(VREventPrototype.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallback CreateRuntime(VREventPrototype listenForEvent, UnityAction callbackFunc = null)
        {
            VREventCallback cb = new VREventCallback();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddRuntimeListener(callbackFunc);
            }
            return cb;
        }

#if UNITY_EDITOR
        public static VREventCallback CreateInEditor(string listenForEvent, UnityAction callbackFunc = null)
        {
            return CreateInEditor(VREventPrototype.Create(listenForEvent), callbackFunc);
        }

        public static VREventCallback CreateInEditor(VREventPrototype listenForEvent, UnityAction callbackFunc = null)
        {
            VREventCallback cb = new VREventCallback();
            cb.SetEventName(listenForEvent.GetEventName());
            if (callbackFunc != null) {
                cb.AddPersistentListener(callbackFunc);
            }
            return cb;
        }
#endif

        // ---


        public VREventCallback()
        {
            m_EventPrototype = new VREventPrototype();
            m_VRCallback = new VRCallback();
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

        public IVREventPrototype Clone()
        {
            return m_EventPrototype.Clone();
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

#if UNITY_EDITOR
        public void AddPersistentListener(UnityAction listener)
        {
            m_VRCallback.AddPersistentListener(listener);
        }

        public void RemovePersistentListener(UnityAction listener)
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
                VREngine.Instance.eventManager.AddEventListener(this);
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
                VREngine.Instance.eventManager.RemoveEventListener(this);
            }
        }

        [SerializeField] private VRCallback m_VRCallback;
        [SerializeField] private VREventPrototype m_EventPrototype;
    }

} // end namespace
