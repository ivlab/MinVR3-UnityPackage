using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IVLab.MinVR3
{

    public class PaintingTool : MonoBehaviour
    {
        // PAINTING STATE CALLBACKS

        public void Painting_OnUpdate()
        {
            GameObject paintBlob;
            if (paintBlobPrefab != null) {
                paintBlob = Instantiate(paintBlobPrefab);
                Material tmpMat = new Material(paintBlob.GetComponent<Renderer>().sharedMaterial);
                float r = (float)(brushCursor.transform.position.x - Math.Truncate(brushCursor.transform.position.x));
                float g = (float)(brushCursor.transform.position.y - Math.Truncate(brushCursor.transform.position.y));
                float b = (float)(brushCursor.transform.position.z - Math.Truncate(brushCursor.transform.position.z));
                tmpMat.color = new Color(r,g,b);
                paintBlob.GetComponent<Renderer>().sharedMaterial = tmpMat;
            } else {
                paintBlob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }
            paintBlob.transform.parent = artworkParent.transform;
            paintBlob.transform.position = brushCursor.transform.position;
            paintBlob.transform.rotation = brushCursor.transform.rotation;
        }


        // REFRAMING STATE CALLBACKS

        public void Reframing_OnEnter()
        {
            m_LastHandPos = handCursor.transform.position;
            m_LastHandRot = handCursor.transform.rotation;
        }

        public void Reframing_OnUpdate()
        {
            Vector3 deltaPos = handCursor.transform.position - m_LastHandPos;
            Quaternion deltaRot = handCursor.transform.rotation * Quaternion.Inverse(m_LastHandRot);

            if (artworkParent != null) {
                artworkParent.transform.localPosition += deltaPos;
                artworkParent.transform.localRotation *= deltaRot;
            }

            m_LastHandPos = handCursor.transform.position;
            m_LastHandRot = handCursor.transform.rotation;
        }

        [Tooltip("Parent GameObject for any 3D geometry produced by painting")]
        public GameObject artworkParent;

        [Tooltip("Prefab for a single `paint blob' deposited as the brush moves around")]
        public GameObject paintBlobPrefab;

        public GameObject brushCursor;
        public GameObject handCursor;

        private Vector3 m_LastHandPos;
        private Quaternion m_LastHandRot;
    }

} // namespace
