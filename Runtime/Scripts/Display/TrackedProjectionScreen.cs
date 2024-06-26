using UnityEngine;
using System.Collections;

namespace IVLab.MinVR3 {

    /** Use this script to update a Camera's matrices for displaying
     * head-tracked stereoscopic graphics with off-axis projection on a flat
     * projection screen -- used for powerwalls, cave walls, fishtank vr, etc.
     *
     * The screen is defined by 4 corners, which are defined in tracking space, the
     * local coordinate system reported by the tracking system so that both the screen
     * corners and head position are defined relative to the same physical coordinates.
     * The projection screen must be a rectangle.
     *
     * By default the script will modify the Camera component attached to the same
     * GameObject as this script or the Main Camera if no Camera component is found.
     * You can override this by setting the cam field explicitly.
     *
     * To specify a default or initial head transform to use when head tracking is not
     * active, set the position of the head using the camera's transform.
     * 
	 * Useful references for off-axis projection math:
	 * https://en.wikibooks.org/wiki/Cg_Programming/Unity/Projection_for_Virtual_Reality
	 * https://github.com/MinVR/MinVR/blob/master/MVRCore/source/CameraOffAxis.cpp
	 * http://csc.lsu.edu/~kooima/articles/genperspective/ 
	 */
    [ExecuteAlways]
    [AddComponentMenu("MinVR/Display/Tracked Projection Screen")]
    public class TrackedProjectionScreen : MonoBehaviour, IVREventListener {

        [System.Serializable]
        public class ScreenCorners {
            public Vector3 topLeft = new Vector3(-4f, 4f, 4f);
            public Vector3 topRight = new Vector3(4f, 4f, 4f);
            public Vector3 bottomRight = new Vector3(4f, -4f, 4f);
            public Vector3 bottomLeft = new Vector3(-4f, -4f, 4f);
        }

        [Header("Physical Setup")]
        [Tooltip("Positions in the physical tracking space coordinate system for the four corners of the " +
            "projection screen.  These must form a rectangle.")]
        public ScreenCorners trackingSpaceCorners;

        [Tooltip("Use world space for tracking corners")]
        public bool useWorldSpaceForCorners = true;

        [Header("Camera Option #1")]
        [Tooltip("Set this camera to render to single camera that should render both eyes using Unity's built-in " +
            "support for that.  This class will access Camera.stereoTargetEye to determine which eye is currently " +
            "and Camera.stereoSeparation to determine the interocular distance.")]
        public Camera stereoCam;

        [Header("Camera Option #2")]
        [Tooltip("Set these two cameras and the interocular distance to use two different cameras, one per eye.")]
        public Camera leftCam;
        public Camera rightCam;
        [Tooltip("Distance between the eyes in meters.  Same idea as Unity's Camera.stereoSeparation property.")]
        public float interOcularDistance;

        [Header("Tracking")]
        [Tooltip("The VREvent that provides head tracking position updates.")]
        public VREventPrototypeVector3 headTrackingPosEvent;

        [Tooltip("The VREvent that provides head tracking rotation updates.")]
        public VREventPrototypeQuaternion headTrackingRotEvent;
        
        public enum ProjectionType { Perspective, Parallel };

        [Header("Misc")]
        [Tooltip("Perspective projection is typically (always?) used for head tracked displays.")]
        public ProjectionType projectionType = ProjectionType.Perspective;

        [Tooltip("Color to use when drawing the projection plane and off-axis view frustrum in the editor.")]
        public Color debugColor = Color.green;


        void Reset()
        {
            stereoCam = null;
            leftCam = null;
            rightCam = null;
            interOcularDistance = 0.063f; // 63mm on average for adults
            trackingSpaceCorners = new ScreenCorners();
            headTrackingPosEvent = null;
            headTrackingRotEvent = null;
            projectionType = ProjectionType.Perspective;
            debugColor = Color.green;
        }

        void OnEnable() {
            if (Application.IsPlaying(gameObject)) { // play mode
                VREngine.Instance.eventManager.AddEventListener(this);
            }
        }

        void OnDisable()
        {
            if (Application.IsPlaying(gameObject)) { // play mode
                VREngine.Instance?.eventManager?.RemoveEventListener(this);
            }
        }

        void Start()
        {
            Camera cam = stereoCam ? stereoCam : leftCam;
            Debug.Assert(cam, "Set either the stereoTargetCam OR the leftCam and rightCam");
        }
        
