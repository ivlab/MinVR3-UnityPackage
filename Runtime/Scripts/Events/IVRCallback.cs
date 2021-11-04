using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IVLab.MinVR3
{

    public interface IVRCallback
    {
        void InvokeWithVREvent(VREvent e);
    }

} // namespace
