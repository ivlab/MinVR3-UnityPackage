using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace IVLab.MinVR3
{

    [DisallowMultipleComponent]
    [AddComponentMenu("MinVR/Engine/VREvent Manager")]
    public class VREventManager : MonoBehaviour, IVREventDistributor
    {
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
            m_Queue.Add(new VREventInstance(eventName));
        }

        public void QueueEvent<T>(string eventName, T eventData)
        {
            m_Queue.Add(new VREventInstance<T>(eventName, eventData));
        }


        public void Update()
        {
            foreach (VREventInstance e in m_Queue) {
                foreach (IVREventReceiver r in m_EventReceivers) {
                    r.OnVREvent(e);
                }
            }
            m_Queue.Clear();
        }


        // These lists are populated at runtime as other objects register with the manager
        [NonSerialized] private List<IVREventProducer> m_EventProducers = new List<IVREventProducer>();
        [NonSerialized] private List<IVREventReceiver> m_EventReceivers = new List<IVREventReceiver>();

        [NonSerialized] private List<VREventInstance> m_Queue = new List<VREventInstance>();



        
        /// <summary>
        /// Not fast; intended only for populating dropdown lists in the Unity Editor
        /// </summary>
        /// <returns>A map of all events produced by all sources, where the key is the event name and
        /// the value is a string representation of the event datatype or "" if the event has no data.</returns>
        public Dictionary<string, string> GetAllEventNamesAndTypes()
        {
            var expectedEvents = new Dictionary<string, string>();

            var eventProducers = FindObjectsOfType<MonoBehaviour>().OfType<IVREventProducer>();
            foreach (var producer in eventProducers) {
                var expectedFromThisSource = producer.GetEventNamesAndTypes();
                foreach (string e in expectedFromThisSource.Keys) {
                    expectedEvents.Add(e, expectedFromThisSource[e]);
                }
            }
            return expectedEvents;
        }

        public string GetEventDataType(string eventName)
        {
            var expectedEvents = GetAllEventNamesAndTypes();
            if (expectedEvents.ContainsKey(eventName)) {
                return expectedEvents[eventName];
            } else {
                return "";
            }
        }


        static public string EventDisplayName(string eventName, string dataType)
        {
            if (dataType == "") {
                return eventName;
            } else {
                return eventName + " (" + dataType + ")";
            }
        }
    }

} // namespace
