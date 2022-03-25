using System;
using UnityEngine;

namespace IVLab.MinVR3
{

    public class MainPaintingAndReframingUI : MonoBehaviour
    {
        // PAINTING STATE CALLBACKS

        public void Painting_OnUpdate()
        {

            // Find way for continous line
            // get previous postion and current position
            // draw triangles between them
            GameObject paintBlobOne;

            if (paintBlobPrefabOne != null) {
                paintBlobOne = Instantiate(paintBlobPrefabOne);
                Material tmpMat = new Material(paintBlobOne.GetComponent<Renderer>().sharedMaterial);
                float r = (float)(brushCursor.transform.position.x - Math.Truncate(brushCursor.transform.position.x));
                float g = (float)(brushCursor.transform.position.y - Math.Truncate(brushCursor.transform.position.y));
                float b = (float)(brushCursor.transform.position.z - Math.Truncate(brushCursor.transform.position.z));
                tmpMat.color = new Color(r,g,b);
                paintBlobOne.GetComponent<Renderer>().sharedMaterial = tmpMat;
            } else {
                paintBlobOne = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }
            paintBlobOne.transform.parent = artworkParent.transform;
            paintBlobOne.transform.position = brushCursor.transform.position;
            paintBlobOne.transform.rotation = brushCursor.transform.rotation;
            Vector3 sOne = paintBlobOne.transform.localScale;
            sOne.Scale(brushCursor.transform.localScale);
            paintBlobOne.transform.localScale = sOne;
            
            
            GameObject paintBlobTwo;

            if (paintBlobPrefabTwo != null) {
                paintBlobTwo = Instantiate(paintBlobPrefabTwo);
                Material tmpMat = new Material(paintBlobTwo.GetComponent<Renderer>().sharedMaterial);
                float r = (float)(brushCursor.transform.position.x - Math.Truncate(brushCursor.transform.position.x));
                float g = (float)(brushCursor.transform.position.y - Math.Truncate(brushCursor.transform.position.y));
                float b = (float)(brushCursor.transform.position.z - Math.Truncate(brushCursor.transform.position.z));
                tmpMat.color = new Color(r,g,b);
                paintBlobTwo.GetComponent<Renderer>().sharedMaterial = tmpMat;
            } else {
                paintBlobTwo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            }
            

            paintBlobTwo.transform.parent = artworkParent.transform;
            paintBlobTwo.transform.position = brushCursor.transform.position;
            paintBlobTwo.transform.rotation = brushCursor.transform.rotation;
            Vector3 sTwo = paintBlobTwo.transform.localScale;
            sTwo.Scale(brushCursor.transform.localScale);
            paintBlobTwo.transform.localScale = sTwo;


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
        public GameObject paintBlobPrefabOne;
        public GameObject paintBlobPrefabTwo;


        public GameObject brushCursor;
        public GameObject handCursor;

        private Vector3 m_LastHandPos;
        private Quaternion m_LastHandRot;

        private Vector3 m_LastBrushPos;

    }

} // namespace
