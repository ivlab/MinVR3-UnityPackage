using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.MinVR3
{

    [AddComponentMenu("MinVR/Interaction/Basic Highlighter")]
    public class BasicHighlighter : MonoBehaviour, IVREventListener
    {

        public VREventPrototypeGameObject m_SelectEvent;
        public VREventPrototypeGameObject m_UnselectEvent;

        public Material m_HighlightMaterial;
        private Material m_OrigMaterial;

        public void OnVREvent(VREvent vrEvent)
        {
            if (vrEvent.Matches(m_SelectEvent)) {
                GameObject go = vrEvent.GetData<GameObject>();
                Renderer r = go.GetComponent<Renderer>();
                if (r != null) {
                    m_OrigMaterial = r.sharedMaterial;
                    r.material = m_HighlightMaterial;
                }
            } else if (vrEvent.Matches(m_UnselectEvent)) {
                GameObject go = vrEvent.GetData<GameObject>();
                Renderer r = go.GetComponent<Renderer>();
                if (r != null) {
                    r.material = m_OrigMaterial;
                }
            }
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
            VREngine.instance.eventManager.AddEventListener(this);
        }

        public void StopListening()
        {
            VREngine.instance?.eventManager?.RemoveEventListener(this);
        }
    }

} // end namespace
