using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    [AddComponentMenu("MinVR/Input/VREvent Alias")]
    public class VREventAlias : MonoBehaviour, IVREventFilter, IVREventProducer
    {
        public enum AliasStrategy
        {
            RenameOriginal,
            RenameClone
        }

        void OnEnable()
        {
            VREngine.Instance.eventManager.AddEventFilter(this);
        }

        void OnDisable()
        {
            VREngine.Instance?.eventManager?.RemoveEventFilter(this);
        }

        protected virtual void Reset()
        {
            m_AliasStrategy = AliasStrategy.RenameOriginal;
            m_AliasEventName = "MyEvent (Alias)";
            m_OriginalEvents = new List<VREventPrototypeAny>();
        }

        public bool FilterEvent(VREvent e, ref List<VREvent> filterResult)
        {
            foreach (var prototype in m_OriginalEvents) {
                if (e.Matches(prototype)) {
                    if (m_AliasStrategy == AliasStrategy.RenameOriginal) {
                        e.name = m_AliasEventName;
                        filterResult.Add(e);
                        return true;
                    } else {
                        VREvent e2 = e.Clone();
                        e2.name = m_AliasEventName;
                        filterResult.Add(e);
                        filterResult.Add(e2);
                        return true;
                    }
                }
            }
            return false;
        }


        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> generatedEvents = new List<IVREventPrototype>();
            if ((m_OriginalEvents.Count > 0) && (m_OriginalEvents[0].GetEventName() != "")) {
                VREventPrototypeAny p = VREventPrototypeAny.Create(m_AliasEventName);
                p.SetEventDataType(m_OriginalEvents[0].GetEventDataTypeName());
                generatedEvents.Add(p);
            }
            return generatedEvents;
        }

        void OnValidate()
        {
            if (m_OriginalEvents.Count >= 2) {
                string dataType = m_OriginalEvents[0].GetEventDataTypeName();
                for (int i = m_OriginalEvents.Count - 1; i >= 1; i--) {
                    if (m_OriginalEvents[i].GetEventDataTypeName() != dataType) {
                        Debug.LogError("All events mapped to the alias '" + m_AliasEventName + "' must have the same data type");
                        m_OriginalEvents.RemoveAt(i);
                    }
                }
            }
        }

        [Tooltip("Set to RenameClone if you want the filter to clone the original event, and pass through both " +
            "the original and a new renamed clone.  Set to RenameOriginal (slightly more efficient) if you do not " +
            "need to listen for the original event anywhere in your application")]
        [SerializeField] private AliasStrategy m_AliasStrategy = AliasStrategy.RenameOriginal;
        [Tooltip("The original event(s) are renamed to this.")]
        [SerializeField] private string m_AliasEventName = "MyEvent (Alias)";
        [Tooltip("One or more events that you wish to rename or clone and rename using the alias")]
        [SerializeField] private List<VREventPrototypeAny> m_OriginalEvents = new List<VREventPrototypeAny>();
    }

} // end namespace
