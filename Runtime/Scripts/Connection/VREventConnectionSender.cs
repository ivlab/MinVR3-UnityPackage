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
        public List<VREventPrototypeAny> sendList {
            get { return m_SendList; }
            set { m_SendList = value; }
        }

        public List<VREventPrototypeAny> noSendList {
            get { return m_NoSendList; }
            set { m_NoSendList = value; }
        }


        void Reset()
        {
            m_SendList = null;
            m_NoSendList = null;
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


        public bool UsingSendList()
        {
            return (m_SendList != null) && (m_SendList.Count > 0);
        }

        public bool InSendList(VREvent evt)
        {
            if ((m_SendList != null) || (m_SendList.Count > 0)) {
                foreach (var p in m_SendList) {
                    if (evt.Matches(p)) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool UsingNoSendList()
        {
            return (m_NoSendList != null) && (m_NoSendList.Count > 0);
        }

        public bool InNoSendList(VREvent evt)
        {
            if ((m_NoSendList != null) || (m_NoSendList.Count > 0)) {
                foreach (var p in m_NoSendList) {
                    if (evt.Matches(p)) {
                        return true;
                    }
                }
            }
            return false;
        }


        public void OnVREvent(VREvent evt)
        {
            if (m_Connection != null) {
                if ((UsingNoSendList()) && (InNoSendList(evt))) {
                    return;
                } else if ((UsingSendList()) && (!InSendList(evt))) {
                    return;
                } else {
                    m_Connection.Send(evt);
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


        [SerializeField] private List<VREventPrototypeAny> m_SendList;
        [SerializeField] private List<VREventPrototypeAny> m_NoSendList;

        private IVREventConnection m_Connection;
    }
}