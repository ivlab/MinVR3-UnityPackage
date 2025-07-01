using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace IVLab.MinVR3
{
    [AddComponentMenu("MinVR Interaction/Widgets/Color Picker (CavePainting Style)")]
    public class ColorPicker : MonoBehaviour, IVREventListener, IVREventProducer
    {
        public Color initialColor
        {
            get { return m_InitialColor; }
            set { m_InitialColor = value; }
        }

        public Color currentColor
        {
            get { return m_CurrentColor; }
        }

        void Reset()
        {
            m_CursorDownEvent = null;
            m_CursorPosEvent = null;
            m_CursorUpEvent = null;

            m_RequireToken = null;

            m_ColorModifiedEventName = "ColorPicker/ColorModified";
            m_ColorSelectedEventName = "ColorPicker/ColorSelected";
            m_ColorCancelledEventName = "ColorPicker/ColorCancelled";

#if UNITY_EDITOR
            if (GraphicsSettings.defaultRenderPipeline == null){
                // Using built-in pipeline
                m_BaseMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
            }
            else{
                m_BaseMaterial = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.defaultMaterial;
            }
#endif
            m_Radius = 0.4f;
            m_CancelRadius = 0.5f;
            m_CancelModeAlpha = 0.25f;
            m_CursorRadius = 0.1f;
            m_ColoredPointsRadius = 0.05f;
            m_LineWidth = 0.005f;
            m_NumPointsOnColorWheel = 12;
            m_FadeDuration = 0.5f;
        }


        void Start()
        {
            if (GraphicsSettings.defaultRenderPipeline == null){
                // Using built-in pipeline
                m_ColorPropertyName = "_Color";
            }
            else{
                m_ColorPropertyName = "_BaseColor";
            }

            m_InitialColor = new Color(0.75f, 0.75f, 0.75f);
            m_CurrentColor = new Color(0.75f, 0.75f, 0.75f);

            m_AllRenderers = new List<Renderer>();

            GameObject topSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            topSphere.transform.SetParent(this.transform, false);
            topSphere.transform.localPosition = m_Radius * Vector3.up;
            topSphere.transform.localScale = m_ColoredPointsRadius * Vector3.one;
            Material topMaterial = new Material(m_BaseMaterial); // Creating new materials is better in URP than using material property blocks because the SRP Batcher works at the shader level
            topMaterial.SetColor(m_ColorPropertyName, Color.white);
            topSphere.GetComponent<MeshRenderer>().sharedMaterial = topMaterial;
            m_AllRenderers.Add(topSphere.GetComponent<MeshRenderer>());


            GameObject botSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            botSphere.transform.SetParent(this.transform, false);
            botSphere.transform.localPosition = m_Radius * -Vector3.up;
            botSphere.transform.localScale = m_ColoredPointsRadius * Vector3.one;
            Material botMaterial = new Material(m_BaseMaterial);
            botMaterial.SetColor(m_ColorPropertyName, Color.black);
            botSphere.GetComponent<MeshRenderer>().material = botMaterial;
            m_AllRenderers.Add(botSphere.GetComponent<MeshRenderer>());


            Material lineMaterial = new Material(m_BaseMaterial);
            lineMaterial.SetColor(m_ColorPropertyName, Color.gray);

            float aInc = 360.0f / (float)m_NumPointsOnColorWheel;
            for (float a = 0; a < 360; a += aInc)
            {
                Vector3 p = new Vector3(m_Radius * Mathf.Cos(Mathf.Deg2Rad * a), 0, m_Radius * Mathf.Sin(Mathf.Deg2Rad * a));

                GameObject colSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                colSphere.transform.SetParent(this.transform, false);
                colSphere.transform.localPosition = p;
                colSphere.transform.localScale = m_ColoredPointsRadius * Vector3.one;
                Material colMaterial = new Material(m_BaseMaterial);
                colMaterial.SetColor(m_ColorPropertyName, PointToColor(p));
                colSphere.GetComponent<MeshRenderer>().sharedMaterial = colMaterial;
                m_AllRenderers.Add(colSphere.GetComponent<MeshRenderer>());

                GameObject colLine = new GameObject("Line", typeof(LineRenderer));
                colLine.transform.SetParent(this.transform, false);
                LineRenderer l = colLine.GetComponent<LineRenderer>();
                l.sharedMaterial = lineMaterial;
                m_AllRenderers.Add(l.GetComponent<LineRenderer>());


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
            m_CursorMaterial = new Material(m_BaseMaterial);
            m_CursorMaterial.SetColor(m_ColorPropertyName, Color.white);
            m_CursorSphere.GetComponent<MeshRenderer>().sharedMaterial = m_CursorMaterial;
            m_AllRenderers.Add(m_CursorSphere.GetComponent<MeshRenderer>());

            // turn all of the child geometry off until the widget is activated
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }

            m_Active = false;
            m_InCancelState = false;
        }

        void OnCursorDown()
        {
            if (((m_RequireCondition == null) || (m_RequireCondition.isTrue)) &&
                ((m_RequireToken == null) || (m_RequireToken.RequestToken(this))))
            {
                m_Active = true;
                m_InCancelState = false;

                // reset (zero out) colorpicker transform and get the cursor pos in the local space
                transform.localPosition = Vector3.zero;
                Vector3 cursorPosLocal = transform.RoomPointToLocalSpace(m_CursorPosRoom);

                // set the transform to place the origin where the cursor is offset by where the cursor
                // should be relative to the color picker origin given the initial color
                transform.localPosition = cursorPosLocal - ColorToPoint(m_InitialColor);

                m_CursorSphere.transform.localPosition = ColorToPoint(m_InitialColor);
                m_CurrentColor = m_InitialColor;
                m_CursorMaterial.SetColor(m_ColorPropertyName, m_InitialColor);

                StartCoroutine(FadeIn(m_FadeDuration));
            }
        }

        void OnCursorMove()
        {
            if (m_Active)
            {
                Vector3 cursorPosLocal = transform.RoomPointToLocalSpace(m_CursorPosRoom);

                m_CursorSphere.transform.localPosition = cursorPosLocal;
                Vector4 data;
                if (IsOutsideCancelSphere())
                {
                    if (!m_InCancelState)
                    {
                        m_InCancelState = true;
                        SetColorPickerAlpha(m_CancelModeAlpha);
                    }
                    data = new Vector4(m_InitialColor[0], m_InitialColor[1], m_InitialColor[2], m_InitialColor[3]);
                    m_CurrentColor = m_InitialColor;
                }
                else
                {
                    if (m_InCancelState)
                    {
                        m_InCancelState = false;
                        SetColorPickerAlpha(1.0f);
                    }
                    Color col = PointToColor(cursorPosLocal);
                    data = new Vector4(col[0], col[1], col[2], col[3]);
                    m_CurrentColor = col;
                }
                m_CursorMaterial.SetColor(m_ColorPropertyName, m_CurrentColor);
                VREngine.instance.eventManager.InsertInQueue(new VREventVector4(m_ColorModifiedEventName, data));
            }
        }

        void OnCursorUp()
        {
            if (m_Active)
            {
                Vector3 cursorPosLocal = transform.RoomPointToLocalSpace(m_CursorPosRoom);

                Vector4 data;
                if (m_InCancelState)
                {
                    data = new Vector4(m_InitialColor[0], m_InitialColor[1], m_InitialColor[2], m_InitialColor[3]);
                    VREngine.instance.eventManager.InsertInQueue(new VREventVector4(m_ColorCancelledEventName, data));
                }
                else
                {
                    Color col = m_CurrentColor;
                    data = new Vector4(col[0], col[1], col[2], col[3]);
                    VREngine.instance.eventManager.InsertInQueue(new VREventVector4(m_ColorSelectedEventName, data));
                    m_InitialColor = col;
                }
                m_RequireToken?.ReleaseToken(this);
                m_Active = false;

                StartCoroutine(FadeOut(m_FadeDuration));
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
            if (offset.magnitude > d)
            {
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
            else
            {
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
            if (maximum == minimum)
            {
                s = 0;
                h = 0;
            }
            else
            {
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
            if (vrEvent.Matches(m_CursorDownEvent))
            {
                OnCursorDown();
            }
            else if (vrEvent.Matches(m_CursorUpEvent))
            {
                OnCursorUp();
            }
            else if (vrEvent.Matches(m_CursorPosEvent))
            {
                m_CursorPosRoom = (vrEvent as VREventVector3).GetData();
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
            eventPrototypes.Add(VREventPrototypeVector4.Create(m_ColorModifiedEventName));
            eventPrototypes.Add(VREventPrototypeVector4.Create(m_ColorSelectedEventName));
            eventPrototypes.Add(VREventPrototypeVector4.Create(m_ColorCancelledEventName));
            return eventPrototypes;
        }

        private IEnumerator FadeIn(float duration)
        {
            float elapsedTime = 0f;

            // activate children
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }

            while (elapsedTime < duration)
            {
                float elapsedFrac = elapsedTime / duration;
                float alpha = Mathf.Clamp(elapsedFrac, 0.0f, 1.0f);
                SetColorPickerAlpha(alpha);
                yield return null;
                elapsedTime += Time.deltaTime;
            }

            // fade-in complete, make double-sure that the last call sets alpha = 1 and rendering mode to opaque
            SetColorPickerAlpha(1.0f);
        }


        private IEnumerator FadeOut(float duration)
        {
            float elapsedTime = 0f;

            // special case: if in cancel state, the widget is already partially transparent, treat this as if
            // the fade out is already partially complete.
            if (m_InCancelState)
            {
                elapsedTime += (1.0f - m_CancelModeAlpha) * duration;
            }

            while (elapsedTime < duration)
            {
                float elapsedFrac = elapsedTime / duration;
                float alpha = Mathf.Clamp(1.0f - elapsedFrac, 0.0f, 1.0f);
                SetColorPickerAlpha(alpha);
                yield return null;
                elapsedTime += Time.deltaTime;
            }

            // fade-out complete, deactivate children
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }


        private void SetColorPickerAlpha(float alpha)
        {
            // Unity render modes: 0 = opaque, 1 = cutout, 2 = fade, 3 = transparent
            // use opaque mode when alpha = 1, fade mode otherwise
            float renderMode = alpha >= 1.0 ? 0 : 2;

            for (int i = 0; i < m_AllRenderers.Count; i++)
            {
                Material mat = m_AllRenderers[i].sharedMaterial;
                Color c = mat.GetColor(m_ColorPropertyName);
                c.a = alpha;
                mat.SetColor(m_ColorPropertyName, c);
                if (GraphicsSettings.defaultRenderPipeline == null)
                {
                    mat.SetFloat("_Mode", renderMode); // built-in pipeline
                }
                else
                {
                    mat.SetFloat("_Blend", renderMode);
                }
            }
        }


        [Header("Input Events")]
        [SerializeField] private VREventPrototype m_CursorDownEvent;
        [SerializeField] private VREventPrototypeVector3 m_CursorPosEvent;
        [SerializeField] private VREventPrototype m_CursorUpEvent;
        [SerializeField] private SharedToken m_RequireToken;
        [SerializeField] private Condition m_RequireCondition;

        [Header("Output Events")]
        [SerializeField] private string m_ColorModifiedEventName;
        [SerializeField] private string m_ColorSelectedEventName;
        [SerializeField] private string m_ColorCancelledEventName;

        [Header("Appearance")]
        [SerializeField] private Material m_BaseMaterial;
        [SerializeField] private float m_Radius;
        [SerializeField] private float m_CancelRadius;
        [SerializeField] private float m_CancelModeAlpha;
        [SerializeField] private float m_CursorRadius;
        [SerializeField] private float m_ColoredPointsRadius;
        [SerializeField] private float m_LineWidth;
        [SerializeField] private int m_NumPointsOnColorWheel;
        [SerializeField] private float m_FadeDuration;



        // runtime
        bool m_Active;
        bool m_InCancelState;
        Color m_InitialColor;
        Color m_CurrentColor;
        Vector3 m_CursorPosRoom;
        GameObject m_CursorSphere;
        Material m_CursorMaterial;
        List<Renderer> m_AllRenderers;
        string m_ColorPropertyName;
    }

}
