using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif

namespace IVLab.MinVR3
{
    [Serializable]
    public class VRCallbackAny : IVRCallback
    {
        // --- Static Constructors for Convenience ---

        public static VRCallbackAny CreateRuntime(UnityAction callbackFunc)
        {
            var cb = new VRCallbackAny();
            cb.SetEventDataType("");
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

#if UNITY_EDITOR
        public static VRCallbackAny CreateInEditor(UnityAction callbackFunc)
        {
            var cb = new VRCallbackAny();
            cb.SetEventDataType("");
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
#endif

        public static VRCallbackAny CreateRuntime<T>(UnityAction<T> callbackFunc)
        {
            var cb = new VRCallbackAny();
            Debug.Assert(cb.SupportsDataType(typeof(T)),
                "VREvents with data type '" + typeof(T).Name + "' are not supported.");
            cb.SetEventDataType(typeof(T));
            cb.AddRuntimeListener(callbackFunc);
            return cb;
        }

#if UNITY_EDITOR
        public static VRCallbackAny CreateInEditor<T>(UnityAction<T> callbackFunc)
        {
            var cb = new VRCallbackAny();
            Debug.Assert(cb.SupportsDataType(typeof(T)),
                "VREvents with data type '" + typeof(T).Name + "' are not supported.");
            cb.SetEventDataType(typeof(T));
            cb.AddPersistentListener(callbackFunc);
            return cb;
        }
#endif

        // ---

        public VRCallbackAny()
        {
            m_ShowDataTypeInEditor = true;
            InitAllCallbacks();
        }


        public bool SupportsDataType(Type dataType)
        {
            string name = "";
            if (dataType != null) {
                name = dataType.Name;
            }
            return m_AllCallbacks.ContainsKey(name);
        }

        public bool SupportsDataType(string dataTypeName)
        {
            return m_AllCallbacks.ContainsKey(dataTypeName);
        }


        public void SetEventDataType(Type eventDataType)
        {
            string name = "";
            if (eventDataType != null) {
                name = eventDataType.Name;
            }
            Debug.Assert(m_AllCallbacks.ContainsKey(name), "No callback available for type " + name);
            m_DataTypeName = name;
        }

        public void SetEventDataType(string eventDataTypeName)
        {
            Debug.Assert(m_AllCallbacks.ContainsKey(eventDataTypeName), "No callback available for type " + eventDataTypeName);
            m_DataTypeName = eventDataTypeName;
        }

        /// <summary>
        /// For callbacks created while your program is running (i.e., from within Start(), Update(), etc.).
        /// Note that callbacks added this way will not show up in the inspector -- that is a limitation/feature
        /// of the UnityEvent* classes upon which this implmementation is based. 
        /// </summary>
        public void AddRuntimeListener(UnityAction listener)
        {
            Debug.Assert(m_DataTypeName == "", "Expected a callback function of the form OnVREvent()");
            m_Callback.AddListener(listener);
        }

        public void AddRuntimeListener<T>(UnityAction<T> listener)
        {
            Debug.Assert(m_DataTypeName == typeof(T).Name,
                "Expected a callback function of the form OnVREvent(" + m_DataTypeName + ")");
            UnityEvent<T> cb = m_AllCallbacks[m_DataTypeName] as UnityEvent<T>;
            cb.AddListener(listener);
        }

        public void RemoveRuntimeListener(UnityAction listener)
        {
            Debug.Assert(m_DataTypeName == "", "Expected a callback function of the form OnVREvent()");
            m_Callback.RemoveListener(listener);
        }

        public void RemoveRuntimeListener<T>(UnityAction<T> listener)
        {
            Debug.Assert(m_DataTypeName == typeof(T).Name,
                "Expected a callback function of the form OnVREvent(" + m_DataTypeName + ")");
            UnityEvent<T> cb = m_AllCallbacks[m_DataTypeName] as UnityEvent<T>;
            cb.RemoveListener(listener);
        }

#if UNITY_EDITOR
        /// <summary>
        /// For callbacks created while in editor mode (i.e., from Reset() or custom editors and property drawers).
        /// Callbacks added this way will be displayed in the Inspector.
        /// </summary>
        public void AddPersistentListener(UnityAction listener)
        {
            Debug.Assert(m_DataTypeName == "", "Expected a callback function of the form OnVREvent()");
            UnityEventTools.AddPersistentListener(m_Callback, listener);
        }

        public void AddPersistentListener<T>(UnityAction<T> listener)
        {
            Debug.Assert(m_DataTypeName == typeof(T).Name,
                "Expected a callback function of the form OnVREvent(" + m_DataTypeName + ")");
            UnityEvent<T> cb = m_AllCallbacks[m_DataTypeName] as UnityEvent<T>;
            UnityEventTools.AddPersistentListener(cb, listener);
        }

        public void RemovePersistentListener(UnityAction listener)
        {
            Debug.Assert(m_DataTypeName == "", "Expected a callback function of the form OnVREvent()");
            UnityEventTools.RemovePersistentListener(m_Callback, listener);
        }

        public void RemovePersistentListener<T>(UnityAction<T> listener)
        {
            Debug.Assert(m_DataTypeName == typeof(T).Name,
                "Expected a callback function of the form OnVREvent(" + m_DataTypeName + ")");
            UnityEvent<T> cb = m_AllCallbacks[m_DataTypeName] as UnityEvent<T>;
            UnityEventTools.RemovePersistentListener(cb, listener);
        }
#endif


        public void InvokeWithVREvent(VREvent e)
        {
            m_AllCallbacks[m_DataTypeName].InvokeWithVREvent(e);
        }

        public void SetShowDataTypeInEditor(bool show)
        {
            m_ShowDataTypeInEditor = show;
        }


        [SerializeField] private string m_DataTypeName;
        [SerializeField] private bool m_ShowDataTypeInEditor;

        [NonSerialized] private Dictionary<string, IVRCallback> m_AllCallbacks;
        

        public void InitAllCallbacks()
        {
            // TODO: BUG: This list does not include all of the current VREvent Types.
            // This can likely be revised to follow the same approach as in VREventPrototypeAny of using
            // reflection to automatically detect all of the available types so that there are fewer
            // places to edit when a new type is added.  When fixing this, remember to also update the
            // editor scripts.  They assume all the callbacks are listed in this class and that they
            // all begin with "m_Callback*".  The editor for VREventPrototypeAny includes an alternative
            // strategy that should translate here as well.

            m_AllCallbacks = new Dictionary<string, IVRCallback>();

            m_Callback = new VRCallback();
            m_AllCallbacks.Add("", m_Callback);

            m_CallbackInt32 = new VRCallbackInt();
            m_AllCallbacks.Add(typeof(int).Name, m_CallbackInt32);

            m_CallbackSingle = new VRCallbackFloat();
            m_AllCallbacks.Add(typeof(float).Name, m_CallbackSingle);

            m_CallbackVector2 = new VRCallbackVector2();
            m_AllCallbacks.Add(typeof(Vector2).Name, m_CallbackVector2);

            m_CallbackVector3 = new VRCallbackVector3();
            m_AllCallbacks.Add(typeof(Vector3).Name, m_CallbackVector3);

            m_CallbackQuaternion = new VRCallbackQuaternion();
            m_AllCallbacks.Add(typeof(Quaternion).Name, m_CallbackQuaternion);

            // Location 1 of 2 to edit when adding a new event data type.
        }

        [SerializeField] private VRCallback m_Callback;
        [SerializeField] private VRCallbackInt m_CallbackInt32;
        [SerializeField] private VRCallbackFloat m_CallbackSingle;
        [SerializeField] private VRCallbackVector2 m_CallbackVector2;
        [SerializeField] private VRCallbackVector3 m_CallbackVector3;
        [SerializeField] private VRCallbackQuaternion m_CallbackQuaternion;

        // Location 2 of 2 to edit when adding a new event data type.
    }

} // end namespace
