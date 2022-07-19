#if WEBSOCKET_PRESENT

using UnityEngine;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Threading.Tasks;

namespace IVLab.MinVR3
{
    /// <summary>
    /// WebSocket-based network connection for VREvents. Can be used with a Web
    /// Browser. Bundled with a web server that serves a static directory of
    /// your choice via HTTP.
    /// </summary>
    public class HttpWebSocketVREventConnection : MonoBehaviour, IVREventConnection
    {

        private enum WebServerRoot
        {
            AssetsFolder,
            StreamingAssetsFolder,
            PersistentDataFolder,
        }

        [Header("Networking Setup")]
        [SerializeField, Tooltip("WebSocket server host (usually IP address of this machine)")]
        private string host;
        public string Host { get => host; }

        [SerializeField, Tooltip("WebSocket server port")]
        private string port;
        public string Port { get => port; }

        [SerializeField, Tooltip("Root of the web server")]
        private WebServerRoot webServerLocation;

        [SerializeField, Tooltip("Folder inside the root of the web server from which to serve content")]
        private string webServerFolder;

        // Alternate implementation using UnityEvents which doesn't work nicely w/threads
        // [Header("Behaviour when a VREvent is received")]
        // [SerializeField, Tooltip("Extra methods to call when a VREvent is received")]
        // private NetworkVREvent m_OnVREventReceived;

        private HttpServer wssv;

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
            wssv = new HttpServer("http://" + host + ":" + port);
            wssv.AddWebSocketService<VREventWebSocketMessage>(VREventPath, s => s.owner = this);

            // Set up HTTP server
            switch (webServerLocation)
            {
                case WebServerRoot.AssetsFolder:
                    wssv.DocumentRootPath = Application.dataPath;
                    break;
                case WebServerRoot.StreamingAssetsFolder:
                    wssv.DocumentRootPath = Application.streamingAssetsPath;
                    break;
                case WebServerRoot.PersistentDataFolder:
                    wssv.DocumentRootPath = Application.persistentDataPath;
                    break;
                default:
                    break;
            }
            wssv.DocumentRootPath = System.IO.Path.Combine(wssv.DocumentRootPath, webServerFolder);
            Debug.Log("Serving HTTP from " + wssv.DocumentRootPath);

            var typeDict = new Dictionary<string, string>()
            {
                { "html", "text/html" },
                { "txt", "text/plain" },
                { "js", "text/javascript" },
                { "png", "image/png" },
                { "jpg", "image/jpeg" },
            };

            wssv.OnGet += (sender, e) => {
                // Debug.Log("GET " + e.Request.RawUrl);
                string path = e.Request.RawUrl;
                if (path == "/")
                    path += "index.html";

                int lastDot = path.LastIndexOf('.');
                string ext = path.Substring(lastDot + 1);

                if (!typeDict.ContainsKey(ext))
                {
                    e.Response.StatusCode = (int) System.Net.HttpStatusCode.BadRequest;
                    return;
                }

                byte[] contents;
                if (!e.TryReadFile(path, out contents))
                {
                    e.Response.StatusCode = (int) System.Net.HttpStatusCode.NotFound;
                    return;
                }

                e.Response.ContentEncoding = System.Text.Encoding.UTF8;
                e.Response.ContentType = typeDict[ext];
                e.Response.ContentLength64 = contents.LongLength;
                e.Response.WriteContent(contents);
                e.Response.Close(contents, true);
            };

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
            public HttpWebSocketVREventConnection owner;

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
