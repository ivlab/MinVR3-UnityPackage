using UnityEngine;
using System;

namespace IVLab.MinVR3
{

    [Serializable]
    public class VREventReference
    {
        public VREventReference(string eventName, string eventDataType, bool lockDataType=false)
        {
            m_EventName = eventName;
            m_EventDataType = eventDataType;
            m_LockDataType = lockDataType;
        }

        public string name {
            get => m_EventName;
            set => m_EventName = value;
        }

        public string dataType {
            get => m_EventDataType;
            set {
                Debug.Assert(!m_LockDataType, "Cannot change the event reference data type; it is locked.");
                if (!m_LockDataType) {
                    m_EventDataType = value;
                }
            }
        }

        [SerializeField] private string m_EventName;
        [SerializeField] private string m_EventDataType;
        [SerializeField] private bool m_LockDataType;
    }

} // namespace
