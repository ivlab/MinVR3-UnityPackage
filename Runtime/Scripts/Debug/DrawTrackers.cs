using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace IVLab.MinVR3 {

    [AddComponentMenu("MinVR/Debug/Draw Trackers")]
    public class DrawTrackers : MonoBehaviour, IVREventListener {
        
        [Tooltip("Prefab of axes geometry to use as a cursor.  If the GameObject has a TextMesh component, the text will be set to the tracker name.")]
        public GameObject cursorPrefab;

        public float cursorSize = 0.1f;

        [Serializable]
        public class TrackerDescription {
            public TrackerDescription()
            {
                displayName = "Unknown Tracker (" + counter + ")";
                counter++;
                positionEvent = VREventPrototypeVector3.Create("(none)");
                rotationEvent = VREventPrototypeQuaternion.Create("(none)");
            }
            public string displayName;
            public VREventPrototypeVector3 positionEvent;
            public VREventPrototypeQuaternion rotationEvent;
            static int counter = 1;
        }

        [Tooltip("Add an entry for each tracker you wish to display.")]
        public List<TrackerDescription> trackers;

        void Reset()
        {
            trackers = new List<TrackerDescription>();

            TrackerDescription example = new TrackerDescription();
            trackers.Add(example);
        }
        
        private void OnEnable()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        private void OnDisable()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }

        public void OnVREvent(VREvent vrEvent)
        {
            foreach (TrackerDescription t in trackers) {
                if ((vrEvent.Matches(t.positionEvent)) || (vrEvent.Matches(t.rotationEvent))) {
                    if (!cursors.ContainsKey(t.displayName)) {
                        GameObject newCursorObj = Instantiate(cursorPrefab);
                        newCursorObj.transform.localScale = Vector3.one * cursorSize;
                        TextMesh label = newCursorObj.GetComponentInChildren<TextMesh>();
                        if (label != null) {
                            label.text = t.displayName;
                        }
                        cursors[t.displayName] = newCursorObj;
                    }
                    GameObject cursorObj = cursors[t.displayName];
                    if (vrEvent.Matches(t.positionEvent)) {
                        cursorObj.transform.position = vrEvent.GetData<Vector3>();
                    } else if (vrEvent.Matches(t.rotationEvent)) {
                        cursorObj.transform.rotation = vrEvent.GetData<Quaternion>();
                    }
                }
            }
        }

        public void StartListening()
        {
        }

        public void StopListening()
        {
        }


        // map tracker names to game objects
        [NonSerialized] private Dictionary<string, GameObject> cursors = new Dictionary<string, GameObject>();
        [NonSerialized] private bool m_Listening = false;
    }

} // namespace
