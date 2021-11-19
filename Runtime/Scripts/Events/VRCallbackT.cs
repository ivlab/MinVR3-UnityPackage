using UnityEngine;
using UnityEngine.Events;
using System;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace IVLab.MinVR3
{

    [Serializable]
    public class VRCallbackT<T> : UnityEvent<T>, IVRCallback
    {
        // Type-Specific Static Constructors should be implemented in VREvent_<DataTypeName>.cs files.
        // It is recommended to use the type-specific subclasses (i.e., use VRCallbackInt rather than
        // the generic version VRCallback<int>) in your code because Unity's serializer has difficulty
        // correctly serializing and deserializing generic types in some instances.

        public VRCallbackT()
        {
        }

        /// <summary>
        /// For callbacks created while your program is running (i.e., from within Start(), Update(), etc.).
        /// Note that callbacks added this way will not show up in the inspector -- that is a limitation/feature
        /// of the UnityEvent* classes upon which this implmementation is based. 
        /// </summary>
        public void AddRuntimeListener(UnityAction<T> listener)
        {
            AddListener(listener);
        }

        public void RemoveRuntimeListener(UnityAction<T> listener)
        {
            RemoveListener(listener);
        }

#if UNITY_EDITOR
        /// <summary>
        /// For callbacks created while in editor mode (i.e., from Reset() or custom editors and property drawers).
        /// Callbacks added this way will be displayed in the Inspector.
        /// </summary>
        public void AddPersistentListener(UnityAction<T> listener)
        {
            UnityEventTools.AddPersistentListener(this, listener);
        }

        public void RemovePersistentListener(UnityAction<T> listener)
        {
            UnityEventTools.RemovePersistentListener(this, listener);
        }
#endif

        public void InvokeWithVREvent(VREvent e)
        {
            base.Invoke((e as VREventT<T>).data);
        }
    }

} // end namespace
