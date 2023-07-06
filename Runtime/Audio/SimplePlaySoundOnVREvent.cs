using UnityEngine;

namespace IVLab.MinVR3
{
    [RequireComponent(typeof(SpatialAudioClient))]
    public class SimplePlaySoundOnVREvent : MonoBehaviour, IVREventListener
    {
        [SerializeField, Tooltip("Name of the sound file to play (MUST exist on Sound Server)")]
        public string soundFileName;

        [SerializeField, Tooltip("Play the sound file when THIS VREvent is received")]
        public VREventPrototypeAny playSoundOnVREvent;

        private SpatialAudioClient audioClient;

        void OnEnable()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        void OnDisable()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }

        void Start()
        {
            // Ensure spatial audio client exists. Ideally should be set up
            // in-scene prior to runtime so that you can set up its parameters.
            audioClient = this.GetComponent<SpatialAudioClient>();
        }

        public void OnVREvent(VREvent evt)
        {
            if (evt.Matches(playSoundOnVREvent))
            {
                audioClient.PlaySimple(soundFileName);
            }
        }

        public void StartListening() { }
        public void StopListening() { }
    }
}