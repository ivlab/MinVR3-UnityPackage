
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IVLab.MinVR3
{

    /// <summary>
    /// This implements a user interface for controlling the camera with the mouse.
    /// It is a special interface inspired by the "Unicam" technique developed by
    /// Zeleznik et al.
    ///
    /// The key feature is that this interface makes it possible to control camera pan,
    /// dolly, and rotation with only a single mouse button.That is quite useful
    /// because it leaves the other mouse buttons free for pointing, sketching, or
    /// other interaction techniques.
    ///
    /// The only downside of this technique is that it can take some time to learn.  In
    /// order to enjoy it, you will need to read these brief instructions on how to Pan,
    /// Dolly, Rotate, and Spin:
    ///      
    /// - Pan: Click and drag horizontally with the mouse.Once you make an initial
    /// horizontal movement you can than pan up and down as well, but the key to entering
    /// pan mode is to start with a horizontal movement.
    /// 
    /// - Dolly: Click and drag vertically with the mouse.The initial movement must
    /// be vertical.  If you click on some object in the scene, then the speed of dollying
    /// is set so that the object will come all the up to the camera lens if you drag
    /// the mouse to the bottom of the screen.
    /// 
    /// - Rotate: Start with a quick click and release to set the center of rotation.
    /// This is most useful if you click on some object in the scene.  You will see a
    /// black dot appear to mark the center of rotation.If you click on the background
    /// then a center of rotation will be selected for you.It will be a point straight
    /// ahead and at a depth 4.0 units away.  The depth can be adjusted for your application
    /// with set_default_depth().  Once your center of rotation is established, move
    /// your mouse away a bit and then click and drag to do a trackball rotatation of
    /// the scene around this point.Come to a stop before letting go of the mouse
    /// button in order to avoid entering the spin state!
    /// 
    /// - Spin: For some fun, try "throwing" the scene so that it continues to rotate
    /// even after you let go.To do this, start a rotation and then let go of the
    /// mouse button while your mouse is still moving.  To stop spinning just click and
    /// release the mouse once to "catch" the scene.
    /// </summary>
    [AddComponentMenu("MinVR Interaction/Desktop/UniCam")]
    public class UniCam : MonoBehaviour, IVREventListener
    {

        public void Reset()
        {
            m_CursorDownEvent = VREventPrototype.Create("Mouse/Left/Down");
            m_CursorMoveEvent = VREventPrototypeVector2.Create("Mouse/Position");
            m_CursorUpEvent = VREventPrototype.Create("Mouse/Left/Up");
            m_DefaultDepth = 10.0f;
            m_DollyFactor = 1.0f;
        }

        public void Awake()
        {
            m_State = UniCamState.START;
            m_MouseDown = false;
            m_BoundingSphereRad = 1.0f;
            m_DollyInitialized = false;
            m_ElapsedTime = 0.0;
            m_HitGeometry = false;
            m_RotAngularVel = 0.0;
            m_RotInitialized = false;
            m_RotLastTime = 0.0;
            m_ShowIcon = false;
            m_RotAngularVelBuffer = new List<KeyValuePair<double, double>>();
        }

        void Start()
        {
            if (m_Camera == null) {
                m_Camera = Camera.main;
            }
        }

        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_CursorDownEvent)) {
                m_MouseDown = true;
                OnButtonDown();
            } else if (vrEvent.Matches(m_CursorMoveEvent)) {
                Vector2 newMousePosScreen = (vrEvent as VREventVector2).GetData();
                m_MousePosScreen = (vrEvent as VREventVector2).GetData();
                m_MousePosNDC = ScreenCoordsToNormalizedDeviceCoords(m_MousePosScreen);
                if (m_MouseDown) {
                    OnDrag();
                }
            } else if (vrEvent.Matches(m_CursorUpEvent)) {
                m_MouseDown = false;
                OnButtonUp();
            }
        }


        private void OnButtonDown()
        {
            Debug.Assert(m_DefaultDepth > 0);
            Debug.Assert(m_Camera != null);

            if (m_State == UniCamState.START) {
                m_InitialClickPos = m_MousePosNDC;
                m_MouseLastNDC = m_MousePosNDC;
                m_MouseLastScreen = m_MousePosScreen;
                m_ElapsedTime = 0.0;
                m_RotInitialized = false;
                m_DollyInitialized = false;

                Ray ray = m_Camera.ScreenPointToRay(new Vector3(m_MousePosScreen.x, m_MousePosScreen.y, 0), Camera.MonoOrStereoscopicEye.Mono);
                RaycastHit hit;
                m_HitGeometry = Physics.Raycast(ray, out hit);
                if (m_HitGeometry) {
                    m_HitPoint = hit.point;
                } else {
                    m_HitPoint = m_Camera.ScreenToWorldPoint(new Vector3(m_MousePosScreen.x, m_MousePosScreen.y, m_DefaultDepth), Camera.MonoOrStereoscopicEye.Mono);
                }
                m_ShowIcon = true;
                m_State = UniCamState.PAN_DOLLY_ROT_DECISION;
            } else if (m_State == UniCamState.ROT_WAIT_FOR_SECOND_CLICK) {
                // we have the second click now, and we will start the trackball rotate interaction
                m_State = UniCamState.ROT;
            } else if (m_State == UniCamState.SPINNING) {
                // this click is to "catch" the model, stopping it from spinning.
                m_State = UniCamState.START;
            } else {
                Debug.Log("UniCam::OnButtonDown() unexpected state.");
            }

        }


        private void OnDrag()
        {
            if (m_State == UniCamState.PAN_DOLLY_ROT_DECISION) {
                if (Mathf.Abs(m_MousePosNDC[0] - m_InitialClickPos[0]) > m_PanMovementThreshold) {
                    // already lots of horizontal movement, we can go right to pan
                    m_State = UniCamState.PAN;
                    m_ShowIcon = false;
                } else if (Mathf.Abs(m_MousePosNDC[1] - m_InitialClickPos[1]) > m_DollyMovementThreshold) {
                    // already lots of vertical movement, we can go right to dolly
                    m_State = UniCamState.DOLLY;
                    m_ShowIcon = false;
                } else if (m_ElapsedTime > 1.0) {
                    // timeout, this was not a quick click to set a center of rotation,
                    // so there is no intent to rotate.  instead we will be doing either
                    // pan or dolly.
                    m_State = UniCamState.PAN_DOLLY_DECISION;
                    m_ShowIcon = false;
                }

            } else if (m_State == UniCamState.PAN_DOLLY_DECISION) {
                if (Mathf.Abs(m_MousePosNDC[0] - m_InitialClickPos[0]) > m_PanMovementThreshold) {
                    // lots of horizontal movement, go to pan
                    m_State = UniCamState.PAN;
                } else if (Mathf.Abs(m_MousePosNDC[1] - m_InitialClickPos[1]) > m_DollyMovementThreshold) {
                    // lots of vertical movement, go to dolly
                    m_State = UniCamState.DOLLY;
                }

            } else if (m_State == UniCamState.PAN) {
                // Translate parallel to the filmplane
                Vector3 eye = m_Camera.transform.position;
                Vector3 look = m_Camera.transform.forward;
                float depth = Vector3.Dot(m_HitPoint - eye, look);
                Vector3 pWorld1 = m_Camera.ScreenToWorldPoint(new Vector3(m_MouseLastScreen.x, m_MouseLastScreen.y, depth));
                Vector3 pWorld2 = m_Camera.ScreenToWorldPoint(new Vector3(m_MousePosScreen.x, m_MousePosScreen.y, depth));
                m_Camera.transform.TranslateByWorldVector(pWorld1 - pWorld2);
                m_MouseLastScreen = m_MousePosScreen;
                m_MouseLastNDC = m_MousePosNDC;

            } else if (m_State == UniCamState.DOLLY) {
                Vector3 look = m_Camera.transform.forward;
                if (!m_DollyInitialized) {
                    // Setup dollyFactor so that if you move the mouse to the bottom of the screen, the point
                    // you clicked on will be right on top of the camera.
                    Vector3 eye = m_Camera.transform.position;
                    float depth = Vector3.Dot(m_HitPoint - eye, look);
                    float deltaYToBottom = m_InitialClickPos[1] + 1;
                    m_DollyFactor = depth / deltaYToBottom;
                    m_DollyInitialized = true;
                }
                float d = -m_DollyFactor * (m_MousePosNDC[1] - m_MouseLastNDC[1]);
                m_Camera.transform.TranslateByWorldVector(d * look);
                m_MouseLastScreen = m_MousePosScreen;
                m_MouseLastNDC = m_MousePosNDC;

            } else if (m_State == UniCamState.ROT) {
                if (!m_RotInitialized) {
                    float depth;
                    Vector2 centerScreenPt = NormalizedDeviceCoordsToScreenCoords(Vector2.zero);
                    if (m_HitGeometry) {
                        // if we hit some geometry, then make that the center of rotation
                        m_BoundingSphereCtr = m_HitPoint;
                        Vector3 eye = m_Camera.transform.position;
                        Vector3 look = m_Camera.transform.forward;
                        depth = Vector3.Dot(m_HitPoint - eye, look);
                    } else {
                        // if we did not hit any geometry, then set the center of rotation at a distance that can be
                        // configured by the user.

                        // uncomment to center the bounding sphere on the screen rather than under the mouse
                        //m_HitPoint = m_Camera.ScreenToWorldPoint(new Vector3(centerScreenPt.x, centerScreenPt.y, m_DefaultDepth));

                        m_BoundingSphereCtr = m_HitPoint;
                        depth = m_DefaultDepth;
                    }
                    
                    // determine the size of the bounding sphere by projecting a NDC screen-space
                    // distance of 0.5 units to the depth of the sphere center
                    Vector2 offsetInScreenCoords = NormalizedDeviceCoordsToScreenCoords(new Vector2(0, 0.5f));
                    Vector2 offsetScreenPt = centerScreenPt + offsetInScreenCoords;
                    Vector3 pWorld1 = m_Camera.ScreenToWorldPoint(new Vector3(centerScreenPt.x, centerScreenPt.y, depth));
                    Vector3 pWorld2 = m_Camera.ScreenToWorldPoint(new Vector3(offsetScreenPt.x, offsetScreenPt.y, depth));
                    m_BoundingSphereRad = (pWorld2 - pWorld1).magnitude;

                    m_MouseLastScreen = m_MousePosScreen;
                    m_MouseLastNDC = m_MousePosNDC;
                    m_RotLastTime = m_ElapsedTime;
                    m_RotAngularVelBuffer.Clear();
                    m_RotInitialized = true;
                } else {
                    // Do a trackball rotation based on the mouse movement and the bounding sphere
                    // setup earlier.

                    // last mouse pos
                    Ray ray1 = m_Camera.ScreenPointToRay(new Vector3(m_MouseLastScreen.x, m_MouseLastScreen.y, m_Camera.nearClipPlane));
                    float t1;
                    Vector3 iPoint1;
                    bool hit1 = RayIntersectSphere(ray1, m_BoundingSphereCtr, m_BoundingSphereRad, out t1, out iPoint1);

                    // current mouse pos
                    Ray ray2 = m_Camera.ScreenPointToRay(new Vector3(m_MousePosScreen.x, m_MousePosScreen.y, m_Camera.nearClipPlane));
                    float t2;
                    Vector3 iPoint2;
                    bool hit2 = RayIntersectSphere(ray2, m_BoundingSphereCtr, m_BoundingSphereRad, out t2, out iPoint2);
                    m_RotLastIPoint = iPoint2;

                    if (hit1 && hit2) {
                        Vector3 v1 = (iPoint1 - m_BoundingSphereCtr).normalized;
                        Vector3 v2 = (iPoint2 - m_BoundingSphereCtr).normalized;

                        m_RotAxis = Vector3.Cross(v1, v2).normalized;
                        float angle = Mathf.Acos(Vector3.Dot(v1, v2));

                        if ((!float.IsNaN(angle)) && (!float.IsInfinity(angle))) {
                            Quaternion R = Quaternion.AngleAxis(angle, m_RotAxis);
                            m_Camera.transform.RotateAroundWorldPoint(m_BoundingSphereCtr, R);

                            // add a sample to the angular vel vector
                            double dt = m_ElapsedTime - m_RotLastTime;
                            double avel = angle / dt;
                            if ((!double.IsNaN(avel)) && (!double.IsInfinity(avel))) {
                                m_RotAngularVelBuffer.Add(new KeyValuePair<double, double>(m_ElapsedTime, avel));
                            }
                            m_RotLastTime = m_ElapsedTime;
                        }
                    }

                    RecalcAngularVel();
                    m_MouseLastScreen = m_MousePosScreen;
                    m_MouseLastNDC = m_MousePosNDC;
                }

            } else if (m_State == UniCamState.START) {
                // picked up a little mouse movement after "catching" a spinning model
                // nothing to do, just wait for the button up.

            } else {
                Debug.Log("UniCam::OnDrag() unexpected state.");
            }
        }

        private void OnButtonUp()
        {
            if (m_State == UniCamState.PAN_DOLLY_ROT_DECISION) {
                // here, we got a quick click of the mouse to indicate a center of rotation
                // so we now go into a mode of waiting for a second click to start rotating
                // around that point.
                m_State = UniCamState.ROT_WAIT_FOR_SECOND_CLICK;
            } else if (m_State == UniCamState.ROT) {
                m_ShowIcon = false;
                // if we are leaving the rotation state and the angular velocity is
                // greater than some thresold, then the user has "thrown" the model
                // keep rotating the same way by entering the spinning state.

                RecalcAngularVel();
                //Debug.Log("check for spin: " + n-start + " " + rotAngularVel_ + " " + avel2);

                if (Mathf.Abs((float)m_RotAngularVel) > m_SpinAngluarVelThreshold) {
                    m_State = UniCamState.SPINNING;
                } else {
                    m_State = UniCamState.START;
                }
            } else {
                m_ShowIcon = false;
                // all other cases go back to the start state
                m_State = UniCamState.START;
            }
        }



        void RecalcAngularVel()
        {
            float cutoff = (float)m_ElapsedTime - m_AngluarVelTimeWindow; // look just at the last 0.2 secs
            List<KeyValuePair<double, double>> tmp = m_RotAngularVelBuffer;
            m_RotAngularVelBuffer = new List<KeyValuePair<double, double>>();
            for (int i = 0; i < tmp.Count; i++) {
                if (tmp[i].Key >= cutoff) {
                    m_RotAngularVelBuffer.Add(tmp[i]);
                }
            }
            m_RotAngularVel = 0.0;
            for (int i = 0; i < m_RotAngularVelBuffer.Count; i++) {
                m_RotAngularVel += m_RotAngularVelBuffer[i].Value;
            }
            m_RotAngularVel /= m_RotAngularVelBuffer.Count;
        }

        /// <summary>
        /// Normalized Device Coords have (0,0) at the center of the screen, (-1,-1) in the bottom left,
        /// and (1,1) in the top right.
        /// </summary>
        /// <param name="screenPt"></param>
        /// <returns></returns>
        public Vector2 ScreenCoordsToNormalizedDeviceCoords(Vector2 screenPt) {
            Vector2 viewportPt = m_Camera.ScreenToViewportPoint(screenPt);
            Vector2 ndcPt = 2.0f * viewportPt + new Vector2(-1, -1);
            return ndcPt;
        }

        public Vector2 NormalizedDeviceCoordsToScreenCoords(Vector2 ndcPt) {
            Vector2 viewportPt = 0.5f * ndcPt + new Vector2(0.5f, 0.5f);
            Vector2 screenPt = m_Camera.ViewportToScreenPoint(viewportPt);
            return screenPt;
        }


        public bool RayIntersectSphere(Ray ray,Vector3 center, float radius,
                                       out float iTime, out Vector3 iPoint) {
            iTime = 0;
            iPoint = ray.origin;

            Vector3 P = ray.origin + (Vector3.zero - center);
            Vector3 D = ray.direction;

            // A = (Dx^2 + Dy^2 + Dz^2)
            double A = ((double)D[0] * D[0] + (double)D[1] * D[1] + (double)D[2] * D[2]);

            // B = (Px * Dx + Py * Dy + Pz * Dz)
            double B = ((double)P[0] * D[0] + (double)P[1] * D[1] + (double)P[2] * D[2]);

            // C = (Px^2 + Py^2 + Pz^2 - (sphere radius)^2)
            double C = ((double)P[0] * P[0] + (double)P[1] * P[1] + (double)P[2] * P[2] - (double)radius * radius);

            // Discriminant of quadratic = B^2 - A * C
            double discriminant = B * B - A * C;

            if (discriminant < 0.0) {
                return false;
            } else {
                double discRoot = Mathf.Sqrt((float)discriminant);
                double t1 = (-B - discRoot) / A;
                double t2 = (-B + discRoot) / A;
                bool hit1 = false;
                bool hit2 = false;
                if (t1 > 0.000001) {
                    hit1 = true;
                    iTime = (float)t1;
                }
                if (t2 > 0.000001) {
                    hit2 = true;
                    iTime = (float)t2;
                }
                if ((!hit1) && (!hit2)) {
                    return false;
                }
                if ((hit1) && (hit2)) {
                    if (t1 < t2) {
                        iTime = (float)t1;
                    }
                }

                iPoint = ray.origin + iTime * ray.direction;
                return true;
            }
        }


        void Update() {
            m_ElapsedTime += Time.deltaTime;

            if (m_State == UniCamState.SPINNING) {
                double deltaT = m_ElapsedTime - m_RotLastTime;
                m_RotLastTime = m_ElapsedTime;
                double angle = m_RotAngularVel * deltaT;

                Quaternion R = Quaternion.AngleAxis((float)angle, m_RotAxis);
                m_Camera.transform.RotateAroundWorldPoint(m_BoundingSphereCtr, R);
            }


            if (m_ShowIcon) {
                Vector3 eye = m_Camera.transform.position;
                Vector3 look = m_Camera.transform.forward;
                float depth = Vector3.Dot(m_HitPoint - eye, look);
                Vector2 pScreen1 = NormalizedDeviceCoordsToScreenCoords(Vector2.zero);
                Vector3 pWorld1 = m_Camera.ScreenToWorldPoint(new Vector3(pScreen1.x, pScreen1.y, depth));
                Vector2 pScreen2 = NormalizedDeviceCoordsToScreenCoords(new Vector2(0.015f, 0));
                Vector3 pWorld2 = m_Camera.ScreenToWorldPoint(new Vector3(pScreen2.x, pScreen2.y, depth));
                float rad = (pWorld2 - pWorld1).magnitude;
                DebugDraw.Sphere(m_HitPoint, rad, Color.black);
            }
        }

        void OnEnable()
        {
            StartListening();
        }

        void OnDisable()
        {
            StopListening();
        }

        public void StartListening()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        public void StopListening()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }



        [Tooltip("The camera to control. If not set, defaults to Camera.main on Start().")]
        [SerializeField] private Camera m_Camera;

        [Tooltip("Event to start the interaction, typically the 'Down' event of one of the mouse buttons.")]
        [SerializeField] private VREventPrototype m_CursorDownEvent;
        [Tooltip("Event to stop the interaction, typically the 'Up' event of the same mouse button.")]
        [SerializeField] private VREventPrototype m_CursorUpEvent;
        [Tooltip("Event to update the position of the cursor, typically the 'Position' event of the mouse.")]
        [SerializeField] private VREventPrototypeVector2 m_CursorMoveEvent;

        [Tooltip("Pan, Dolly, and Rotate all work best when the user clicks on some geometry in the scene because " +
            "they can then map mouse movement to a particular depth from the camera.  Whenever the mouse is clicked " +
            "on an object that has a collider (so responds to Physics.Raycast), the depth of that intersection point " +
            "will be used to drive the Pan, Dolly, and Rotate actions.  However, when the user begins an operation " +
            "by clicking on the background or an object that does not have a collider, this default depth value is " +
            "used for the depth.  The interaction will work best if this is set on an per-application basis so that " +
            "the geometry you are rendering is located about this depth from the camera.")]
        [SerializeField] private float m_DefaultDepth;

        [Header("Advanced")]
        [Tooltip("The horizontal distance (in Normalized Device Coordinates) the mouse must move after the initial " +
            "click in order to kick into pan mode.")]
        [SerializeField] private float m_PanMovementThreshold = 0.01f;
        [Tooltip("The vertical distance (in Normalized Device Coordinates) the mouse must move after the initial " +
            "click in order to kick into dolly mode.")]
        [SerializeField] private float m_DollyMovementThreshold = 0.01f;


        [Tooltip("The time window (in sec) for calculating the current angular velocity by averaging the velocities " +
            "within this time window.")]
        [SerializeField] private float m_AngluarVelTimeWindow = 0.2f;

        [Tooltip("The threshold the current angular velocity (radians/sec) must exceed at the end of a rotation in " +
            "order to enter spin mode, where the camera contines to rotate on its own until stopped with a second click.")]
        [SerializeField] private float m_SpinAngluarVelThreshold = 0.2f;

        // runtime only
        private enum UniCamState
        {
            START,
            PAN_DOLLY_ROT_DECISION,
            PAN_DOLLY_DECISION,
            ROT_WAIT_FOR_SECOND_CLICK,
            PAN,
            DOLLY,
            ROT,
            SPINNING
        };
        UniCamState m_State;

        private Vector2 m_MousePosScreen;
        private Vector2 m_MouseLastScreen;
        private Vector2 m_MousePosNDC;
        private Vector2 m_MouseLastNDC;

        private bool m_MouseDown;
        private double m_ElapsedTime;

        private Vector2 m_InitialClickPos;
        private bool m_HitGeometry;
        private Vector3 m_HitPoint;

        private bool m_RotInitialized;
        private Vector3 m_RotLastIPoint;
        private Vector3 m_BoundingSphereCtr;
        private float m_BoundingSphereRad;
        private double m_RotLastTime;
        private List<KeyValuePair<double, double>> m_RotAngularVelBuffer;
        private double m_RotAngularVel;
        private Vector3 m_RotAxis;

        private bool m_DollyInitialized;
        private float m_DollyFactor;

        private bool m_ShowIcon;
    }
    
} // end namespace