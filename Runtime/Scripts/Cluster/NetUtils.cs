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

namespace IVLab.MinVR3 {

    public static class NetUtils {

        // PART 1:  SENDING/RECEIVING SMALL CONTROL MESSAGES AND 1-BYTE MESSAGE HEADERS

        // unique identifiers for different network messages
        public static readonly byte[] INPUT_EVENTS_MSG = { 1 };
        public static readonly byte[] SWAP_BUFFERS_REQUEST_MSG = { 2 };
        public static readonly byte[] SWAP_BUFFERS_NOW_MSG = { 3 };


        public static void SendOneByteMessage(ref TcpClient client, byte[] message) {
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            // this message consists only of a 1-byte header
            try {
                client.GetStream().Write(message, 0, 1);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
        }


        public static void SendSwapBuffersRequest(ref TcpClient client) {
            SendOneByteMessage(ref client, NetUtils.SWAP_BUFFERS_REQUEST_MSG);
        }

        public static void SendSwapBuffersNow(ref TcpClient client) {
            SendOneByteMessage(ref client, NetUtils.SWAP_BUFFERS_NOW_MSG);
        }


        // Blocks until the specific message specified is received
        public static void ReceiveOneByteMessage(ref TcpClient client, byte[] message) {
            byte[] received = new byte[1];
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            while (received[0] != message[0]) {
                int status = -1;
                if (!client.Connected) {
                    BrokenConnectionError();
                    return;
                }
                try {
                    status = client.GetStream().Read(received, 0, 1);
                }
                catch (Exception e) {
                    Console.WriteLine("Exception: {0}", e);
                    BrokenConnectionError();
                    return;
                }
                if (status == -1) {
                    Console.WriteLine("WaitForAndReceiveMessageHeader failed");
                    return;
                }
                else if ((status == 1) && (received[0] != message[0])) {
                    Console.WriteLine("WaitForAndReceiveMessageHeader error: expected {0} got {1}", message[0], received[0]);
                    return;
                }
                if (stopwatch.Elapsed.TotalSeconds > 5) {
                    BrokenConnectionError();
                    return;
                }
            }
        }

        public static void ReceiveSwapBuffersRequest(ref TcpClient client) {
            ReceiveOneByteMessage(ref client, NetUtils.SWAP_BUFFERS_REQUEST_MSG);
        }

        public static void ReceiveSwapBuffersNow(ref TcpClient client) {
            ReceiveOneByteMessage(ref client, NetUtils.SWAP_BUFFERS_NOW_MSG);
        }





        // PART 2:  LARGER MESSAGES FOR SYNCING INPUT EVENTS

        public static void SendEventData(ref TcpClient client, in List<VREvent> inputEvents) {
            // Debug.Log("SendInputEvents");

            // 1. send 1-byte message header
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            try {
                client.GetStream().Write(NetUtils.INPUT_EVENTS_MSG, 0, 1);
            } catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }


            // 2. send event data
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            try {
                BinaryFormatter bf = binaryFormatter;

                using (MemoryStream ms = new MemoryStream()) {
                    bf.Serialize(ms, inputEvents);
                    byte[] bytes = ms.ToArray();
                    WriteInt32(ref client, bytes.Length);
                    client.GetStream().Write(bytes, 0, bytes.Length);
                }
            } catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
        }


        public static void ReceiveEventData(ref TcpClient client, ref List<VREvent> inputEvents) {
            // Debug.Log("WaitForAndReceiveInputEvents");

            // 1. receive 1-byte message header
            ReceiveOneByteMessage(ref client, NetUtils.INPUT_EVENTS_MSG);

            // 2. receive event data
            try {
                int dataSize = ReadInt32(ref client);
                byte[] bytes = new byte[dataSize];
                int status = ReceiveAll(ref client, ref bytes, dataSize);
                if (status == -1) {
                    Console.WriteLine("ReceiveEventData error reading data");
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
                BrokenConnectionError();
            }
        }




        // PART 3:  LOWER-LEVEL NETWORK ROUTINES

        // Blocks and continues reading until len bytes are read into buf
        public static int ReceiveAll(ref TcpClient client, ref byte[] buf, int len) {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            int total = 0;        // how many bytes we've received
            int bytesleft = len; // how many we have left to receive
            int n;
            stopwatch.Start();
            while (total < len) {
                if (!client.Connected) {
                    BrokenConnectionError();
                    return -1;
                }
                try {
                    n = client.GetStream().Read(buf, total, bytesleft);
                    total += n;
                    bytesleft -= n;
                }
                catch (Exception e) {
                    Console.WriteLine("Exception: {0}", e);
                    BrokenConnectionError();
                    return -1;
                }
                if (stopwatch.Elapsed.TotalSeconds > 5) {
                    BrokenConnectionError();
                    return -1;                        
                }
            }
            return total; // return -1 on failure, total on success
        }


        public static void WriteInt32(ref TcpClient client, Int32 i) {
            if (!BitConverter.IsLittleEndian) {
                i = SwapEndianness(i);
            }
            byte[] buf = BitConverter.GetBytes(i);
            if (!client.Connected) {
                BrokenConnectionError();
                return;
            }
            try {
                client.GetStream().Write(buf, 0, 4);
            }
            catch (Exception e) {
                Console.WriteLine("Exception: {0}", e);
                BrokenConnectionError();
            }
        }

        public static Int32 ReadInt32(ref TcpClient client) {
            byte[] buf = new byte[4];
            int status = ReceiveAll(ref client, ref buf, 4);
            if (status == -1) {
                Console.WriteLine("ReadInt32() error reading data");
                return 0;
            }
            Int32 i = BitConverter.ToInt32(buf, 0);
            if (!BitConverter.IsLittleEndian) {
                i = SwapEndianness(i);
            }
            return i;
        }

        public static Int32 SwapEndianness(Int32 value) {
            var b1 = (value >> 0) & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = (value >> 24) & 0xff;
            return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
        }


        public static void BrokenConnectionError() {
            Debug.Log("Network connection broken, shutting down.");
            Console.WriteLine("Network connection broken, shutting down.");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit(1);
            #endif
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
