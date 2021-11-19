using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    public interface IPolledInputDevice : IVREventProducer
    {
        /// <summary>
        /// Calling Poll adds to the queue any input events generated since the last
        /// call to poll.
        /// </summary>
        void PollForEvents(ref List<VREvent> eventQueue);
    }

}

// end namespace
