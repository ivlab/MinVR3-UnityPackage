using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{
    [AddComponentMenu("MinVR/Interaction/Bimanual Object Selector")]
    public class BimanualObjectSelector : BasicObjectSelector
    {
        public override void Reset()
        {
            m_CursorTransform2 = transform;
            m_SelectionMode2 = SelectionMode.Touch;
            m_IgnoreMask2 = new LayerMask();
            m_TouchRadius2 = 0.01f;
            m_PointerDistance2 = 1.0f;
            base.Reset();
        }


        public override void Update()
        {
            Collider nowSelected1 = CheckSelection(m_CursorTransform, m_SelectionMode, m_IgnoreMask, m_TouchRadius, m_PointerDistance);
            Collider nowSelected2 = CheckSelection(m_CursorTransform2, m_SelectionMode2, m_IgnoreMask2, m_TouchRadius2, m_PointerDistance2);

            if ((nowSelected1 != null) && (nowSelected1 == m_Selected)) {
                // an existing selection from cursor1 is still valid; do nothing
            } else if ((nowSelected2 != null) && (nowSelected2 == m_Selected)) {
                // an existing selection from cursor2 is still valid; do nothing
            } else if ((nowSelected1 != null) || (nowSelected2 != null)) {
                // there is a new selection from (at least) one cursor; try to change the selection to (either one of the) newly selected object(s)
                bool changed = false;
                if (nowSelected1 != null) {
                    changed = TryChangeSelection(nowSelected1);
                }
                if ((!changed) && (nowSelected2 != null)) {
                    TryChangeSelection(nowSelected2);
                }
            } else if (m_Selected != null) {
                // nothing is selected right now, but something was selected in the previous frame, so try to change the selection to null
                TryChangeSelection(null);
            } // else nothing is selected and m_Selected is already null; do nothing
        }


        [Header("Cursor 2")]
        [Tooltip("The transform for the cursor used to make selections. Position is used for touch mode.  Position and rotation are used for point mode.")]
        [SerializeField] private Transform m_CursorTransform2;
        [Tooltip("Point mode uses the transform's position and forward direction to create a laser pointer.  Touch mode uses the transform's position and a small radius to create an sphere collider.")]
        [SerializeField] private SelectionMode m_SelectionMode2 = SelectionMode.Touch;
        [Tooltip("Objects in these layers will NOT be selected.")]
        [SerializeField] private LayerMask m_IgnoreMask2 = new LayerMask();
        [Tooltip("Radius for the sphere collider used in touch mode, can be zero in which case the cursor origin must lie totally inside the collider.")]
        [SerializeField] private float m_TouchRadius2 = 0.01f;
        [Tooltip("Distance for the raycast used in the pointat mode.")]
        [SerializeField] private float m_PointerDistance2 = 1.0f;
    }

}
