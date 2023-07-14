using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace IVLab.MinVR3 {

    public static class NetUtils {

        // PART 0: INIT / SHUTDOWN

        /// <summary>
        /// Try once to connect, catch and print any errors, and return <see langword="true"/>/<see langword="false"/> for success.
        /// </summary>
        public static bool TryConnectToTcpServer(string serverIP, int serverPort, out TcpClient client)
        {
            client = null;
            try {
                client = new TcpClient(AddressFamily.InterNetwork);
                client.NoDelay = true;
                client.Connect(IPAddress.Parse(serverIP), serverPort);
            } catch (Exception e) {
                Debug.Log(String.Format("Exception: {0}", e));
                Console.WriteLine("Exception: {0}", e);
            }
            return client.Connected;
        }


        /// <summary>
        /// Block and keep trying to connect until the connection succeeds; quit after 1 minute of unsuccessful attempts.
        /// </summary>
        public static TcpClient ConnectToTcpServer(string serverIP, int serverPort)
        {
            TcpClient client = null;
            // continue trying to connect until we have success
            bool success = false;
            int retries = 0;
            while (!success) {
                try {
                    client = new TcpClient(AddressFamily.InterNetwork);
                    client.NoDelay = true;
                    client.Connect(IPAddress.Parse(serverIP), serverPort);
                    //stream = client.GetStream();
                    success = client.Connected;
                } catch (Exception e) {
                    Debug.Log(String.Format("Exception: {0}", e));
                    Console.WriteLine("Exception: {0}", e);
                }
                if (!success) {
                    Debug.Log($"NetUtils.ConnectToTcpServer(): Trouble connecting to {serverIP}:{serverPort}.  Trying again ({retries})...");
                    Console.WriteLine($"NetUtils.ConnectToTcpServer(): Trouble connecting to {serverIP}:{serverPort}.  Trying again ({retries})...");
                    Thread.Sleep(500);
                    retries++;
                }

                if (retries >= 120) {
                    Debug.Log("NetUtils.ConnectToTcpServer(): Giving up after trying for 1 minute.");
                    Console.WriteLine("NetUtils.ConnectToTcpServer(): Giving up after trying for 1 minute.");
                    #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    #else
                    Application.Quit();
                    #endif
                    return null;
                }
            }
            return client;
        }


        public static void CloseTcpClient(TcpClient client, bool quitOnError)
        {
            try {
                client.GetStream()?.Close();
                client.Close();
            } catch (Exception e) {
                Debug.Log(String.Format("Exception: {0}", e));
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError(quitOnError, "NetUtils.CloseTcpClient(): " + e);
            }
        }



        // PART 1:  SENDING/RECEIVING SMALL CONTROL MESSAGES AND 1-BYTE MESSAGE HEADERS

        // unique identifiers for different network messages
        public static readonly byte[] INPUT_EVENTS_MSG = { 1 };
        public static readonly byte[] SWAP_BUFFERS_REQUEST_MSG = { 2 };
        public static readonly byte[] SWAP_BUFFERS_NOW_MSG = { 3 };


        public static void SendOneByteMessage(ref TcpClient client, byte[] message, bool quitOnError) {
            if (!client.Connected) {
                BrokenConnectionError(quitOnError, "NetUtils.SendOneByteMessage(): Client no longer connected");
                return;
            }
            // this message consists only of a 1-byte header
            try {
                client.GetStream().Write(message, 0, 1);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError(quitOnError, "NetUtils.SendOneByteMessage(): " + e);
            }
        }


        public static void SendSwapBuffersRequest(ref TcpClient client, bool quitOnError) {
            SendOneByteMessage(ref client, NetUtils.SWAP_BUFFERS_REQUEST_MSG, quitOnError);
        }

        public static void SendSwapBuffersNow(ref TcpClient client, bool quitOnError) {
            SendOneByteMessage(ref client, NetUtils.SWAP_BUFFERS_NOW_MSG, quitOnError);
        }


        // Blocks until the specific message specified is received
        public static void ReceiveOneByteMessage(ref TcpClient client, byte[] message, bool quitOnError) {
            byte[] received = new byte[1];
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            while (received[0] != message[0]) {
                int status = -1;
                if (!client.Connected) {
                    BrokenConnectionError(quitOnError, "NetUtils.ReceiveOneByteMessage(): Client no longer connected");
                    return;
                }
                try {
                    status = client.GetStream().Read(received, 0, 1);
                }
                catch (Exception e) {
                    Console.WriteLine("Exception: {0}", e);
                    BrokenConnectionError(quitOnError, "NetUtils.ReceiveOneByteMessage(): " + e);
                    return;
                }
                if (status == -1) {
                    Console.WriteLine("NetUtils.ReceiveOneByteMessage(): WaitForAndReceiveMessageHeader failed");
                    return;
                }
                else if ((status == 1) && (received[0] != message[0])) {
                    Console.WriteLine("NetUtils.ReceiveOneByteMessage(): WaitForAndReceiveMessageHeader error: expected {0} got {1}", message[0], received[0]);
                    return;
                }
                if (stopwatch.Elapsed.TotalSeconds > 30) {
                    BrokenConnectionError(quitOnError, "NetUtils.ReceiveOneByteMessage(): Connection timed out");
                    return;
                }
            }
        }

        public static void ReceiveSwapBuffersRequest(ref TcpClient client, bool quitOnError) {
            ReceiveOneByteMessage(ref client, NetUtils.SWAP_BUFFERS_REQUEST_MSG, quitOnError);
        }

        public static void ReceiveSwapBuffersNow(ref TcpClient client, bool quitOnError) {
            ReceiveOneByteMessage(ref client, NetUtils.SWAP_BUFFERS_NOW_MSG, quitOnError);
        }





        // PART 2a:  LARGER MESSAGES FOR SYNCING INPUT EVENTS (USING BINARY FORMATTER FOR CLUSTER MODE)

        public static void SendEventData(ref TcpClient client, in List<VREvent> inputEvents, bool quitOnError) {
            // Debug.Log("SendInputEvents");

            // 1. send 1-byte message header
            if (!client.Connected) {
                BrokenConnectionError(quitOnError, "NetUtils.SendEventData(): Client no longer connected");
                return;
            }
            try {
                client.GetStream().Write(NetUtils.INPUT_EVENTS_MSG, 0, 1);
            } catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError(quitOnError, "NetUtils.SendEventData(): " + e);
            }


            // 2. send event data
            if (!client.Connected) {
                BrokenConnectionError(quitOnError, "NetUtils.SendEventData(): Client no longer connected");
                return;
            }
            try {
                BinaryFormatter bf = binaryFormatter;

                using (MemoryStream ms = new MemoryStream()) {
                    bf.Serialize(ms, inputEvents);
                    byte[] bytes = ms.ToArray();
                    WriteInt32(ref client, bytes.Length, quitOnError);
                    client.GetStream().Write(bytes, 0, bytes.Length);
                }
            } catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError(quitOnError, "NetUtils.SendEventData(): " + e);
            }
        }


        public static void ReceiveEventData(ref TcpClient client, ref List<VREvent> inputEvents, bool quitOnError) {
            // Debug.Log("WaitForAndReceiveInputEvents");

            // 1. receive 1-byte message header
            ReceiveOneByteMessage(ref client, NetUtils.INPUT_EVENTS_MSG, quitOnError);

            // 2. receive event data
            try {
                int dataSize = ReadInt32(ref client, quitOnError);
                byte[] bytes = new byte[dataSize];
                int status = ReceiveAll(ref client, ref bytes, dataSize, quitOnError);
                if (status == -1) {
                    Console.WriteLine("NetUtils.ReceiveEventData(): error reading data");
                    return;
                }
                
                BinaryFormatter bf = binaryFormatter;
                using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length)) {
                    List<VREvent> events = (List<VREvent>)bf.Deserialize(ms);
                    inputEvents.AddRange(events);
                }
            }
            catch (Exception e) {
                Debug.Log("Exception: " + e);
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError(quitOnError, "NetUtils.ReceiveEventData(): " + e);
            }
        }


        // PART 2b:  LARGER MESSAGES FOR SYNCING INPUT EVENTS (USING JSON SERIALIZATION FOR CONNECTION MODE)
        public static void SendEventAsJson(ref TcpClient client, in VREvent evt, bool quitOnError)
        {
            WriteString(ref client, JsonUtility.ToJson(evt), quitOnError);
        }


        public static VREvent ReceiveEventAsJson(ref TcpClient client, bool quitOnError)
        {
            return VREvent.CreateFromJson(ReadString(ref client, quitOnError));
        }



        // PART 3:  LOWER-LEVEL NETWORK ROUTINES

        // Blocks and continues reading until len bytes are read into buf
        public static int ReceiveAll(ref TcpClient client, ref byte[] buf, int len, bool quitOnError) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            int total = 0;        // how many bytes we've received
            int bytesleft = len; // how many we have left to receive
            int n;
            stopwatch.Start();
            while (total < len) {
                if (!client.Connected) {
                    BrokenConnectionError(quitOnError, "NetUtils.ReceiveAll(): client no longer connected");
                    return -1;
                }
                try {
                    n = client.GetStream().Read(buf, total, bytesleft);
                    total += n;
                    bytesleft -= n;
                }
                catch (Exception e) {
                    Console.WriteLine("Exception: {0}", e);
                    BrokenConnectionError(quitOnError, "NetUtils.ReceiveAll(): " + e);
                    return -1;
                }
                if (stopwatch.Elapsed.TotalSeconds > 5) {
                    BrokenConnectionError(quitOnError, "NetUtils.ReceiveAll(): timeout");
                    return -1;                        
                }
            }
            return total; // return -1 on failure, total on success
        }


        public static void WriteUInt32(ref TcpClient client, UInt32 i, bool quitOnError)
        {
            if (!BitConverter.IsLittleEndian) {
                i = SwapEndianness(i);
            }
            byte[] buf = BitConverter.GetBytes(i);
            if (!client.Connected) {
                BrokenConnectionError(quitOnError, "NetUtils.WriteUInt32(): client no longer connected");
                return;
            }
            try {
                client.GetStream().Write(buf, 0, 4);
            } catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError(quitOnError, "NetUtils.WriteUInt32(): " + e);
            }
        }

        public static UInt32 ReadUInt32(ref TcpClient client, bool quitOnError)
        {
            byte[] buf = new byte[4];
            int status = ReceiveAll(ref client, ref buf, 4, quitOnError);
            if (status == -1) {
                Console.WriteLine("NetUtils.ReadUInt32(): error reading data");
                return 0;
            }
            UInt32 i = BitConverter.ToUInt32(buf, 0);
            if (!BitConverter.IsLittleEndian) {
                i = SwapEndianness(i);
            }
            return i;
        }

        public static void WriteInt32(ref TcpClient client, Int32 i, bool quitOnError) {
            if (!BitConverter.IsLittleEndian) {
                i = SwapEndianness(i);
            }
            byte[] buf = BitConverter.GetBytes(i);
            if (!client.Connected) {
                BrokenConnectionError(quitOnError, "NetUtils.WriteInt32(): Client no longer connected");
                return;
            }
            try {
                client.GetStream().Write(buf, 0, 4);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError(quitOnError, "NetUtils.WriteInt32(): " + e);
            }
        }

        public static Int32 ReadInt32(ref TcpClient client, bool quitOnError) {
            byte[] buf = new byte[4];
            int status = ReceiveAll(ref client, ref buf, 4, quitOnError);
            if (status == -1) {
                Console.WriteLine("NetUtils.ReadInt32(): error reading data");
                return 0;
            }
            Int32 i = BitConverter.ToInt32(buf, 0);
            if (!BitConverter.IsLittleEndian) {
                i = SwapEndianness(i);
            }
            return i;
        }

        public static void WriteString(ref TcpClient client, String s, bool quitOnError)
        {
            if (!client.Connected) {
                BrokenConnectionError(quitOnError, "NetUtils.WriteString(): Client no longer connected");
                return;
            }
            try {
                // 1. send an unsigned int with the size of the string
                WriteUInt32(ref client, (UInt32)s.Length, quitOnError);
                // 3. send the raw bytes string
                byte[] bytes = Encoding.ASCII.GetBytes(s);
                client.GetStream().Write(bytes, 0, bytes.Length);
            } catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError(quitOnError, "NetUtils.WriteString(): " + e);
            }
        }

        public static String ReadString(ref TcpClient client, bool quitOnError)
        {
            // 1. read an unsigned int with the size of the string
            int size = (int)ReadUInt32(ref client, quitOnError);
            // 2. read raw bytes of the string
            byte[] buf = new byte[size];
            int status = ReceiveAll(ref client, ref buf, size, quitOnError);
            if (status == -1) {
                Console.WriteLine("NetUtils.ReadString(): error reading data");
                return "";
            }
            return Encoding.ASCII.GetString(buf);
        }

        public static Int32 SwapEndianness(Int32 value) {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = (value >> 24) & 0xff;
            return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
        }

        public static UInt32 SwapEndianness(UInt32 value)
        {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = (value >> 24) & 0xff;
            return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
        }

        public static void BrokenConnectionError(bool quit, string info="") {
            Debug.LogError("Network connection broken: " + info);
            if (quit) {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit(1);
#endif
            }
        }


        private static BinaryFormatter binaryFormatter {
            get {
                if (s_BinaryFormatter == null) {
                    BinaryFormatter bf = new BinaryFormatter();

                    SurrogateSelector ss = new SurrogateSelector();
                    ss.AddSurrogate(typeof(Vector2), new StreamingContext(StreamingContextStates.All),
                                    new Vector2SerializationSurrogate());
                    ss.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All),
                                    new Vector3SerializationSurrogate());
                    ss.AddSurrogate(typeof(Vector4), new StreamingContext(StreamingContextStates.All),
                                    new Vector4SerializationSurrogate());
                    ss.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All),
                                    new QuaternionSerializationSurrogate());
                    bf.SurrogateSelector = ss;
                    s_BinaryFormatter = bf;
                }
                return s_BinaryFormatter;
            }
        }


        private static BinaryFormatter s_BinaryFormatter;
    }

} // namespace
