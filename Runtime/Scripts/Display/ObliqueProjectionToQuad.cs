using UnityEngine;


namespace IVLab.MinVR3
{
    /**
     * Useful references for off-axis projection math:
	 * This script is based on the Cg_Programming article, which is based on the Koomia 
	 * article, which we have also used as a reference for earlier versions of MinVR.
     *
	 * https://en.wikibooks.org/wiki/Cg_Programming/Unity/Projection_for_Virtual_Reality
     * http://csc.lsu.edu/~kooima/articles/genperspective/ 
	 * https://github.com/MinVR/MinVR/blob/master/MVRCore/source/CameraOffAxis.cpp
	 * 
	 * To use this script, attach it to a camera.  Then, create a Quad object (GameObject > 3D Object > Quad 
	 * in the main menu) and place it into the virtual scene to define the view plane. Deactivate the Mesh 
	 * Renderer of the Quad in the Inspector Window to make it invisible (it is only a placeholder). Select
	 * the camera object and drag the Quad object to Projection Screen Quad in the Inspector.
	 * 
	 * To make the camera update with head tracking info, add a TrackedPoseDriver component to the camera.
     */
    [AddComponentMenu("MinVR Interaction/Display/Oblique Projection to Quad")]
    public class ObliqueProjectionToQuad : MonoBehaviour
    {
        [Header("Frustum")]
        public GameObject projectionScreenQuad;
        public bool estimateViewFrustum = true;
        public bool setNearClipPlane = false;
        public float minNearClipDistance = 0.0001f;
        public float nearClipDistanceOffset = -0.01f;

        [Header("Tracking and Stereo")]
        public TrackedHeadPoseDriver trackedHeadPoseDriver;
        public bool applyStereoEyeOffset = true;
        public enum Eye { LeftEye, RightEye};
        public Eye whichEye = Eye.LeftEye;


        private Camera cameraComponent;

        void OnPreCull()
        {
            cameraComponent = GetComponent<Camera>();
            if (null != projectionScreenQuad && null != cameraComponent && null != trackedHeadPoseDriver) {

                // set the camera's position, which depends on which eye the camera represents
                Vector3 pe;
                if (!applyStereoEyeOffset)
                {
                    pe = trackedHeadPoseDriver.GetHeadPositionInWorldSpace();
                }
                else if (whichEye == Eye.LeftEye)
                {
                    pe = trackedHeadPoseDriver.GetLeftEyePositionInWorldSpace();
                }
                else
                {
                    pe = trackedHeadPoseDriver.GetRightEyePositionInWorldSpace();
                }
                transform.position = pe;


                // lower left corner in world coordinates
                Vector3 pa = projectionScreenQuad.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0.0f));
                // lower right corner
                Vector3 pb = projectionScreenQuad.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0.0f));
                // upper left corner
                Vector3 pc = projectionScreenQuad.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0.0f));

                // distance of near clipping plane
                float n = cameraComponent.nearClipPlane;
                // distance of far clipping plane
                float f = cameraComponent.farClipPlane;

                Vector3 va; // from pe to pa
                Vector3 vb; // from pe to pb
                Vector3 vc; // from pe to pc
                Vector3 vr; // right axis of screen
                Vector3 vu; // up axis of screen
                Vector3 vn; // normal vector of screen

                float l; // distance to left screen edge
                float r; // distance to right screen edge
                float b; // distance to bottom screen edge
                float t; // distance to top screen edge
                float d; // distance from eye to screen

                vr = pb - pa;
                vu = pc - pa;
                va = pa - pe;
                vb = pb - pe;
                vc = pc - pe;

                // are we looking at the backface of the plane object?
                if (Vector3.Dot(-Vector3.Cross(va, vc), vb) < 0.0f) {
                    Debug.Log("Back");
                    // mirror points along the x axis (most users 
                    // probably expect the y axis to stay fixed)
                    vr = -vr;
                    pa = pb;
                    pb = pa + vr;
                    pc = pa + vu;
                    va = pa - pe;
                    vb = pb - pe;
                    vc = pc - pe;
                }

                vr.Normalize();
                vu.Normalize();
                vn = -Vector3.Cross(vr, vu);
                // we need the minus sign because Unity 
                // uses a left-handed coordinate system
                vn.Normalize();

                d = -Vector3.Dot(va, vn);
                if (setNearClipPlane) {
                    n = Mathf.Max(minNearClipDistance, d + nearClipDistanceOffset);
                    cameraComponent.nearClipPlane = n;
                }
                l = Vector3.Dot(vr, va) * n / d;
                r = Vector3.Dot(vr, vb) * n / d;
                b = Vector3.Dot(vu, va) * n / d;
                t = Vector3.Dot(vu, vc) * n / d;

                Matrix4x4 p = new Matrix4x4(); // projection matrix 
                p[0, 0] = 2.0f * n / (r - l);
                p[0, 1] = 0.0f;
                p[0, 2] = (r + l) / (r - l);
                p[0, 3] = 0.0f;

                p[1, 0] = 0.0f;
                p[1, 1] = 2.0f * n / (t - b);
                p[1, 2] = (t + b) / (t - b);
                p[1, 3] = 0.0f;

                p[2, 0] = 0.0f;
                p[2, 1] = 0.0f;
                p[2, 2] = (f + n) / (n - f);
                p[2, 3] = 2.0f * f * n / (n - f);

                p[3, 0] = 0.0f;
                p[3, 1] = 0.0f;
                p[3, 2] = -1.0f;
                p[3, 3] = 0.0f;

                Matrix4x4 rm = new Matrix4x4(); // rotation matrix;
                rm[0, 0] = vr.x;
                rm[0, 1] = vr.y;
                rm[0, 2] = vr.z;
                rm[0, 3] = 0.0f;

                rm[1, 0] = vu.x;
                rm[1, 1] = vu.y;
                rm[1, 2] = vu.z;
                rm[1, 3] = 0.0f;

                rm[2, 0] = vn.x;
                rm[2, 1] = vn.y;
                rm[2, 2] = vn.z;
                rm[2, 3] = 0.0f;

                rm[3, 0] = 0.0f;
                rm[3, 1] = 0.0f;
                rm[3, 2] = 0.0f;
                rm[3, 3] = 1.0f;

                Matrix4x4 tm = new Matrix4x4(); // translation matrix;
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

                // set matrices
                // The original paper puts everything into the projection 
                // matrix (i.e. sets it to p * rm * tm and the other 
                // matrix to the identity), but this doesn't appear to 
                // work with Unity's shadow maps.
                cameraComponent.projectionMatrix = p;
                cameraComponent.worldToCameraMatrix = rm * tm;

                if (estimateViewFrustum) {
                    // rotate camera to screen for culling to work
                    Quaternion q = new Quaternion();
                    q.SetLookRotation((0.5f * (pb + pc) - pe), vu);
                    // look at center of screen
                    cameraComponent.transform.rotation = q;

                    // set fieldOfView to a conservative estimate 
                    // to make frustum tall enough
                    if (cameraComponent.aspect >= 1.0f) {
                        cameraComponent.fieldOfView = Mathf.Rad2Deg *
                            Mathf.Atan(((pb - pa).magnitude + (pc - pa).magnitude) /
                                va.magnitude);
                    } else {
                        // take the camera aspect into account to 
                        // make the frustum wide enough 
                        cameraComponent.fieldOfView =
                            Mathf.Rad2Deg / cameraComponent.aspect *
                            Mathf.Atan(((pb - pa).magnitude + (pc - pa).magnitude) /
                                va.magnitude);
                    }
                }
            }
        }
    }

}
