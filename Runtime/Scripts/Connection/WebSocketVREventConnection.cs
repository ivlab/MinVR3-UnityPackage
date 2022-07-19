#if WEBSOCKET_PRESENT

using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Threading.Tasks;

using IVLab.Utilities;

namespace IVLab.MinVR3
{
    /// <summary>
    /// WebSocket-based network connection for VREvents. Can be used with a Web Browser.
    /// </summary>
    public class WebSocketVREventConnection : MonoBehaviour, IVREventConnection
    {

        [Header("Networking Setup")]
        [SerializeField, Tooltip("WebSocket server host (usually IP address of this machine)")]
        private string host;

        [SerializeField, Tooltip("WebSocket server port")]
        private string port;

        // Alternate implementation using UnityEvents which doesn't work nicely w/threads
        // [Header("Behaviour when a VREvent is received")]
        // [SerializeField, Tooltip("Extra methods to call when a VREvent is received")]
        // private NetworkVREvent m_OnVREventReceived;

        private WebSocketServer wssv;

        private const string VREventPath = "/vrevent";

        #region VR Event Connection Send/Receive
        public IVREventConnection.VREventReceivedDelegate OnVREventReceived { get; set; }

        public void Send(in VREvent evt)
        {
            string serializedEvent = JsonUtility.ToJson(evt);
            wssv.WebSocketServices[VREventPath].Sessions.Broadcast(serializedEvent);
        }
        #endregion


        #region Unity MonoBehaviour Methods
        void Reset()
        {
            host = "127.0.0.1";
            port = "8000";
        }

        void Start()
        {
            wssv = new WebSocketServer("ws://" + host + ":" + port);
            wssv.AddWebSocketService<VREventWebSocketMessage>(VREventPath, s => s.owner = this);

            Task.Run(() => wssv.Start());
        }

        void OnDestroy()
        {
            wssv.Stop();
        }
        #endregion

        #region WebSocketSharp message handlers
        private class VREventWebSocketMessage : WebSocketBehavior
        {
            public WebSocketVREventConnection owner;

            protected override void OnOpen()
            {
                Debug.Log("New WebSocket VREvent connection");
            }
            protected override void OnClose(CloseEventArgs e)
            {
                Debug.Log("WebSocket VREvent connection terminated");
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                VREvent evt = VREvent.CreateFromJson(e.Data);
                // Send the event to all listeners (usually, this might just go in the VREvent queue)
                owner.OnVREventReceived.Invoke(evt);
            }
        }
        #endregion
    }
}

#endif
