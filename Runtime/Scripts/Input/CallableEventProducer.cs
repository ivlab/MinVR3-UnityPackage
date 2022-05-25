using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Produces the defined event when the function ProduceEvent() is called.
    /// This functions as a null event producer if the function is never called.
    /// NOTE: only works with VREvents w/no payload.
    /// </summary>
    public class CallableEventProducer : MonoBehaviour, IVREventProducer
    {
        [SerializeField, Tooltip("Names of the possible event this script will produce")]
        private List<string> eventNames;

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();

            foreach (var evtName in eventNames)
            {
                eventsProduced.Add(VREventPrototype.Create(evtName));
            }

            return eventsProduced;
        }

        public void ProduceEvent(string name)
        {
            if (eventNames.Contains(name))
            {
                VREngine.Instance.eventManager.QueueEvent(new VREvent(name));
            }
            else
            {
                Debug.LogWarning($"CallableEventProducer: tried to produce an event `{name}` which is not one of the defined types `{string.Join(", ", eventNames)}`");
            }
        }
    }
}
