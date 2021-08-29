using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    public interface IVREventDistributor
    {
        public void AddEventReceiver(IVREventReceiver eventReceiver);
        public void RemoveEventReceiver(IVREventReceiver eventReceiver);
    }

} // namespace
