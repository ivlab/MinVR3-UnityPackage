using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

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
        private Task connectionTask;
        private CancellationTokenSource connectTaskTokenSource;

        private static int SourceCount = 0;

        void OnEnable()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        void OnDisable()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
            audioClient.DeleteSource(sourceID);
            connectTaskTokenSource.Cancel();
        }

        void Start()
        {
            // Create the audio source.
            sourceID = (int) System.DateTimeOffset.Now.ToUnixTimeSeconds() + SourceCount;
            SourceCount += 1;
            audioClient = this.GetComponent<SpatialAudioClient>();

            connectTaskTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                try
                {
                    await audioClient.WaitForInitialized();

                    audioClient.CreateSource(sourceID, soundFileName);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }, connectTaskTokenSource.Token);
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