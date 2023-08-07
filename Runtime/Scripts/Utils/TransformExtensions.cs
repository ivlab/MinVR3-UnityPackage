using UnityEngine;

namespace IVLab.MinVR3.ExtensionMethods
{
    public static class TransformExtensions
    {
        public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.SetGlobalScale(matrix.GetScaleFast());
            transform.rotation = matrix.GetRotationFast();
            transform.position = matrix.GetTranslationFast();
        }
        public static void SetGlobalScale(this Transform transform, Vector3 globalScale)
        {
            transform.localScale = Vector3.one;
            transform.localScale = new Vector3(globalScale.x / transform.lossyScale.x, globalScale.y / transform.lossyScale.y, globalScale.z / transform.lossyScale.z);
        }

        // ok
        public static void TranslateByLocalVector(this Transform t, Vector3 deltaTrans)
        {
            t.position += t.TransformVector(deltaTrans);
        }

        // ok
        public static void TranslateByWorldVector(this Transform t, Vector3 deltaTrans)
        {
            t.position += deltaTrans;
        }

        // ok
        public static void RotateAroundLocalOrigin(this Transform t, Quaternion deltaRot)
        {
            t.localRotation = t.localRotation * deltaRot;
        }

        // ok
        public static void RotateAroundWorldOrigin(this Transform t, Quaternion deltaRot)
        {
            t.position = deltaRot * t.position;
            t.rotation = t.rotation * (Quaternion.Inverse(t.rotation) * deltaRot * t.rotation);
        }

        // ok
        public static void RotateAroundLocalPoint(this Transform t, Vector3 point, Quaternion deltaRot)
        {
            float angle;
            Vector3 localAxis;
            deltaRot.ToAngleAxis(out angle, out localAxis);
            Vector3 worldAxis = t.TransformDirection(localAxis);
            Vector3 worldPoint = t.TransformPoint(point);
            t.RotateAround(worldPoint, worldAxis, angle); // unity built-in works with world axis angle
        }

        // ok
        public static void RotateAroundWorldPoint(this Transform t, Vector3 point, Quaternion deltaRot)
        {
            Vector3 worldPos = t.position;
            Vector3 dif = worldPos - point;
            dif = deltaRot * dif;
            worldPos = point + dif;
            t.position = worldPos;
            t.rotation = t.rotation * (Quaternion.Inverse(t.rotation) * deltaRot * t.rotation);
        }

        // ok
        public static void ScaleAroundLocalOrigin(this Transform t, float deltaScale)
        {
            t.localScale = t.localScale * deltaScale;
        }


        public static void ScaleAroundWorldOrigin(this Transform t, float deltaScale)
        {
            t.ScaleAroundWorldPoint(Vector3.zero, deltaScale);
        }

        // ok
        public static void ScaleAroundLocalPoint(this Transform t, Vector3 point, float deltaScale)
        {
            Vector3 pointAfterScale = point * deltaScale;
            Vector3 diff = point - pointAfterScale;
            t.TranslateByLocalVector(diff);
            t.localScale = t.localScale * deltaScale;
        }

        // ok
        public static void ScaleAroundWorldPoint(this Transform t, Vector3 point, float deltaScale)
        {
            t.ScaleAroundLocalPoint(t.InverseTransformPoint(point), deltaScale);
        }



        // LOCAL TO WORLD

        public static Vector3 LocalPointToWorldSpace(this Transform t, Vector3 localPoint)
        {
            return t.TransformPoint(localPoint);
        }

        public static Vector3 LocalVectorToWorldSpace(this Transform t, Vector3 localVector)
        {
            return t.TransformVector(localVector);
        }

        public static Vector3 LocalDirectionToWorldSpace(this Transform t, Vector3 localDirection)
        {
            return t.TransformDirection(localDirection);
        }


        // WORLD TO LOCAL

        public static Vector3 WorldPointToLocalSpace(this Transform t, Vector3 worldPoint)
        {
            return t.InverseTransformPoint(worldPoint);
        }

        public static Vector3 WorldVectorToLocalSpace(this Transform t, Vector3 worldVector)
        {
            return t.InverseTransformVector(worldVector);
        }

