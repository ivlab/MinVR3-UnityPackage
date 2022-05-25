using UnityEngine.Events;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Event communicator for transferring asynchronous events to/from MinVR over the network.
    /// </summary>
    public interface IVREventConnection
    {
        /// <summary>
        /// Send a VR event to the other end of this "connection"
        /// </summary>
        public void Send(in VREvent evt);

        // UnityEvents are an alternate way to handle VREvents that are
        // received. However, this does not work nicely because UnityEvents'
        // `Invoke()` method needs to be called from the Main thread, which
        // these networked communicators frequently aren't.
        // NetworkVREvent OnVREventReceived { get; }

        /// <summary>
        /// Delegate that is called whenever a VREvent is received over the network "connection"
        /// </summary>
        public VREventReceivedDelegate OnVREventReceived { get; set; }

        /// <summary>
        /// Definition for the VREventReceived delegate
        /// </summary>
        public delegate void VREventReceivedDelegate(VREvent evt);
    }

    // [System.Serializable]
    // Alternate implementation using UnityEvents
    // public class NetworkVREvent : UnityEvent<VREvent> { }
}