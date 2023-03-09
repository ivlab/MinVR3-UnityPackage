using UnityEngine;
using System.Collections.Generic;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Listens for two events: a base event and a modifier event. The modifier
    /// event must support some kind of ON/OFF logic (e.g. a keyboard key UP /
    /// DOWN). When the modifier is ON, this script will send "modified" events
    /// as specified by the modified event naming strategy
    /// </summary>
    [AddComponentMenu("MinVR/Input/Modified Event Producer")]
    public class ModifiedEventProducer : MonoBehaviour, IVREventFilter, IVREventProducer
    {
        void Reset()
        {
            m_BaseEvent = null;
            m_ModifierOnEvent = null;
            m_ModifierOffEvent = null;
            m_EventNameWhenModified = "/Modified";
        }

        void OnEnable()
        {
            VREngine.Instance.eventManager.AddEventFilter(this, -1);
        }

        void OnDisable()
        {
            VREngine.Instance?.eventManager?.RemoveEventFilter(this);
        }

        public bool FilterEvent(VREvent e, ref List<VREvent> filterResult)
        {
            bool modified = false;
            if (e.Matches(m_ModifierOnEvent))
            {
                m_ModifierOn = true;
            }
            else if (e.Matches(m_ModifierOffEvent))
            {
                m_ModifierOn = false;
            }

            if (e.Matches(m_BaseEvent))
            {
                if (m_ModifierOn)
                {
                    VREvent newEvt = e;
                    if (m_RenameStrategy == VREventAlias.AliasStrategy.RenameClone)
                        newEvt = e.Clone();
                    newEvt.name = e.name + m_EventNameWhenModified;
                    filterResult.Add(e);
                    modified = true;
                }
            }
            return modified;
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventPrototypes = new List<IVREventPrototype>();
            eventPrototypes.Add(VREventPrototype.Create(m_BaseEvent.GetEventName() + m_EventNameWhenModified));
            return eventPrototypes;
        }

        [SerializeField, Tooltip("Base event to listen for")]
        private VREventPrototypeAny m_BaseEvent;

        [SerializeField, Tooltip(
            "Modifier ON event to listen for. When modifier is on, " +
            "will send modified events according to the modified event naming strategy.")]
        private VREventPrototypeAny m_ModifierOnEvent;

        [SerializeField, Tooltip("Modifier OFF event to listen for")]
        private VREventPrototypeAny m_ModifierOffEvent;

        [SerializeField, Tooltip("String to append to event name when modifier has been applied")]
        private string m_EventNameWhenModified;

        [SerializeField, Tooltip("Set to RenameClone if you want the filter to clone the original event, and pass through both " +
            "the original and a new renamed clone.  Set to RenameOriginal (slightly more efficient) if you do not " +
            "need to listen for the original event anywhere in your application")]
        private VREventAlias.AliasStrategy m_RenameStrategy;

        private bool m_ModifierOn = false;
    }
}