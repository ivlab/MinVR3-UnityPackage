using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    [AddComponentMenu("MinVR/Interaction/Bounds Highlighter")]
    public class BoundsHighlighter : MonoBehaviour, IVREventListener
    {

        public VREventPrototypeGameObject m_SelectEvent;
        public VREventPrototypeGameObject m_UnselectEvent;

        public Color highlightColor = Color.green;
        public float boundsLineWidth = 0.001f;
        private Material m_OrigMaterial;
        private HashSet<BoxCollider> colliders;

        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_SelectEvent))
            {
                GameObject go = vrEvent.GetData<GameObject>();
                BoxCollider collider = go.GetComponent<BoxCollider>();
                colliders.Add(collider);
            }
            else if (vrEvent.Matches(m_UnselectEvent))
            {
                GameObject go = vrEvent.GetData<GameObject>();
                BoxCollider collider = go.GetComponent<BoxCollider>();
                colliders.Remove(collider);
            }
        }

        void Update()
        {
            foreach (BoxCollider c in colliders)
            {
                DebugDraw.Bounds(new Bounds(c.center, c.size), highlightColor, c.transform.localToWorldMatrix, thickness: boundsLineWidth);
            }
        }

        void Start()
        {
            colliders = new HashSet<BoxCollider>();
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
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        public void StopListening()
        {
            VREngine.Instance?.eventManager?.RemoveEventListener(this);
        }
    }

} // end namespace
