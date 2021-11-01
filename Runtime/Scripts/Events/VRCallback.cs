using UnityEngine;
using UnityEngine.Events;
using UnityEditor.Events;
using System;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VRCallback : UnityEvent, IVRCallback
    {
        // --- Static Constructors for Convenience ---

        public static VRCallback CreateRuntime(UnityAction callbackFunc)
        {
            var cb = new VRCallback();
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

        public static VRCallback CreateInEditor(UnityAction callbackFunc)
        {
            var cb = new VRCallback();
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }

        // ---


        public VRCallback()
        {
        }

        /// <summary>
        /// For callbacks created while your program is running (i.e., from within Start(), Update(), etc.).
        /// Note that callbacks added this way will not show up in the inspector -- that is a limitation/feature
        /// of the UnityEvent* classes upon which this implmementation is based. 
        /// </summary>
        public void AddRuntimeListener(UnityAction listener)
        {
            AddListener(listener);
        }

        public void RemoveRuntimeListener(UnityAction listener)
        {
            RemoveListener(listener);
        }

        /// <summary>
        /// For callbacks created while in editor mode (i.e., from Reset() or custom editors and property drawers).
        /// Callbacks added this way will be displayed in the Inspector.
        /// </summary>
        public void AddPersistentListener(UnityAction listener)
        {
            UnityEventTools.AddPersistentListener(this, listener);
        }

        public void RemovePersistentListener(UnityAction listener)
        {
            UnityEventTools.RemovePersistentListener(this, listener);
        }

        public void Invoke(VREvent e)
        {
            Invoke();
        }
    }

} // end namespace
