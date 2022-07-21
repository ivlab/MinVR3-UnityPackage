using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace IVLab.MinVR3
{

   [DefaultExecutionOrder(VREventManager.ScriptPriority)]
   [DisallowMultipleComponent]
    public class VREventManager : MonoBehaviour
    {
        // This script should run immediately after the VREngine, which is in charage of synchronizing
        // events across all nodes when running in cluster mode.
        public const int ScriptPriority = VREngine.ScriptPriority + 1;

        public const int DefaultListenerPriority = 10;


        private void Reset()
        {
            m_ShowDebuggingOutput = false;
        }

        public void AddPolledInputDevice(IPolledInputDevice device)
        {
            if (!m_PolledInputDevices.Contains(device)) {
                m_PolledInputDevices.Add(device);
            }
        }

        public void RemovePolledInputDevice(IPolledInputDevice device)
        {
            m_PolledInputDevices.Remove(device);
        }


        public void AddEventListener(IVREventListener listener, int priority = DefaultListenerPriority)
        {
            int index = m_EventListeners.FindIndex(entry => entry.Item2 == listener);
            if (index == -1) {
                m_EventListeners.Add(new Tuple<int, IVREventListener>(priority, listener));
                m_EventListeners.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            }
        }

        public void RemoveEventListener(IVREventListener listener)
        {
            int index = m_EventListeners.FindIndex(entry => entry.Item2 == listener);
            m_EventListeners.RemoveAt(index);
        }

        public void AddEventFilter(IVREventFilter filter)
        {
            if (!m_EventFilters.Contains(filter)) {
                m_EventFilters.Add(filter);
                int maxPriority = 0;
                try { maxPriority = m_EventFilterPriorities.Max(); }
                catch (InvalidOperationException) { }
                m_EventFilterPriorities.Add(maxPriority + 1);
            }
        }

        /// <summary>
        /// Add an event filter and specify the priority (index) with which it
        /// should be run relative to other event filters.
        /// </summary>
        public void AddEventFilter(IVREventFilter filter, int priority)
        {
            int insertPriority = m_EventFilterPriorities.FindIndex(p => p >= priority);
            if (!m_EventFilters.Contains(filter)) {
                if (insertPriority >= 0)
                {
                    m_EventFilters.Insert(insertPriority, filter);
                    m_EventFilterPriorities.Insert(insertPriority, priority);
                }
                else
                {
                    m_EventFilters.Add(filter);
                    m_EventFilterPriorities.Add(priority);
                }
            }
        }

        public void RemoveEventFilter(IVREventFilter filter)
        {
            m_EventFilters.Remove(filter);
        }

        public void QueueEvent(VREvent e)
        {
            lock (m_Queue) {
                m_Queue.Add(e);
            }
        }

        public void InsertInQueue(VREvent e)
        {
            m_DerivedQueue.Add(e);
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



        public void PollInputDevices()
        {
            lock (m_Queue) {
                foreach (IPolledInputDevice device in m_PolledInputDevices) {
                    device.PollForEvents(ref m_Queue);
                }
            }
        }

        List<VREvent> RunEventFilters(VREvent e)
        {
            List<VREvent> resultAll = new List<VREvent>();
            bool caught = false;
            foreach (IVREventFilter filter in m_EventFilters) {
                // if a filter does catch the event, then it decides what to return as a result.
                // it could return the same event but with a new name, or the same event plus an
                // additional new event or it could consume the event and return nothing, etc.
                List<VREvent> result = new List<VREvent>();
                if (filter.FilterEvent(e, ref result)) {
                    caught = true;
                    resultAll.AddRange(result);
                }
            }
            if (!caught) {
                // if no filters caught the event, then pass it through unmodified
                resultAll.Add(e);
            }
            return resultAll;
        }

        public void ProcessEvent(VREvent e)
        {
            if (m_ShowDebuggingOutput) {
                Debug.Log("Processing event " + e.name);
            }
            foreach (Tuple<int,IVREventListener> listenerTuple in m_EventListeners.ToList()) {
                listenerTuple.Item2.OnVREvent(e);
            }
        }

        public void ProcessEventQueue()
        {
            lock (m_Queue) {
                foreach (VREvent e in m_Queue) {
                    // run each event in the queue through the event filters
                    List<VREvent> filterResults = RunEventFilters(e);
                    foreach (VREvent eFiltered in filterResults) {
                        // process each event to come out of the filters
                        ProcessEvent(eFiltered);

                        // if processing caused any derived events to be inserted
                        // in the queue process them right away
                        foreach (VREvent eDerived in m_DerivedQueue) {
                            ProcessEvent(eDerived);
                        }
                        m_DerivedQueue.Clear();
                    }
                }
                m_Queue.Clear();
            }
        }

        public void Update()
        {
            ProcessEventQueue();
        }


        /// <summary>
        /// Not fast; intended only for populating dropdown lists in the Unity Editor
        /// </summary>
        /// <returns>A list of all events produced by all sources</returns>
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
        /// <returns>A list of all events with the specified datatype produced by all sources</returns>
        static public List<IVREventPrototype> GetMatchingEventPrototypes(string dataTypeName, bool includeInactive = true)
        {
#if UNITY_EDITOR
            IVREventProducer[] eventProducers;
            if (includeInactive) {
                eventProducers = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<IVREventProducer>().ToArray();
            } else {
                eventProducers = FindObjectsOfType<MonoBehaviour>().OfType<IVREventProducer>().ToArray();
            }

            var expectedEvents = new List<IVREventPrototype>();
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
#else
            throw new Exception("VREventManager.GetMatchingEventPrototypes() should only be called from within the Unity Editor.");
#endif
        }


        [Tooltip("Logs events to the console as they are processed")]
        public bool m_ShowDebuggingOutput = false;

        // These lists are populated at runtime as other objects register with the manager
        [NonSerialized] private List<IPolledInputDevice> m_PolledInputDevices = new List<IPolledInputDevice>();
        [NonSerialized] private List<Tuple<int, IVREventListener>> m_EventListeners = new List<Tuple<int, IVREventListener>>();
        [NonSerialized] private List<IVREventFilter> m_EventFilters = new List<IVREventFilter>();
        [NonSerialized] private List<int> m_EventFilterPriorities = new List<int>();
        [NonSerialized] private List<VREvent> m_Queue = new List<VREvent>();
        [NonSerialized] private List<VREvent> m_DerivedQueue = new List<VREvent>();

    }

} // namespace
