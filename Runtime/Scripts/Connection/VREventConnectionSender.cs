using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Listens for one or more VREvents and forwards them on to whomever is listening at the
    /// other end of an IVREventConnection.  Depending on the type of connection, the receivers
    /// could include a web browser listening over a WebSocket connection, other MinVR3 apps
    /// listening over a Tcp connection, or somebody else over any type of connection that
    /// implements IVREventConnection. See also <seealso cref="VREventConnectionReceiver"/>.
    /// (formerly ConnectionVREventListener)
    /// </summary>
    [RequireComponent(typeof(IVREventConnection))]
    [AddComponentMenu("MinVR/Connection/VREventConnectionSender")]
    public class VREventConnectionSender : MonoBehaviour, IVREventListener
    {

        public bool useSendList {
            get { return m_UseSendList; }
            set { m_UseSendList = value; }
        }

        public List<VREventPrototypeAny> sendListPrototypes {
            get { return m_SendListPrototypes; }
            set { m_SendListPrototypes = value; }
        }

        public List<string> sendListStartsWithStrings {
            get { return m_SendListStartsWithStrings; }
            set { m_SendListStartsWithStrings = value; }
        }


        public bool useNoSendList {
            get { return m_UseNoSendList; }
            set { m_UseNoSendList = value; }
        }

        public List<VREventPrototypeAny> noSendListPrototypes {
            get { return m_NoSendListPrototypes; }
            set { m_NoSendListPrototypes = value; }
        }

        public List<string> noSendListStartsWithStrings {
            get { return m_NoSendListStartsWithStrings; }
            set { m_NoSendListStartsWithStrings = value; }
        }

        void Reset()
        {
            m_SendListPrototypes = null;
            m_NoSendListPrototypes = null;
            m_SendListStartsWithStrings = null;
            m_NoSendListStartsWithStrings = null;
        }

        void Start()
        {
            // find the first attached component that implements IVREventConnection AND is enabled
            IVREventConnection[] connections = this.GetComponents<IVREventConnection>();
            m_Connection = connections[0];
            int i = 0;
            while ((i < connections.Length) && (!((MonoBehaviour)m_Connection).isActiveAndEnabled)) {
                i++;
                m_Connection = connections[i];
            }
        }


        public bool InSendList(VREvent evt)
        {
            // check the list of complete event prototypes for a match
            if ((m_SendListPrototypes != null) && (m_SendListPrototypes.Count > 0)) {
                foreach (var p in m_SendListPrototypes) {
                    if (evt.Matches(p)) {
                        return true;
                    }
                }
            }
            // also check the list of starts-with strings for a match
            if ((m_SendListStartsWithStrings != null) && (m_SendListStartsWithStrings.Count > 0)) {
                foreach (string s in m_SendListStartsWithStrings) {
                    if (evt.GetName().StartsWith(s)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool InNoSendList(VREvent evt)
        {
            // check the list of complete event prototypes for a match
            if ((m_NoSendListPrototypes != null) && (m_NoSendListPrototypes.Count > 0)) {
                foreach (var p in m_NoSendListPrototypes) {
                    if (evt.Matches(p)) {
                        return true;
                    }
                }
            }
            // also check the list of starts-with strings for a match
            if ((m_NoSendListStartsWithStrings != null) && (m_NoSendListStartsWithStrings.Count > 0)) {
                foreach (string s in m_NoSendListStartsWithStrings) {
                    if (evt.GetName().StartsWith(s)) {
                        return true;
                    }
                }
            }

            return false;
        }


        public void OnVREvent(VREvent evt)
        {
            if (m_Connection != null) {

                // case 0: not using either list, send all events
                if ((!m_UseSendList) && (!m_UseNoSendList)) {
                    m_Connection.Send(evt);
                }

                // case 1: using the send list
                else if (m_UseSendList) {
                    if (InSendList(evt)) {
                        // case 1a: also using nosend list, only send if evt is not in the nosend list
                        if (m_UseNoSendList) {
                            if (!InNoSendList(evt)) {
                                m_Connection.Send(evt);
                            }
                        }
                        // case 1b: not using nosend, ok to send
                        else {
                            m_Connection.Send(evt);
                        }
                    }
                }

                // case 2: using the nosend list (and if we reach here we know we are not using the send list)
                else if (m_UseNoSendList) {
                    if (!InNoSendList(evt)) {
                        m_Connection.Send(evt);
                    }
                }

            }
        }

        void OnEnable()
        {
            StartListening();
        }

        void OnDisable()
        {
            StopListening();
        }

        public void StartListening() {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        public void StopListening() {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }

        [SerializeField] private bool m_UseSendList;
        [SerializeField] private List<VREventPrototypeAny> m_SendListPrototypes;
        [SerializeField] private List<string> m_SendListStartsWithStrings;

        [SerializeField] private bool m_UseNoSendList;
        [SerializeField] private List<VREventPrototypeAny> m_NoSendListPrototypes;
        [SerializeField] private List<string> m_NoSendListStartsWithStrings;

        private IVREventConnection m_Connection;
    }
}