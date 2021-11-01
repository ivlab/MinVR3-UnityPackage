using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    public interface IVREventListener
    {
        public void StartListening();
        public void StopListening();
        public void OnVREvent(VREvent vrEvent);
    }

} // namespace
