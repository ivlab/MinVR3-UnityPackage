using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace IVLab.MinVR3 {

    [AddComponentMenu("MinVR/Interaction/Debug Draw Trackers")]
    public class DebugDrawTrackers : MonoBehaviour, IVREventReceiver {
        
        [Tooltip("Prefab of axes geometry to use as a cursor.  If the GameObject has a TextMesh component, the text will be set to the tracker name.")]
        public GameObject cursorPrefab;

        [Serializable]
        public class TrackerDescription {
            public TrackerDescription()
            {
                displayName = "Unknown Tracker (" + counter + ")";
                counter++;
                positionEvent = new VREventReference("", "Vector3", true);
                rotationEvent = new VREventReference("", "Quaternion", true);
            }
            public string displayName;
            public VREventReference positionEvent;
            public VREventReference rotationEvent;
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
            VREngine.instance.eventManager.RemoveEventReceiver(this);
        }

        public void OnVREvent(VREventInstance vrEvent)
        {
            foreach (TrackerDescription t in trackers) {
                if ((t.positionEvent.name == vrEvent.name) || (t.rotationEvent.name == vrEvent.name)) {
                    if (!cursors.ContainsKey(t.displayName)) {
                        GameObject newCursorObj = Instantiate(cursorPrefab);
                        TextMesh label = newCursorObj.GetComponentInChildren<TextMesh>();
                        if (label != null) {
                            label.text = t.displayName;
                        }
                        cursors[t.displayName] = newCursorObj;
                    }
                    GameObject cursorObj = cursors[t.displayName];
                    if (t.positionEvent.name == vrEvent.name) {
                        cursorObj.transform.position = (vrEvent as VREventInstance<Vector3>).data;
                    } else if (t.rotationEvent.name == vrEvent.name) {
                        cursorObj.transform.rotation = (vrEvent as VREventInstance<Quaternion>).data;
                    }
                }
            }
        }


        // map tracker names to game objects
        private Dictionary<string, GameObject> cursors = new Dictionary<string, GameObject>();
    }

} // namespace
