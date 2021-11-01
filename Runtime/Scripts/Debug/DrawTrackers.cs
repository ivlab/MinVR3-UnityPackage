using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace IVLab.MinVR3 {

    [AddComponentMenu("MinVR/Debug/Draw Trackers")]
    public class DrawTrackers : MonoBehaviour, IVREventListener {
        
        [Tooltip("Prefab of axes geometry to use as a cursor.  If the GameObject has a TextMesh component, the text will be set to the tracker name.")]
        public GameObject cursorPrefab;

        [Serializable]
        public class TrackerDescription {
            public TrackerDescription()
            {
                displayName = "Unknown Tracker (" + counter + ")";
                counter++;
                positionEvent = new VREventPrototype<Vector3>();
                rotationEvent = new VREventPrototype<Quaternion>();
            }
            public string displayName;
            public VREventPrototype<Vector3> positionEvent;
            public VREventPrototype<Quaternion> rotationEvent;
            static int counter = 1;
        }

        [Tooltip("Add an entry for each tracker you wish to display.")]
        public List<TrackerDescription> trackers;

        
        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        public void OnVREvent(VREvent vrEvent)
        {
            foreach (TrackerDescription t in trackers) {
                if ((vrEvent.Matches(t.positionEvent)) || (vrEvent.Matches(t.rotationEvent))) {
                    if (!cursors.ContainsKey(t.displayName)) {
                        GameObject newCursorObj = Instantiate(cursorPrefab);
                        TextMesh label = newCursorObj.GetComponentInChildren<TextMesh>();
                        if (label != null) {
                            label.text = t.displayName;
                        }
                        cursors[t.displayName] = newCursorObj;
                    }
                    GameObject cursorObj = cursors[t.displayName];
                    if (vrEvent.Matches(t.positionEvent)) {
                        cursorObj.transform.position = (vrEvent as VREvent<Vector3>).data;
                    } else if (vrEvent.Matches(t.rotationEvent)) {
                        cursorObj.transform.rotation = (vrEvent as VREvent<Quaternion>).data;
                    }
                }
            }
        }

        public bool IsListening()
        {
            return m_Listening;
        }

        public void StartListening()
        {
            VREngine.instance.eventManager.AddEventReceiver(this);
            m_Listening = true;
        }

        public void StopListening()
        {
            VREngine.instance?.eventManager?.RemoveEventReceiver(this);
            m_Listening = false;
        }


        // map tracker names to game objects
        [NonSerialized] private Dictionary<string, GameObject> cursors = new Dictionary<string, GameObject>();
        [NonSerialized] private bool m_Listening = false;
    }

} // namespace
