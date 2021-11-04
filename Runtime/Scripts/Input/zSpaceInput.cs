using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zSpace.Core;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Grabs input from the zSpace using the zCore library and converts that input to VREvents
    /// </summary>
    [AddComponentMenu("MinVR/Input/zSpace Input")]
    [DefaultExecutionOrder(-998)] // make sure this script runs right before VREngine.cs
    public class zSpaceInput : MonoBehaviour, IVREventProducer
    {
        void Reset()
        {
            m_HeadEventBaseName = "Head";
            m_StylusEventBaseName = "Stylus";
            m_ButtonEventBaseNames = new string[3];
            m_ButtonEventBaseNames[0] = "Stylus/Primary Button";
            m_ButtonEventBaseNames[1] = "Stylus/Button 2";
            m_ButtonEventBaseNames[2] = "Stylus/Button 3";
            m_TapEventBaseName = "Stylus/Tap";
        }

        void Start()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // zSpace only runs on Windows.  The scripts compile just fine on other platforms
            // no zCore plugin library is provided, so an assertion is fired if the zCore
            // component is included in the scene.  This class provides a wrapper for zSpace
            // and only adds the zCore component if running on Windows.  The remainder of the
            // script should always check to make sure m_zCore != null before accessing it.

            m_zCore = FindObjectOfType<ZCore>();
            if (m_zCore == null) {
                m_zCore = gameObject.AddComponent<ZCore>();
            }            
#endif

            if (m_zCore == null) {
                Debug.LogError("Unable to find reference to zSpace.Core.Core Monobehaviour.");
                this.enabled = false;
                return;
            } else {
                // Register event handlers.
                m_zCore.TargetMove += HandleMove;
                m_zCore.TargetButtonPress += HandleButtonPress;
                m_zCore.TargetButtonRelease += HandleButtonRelease;
                m_zCore.TargetTapPress += HandleTapPress;
                m_zCore.TargetTapRelease += HandleTapRelease;
            }
        }

        void OnDestroy()
        {
            if (m_zCore != null) {
                // Unregister event handlers.
                m_zCore.TargetMove -= HandleMove;
                m_zCore.TargetButtonPress -= HandleButtonPress;
                m_zCore.TargetButtonRelease -= HandleButtonRelease;
                m_zCore.TargetTapPress -= HandleTapPress;
                m_zCore.TargetTapRelease -= HandleTapRelease;
            }
        }


        private void HandleMove(ZCore sender, ZCore.TrackerEventInfo info)
        {
            if (info.TargetType == ZCore.TargetType.Primary) {
                VREngine.instance.eventManager.QueueEvent(m_StylusEventBaseName + "/Position", info.WorldPose.Position);
                VREngine.instance.eventManager.QueueEvent(m_StylusEventBaseName + "/Rotation", info.WorldPose.Rotation);
            } else if (info.TargetType == ZCore.TargetType.Head) {
                VREngine.instance.eventManager.QueueEvent(m_HeadEventBaseName + "/Position", info.WorldPose.Position);
                VREngine.instance.eventManager.QueueEvent(m_HeadEventBaseName + "/Rotation", info.WorldPose.Rotation);
            }
        }

        private void HandleButtonPress(ZCore sender, ZCore.TrackerButtonEventInfo info)
        {
            if (info.TargetType == ZCore.TargetType.Primary) {
                VREngine.instance.eventManager.QueueEvent(m_ButtonEventBaseNames[info.ButtonId] + " DOWN");
            }
        }

        private void HandleButtonRelease(ZCore sender, ZCore.TrackerButtonEventInfo info)
        {
            if (info.TargetType == ZCore.TargetType.Primary) {
                VREngine.instance.eventManager.QueueEvent(m_ButtonEventBaseNames[info.ButtonId] + " UP");
            }
        }

        private void HandleTapPress(ZCore sender, ZCore.TrackerEventInfo info)
        {
            if (info.TargetType == ZCore.TargetType.Primary) {
                VREngine.instance.eventManager.QueueEvent(m_TapEventBaseName + " DOWN");
            }
        }

        private void HandleTapRelease(ZCore sender, ZCore.TrackerEventInfo info)
        {
            if (info.TargetType == ZCore.TargetType.Primary) {
                VREngine.instance.eventManager.QueueEvent(m_TapEventBaseName + " UP");
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
            eventsProduced.Add(VREventPrototype.Create(m_TapEventBaseName + " DOWN"));
            eventsProduced.Add(VREventPrototype.Create(m_TapEventBaseName + " UP"));

            return eventsProduced;
        }


        [SerializeField] private string m_HeadEventBaseName;
        [SerializeField] private string m_StylusEventBaseName;
        [SerializeField] private string[] m_ButtonEventBaseNames;
        [SerializeField] private string m_TapEventBaseName;

        private ZCore m_zCore = null;
    }

} // end namespace
