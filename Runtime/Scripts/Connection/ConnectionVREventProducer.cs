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
    [ExecuteInEditMode]
    public class ConnectionVREventProducer : MonoBehaviour, IVREventProducer
    {
        [SerializeField, Tooltip("VREventConnection source to convert events from")]
        private IVREventConnection connection;

        [SerializeField, Tooltip("Events that you expect to receive from the in this application")]
        private List<string> m_ExpectedEventNames;

        [SerializeField, Tooltip("Prototype event types that correspond with the above event names")]
        private List<string> m_ExpectedDataTypes;

        public List<string> EventNames { get => m_ExpectedEventNames; }
        public List<string> EventTypes { get => m_ExpectedDataTypes; }

        void Reset()
        {
            m_ExpectedEventNames = new List<string>();
            m_ExpectedDataTypes = new List<string>();
        }

        void Start()
        {
            connection = this.GetComponent<IVREventConnection>();
            connection.OnVREventReceived += VREventHandler;
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            var assembly = Assembly.GetExecutingAssembly();
            VREventPrototypeAny any = new VREventPrototypeAny();
            var protoNames = assembly.GetTypes()
                .Where(t => t.IsAssignableFrom(typeof(VREventPrototype)))
                .Select(t => t.Name);

            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();
            for (int i = 0; i < Math.Min(m_ExpectedEventNames.Count, m_ExpectedDataTypes.Count); i++)
            {
                try
                {
                    Type evtType = any.AllEventPrototypes[m_ExpectedDataTypes[i]].GetType();
                    var createMethod = evtType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                    var evtProto = createMethod.Invoke(null, new object[] { m_ExpectedEventNames[i] }) as IVREventPrototype;
                    eventsProduced.Add(evtProto);
                }
                catch
                {
                    Debug.LogError("Unable to find a prototype event type matching " + m_ExpectedDataTypes[i] + ", try one of the following: " + string.Join(", ", protoNames));
                }
            }
            return eventsProduced;
        }

        private void VREventHandler(VREvent evt)
        {
            VREngine.Instance.eventManager.QueueEvent(evt);
        }
    }
}