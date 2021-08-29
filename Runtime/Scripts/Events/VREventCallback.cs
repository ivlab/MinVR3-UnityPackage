using System.Collections.Generic;
using UnityEngine;
using System;

namespace IVLab.MinVR3
{
    /* This class along with the CallbackHelper* classes implement a wrapper around Unity's UnityEvent and UnityEvent<T> 
     * callback mechanisms.  UnityEvents are lovely to use in the editor and can support callbacks of the form OnEvent(),
     * OnEvent(int data), OnEvent(float data), OnEvent(Vector3 data), etc.  Given the name of a VREvent to listen for,
     * this wrapper adds the functionality of automatically selecting the appropriate function signiture to use for that
     * event by asking the VREventManager what datatype to expect.  
     * 
     * The datatype can be any of the common data types used with VREvents, currently:
     * - void
     * - bool
     * - int
     * - float
     * - Vector2
     * - Vector3
     * - Quaternion
     * - Touch
     * 
     * Support for additional types can be added with 3 lines of code:
     *   1,2: See the two lines marked TODO below. 
     *   3: See the line marked TODO in Editor/Scripts/VREventCallbackDrawer.cs
     */
    [Serializable]
    public class VREventCallback : ISerializationCallbackReceiver
    {
        public VREventReference listeningFor {
            get => m_ListenFor;
            set {
                m_ListenFor = value;
                Validate();
            }
        }
        
        public void Validate()
        {
            if (GetCallbackForDataType(m_ListenFor.dataType) != null) {
                m_CallbackType = m_ListenFor.dataType;
            } else {
                Debug.Log($"VREventCallback does not recognize {m_ListenFor.name}'s data type of '{m_ListenFor.dataType}';" +
                    " defaulting to a simple callback function with no data parameter.");
                m_CallbackType = "";
            }
            List<CallbackHelperBase> notSelectedCBs = GetCallbacksNotForDataType(m_CallbackType);
            foreach (var cb in notSelectedCBs) {
                cb.Reset();
            }
        }

        public CallbackHelperBase GetCallbackForDataType(string dataType)
        {
            if (m_AllCallbacks == null) {
                CreateAllCallbacksList();
            }
            foreach (var cb in m_AllCallbacks) {
                if (cb.GetDataType() == dataType) {
                    return cb;
                }
            }
            return null;
        }

        public List<CallbackHelperBase> GetCallbacksNotForDataType(string dataType)
        {
            if (m_AllCallbacks == null) {
                CreateAllCallbacksList();
            }
            List<CallbackHelperBase> not = new List<CallbackHelperBase>();
            foreach (var cb in m_AllCallbacks) {
                if (cb.GetDataType() != dataType) {
                    not.Add(cb);
                }
            }
            return not;
        }

        public void OnBeforeSerialize()
        {
            Validate();
        }

        public void OnAfterDeserialize()
        {
        }

        public void Invoke(VREventInstance e)
        {
            GetCallbackForDataType(m_CallbackType)?.Invoke(e);
        }


        [SerializeField] private VREventReference m_ListenFor = new VREventReference("", "");
        [SerializeField] private string m_CallbackType;

        [SerializeField] private CallbackHelperNoData m_VoidCallback = new CallbackHelperNoData();
        [SerializeField] private CallbackHelperWithData<bool> m_BoolCallback = new CallbackHelperWithData<bool>();
        [SerializeField] private CallbackHelperWithData<int> m_IntCallback = new CallbackHelperWithData<int>();
        [SerializeField] private CallbackHelperWithData<float> m_FloatCallback = new CallbackHelperWithData<float>();
        [SerializeField] private CallbackHelperWithData<Vector2> m_Vector2Callback = new CallbackHelperWithData<Vector2>();
        [SerializeField] private CallbackHelperWithData<Vector3> m_Vector3Callback = new CallbackHelperWithData<Vector3>();
        [SerializeField] private CallbackHelperWithData<Quaternion> m_QuaternionCallback = new CallbackHelperWithData<Quaternion>();
        [SerializeField] private CallbackHelperWithData<Touch> m_TouchCallback = new CallbackHelperWithData<Touch>();
        // TODO: Add more here as needed here (1/3)

        private void CreateAllCallbacksList()
        {
            m_AllCallbacks = new List<CallbackHelperBase>();
            m_AllCallbacks.Add(m_VoidCallback);
            m_AllCallbacks.Add(m_BoolCallback);
            m_AllCallbacks.Add(m_IntCallback);
            m_AllCallbacks.Add(m_FloatCallback);
            m_AllCallbacks.Add(m_Vector2Callback);
            m_AllCallbacks.Add(m_Vector3Callback);
            m_AllCallbacks.Add(m_QuaternionCallback);
            m_AllCallbacks.Add(m_TouchCallback);
            // TODO: Add more here as needed (2/3)
        }

        // runtime list of all of the individual typed callbacks above
        private List<CallbackHelperBase> m_AllCallbacks;
    }

} // namespace