        public static Vector3 WorldDirectionToLocalSpace(this Transform t, Vector3 worldDirection)
        {
            return t.InverseTransformDirection(worldDirection);
        }


        // LOCAL TO PARENT

        public static Vector3 LocalPointToParentSpace(this Transform t, Vector3 localPoint)
        {
            Debug.Assert(t.parent != null, "The transform must have a parent");
            return t.parent.InverseTransformPoint(t.TransformPoint(localPoint));
        }

        public static Vector3 LocalVectorToParentSpace(this Transform t, Vector3 localVector)
        {
            Debug.Assert(t.parent != null, "The transform must have a parent");
            return t.parent.InverseTransformVector(t.TransformVector(localVector));
        }

        public static Vector3 LocalDirectionToParentSpace(this Transform t, Vector3 localDirection)
        {
            Debug.Assert(t.parent != null, "The transform must have a parent");
            return t.parent.InverseTransformDirection(t.TransformDirection(localDirection));
        }


        // PARENT TO LOCAL

        public static Vector3 ParentPointToLocalSpace(this Transform t, Vector3 parentPoint)
        {
            Debug.Assert(t.parent != null, "The transform must have a parent");
            return t.InverseTransformPoint(t.parent.TransformPoint(parentPoint));
        }

        public static Vector3 ParentVectorToLocalSpace(this Transform t, Vector3 parentVector)
        {
            Debug.Assert(t.parent != null, "The transform must have a parent");
            return t.InverseTransformVector(t.parent.TransformVector(parentVector));
        }

        public static Vector3 ParentDirectionToLocalSpace(this Transform t, Vector3 parentDirection)
        {
            Debug.Assert(t.parent != null, "The transform must have a parent");
            return t.InverseTransformDirection(t.parent.TransformDirection(parentDirection));
        }



        // Note: The functions above are useful in any Unity program.  Below this point, they are
        // specific to MinVR3 programs, where it is required that each scene have one object that
        // is marked as the root of Room Space by attaching a RoomSpaceOrigin component.

        // LOCAL TO ROOM (MINVR SPECIFIC)

        public static Vector3 LocalPointToRoomSpace(this Transform t, Vector3 localPoint)
        {
            IVLab.MinVR3.RoomSpaceOrigin roomSpaceOrigin = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin;
            return roomSpaceOrigin.WorldPointToRoomSpace(t.TransformPoint(localPoint));
        }

        public static Vector3 LocalVectorToRoomSpace(this Transform t, Vector3 localVector)
        {
            IVLab.MinVR3.RoomSpaceOrigin roomSpaceOrigin = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin;
            return roomSpaceOrigin.WorldVectorToRoomSpace(t.TransformVector(localVector));
        }

        public static Vector3 LocalDirectionToRoomSpace(this Transform t, Vector3 localDirection)
        {
            IVLab.MinVR3.RoomSpaceOrigin roomSpaceOrigin = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin;
            return roomSpaceOrigin.WorldDirectionToRoomSpace(t.TransformDirection(localDirection));
        }


        // ROOM TO LOCAL (MINVR SPECIFIC)

        public static Vector3 RoomPointToLocalSpace(this Transform t, Vector3 roomPoint)
        {
            IVLab.MinVR3.RoomSpaceOrigin roomSpaceOrigin = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin;
            return t.InverseTransformPoint(roomSpaceOrigin.RoomPointToWorldSpace(roomPoint));
        }

        public static Vector3 RoomVectorToLocalSpace(this Transform t, Vector3 roomVector)
        {
            IVLab.MinVR3.RoomSpaceOrigin roomSpaceOrigin = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin;
            return t.InverseTransformVector(roomSpaceOrigin.RoomVectorToWorldSpace(roomVector));
        }

        public static Vector3 RoomDirectionToLocalSpace(this Transform t, Vector3 roomDirection)
        {
            IVLab.MinVR3.RoomSpaceOrigin roomSpaceOrigin = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin;
            return t.InverseTransformDirection(roomSpaceOrigin.RoomDirectionToWorldSpace(roomDirection));
        }
    }

}