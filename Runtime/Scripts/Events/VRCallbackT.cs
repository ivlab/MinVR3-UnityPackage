using UnityEngine;
using UnityEngine.Events;
using UnityEditor.Events;
using System;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VRCallback<T> : UnityEvent<T>, IVRCallback
    {
        // Type-Specific Static Constructors should be implemented in VREvent_<DataTypeName>.cs files.
        // It is recommended to use the type-specific subclasses (i.e., use VRCallbackInt rather than
        // the generic version VRCallback<int>) in your code because Unity's serializer has difficulty
        // correctly serializing and deserializing generic types in some instances.

        public VRCallback()
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

        public void Invoke(VREvent e)
        {
            Invoke((e as VREvent<T>).data);
        }
    }

} // end namespace
