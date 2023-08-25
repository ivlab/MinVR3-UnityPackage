using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace IVLab.MinVR3
{
    /// <summary>
    /// Implements a trackball rotate interaction for use with any GameObjects in the scene that have a Collider
    /// attached.  When the mouse is clicked, a raycast is used to see if any object was picked.  By default, this
    /// raycast will look at all objects in the scene that have colliders attached, but you can restrict it to a
    /// subset by assigning different layers to your gameobjects and including the layers to ignore in the
    /// ignoreLayers LayerMask.  If the collider on the object selected by the mouse is not a sphere collider,
    /// then it is disabled immediately after the initial selection and a temporary sphere collider is added to
    /// object since the trackball effect requires intersecting with a bounding sphere.  When the mouse button is
    /// released, the temporary collider is removed and the original is reenabled.
    ///
    /// Created by Dan, Morgan, &amp; Sean 2/10/21
    /// </summary>
    [AddComponentMenu("MinVR Interaction/Desktop/Mouse-Object Manipulator")]
    public class MouseObjectManipulator : MonoBehaviour
    {
        [Tooltip("Click and drag with this button to translate the object at its current depth in a plane parallel to the filmplane.  [Default: Mouse Left]")]
        public KeyCode translateButton;

        [Tooltip("Click and drag with this button to rotate an object. [Default: Mouse Right]")]
        public KeyCode rotateButton;

        [Tooltip("Click and drag with this button to translate the object in depth (in and out of the screen). [Default: Mouse Middle]")]
        public KeyCode dollyButton;

        [Tooltip("Layers to include when doing a Physics.Raycast to determine which collider(s) the mouse has clicked on.")]
        public LayerMask layers;


        private void Reset()
        {
            layers = LayerMask.GetMask("Default");
            translateButton = KeyCode.Mouse0;
            rotateButton = KeyCode.Mouse1;
            dollyButton = KeyCode.Mouse2;
        }


        void Update()
        {
            if (m_State == UIState.Idle) {
                if (Input.GetKeyDown(translateButton)) {
                    OnTranslateDown(Input.mousePosition);
                } else if (Input.GetKeyDown(rotateButton)) {
                    OnRotateDown(Input.mousePosition);
                } else if (Input.GetKeyDown(dollyButton)) {
                    OnDollyDown(Input.mousePosition);
                }
            } else if (m_State == UIState.Translate) {
                if (Input.GetKeyUp(translateButton)) {
                    OnTranslateUp(Input.mousePosition);
                } else {
                    OnTranslateMove(Input.mousePosition);
                }
            } else if (m_State == UIState.Rotate) {
                if (Input.GetKeyUp(rotateButton)) {
                    OnRotateUp(Input.mousePosition);
                } else {
                    OnRotateMove(Input.mousePosition);
                }
            } else if (m_State == UIState.Dolly) {
                if (Input.GetKeyUp(dollyButton)) {
                    OnDollyUp(Input.mousePosition);
                } else {
                    OnDollyMove(Input.mousePosition);
                }
            }
        }


        // This implements a "filmplane translate" operation, which means, the object that is selected stays at its
        // current depth and translates in plane parallel to the filmplane at that depth.

        void OnTranslateDown(Vector3 mousePosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layers)) {
                // hit something!
                m_OrigCollider = hit.collider;
                m_Filmplane = new Plane(-Camera.main.transform.forward, hit.point);
                m_State = UIState.Translate;
            }
            m_LastMousePosition = mousePosition;
        }

        void OnTranslateMove(Vector3 mousePosition)
        {
            float lastHitDist;
            Ray lastMousePickRay = Camera.main.ScreenPointToRay(m_LastMousePosition);
            float newHitDist;
            Ray newMousePickRay = Camera.main.ScreenPointToRay(mousePosition);

            if ((m_Filmplane.Raycast(lastMousePickRay, out lastHitDist)) &&
                (m_Filmplane.Raycast(newMousePickRay, out newHitDist))) {

                Vector3 lastPtWorld = lastMousePickRay.GetPoint(lastHitDist);
                Vector3 newPtWorld = newMousePickRay.GetPoint(newHitDist);
                Vector3 deltaWorld = newPtWorld - lastPtWorld;

                m_OrigCollider.gameObject.transform.position += deltaWorld;
            }
            m_LastMousePosition = mousePosition;
        }

        void OnTranslateUp(Vector3 mousePosition)
        {
            m_LastMousePosition = mousePosition;
            m_State = UIState.Idle;
        }


        // This implements dolly motion where the object is translated forward/backward relative to the camera look vector.
        // Moving the mouse toward the top of the window, translates away from the camera.  The scale factors are set so
        // that moving to the top of the screen will translate the object to the camera's far plane and moving to the bottom
        // of the screen will bring the object to the near plane. 
        void OnDollyDown(Vector3 mousePosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layers)) {
                // hit something!
                m_OrigCollider = hit.collider;

                float depth = Vector3.Dot(hit.point - Camera.main.transform.position, Camera.main.transform.forward);

                float deltaYToBottom = Screen.height - mousePosition[1];
                m_DollyScaleFactor = depth / deltaYToBottom;
                m_State = UIState.Dolly;
            }
            m_LastMousePosition = mousePosition;
        }

        void OnDollyMove(Vector3 mousePosition)
        {
            Vector3 deltaWorld = new Vector3(0, 0, m_DollyScaleFactor * (mousePosition[1] - m_LastMousePosition[1]));
            m_OrigCollider.gameObject.transform.position += deltaWorld;

            m_LastMousePosition = mousePosition;
        }

        void OnDollyUp(Vector3 mousePosition)
        {
            m_LastMousePosition = mousePosition;
            m_State = UIState.Idle;
        }


        // The mouse has just been pressed, use a raycast into the scene to see if the cursor is over a selectable object
        void OnRotateDown(Vector3 mousePosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layers)) {
                // hit something!
                m_OrigCollider = hit.collider;
                // we need a sphere collider to do a trackball interface.
                // if we can cast the original collider to a spherecollider, then yes it is.
                m_BoundingSphereCollider = m_OrigCollider as SphereCollider;
                if (m_BoundingSphereCollider == null) {
                    // since the value is null, the cast didn't work, and we know this collider must not be a spherecollider
                    // so, let's create a new spherecollider and add it to the gameobject that was selected
                    m_BoundingSphereCollider = m_OrigCollider.gameObject.AddComponent<SphereCollider>();
                    // and disable the original collider.
                    m_OrigCollider.enabled = false;
                }
                m_State = UIState.Rotate;
            } // else, didn't hit anything
            m_LastMousePosition = mousePosition;
        }


        // We're in the middle of a trackball rotate interaction, apply a rotation to the selected object based on
        // the movement of the mouse from the last frame to this frame.
        void OnRotateMove(Vector3 mousePosition)
        {
            RaycastHit hitLast;
            Ray lastMousePickRay = Camera.main.ScreenPointToRay(m_LastMousePosition);
            RaycastHit hitNew;
            Ray newMousePickRay = Camera.main.ScreenPointToRay(mousePosition);

            // We can only calculate a correct rotation if both rays hit the bounding sphere of the object 
            if ((m_BoundingSphereCollider.Raycast(lastMousePickRay, out hitLast, Mathf.Infinity)) &&
                (m_BoundingSphereCollider.Raycast(newMousePickRay, out hitNew, Mathf.Infinity))) {

                // Find where the pick rays hit the bounding sphere
                Vector3 p1World = hitLast.point;
                Vector3 p2World = hitNew.point;

                // Find the vector from the center of the sphere to these points and normalize these because we just want
                // to know the direction, not the magnitude.  Be careful to be consistent with coordinate spaces.  The center
                // of the bounding sphere is provided in local coordinates, but the hitpoints above are in world coordinates.
                // We could do all these calculations in either local or world space, it doesn't really matter which, but
                // we need to pick one and be consistent.  We've picked world here.  So, make sure to convert that center
                // point to world space before using it.
                Vector3 centerLocal = m_BoundingSphereCollider.center;
                Vector3 centerWorld = m_BoundingSphereCollider.gameObject.transform.localToWorldMatrix.MultiplyPoint(centerLocal);
                Vector3 v1World = Vector3.Normalize(p1World - centerWorld);
                Vector3 v2World = Vector3.Normalize(p2World - centerWorld);

                // The axis of rotation for the trackball is the axis that is perpendicular to v1 and v2 -- easy to find
                // with a cross product
                Vector3 axis = Vector3.Cross(v1World, v2World).normalized;

                // The angle to rotate is the angle between v1 and v2.  Recall, the definition of the dot product is
                // dot(v1,v2) = |v1||v2|cos(a) where a is the angle between v1 and v2.  Since we made v1 and v2 unit vectors
                // this simplifies to dot(v1,v2) = cos(a).  If we take the inverse cosine or arccos of both sides we have
                // arccos(dot(v1,v2)) = a.
                float angle = Mathf.Acos(Vector3.Dot(v1World, v2World)); // angle between v1 and v2 in radians

                // So, the world-space incremental rotation we should apply to the object is as follows
                Quaternion rotBy = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, axis);

                // We need to be careful with how we apply this to the GameObject's transform. The center point for the
                // rotation is not the world space origin, it will be the center of the object's bounding sphere.  When
                // learning graphics, you learn to overcome this by composing multiple transformations together, like
                // translate to the origin, then rotate, then translate back.  Unity makes this annoying difficult because
                // of the way they split the transformation up into position, rotation, and scale components.  We could
                // convert everything to Matrix4x4s, compose them all together, and the extract position, rotation, and
                // scale.  Or, I have created some extension methods as part of MinVR3 that accomplish the same result
                // without needing to convert back and forth between the different representations.  Since, MinVR3 may not
                // be included in every project, I'll just copy the one function we need into this class.
                RotateAroundWorldPoint(m_BoundingSphereCollider.gameObject.transform, centerWorld, rotBy);
            }
            m_LastMousePosition = mousePosition;
        }


        // Copied from MinVR3/Runtime/Utils/TransformExtensions.cs
        void RotateAroundWorldPoint(Transform t, Vector3 point, Quaternion deltaRot)
        {
            Vector3 worldPos = t.position;
            Vector3 dif = worldPos - point;
            dif = deltaRot * dif;
            worldPos = point + dif;
            t.position = worldPos;
            t.rotation = t.rotation * (Quaternion.Inverse(t.rotation) * deltaRot * t.rotation);
        }


        // Done rotating the object, deselect it, and restore it's original collider if we had to add a temp spherecollider
        // during the OnTrackballStart method
        void OnRotateUp(Vector3 mousePosition)
        {
            if (m_OrigCollider != m_BoundingSphereCollider) {
                Destroy(m_BoundingSphereCollider);
                m_OrigCollider.enabled = true;
            }
            m_BoundingSphereCollider = null;
            m_LastMousePosition = mousePosition;
            m_State = UIState.Idle;
        }

        enum UIState
        {
            Idle,
            Translate,
            Rotate,
            Dolly
        }

        UIState m_State = UIState.Idle;
        bool m_MouseDown = false;
        Vector3 m_LastMousePosition; // 2D mouse position from the previous frame
        SphereCollider m_BoundingSphereCollider; // this is the one we use to calculate the rotation
        Collider m_OrigCollider; // if not a sphere, we disable this one, add a spherecollider, and reenable later
        Plane m_Filmplane; // used for the filmplane translate
        float m_DollyScaleFactor;
    }

}