        void Update() {
            if (!Application.IsPlaying(gameObject)) {  // edit mode only
                Color c = debugColor;
                if (!IsRectangle(trackingSpaceCorners)) {
                    // Switch to the hot pick color Unity uses to indicate errors
                    c = new Color(1.0f, 0.0275f, 1.0f);
                }

                Debug.DrawLine(GetTopLeftCorner(), GetTopRightCorner(), c);
                Debug.DrawLine(GetTopRightCorner(), GetBottomRightCorner(), c);
                Debug.DrawLine(GetBottomRightCorner(), GetBottomLeftCorner(), c);
                Debug.DrawLine(GetBottomLeftCorner(), GetTopLeftCorner(), c);

                Camera cam = stereoCam ? stereoCam : leftCam;
                Debug.DrawLine(cam.transform.position, GetTopRightCorner(), c);
                Debug.DrawLine(cam.transform.position, GetBottomRightCorner(), c);
                Debug.DrawLine(cam.transform.position, GetBottomLeftCorner(), c);
                Debug.DrawLine(cam.transform.position, GetTopLeftCorner(), c);
            }
        }



        void LateUpdate() {
            // screen corners in world space
            Vector3 bl = GetBottomLeftCorner();
            Vector3 br = GetBottomRightCorner();
            Vector3 tl = GetTopLeftCorner();
            Vector3 tr = GetTopRightCorner();

            Vector3 vr; // right axis of screen
            Vector3 vu; // up axis of screen
            Vector3 vn; // normal vector of screen

            vr = br - bl;
            vu = tl - bl;
            vr.Normalize();
            vu.Normalize();
            vn = -Vector3.Cross(vr, vu); // we need the minus sign because Unity uses a left-handed coordinate system
            vn.Normalize();

            // set matrices: projection and view matrices

            if (stereoCam) {
                // Using a single camera with a stereoTargetEye flag.

                /// for cyclops...        
                stereoCam.projectionMatrix = GetStereoProjectionMatrix(stereoCam.transform.position, vr, vu, vn, stereoCam.nearClipPlane, stereoCam.farClipPlane);
                stereoCam.worldToCameraMatrix = GetStereoViewMatrix(stereoCam.transform.position, vr, vu, vn);

                //// for left eye...
                if (stereoCam.stereoTargetEye == StereoTargetEyeMask.Both || stereoCam.stereoTargetEye == StereoTargetEyeMask.Left) {
                    Vector3 pe = GetLeftEyePosition(); // eye position
                    stereoCam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, GetStereoProjectionMatrix(pe, vr, vu, vn, stereoCam.nearClipPlane, stereoCam.farClipPlane));
                    stereoCam.SetStereoViewMatrix(Camera.StereoscopicEye.Left, GetStereoViewMatrix(pe, vr, vu, vn));
                }
                //// for right eye...
                if (stereoCam.stereoTargetEye == StereoTargetEyeMask.Both || stereoCam.stereoTargetEye == StereoTargetEyeMask.Right) {
                    Vector3 pe = GetRightEyePosition(); // eye position
                    stereoCam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, GetStereoProjectionMatrix(pe, vr, vu, vn, stereoCam.nearClipPlane, stereoCam.farClipPlane));
                    stereoCam.SetStereoViewMatrix(Camera.StereoscopicEye.Right, GetStereoViewMatrix(pe, vr, vu, vn));
                }
            } else {
                // Separate Left and Right Cameras

                // left camera
                Vector3 lEyePos = GetLeftEyePosition(); // eye position
                leftCam.projectionMatrix = GetStereoProjectionMatrix(lEyePos, vr, vu, vn, leftCam.nearClipPlane, leftCam.farClipPlane);
                leftCam.worldToCameraMatrix = GetStereoViewMatrix(lEyePos, vr, vu, vn);

                // right camera
                Vector3 rEyePos = GetLeftEyePosition(); // eye position
                rightCam.projectionMatrix = GetStereoProjectionMatrix(rEyePos, vr, vu, vn, rightCam.nearClipPlane, rightCam.farClipPlane);
                rightCam.worldToCameraMatrix = GetStereoViewMatrix(rEyePos, vr, vu, vn);
            }


            // The original paper puts everything into the projection  matrix (i.e. sets it to p * rm * tm and the other 
            // matrix to the identity), but this doesn't appear to work with Unity's shadow maps.
            // https://en.wikibooks.org/wiki/Cg_Programming/Unity/Projection_for_Virtual_Reality
            // talks about Unity using camera's rotation and its fov for view culling
            // they provide an implmentation to adjust them according to the above computations. 
            // we have not implemented this here: seems to work fine for now and VR Tracker Listener might be rotating the camera. 

