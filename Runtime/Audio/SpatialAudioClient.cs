using UnityEngine;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Implements a spatial audio client that connects to the sound_server.py spatial audio server.
    ///
    /// The spatial audio server must be installed and running for this script
    /// to do anything. See https://github.umn.edu/ivlab-cs/sound_server for
    /// details on the sound server.
    /// </summary>
    [AddComponentMenu("MinVR/Audio/Spatial Audio Client")]
    [DefaultExecutionOrder(ScriptPriority)]
    public class SpatialAudioClient : MonoBehaviour
    {
        [SerializeField, Tooltip("Spatial audio server address to connect to (sound_server.py)")]
        public string serverAddress = "http://localhost:8000";

        [SerializeField, Tooltip("Reset the audio server on startup")]
        public bool resetServerOnStartup = true;


        // NOTE: This is a guess based on what sounded right in the CAVE...
        // a right-handed +y-up +z-forward coord system theoretically shouldn't exist though.
        [SerializeField, Tooltip("Coordinate system to output sound coordinates to")]
        public CoordConversion.CoordSystem outputCoordSystem = new CoordConversion.CoordSystem(
            CoordConversion.CoordSystem.Handedness.RightHanded,
            CoordConversion.CoordSystem.Axis.PosY,
            CoordConversion.CoordSystem.Axis.PosZ
        );

        public const int ScriptPriority = VREngine.ScriptPriority + 1;


        private HttpClient client;
        private bool clientInitialized = false;
        public bool Initialized { get => clientInitialized; }

        public Task WaitForInitialized() { return waitForInitialized; }
        private Task waitForInitialized;

        protected void Awake()
        {
            client = new HttpClient
            {
                BaseAddress = new System.Uri(serverAddress)
            };

            waitForInitialized = client.GetAsync("").ContinueWith(
                (prevTask) =>
                {
                    try
                    {
                        var response = prevTask.Result;

                        Debug.Log("Connected to spatial audio server " + serverAddress + $" (status {response.StatusCode})");
                        clientInitialized = true;

                        if (resetServerOnStartup)
                        {
                            ResetAudio();
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Unable to connect to spatial audio server " + serverAddress + $" ({e.InnerException.Message})");
                    }
                }
            );
        }

        /// <summary>
        /// Make a request to the server
        /// </summary>
        private bool MakeAudioRequest(string target, Dictionary<string, string> parameters)
        {
            if (!clientInitialized)
            {
                return false;
            }
            string[] parameterPairs = parameters.Select(kv => kv.Key + "=" + kv.Value).ToArray();
            string parameterString = string.Join("&", parameterPairs);
            Task t = Task.Run(() => client.GetAsync(target + "?" + parameterString));
            t.Wait();
            return true;
        }

        private bool MakeAudioRequest(string target, Dictionary<string, float> parameters)
        {
            if (!clientInitialized)
            {
                return false;
            }
            string[] parameterPairs = parameters.Select(kv => kv.Key + "=" + kv.Value).ToArray();
            string parameterString = string.Join("&", parameterPairs);
            Task t = Task.Run(() => client.GetAsync(target + "?" + parameterString));
            t.Wait();
            return true;
        }

        private void MakeAudioRequest(string target)
        {
            client.GetAsync(target);
        }

        public void ResetAudio()
        {
            MakeAudioRequest("reset");
        }

#region Listener Methods

        public void SetListenerPosition(Vector3 v)
        {
            v = CoordConversion.FromUnity(v, outputCoordSystem);
            MakeAudioRequest("listener_param", new Dictionary<string, float>
            {
                { "x", v.x },
                { "y", v.y },
                { "z", v.z },
            });
        }

        public void SetListenerVelocity(Vector3 v)
        {
            v = CoordConversion.FromUnity(v, outputCoordSystem);
            MakeAudioRequest("listener_param", new Dictionary<string, float>
            {
                { "vx", v.x },
                { "vy", v.y },
                { "vz", v.z },
            });
        }

        public void SetListenerFront(Vector3 v)
        {
            v = CoordConversion.FromUnity(v, outputCoordSystem);
            MakeAudioRequest("listener_param", new Dictionary<string, float>
            {
                { "frontx", v.x },
                { "fronty", v.y },
                { "frontz", v.z },
            });
        }

        public void SetListenerUp(Vector3 v)
        {
            v = CoordConversion.FromUnity(v, outputCoordSystem);
            MakeAudioRequest("listener_param", new Dictionary<string, float>
            {
                { "upx", v.x },
                { "upy", v.y },
                { "upz", v.z },
            });
        }

        public void SetListenerGain(float g)
        {
            MakeAudioRequest("listener_param", new Dictionary<string, float> {{ "gain", g }});
        }

#endregion

#region Source Methods
        public void CreateSource(int sourceID, string soundFile)
        {
            MakeAudioRequest("create_source", new Dictionary<string, string>
            {
                { "id", sourceID.ToString() },
                { "snd", soundFile }
            });
        }

        public void CreateSource(int sourceID, string soundFile, Vector3 position)
        {
            position = CoordConversion.FromUnity(position, outputCoordSystem);
            MakeAudioRequest("create_source", new Dictionary<string, string>
            {
                { "id", sourceID.ToString() },
                { "snd", soundFile },
                { "x", position.x.ToString() },
                { "y", position.y.ToString() },
                { "z", position.z.ToString() }
            });
        }

        public void CreateSource(int sourceID, string soundFile, Vector3 position, bool looping)
        {
            position = CoordConversion.FromUnity(position, outputCoordSystem);
            MakeAudioRequest("create_source", new Dictionary<string, string>
            {
                { "id", sourceID.ToString() },
                { "snd", soundFile },
                { "x", position.x.ToString() },
                { "y", position.y.ToString() },
                { "z", position.z.ToString() },
                { "looping", looping.ToString() }
            });
        }

        public void SetSourcePosition(int sourceID, Vector3 v)
        {
            v = CoordConversion.FromUnity(v, outputCoordSystem);
            MakeAudioRequest("source_param", new Dictionary<string, float>
            {
                { "id", sourceID },
                { "x", v.x },
                { "y", v.y },
                { "z", v.z },
            });
        }

        public void SetSourceVelocity(int sourceID, Vector3 v)
        {
            v = CoordConversion.FromUnity(v, outputCoordSystem);
            MakeAudioRequest("source_param", new Dictionary<string, float>
            {
                { "id", sourceID },
                { "vx", v.x },
                { "vy", v.y },
                { "vz", v.z },
            });
        }

        public void SetSourceFront(int sourceID, Vector3 v)
        {
            v = CoordConversion.FromUnity(v, outputCoordSystem);
            MakeAudioRequest("source_param", new Dictionary<string, float>
            {
                { "id", sourceID },
                { "frontx", v.x },
                { "fronty", v.y },
                { "frontz", v.z },
            });
        }

        public void SetSourceUp(int sourceID, Vector3 v)
        {
            v = CoordConversion.FromUnity(v, outputCoordSystem);
            MakeAudioRequest("source_param", new Dictionary<string, float>
            {
                { "id", sourceID },
                { "upx", v.x },
                { "upy", v.y },
                { "upz", v.z },
            });
        }

        public void SetSourceDirection(int sourceID, Vector3 v)
        {
            v = CoordConversion.FromUnity(v, outputCoordSystem);
            MakeAudioRequest("source_param", new Dictionary<string, float>
            {
                { "id", sourceID },
                { "dx", v.x },
                { "dy", v.y },
                { "dz", v.z },
            });
        }

        public void SetSourceGain(int sourceID, float g)
        {
            MakeAudioRequest("source_param", new Dictionary<string, float>
            {
                { "id", sourceID },
                { "gain", g }
            });
        }

        public void SetSourcePitch(int sourceID, float p)
        {
            MakeAudioRequest("source_param", new Dictionary<string, float>
            {
                { "id", sourceID },
                { "pitch", p }
            });
        }

        public void SetSourceLooping(int sourceID, bool l)
        {
            MakeAudioRequest("source_param", new Dictionary<string, string>
            {
                { "id", sourceID.ToString() },
                { "looping", l.ToString() }
            });
        }

        public void PlaySource(int sourceID)
        {
            MakeAudioRequest("play_source", new Dictionary<string, string>
            {
                { "id", sourceID.ToString() }
            });
        }

        public void PauseSource(int sourceID)
        {
            MakeAudioRequest("pause_source", new Dictionary<string, string>
            {
                { "id", sourceID.ToString() }
            });
        }

        public void StopSource(int sourceID)
        {
            MakeAudioRequest("stop_source", new Dictionary<string, string>
            {
                { "id", sourceID.ToString() }
            });
        }

        public void RewindSource(int sourceID)
        {
            MakeAudioRequest("rewind_source", new Dictionary<string, string>
            {
                { "id", sourceID.ToString() }
            });
        }

        public void DeleteSource(int sourceID)
        {
            MakeAudioRequest("del_source", new Dictionary<string, string>
            {
                { "id", sourceID.ToString() }
            });
        }

#endregion

#region Simple Audio (Non-Spatial)

        public void PlaySimple(string soundFile)
        {
            MakeAudioRequest("play", new Dictionary<string, string>
            {
                { "snd", soundFile }
            });
        }

        public void LoopSimple(string soundFile)
        {
            MakeAudioRequest("loop", new Dictionary<string, string>
            {
                { "snd", soundFile }
            });
        }

        public void StopSimple(string soundFile)
        {
            MakeAudioRequest("stop", new Dictionary<string, string>
            {
                { "snd", soundFile }
            });
        }

        public void StopAllSimple()
        {
            MakeAudioRequest("stop_all");
        }
#endregion
    }
}