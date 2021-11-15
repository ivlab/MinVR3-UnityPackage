using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zSpace.Core;
using zSpace.Core.Input;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Grabs input from the zSpace using the zCore library and converts that input to VREvents
    /// </summary>
    [AddComponentMenu("MinVR/Input/zSpace Input")]
    public class zSpaceInput : MonoBehaviour, IPolledInputDevice
    {
        void Reset()
        {
            m_HeadEventBaseName = "Head";
            m_StylusEventBaseName = "Stylus";
            m_ButtonEventBaseNames = new string[3];
            m_ButtonEventBaseNames[0] = "Stylus/Primary Button";
            m_ButtonEventBaseNames[1] = "Stylus/Button 2";
            m_ButtonEventBaseNames[2] = "Stylus/Button 3";
        }

        private void OnEnable()
        {
            VREngine.instance.eventManager.AddPolledInputDevice(this);
        }

        private void OnDisable()
        {
            VREngine.instance?.eventManager?.RemovePolledInputDevice(this);
        }

        void Start()
        {
            m_zCameraRig = GameObject.FindObjectOfType<ZCameraRig>();
            m_zStylus = GameObject.FindObjectOfType<ZStylus>();
            Debug.Assert(m_zCameraRig && m_zStylus,
                "zSpace input requires a ZCameraRig and a ZStylus object to be active in the scene.");
            m_LastStylusPos = new Vector3();
            m_LastStylusRot = Quaternion.identity;
            m_LastHeadPos = new Vector3();
            m_LastHeadRot = Quaternion.identity;
        }

        public void PollForEvents(ref List<VREvent> eventQueue)
        {
            if (m_zStylus.transform.position != m_LastStylusPos) {
                eventQueue.Add(new VREventVector3(m_StylusEventBaseName + "/Position", m_zStylus.transform.position));
                m_LastStylusPos = m_zStylus.transform.position;
            }
            if (m_zStylus.transform.rotation != m_LastStylusRot) {
                eventQueue.Add(new VREventQuaternion(m_StylusEventBaseName + "/Rotation", m_zStylus.transform.rotation));
                m_LastStylusRot = m_zStylus.transform.rotation;
            }

            if (m_zCameraRig.transform.position != m_LastHeadPos) {
                eventQueue.Add(new VREventVector3(m_HeadEventBaseName + "/Position", m_zCameraRig.transform.position));
                m_LastHeadPos = m_zCameraRig.transform.position;
            }
            if (m_zCameraRig.transform.rotation != m_LastHeadRot) {
                eventQueue.Add(new VREventQuaternion(m_HeadEventBaseName + "/Rotation", m_zCameraRig.transform.rotation));
                m_LastHeadRot = m_zCameraRig.transform.rotation;
            }

            for (int i = 0; i < 3; i++) {
                if (m_zStylus.GetButtonDown(i)) {
                    eventQueue.Add(new VREvent(m_ButtonEventBaseNames[i] + " DOWN"));
                }
                if (m_zStylus.GetButtonUp(i)) {
                    eventQueue.Add(new VREvent(m_ButtonEventBaseNames[i] + " UP"));
                }
            }
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventsProduced = new List<IVREventPrototype>();
            eventsProduced.Add(VREventPrototypeVector3.Create(m_HeadEventBaseName + "/Position"));
            eventsProduced.Add(VREventPrototypeQuaternion.Create(m_HeadEventBaseName + "/Rotation"));
            eventsProduced.Add(VREventPrototypeVector3.Create(m_StylusEventBaseName + "/Position"));
            eventsProduced.Add(VREventPrototypeQuaternion.Create(m_StylusEventBaseName + "/Rotation"));
            for (int i = 0; i < m_ButtonEventBaseNames.Length; i++) {
                eventsProduced.Add(VREventPrototype.Create(m_ButtonEventBaseNames[i] + " DOWN"));
                eventsProduced.Add(VREventPrototype.Create(m_ButtonEventBaseNames[i] + " UP"));
            }

            return eventsProduced;
        }

        [SerializeField] private string m_HeadEventBaseName;
        [SerializeField] private string m_StylusEventBaseName;
        [SerializeField] private string[] m_ButtonEventBaseNames;


        private ZCameraRig m_zCameraRig;
        private ZStylus m_zStylus;
        private Vector3 m_LastStylusPos;
        private Quaternion m_LastStylusRot;
        private Vector3 m_LastHeadPos;
        private Quaternion m_LastHeadRot;
    }

} // end namespace
