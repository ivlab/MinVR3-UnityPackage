using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace IVLab.MinVR3
{

    [DisallowMultipleComponent]
    public class VREventManager : MonoBehaviour
    {
        [Tooltip("Logs events to the console as they are processed")]
        public bool m_ShowDebuggingOutput = false;

        
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


        public void AddEventReceiver(IVREventListener eventReceiver)
        {
            if (!m_EventReceivers.Contains(eventReceiver)) {
                m_EventReceivers.Add(eventReceiver);
            }
        }

        public void RemoveEventReceiver(IVREventListener eventReceiver)
        {
            m_EventReceivers.Remove(eventReceiver);
        }



        public void QueueEvent(string eventName)
        {
            lock (m_Queue) {
                m_Queue.Add(new VREvent(eventName));
            }
        }

        public void QueueEvent<T>(string eventName, T eventData)
        {
            lock (m_Queue) {
                m_Queue.Add(new VREvent<T>(eventName, eventData));
            }
        }

        public List<VREvent> GetEventQueue()
        {
            return m_Queue;
        }

        public void SetEventQueue(List<VREvent> newQueue)
        {
            lock (m_Queue) {
                m_Queue = newQueue;
            }
        }

        public void Update()
        {
            lock (m_Queue) {
                foreach (VREvent e in m_Queue) {
                    if (m_ShowDebuggingOutput) {
                        Debug.Log("Processing event " + e.name);
                    }
                    foreach (IVREventListener r in m_EventReceivers) {
                        r.OnVREvent(e);
                    }
                }
                m_Queue.Clear();
            }
        }
        
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
                    if ((e.GetEventDataTypeName() == dataTypeName) || (dataTypeName == "*")) {
                        foreach (IVREventPrototype e2 in expectedEvents) {
                            if ((e.GetEventName() == e2.GetEventName()) && (e.GetEventDataTypeName() != e2.GetEventDataTypeName())) {
                                throw new Exception($"Two IVRInputDevices expect to produce an event named {e.GetEventName()} but the expected data types differ: '{e.GetEventDataTypeName()}' and '{e2.GetEventDataTypeName()}'");
                            }
                        }
                        expectedEvents.Add(e);
                    }
                }
            }
            return expectedEvents;
        }

        // These lists are populated at runtime as other objects register with the manager
        [NonSerialized] private List<IVREventProducer> m_EventProducers = new List<IVREventProducer>();
        [NonSerialized] private List<IVREventListener> m_EventReceivers = new List<IVREventListener>();

        [NonSerialized] private List<VREvent> m_Queue = new List<VREvent>();
    }


} // namespace
