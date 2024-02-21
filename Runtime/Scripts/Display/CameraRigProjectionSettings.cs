using UnityEngine;


namespace IVLab.MinVR3
{
    /// <summary>
    /// 
    /// </summary>
    public class CameraRigProjectionSettings : MonoBehaviour
    {
        // common properties of child ObliqueProjectionToQuad
        [Range(0, 0.09f)]
        [SerializeField] private float _ipd = 0.063f; // 63mm avg adult

        // common properties of child cameras
        [Range(0, 1)]
        [SerializeField] private float _nearClip = 0.001f;

        [Range(1.001f, 500000.0f)]
        [SerializeField] private float _farClip = 10000.0f;



        public float nearClip {
            get => _nearClip;
            set {
                _nearClip = value;
                UpdateChildCameras();
            }
        }

        public float farClip {
            get => _farClip;
            set {
                _farClip = value;
                UpdateChildCameras();
            }
        }

        public float IPD {
            get => _ipd;
            set {
                _ipd = value;
                UpdateChildObliqueProjections();
            }
        }

        private void UpdateChildCameras()
        {
            Camera[] cameras = GetComponentsInChildren<Camera>();
            foreach (var cam in cameras) {
                cam.nearClipPlane = _nearClip;
                cam.farClipPlane = _farClip;
            }
        }

        private void UpdateChildObliqueProjections()
        {
            ObliqueProjectionToQuad[] obliqueProjections = GetComponentsInChildren<ObliqueProjectionToQuad>();
            foreach (var op in obliqueProjections) {
                op.interpupillaryDistance = _ipd;
            }
        }

        private void OnEnable()
        {
            UpdateChildCameras();
            UpdateChildObliqueProjections();
        }

        private void OnValidate()
        {
            UpdateChildCameras();
            UpdateChildObliqueProjections();
        }
    }

}
