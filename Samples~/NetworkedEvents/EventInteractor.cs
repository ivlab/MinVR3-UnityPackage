using UnityEngine;
using IVLab.Utilities;

namespace IVLab.MinVR3.Samples
{
    public class EventInteractor : MonoBehaviour, IVREventListener
    {
        [SerializeField, Tooltip("Event to control the position of the cursor")]
        private VREventPrototypeVector2 positionEvent;
    
        [SerializeField, Tooltip("Event to control the color of the cursor")]
        private VREventPrototypeVector4 colorEvent;

        [SerializeField, Tooltip("Current color of the cursor")]
        private Color cursorColor;

        private Vector3 previousPosition;
        private Color previousColor;
        private bool positionUpdatedFromConnection = false;
        private bool colorUpdatedFromConnection = false;

        void Start()
        {
            VREngine.Instance.eventManager.AddEventListener(this);
            cursorColor = this.GetComponent<Renderer>().material.color;
        }

        void Update()
        {
            Vector3 diff = this.transform.position - previousPosition;
            if (!Mathf.Approximately(diff.magnitude, 0.0f) && !positionUpdatedFromConnection)
            {
                VREngine.Instance.eventManager.QueueEvent(new VREventVector3("Cursor3D/Position", this.transform.position));
            }

            Vector4 colorVec = new Vector4(this.cursorColor.r, this.cursorColor.g, this.cursorColor.b, this.cursorColor.a);
            Vector4 prevColorVec = new Vector4(previousColor.r, previousColor.g, previousColor.b, previousColor.a);
            if (!Mathf.Approximately((colorVec - prevColorVec).magnitude, 0.0f) && !colorUpdatedFromConnection)
            {
                this.GetComponent<Renderer>().material.color = cursorColor;
                VREngine.Instance.eventManager.QueueEvent(new VREventVector4("Cursor3D/Color", colorVec));
            }

            previousPosition = this.transform.position;
            previousColor = this.cursorColor;
            positionUpdatedFromConnection = false;
            colorUpdatedFromConnection = false;
        }

        [FunctionDebugger]
        public void MakeEvent()
        {
            VREngine.Instance.eventManager.QueueEvent(new VREventVector2("TestEvent/Position", Vector2.one * Time.realtimeSinceStartup));
        }

        // Called when a new VREvent appears in the event queue
        public void OnVREvent(VREvent evt)
        {
            if (evt.Matches(positionEvent))
            {
                Vector2 data = evt.GetData<Vector2>();
                this.transform.position = new Vector3(data.x, 0.0f, data.y);
                positionUpdatedFromConnection = true;
            }
            else if (evt.Matches(colorEvent))
            {
                Vector4 colorVec = evt.GetData<Vector4>();
                cursorColor = new Color(colorVec.x, colorVec.y, colorVec.z, colorVec.w);
                this.GetComponent<Renderer>().material.color = cursorColor;
                colorUpdatedFromConnection = true;
            }
        }

        public void StartListening() { }
        public void StopListening() { }
    }
}