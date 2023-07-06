using UnityEngine;

namespace IVLab.MinVR3
{
    public class SimplePlaySoundOnVREvent : MonoBehaviour, IVREventListener
    {
        [SerializeField, Tooltip("Name of the sound file to play (MUST exist on Sound Server)")]
        public string soundFileName;

        [SerializeField, Tooltip("Play the sound file when THIS VREvent is received")]
        public VREventPrototypeAny playSoundOnVREvent;

        void OnEnable()
        {
            VREngine.Instance.eventManager.AddEventListener(this);

            // Ensure spatial audio client exists. Ideally should be set up
            // in-scene prior to runtime so that you can set up its parameters.
            SpatialAudioClient.GetInstance();
        }

        void OnDisable()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }

        public void OnVREvent(VREvent evt)
        {
            if (evt.Matches(playSoundOnVREvent))
            {
                SpatialAudioClient.Instance.PlaySimple(soundFileName);
            }
        }

        public void StartListening() { }
        public void StopListening() { }
    }
}