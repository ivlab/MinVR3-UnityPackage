using System;
using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// String representation of a VREvent Type, useful for choosing VREvent
    /// producer types with unknown origin
    /// </summary>
    [System.Serializable]
    public class VREventTypeRepr
    {
        [SerializeField]
        private string eventType;

        public Type EventDataType { get => VREvent.AvailableDataTypes[eventType]; }

        public override string ToString() => eventType;
    }
}