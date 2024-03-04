using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// This class includes useful routines for converting between 3D coordinate systems that follow
    /// different conventions for handedness and up/forward directions.
    ///
    ///
    /// # Brief background on Left-Handed vs. Right Handed Coordinates
    ///
    /// Many (most?) scientific datasets and modeling programs these days use right-handed coordinates
    /// but that is just a convention.  Mathematically speaking, it is just as reasonable to use left-
    /// handed coordinates, as Unity has done and several other major graphics packages have done in
    /// the past.  Luckily, you can convert between LH and RH coordinates.  The conversion itself is
    /// not too hard.  The hard part is that there are several valid ways to do the conversion so you
    /// need to know which conventions are used and stay consistent.  This class is intended to help
    /// with this by organizing all of the conversion routines here in one place.
    ///
    /// # Converting LH to RH: The big question is, which axis to negate?
    ///
    /// Converting between LH and RH coordinate systems requires negating one axis.  However, the
    /// choice of which axis to negate is arbitrary.  When Unity does these conversions automatically,
    /// like when you drag and drop a model file into your Unity project, it seems to have adopted
    /// the convention of negating the X axis (i.e., vertices with positive x values will have negative
    /// x values after importing).  So, in this class, we intentionally follow the same convention of
    /// negating X.  If you look for more info online, you will find examples that do the conversion
    /// by negating Y or Z.  All of these are completely valid, it's just a convention, and we'll
    /// try to be as consistent with Unity as possible by doing the same thing and negating x.
    ///
    /// # Converting between +Y=Up, +Z=Up, and other variations
    ///
    /// In addition to deciding to use a left or right-handed system, datasets and graphics toolkits
    /// often also adopt a *second* convention about which axis points "Up", which points "Forward", etc.
    /// This really has nothing to do with RH vs. LH decision.  You could decide to use left-handed
    /// with +Y=Up or right-handed with +Y=Up or right-handed with +Z=Up or something else.  However,
    /// once you pick two directions (say, Up and Forward), the third axis must follow either the right
    /// -hand rule or left-hand rule based on whether you are using right or left handed coordinates.
    /// Use the left or right hand as appropriate, start with your fingers all pointing toward +X,
    /// sweep your palm and curl your fingers toward +Y, and then your thumb should point in +Z.
    /// (Or, just Google for a picture if needed.)  Once you have converted from RH to LH or vice versa
    /// you can easily change the up, forward, and right directions by applying a regular old rotational
    /// transformation.  So, it can be helpful to think of the entire conversion in two steps.  If you
    /// have RH data with +Z up, then step 1 is convert the data to LH (this will at least make them
    /// viewable in Unity), then step 2 is to apply a rotation as you would for any other object in
    /// Unity so that the data's +Z will be up.  Since Unity has the convention that +Y is up, this
    /// rotation can just be a simple 90 degree rotation around the X axis.  Since +Z=Up is pretty
    /// common in a lot of our data, we include some routines for doing this kind of conversion with
    /// one function call, but if you look at the implementation, you'll see that it is accomplishing
    /// this by first converting RH to LH, then applying a rotation.
    ///
    ///
    /// # Commonly Encountered Coordinate Systems and Conventions
    ///
    /// ## Left-Handed, Y=Up, Z=Forward (Unity)
    ///     +X = Right
    ///     +Y = Up
    ///     +Z = Forward (Into the screen)
    ///
    /// ## Right-Handed, Y=Up, Z=Backward (MinGfx, ParaView, many others ...)
    ///   +X = Right
    ///   +Y = Up
    ///   +Z = Backward (Out of Screen)
    ///
    /// ## Right-Handed, Z=Up, Y=Forward (Blender, XROMM, Joint-Track, ...)
    ///   +X = Right
    ///   +Y = Forward (Into Screen)
    ///   +Z = Up
    ///
    /// </summary>
    public class CoordConversion
    {
        /// <summary>
        /// This small internal class defines a coordinate system convention based on knowing 3 things:
        ///   1. Whether right-handed or left-handed coordinates are used.
        ///   2. Which direction is considered "Up"
        ///   3. Which direction is considered "Forward"
        /// </summary>
        [System.Serializable]
        public class CoordSystem
        {
            public enum Handedness
            {
                LeftHanded,
                RightHanded
            }

            // note: no PosX or NegX becuase our convert RH <-> LH strategy negates the X coord, and not 100%
            // sure that the rotations will work correctly if the Forward or Up dirs are in the +/-X direction.
            // It would be great to add these after some testing (and adding special cases as needed) to make
            // sure they work.
            public enum Axis
            {
                PosY,
                NegY,
                PosZ,
                NegZ
            }

            public CoordSystem(Handedness h, Axis up, Axis forward)
            {
                handedness = h;
                upAxis = up;
                forwardAxis = forward;
            }

            public Handedness handedness;
            public Axis upAxis;
            public Axis forwardAxis;

            public Vector3 upVector {
                get { return AxisToVector(upAxis); }
            }

            public Vector3 forwardVector {
                get { return AxisToVector(forwardAxis); }
            }

            private Vector3 AxisToVector(Axis a)
            {
                if (a == Axis.PosY) return new Vector3(0, 1, 0);
                else if (a == Axis.PosZ) return new Vector3(0, 0, 1);
                else if (a == Axis.NegY) return new Vector3(0, -1, 0);
                else /*if (a == Axis.NegZ)*/ return new Vector3(0, 0, -1);
            }
        }


        /// <summary>
        /// Converts a point or vector defined according to the provided origCS coordinate system convention into
        /// a vector in Unity's coordinate system convention (left-handed, +Y = Up, +Z = Forward).
        /// </summary>
        public static Vector3 ToUnity(Vector3 origVector, CoordSystem origCS)
        {
            Vector3 v = origVector;
            // First swap the handedness of the vector if needed.  Use the negate x approach to stay consistent
            // with how Unity converts from RH to LH when loading RHed model files into the project in the editor.
            if (origCS.handedness != CoordSystem.Handedness.LeftHanded) {
                v = new Vector3(-v[0], v[1], v[2]);
            }
            // Now v is in LH coordinates.  If needed, apply a rotation to align the up and forward axes with
            // Unity's convention of up = +Y and forward = +Z.
            if ((origCS.upAxis != CoordSystem.Axis.PosY) || (origCS.forwardAxis != CoordSystem.Axis.PosZ)) {
                v = Quaternion.Inverse(Quaternion.LookRotation(origCS.forwardVector, origCS.upVector)) * v;
            }
            return v;
        }

        /// <summary>
        /// Converts a quaternion defined according to the provided origCS coordinate system convention into
        /// a vector in Unity's coordinate system convention (left-handed, +Y = Up, +Z = Forward).
        /// </summary>
        public static Quaternion ToUnity(Quaternion origQuat, CoordSystem origCS)
        {
            Quaternion q = origQuat;

            // First swap the handedness of the quaternion if needed.
            if (origCS.handedness != CoordSystem.Handedness.LeftHanded) {
                Vector3 axis;
                float angle;
                q.ToAngleAxis(out angle, out axis);
                angle = -angle;
                axis = ToUnity(axis, origCS);
                q = Quaternion.AngleAxis(angle, axis);
            }

            /*
            // First swap the handedness of the quaternion if needed.  Reference for how to do this:
            // https://gamedev.stackexchange.com/questions/129204/switch-axes-and-handedness-of-a-quaternion
            if (origCS.handedness != CoordSystem.Handedness.LeftHanded) {
                // Extract the axis (imaginary part) of the quaternion and use the negate x convention as
                // usual to swap handedness of this vector
                Vector3 imaginaryPart = new Vector3(-q.x, q.y, q.z);
                // In the new coordinate system the angle of rotation will take the opposite sign.  This leaves
                // the real part (w) unchanged since cos(theta) = cos(-theta) and negates the imaginary part,
                // since sin(theta) = -sin(-theta)
                q = new Quaternion(-imaginaryPart.x, -imaginaryPart.y, -imaginaryPart.z, q.w);
            }

            // Now q is in LH coordinates.  If needed apply a rotation to align the up and forward axes with
            // Unity's convention of up = +Y and forward = +Z.
            if ((origCS.upAxis != CoordSystem.Axis.PosY) || (origCS.forwardAxis != CoordSystem.Axis.PosZ)) {
                q = Quaternion.Inverse(Quaternion.LookRotation(origCS.forwardVector, origCS.upVector)) * q;
            }*/
            return q;
        }

        /// <summary>
        /// Converts a rigid body transformation matrix (no scaling) defined according to the provided
        /// origCS coordinate system convention into a transformation matrix in Unity's coordinate system
        /// convention (left-handed, +Y = Up, +Z = Forward).
        /// </summary>
        public static Matrix4x4 ToUnity(Matrix4x4 origMat, CoordSystem origCS)
        {
            Quaternion newRot = ToUnity(GetRotation(origMat), origCS);
            Vector3 newTrans = ToUnity(GetTranslation(origMat), origCS);
            Matrix4x4 newMatrix = Matrix4x4.identity;
            newMatrix.SetTRS(newTrans, newRot, Vector3.one);
            return newMatrix;
        }



        /// <summary>
        /// Converts a point or vector defined in Unity's coordinate system convention (left-handed,
        /// +Y = Up, +Z = Forward) into some new coordinate system convention.
        /// </summary>
        public static Vector3 FromUnity(Vector3 unityVector, CoordSystem newCS)
        {
            Vector3 v = unityVector;
            // First swap the handedness of the vector if needed.  Use the negate x approach to stay consistent
            // with how Unity converts from RH to LH when loading RHed model files into the project in the editor.
            if (newCS.handedness != CoordSystem.Handedness.LeftHanded) {
                v = new Vector3(-v[0], v[1], v[2]);
            }
            // Now v is in the correct handedness for the new CS.  If needed, apply a rotation to make Unity's
            // convention of up = +Y and forward = +Z line up with the convention used in newCS.
            if ((newCS.upAxis != CoordSystem.Axis.PosY) || (newCS.forwardAxis != CoordSystem.Axis.PosZ)) {
                v = Quaternion.LookRotation(newCS.forwardVector, newCS.upVector) * v;
            }
            return v;
        }

        /// <summary>
        /// Converts a quaternion defined in Unity's coordinate system convention (left-handed,
        /// +Y = Up, +Z = Forward) into some new coordinate system convention.
        /// </summary>
        public static Quaternion FromUnity(Quaternion unityQuat, CoordSystem newCS)
        {
            Quaternion q = unityQuat;

            // First swap the handedness of the quaternion if needed.
            if (newCS.handedness != CoordSystem.Handedness.LeftHanded) {
                Vector3 axis;
                float angle;
                q.ToAngleAxis(out angle, out axis);
                angle = -angle;
                axis = FromUnity(axis, newCS);
                q = Quaternion.AngleAxis(angle, axis);
            }

            /**
            // Reference for how to do this:
            // https://gamedev.stackexchange.com/questions/129204/switch-axes-and-handedness-of-a-quaternion
            if (newCS.handedness != CoordSystem.Handedness.LeftHanded) {
                // Extract the axis (imaginary part) of the quaternion and use the negate x convention as
                // usual to swap handedness of this vector
                Vector3 imaginaryPart = new Vector3(-q.x, q.y, q.z);
                // In the new coordinate system the angle of rotation will take the opposite sign.  This leaves
                // the real part (w) unchanged since cos(theta) = cos(-theta) and negates the imaginary part,
                // since sin(theta) = -sin(-theta)
                q = new Quaternion(-imaginaryPart.x, -imaginaryPart.y, -imaginaryPart.z, q.w);
            }

            // Now q is in the same handedness as newCS.  If needed apply a rotation to align the up and forward
            // axes with Unity's convention of up = +Y and forward = +Z.
            if ((newCS.upAxis != CoordSystem.Axis.PosY) || (newCS.forwardAxis != CoordSystem.Axis.PosZ)) {
                q = Quaternion.LookRotation(newCS.forwardVector, newCS.upVector) * q;
            }*/
            return q;
        }

        /// <summary>
        /// Converts a rigid body transformation matrix defined in Unity's coordinate system convention
        /// (left-handed, +Y = Up, +Z = Forward) into some new coordinate system convention.
        /// </summary>
        public static Matrix4x4 FromUnity(Matrix4x4 unityMat, CoordSystem newCS)
        {
            Quaternion newRot = ToUnity(GetRotation(unityMat), newCS);
            Vector3 newTrans = ToUnity(GetTranslation(unityMat), newCS);
            Matrix4x4 newMatrix = Matrix4x4.identity;
            newMatrix.SetTRS(newTrans, newRot, Vector3.one);
            return newMatrix;
        }


        /// <summary>
        /// Utility to convert the rotational component (upper 3x3) of a transformation matrix to a
        /// quaternion.  Assumes this is a simple matrix with no non-uniform scale so extracting the
        /// rotation is a simple operation.
        /// </summary>
        public static Quaternion GetRotation(Matrix4x4 m)
        {
            // column 2 is the Z axis, which is the Forward dir
            // column 1 is the Y axis, which is the Up dir
            Vector3 forward = new Vector3(m.GetColumn(2).x, m.GetColumn(2).y, m.GetColumn(2).z).normalized;
            Vector3 up = new Vector3(m.GetColumn(1).x, m.GetColumn(1).y, m.GetColumn(1).z).normalized;
            return Quaternion.LookRotation(forward, up);
        }

        /// <summary>
        /// Utility to convert the translational component (right column) of a transformation matrix to a
        /// vector.
        /// </summary>
        public static Vector3 GetTranslation(Matrix4x4 m)
        {
            // column 3 is the translational part of the matrix
            return m.GetColumn(3);
        }
    }

}
