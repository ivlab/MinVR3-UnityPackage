﻿using UnityEngine;
using System.Collections.Generic;

namespace IVLab.MinVR3 {

    /* Genertates fake VREvents for 3 trackers (usually assigned to the head and 2 hands).  This is similar to the 
	 * XR Device Simulator provided with Unity's XR Interaction Toolkit, but this is a simpler simulator and has better
	 * mouse and keyboard controls (IMHO).
	 * 
	 * This is only intended to be used while debugging and testing; not for an actual deployed VR app!
	 * 
	 * To make debugging VR apps easier, you can use the mouse and keyboard to create 'fake' input for two trackers.
	 * "Press the '1' or '2' key to switch between controlling tracker1 or tracker2. Move the mouse around the screen
	 * to move the 3D position of that tracker within a plane parallel to the screen.  Hold down 'left shift' while
	 * moving the mouse vertically to change the 3D depth. Hold 'x', 'y', or 'z' while moving the mouse horizontally
	 * to rotate the tracker around the X, Y, or Z axis.
	 * 
	 * Move the head around with the arrow keys.
	 */
    [AddComponentMenu("MinVR/Input/Legacy Input Module/Fake Tracking Input")]
    public class FakeTrackingInputLegacy : MonoBehaviour, IVREventProducer {
        [Header("Head Tracker")]
        [SerializeField] private KeyCode moveHeadForwardKey;
        [SerializeField] private KeyCode moveHeadBackKey;
        [SerializeField] private KeyCode turnHeadLeftKey;
        [SerializeField] private KeyCode turnHeadRightKey;

        [Tooltip("Fake head tracking with arrow keys. 'up' moves forward, 'down' moves backward, 'left' rotates left, 'right' rotates right.")]
        public string headTrackerBaseName;

        public Vector3 initialHeadPos;
        private Vector3 headTrackerPos;

        public Vector3 initialHeadRot;
        private Quaternion headTrackerRot;


        [Header("Trackers")]
        [SerializeField] private KeyCode toggleTracker1Key;
        [SerializeField] private KeyCode toggleTracker2Key;

        [SerializeField] private KeyCode rotateXKey;
        [SerializeField] private KeyCode rotateYKey;
        [SerializeField] private KeyCode rotateZKey;
        [SerializeField] private KeyCode dollyKey;

        [Tooltip("Base name of the VREvent generated by the first fake tracker.")]
        public string tracker1BaseName;

        public Vector3 initialTracker1Pos;
        private Vector3 tracker1Pos;
        public Vector3 initialTracker1Rot;
        private Quaternion tracker1Rot;

        [Tooltip("Base name of the VREvent generated by the second fake tracker.")]
        public string tracker2BaseName;

        public Vector3 initialTracker2Pos;
        private Vector3 tracker2Pos;
        public Vector3 initialTracker2Rot;
        private Quaternion tracker2Rot;

        private int curTracker;
        private float lastx;
        private float lasty;


        private void OnEnable()
        {
            VREngine.instance.eventManager.AddEventProducer(this);
        }

        private void OnDisable()
        {
            VREngine.instance?.eventManager?.RemoveEventProducer(this);
        }

        void Reset()
        {
            headTrackerBaseName = "Head";
            initialHeadPos = new Vector3(0, 1, -2);
            initialHeadRot = new Vector3();
            
            tracker1BaseName = "DHand";
            initialTracker1Pos = new Vector3();
            initialTracker1Rot = new Vector3();

            tracker2BaseName = "NDHand";
            initialTracker2Pos = new Vector3();
            initialTracker2Rot = new Vector3();

            moveHeadForwardKey = KeyCode.UpArrow;
            moveHeadBackKey = KeyCode.DownArrow;
            turnHeadLeftKey = KeyCode.LeftArrow;
            turnHeadRightKey = KeyCode.RightArrow;
            toggleTracker1Key = KeyCode.Alpha1;
            toggleTracker2Key = KeyCode.Alpha2;
            rotateXKey = KeyCode.X;
            rotateYKey = KeyCode.Y;
            rotateZKey = KeyCode.Z;
            dollyKey = KeyCode.LeftShift;
        }

        void Start() {
            curTracker = 0;
            lastx = float.NaN;
            lasty = float.NaN;
            headTrackerPos = initialHeadPos;
            headTrackerRot = Quaternion.Euler(initialHeadRot);
            tracker1Pos = initialTracker1Pos;
            tracker1Rot = Quaternion.Euler(initialTracker1Rot);
            tracker2Pos = initialTracker2Pos;
            tracker2Rot = Quaternion.Euler(initialTracker1Rot);
        }

        void Update()
        {
            QueueHeadTrackerEvents();
            QueueRegularTrackerEvents();
        }



        private bool IsPressed(KeyCode key)
        {
            return Input.GetKey(key);
        }

