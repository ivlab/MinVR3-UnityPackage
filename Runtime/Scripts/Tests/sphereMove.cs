using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IVLab.MinVR3;

namespace IVLab.MinVR3.TUIO11 {
    public class sphereMove : MonoBehaviour, IVREventListener {
        //get sphere object
        //get coordinate
        //move sphere object based on coordinate -- this will require the dome conversion
               
        public int distanceMultiplier;
        float m_Angle;
        Vector3 sphereAxyz;
        Vector3 sphereBxyz;
        float x;
        float y;

        public VREventPrototypeVector2 touchDownEventA;
        public VREventPrototypeVector2 touchMoveEventA;
        public VREventPrototypeVector2 touchUpEventA;
        public VREventPrototypeVector2 touchDownEventB;
        public VREventPrototypeVector2 touchMoveEventB;
        public VREventPrototypeVector2 touchUpEventB;

        public GameObject movingSphereA;
        public GameObject movingSphereB;

        public void OnVREvent(VREvent vrEvent) {
            if (vrEvent.Matches(touchDownEventA)) {
                movingSphereA.SetActive(true);
                Vector2 xyA = vrEvent.GetData<Vector2>();
                movingSphereA.transform.position = new Vector3(xyA.x * distanceMultiplier, xyA.y * distanceMultiplier, 0);
            } else if (vrEvent.Matches(touchMoveEventA)) {
                Vector2 xyA = vrEvent.GetData<Vector2>();
                movingSphereA.transform.position = new Vector3(xyA.x * distanceMultiplier, xyA.y * distanceMultiplier, 0);
            } else if (vrEvent.Matches(touchUpEventA)) {
                movingSphereA.SetActive(false);
            }

            
            else if (vrEvent.Matches(touchDownEventB))
            {
                movingSphereB.SetActive(true);
                Vector2 xyB = vrEvent.GetData<Vector2>();
                movingSphereB.transform.position = new Vector3(xyB.x * distanceMultiplier, xyB.y * distanceMultiplier, 0);
            }
            else if (vrEvent.Matches(touchMoveEventB))
            {
                Vector2 xyB = vrEvent.GetData<Vector2>();
                movingSphereB.transform.position = new Vector3(xyB.x * distanceMultiplier, xyB.y * distanceMultiplier, 0);
            }
            else if (vrEvent.Matches(touchUpEventB))
            {
                movingSphereB.SetActive(false);
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

        public void CalculateAngleBetweenPoints()
        {
            if ((movingSphereA.activeSelf == true) && (movingSphereB.activeSelf == true))
            {
                //fetch positions of moving sphere A and B
                sphereAxyz = new Vector3(movingSphereA.transform.position.x, movingSphereA.transform.position.y, movingSphereA.transform.position.z);
                sphereBxyz = new Vector3(movingSphereB.transform.position.x, movingSphereB.transform.position.y, movingSphereA.transform.position.z);
                //m_Angle = Vector2.Angle(sphereAxy, sphereBxy);

                x = movingSphereB.transform.position.x - movingSphereA.transform.position.x;
                y = movingSphereB.transform.position.y - movingSphereA.transform.position.y;
                m_Angle = Mathf.Atan2(x, y) * Mathf.Rad2Deg;

                //Debug.Log("Position of Sphere A " + sphereAxyz);
                //Debug.Log("Position of Sphere B " + sphereBxyz);

                Debug.DrawLine(sphereAxyz, sphereBxyz, Color.magenta);
                Debug.Log("Angle Between objects: " + m_Angle);
            }  
        }


        // Start is called before the first frame update
        void Start() {
            m_Angle = 0.0f;
            sphereAxyz = Vector3.zero;
            sphereBxyz = Vector3.zero;
            x = 0.0f;
            y = 0.0f;

            if (movingSphereA == null) {
                movingSphereA = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                movingSphereA.transform.SetParent(transform);
            }
            movingSphereA.SetActive(false);
        }

        // Update is called once per frame
        void Update() {
            //TUIO.TuioCursor
            //touchTuio.addTuioCursor

            CalculateAngleBetweenPoints();

            
        }
    }
}
