using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// MonoBehaviour that takes events from a specified VREventConnection and
    /// places them in the VR Event Manager's Queue.
    /// </summary>
    [RequireComponent(typeof(IVREventConnection))]
    public class ConnectionVREventProducer : MonoBehaviour, IVREventProducer
    {
        [SerializeField, Tooltip("VREventConnection source to convert events from")]
        private IVREventConnection connection;

        [SerializeField, Tooltip("Events that you expect to receive from the in this application")]
        private List<string> expectedEventNames;

        [SerializeField, Tooltip("Prototype event types that correspond with the above event names")]
        private List<string> expectedPrototypeEventTypes;

        void Reset()
        {
            expectedEventNames = new List<string>();
            expectedPrototypeEventTypes = new List<string>();
        }

        void Start()
        {
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var protoNames = assembly.GetTypes()
                .Where(t => t.IsAssignableFrom(typeof(VREventPrototype)))
                .Select(t => t.Name);

            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();
            for (int i = 0; i < Math.Min(expectedEventNames.Count, expectedPrototypeEventTypes.Count); i++)
            {
                try
                {
                    Type evtType = assembly.GetType(expectedPrototypeEventTypes[i]);
                    var createMethod = evtType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                    var evtProto = createMethod.Invoke(null, new object[] { expectedEventNames[i] }) as IVREventPrototype;
                    eventsProduced.Add(evtProto);
                }
                catch
                {
                    Debug.LogError("Unable to find a prototype event type matching " + expectedPrototypeEventTypes[i] + ", try one of the following: " + string.Join(", ", protoNames));
                }
            }
            return eventsProduced;
        }
    }
}