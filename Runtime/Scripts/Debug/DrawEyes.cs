using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    /// <summary>
    /// This is useful for debugging displays that use off-axis projection, like the walls of a Cave of a fishtank VR
    /// or Powerwall display.  The position of each eye (as reported via head tracking) is projected onto the plane of
    /// the display and a sphere is drawn for each eye at its projected position.  You can then move the glasses around
    /// and make sure that these spheres move correctly with them to confirm that you have set offsets from the tracker
    /// appropriately (e.g., the offset from the tracking device mounted on the glasses to each eye, making sure the
    /// left eye is indeed on the left side, etc.)
    /// </summary>
    [AddComponentMenu("MinVR/Debug/Draw Eyes")]
    public class DrawEyes : MonoBehaviour
    {
        public float sphereScale = 0.05f;

        [Header("Option 1: Tracked Head Pose Driver and Oblique Projectino to Quad")]
        [Tooltip("MinVR3 has two ways to update the projection matrices for tracked projection screens. " +
            "Set the fields below if using the TrackedHeadPoseDriver.cs and ObliqueProjectionToQuad.cs scripts.")]
        public TrackedHeadPoseDriver headPoseDriver;
        [Tooltip("The same Quad gameobject that is passed to the ObliqueProjectionToQuad script.")]
        public GameObject projectionScreenQuad;

        [Header("Option 2: Tracked Projection Screen")]
        [Tooltip("MinVR3 has two ways to update the projection matrices for tracked projection screens. " +
            "Set the field below if using the TrackedProjectionScreen.cs script.")]
        public TrackedProjectionScreen screen;

        // Start is called before the first frame update
        void Start()
        {
            leftObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftObj.transform.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
            leftObj.GetComponent<Renderer>().material.color = Color.red;

            rightObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightObj.transform.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
            rightObj.GetComponent<Renderer>().material.color = Color.green;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 lp = Vector3.zero;
            Vector3 rp = Vector3.zero;

            if (screen != null)
            {
                Plane p = new Plane(screen.GetTopLeftCorner(), screen.GetTopRightCorner(), screen.GetBottomRightCorner());
                lp = p.ClosestPointOnPlane(screen.GetLeftEyePosition());
                rp = p.ClosestPointOnPlane(screen.GetRightEyePosition());
            }
            else if ((headPoseDriver != null) && (projectionScreenQuad != null))
            {
                Plane p1 = new Plane(-projectionScreenQuad.transform.forward, projectionScreenQuad.transform.position);
                lp = p1.ClosestPointOnPlane(headPoseDriver.GetLeftEyePositionInWorldSpace());

                Plane p2 = new Plane(-projectionScreenQuad.transform.forward, projectionScreenQuad.transform.position);
                rp = p2.ClosestPointOnPlane(headPoseDriver.GetRightEyePositionInWorldSpace());
            }

            leftObj.transform.position = lp;
            rightObj.transform.position = rp;
        }

        private GameObject leftObj;
        private GameObject rightObj;
    }

} // namespace
