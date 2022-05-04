using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Clickable and draggable camera for trackball camera movement in Unity. Can
    /// optionally enable "acceleration" to avoid users getting sick in situations
    /// like a planetarium.
    /// </summary>
    public class TrackballCamera : MonoBehaviour
    {
        public float rotationSpeed = 0.00001f;
        public float panSpeed = 0.0005f;
        public float zoomSpeed = 0.001f;

        public VREventPrototypeVector2 mousePositionEvent;
        public VREventPrototype orbitStartEvent;
        public VREventPrototype orbitEndEvent;
        public VREventPrototype rotateStartEvent;
        public VREventPrototype rotateEndEvent;
        public VREventPrototype truckStartEvent;
        public VREventPrototype truckEndEvent;
        public VREventPrototype panStartEvent;
        public VREventPrototype panEndEvent;

        public GameObject rotationWidget;
        public GameObject truckWidget;
        public GameObject panWidget;
        public GameObject axesWidget;

        public bool showWidgets = true;
        public bool weightedControl = false;

        //zzzzzzzzzzzzzzz
        private Quaternion rotationVelocity = Quaternion.identity;
        Vector2 panAmt = Vector2.zero;
        Vector3 panVelocity = Vector3.zero;

        float speedMult = 1000.0f;

        bool shiftPressed = false;
        bool ctrlPressed = false;

        private Vector2 mousePositionAtStart = Vector2.zero;
        private Vector2 lastMousePosition = Vector2.zero;
        private bool mousePressed = false;

        private FSM fsm;

        private bool firstMovement = false;
        private Dictionary<TrackballMovement, bool> trackballState;
        private float slowing = 0.0f;
        private const float SlowingDuration = 5.0f; // seconds


        // Apparently in Unity it is better to represent angular velocity as
        // Euler Angles rather than as Quaternions. Absolute rotation remains as
        // quaternions.
        private Vector3 angularVelocity = Vector3.zero;

        [System.Serializable]
        public enum TrackballMovement
        {
            Orbit = 0,
            Rotate = 1,
            Pan = 2,
            Truck = 3
        }

        void Reset()
        {
            mousePositionEvent = VREventPrototypeVector2.Create("Mouse/Position");
        }

        void Start()
        {
            axesWidget.SetActive(showWidgets);
            trackballState = new Dictionary<TrackballMovement, bool>
            {
                { TrackballMovement.Orbit, false },
                { TrackballMovement.Truck, false },
                { TrackballMovement.Pan, false },
                { TrackballMovement.Rotate, false },
            };

            fsm = this.gameObject.AddComponent<FSM>();

            fsm.AddState("Idle");
            fsm.AddState("Moving");
            // foreach (var state in trackballState.Keys)
            // {
            //     fsm.AddState(
            //         state.ToString("G"),
            //         onEnterCallback: VRCallback.CreateRuntime(() => OnMovementStart(state)),
            //         onExitCallback: VRCallback.CreateRuntime(() => OnMovementEnd(state))
            //     );
            // }

            var startCallbacks = new Dictionary<TrackballMovement, VREventPrototype>
            {
                { TrackballMovement.Orbit, orbitStartEvent },
                { TrackballMovement.Truck, truckStartEvent },
                { TrackballMovement.Pan, panStartEvent },
                { TrackballMovement.Rotate, rotateStartEvent },
            };
            var endCallbacks = new Dictionary<TrackballMovement, VREventPrototype>
            {
                { TrackballMovement.Orbit, orbitEndEvent },
                { TrackballMovement.Truck, truckEndEvent },
                { TrackballMovement.Pan, panEndEvent },
                { TrackballMovement.Rotate, rotateEndEvent },
            };

            foreach (var state in trackballState.Keys)
            {
                // fsm.AddArc("Idle", state.ToString("G"), VREventCallbackAny.CreateRuntime(startCallbacks[state]));
                // fsm.AddArc(state.ToString("G"), "Idle", VREventCallbackAny.CreateRuntime(endCallbacks[state]));
                fsm.AddArc("Idle", "Moving", VREventCallbackAny.CreateRuntime(startCallbacks[state]));
                fsm.AddArc("Moving", "Idle", VREventCallbackAny.CreateRuntime(endCallbacks[state]));
                fsm.AddArc("Moving", "Moving", VREventCallbackAny.CreateRuntime(mousePositionEvent, pos => OnMovement(state, pos)));
            }
        }

        public void OnMovementStart(TrackballMovement mvmt)
        {
            trackballState[mvmt] = true;
            firstMovement = true;
            slowing = 0.0f;
        }

        public void OnMovementEnd(TrackballMovement mvmt)
        {
            trackballState[mvmt] = false;

            // Start slowing down if all movement has ended
            if (trackballState.Values.All(v => !v))
            {
                slowing = SlowingDuration;
            }
        }

        public void OnMovement(TrackballMovement movement, Vector2 mousePosition)
        {
            if (firstMovement)
            {
                mousePositionAtStart = mousePosition;
                firstMovement = false;
            }

            Vector2 mouseDelta = mousePosition - mousePositionAtStart;

            if (trackballState[TrackballMovement.Orbit])
            {
                angularVelocity += new Vector3(
                    -mouseDelta.y * rotationSpeed * Time.deltaTime * speedMult,
                    mouseDelta.x * rotationSpeed * Time.deltaTime * speedMult,
                    0.0f
                );
            }

            lastMousePosition = mousePosition;
        }

        void Update()
        {
            if (slowing > 0.0f)
            {
                angularVelocity = Vector3.LerpUnclamped(Vector3.zero, angularVelocity, slowing / SlowingDuration);
                slowing -= Time.deltaTime;
            }
            // transform.rotation *= rotationVelocity;
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
            rotationWidget.SetActive(showWidgets && (trackballState[TrackballMovement.Orbit] || trackballState[TrackballMovement.Rotate]));
            truckWidget.SetActive(showWidgets && trackballState[TrackballMovement.Truck]);
            panWidget.SetActive(showWidgets && trackballState[TrackballMovement.Pan]);
        }

        void SchmUpdate()
        {
            // Sync the trick/theta with the actual transforms
            float truck = this.transform.GetChild(0).transform.localPosition.z;

            // Make sure the mouse is not over the GUI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                showWidgets = !showWidgets;
            }

            // Shift-click pans the camera
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                shiftPressed = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            {
                shiftPressed = false;
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            {
                ctrlPressed = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
            {
                ctrlPressed = false;
            }

            var pan = Input.GetMouseButton(2) && !shiftPressed && !ctrlPressed;
            var rotate = Input.GetMouseButton(0) && shiftPressed && !ctrlPressed;
            var orbit = Input.GetMouseButton(0) && !shiftPressed && !ctrlPressed;
            var zoom = Input.GetMouseButton(1) || (Input.GetMouseButton(0) && ctrlPressed);

            // This doesn't seem to work in Unity (mouse scroll)
            // var zoomWheel = Input.mouseScrollDelta.y > 0;

            bool mouseDown = (pan || rotate || orbit || zoom);

            Vector2 mousePos = Input.mousePosition;

            if (mouseDown && !mousePressed)
            {
                mousePositionAtStart = mousePos;
                mousePressed = true;

            }
            else if (!mouseDown && mousePressed)
            {
                mousePressed = false;
            }


            if (mousePressed)
            {
                rotationVelocity = Quaternion.identity;
                Vector2 mouseDelta = mousePos - mousePositionAtStart;
                Vector2 mouseDeltaInstant = new Vector2(
                    Input.GetAxis("Mouse X"),
                    Input.GetAxis("Mouse Y")
                );

                if (orbit)
                {
                    if (weightedControl)
                    {
                        rotationVelocity *= new Quaternion(-mouseDelta.y * rotationSpeed * Time.deltaTime, mouseDelta.x * rotationSpeed * Time.deltaTime, 0.0f, Time.deltaTime);
                    }
                    else
                    {
                        rotationVelocity = new Quaternion(-mouseDeltaInstant.y * rotationSpeed * Time.deltaTime * speedMult, mouseDeltaInstant.x * rotationSpeed * Time.deltaTime * speedMult, 0.0f, Time.deltaTime);
                    }
                }

                if (rotate)
                {
                    if (weightedControl)
                    {
                        rotationVelocity *= new Quaternion(0.0f, 0.0f, -mouseDelta.y * rotationSpeed * Time.deltaTime + mouseDelta.x * rotationSpeed * Time.deltaTime, Time.deltaTime);
                    }
                    else
                    {
                        rotationVelocity = new Quaternion(0.0f, 0.0f, -mouseDeltaInstant.y * rotationSpeed * Time.deltaTime * speedMult + mouseDeltaInstant.x * rotationSpeed * Time.deltaTime * speedMult, Time.deltaTime);
                    }
                }

                if (pan)
                {
                    if (weightedControl)
                    {
                        panVelocity += new Vector3(-mouseDelta.x * panSpeed, -mouseDelta.y * panSpeed, 0.0f) * Time.deltaTime;
                    }
                    else
                    {
                        panVelocity = new Vector3(-mouseDeltaInstant.x * panSpeed * speedMult * 20, -mouseDeltaInstant.y * panSpeed * speedMult * 20, 0.0f) * Time.deltaTime;
                    }
                }

                if (zoom)
                {
                    if (weightedControl)
                    {
                        panVelocity += new Vector3(0.0f, 0.0f, mouseDelta.y * zoomSpeed) * Time.deltaTime;
                    }
                    else
                    {
                        panVelocity = new Vector3(0.0f, 0.0f, mouseDeltaInstant.y * zoomSpeed * speedMult * 20) * Time.deltaTime;
                    }
                }
            }
            else
            {
                if (weightedControl)
                {
                    rotationVelocity = Quaternion.LerpUnclamped(rotationVelocity, Quaternion.identity, rotationSpeed * speedMult);
                    panVelocity = Vector3.LerpUnclamped(panVelocity, Vector3.zero, panSpeed * speedMult);
                }
                else
                {
                    rotationVelocity = Quaternion.identity;
                    panVelocity = Vector3.zero;
                }
            }
        }
    }
}