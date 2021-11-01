using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{

    public interface IVREventPrototype
    {
        public void SetEventName(string eventName);

        public string GetEventName();
        public string GetEventDataTypeName();
        public string GetEventDisplayName();
    }

} // namespace
