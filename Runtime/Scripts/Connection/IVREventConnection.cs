using UnityEngine.Events;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Defines an interface for classes that can send and/or receive VREvents over some sort
    /// of remote network connection.  Unlike the network transport provided by MinVR3's
    /// Cluster Mode, which supports frame-level synchronization of the entire event queue,
    /// this network connection is intended for sending/receiving a subset of VREvents and
    /// for situations where the clients at either end of the connection are running
    /// asynchronously.  This can be used to connect a MinVR Unity app to a web browser or
    /// to connect multiple MinVR apps running on various brands of headsets -- basically
    /// all situations *other than* a cluster powering a tiled display.
    /// </summary>
    public interface IVREventConnection
    {
        /// <summary>
        /// Returns true if the VREventConnection supports sending VREvents.
        /// </summary>
        bool CanSend();

        /// <summary>
        /// Send a VR event to the other end of this "connection"
        /// </summary>
        void Send(in VREvent evt);


        /// <summary>
        /// Returns true if the VREventConnection supports receiving VREvents.
        /// </summary>
        bool CanReceive();

        /// <summary>
        /// Subscribe to OnVREventReceived to recieve a callback whenever a VREvent is
        /// received over the network "connection",
        /// </summary>
        OnVREventReceived.OnVREventReceivedDelegate OnVREventReceived { get; set; }
    }

    public static class OnVREventReceived
    {
        /// <summary>
        /// Delagate to define the structure for OnVREventReceived callbacks.
        /// </summary>
        public delegate void OnVREventReceivedDelegate(VREvent evt);
    }


    // Notes on pros/cons of an alternate implementation using UnityEvents:

    // [System.Serializable]
    // public class NetworkVREvent : UnityEvent<VREvent> { }

    // UnityEvents are an alternate way to handle VREvents that are
    // received. However, this does not work nicely because UnityEvents'
    // `Invoke()` method needs to be called from the Main thread, which
    // these networked communicators frequently aren't.
    // NetworkVREvent OnVREventReceived { get; }
}