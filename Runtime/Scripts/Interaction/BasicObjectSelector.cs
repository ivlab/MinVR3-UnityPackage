using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    [AddComponentMenu("MinVR/Interaction/Basic Object Selector")]
    public class BasicObjectSelector : MonoBehaviour, IVREventProducer
    {
        public Transform CursorTransform { get => m_CursorTransform; set => m_CursorTransform = value; }

        public enum SelectionMode
        {
            PointAt, // uses the transform's position and forward direction to create a laser pointer
            Touch    // uses the transform's position and a small radius to create an sphere collider
        }

        public virtual void Reset()
        {
            m_CursorTransform = transform;
            m_SelectionMode = SelectionMode.Touch;
            m_IgnoreMask = new LayerMask();
            m_TouchRadius = 0.01f;
            m_PointerDistance = 1.0f;
            m_SelectEventName = "Select";
            m_DeselectEventName = "Deselect";
        }


        void Start()
        {
            m_Selected = null;
        }

        public virtual void Update()
        {
            Collider nowSelected = CheckSelection(m_CursorTransform, m_SelectionMode, m_IgnoreMask, m_TouchRadius, m_PointerDistance);

            if (nowSelected != null) {
                if (nowSelected != m_Selected) {
                    TryChangeSelection(nowSelected);
                } // else nowSelected == m_Selected, so the selection has not changed; do nothing
            } else { // nowSelected == null
                if (m_Selected != null) {
                    TryChangeSelection(null);
                } // else m_Selected == null, so the selection has not changed; do nothing
            }
        }


        protected bool TryChangeSelection(Collider nowSelected)
        {
            if ((m_RequireToken == null) || (m_RequireToken.RequestToken(this))) {
                if (nowSelected != null) {
                    if (m_Selected != null) {
                        VREngine.Instance.eventManager.InsertInQueue(new VREventGameObject(m_DeselectEventName, m_Selected.gameObject));
                    }
                    VREngine.Instance.eventManager.InsertInQueue(new VREventGameObject(m_SelectEventName, nowSelected.gameObject));
                } else {
                    VREngine.Instance.eventManager.InsertInQueue(new VREventGameObject(m_DeselectEventName, m_Selected.gameObject));
                }
                m_Selected = nowSelected;
                if (m_RequireToken != null) {
                    // return the token immediately
                    m_RequireToken.ReleaseToken(this);
                }
                return true;
            } else {
                // could not acquire the token, so could not change the selection
                return false;
            }
        }
        

        protected Collider CheckSelection(Transform cursorTransform, SelectionMode selectionMode, LayerMask ignoreMask, float touchRadius, float pointerDistance)
        {
            Collider nowSelected = null;
            if (selectionMode == SelectionMode.Touch) {
                Collider[] hitColliders = Physics.OverlapSphere(cursorTransform.position, touchRadius, ~ignoreMask, QueryTriggerInteraction.Ignore);
                foreach (var hitCollider in hitColliders) {
                    if (nowSelected == null) {
                        nowSelected = hitCollider;
                    } else if (hitCollider.bounds.extents.magnitude < nowSelected.bounds.extents.magnitude) {
                        nowSelected = hitCollider;
                    }
                }

            } else if (selectionMode == SelectionMode.PointAt) {
                RaycastHit hit;
                if (Physics.Raycast(cursorTransform.position, cursorTransform.TransformDirection(Vector3.forward), out hit, pointerDistance, ~ignoreMask, QueryTriggerInteraction.Ignore)) {
                    nowSelected = hit.collider;
                    //Debug.DrawRay(cursorTransform.position, cursorTransform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                } else {
                    //Debug.DrawRay(cursorTransform.position, cursorTransform.TransformDirection(Vector3.forward) * 1000, Color.white);
                }
            }
            return nowSelected;
        }
            

        public List<IVREventPrototype> GetEventPrototypes()
        {
            var generated = new List<IVREventPrototype>();
            generated.Add(VREventPrototypeGameObject.Create(m_SelectEventName));
            generated.Add(VREventPrototypeGameObject.Create(m_DeselectEventName));
            return generated;
        }


        [Tooltip("[Optional] If set, selections will only change if the token can be aquired")]
        [SerializeField] private SharedToken m_RequireToken;
        [Tooltip("Name of the VREventGameObject that will be generated when a Collider is selected")]
        [SerializeField] private string m_SelectEventName = "Select";
        [Tooltip("Name of the VREventGameObject that will be generated when a Collider is deselected")]
        [SerializeField] private string m_DeselectEventName = "Deselect";

        [Header("Cursor")]
        [Tooltip("The transform for the cursor used to make selections. Position is used for touch mode.  Position and rotation are used for point mode.")]
        [SerializeField] protected Transform m_CursorTransform;
        [Tooltip("Point mode uses the transform's position and forward direction to create a laser pointer.  Touch mode uses the transform's position and a small radius to create an sphere collider.")]
        [SerializeField] protected SelectionMode m_SelectionMode = SelectionMode.Touch;
        [Tooltip("Objects in these layers will NOT be selected.")]
        [SerializeField] protected LayerMask m_IgnoreMask = new LayerMask();
        [Tooltip("Radius for the sphere collider used in touch mode, can be zero in which case the cursor origin must lie totally inside the collider.")]
        [SerializeField] protected float m_TouchRadius = 0.01f;
        [Tooltip("Distance for the raycast used in the pointat mode.")]
        [SerializeField] protected float m_PointerDistance = 1.0f;

        // runtime only
        protected Collider m_Selected;
    }

} // end namespace
