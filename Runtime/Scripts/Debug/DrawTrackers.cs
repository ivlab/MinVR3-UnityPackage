using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace IVLab.MinVR3 {

    [AddComponentMenu("MinVR/Debug/Draw Trackers")]
    public class DrawTrackers : MonoBehaviour, IVREventReceiver {
        
        [Tooltip("Prefab of axes geometry to use as a cursor.  If the GameObject has a TextMesh component, the text will be set to the tracker name.")]
        public GameObject cursorPrefab;

        [Serializable]
        public class TrackerDescription {
            public TrackerDescription()
            {
                displayName = "Unknown Tracker (" + counter + ")";
                counter++;
                positionEvent = new VREventPrototype<Vector3>("");
                rotationEvent = new VREventPrototype<Quaternion>("");
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
            VREngine.instance.eventManager.AddEventReceiver(this);
        }

        private void OnDisable()
        {
            VREngine.instance?.eventManager?.RemoveEventReceiver(this);
        }

        public void OnVREvent(VREvent vrEvent)
        {
            foreach (TrackerDescription t in trackers) {
                if ((t.positionEvent.Matches(vrEvent)) || (t.rotationEvent.Matches(vrEvent))) {
                    if (!cursors.ContainsKey(t.displayName)) {
                        GameObject newCursorObj = Instantiate(cursorPrefab);
                        TextMesh label = newCursorObj.GetComponentInChildren<TextMesh>();
                        if (label != null) {
                            label.text = t.displayName;
                        }
                        cursors[t.displayName] = newCursorObj;
                    }
                    GameObject cursorObj = cursors[t.displayName];
                    if (t.positionEvent.Matches(vrEvent)) {
                        cursorObj.transform.position = (vrEvent as VREvent<Vector3>).data;
                    } else if (t.rotationEvent.Matches(vrEvent)) {
                        cursorObj.transform.rotation = (vrEvent as VREvent<Quaternion>).data;
                    }
                }
            }
        }


        // map tracker names to game objects
        private Dictionary<string, GameObject> cursors = new Dictionary<string, GameObject>();
    }

} // namespace
