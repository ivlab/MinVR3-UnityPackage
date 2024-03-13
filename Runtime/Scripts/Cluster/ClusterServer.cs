using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net; 
using System.Net.Sockets;
using System.Threading;

namespace IVLab.MinVR3 {

    [AddComponentMenu("MinVR/Cluster/ClusterServer")]
    [DisallowMultipleComponent]
    public class ClusterServer : MonoBehaviour, IClusterNode
    {
        [Tooltip("The number of clients that should connect to the server.")]
        public int numClients;

        [Tooltip("The port the server should run on.")]
        public int serverPort;

        [Tooltip("Number of seconds to wait on startup for all clients to connect.")]
        public int secondsToWaitForClientsToConnect;


        TcpListener server;
        List<TcpClient> clients;
        System.Diagnostics.Stopwatch connectionStopwatch;

        public void Reset()
        {
            numClients = 1;
            serverPort = 3490;
            secondsToWaitForClientsToConnect = 30;
        }

        public void Initialize() {
            clients = new List<TcpClient>();
            string hostname = "localhost";
            IPHostEntry host = Dns.GetHostEntry(hostname);

            Console.WriteLine($"GetHostEntry({hostname}) returns:");

            foreach (IPAddress address in host.AddressList) {
                Console.WriteLine($"    {address}");
            }
            

            Debug.Log("Cluster Server: Starting TCP Listener");
            Console.WriteLine("Cluster Server: Starting TCP Listener");
            server = new TcpListener(IPAddress.Any, serverPort);
            server.Start();

            Debug.Log("Server waiting for " + numClients + " connection(s)...");
            Console.WriteLine("Server waiting for " + numClients + " connection(s)...");
            connectionStopwatch = new System.Diagnostics.Stopwatch();
            connectionStopwatch.Start();
            while (clients.Count < numClients) {
                if (server.Pending()) {
                    try {
                        // Blocking call to accept requests
                        TcpClient client = server.AcceptTcpClient();
                        if (client.Connected) {
                            client.NoDelay = true;
                            clients.Add(client);
                            Debug.Log("Accepted connection #" + clients.Count);
                            Console.WriteLine("Accepted connection #" + clients.Count);
                        }
                    } catch (Exception e) {
                        Debug.Log(String.Format("Exception: {0}", e));
                        Console.WriteLine("Exception: {0}", e);
                        #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
                        #else
                        Application.Quit();
                        #endif
                        return;
                    }
                } else {
                    if (connectionStopwatch.ElapsedMilliseconds > secondsToWaitForClientsToConnect * 1000) {
                        Debug.Log("Timed out waiting for client(s) to connect.");
                        Console.WriteLine("Timed out waiting for client(s) to connect.");
                        Shutdown();
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                        return;
                    }
                    Thread.Sleep(50);
                }
            }
        }


        public void Shutdown() {
            for (int i=0; i<clients.Count; i++) {
                clients[i].Close();
            }
        }

        public void SynchronizeInputEventsAcrossAllNodes(ref List<VREvent> inputEvents) {
            // 1. FOR EACH CLIENT, RECEIVE A LIST OF INPUT EVENTS GENERATED ON THE CLIENT
            // AND ADD THEM TO THE SERVER'S INPUTEVENTS LIST

            // the following section implements something similar to a socket select statement.
            // we need to receive data from all clients, but socket 4 may be ready to send data
            // before socket 1, so we loop through the sockets reading from the first we find
            // that is ready to send data, then continue looping until we have read from all.

            // initialize list to include all streams in the list to read from
            List<bool> receivedFromClients = Enumerable.Repeat(false, clients.Count).ToList();


            // loop until the list of streams to read from is empty
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // Loop through until we have data for all clients
            while (receivedFromClients.Any(d => !d))
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].GetStream().DataAvailable)
                    {
                        // if ready to read, read data and mark the client as successfully received
                        TcpClient c = clients[i];
                        NetUtils.ReceiveEventData(ref c, ref inputEvents, true);
                        receivedFromClients[i] = true;
                    }
                }

                if (stopwatch.Elapsed.TotalSeconds > 5)
                {
                    NetUtils.BrokenConnectionError(true, "Cluster server timed out synchronizing events");
                    return;
                }
            }

            // 2. SEND THE COMBINED INPUT EVENTS LIST OUT TO ALL CLIENTS
            for (int i = 0; i < clients.Count; i++) {
                TcpClient c = clients[i];
                //Debug.Log("Sending input events " + inputEvents.Count);
                NetUtils.SendEventData(ref c, in inputEvents, true);
            }
        }



        public void SynchronizeSwapBuffersAcrossAllNodes() {
            // 1. WAIT FOR A SWAP BUFFERS REQUEST MESSAGE FROM ALL CLIENTS

            // the following section implements something similar to a socket select statement.
            // we need to receive data from all clients, but socket 4 may be ready to send data
            // before socket 1, so we loop through the sockets reading from the first we find
            // that is ready to send data, then continue looping until we have read from all.

            // initialize list to include all streams in the list to read from
            List<bool> receivedFromClients = Enumerable.Repeat(false, clients.Count).ToList();

            // loop until the list of streams to read from is empty
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // Loop through until we have data for all clients
            while (receivedFromClients.Any(d => !d))
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].GetStream().DataAvailable)
                    {
                        // if ready to read, read data and remove from the list of streams to read from
                        TcpClient c = clients[i];
                        NetUtils.ReceiveSwapBuffersRequest(ref c, true);
                        receivedFromClients[i] = true;
                    }
                }

                if (stopwatch.Elapsed.TotalSeconds > 5)
                {
                    NetUtils.BrokenConnectionError(true, "Cluster server timed out syncing swap buffers");
                    return;
                }
            }

            // 2. SEND A SWAP BUFFERS NOW MESSAGE TO ALL CLIENTS
            for (int i = 0; i < clients.Count; i++) {
                TcpClient c = clients[i];
                NetUtils.SendSwapBuffersNow(ref c, true);
            }

        }

    }

} // namespace
