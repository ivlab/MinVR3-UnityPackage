using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    [AddComponentMenu("MinVR/Input/Convert Button Events to a Float Event")]
    public class ButtonEventsToFloatEvent : MonoBehaviour, IVREventListener, IVREventProducer
    {
        void Reset()
        {
            m_ButtonDownEvent = new VREventPrototype();
            m_ButtonUpEvent = new VREventPrototype();
            m_VREventFloatName = "MyFloatEvent";
            m_ButtonDownDataValue = 1.0f;
            m_ButtonUpDataValue = 0.0f;
        }
    
        void Start()
        {
        }

        void OnEnable()
        {
            StartListening();
        }

        void OnDisable()
        {
            StopListening();
        }

        public void OnVREvent(VREvent e)
        {
            if (e.Matches(m_ButtonDownEvent)) {
                VREngine.Instance.eventManager.InsertInQueue(new VREventFloat(m_VREventFloatName, m_ButtonDownDataValue));
            } else if (e.Matches(m_ButtonUpEvent)) {
                VREngine.Instance.eventManager.InsertInQueue(new VREventFloat(m_VREventFloatName, m_ButtonUpDataValue));
            }
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventPrototypes = new List<IVREventPrototype>();
            eventPrototypes.Add(VREventPrototypeFloat.Create(m_VREventFloatName));
            return eventPrototypes;
        }

        public void StartListening()
        {
            VREngine.Instance.eventManager.AddEventListener(this, VREventManager.DefaultListenerPriority - 1);
        }

        public void StopListening()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }

        [Tooltip("The button down event to listen for.")]
        [SerializeField] private VREventPrototype m_ButtonDownEvent;

        [Tooltip("The button up event to listen for.")]
        [SerializeField] private VREventPrototype m_ButtonUpEvent;

        [Tooltip("The name to use for the VREventFloats that are generated.")]
        [SerializeField] private string m_VREventFloatName;

        [Tooltip("The data value to use for the VREventFloat when the button down event is received.")]
        [SerializeField] private float m_ButtonDownDataValue;

        [Tooltip("The data value to use for the VREventFloat when the button up event is received.")]
        [SerializeField] private float m_ButtonUpDataValue;
    }

}
