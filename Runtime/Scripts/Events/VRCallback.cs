using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System;

namespace IVLab.MinVR3
{

    public interface IVRCallback
    {
        public string GetDataTypeName();
        public bool Matches(VREvent vrEvent);
        public bool Matches(IVREventPrototype vrEvent);
        public void Invoke(VREvent vrEvent);
    }


    [Serializable]
    public class VRCallback : IVRCallback
    {
        public VRCallback()
        {
            m_DataTypeName = "";
            m_NotifyList = new UnityEvent();
        }

        public VRCallback(UnityAction callbackFunction)
        {
            m_DataTypeName = "";
            m_NotifyList = new UnityEvent();
            m_NotifyList.AddListener(callbackFunction);
        }

        public string dataTypeName {
            get => m_DataTypeName;
        }

        public string GetDataTypeName()
        {
            return m_DataTypeName;
        }

        public bool Matches(VREvent vrEvent)
        {
            return (vrEvent.GetDataTypeName() == dataTypeName);
        }

        public bool Matches(IVREventPrototype eventPrototype)
        {
            return (eventPrototype.GetDataTypeName() == dataTypeName);
        }

        public void Invoke()
        {
            m_NotifyList.Invoke();
        }

        public void Invoke(VREvent vrEvent)
        {
            m_NotifyList.Invoke();
        }

        public void AddCallback(UnityAction callbackFunc)
        {
            if (callbackFunc == null) return;
            m_NotifyList.AddListener(callbackFunc);
        }

        public void RemoveCallback(UnityAction callbackFunc)
        {
            if (callbackFunc == null) return;
            m_NotifyList.RemoveListener(callbackFunc);
        }

        [SerializeField] private UnityEvent m_NotifyList;
        [SerializeField] private string m_DataTypeName;
    }



    [Serializable]
    public class VRCallback<T> : IVRCallback
    {
        public VRCallback()
        {
            m_DataTypeName = typeof(T).Name;
            m_NotifyList = new UnityEvent<T>();
        }

        public VRCallback(UnityAction<T> callbackFunction)
        {
            m_DataTypeName = "";
            m_NotifyList = new UnityEvent<T>();
            m_NotifyList.AddListener(callbackFunction);
        }

        public string dataTypeName {
            get => m_DataTypeName;
        }

        public string GetDataTypeName()
        {
            return m_DataTypeName;
        }

        public bool Matches(VREvent vrEvent)
        {
            return (vrEvent.GetDataTypeName() == m_DataTypeName);
        }

        public bool Matches(IVREventPrototype eventPrototype)
        {
            return (eventPrototype.GetDataTypeName() == dataTypeName);
        }

        public void Invoke(VREvent vrEvent)
        {
            m_NotifyList.Invoke(((VREvent<T>)vrEvent).data);
        }

        public void AddCallback(UnityAction<T> callbackFunc)
        {
            if (callbackFunc == null) return;
            m_NotifyList.AddListener(callbackFunc);
        }

        public void RemoveCallback(UnityAction<T> callbackFunc)
        {
            if (callbackFunc == null) return;
            m_NotifyList.RemoveListener(callbackFunc);
        }

        [SerializeField] private UnityEvent<T> m_NotifyList;
        [SerializeField] private string m_DataTypeName;
    }



    [Serializable]
    public class VRCallbackAny : IVRCallback
    {
        public VRCallbackAny()
        {
            m_DataTypeName = "";
            InitAllCallbacks();
        }

        public VRCallbackAny(System.Type argDataType)
        {
            SetDataType(argDataType);
            InitAllCallbacks();
        }


        public static VRCallbackAny Create(UnityAction callbackFunction)
        {
            VRCallbackAny cb = new VRCallbackAny();
            cb.SetDataType("");
            cb.AddCallback(callbackFunction);
            return cb;
        }

        public static VRCallbackAny Create<T>(UnityAction<T> callbackFunction)
        {
            VRCallbackAny cb = new VRCallbackAny();
            cb.SetDataType(typeof(T));
            cb.AddCallback(callbackFunction);
            return cb;
        }

        public string dataTypeName {
            get => m_DataTypeName;
        }

        public string GetDataTypeName()
        {
            return m_DataTypeName;
        }

        public void SetDataType(System.Type dataType)
        {
            string name = "";
            if (dataType != null) {
                name = dataType.Name;
            }
            Debug.Assert(m_AllCallbacks.ContainsKey(name), "No callback available for type " + dataType.Name);
            m_DataTypeName = name;
        }

