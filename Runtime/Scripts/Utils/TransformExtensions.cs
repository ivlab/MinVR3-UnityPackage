using UnityEngine;

namespace IVLab.MinVR3.ExtensionMethods
{
    public static class TransformExtensions
    {
        public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.SetGlobalScale(matrix.GetScaleFast());
            transform.rotation = matrix.GetRotationFast();
            transform.position = matrix.GetPosition();
        }
        public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
        }
    }

}