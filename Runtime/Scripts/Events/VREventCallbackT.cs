using UnityEngine;
using UnityEngine.Events;
using System;


namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventCallbackT<T> : IVREventPrototype, IVRCallback, IVREventListener
    {
        // Type-Specific Static Constructors should be implemented in VREvent_<DataTypeName>.cs files.
        // It is recommended to use the type-specific subclasses (i.e., use VRCallbackInt rather than
        // the generic version VRCallback<int>) in your code because Unity's serializer has difficulty
        // correctly serializing and deserializing generic types in some instances.

        public VREventCallbackT()
        {
            m_EventPrototype = new VREventPrototypeT<T>();
            m_VRCallback = new VRCallbackT<T>();
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


        // --- FORWARD THESE CALLS TO THE VRCALLBACK ---

        public void AddRuntimeListener(UnityAction<T> listener)
        {
            m_VRCallback.AddRuntimeListener(listener);
        }

        public void RemoveRuntimeListener(UnityAction<T> listener)
        {
            m_VRCallback.RemoveRuntimeListener(listener);
        }

#if UNITY_EDITOR
        public void AddPersistentListener(UnityAction<T> listener)
        {
            m_VRCallback.AddPersistentListener(listener);
        }

        public void RemovePersistentListener(UnityAction<T> listener)
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
                VREngine.instance.eventManager.AddEventReceiver(this);
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
                VREngine.instance?.eventManager?.RemoveEventReceiver(this);
            }
        }

        [SerializeField] private VRCallbackT<T> m_VRCallback;
        [SerializeField] private VREventPrototypeT<T> m_EventPrototype;
    }

} // end namespace
