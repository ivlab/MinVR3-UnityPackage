using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace IVLab.MinVR3
{
    [RequireComponent(typeof(FSM))]
    public class PaintingTool : VRTool
    {
        // PAINTING STATE CALLBACKS

        public void Painting_OnUpdate()
        {
            GameObject paintSplat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            paintSplat.transform.parent = artworkParent.transform;
            paintSplat.transform.position = brushCursor.transform.position;
            paintSplat.transform.rotation = brushCursor.transform.rotation;
            paintSplat.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
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


        private void Start()
        {
            m_FSM = GetComponent<FSM>();
        }

        public override void OnInputAction(InputAction.CallbackContext context)
        {
            m_FSM.OnInputAction(context);
        }

        public override bool CanReleaseFocus()
        {
            return m_FSM.currentStateID == m_FSM.GetStateID("START");
        }

        [Tooltip("Parent GameObject for any 3D geometry produced by painting")]
        public GameObject artworkParent;

        public GameObject brushCursor;
        public GameObject handCursor;

        private Vector3 m_LastHandPos;
        private Quaternion m_LastHandRot;

        private FSM m_FSM;
    }

} // namespace
