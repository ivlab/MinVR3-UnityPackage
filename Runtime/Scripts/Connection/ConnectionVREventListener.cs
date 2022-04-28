using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// MonoBehaviour that takes all MinVR3 events and sends them along this
    /// VREventConnection (for example, to a web browser.)
    /// </summary>
    [RequireComponent(typeof(IVREventConnection))]
    public class ConnectionVREventListener : MonoBehaviour, IVREventFilter
    {
        public List<string> EventNames { get => eventsToSend; }
        public List<string> EventTypes { get => eventsTypesToSend; }

        [SerializeField, Tooltip("Event names to send along the connection. If empty, will send all events -- it's usually best to provide an explicit set of events to forward, though.")]
        private List<string> eventsToSend;

        [SerializeField, Tooltip("Event types to send along the connection.")]
        private List<string> eventsTypesToSend;

        private IVREventConnection connection;

        void Reset()
        {
            eventsToSend = new List<string>();
            eventsTypesToSend = new List<string>();
        }

        void Start()
        {
            VREngine.Instance.eventManager.AddEventFilter(this);
            connection = this.GetComponent<IVREventConnection>();
        }

        public bool FilterEvent(VREvent evt, ref List<VREvent> filterResult)
        {
            // Send the event to the connection, if it's one of the events we've selected to send along
            if (eventsToSend.Count == 0 || eventsToSend.Contains(evt.name))
            {
                connection.Send(evt);
            }

            // Does not actually modify the event list, only receives the events
            // and sends them along the connection
            return false;
        }
    }
}