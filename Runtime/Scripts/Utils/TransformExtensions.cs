using UnityEngine;

namespace IVLab.MinVR3
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

        public static float LocalLengthToWorldSpace(this Transform t, float localLength)
        {
            // assumes uniform scale between t and world
            return t.LocalVectorToWorldSpace(new Vector3(localLength, 0, 0)).magnitude;
        }

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

        public static Quaternion LocalRotationToWorldSpace(this Transform t, Quaternion localRotation)
        {
            // TODO: Test this
            Quaternion R = localRotation;
            Transform p = t;
            while (p) {
                R = p.localRotation * R;
                p = p.parent;
            }
            return R;
        }


        // WORLD TO LOCAL

        public static float WorldLengthToLocalSpace(this Transform t, float worldLength)
        {
            // assumes uniform scale between t and world
            return t.WorldVectorToLocalSpace(new Vector3(worldLength, 0, 0)).magnitude;
        }

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

        public static Quaternion WorldRotationToLocalSpace(this Transform t, Quaternion worldRotation)
        {
            // TODO: Test this
            return Quaternion.Inverse(t.rotation) * worldRotation;
        }


        // LOCAL TO PARENT

        public static float LocalLengthToParentSpace(this Transform t, float localLength)
        {
            // assumes uniform scale between t and parent
            Debug.Assert(t.parent != null, "The transform must have a parent");
            return t.LocalVectorToParentSpace(new Vector3(localLength, 0, 0)).magnitude;
        }

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

        public static Quaternion LocalRotationToParentSpace(this Transform t, Quaternion localRotation)
        {
            // TODO: Test this
            Quaternion R = t.localRotation * localRotation;
            if (t.parent) {
                R = t.parent.localRotation * R;
            }
            return R;
        }


        // PARENT TO LOCAL

        public static float ParentLengthToLocalSpace(this Transform t, float parentLength)
        {
            // assumes uniform scale between t and world
            return t.ParentVectorToLocalSpace(new Vector3(parentLength, 0, 0)).magnitude;
        }

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

        public static Quaternion ParentRotationToLocalSpace(this Transform t, Quaternion parentRotation)
        {
            // TODO: Test this
            Debug.Assert(t.parent != null, "The transform must have a parent");
            return Quaternion.Inverse(t.localRotation) * parentRotation;
        }



        // Note: The functions above are useful in any Unity program.  Below this point, they are
        // specific to MinVR3 programs, where it is required that each scene have one object that
        // is marked as the root of Room Space by attaching a RoomSpaceOrigin component.

        // LOCAL TO ROOM (MINVR SPECIFIC)

        public static float LocalLengthToRoomSpace(this Transform t, float localLength)
        {
            IVLab.MinVR3.RoomSpaceOrigin roomSpaceOrigin = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin;
            return roomSpaceOrigin.WorldLengthToRoomSpace(t.LocalLengthToWorldSpace(localLength));
        }

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

        public static Quaternion LocalRotationToRoomSpace(this Transform t, Quaternion localRotation)
        {
            IVLab.MinVR3.RoomSpaceOrigin roomSpaceOrigin = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin;
            return roomSpaceOrigin.WorldRotationToRoomSpace(t.LocalRotationToWorldSpace(localRotation));
        }


        // ROOM TO LOCAL (MINVR SPECIFIC)

        public static float RoomLengthToLocalSpace(this Transform t, float roomLength)
        {
            IVLab.MinVR3.RoomSpaceOrigin roomSpaceOrigin = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin;
            return t.WorldLengthToLocalSpace(roomSpaceOrigin.RoomLengthToWorldSpace(roomLength));
        }

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

        public static Quaternion RoomRotationToLocalSpace(this Transform t, Quaternion roomRotation)
        {
            IVLab.MinVR3.RoomSpaceOrigin roomSpaceOrigin = IVLab.MinVR3.VREngine.instance.roomSpaceOrigin;
            return t.WorldRotationToLocalSpace(roomSpaceOrigin.RoomRotationToWorldSpace(roomRotation));
        }

    }

}