using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    public interface IVREventReceiver
    {
        public void OnVREvent(VREvent vrEvent);
    }

} // namespace
