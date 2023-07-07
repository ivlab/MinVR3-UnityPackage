using UnityEngine;

namespace IVLab.MinVR3
{
    /// <summary>
    /// Rotates an object to always face the camera (billboard-style)
    /// </summary>
    [AddComponentMenu("MinVR Interaction/Navigation and Manipulation/Object Face Camera")]
    public class FaceCamera : MonoBehaviour
    {
        [SerializeField, Tooltip("Camera to mirror the rotation of (will default to MainCamera tag if empty)")]
        public Camera cameraToFace;


        void Start()
        {
            if (cameraToFace == null)
            {
                cameraToFace = Camera.main;
            }

            if (cameraToFace == null)
            {
                cameraToFace = GameObject.FindObjectOfType<Camera>();
            }
        }


        void Update()
        {
            this.transform.rotation = cameraToFace.transform.rotation;
        }
    }
}