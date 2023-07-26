using System.Collections.Generic;
using System;
using UnityEngine;
using IVLab.MinVR3.ExtensionMethods;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Clickable and draggable camera for trackball camera movement in Unity. Can
    /// optionally enable "acceleration" to avoid users getting sick in situations
    /// like a planetarium.
    ///
    /// TODO: @bridger: It seems like this class only works properly if the hierarchy
    /// is setup in a certain way.  It seems this script must be on an object
    /// under another object that is the "trackball parent" -- does that mean it
    /// contains all of the geometry that should be rotated by the trackball?
    /// And, the camera must be a child of the gameobject this script is attached to?
    /// Can you add documentation on how the hierarchy must be arranged?  And, for
    /// the GameObject menu, when you add one of these to the scene, it would be
    /// useful if it could create the required mini-hierarchy for you.
    /// </summary>
    [AddComponentMenu("MinVR Interaction/Desktop/TrackballCamera")]
    public class TrackballCamera : MonoBehaviour
    {
        [Header("Click checkbox to reset view")]
        [SerializeField, Tooltip("Reset the camera's view to its initial view")]
        private bool resetView;

        [Header("Styling controls")]
        [SerializeField, Tooltip("Show the on-screen widgets for indicating current axis and orbit/zoom/pan/rotate mode")]
        private bool showWidgets;

        [Header("Speed controls")]
        [SerializeField, Tooltip("Multiplier for orbit and rotational speed")]
        private float rotationSpeed = 0.01f;
        [SerializeField, Tooltip("Multiplier for pan speed")]
        private float panSpeed = 0.0005f;
        [SerializeField, Tooltip("Multiplier for zoom/truck speed")]
        private float zoomSpeed = 0.001f;

        [Header("Camera control events")]
        [SerializeField, Tooltip("Event for mouse position on the screen, in pixel coordinates")]
        private VREventPrototypeVector2 mousePositionEvent;
        [SerializeField, Tooltip("Event for orbit start")]
        private VREventPrototype orbitStartEvent;
        [SerializeField, Tooltip("Event for orbit end")]
        private VREventPrototype orbitEndEvent;
        [SerializeField, Tooltip("Event for truck/zoom start")]
        private VREventPrototype truckStartEvent;
        [SerializeField, Tooltip("Event for truck/zoom end")]
        private VREventPrototype truckEndEvent;
        [SerializeField, Tooltip("Event for pan start")]
        private VREventPrototype panStartEvent;
        [SerializeField, Tooltip("Event for pan end")]
        private VREventPrototype panEndEvent;
        [SerializeField, Tooltip("Event for rotate start")]
        private VREventPrototype rotateStartEvent;
        [SerializeField, Tooltip("Event for rotate end")]
        private VREventPrototype rotateEndEvent;

        // Widgets for on-screen display of movement mode
        private GameObject rotationWidget;
        private GameObject truckWidget;
        private GameObject panWidget;
        private GameObject axesWidget;

        // State indicators
        private Vector2 mousePositionAtStart = Vector2.zero;
        private Vector2 lastMousePosition = Vector2.zero;
        //private bool mousePressed = false;
        private bool firstMovement = false;
        private float slowing = 0.0f;
        private const float SlowingDuration = 5.0f; // seconds

        // State machines for each type of interaction/movement
        private Dictionary<TrackballState, FSM> fsms;

        // Apparently in Unity it is better to represent angular velocity as
        // Euler Angles rather than as Quaternions. Absolute rotation remains as
        // quaternions.
        private Vector3 angularVelocity = Vector3.zero;
        private Vector3 panVelocity = Vector3.zero;

        // Keep track of initial transforms of Trackball / Camera Pivot (this) / Main
        // Camera GameObjects
        private Transform trackballParent;
        private Transform mainCamera;

        private Matrix4x4 initialTrackballXform;
        private Matrix4x4 initialThisXform;
        private Matrix4x4 initialMainCameraXform;

        // Does not lines up with state specified in Finite State Machines
        // (since each FSM only has 2 states)
        public enum TrackballState
        {
            Orbit = 1,
            Truck = 2,
            Pan = 3,
            Rotate = 4,
        }

        void Reset()
        {
            // Reset widget visibility
            showWidgets = true;

            // Reset movement multipliers
            rotationSpeed = 0.01f;
            panSpeed = 0.0005f;
            zoomSpeed = 0.001f;

            // Reset events to default
            mousePositionEvent  = VREventPrototypeVector2.Create("TrackballCamera/MousePosition");
            orbitStartEvent     = VREventPrototype.Create("TrackballCamera/Orbit/On");
            orbitEndEvent       = VREventPrototype.Create("TrackballCamera/Orbit/Off");
            truckStartEvent     = VREventPrototype.Create("TrackballCamera/Truck/On");
            truckEndEvent       = VREventPrototype.Create("TrackballCamera/Truck/Off");
            panStartEvent       = VREventPrototype.Create("TrackballCamera/Pan/On");
            panEndEvent         = VREventPrototype.Create("TrackballCamera/Pan/Off");
            rotateStartEvent    = VREventPrototype.Create("TrackballCamera/Rotate/On");
            rotateEndEvent      = VREventPrototype.Create("TrackballCamera/Rotate/Off");
        }

        void Start()
        {
            // Populate relative GameObjects
            trackballParent = this.transform.parent;
            mainCamera = this.transform.GetComponentInChildren<Camera>().transform;

            // Populate starting transforms
            initialTrackballXform = trackballParent.localToWorldMatrix;
            initialThisXform = this.transform.localToWorldMatrix;
            initialMainCameraXform = mainCamera.localToWorldMatrix;

            // Find widgets in scene
            axesWidget = transform.GetChild(0).Find("LeftHandAxis").gameObject;
            panWidget = transform.GetChild(0).Find("pan_arrow").gameObject;
            truckWidget = transform.GetChild(0).Find("truck_arrow").gameObject;
            rotationWidget = transform.GetChild(0).Find("rotation_axes").gameObject;

            axesWidget.SetActive(showWidgets);

            var startCallbacks = new Dictionary<TrackballState, VREventPrototype>
            {
                { TrackballState.Orbit, orbitStartEvent },
                { TrackballState.Truck, truckStartEvent },
                { TrackballState.Pan, panStartEvent },
                { TrackballState.Rotate, rotateStartEvent },
            };
            var endCallbacks = new Dictionary<TrackballState, VREventPrototype>
            {
                { TrackballState.Orbit, orbitEndEvent },
                { TrackballState.Truck, truckEndEvent },
                { TrackballState.Pan, panEndEvent },
                { TrackballState.Rotate, rotateEndEvent },
            };

            fsms = new Dictionary<TrackballState, FSM>();
            foreach (var state in Enum.GetValues(typeof(TrackballState)))
            {
                var fsm = this.gameObject.AddComponent<FSM>();

                fsm.AddState("Idle");
                fsm.AddState(
                    state.ToString(),
                    onEnterCallback: VRCallback.CreateRuntime(() => OnMovementStart((TrackballState) state)),
                    onExitCallback: VRCallback.CreateRuntime(() => OnMovementEnd((TrackballState) state))
                );

                fsm.AddArc("Idle", state.ToString(), VREventCallbackAny.CreateRuntime(startCallbacks[(TrackballState) state]));
                fsm.AddArc(state.ToString(), "Idle", VREventCallbackAny.CreateRuntime(endCallbacks[(TrackballState) state]));
                fsm.AddArc(state.ToString(), state.ToString(), VREventCallbackAny.CreateRuntime(mousePositionEvent, pos => OnMovement((TrackballState) state, pos)));

                fsms.Add((TrackballState) state, fsm);
            }
        }

        public void OnMovementStart(TrackballState mvmt)
        {
            firstMovement = true;
            slowing = 0.0f;
        }

        public void OnMovementEnd(TrackballState mvmt)
        {
            slowing = SlowingDuration;
        }

        public void OnMovement(TrackballState movement, Vector2 mousePosition)
        {
            if (firstMovement)
            {
                mousePositionAtStart = mousePosition;
                firstMovement = false;
            }

            Vector2 mouseDelta = mousePosition - mousePositionAtStart;

            if (movement == TrackballState.Orbit)
            {
                angularVelocity += new Vector3(-mouseDelta.y, mouseDelta.x, 0.0f) * Time.deltaTime * rotationSpeed;
            }
            else if (movement == TrackballState.Truck)
            {
                panVelocity += new Vector3(0.0f, 0.0f, mouseDelta.y) * Time.deltaTime * zoomSpeed;
            }
            else if (movement == TrackballState.Pan)
            {
                panVelocity += new Vector3(-mouseDelta.x, -mouseDelta.y, 0.0f) * Time.deltaTime * panSpeed;
            }
            else if (movement == TrackballState.Rotate)
            {
                angularVelocity += new Vector3(0.0f, 0.0f, -mouseDelta.y + mouseDelta.x) * Time.deltaTime * rotationSpeed;
            }

            lastMousePosition = mousePosition;
        }

        void ResetView()
        {
            angularVelocity = Vector3.zero;
            panVelocity = Vector3.zero;

            trackballParent.position = initialTrackballXform.GetTranslationFast();
            trackballParent.rotation = initialTrackballXform.GetRotationFast();
            trackballParent.localScale = initialTrackballXform.GetScaleFast();

            this.transform.position = initialThisXform.GetTranslationFast();
            this.transform.rotation = initialThisXform.GetRotationFast();
            this.transform.localScale = initialThisXform.GetScaleFast();

            mainCamera.position = initialMainCameraXform.GetTranslationFast();
            mainCamera.rotation = initialMainCameraXform.GetRotationFast();
            mainCamera.localScale = initialMainCameraXform.GetScaleFast();
        }

        void Update()
        {
            if (slowing > 0.0f)
            {
                angularVelocity = Vector3.LerpUnclamped(Vector3.zero, angularVelocity, slowing / SlowingDuration);
                panVelocity = Vector3.LerpUnclamped(Vector3.zero, panVelocity, slowing / SlowingDuration);
                slowing -= Time.deltaTime;
            }
            transform.rotation *= Quaternion.Euler(angularVelocity);

            Transform cam = this.transform.GetChild(0);
            var pos = this.transform.parent.position;
            var camLocalPosition = cam.transform.localPosition;
            pos += cam.transform.right * panVelocity.x;
            pos += cam.transform.up * panVelocity.y;
            camLocalPosition.z += panVelocity.z;
            this.transform.parent.position = pos;
            cam.transform.localPosition = camLocalPosition;

            // Set the position of the axes widget based on the camera's aspect
            // ratio
            // Here instead of Start() in case window size changes
            Camera c = cam.GetComponent<Camera>();
            var camPos = axesWidget.transform.localPosition;
            camPos.x = camPos.y * c.aspect;
            axesWidget.transform.localPosition = camPos;
            axesWidget.SetActive(showWidgets);

            // Set the rotation of the axes and rotation widgets so they line up with global xyz
            axesWidget.transform.rotation = Quaternion.identity;
            rotationWidget.transform.rotation = Quaternion.identity;

            // Set visibility of other widgets
            rotationWidget.SetActive(showWidgets && (fsms[TrackballState.Orbit].currentStateID != 0 || fsms[TrackballState.Rotate].currentStateID != 0));
            truckWidget.SetActive(showWidgets && fsms[TrackballState.Truck].currentStateID != 0);
            panWidget.SetActive(showWidgets && fsms[TrackballState.Pan].currentStateID != 0);

            if (resetView)
            {
                ResetView();
                resetView = false;
            }
        }
    }
}