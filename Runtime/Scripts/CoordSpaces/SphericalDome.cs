using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace IVLab.MinVR3
{

    /// <summary>
    /// A brush up on spherical coordinate systems may be helpful :)
    /// https://en.wikipedia.org/wiki/Spherical_coordinate_system
    /// https://en.wikipedia.org/wiki/Spherical_cap
    /// 
    /// This class assumes the dome is a spherical cap, i.e., a portion of a sphere produced by
    /// slicing the sphere with a plane.  The Bell Museum dome is a hemisphere, meaning the plane
    /// slices right through the sphere's center point.  However, the traveling "blow up" planetariums
    /// (think bouncy house) we use when bringing the show on the road are not a full hemisphere.
    /// Instead of covering a full 180 degrees, they cover more like 120 degree, so the cap is smaller.
    /// This class handles both cases, just set the domeViewAngle appropriately.
    ///
    /// ## Definitions and Coordinate Spaces
    ///
    /// **Dome Space** is a coordinate system aligned with the spherical cap of the dome so that the
    /// Up direction (+Y) points from the center of the sphere to the center point (i.e., "North Pole")
    /// on the cap.  This direction is also the normal of the slicing plane used to create the
    /// spherical cap.  We also call this direction the "zenith direction".  When using polar
    /// coordinates in Dome Space, polar angles are measured relative to this reference direction.
    /// The forward (+Z) direction lies within the slicing plane and, subject to this constraint, points
    /// as closely as possible toward the Room Space forward direction, that is, the default direction
    /// the audience within the dome is facing.  When using polar coordinates in Dome Space, this
    /// direction is the reference direction for azimuth angles.  Starting a 0 degrees, Azimuth angles
    /// increase positively when looking to the right of forward, and negatively when looking to the left.
    /// The origin of Dome Space lies at the "center" of the sphere.
    /// 
    /// **Room Space**  Just like MinVR's conventions for AR/VR, we call the coordinate system defined
    /// by the physical room that the dome is in *Room Space*.  If the dome is oriented so that its
    /// slicing plane is parallel to the ground, then Room Space and Dome Space will be equivalent.
    /// However, many domes are tilted slightly relative to the phyical ground.  For example, in the
    /// Bell Planetarium, the dome is tilted 15 degrees so that the forward edge of the projection screen
    /// is 15 degrees below the horizon and the rear edge is 15 degrees above the horizon.  It can be
    /// useful to work in both Dome Space and Room Space.  If you want to position an object directly
    /// overhead, as in gravity would make it fall straight down onto your head, then the object should
    /// be offset above your head using the Room Space Up vector.  However, if you want an object to
    /// appear in the exact center of the dome's projection screen, then you should position that
    /// object along the Dome Space zenith direction.
    ///
    /// **Dome Space to Room Space Transform** Following MinVR's conventions, we recommend placing this
    /// script on a new GameObject called "Dome Space" that is a child of the MinVR "Room Space"
    /// GameObject.  That will mean that the transform for the GameObject this script is attached to
    /// is responsbile for transforming Dome Space into Room Space.  In other words, the local coordinate
    /// system for this GameObject is Dome Space, and the parent coordinate system for this GameObject
    /// is Room Space.  Then, any objects you attach as children of the "Dome Space" object will be
    /// positioned relative to the Dome Space coordinate system described above, and just like any other
    /// MinVR configuration, any objects you make direct children of the Room Space object, will be
    /// positioned in the Room Space coordinate system described above.
    ///
    /// **Spherical Coordinates** Internally, Unity uses rectangular (x,y,z) coordinates, so the positions
    /// and directions you use will need to be in rectangular coordiantes in order to, for example,
    /// position a GameObject by setting its transform.  However, when working with these dome spaces,
    /// it's often more convenient to work with spherical coordinates.  This class helps you do that
    /// and then convert the spherical coordinates to rectangular.  For example, to position an object
    /// directly on the surface of the dome, it is easiest to specify the center point for that object
    /// Dome Space polar coordinates (polar angle, azimuth angle, radius), but to actually position
    /// a Unity GameObject at that location you will want to:  1. Add the GameObject as a child of this
    /// "Dome Space" object.  2. Convert the Dome Space spherical coordinates to Dome Space rectangular
    /// coordinates using the DomeSpaceSphericalToRectangular() function, 3. Set your GameObject's
    /// localPosition equal to the result.  Example code:
    /// ```
    /// // Create a new GameObject
    /// GameObject objOnDomeSurface = new GameObject();
    /// // Make it a Dome Space object, i.e., a child of the GameObject this script is attached to
    /// objOnDomeSurface.transform.parent = this.transform.parent;
    ///
    /// // Define it's position in Dome Space spherical coordinates
    /// float polarAngle = 5.0f;
    /// float azimuthAngle = 20.0f;
    /// 
    /// // Convert these spherical coordinates to a x,y,z point on the surface of the dome
    /// Vector3 domeSpacePosition = PointOnDome(polarAngle, azimuthAngle);
    /// 
    /// // Assign the x,y,z to the local position of the GameObject
    /// objOnDomeSurface.transform.localPosition = domeSpacePosition;
    /// ```
    /// </summary>
    [ExecuteAlways]
    public class SphericalDome : MonoBehaviour
    {
        /// <summary>
        /// Radius of the spherical dome (in meters)
        /// </summary>
        public float domeRadius
        {
            get { return m_DomeRadius; }
            set
            {
                m_DomeRadius = value;
                m_DebugLinesDirty = true;
            }
        }

        /// <summary>
        /// 180 degrees if the dome is a full hemisphere; otherwise the total angle covered by
        /// the projection (in degrees), i.e., two times the polar angle from zenith down to the
        /// bottom edge of the projection.
        /// </summary>
        public float domeViewAngle
        {
            get { return m_DomeViewAngle; }
            set
            {
                m_DomeViewAngle = value;
                m_DebugLinesDirty = true;
            }
        }

        /// <summary>
        /// In the dome's spherical coordinate system, the polar angle is measured from the zenith
        /// direction, which points straight up toward the center point of the dome.  This is the
        /// maximum polar angle that can be displayed on the dome, i.e., graphics positioned at this
        /// angle will be drawn right on the edge of the spherical dome, any greater angle will make
        /// the graphics fall "off the screen".
        /// </summary>
        public float maxPolarAngleInView
        {
            get { return domeViewAngle / 2.0f; }
        }

        /// <summary>
        /// A spherical dome is like taking a ball and cutting through it with a plane, leaving a cap.
        /// The zenith direction points from the center of the original ball to the center point on
        /// the cap.  If we think of being inside a big ball and taking a slice through the ball that
        /// is parallel to the "ground", then the zenith direction is straight up.  However, if we
        /// then rotate the cap by, say, 15 degrees around the X-axis, as is often done in a planetarium
        /// with stadium seating, the zenith will be tilted in the same way.  Put another way, the
        /// zenith will always point from the center of the sphere toward the center of the projection
        /// screen.  In "dome space", this will always be the Up vector, but if the dome is tilted,
        /// that means "dome space" is rotated relative to the physical "room space".  In this case,
        /// the zenith will not be the same as the "room space" Up vector.
        /// </summary>
        public Vector3 zenith
        {
            get { return Vector3.up; }
        }


        /// <summary>
        /// The dome's forward direction in Dome Space coordinates.  (See documentation at the top of
        /// the class for more description.)
        /// </summary>
        public Vector3 forward
        {
            get { return Vector3.forward; }
        }

        public Vector3 CenterInWorldSpace()
        {
            return DomePointToWorldSpace(Vector3.zero);
        }

        public Vector3 DomePointToWorldSpace(Vector3 domePoint)
        {
            return transform.LocalPointToWorldSpace(domePoint);
        }

        public Vector3 DomeVectorToWorldSpace(Vector3 domeVector)
        {
            return transform.LocalVectorToWorldSpace(domeVector);
        }

        public Vector3 DomeDirectionToWorldSpace(Vector3 domeDirection)
        {
            return transform.LocalDirectionToWorldSpace(domeDirection);
        }


        public Vector3 WorldPointToDomeSpace(Vector3 worldPoint)
        {
            return transform.WorldPointToLocalSpace(worldPoint);
        }

        public Vector3 WorldVectorToDomeSpace(Vector3 worldVector)
        {
            return transform.WorldVectorToLocalSpace(worldVector);
        }

        public Vector3 WorldDirectionToDomeSpace(Vector3 worldDirection)
        {
            return transform.WorldDirectionToLocalSpace(worldDirection);
        }


        public Vector3 DomePointToRoomSpace(Vector3 domePoint)
        {
            return transform.LocalPointToRoomSpace(domePoint);
        }

        public Vector3 DomeVectorToRoomSpace(Vector3 domeVector)
        {
            return transform.LocalVectorToRoomSpace(domeVector);
        }

        public Vector3 DomeDirectionToRoomSpace(Vector3 domeDirection)
        {
            return transform.LocalDirectionToRoomSpace(domeDirection);
        }


        public Vector3 RoomPointToDomeSpace(Vector3 roomPoint)
        {
            return transform.RoomPointToLocalSpace(roomPoint);
        }

        public Vector3 RoomVectorToDomeSpace(Vector3 roomVector)
        {
            return transform.RoomVectorToLocalSpace(roomVector);
        }

        public Vector3 RoomDirectionToDomeSpace(Vector3 roomDirection)
        {
            return transform.RoomDirectionToLocalSpace(roomDirection);
        }

        /// <summary>
        /// Recall, the dome is a spherical cap, i.e., a portion of a sphere produced by slicing the
        /// sphere with a plane.  This function returns true if the point would lie anywhere inside
        /// the original sphere before it was sliced.
        /// </summary>
        public bool IsPointInsideSphere(Vector3 pointInDomeSpace)
        {
            return pointInDomeSpace.magnitude > domeRadius;
        }

        /// <summary>
        /// Recall, the dome is a spherical cap, i.e., a portion of a sphere produced by slicing the
        /// sphere with a plane.  This function returns true if the point would lie anywhere inside
        /// the portion of the sphere that was sliced by the plane to form the spherical cap.
        /// </summary>
        public bool IsPointInsideDome(Vector3 pointInDomeSpace)
        {
            SphericalCoordinate s = RectangularPointToSpherical(pointInDomeSpace);
            return (s.polarAngleInDeg <= maxPolarAngleInView) && (s.radialDist <= domeRadius);
        }

        /// <summary>
        /// Returns the Dome Space rectangular coordinates for the point on the dome sphere's surface
        /// uniquely defined by the two angles and the dome's radius.  Note, if the polar angle is greater
        /// than the max polar angle in view, the point will like on the dome's sphere, but will not
        /// be visible on the dome.
        /// </summary>
        public Vector3 PointOnSphere(float polarAngleInDeg, float azimuthalAngleInDeg)
        {
            var sPoint = new SphericalCoordinate(domeRadius, polarAngleInDeg, azimuthalAngleInDeg);
            return SphericalPointToRectangular(sPoint);
        }

        /// <summary>
        /// If the polarAngleInDeg is less than the maxPolarAngleInView, returns the Dome Space
        /// rectangular coordinates for the point on the dome's surface uniquely defined by the two
        /// angles and the dome's radius.  Otherwise, the polarAngleInDeg is clamped to the maximum
        /// value, and the closet point that is actually visible on the dome's screen is returned.
        /// </summary>
        public Vector3 PointOnDome(float polarAngleInDeg, float azimuthalAngleInDeg)
        {
            var pAngle = Mathf.Clamp(polarAngleInDeg, 0, maxPolarAngleInView);
            var sPoint = new SphericalCoordinate(domeRadius, pAngle, azimuthalAngleInDeg);
            return SphericalPointToRectangular(sPoint);
        }

        /// <summary>
        /// Returns the rotation that will orient a GameObject on the surface of the dome so that
        /// the GameObject's forward direction (it's local +Z) faces away from the center of the dome sphere
        /// and the GameObject's up direction (it's local +Y) faces as close to the zenith as possible.
        /// </summary>
        public Quaternion InwardFacingRotation(float polarAngleInDeg, float azimuthalAngleInDeg)
        {
            Vector3 p = PointOnSphere(polarAngleInDeg, azimuthalAngleInDeg);
            return Quaternion.LookRotation(p, Vector3.up);
        }

        /// <summary>
        /// Returns the rotation that will orient a GameObject on the surface of the dome so that
        /// the GameObject's forward direction (it's local +Z) faces away from the center of the dome sphere
        /// and the GameObject's up direction (it's local +Y) faces as close to the zenith as possible.
        /// </summary>
        public Quaternion InwardFacingRotation(Vector3 pointInDomeSpace)
        {
            return Quaternion.LookRotation(pointInDomeSpace, Vector3.up);
        }

        /// <summary>
        /// Given a Dome Space point, p, that does not necessarily lie on the surface of the Dome's,
        /// sphere, returns the closest point to p that is on the surface of the sphere.
        /// </summary>
        public Vector3 ClosestPointOnSphere(Vector3 p)
        {
            SphericalCoordinate s = RectangularPointToSpherical(p);
            return PointOnSphere(s.polarAngleInDeg, s.azimuthalAngleInDeg);
        }

        /// <summary>
        /// Given a Dome Space point, p, that does not necessarily lie on the surface of the Dome,
        /// returns the closest point to p that is on the visible, projection-screen surface of the dome.
        /// </summary>
        public Vector3 ClosestPointOnDome(Vector3 p)
        {
            SphericalCoordinate s = RectangularPointToSpherical(p);
            return PointOnDome(s.polarAngleInDeg, s.azimuthalAngleInDeg);
        }


        /// <summary>
        /// Returns a random point on the surface of the dome.
        /// </summary>
        public Vector3 RandomPointOnDome()
        {
            if (m_Random == null)
            {
                m_Random = new System.Random();
            }
            float pa = (float)m_Random.NextDouble() * maxPolarAngleInView;
            float az = (float)m_Random.NextDouble() * 360.0f - 180.0f;
            return PointOnDome(pa, az);
        }


        /// <summary>
        /// Converts points in Dome Space spherical coordinates to Dome Space rectangular coordinates.
        /// </summary>
        public Vector3 SphericalPointToRectangular(SphericalCoordinate sPoint)
        {
            float paRad = Mathf.Deg2Rad * (90.0f - sPoint.polarAngleInDeg);
            float azRad = Mathf.Deg2Rad * sPoint.azimuthalAngleInDeg;

            // this offset puts azimuthAngle=0 directly forward
            float azOffset = Mathf.PI / 2.0f;

            Vector3 position = new Vector3();
            position.x = Mathf.Cos(paRad) * Mathf.Cos(-azRad + azOffset);
            position.y = Mathf.Sin(paRad);
            position.z = Mathf.Cos(paRad) * Mathf.Sin(-azRad + azOffset);
            return sPoint.radialDist * position;
        }

        /// <summary>
        /// Converts points in Dome Space rectangular coordinates to Dome Space spherical coordinates.
        /// </summary>
        public SphericalCoordinate RectangularPointToSpherical(Vector3 rPoint)
        {
            float r = rPoint.magnitude;
            float pa = Mathf.Rad2Deg * Mathf.Acos(rPoint.y / r);
            float az = Mathf.Rad2Deg * Mathf.Atan2(rPoint.x, rPoint.z);
            return new SphericalCoordinate(r, pa, az);
        }




        private void Start()
        {
            m_DebugLinesDirty = true;
            m_RoomSpaceOrigin = FindObjectOfType<RoomSpaceOrigin>();
            Debug.Assert(m_RoomSpaceOrigin != null, "MinVR requires that there is one GameObject in the scene marked as the Room Space Origin by attaching a RoomSpaceOrigin component.");
        }

        private void Update()
        {
            if (m_DebugLinesDirty)
            {
                RebuildDebugLines();
            }
        }

        private void RebuildDebugLines()
        {
            // wipe out any previous geometry
            if (m_LinesParent != null)
            {
                DestroyImmediate(m_LinesParent);
            }

            if (m_ShowDebugLines)
            {

                if (m_DebugLinesMaterial == null)
                {
                    // create a tmp gameobject to get access to a default material
                    GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    m_DebugLinesMaterial = tmp.GetComponent<MeshRenderer>().sharedMaterial;
                    DestroyImmediate(tmp);
                }

                m_LinesParent = new GameObject(k_DebugLinesGOName);

                // LINES OF EQUAL POLAR ANGLE (STACKS)
                // polarAngle = 0 is at the top center of the dome.
                // the angle increased going down from that point until it reaches maxPolarAngleInView,
                // which would be 90 deg for a dome with 180 deg view angle.

                for (float pa = 0.0f; pa <= maxPolarAngleInView; pa += 10.0f)
                {
                    GameObject go = new GameObject("Polar Angle " + pa);

                    go.transform.SetParent(m_LinesParent.transform);
                    LineRenderer lr = go.AddComponent<LineRenderer>();
                    lr.receiveShadows = false;
                    lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    lr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    lr.useWorldSpace = false;
                    lr.loop = true;
                    lr.generateLightingData = false;
                    lr.sharedMaterial = m_DebugLinesMaterial;
                    lr.startWidth = m_DegugLinesWidth;
                    lr.endWidth = m_DegugLinesWidth;

                    Vector3[] positions = new Vector3[36];
                    int i = 0;
                    // az = Azimuth Angle 0 to 360
                    for (float az = 0.0f; az < 360.0f; az += 10.0f)
                    {
                        positions[i] = PointOnDome(pa, az);
                        i++;
                    }
                    lr.positionCount = positions.Length;
                    lr.SetPositions(positions);
                }


                // LINES OF EQUAL AZIMUTH ANGLE (SLICES)
                for (float az = -170.0f; az <= 180.0f; az += 10.0f)
                {
                    GameObject go = new GameObject("Azimuth Angle " + az);
                    go.transform.SetParent(m_LinesParent.transform);
                    LineRenderer lr = go.AddComponent<LineRenderer>();
                    lr.receiveShadows = false;
                    lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    lr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    lr.useWorldSpace = false;
                    lr.loop = false;
                    lr.generateLightingData = false;
                    lr.sharedMaterial = m_DebugLinesMaterial;
                    lr.startWidth = m_DegugLinesWidth;
                    lr.endWidth = m_DegugLinesWidth;

                    int nPositions = (int)Mathf.Floor(maxPolarAngleInView / 10.0f) + 1;
                    Vector3[] positions = new Vector3[nPositions];
                    int i = 0;
                    for (float pa = 0.0f; pa <= maxPolarAngleInView; pa += 10.0f)
                    {
                        positions[i] = PointOnDome(pa, az);
                        i++;
                    }
                    lr.positionCount = positions.Length;
                    lr.SetPositions(positions);
                }

                // add to the hierarchy under the Dome Space object
                m_LinesParent.transform.SetParent(transform, false);
            }
            m_DebugLinesDirty = false;
        }


        void OnValidate()
        {
            m_DebugLinesDirty = true;
        }

        private void Reset()
        {
            m_DomeRadius = 8.0f;
            m_DomeViewAngle = 180.0f;
            m_ShowDebugLines = true;
            m_DegugLinesWidth = 0.05f;
            m_DebugLinesMaterial = null;
            m_DebugLinesDirty = true;
        }

        const string k_DebugLinesGOName = "Debug Lines [Generated]";

        // serialized member vars
        [Header("Dome Geometry")]
        [Tooltip("Radius of the dome (in meters).")]
        [Range(0, 100)]
        [SerializeField] private float m_DomeRadius = 8.0f;
        [Tooltip("The total angle covered by the projection (in degrees); 180 for a hemisphere dome.")]
        [Range(0, 360)]
        [SerializeField] private float m_DomeViewAngle = 180.0f;

        [Header("Calibration Aids")]
        [Tooltip("Draws grid lines across the surface of the dome.")]
        [SerializeField] private bool m_ShowDebugLines = true;
        [Tooltip("The thickness of the grid lines.")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float m_DegugLinesWidth = 0.05f;
        [Tooltip("The material for the lines, Unity's default material is used.")]
        [SerializeField] public Material m_DebugLinesMaterial;

        [SerializeField, HideInInspector] private GameObject m_LinesParent;

        // runtime only member vars
        private RoomSpaceOrigin m_RoomSpaceOrigin;
        private bool m_DebugLinesDirty;
        private System.Random m_Random;
    }

}
