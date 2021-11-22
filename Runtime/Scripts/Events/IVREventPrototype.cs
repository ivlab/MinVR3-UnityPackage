using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{

    public interface IVREventPrototype
    {
        void SetEventName(string eventName);

        string GetEventName();
        string GetEventDataTypeName();
        string GetEventDisplayName();

        IVREventPrototype Clone();
    }

} // namespace