            // TODO check if stereo convergence is needed.
            //Plane plane = new Plane(GetTopLeftCorner(), GetTopRightCorner(), GetBottomRightCorner());
            //cam.stereoConvergence = plane.GetDistanceToPoint(cam.transform.position);

        }

        // Vector3 pe; // eye position
        // Vector3 vr; // right axis of screen
        // Vector3 vu; // up axis of screen
        // Vector3 vn; // normal vector of screen
        private Matrix4x4 GetStereoViewMatrix(Vector3 pe, Vector3 vr, Vector3 vu, Vector3 vn) {
            // rotation matrix;
            Matrix4x4 rm = Matrix4x4.identity;
            rm[0, 0] = vr.x;
            rm[0, 1] = vr.y;
            rm[0, 2] = vr.z;
            rm[0, 3] = 0.0f;

            rm[1, 0] = vu.x;
            rm[1, 1] = vu.y;
            rm[1, 2] = vu.z;
            rm[1, 3] = 0.0f;

            rm[2, 0] = -vn.x;
            rm[2, 1] = -vn.y;
            rm[2, 2] = -vn.z;
            rm[2, 3] = 0.0f;

            rm[3, 0] = 0.0f;
            rm[3, 1] = 0.0f;
            rm[3, 2] = 0.0f;
            rm[3, 3] = 1.0f;

            // translation matrix;
            Matrix4x4 tm = Matrix4x4.identity;
            tm[0, 0] = 1.0f;
            tm[0, 1] = 0.0f;
            tm[0, 2] = 0.0f;
            tm[0, 3] = -pe.x;

            tm[1, 0] = 0.0f;
            tm[1, 1] = 1.0f;
            tm[1, 2] = 0.0f;
            tm[1, 3] = -pe.y;

            tm[2, 0] = 0.0f;
            tm[2, 1] = 0.0f;
            tm[2, 2] = 1.0f;
            tm[2, 3] = -pe.z;

            tm[3, 0] = 0.0f;
            tm[3, 1] = 0.0f;
            tm[3, 2] = 0.0f;
            tm[3, 3] = 1.0f;

            // view matrix;
            return rm * tm;
        }

        // Vector3 pe; // eye position
        // Vector3 vr; // right axis of screen
        // Vector3 vu; // up axis of screen
        // Vector3 vn; // normal vector of screen
        private Matrix4x4 GetStereoProjectionMatrix(Vector3 pe, Vector3 vr, Vector3 vu, Vector3 vn, float n, float f) {
            // screen corners in world space
            Vector3 bl = GetBottomLeftCorner();
            Vector3 br = GetBottomRightCorner();
            Vector3 tl = GetTopLeftCorner();
            Vector3 tr = GetTopRightCorner();

            float l; // distance to left screen edge
            float r; // distance to right screen edge
            float b; // distance to bottom screen edge
            float t; // distance to top screen edge
            float d; // distance from eye to screen 

            //
            Vector3 vbl; // from pe to bl
            Vector3 vbr; // from pe to br
            Vector3 vtl; // from pe to tl
            if (projectionType == ProjectionType.Perspective) {
                vbl = bl - pe; // va
                vbr = br - pe; // vb
                vtl = tl - pe; // vc

                d = -Vector3.Dot(vbl, vn);
                l = Vector3.Dot(vr, vbl) * n / d;
                r = Vector3.Dot(vr, vbr) * n / d;
                b = Vector3.Dot(vu, vbl) * n / d;
                t = Vector3.Dot(vu, vtl) * n / d;
            }
            else { // parallel projection
                float halfWidth = (br - bl).magnitude / 2.0f;
                float halfHeight = (tl - bl).magnitude / 2.0f;

                l = -halfWidth;
                r = halfWidth;
                b = -halfHeight;
                t = halfHeight;
            }

            // projection matrix 
            Matrix4x4 p = Matrix4x4.identity;
            if (projectionType == ProjectionType.Perspective) {
                p[0, 0] = 2.0f * n / (r - l);
                p[0, 1] = 0.0f;
                p[0, 2] = (l + r) / (l - r);
                p[0, 3] = 0.0f;

                p[1, 0] = 0.0f;
                p[1, 1] = 2.0f * n / (t - b);
                p[1, 2] = (t + b) / (b - t);
                p[1, 3] = 0.0f;

                p[2, 0] = 0.0f;
                p[2, 1] = 0.0f;
                p[2, 2] = (f + n) / (f - n);
                p[2, 3] = 2.0f * n * f / (n - f);

                p[3, 0] = 0.0f;
                p[3, 1] = 0.0f;
                p[3, 2] = 1.0f;
                p[3, 3] = 0.0f;
            }
            else { // parallel projection
                p = Matrix4x4.Ortho(l, r, b, t, n, f);

                // compute a sheering matrix for off-axis parallel proejction
                Vector3 center = (tl + tr + br + bl) / 4.0f; // center of the screen
                Vector3 vc = pe - center; // from eye to the center

                Vector3 vcOnVrVnPlane = vc - (Vector3.Dot(vc, vu) * vu);
                Vector3 vcOnVuVnPlane = vc - (Vector3.Dot(vc, vr) * vr);

                float theta = Vector3.Angle(vr, vcOnVrVnPlane) * Mathf.Deg2Rad;
                float phi = Vector3.Angle(vu, vcOnVuVnPlane) * Mathf.Deg2Rad;

                Matrix4x4 h = Matrix4x4.identity; // a sheering matrix
                h[0, 2] = -1f / Mathf.Tan(theta);
                h[1, 2] = -1f / Mathf.Tan(phi);
                p *= h;
            }

            return p;
        }

        public Vector3 GetLeftEyePosition() {
            // offset to the left by 1/2 the interocular distance
            if (stereoCam) {
                Vector3 offset = stereoCam.transform.TransformVector(new Vector3(-stereoCam.stereoSeparation / 2f, 0f, 0f));
                return stereoCam.transform.position + offset;

            } else {
                Vector3 offset = leftCam.transform.TransformVector(new Vector3(-interOcularDistance / 2f, 0f, 0f));
                return leftCam.transform.position + offset;
            }
        }

        public Vector3 GetRightEyePosition() {
            // offset to the right by 1/2 the interocular distance
            if (stereoCam) {
                Vector3 offset = stereoCam.transform.TransformVector(new Vector3(stereoCam.stereoSeparation / 2f, 0f, 0f));
                return stereoCam.transform.position + offset;
            } else {
                Vector3 offset = rightCam.transform.TransformVector(new Vector3(interOcularDistance / 2f, 0f, 0f));
                return rightCam.transform.position + offset;
            }
        }
 



        public Vector3 GetTopLeftCorner() {
            if (useWorldSpaceForCorners)
                return trackingSpaceCorners.topLeft;
            else
                return transform.localToWorldMatrix.MultiplyPoint3x4(trackingSpaceCorners.topLeft);
        }

        public Vector3 GetTopRightCorner() {
            if (useWorldSpaceForCorners)
                return trackingSpaceCorners.topRight;
            else
                return transform.localToWorldMatrix.MultiplyPoint3x4(trackingSpaceCorners.topRight);
        }

        public Vector3 GetBottomRightCorner() {
            if (useWorldSpaceForCorners)
                return trackingSpaceCorners.bottomRight;
            else
                return transform.localToWorldMatrix.MultiplyPoint3x4(trackingSpaceCorners.bottomRight);
        }

        public Vector3 GetBottomLeftCorner() {
            if (useWorldSpaceForCorners)
                return trackingSpaceCorners.bottomLeft;
            else
                return transform.localToWorldMatrix.MultiplyPoint3x4(trackingSpaceCorners.bottomLeft);
        }


        private bool IsRectangle(ScreenCorners corners) {
            const float epsilon = 0.0001f;

            // checking if right angles
            // from a top left corner
            if (Mathf.Abs(Vector3.Dot(trackingSpaceCorners.topRight - trackingSpaceCorners.topLeft, trackingSpaceCorners.bottomLeft - trackingSpaceCorners.topLeft)) >= epsilon) {
                return false;
            }
            // from a top right corner
            if (Mathf.Abs(Vector3.Dot(trackingSpaceCorners.topLeft - trackingSpaceCorners.topRight, trackingSpaceCorners.bottomRight - trackingSpaceCorners.topRight)) >= epsilon) {
                return false;
            }
            // from a bottom right corner
            if (Mathf.Abs(Vector3.Dot(trackingSpaceCorners.topRight - trackingSpaceCorners.bottomRight, trackingSpaceCorners.bottomLeft - trackingSpaceCorners.bottomRight)) >= epsilon) {
                return false;
            }
            // from a bottom left corner
            if (Mathf.Abs(Vector3.Dot(trackingSpaceCorners.topLeft - trackingSpaceCorners.bottomLeft, trackingSpaceCorners.bottomRight - trackingSpaceCorners.bottomLeft)) >= epsilon) {
                return false;
            }

            return true;
        }

        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(headTrackingPosEvent)) {
                if (stereoCam) {
                    stereoCam.transform.position = vrEvent.GetData<Vector3>();
                } else {
                    leftCam.transform.position = vrEvent.GetData<Vector3>();
                    rightCam.transform.position = vrEvent.GetData<Vector3>();
                }
            } else if (vrEvent.Matches(headTrackingRotEvent)) {
                if (stereoCam) {
                    stereoCam.transform.rotation = vrEvent.GetData<Quaternion>();
                } else {
                    leftCam.transform.rotation = vrEvent.GetData<Quaternion>();
                    rightCam.transform.rotation = vrEvent.GetData<Quaternion>();
                }
            }
        }

        public bool IsListening()
        {
            return m_Listening;
        }

        public void StartListening()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
            m_Listening = true;
        }

        public void StopListening()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
            m_Listening = false;
        }

        private bool m_Listening = false;
    }
}
