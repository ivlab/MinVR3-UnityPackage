using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IVLab.MinVR3;

namespace IVLab.MinVR3.TUIO11 {
    public class sphereMove : MonoBehaviour, IVREventListener {
        //get sphere object
        //get coordinate
        //move sphere object based on coordinate -- this will require the dome conversion

        public VREventPrototypeVector2 touchDownEvent;
        public VREventPrototypeVector2 touchMoveEvent;
        public VREventPrototypeVector2 touchUpEvent;

        public GameObject movingSphere;

        public void OnVREvent(VREvent vrEvent) {
            if (vrEvent.Matches(touchDownEvent)) {
                movingSphere.SetActive(true);
                Vector2 xy = vrEvent.GetData<Vector2>();
                movingSphere.transform.position = new Vector3(xy.x, xy.y, 0);
            } else if (vrEvent.Matches(touchMoveEvent)) {
                Vector2 xy = vrEvent.GetData<Vector2>();
                movingSphere.transform.position = new Vector3(xy.x, xy.y, 0);
            } else if (vrEvent.Matches(touchUpEvent)) {
                movingSphere.SetActive(false);
            }
        }

        void OnEnable() {
            StartListening();
        }

        void OnDisable() {
            StopListening();
        }

        public void StartListening() {
            VREngine.instance.eventManager.AddEventListener(this);
        }

        public void StopListening() {
            VREngine.instance?.eventManager?.RemoveEventListener(this);
        }

        // Start is called before the first frame update
        void Start() {
            if (movingSphere == null) {
                movingSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                movingSphere.transform.SetParent(transform);
            }
            movingSphere.SetActive(false);
        }

        // Update is called once per frame
        void Update() {
        //TUIO.TuioCursor
        //touchTuio.addTuioCursor
        }
    }
}
