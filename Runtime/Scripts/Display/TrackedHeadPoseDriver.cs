using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    public class TrackedHeadPoseDriver : TrackedPoseDriver
    {
        [Tooltip("Distance between the eyes; default to 63mm for the average adult.")]
        public float interpupillaryDistance = 0.063f;


        public Vector3 GetHeadPositionInRoomSpace()
        {
            return transform.LocalPointToRoomSpace(Vector3.zero);
        }

        public Vector3 GetHeadPositionInWorldSpace()
        {
            return transform.LocalPointToWorldSpace(Vector3.zero);
        }

        public Vector3 GetLeftEyePositionInRoomSpace()
        {
            Vector3 leftEyeInLocalSpace = new Vector3(-0.5f * interpupillaryDistance, 0, 0);
            return transform.LocalPointToRoomSpace(leftEyeInLocalSpace);
        }

        public Vector3 GetLeftEyePositionInWorldSpace()
        {
            Vector3 leftEyeInLocalSpace = new Vector3(-0.5f * interpupillaryDistance, 0, 0);
            return transform.LocalPointToWorldSpace(leftEyeInLocalSpace);
        }

        public Vector3 GetRightEyePositionInRoomSpace()
        {
            Vector3 rightEyeInLocalSpace = new Vector3(0.5f * interpupillaryDistance, 0, 0);
            return transform.LocalPointToRoomSpace(rightEyeInLocalSpace);
        }

        public Vector3 GetRightEyePositionInWorldSpace()
        {
            Vector3 rightEyeInLocalSpace = new Vector3(0.5f * interpupillaryDistance, 0, 0);
            return transform.LocalPointToWorldSpace(rightEyeInLocalSpace);
        }


        public Vector3 GetLookDirInRoomSpace()
        {
            return Vector3.Normalize(transform.LocalVectorToRoomSpace(Vector3.forward));
        }

        public Vector3 GetLookDirInWorldSpace()
        {
            return Vector3.Normalize(transform.LocalVectorToWorldSpace(Vector3.forward));
        }
    }

}