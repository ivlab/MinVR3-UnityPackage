using UnityEngine;

namespace MinVR
{
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class FisheyeCameraSetup : MonoBehaviour
    {
        [Header("Fisheye camera settings are located in the FisheyeCamera GameObject")]
        [SerializeField, Tooltip("Render the fisheye view to the screen (using this camera). If false, the fisheye view will be left as a RenderTexture inside the FisheyeRenderer GameObject.")]
        private bool renderToScreen = true;


        [Header("You should not need to change any of the following settings")]
        [SerializeField, Tooltip("Render texture to force the 'Scene Camera' to render to")]
        private RenderTexture mainCameraTextureOverride;
        [SerializeField, Tooltip("Instance of the fisheye blit script that overrides the main camera view")]
        private BlitFisheye fisheyeBlit;
        private Camera sceneCamera;

        void Start()
        {
            sceneCamera = gameObject.GetComponent<Camera>();
        }

        void Update()
        {
            if (fisheyeBlit != null)
                fisheyeBlit.overrideBlit = renderToScreen;

            if (sceneCamera != null)
            {
                if (renderToScreen)
                    sceneCamera.targetTexture = mainCameraTextureOverride;
                else
                    sceneCamera.targetTexture = null;
            }
        }
    }
}