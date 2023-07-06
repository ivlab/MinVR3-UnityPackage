using UnityEngine;
using System.Threading.Tasks;

namespace IVLab.MinVR3
{
    // ensure this runs before Spatial Audio Client so it can destroy properly
    [DefaultExecutionOrder(SpatialAudioClient.ScriptPriority - 1)]
    [RequireComponent(typeof(SpatialAudioClient))]
    public class SpatialPlaySoundOnVREvent : MonoBehaviour, IVREventListener
    {
        [SerializeField, Tooltip("Name of the sound file to play (MUST exist on Sound Server)")]
        public string soundFileName;

        [SerializeField, Tooltip("Play the sound file when THIS VREvent is received")]
        public VREventPrototypeAny playSoundOnVREvent;

        [SerializeField, Tooltip("Position the listener based on these events")]
        public VREventPrototypeVector3 listenerPositionVREvent;

        [SerializeField, Tooltip("Position the source based on these events")]
        public VREventPrototypeVector3 sourcePositionVREvent;

        private int sourceID;
        private Vector3 listenerPosition;
        private Vector3 sourcePosition;
        private SpatialAudioClient audioClient;

        void OnEnable()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        void OnDisable()
        {
            Debug.Log("Disable spatial sound vrevent " + Time.frameCount);
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
            Debug.Log(audioClient);
            audioClient.DeleteSource(sourceID);
        }

        void OnDestroy()
        {
            Debug.Log("Destroy spatial sound vrevent + " + Time.frameCount);
        }

        void Start()
        {
            // Create the audio source.
            sourceID = (int) System.DateTimeOffset.Now.ToUnixTimeSeconds();
            audioClient = this.GetComponent<SpatialAudioClient>();
            Task.Run(() =>
            {
                // wait for client to initialize
                while (!audioClient.Initialized)
                {
                    System.Threading.Thread.Sleep(10);
                }

                audioClient.CreateSource(sourceID, soundFileName);
            });
        }

        public void OnVREvent(VREvent evt)
        {
            if (evt.Matches(listenerPositionVREvent))
            {
                listenerPosition = evt.GetData<Vector3>();
            }
            if (evt.Matches(sourcePositionVREvent))
            {
                sourcePosition = evt.GetData<Vector3>();
            }

            if (evt.Matches(playSoundOnVREvent))
            {
                // Set the listener and source positions (only do this when
                // sound is played to avoid spamming the server)
                audioClient.SetListenerPosition(listenerPosition);
                audioClient.SetSourcePosition(sourceID, sourcePosition);

                // Play the sound
                audioClient.PlaySource(sourceID);
            }
        }

        public void StartListening() { }
        public void StopListening() { }
    }
}