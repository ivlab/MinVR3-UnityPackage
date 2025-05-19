using UnityEngine;


namespace IVLab.MinVR3
{
    /// <summary>
    /// Provides a shortcut for more easily setting the properties of many child cameras that should have
    /// several settings in common.
    /// </summary>
    [AddComponentMenu("MinVR Interaction/Display/Camera Rig Settings")]
    public class CameraRigSettings : MonoBehaviour
    {
        // common properties of child cameras
        [Range(0, 1)]
        [SerializeField] private float _nearClip = 0.001f;

        [Range(1.001f, 500000.0f)]
        [SerializeField] private float _farClip = 10000.0f;

        [Tooltip("In production, these two colors should always be the same.  For debugging quad-buffered stereo, " +
            "it can be useful to set them to different colors to see if the graphics driver is swapping left/right for example.")]
        [SerializeField] private Color _leftEyeClearColor = Color.black;
        [SerializeField] private Color _rightEyeClearColor = Color.black;

        [SerializeField] private bool _forceMonoMode = false;


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

        public Color leftEyeClearColor
        {
            get => _leftEyeClearColor;
            set
            {
                _leftEyeClearColor = value;
                UpdateChildCameras();
            }
        }

        public Color rightEyeClearColor
        {
            get => _rightEyeClearColor;
            set
            {
                _rightEyeClearColor = value;
                UpdateChildCameras();
            }
        }

        public Color clearColor
        {
            get => _leftEyeClearColor;
            set
            {
                _leftEyeClearColor = value;
                _rightEyeClearColor = value;
                UpdateChildCameras();
            }
        }

        public bool forceMonoMode
        {
            get => _forceMonoMode;
            set
            {
                _forceMonoMode = value;
                UpdateChildCameras();
            }
        }


        private void UpdateChildCameras()
        {
            Camera[] cameras = GetComponentsInChildren<Camera>();
            foreach (var cam in cameras) {
                cam.nearClipPlane = _nearClip;
                cam.farClipPlane = _farClip;

                ObliqueProjectionToQuad oproj = cam.gameObject.GetComponent<ObliqueProjectionToQuad>();
                if (oproj != null)
                {
                    oproj.applyStereoEyeOffset = !_forceMonoMode;
                }
                if ((oproj != null) && (oproj.whichEye == ObliqueProjectionToQuad.Eye.RightEye))
                {
                    cam.backgroundColor = _rightEyeClearColor;
                } else
                {
                    cam.backgroundColor = _leftEyeClearColor;
                }
            }
        }

        private void OnEnable()
        {
            UpdateChildCameras();
        }

        private void OnValidate()
        {
            UpdateChildCameras();
        }
    }

}
