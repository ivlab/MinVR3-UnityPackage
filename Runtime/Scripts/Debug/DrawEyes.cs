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
        public TrackedProjectionScreen screen;
        public float sphereScale = 0.05f;

        // Start is called before the first frame update
        void Start() {
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
            Plane p = new Plane(screen.GetTopLeftCorner(), screen.GetTopRightCorner(), screen.GetBottomRightCorner());

            Vector3 lp = p.ClosestPointOnPlane(screen.GetLeftEyePosition());
            leftObj.transform.position = lp;

            Vector3 rp = p.ClosestPointOnPlane(screen.GetRightEyePosition());
            rightObj.transform.position = rp;
        }

        private GameObject leftObj;
        private GameObject rightObj;
    }

} // namespace
