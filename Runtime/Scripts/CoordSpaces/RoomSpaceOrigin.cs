using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    public class RoomSpaceOrigin : MonoBehaviour
    {

        public Vector3 RoomPointToWorldSpace(Vector3 roomPoint)
        {
            return transform.LocalPointToWorldSpace(roomPoint);
        }

        public Vector3 RoomVectorToWorldSpace(Vector3 roomVector)
        {
            return transform.LocalVectorToWorldSpace(roomVector);
        }

        public Vector3 RoomDirectionToWorldSpace(Vector3 roomDirection)
        {
            return transform.LocalDirectionToWorldSpace(roomDirection);
        }


        public Vector3 WorldPointToRoomSpace(Vector3 worldPoint)
        {
            return transform.WorldPointToLocalSpace(worldPoint);
        }

        public Vector3 WorldVectorToRoomSpace(Vector3 worldVector)
        {
            return transform.WorldVectorToLocalSpace(worldVector);
        }

        public Vector3 WorldDirectionToRoomSpace(Vector3 worldDirection)
        {
            return transform.WorldDirectionToLocalSpace(worldDirection);
        }

    }

}
