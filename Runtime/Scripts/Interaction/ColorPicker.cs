using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IVLab.MinVR3
{
    /**
     * TODO list:
     * - use material (e.g., like from skyvr) that includes an alpha fade parameter
     * - fade in and fade out on activate/deactivate
     * - fade to 50% when outside bounding sphere for the 'cancel' operation
     */
    [AddComponentMenu("MinVR Interaction/Widgets/Color Picker (CavePainting Style)")]
    public class ColorPicker : MonoBehaviour, IVREventListener, IVREventProducer
    {
        public Color initialColor {
            get { return m_InitialColor; }
            set { m_InitialColor = value; }
        }

        void Reset()
        {
            m_CursorDownEvent = null;
            m_CursorPosEvent = null;
            m_CursorUpEvent = null;

            m_RequireToken = null;

            m_Radius = 0.4f;
            m_CancelRadius = 0.5f;
            m_CursorRadius = 0.1f;
            m_ColoredPointsRadius = 0.05f;
            m_LineWidth = 0.005f;
            m_NumPointsOnColorWheel = 12;
        }


        void Start()
        {
            m_InitialColor = new Color(0.75f, 0.75f, 0.75f);
 
            GameObject topSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            topSphere.transform.SetParent(this.transform, false);
            topSphere.transform.localPosition = m_Radius * Vector3.up;
            topSphere.transform.localScale = m_ColoredPointsRadius * Vector3.one;
            Material topMat = topSphere.GetComponent<MeshRenderer>().material;
            topMat.color = Color.white;
            topSphere.GetComponent<MeshRenderer>().sharedMaterial = topMat;

            GameObject botSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            botSphere.transform.SetParent(this.transform, false);
            botSphere.transform.localPosition = m_Radius * -Vector3.up;
            botSphere.transform.localScale = m_ColoredPointsRadius * Vector3.one;
            Material botMat = botSphere.GetComponent<MeshRenderer>().material;
            botMat.color = Color.black;
            botSphere.GetComponent<MeshRenderer>().sharedMaterial = botMat;

            //Material lineMat = new Material(Shader.Find("Unlit/Color"));
            Material lineMat = topMat;


            float aInc = 360.0f / (float)m_NumPointsOnColorWheel;
            for (float a = 0; a < 360; a += aInc) {
                Vector3 p = new Vector3(m_Radius * Mathf.Cos(Mathf.Deg2Rad * a), 0, m_Radius * Mathf.Sin(Mathf.Deg2Rad * a));

                GameObject colSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                colSphere.transform.SetParent(this.transform, false);
                colSphere.transform.localPosition = p;
                colSphere.transform.localScale = m_ColoredPointsRadius * Vector3.one;
                Material colMat = colSphere.GetComponent<MeshRenderer>().material;
                colMat.color = PointToColor(p);
                colSphere.GetComponent<MeshRenderer>().sharedMaterial = colMat;

                GameObject colLine = new GameObject("Line", typeof(LineRenderer));
                colLine.transform.SetParent(this.transform, false);
                LineRenderer l = colLine.GetComponent<LineRenderer>();
                l.sharedMaterial = lineMat;
                List<Vector3> points = new List<Vector3>();
                points.Add(m_Radius * Vector3.up);
                points.Add(p);
                points.Add(m_Radius * -Vector3.up);
                l.startWidth = m_LineWidth;
                l.endWidth = m_LineWidth;
                l.positionCount = points.Count;
                l.SetPositions(points.ToArray());
                l.useWorldSpace = false;
                
            }

            m_CursorSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_CursorSphere.transform.SetParent(this.transform, false);
            m_CursorSphere.transform.localScale = m_CursorRadius * Vector3.one;
            m_CursorSphereMat = m_CursorSphere.GetComponent<MeshRenderer>().material;
            m_CursorSphere.GetComponent<MeshRenderer>().sharedMaterial = m_CursorSphereMat;

            // turn all of the child geometry off until the widget is activated
            for (int i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        void OnCursorDown()
        {
            if ((m_RequireToken == null) || (m_RequireToken.RequestToken(this))) {
                m_Active = true;

                // reset (zero out) colorpicker transform and get the cursor pos in the local space
                transform.localPosition = Vector3.zero;
                Vector3 cursorPosLocal = transform.RoomPointToLocalSpace(m_CursorPosRoom);

                // set the transform to place the origin where the cursor is offset by where the cursor
                // should be relative to the color picker origin given the initial color
                transform.localPosition = cursorPosLocal - ColorToPoint(m_InitialColor);

                m_CursorSphere.transform.localPosition = ColorToPoint(m_InitialColor);
                m_CursorSphereMat.color = m_InitialColor;

                for (int i = 0; i < transform.childCount; i++) {
                    transform.GetChild(i).gameObject.SetActive(true);
                }
            }
        }

        void OnCursorMove()
        {
            if (m_Active) {
                Vector3 cursorPosLocal = transform.RoomPointToLocalSpace(m_CursorPosRoom);

                m_CursorSphere.transform.localPosition = cursorPosLocal;
                Vector4 data;
                if (IsOutsideCancelSphere()) {
                    data = new Vector4(m_InitialColor[0], m_InitialColor[1], m_InitialColor[2], m_InitialColor[3]);
                    m_CursorSphereMat.color = m_InitialColor;
                } else {
                    Color col = PointToColor(cursorPosLocal);
                    data = new Vector4(col[0], col[1], col[2], col[3]);
                    m_CursorSphereMat.color = col;
                }
                VREngine.instance.eventManager.InsertInQueue(new VREventVector4("ColorPicker/ColorModified", data));   
            }
        }

        void OnCursorUp()
        {
            if (m_Active) {
                Vector3 cursorPosLocal = transform.RoomPointToLocalSpace(m_CursorPosRoom);

                Vector4 data;
                if (IsOutsideCancelSphere()) {
                    data = new Vector4(m_InitialColor[0], m_InitialColor[1], m_InitialColor[2], m_InitialColor[3]);
                    VREngine.instance.eventManager.InsertInQueue(new VREventVector4("ColorPicker/ColorCancelled", data));
                } else {
                    Color col = PointToColor(cursorPosLocal);
                    data = new Vector4(col[0], col[1], col[2], col[3]);
                    VREngine.instance.eventManager.InsertInQueue(new VREventVector4("ColorPicker/ColorSelected", data));
                    m_InitialColor = col;
                }
                m_RequireToken?.ReleaseToken(this);
                m_Active = false;
                for (int i = 0; i < transform.childCount; i++) {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }


        /// p is relative to the center of the color picker
        public Color PointToColor(Vector3 p)
        {
            // Clamp offset vec so it doesn't go outside of the color space  
            Vector3 offset = p;
            float l1 = Mathf.Abs(offset[1]);
            float l2 = Mathf.Sqrt(offset[0] * offset[0] + offset[2] * offset[2]);
            float a = Mathf.Atan2(l1, l2);
            float b = 2.3561945f - a;
            float d = m_Radius * 0.70710678f / Mathf.Sin(b);
            if (offset.magnitude > d) {
                offset = offset.normalized * d;
                p = offset;
            }

            //  Determine HLS values
            double h, l, s;
            l = (offset[1] + m_Radius) / (2.0 * m_Radius);
            Vector3 v = (new Vector3(offset[0], 0, offset[2])).normalized;
            float dot = Vector3.Dot(v, new Vector3(1, 0, 0));
            if (offset[2] > 0)
                h = Mathf.Rad2Deg * Mathf.Acos(dot);
            else
                h = 360 - Mathf.Rad2Deg * Mathf.Acos(dot);

            if (offset[1] > m_Radius)
                s = 0.0;
            else if (offset[1] < -m_Radius)
                s = 1.0;
            else
                s = Mathf.Sqrt(offset[0] * offset[0] + offset[2] * offset[2]) /
                  (m_Radius - Mathf.Abs(offset[1]));
            if (s > 1.0)
                s = 1.0;

            // Convert HLS to RGB
            double m1, m2;
            m2 = (l <= 0.5) ? (l * (1.0 + s)) : (l + s - l * s);
            m1 = 2.0 * l - m2;

            Color color;
            if (s == 0)
                color = new Color((float)l, (float)l, (float)l);
            else {
                color = new Color((float)FindValue(m1, m2, h + 120.0),
                                  (float)FindValue(m1, m2, h),
                                  (float)FindValue(m1, m2, h - 120.0f));
            }
            return color;
        }

        /// c is returned in a coordinate space relative to the center of the widget
        public Vector3 ColorToPoint(Color c)
        {
            // Convert RGB to HLS
            float h, l, s;
            float maximum = Mathf.Max(Mathf.Max(c[0], c[1]), c[2]);
            float minimum = Mathf.Min(Mathf.Min(c[0], c[1]), c[2]);
            l = (maximum + minimum) / 2;
            if (maximum == minimum) {
                s = 0;
                h = 0;
            } else {
                if (l <= 0.5)
                    s = (maximum - minimum) / (maximum + minimum);
                else
                    s = (maximum - minimum) / (2.0f - maximum - minimum);
                float delta = maximum - minimum;

                if (c[0] == maximum)
                    h = (c[1] - c[2]) / delta;
                else if (c[1] == maximum)
                    h = 2.0f + (c[2] - c[0]) / delta;
                else //if (c[2] == maximum)
                    h = 4 + (c[0] - c[1]) / delta;
                h *= 60.0f;
                if (h < 0.0f)
                    h += 360.0f;
            }

            // Convert HLS to a point in the color widget space
            double a = Mathf.Deg2Rad * h;
            double maxr;
            if (l < 0.5)
                maxr = m_Radius * l * 2.0;
            else
                maxr = m_Radius * (1.0 - ((l - 0.5) * 2.0));
            Vector3 p = new Vector3();
            p[0] = (float)(maxr * s * Mathf.Cos((float)a));
            p[1] = l * 2.0f * m_Radius - m_Radius;
            p[2] = (float)(maxr * s * Mathf.Sin((float)a));

            return p;
        }

        double FindValue(double n1, double n2, double hue)
        {
            if (hue > 360.0)
                hue -= 360.0;
            else if (hue < 0)
                hue += 360.0;

            if (hue < 60)
                return n1 + (n2 - n1) * hue / 60.0;
            else if (hue < 180.0)
                return n2;
            else if (hue < 240.0)
                return n1 + (n2 - n1) * (240.0 - hue) / 60.0;
            else
                return n1;
        }


        private bool IsOutsideCancelSphere()
        {
            Vector3 cursorPosLocal = transform.RoomPointToLocalSpace(m_CursorPosRoom);
            return cursorPosLocal.magnitude > m_CancelRadius;
        }


        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_CursorDownEvent)) {
                OnCursorDown();
            } else if (vrEvent.Matches(m_CursorUpEvent)) {
                OnCursorUp();
            } else if (vrEvent.Matches(m_CursorPosEvent)) {
                m_CursorPosRoom = (vrEvent as VREventVector3).GetData();
                m_CursorPosWorld = VREngine.instance.roomSpaceOrigin.RoomPointToWorldSpace(m_CursorPosRoom);
                OnCursorMove();
            }
        }

        private void OnEnable()
        {
            StartListening();
        }

        private void OnDisable()
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

        public List<IVREventPrototype> GetEventPrototypes()
        {
            List<IVREventPrototype> eventPrototypes = new List<IVREventPrototype>();
            eventPrototypes.Add(VREventPrototypeVector4.Create("ColorPicker/ColorModified"));
            eventPrototypes.Add(VREventPrototypeVector4.Create("ColorPicker/ColorSelected"));
            eventPrototypes.Add(VREventPrototypeVector4.Create("ColorPicker/ColorCancelled"));
            return eventPrototypes;
        }

        [SerializeField] private VREventPrototype m_CursorDownEvent;
        [SerializeField] private VREventPrototypeVector3 m_CursorPosEvent;
        [SerializeField] private VREventPrototype m_CursorUpEvent;

        [SerializeField] private SharedToken m_RequireToken;

        [SerializeField] private float m_Radius;
        [SerializeField] private float m_CancelRadius;
        [SerializeField] private float m_CursorRadius;
        [SerializeField] private float m_ColoredPointsRadius;
        [SerializeField] private float m_LineWidth;
        [SerializeField] private int m_NumPointsOnColorWheel;


        // runtime
        bool m_Active;
        Color m_InitialColor;
        Vector3 m_CursorPosRoom;
        Vector3 m_CursorPosWorld;
        Vector3 m_WidgetCenter;
        GameObject m_CursorSphere;
        Material m_CursorSphereMat;
    }

}
