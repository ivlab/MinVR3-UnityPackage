using System;
using UnityEngine;

namespace IVLab.MinVR3
{

    public class MainPaintingAndReframingUI : MonoBehaviour
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
            Vector3 s = paintBlob.transform.localScale;
            s.Scale(brushCursor.transform.localScale);
            paintBlob.transform.localScale = s;
        }


        // TRANS-ROT-ARTWORK STATE CALLBACKS

        public void TransRotArtwork_OnEnter()
        {
            m_LastHandPos = handCursor.transform.position;
            m_LastHandRot = handCursor.transform.rotation;
        }

        public void TransRotArtwork_OnUpdate()
        {
            Vector3 handPosWorld = handCursor.transform.position;
            Vector3 deltaPosWorld = handPosWorld - m_LastHandPos;

            Quaternion handRotWorld = handCursor.transform.rotation;
            Quaternion deltaRotWorld =  handRotWorld * Quaternion.Inverse(m_LastHandRot);

            if (artworkParent != null) {
                artworkParent.transform.TranslateByWorldVector(deltaPosWorld);
                artworkParent.transform.RotateAroundWorldPoint(handPosWorld, deltaRotWorld);
            }

            m_LastHandPos = handPosWorld;
            m_LastHandRot = handRotWorld;
        }


        // SCALE-ARTWORK STATE CALLBACKS

        public void ScaleArtwork_OnEnter()
        {
            m_LastBrushPos = brushCursor.transform.position;
        }

        public void ScaleArtwork_OnUpdate()
        {
            Vector3 handPosWorld = handCursor.transform.position;
            Vector3 brushPosWorld = brushCursor.transform.position;
            Vector3 curSpan = handPosWorld - brushPosWorld;
            Vector3 lastSpan = m_LastHandPos - m_LastBrushPos;

            float deltaScale = curSpan.magnitude / lastSpan.magnitude;
            artworkParent.transform.ScaleAroundWorldPoint(handPosWorld, deltaScale);

            m_LastHandPos = handPosWorld;
            m_LastBrushPos = brushPosWorld;
        }


        [Tooltip("Parent GameObject for any 3D geometry produced by painting")]
        public GameObject artworkParent;

        [Tooltip("Prefab for a single `paint blob' deposited as the brush moves around")]
        public GameObject paintBlobPrefab;

        public GameObject brushCursor;
        public GameObject handCursor;

        private Vector3 m_LastHandPos;
        private Quaternion m_LastHandRot;

        private Vector3 m_LastBrushPos;

    }

} // namespace
