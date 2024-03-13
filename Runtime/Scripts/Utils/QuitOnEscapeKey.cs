using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Listens to both Unity and MinVR for Esc key pressed events.  When
    /// running in cluster mode, it can be possible to get an Esc key down
    /// event from MinVR that comes from another cluster node.
    /// </summary>
    public class QuitOnEscapeKey : MonoBehaviour, IVREventListener
    {
        public string minVREscKeyEventName = "Keyboard/Esc/Down";

        VREventPrototype escDownEvent;

        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(escDownEvent)) {
                Shutdown();
            }
        }

        public void StartListening()
        {
            VREngine.instance.eventManager.AddEventListener(this);
        }

        public void StopListening()
        {
            VREngine.instance?.eventManager?.RemoveEventListener(this);
        }

        void OnEnable()
        {
            StartListening();
        }

        void OnDisable()
        {
            StopListening();
        }

        void Start()
        {
            escDownEvent = VREventPrototype.Create(minVREscKeyEventName);
        }

        void Update()
        {
            if (KeyboardState.KeyWasPressedThisFrame(KeyCode.Escape)) {
                Shutdown();
            }
        }

        void Shutdown()
        {
            Debug.Log("Escape Key Pressed.  Quitting Applicaiton.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
