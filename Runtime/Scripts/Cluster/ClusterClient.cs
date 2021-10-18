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
	    NetworkStream stream;

	    public void Initialize() {
            // continue trying to connect until we have success
            bool success = false;
            while (!success) {
                try {
                    client = new TcpClient(AddressFamily.InterNetwork);
                    client.NoDelay = true;
                    client.Connect(IPAddress.Parse(serverIPAddress), serverPort);
                    stream = client.GetStream();
                    success = client.Connected;
                }
                catch (Exception e) {
                    Debug.Log(String.Format("Exception: {0}", e));
                    Console.WriteLine("Exception: {0}", e);
                }
                if (!success) {
                    Debug.Log("Having trouble connecting to the VRNetServer.  Trying again...");
                    Console.WriteLine("Having trouble connecting to the VRNetServer.  Trying again...");
                    Thread.Sleep(500);
                }
            }
	    }
	  
        public void Shutdown() {
            try {
			    stream.Close();         
			    client.Close();         
		    }
            catch (Exception e) {
                Debug.Log(String.Format("Exception: {0}", e));
                Console.WriteLine("Exception: {0}", e);
            }
        }


        public void SynchronizeInputEventsAcrossAllNodes(ref List<VREvent> inputEvents) {
            // 1. send inputEvents to server
            NetUtils.SendEventData(ref client, in inputEvents);

            // 2. receive and parse serverInputEvents
            List<VREvent> serverInputEvents = new List<VREvent>();
            NetUtils.ReceiveEventData(ref client, ref serverInputEvents);
		
		    // 3. inputEvents = serverInputEvents
		    inputEvents = serverInputEvents;
	    }
	
	    public void SynchronizeSwapBuffersAcrossAllNodes() {
            // 1. send a swap_buffers_request message to the server
            NetUtils.SendSwapBuffersRequest(ref client);

            // 2. wait for and receive a swap_buffers_now message from the server
            NetUtils.ReceiveSwapBuffersNow(ref client);
	    }
    }

} // namespace MinVR