        private bool WasPressedThisFrame(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        private void QueueHeadTrackerEvents() {
            bool sendEvent = false;

            if (IsPressed(moveHeadForwardKey)) {
                sendEvent = true;
                headTrackerPos += 0.1f * Camera.main.transform.forward;
            }
            else if (IsPressed(moveHeadBackKey)) {
                sendEvent = true;
                headTrackerPos -= 0.1f * Camera.main.transform.forward;
            }
            else if (IsPressed(turnHeadLeftKey)) {
                sendEvent = true;
                headTrackerRot *= Quaternion.AngleAxis(-1.0f, new Vector3(0f, 1f, 0f));
            }
            else if (IsPressed(turnHeadRightKey)) {
                sendEvent = true;
                headTrackerRot *= Quaternion.AngleAxis(1.0f, new Vector3(0f, 1f, 0f));
            }

            if (sendEvent) {
                VREngine.instance.eventManager.QueueEvent(headTrackerBaseName + "/Position", headTrackerPos);
                VREngine.instance.eventManager.QueueEvent(headTrackerBaseName + "/Rotation", headTrackerRot);
            }
        }


        private void QueueRegularTrackerEvents() {
            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;


            // first time through
            if (float.IsNaN(lastx)) {
                lastx = x;
                lasty = y;
                return;
            }

            bool sendEvent = false;

            if (WasPressedThisFrame(toggleTracker1Key)) {
                curTracker = 0;
            } else if (WasPressedThisFrame(toggleTracker2Key)) {
                curTracker = 1;
            }

            if (IsPressed(rotateXKey)) {
                float angle = 0.1f * (x - lastx);
                sendEvent = (angle != 0.0);
                if (curTracker == 0) {
                    tracker1Rot = Quaternion.AngleAxis(angle, new Vector3(1f, 0f, 0f)) * tracker1Rot;
                } else if (curTracker == 1) {
                    tracker2Rot = Quaternion.AngleAxis(angle, new Vector3(1f, 0f, 0f)) * tracker2Rot;
                }
            } else if (IsPressed(rotateYKey)) {
                float angle = 0.1f * (x - lastx);
                sendEvent = (angle != 0.0);
                if (curTracker == 0) {
                    tracker1Rot = Quaternion.AngleAxis(angle, new Vector3(0f, 1f, 0f)) * tracker1Rot;
                } else if (curTracker == 1) {
                    tracker2Rot = Quaternion.AngleAxis(angle, new Vector3(0f, 1f, 0f)) * tracker2Rot;
                }
            } else if (IsPressed(rotateZKey)) {
                float angle = 0.1f * (x - lastx);
                sendEvent = (angle != 0.0);
                if (curTracker == 0) {
                    tracker1Rot = Quaternion.AngleAxis(angle, new Vector3(0f, 0f, 1f)) * tracker1Rot;
                } else if (curTracker == 1) {
                    tracker2Rot = Quaternion.AngleAxis(angle, new Vector3(0f, 0f, 1f)) * tracker2Rot;
                }
            } else if (IsPressed(dollyKey)) {
                float depth = 0.005f * (y - lasty);
                sendEvent = (depth != 0.0);
                if (curTracker == 0) {
                    tracker1Pos += depth * Camera.main.transform.forward;
                } else if (curTracker == 1) {
                    tracker2Pos += depth * Camera.main.transform.forward;
                }
            } else {
                sendEvent = (x != lastx) || (y != lasty);
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0f));
                Plane p = new Plane();
                float dist = 0.0f;
                if (curTracker == 0) {
                    p.SetNormalAndPosition(-Camera.main.transform.forward, tracker1Pos);
                    if (p.Raycast(ray, out dist)) {
                        tracker1Pos = ray.GetPoint(dist);
                    }
                } else if (curTracker == 1) {
                    p.SetNormalAndPosition(-Camera.main.transform.forward, tracker2Pos);
                    if (p.Raycast(ray, out dist)) {
                        tracker2Pos = ray.GetPoint(dist);
                    }
                }

            }

            if (sendEvent) {
                if (curTracker == 0) {
                    VREngine.instance.eventManager.QueueEvent(tracker1BaseName + "/Position", tracker1Pos);
                    VREngine.instance.eventManager.QueueEvent(tracker1BaseName + "/Rotation", tracker1Rot);
                } else if (curTracker == 1) {
                    VREngine.instance.eventManager.QueueEvent(tracker2BaseName + "/Position", tracker2Pos);
                    VREngine.instance.eventManager.QueueEvent(tracker2BaseName + "/Rotation", tracker2Rot);
                }
            }

            this.lastx = x;
            this.lasty = y;
        }

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> allEvents = new List<IVREventPrototype>();
            allEvents.Add(VREventPrototypeVector3.Create(headTrackerBaseName + "/Position"));
            allEvents.Add(VREventPrototypeQuaternion.Create(headTrackerBaseName + "/Rotation"));

            allEvents.Add(VREventPrototypeVector3.Create(tracker1BaseName + "/Position"));
            allEvents.Add(VREventPrototypeQuaternion.Create(tracker1BaseName + "/Rotation"));

            allEvents.Add(VREventPrototypeVector3.Create(tracker2BaseName + "/Position"));
            allEvents.Add(VREventPrototypeQuaternion.Create(tracker2BaseName + "/Rotation"));

            return allEvents;
        }

    }

} // namespace