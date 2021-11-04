using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    public interface IVREventListener
    {
        void StartListening();
        void StopListening();
        void OnVREvent(VREvent vrEvent);
    }

} // namespace
