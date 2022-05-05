using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace IVLab.MinVR3
{
    /// <summary>
    /// A "null" event producer, which enables us to fill an Event Prototype slot
    /// without any events actually being sent there.
    /// </summary>
    public class NullEventProducer : MonoBehaviour, IVREventProducer
    {
        [SerializeField, Tooltip("Name of the event to produce a name w/no actual events")]
        private string eventName;

        [SerializeField, Tooltip("Event type to produce a name w/no actual events for")]
        private VREventTypeRepr eventType;

        public List<IVREventPrototype> GetEventPrototypes()
        {
            var assembly = Assembly.GetExecutingAssembly();
            VREventPrototypeAny any = new VREventPrototypeAny();
            var protoNames = assembly.GetTypes()
                .Where(t => t.IsAssignableFrom(typeof(VREventPrototype)))
                .Select(t => t.Name);

            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();

            Type evtType = any.AllEventPrototypes[eventType.ToString()].GetType();
            var createMethod = evtType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            var evtProto = createMethod.Invoke(null, new object[] { eventName }) as IVREventPrototype;
            eventsProduced.Add(evtProto);
            return eventsProduced;
        }
    }
}