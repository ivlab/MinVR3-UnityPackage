using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Listens for two 3D position events (typically from VR trackers) and generates VREvents for when the two
    /// trackers are moved close together (i.e., in proximity to each other) and when they move apart.
    /// </summary>
    [AddComponentMenu("MinVR/Input/Proximity Event")]
    public class ProximityEvent : MonoBehaviour, IVREventListener, IVREventProducer
    {
        void Reset()
        {
            m_PositionEvent1 = new VREventPrototypeVector3();
            m_PositionEvent2 = new VREventPrototypeVector3();
            m_ProximityThreshold = 1.0f;
            m_BaseEventName = "Proximity";
        }

        void Start()
        {
            m_Pos1 = new Vector3();
            m_Pos2 = new Vector3();
            m_HaveData1 = false;
            m_HaveData2 = false;
            m_InProximity = false;
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
            if (e.Matches(m_PositionEvent1)) {
                m_Pos1 = e.GetData<Vector3>();
                m_HaveData1 = true;
            } else if (e.Matches(m_PositionEvent2)) {
                m_Pos2 = e.GetData<Vector3>();
                m_HaveData2 = true;
            }

            if (m_HaveData1 && m_HaveData2) {
                if (!m_InProximity) {
                    if ((m_Pos2 - m_Pos1).magnitude < m_ProximityThreshold) {
                        VREngine.Instance.eventManager.InsertInQueue(new VREvent(m_BaseEventName + "/Close"));
                        m_InProximity = true;
                    }
                } else {
                    if ((m_Pos2 - m_Pos1).magnitude > m_ProximityThreshold) {
                        VREngine.Instance.eventManager.InsertInQueue(new VREvent(m_BaseEventName + "/Far"));
                        m_InProximity = false;
                    }
                }
            }
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventPrototypes = new List<IVREventPrototype>();
            eventPrototypes.Add(VREventPrototype.Create(m_BaseEventName + "/Close"));
            eventPrototypes.Add(VREventPrototype.Create(m_BaseEventName + "/Far"));
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

        [SerializeField] private VREventPrototypeVector3 m_PositionEvent1;
        [SerializeField] private VREventPrototypeVector3 m_PositionEvent2;
        [SerializeField] private float m_ProximityThreshold;
        [SerializeField] private string m_BaseEventName;

        private Vector3 m_Pos1;
        private bool m_HaveData1;
        private Vector3 m_Pos2;
        private bool m_HaveData2;
        private bool m_InProximity;
    }

} // end namespace
