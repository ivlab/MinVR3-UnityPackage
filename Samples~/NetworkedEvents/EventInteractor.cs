using UnityEngine;

namespace IVLab.MinVR3.Samples
{
    public class EventInteractor : MonoBehaviour, IVREventListener
    {
        [SerializeField, Tooltip("VR Event Connection to send events to")]
        private IVREventConnection eventConnection;

        [SerializeField, Tooltip("Event to control the position of the cursor")]
        private VREventPrototypeVector2 positionEvent;
    
        [SerializeField, Tooltip("Event to control the color of the cursor")]
        private VREventPrototypeVector4 colorEvent;

        private Vector3 previousPosition;

        void Start()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
        }

        // Called when a new VREvent appears in the event queue
        public void OnVREvent(VREvent evt)
        {
            if (evt.Matches(positionEvent))
            {
                Vector2 data = evt.GetData<Vector2>();
                this.transform.position = new Vector3(data.x, 0.0f, data.y);
            }
            else if (evt.Matches(colorEvent))
            {
                Vector4 colorVec = evt.GetData<Vector4>();
                Color color = new Color(colorVec.x, colorVec.y, colorVec.z, colorVec.w);
                this.GetComponent<Renderer>().material.color = color;
            }
        }

        public void StartListening() { }
        public void StopListening() { }
    }
}