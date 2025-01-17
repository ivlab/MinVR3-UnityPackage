using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    [AddComponentMenu("MinVR/Input/Convert a Float Event to Button Events")]
    public class FloatEventToButtonEvents : MonoBehaviour, IVREventListener, IVREventProducer
    {
        void Reset()
        {
            m_FloatEvent = new VREventPrototypeFloat();
            m_ButtonDownEventName = "MyButton/Down";
            m_ButtonUpEventName = "MyButton/Up";
            m_ButtonDownThreshold = 1.0f;
            m_ButtonUpThreshold = 0.0f;
            m_GenerateNormalizedFloatEvent = true;
            m_NormalizedFloatEventName = "MyNormalizedFloatValue";
        }

        void Start()
        {
        }

        void OnEnable()
        {
            Debug.Assert(m_ButtonDownThreshold != m_ButtonUpThreshold);
            StartListening();
        }

        void OnDisable()
        {
            StopListening();
        }

        public void OnVREvent(VREvent e)
        {
            if (e.Matches(m_FloatEvent)) {
                bool floatIncreasesWhenDown = m_ButtonDownThreshold >= m_ButtonUpThreshold;

                if (lastValue == float.NaN) {
                    // initialization
                    lastValue = e.GetData<float>();
                } else if (floatIncreasesWhenDown) {
                    // float values increase when the button is down
                    float curValue = e.GetData<float>();
                    if ((lastValue < m_ButtonDownThreshold) && (curValue >= m_ButtonDownThreshold)) {
                        // got a button down
                        VREngine.Instance.eventManager.InsertInQueue(new VREvent(m_ButtonDownEventName));
                    } else if ((lastValue > m_ButtonUpThreshold) && (curValue <= m_ButtonUpThreshold)) {
                        // got a button up
                        VREngine.Instance.eventManager.InsertInQueue(new VREvent(m_ButtonUpEventName));
                    }
                    lastValue = curValue;
                } else {
                    // reverse the logic, float values decrease when the button is down
                    float curValue = e.GetData<float>();
                    if ((lastValue > m_ButtonDownThreshold) && (curValue <= m_ButtonDownThreshold)) {
                        // got a button down
                        VREngine.Instance.eventManager.InsertInQueue(new VREvent(m_ButtonDownEventName));
                    } else if ((lastValue < m_ButtonUpThreshold) && (curValue >= m_ButtonUpThreshold)) {
                        // got a button up
                        VREngine.Instance.eventManager.InsertInQueue(new VREvent(m_ButtonUpEventName));
                    }
                    lastValue = curValue;
                }

                if (m_GenerateNormalizedFloatEvent)
                {
                    float min = floatIncreasesWhenDown ? m_ButtonUpThreshold : m_ButtonDownThreshold;
                    float max = floatIncreasesWhenDown ? m_ButtonDownThreshold : m_ButtonUpThreshold;
                    float normalizedValue = Mathf.Clamp((e.GetData<float>() - min) / (max - min), 0, 1);
                    VREngine.Instance.eventManager.InsertInQueue(
                        new VREventFloat(m_NormalizedFloatEventName, normalizedValue)
                    );
                }
            }
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventPrototypes = new List<IVREventPrototype>();
            eventPrototypes.Add(VREventPrototype.Create(m_ButtonDownEventName));
            eventPrototypes.Add(VREventPrototype.Create(m_ButtonUpEventName));
            if (m_GenerateNormalizedFloatEvent)
            {
                eventPrototypes.Add(VREventPrototypeFloat.Create(m_NormalizedFloatEventName));
            }
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

        [Tooltip("The VREventFloat to listen for.")]
        [SerializeField] private VREventPrototypeFloat m_FloatEvent;

        [Tooltip("The name of the button down event to generate when the VREventFloat passes the button down threshold.")]
        [SerializeField] private string m_ButtonDownEventName;

        [Tooltip("The threshold that must be passed for the button down event to be generated.")]
        [SerializeField] private float m_ButtonDownThreshold;

        [Tooltip("The name of the button up event to generate when the VREventFloat passes the button up threshold.")]
        [SerializeField] private string m_ButtonUpEventName;

        [Tooltip("The threshold that must be passed for the button up event to be generated.")]
        [SerializeField] private float m_ButtonUpThreshold;


        [Tooltip("If true, generates a new VREvent whenever the float event being listened for is" +
			"received but normalizes the original float data value to a new range 0..1 based on" +
			"the buttonDown and buttonUp thresholds.")]
        [SerializeField] private bool m_GenerateNormalizedFloatEvent;
        [Tooltip("The name of the new VREvent to generate when Generate Normalized Float Event is true.")]
        [SerializeField] private string m_NormalizedFloatEventName;

        private float lastValue = float.NaN;
    }

}
