using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;


namespace IVLab.MinVR3 {

    [AddComponentMenu("MinVR/Cluster/ClusterClient")]
    [DisallowMultipleComponent]
    public class ClusterClient : MonoBehaviour, IClusterNode {

        [Tooltip("The ip address of the server to connect to.")]
        public string serverIPAddress = "127.0.0.1";

        [Tooltip("The port the server is running on.")]
        public int serverPort = 3490;

        TcpClient client;

	    public void Initialize() {
            client = NetUtils.ConnectToTcpServer(serverIPAddress, serverPort);
	    }
	  
        public void Shutdown() {
            NetUtils.CloseTcpClient(client, true);
        }


        public void SynchronizeInputEventsAcrossAllNodes(ref List<VREvent> inputEvents) {
            // 1. send inputEvents to server
            NetUtils.SendEventData(ref client, in inputEvents, true);

            // 2. receive and parse serverInputEvents
            List<VREvent> serverInputEvents = new List<VREvent>();
            NetUtils.ReceiveEventData(ref client, ref serverInputEvents, true);

            //Debug.Log($"Received {serverInputEvents.Count} events:");
            foreach (var e in serverInputEvents) {
                Debug.Log(e.ToString());
            }

            // 3. inputEvents = serverInputEvents
            inputEvents = serverInputEvents;
	    }
	
	    public void SynchronizeSwapBuffersAcrossAllNodes() {
            // 1. send a swap_buffers_request message to the server
            NetUtils.SendSwapBuffersRequest(ref client, true);

            // 2. wait for and receive a swap_buffers_now message from the server
            NetUtils.ReceiveSwapBuffersNow(ref client, true);
	    }
    }

} // namespace MinVR
