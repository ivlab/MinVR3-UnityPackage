using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    [AddComponentMenu("MinVR/Interaction/Basic Object Selector")]
    public class BasicObjectSelector : MonoBehaviour, IVREventListener, IVREventProducer
    {
        public enum SelectionMode
        {
            PointAt, // uses the transform's position and forward direction to create a laser pointer
            Touch    // uses the transform's position and a small radius to create an sphere collider
        }

        public VREventPrototypeVector3 m_CursorPositionEvent;
        public VREventPrototypeQuaternion m_CursorRotationEvent;

        public SelectionMode m_SelectionMode;
        public LayerMask m_IgnoreMask;
        public float m_TouchRadius;
        public float m_PointerDistance;

        private Collider m_Selected;

        private void Reset()
        {
            m_SelectionMode = SelectionMode.PointAt;
            m_IgnoreMask = new LayerMask();
            m_TouchRadius = 0.1f;
            m_PointerDistance = 10.0f;
        }



        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_CursorPositionEvent)) {
                transform.localPosition = vrEvent.GetData<Vector3>();
                CheckSelection();
            } else if (vrEvent.Matches(m_CursorRotationEvent)) {
                transform.localRotation = vrEvent.GetData<Quaternion>();
                CheckSelection();
            }
        }

        void CheckSelection()
        {
            Collider nowSelected = null;
            if (m_SelectionMode == SelectionMode.Touch) {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, m_TouchRadius);
                foreach (var hitCollider in hitColliders) {
                    if (nowSelected == null) {
                        nowSelected = hitCollider;
                    } else if (hitCollider.bounds.extents.magnitude < nowSelected.bounds.extents.magnitude) {
                        nowSelected = hitCollider;
                    }
                }

            } else if (m_SelectionMode == SelectionMode.PointAt) {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, m_PointerDistance, ~m_IgnoreMask, QueryTriggerInteraction.Ignore)) {
                    nowSelected = hit.collider;
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
                } else {
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                }
            }

            if ((nowSelected != null) && (nowSelected != m_Selected)) {
                VREngine.instance.eventManager.InsertInQueue(new VREventGameObject("Select", nowSelected.gameObject));
            }
            if ((m_Selected != null) && (m_Selected != nowSelected)) {
                VREngine.instance.eventManager.InsertInQueue(new VREventGameObject("Deselect", m_Selected.gameObject));
            }
            m_Selected = nowSelected;
        }



        public List<IVREventPrototype> GetEventPrototypes()
        {
            var generated = new List<IVREventPrototype>();
            generated.Add(VREventPrototypeGameObject.Create("Select"));
            generated.Add(VREventPrototypeGameObject.Create("Deselect"));
            return generated;
        }


        void OnEnable()
        {
            StartListening();
        }

        void OnDisable()
        {
            StopListening();
        }

        public void StartListening()
        {
            VREngine.instance.eventManager.AddEventListener(this, VREventManager.DefaultListenerPriority - 1);
        }

        public void StopListening()
        {
            VREngine.instance?.eventManager?.RemoveEventListener(this);
        }

    }

} // end namespace
