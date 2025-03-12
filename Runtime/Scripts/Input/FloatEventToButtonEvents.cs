using System.Collections;
using System.Collections.Generic;
using Codice.Client.Common;
using UnityEngine;

namespace IVLab.MinVR3
{

    [AddComponentMenu("MinVR/Input/Convert a Float Event to Button Events")]
    public class FloatEventToButtonEvents : MonoBehaviour, IVREventListener, IVREventProducer
    {
        void Reset()
        {
            m_FloatEvent = new VREventPrototypeFloat();
            m_ButtonDownEventName = "_Tool/FirstBtn/Down";
            m_ButtonUpEventName = "_Tool/FirstBtn/Up";
            m_MinFloatValue = 0.0f;
            m_MaxFloatValue = 1.0f;
            m_ThresholdValue = 0.1f;
            m_GenerateNormalizedFloatEvent = true;
            m_NormalizedFloatEventName = "_Tool/FirstBtn/NormalizedValue";
        }

        void Start()
        {
        }

        void OnEnable()
        {
            Debug.Assert(m_MaxFloatValue != m_ThresholdValue);
            StartListening();
        }

        void OnDisable()
        {
            StopListening();
        }

        public void OnVREvent(VREvent e)
        {
            if (e.Matches(m_FloatEvent)) {
                if (lastValue == float.NaN) {
                    // initialization
                    lastValue = e.GetData<float>();
                } else {
                    float curValue = e.GetData<float>();
                    if ((lastValue < m_ThresholdValue) && (curValue >= m_ThresholdValue)) {
                        // got a button down
                        VREngine.Instance.eventManager.InsertInQueue(new VREvent(m_ButtonDownEventName));
                    } else if ((lastValue > m_ThresholdValue) && (curValue <= m_ThresholdValue)) {
                        // got a button up
                        VREngine.Instance.eventManager.InsertInQueue(new VREvent(m_ButtonUpEventName));
                    }
                    lastValue = curValue;
                }

                if (m_GenerateNormalizedFloatEvent)
                {
                    float min = m_MinFloatValue;
                    float max = m_MaxFloatValue;
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

        [Tooltip("The name of the button down event to generate when the VREventFloat first becomes greater than the threshold")]
        [SerializeField] private string m_ButtonDownEventName;


        [Tooltip("The name of the button up event to generate when the VREventFloat first becomes less than the threshold.")]
        [SerializeField] private string m_ButtonUpEventName;


        [Tooltip("The min value of the float event for normalizing the value, i.e., when the float value equals this number the " +
        "normalized version of the event will be 0.0.")]
        [SerializeField] private float m_MinFloatValue;

        [Tooltip("The max value of the float event for normalizing the value, i.e., when the float value equals this number the " +
        "normalized version of the event will be 1.0.")]
        [SerializeField] private float m_MaxFloatValue;


        [Tooltip("The threshold that must be passed for the button up event to be generated. This should be as close to the min value as possible or even equal to the min value if the sensor doesn't have any noise.")]
        [SerializeField] private float m_ThresholdValue;


        [Tooltip("If true, generates a new VREvent whenever the float event being listened for is" +
			"received but normalizes the original float data value to a new range 0..1 based on" +
			"the buttonDown and buttonUp thresholds.")]
        [SerializeField] private bool m_GenerateNormalizedFloatEvent;
        [Tooltip("The name of the new VREvent to generate when Generate Normalized Float Event is true.")]
        [SerializeField] private string m_NormalizedFloatEventName;

        private float lastValue = float.NaN;
    }

}
