using UnityEngine;

namespace IVLab.MinVR3.ExtensionMethods
{
    public static class Matrix4x4Extensions
    {

        /// <summary>
        /// Does not do a full matrix decomposition.  Assumes m is a simple transformation matrix that was
        /// created by composing a scale then a rotation then a translation, like the way Unity builds its
        /// transformations from these three components.  This means there is no shearing in the matrix.
        /// Also assumes the matrix contains only positive scale factors in x, y, z.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Vector3 GetTranslationFast(this Matrix4x4 m)
        {
            return m.GetColumn(3);
        }


        /// <summary>
        /// Does not do a full matrix decomposition.  Assumes m is a simple transformation matrix that was
        /// created by composing a scale then a rotation then a translation, like the way Unity builds its
        /// transformations from these three components.  This means there is no shearing in the matrix.
        /// Also assumes the matrix contains only positive scale factors in x, y, z.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Quaternion GetRotationFast(this Matrix4x4 m)
        {
            Vector3 forward = m.GetColumn(2);
            forward.Normalize();
            Vector3 up = m.GetColumn(1);
            up.Normalize();
            return Quaternion.LookRotation(forward, up);
        }


        /// <summary>
        /// Does not do a full matrix decomposition.  Assumes m is a simple transformation matrix that was
        /// created by composing a scale then a rotation then a translation, like the way Unity builds its
        /// transformations from these three components.  This means there is no shearing in the matrix.
        /// Also assumes the matrix contains only positive scale factors in x, y, z.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Vector3 GetScaleFast(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }
    }
}