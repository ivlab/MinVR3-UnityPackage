using System;
using System.Text.RegularExpressions;
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

        public const int EventAliasFilterPriority = 10;
        public const int DefaultFilterPriority = 20;


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

        /// <summary>
        /// Add an event filter and specify the priority (index) with which it
        /// should be run relative to other event filters.
        /// </summary>
        public void AddEventFilter(IVREventFilter filter, int priority = DefaultFilterPriority)
        {
            int index = m_EventFilters.FindIndex(entry => entry.Item2 == filter);
            if (index == -1) {
                m_EventFilters.Add(new Tuple<int, IVREventFilter>(priority, filter));
                m_EventFilters.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            }
        }

        public void RemoveEventFilter(IVREventFilter filter)
        {
            int index = m_EventFilters.FindIndex(entry => entry.Item2 == filter);
            m_EventFilters.RemoveAt(index);
        }

        public void QueueEvent(VREvent e)
        {
            lock (m_Queue) {
                m_Queue.Add(e);
            }
        }

        public void QueueEvent(int index, VREvent e)
        {
            lock (m_Queue) {
                m_Queue.Insert(index, e);
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

        List<VREvent> RunEventFilters(List<VREvent> inList)
        {
            List<VREvent> outList = new List<VREvent>();
            foreach (Tuple<int, IVREventFilter> priorityAndFilter in m_EventFilters) {
                IVREventFilter filter = priorityAndFilter.Item2;
                foreach (VREvent e in inList) {
                    List<VREvent> filterResult = new List<VREvent>();
                    if (filter.FilterEvent(e, ref filterResult)) {
                        // replace the event at eIndex with result
                        outList.AddRange(filterResult);
                    } else {
                        outList.Add(e);
                    }
                }
                inList = outList;
                outList = new List<VREvent>();
            }
            return outList;
        }

        List<VREvent> RunEventFilters(VREvent e)
        {
            // put the input in a list because filtering a single event can result in 0 or more events
            List<VREvent> inList = new List<VREvent>();
            inList.Add(e);

            if (m_EventFilters.Count == 0) {
                return inList;
            } else {
                // loop through each filter in order, note filters are already sorted by priority
                List<VREvent> outList = new List<VREvent>();
                foreach (Tuple<int, IVREventFilter> priorityAndFilter in m_EventFilters) {
                    IVREventFilter filter = priorityAndFilter.Item2;

                    // run the filter on each event in inList, saving the results in outList
                    outList = new List<VREvent>();
                    foreach (VREvent eInList in inList) {
                        List<VREvent> filterResult = new List<VREvent>();
                        if (filter.FilterEvent(eInList, ref filterResult)) {
                            // replace the event at eIndex with result
                            outList.AddRange(filterResult);
                        } else {
                            // filter did not change the event, just copy over the result
                            outList.Add(eInList);
                        }
                    }

                    // use the output of this filter as the input to the next
                    inList = outList;
                }
                return outList;
            }
        }

        public void ProcessEvent(VREvent e)
        {
            // run the event through the event filters
            List<VREvent> filterResults = RunEventFilters(e);

            // send the results to all event listeners
            foreach (VREvent eFiltered in filterResults)
            {
                if (m_ShowDebuggingOutput)
                {
                    Regex eventNamesToLog = new Regex(m_DebugOutputFilter);
                    if (m_DebugOutputFilter.Length == 0 || eventNamesToLog.IsMatch(eFiltered.GetName()))
                    {
                        Debug.Log("Processing event " + eFiltered.ToString());
                    }
                }
                foreach (Tuple<int, IVREventListener> listenerTuple in m_EventListeners.ToList())
                {
                    listenerTuple.Item2.OnVREvent(eFiltered);
                }
            }
        }

        public void ProcessEventQueue()
        {
            lock (m_Queue) {
                for (int i = 0; i < m_Queue.Count; i++) {
                    VREvent e = m_Queue[i];
                    ProcessEvent(e);

                    // if processing caused any derived events to be inserted
                    // in the queue process them right away
                    for (int j = 0; j < m_DerivedQueue.Count; j++) {
                        VREvent eDerived = m_DerivedQueue[j];
                        ProcessEvent(eDerived);
                    }
                    m_DerivedQueue.Clear();                    
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
            HashSet<IVREventProducer> eventProducersUnique = new HashSet<IVREventProducer>(eventProducers);

            // Get unique events that have been defined across the scene
            var uniqueEvents = new HashSet<IVREventPrototype>();
            foreach (IVREventProducer producer in eventProducersUnique) {
                var expectedFromThisSource = producer.GetEventPrototypes();
                foreach (IVREventPrototype prototype in expectedFromThisSource)
                {
                    if ((prototype.GetEventDataTypeName() == dataTypeName) || (dataTypeName == "*"))
                    {
                        uniqueEvents.Add(prototype);
                    }
                }
            }

            return uniqueEvents.ToList();
#else
            throw new Exception("VREventManager.GetMatchingEventPrototypes() should only be called from within the Unity Editor.");
#endif
        }


#if UNITY_EDITOR
        static public string GetUniqueEventPrototypeName(string baseName, bool includeInactive = true)
        {
            List<IVREventPrototype> existingPrototypes = GetAllEventPrototypes();

            string newName = baseName;
            int nameCounter = 0;

            while (nameCounter < existingPrototypes.Count) {
                bool match = false;
                int i = 0;
                while ((!match) && (i < existingPrototypes.Count)) {
                    if (existingPrototypes[i].GetEventName() == newName) {
                        match = true;
                    } else {
                        i++;
                    }
                }
                if (!match) {
                    return newName;
                } else {
                    nameCounter++;
                    newName = baseName + " " + nameCounter;
                }
            }
            // should never reach this point
            return baseName;
        }

#endif


        [Tooltip("Logs events to the console as they are processed")]
        public bool m_ShowDebuggingOutput = false;
        [Tooltip("Logs events to the console as they are processed")]
        public string m_DebugOutputFilter = "";

        // These lists are populated at runtime as other objects register with the manager
        [NonSerialized] private List<IPolledInputDevice> m_PolledInputDevices = new List<IPolledInputDevice>();
        [NonSerialized] private List<Tuple<int, IVREventListener>> m_EventListeners = new List<Tuple<int, IVREventListener>>();
        [NonSerialized] private List<Tuple<int, IVREventFilter>> m_EventFilters = new List<Tuple<int, IVREventFilter>>();
        [NonSerialized] private List<VREvent> m_Queue = new List<VREvent>();
        [NonSerialized] private List<VREvent> m_DerivedQueue = new List<VREvent>();

    }

} // namespace