        public void SetDataType(string dataTypeName)
        {
            Debug.Assert(m_AllCallbacks.ContainsKey(dataTypeName), "No callback available for type " + dataTypeName);
            m_DataTypeName = dataTypeName;
        }

        public bool Matches(VREvent vrEvent)
        {
            return (vrEvent.GetDataTypeName() == m_AllCallbacks[m_DataTypeName].GetDataTypeName());
        }

        public bool Matches(IVREventPrototype eventPrototype)
        {
            return (eventPrototype.GetDataTypeName() == dataTypeName);
        }

        public void Invoke(VREvent vrEvent)
        {
            m_AllCallbacks[m_DataTypeName].Invoke(vrEvent);
        }

        public void AddCallback(UnityAction callbackFunc)
        {
            if (callbackFunc == null) return;
            Debug.Assert(m_DataTypeName == "",
                "The callback function should take 1 data argument that matches the current data type: " + m_DataTypeName);
            Debug.Assert(m_AllCallbacks.ContainsKey(dataTypeName), "No callback available for type " + dataTypeName);
            (m_AllCallbacks[m_DataTypeName] as VRCallback).AddCallback(callbackFunc);
        }

        public void RemoveCallback(UnityAction callbackFunc)
        {
            if (callbackFunc == null) return;
            Debug.Assert(m_DataTypeName == "",
                "The callback function should take 1 data argument that matches the current data type: " + m_DataTypeName);
            Debug.Assert(m_AllCallbacks.ContainsKey(dataTypeName), "No callback available for type " + dataTypeName);
            (m_AllCallbacks[m_DataTypeName] as VRCallback).RemoveCallback(callbackFunc);
        }

        public void AddCallback<T>(UnityAction<T> callbackFunc)
        {
            if (callbackFunc == null) return;
            Debug.Assert(m_DataTypeName == typeof(T).Name,
                "The callback function's data argument must match the current data type: " + m_DataTypeName);
            Debug.Assert(m_AllCallbacks.ContainsKey(dataTypeName), "No callback available for type " + dataTypeName);
            (m_AllCallbacks[m_DataTypeName] as VRCallback<T>).AddCallback(callbackFunc);
        }

        public void RemoveCallback<T>(UnityAction<T> callbackFunc)
        {
            if (callbackFunc == null) return;
            Debug.Assert(m_DataTypeName == typeof(T).Name,
                            "The callback function's data argument must match the current data type: " + m_DataTypeName);
            Debug.Assert(m_AllCallbacks.ContainsKey(dataTypeName), "No callback available for type " + dataTypeName);
            (m_AllCallbacks[m_DataTypeName] as VRCallback<T>).RemoveCallback(callbackFunc);
        }

        public void SetShowDataTypeDropdown(bool show)
        {
            m_ShowDataTypeDropdown = show;
        }


        private void InitAllCallbacks()
        {
            m_AllCallbacks = new Dictionary<string, IVRCallback>();

            m_Callback = new VRCallback();
            m_AllCallbacks.Add("", m_Callback);

            m_CallbackInt32 = new VRCallback<int>();
            m_AllCallbacks.Add(typeof(int).Name, m_CallbackInt32);

            m_CallbackSingle = new VRCallback<float>();
            m_AllCallbacks.Add(typeof(float).Name, m_CallbackSingle);

            m_CallbackVector2 = new VRCallback<Vector2>();
            m_AllCallbacks.Add(typeof(Vector2).Name, m_CallbackVector2);

            m_CallbackVector3 = new VRCallback<Vector3>();
            m_AllCallbacks.Add(typeof(Vector3).Name, m_CallbackVector3);

            m_CallbackQuaternion = new VRCallback<Quaternion>();
            m_AllCallbacks.Add(typeof(Quaternion).Name, m_CallbackQuaternion);

            m_CallbackTouch = new VRCallback<Touch>();
            m_AllCallbacks.Add(typeof(Touch).Name, m_CallbackTouch);
        }


        [SerializeField] private VRCallback m_Callback;
        [SerializeField] private VRCallback<int> m_CallbackInt32;
        [SerializeField] private VRCallback<float> m_CallbackSingle;
        [SerializeField] private VRCallback<Vector2> m_CallbackVector2;
        [SerializeField] private VRCallback<Vector3> m_CallbackVector3;
        [SerializeField] private VRCallback<Quaternion> m_CallbackQuaternion;
        [SerializeField] private VRCallback<Touch> m_CallbackTouch;
        private Dictionary<string, IVRCallback> m_AllCallbacks;

        [SerializeField] private string m_DataTypeName;
        [SerializeField] private bool m_ShowDataTypeDropdown = true;  // used in the editor only
    }

} // end namespace
