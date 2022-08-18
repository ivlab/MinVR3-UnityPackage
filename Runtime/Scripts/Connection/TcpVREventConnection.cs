using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;


namespace IVLab.MinVR3
{

    public class TcpVREventConnection : MonoBehaviour, IVREventConnection
    {
        public bool CanSend()
        {
            return true;
        }

        public void Send(in VREvent evt)
        {
            List<VREvent> events = new List<VREvent>();
            events.Add(evt);

            if ((m_ServerConnection != null) && (m_ServerConnection.Connected)) {
                NetUtils.SendEventData(ref m_ServerConnection, in events, false);
            }
            for (int i = 0; i < m_AcceptedConnections.Count; i++) {
                TcpClient client = m_AcceptedConnections[i];
                if (client.Connected) {
                    NetUtils.SendEventData(ref client, in events, false);
                }
            }
        }


        public bool CanReceive()
        {
            return true;
        }

        public IVREventConnection.OnVREventReceivedDelegate OnVREventReceived { get; set; }



        // Start is called before the first frame update
        void Start()
        {
            m_AcceptedConnections = new List<TcpClient>();
            m_Listener = null;
            m_ServerConnection = null;

            if (m_ListenForConnections) {
                m_Listener = new TcpListener(IPAddress.Any, m_ListenForConnectionsPort);
                m_Listener.Start();
            }

            if ((m_ConnectToServer) && (m_ServerConnection == null)) {
                NetUtils.TryConnectToTcpServer(m_ConnectToServerIP, m_ConnectToServerPort, out m_ServerConnection);
            }
        }

        // Update is called once per frame
        void Update()
        {
            // note: reestablishing a connection doesn't really work too well for clients that are only receiving
            // data because TcpClient.Connected just reports the status of the last operation.  If the server dies
            // and stops sending data, then the client just thinks there is no data coming.  the best way to handle
            // this is probably to insert some timeout logic, like if there is no activity for n seconds, then
            // assume the connection is broken, close it, and try to reopen a new one.

            // if the server connection needs to be (re)established, try to (re)connect
            if ((m_ConnectToServer) && ((m_ServerConnection == null) || (!m_ServerConnection.Connected))) {
                Debug.Log("Trying to connect to server");
                NetUtils.TryConnectToTcpServer(m_ConnectToServerIP, m_ConnectToServerPort, out m_ServerConnection);
            }

            // if any previously accepted are no longer connected, remove them from our list
            for (int i = 0; i < m_AcceptedConnections.Count; i++) {
                if (!m_AcceptedConnections[i].Connected) {
                    m_AcceptedConnections.RemoveAt(i);
                }
            }

            // if any are requesting to connect, then accept the request
            if ((m_Listener != null) && (m_Listener.Pending())) {
                try {
                    TcpClient client = m_Listener.AcceptTcpClient();
                    if (client.Connected) {
                        client.NoDelay = true;
                        m_AcceptedConnections.Add(client);
                        Debug.Log("Accepted connection #" + m_AcceptedConnections.Count);
                        System.Console.WriteLine("Accepted connection #" + m_AcceptedConnections.Count);
                    }
                } catch (Exception e) {
                    Debug.Log(String.Format("Exception: {0}", e));
                    System.Console.WriteLine("Exception: {0}", e);
                }
            }



            List<VREvent> events = new List<VREvent>();

            // receive any messages coming our way from the server
            if ((m_ServerConnection != null) && (m_ServerConnection.Connected)) {
                while (m_ServerConnection.GetStream().DataAvailable) {
                    NetUtils.ReceiveEventData(ref m_ServerConnection, ref events, false);
                }
            }

            // receive any messages coming our way from accepted connections
            for (int i = 0; i < m_AcceptedConnections.Count; i++) {
                TcpClient client = m_AcceptedConnections[i];
                while (client.GetStream().DataAvailable) {
                    NetUtils.ReceiveEventData(ref client, ref events, false);
                }
            }

            // invoke the callback from the interface for each new event received
            foreach (var evt in events) {
                OnVREventReceived.Invoke(evt);
            }
        }


        [Header("Request Connection")]
        [Tooltip("Set to true to request a Tcp VREventConnection with a server listening on a specific IP and port.")]
        [SerializeField] private bool m_ConnectToServer;
        [Tooltip("Connect to a server at this IP.")]
        [SerializeField] private string m_ConnectToServerIP;
        [Tooltip("The port the server is running on.")]
        [SerializeField] private int m_ConnectToServerPort;


        [Header("Connection Requests")]
        [Tooltip("Set to true if this connection should act as a server, accepting Tcp connection " +
            "requests from others.")]
        [SerializeField] private bool m_ListenForConnections;
        [Tooltip("Listen for connection requests on this port.")]
        [SerializeField] private int m_ListenForConnectionsPort;


        private TcpListener m_Listener;
        private List<TcpClient> m_AcceptedConnections;
        private TcpClient m_ServerConnection;

    }

}
