using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace IVLab.MinVR3
{

    [DisallowMultipleComponent]
    public class VREventManager : MonoBehaviour, IVREventDistributor
    {
        [Tooltip("Logs events to the console as they are processed")]
        public bool m_ShowDebuggingOutput = true;

        
        public void AddEventProducer(IVREventProducer eventProducer)
        {
            if (!m_EventProducers.Contains(eventProducer)) {
                m_EventProducers.Add(eventProducer);
            }
        }

        public void RemoveEventProducer(IVREventProducer eventProducer)
        {
            m_EventProducers.Remove(eventProducer);
        }


        public void AddEventReceiver(IVREventReceiver eventReceiver)
        {
            if (!m_EventReceivers.Contains(eventReceiver)) {
                m_EventReceivers.Add(eventReceiver);
            }
        }

        public void RemoveEventReceiver(IVREventReceiver eventReceiver)
        {
            m_EventReceivers.Remove(eventReceiver);
        }




        public void QueueEvent(string eventName)
        {
            m_Queue.Add(new VREvent(eventName));
        }

        public void QueueEvent<T>(string eventName, T eventData)
        {
            m_Queue.Add(new VREvent<T>(eventName, eventData));
        }


        public List<VREvent> GetEventQueue()
        {
            return m_Queue;
        }

        public void SetEventQueue(List<VREvent> newQueue)
        {
            m_Queue = newQueue;
        }

        public void Update()
        {
            foreach (VREvent e in m_Queue) {
                if (m_ShowDebuggingOutput) {
                    Debug.Log("Processing event " + e.name);
                }
                foreach (IVREventReceiver r in m_EventReceivers) {
                    r.OnVREvent(e);
                }
            }
            m_Queue.Clear();
        }


        // These lists are populated at runtime as other objects register with the manager
        [NonSerialized] private List<IVREventProducer> m_EventProducers = new List<IVREventProducer>();
        [NonSerialized] private List<IVREventReceiver> m_EventReceivers = new List<IVREventReceiver>();

        [NonSerialized] private List<VREvent> m_Queue = new List<VREvent>();



        
        /// <summary>
        /// Not fast; intended only for populating dropdown lists in the Unity Editor
        /// </summary>
        /// <returns>A list of all events produced by all sources
        static public List<IVREventPrototype> GetAllEventPrototypes()
        {
            return GetMatchingEventPrototypes("*");
        }


        /// <summary>
        /// Not fast; intended only for populating dropdown lists in the Unity Editor.
        /// For a given data type, the dataTypeString should be equal to the value returned by
        /// typeof(T).Name. The string "" will match events that do not have a data payload, and
        /// the wildcard character * will match events of any datatype.
        /// </summary>
        /// <returns>A list of all events with the specified datatype produced by all sources
        static public List<IVREventPrototype> GetMatchingEventPrototypes(string dataTypeName)
        {
            var expectedEvents = new List<IVREventPrototype>();

            var eventProducers = FindObjectsOfType<MonoBehaviour>().OfType<IVREventProducer>();
            foreach (var producer in eventProducers) {
                var expectedFromThisSource = producer.GetEventPrototypes();
                foreach (IVREventPrototype e in expectedFromThisSource) {
                    if ((e.GetDataTypeName() == dataTypeName) || (dataTypeName == "*")) {
                        foreach (IVREventPrototype e2 in expectedEvents) {
                            if ((e.GetEventName() == e2.GetEventName()) && (e.GetDataTypeName() != e2.GetDataTypeName())) {
                                throw new Exception($"Two IVREventProducers expect to produce an event named {e.GetEventName()} but the expected data types differ: '{e.GetDataTypeName()}' and '{e2.GetDataTypeName()}'");
                            }
                        }
                        expectedEvents.Add(e);
                    }
                }
            }
            return expectedEvents;
        }

    }

} // namespace
