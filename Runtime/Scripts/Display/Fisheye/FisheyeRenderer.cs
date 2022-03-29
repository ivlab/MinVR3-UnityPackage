////////////////////////////////////////////////////////////////////////////////////////
//
// COPYRIGHT (C) Evans & Sutherland Computer Corporation
// All rights reserved.
//
////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace ES
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Evans & Sutherland/Fisheye Renderer")]
    public class FisheyeRenderer : MonoBehaviour
    {
        private static readonly Quaternion FaceRight = Quaternion.Euler(0, 90, 0);
        private static readonly Quaternion FaceLeft = Quaternion.Euler(0, 270, 0);
        private static readonly Quaternion FaceUp = Quaternion.Euler(90, 0, 0);
        private static readonly Quaternion FaceDown = Quaternion.Euler(270, 0, 0);
        private static readonly Quaternion FaceBack = Quaternion.Euler(0, 180, 0);

        public enum Resolution : int
        {
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            //_8192 = 8192
        };

        [SerializeField, Tooltip("The camera used to capture the scene. Should NOT be the same camera this script is attached to.")]
        private Camera sceneCamera = null;
        [SerializeField, Tooltip("When the scene camera is null or disabled, should a new camera be automatically be selected.")]
        private bool autoSelectCamera = true;
        [SerializeField, Tooltip("The cubemap resolution to capture the scene at. Usually one order lower than the output resolution is good.")]
        private Resolution captureResolution = Resolution._1024;
        [SerializeField, Tooltip("The resolution of the rendered fisheye image. Usually one order higher than the capture resolution is good.")]
        private Resolution fisheyeResolution = Resolution._2048;
        [SerializeField, Tooltip("The angle of the fisheye image."), Range(1f, 360f)]
        private float fisheyeAngle = 180;
        [SerializeField, Tooltip("The rotation offset to apply to the source camera when rendering the scene.")]
        private Vector3 rotationOffset = Vector3.zero;


        private Camera fisheyeCamera;
        private RenderTexture sceneCubemap;
        private RenderTexture sceneFace;
        private RenderTexture fisheyeTexture;
        private RenderTexture cameraTargetTexture;
        private ComputeShader fisheyeShader;
        private int fisheyeKernelId;
        private int fisheyeAngleId;
        private int invTextureSizeId;
        private int sourceCubemapId;
        private int resultTextureId;
        private bool renderingSceneCubemap = false;

        private void InitializeShader()
        {
            fisheyeShader = (ComputeShader)Resources.Load("Fisheye");
            fisheyeKernelId = fisheyeShader.FindKernel("fisheye");
            fisheyeAngleId = Shader.PropertyToID("fisheyeAngle");
            invTextureSizeId = Shader.PropertyToID("invTextureSize");
            sourceCubemapId = Shader.PropertyToID("sourceCubemap");
            resultTextureId = Shader.PropertyToID("result");
        }

        private void FindSourceCamera()
        {
            sceneCamera = Camera.main;
            if (sceneCamera == null || !sceneCamera.isActiveAndEnabled)
            {
                var cameras = FindObjectsOfType<Camera>();
                foreach (var camera in cameras)
                {
                    if (camera != fisheyeCamera && camera.isActiveAndEnabled)
                    {
                        sceneCamera = camera;
                        break;
                    }
                }
            }
        }

        private void RenderSceneCubemap()
        {
            renderingSceneCubemap = true;

            var originalTarget = sceneCamera.targetTexture;
            var originalRotation = sceneCamera.transform.localRotation;
            var originalFOV = sceneCamera.fieldOfView;
            var originalAspect = sceneCamera.aspect;
            sceneCamera.targetTexture = sceneFace;
            sceneCamera.fieldOfView = 90;
            sceneCamera.aspect = 1;

            var baseRotation = originalRotation * Quaternion.Euler(rotationOffset);

            // render forward
            sceneCamera.transform.localRotation = baseRotation;
            sceneCamera.Render();
            Graphics.CopyTexture(sceneFace, 0, sceneCubemap, (int)CubemapFace.PositiveZ);

            // render right
            sceneCamera.transform.localRotation = baseRotation * FaceRight;
            sceneCamera.Render();
            Graphics.CopyTexture(sceneFace, 0, sceneCubemap, (int)CubemapFace.PositiveX);

            // render left
            sceneCamera.transform.localRotation = baseRotation * FaceLeft;
            sceneCamera.Render();
            Graphics.CopyTexture(sceneFace, 0, sceneCubemap, (int)CubemapFace.NegativeX);

            // render up
            sceneCamera.transform.localRotation = baseRotation * FaceUp;
            sceneCamera.Render();
            Graphics.CopyTexture(sceneFace, 0, sceneCubemap, (int)CubemapFace.PositiveY);

            // render down
            sceneCamera.transform.localRotation = baseRotation * FaceDown;
            sceneCamera.Render();
            Graphics.CopyTexture(sceneFace, 0, sceneCubemap, (int)CubemapFace.NegativeY);

            // render back
            sceneCamera.transform.localRotation = baseRotation * FaceBack;
            sceneCamera.Render();
            Graphics.CopyTexture(sceneFace, 0, sceneCubemap, (int)CubemapFace.NegativeZ);

            // reset source camera settings
            sceneCamera.transform.localRotation = originalRotation;
            sceneCamera.targetTexture = originalTarget;
            sceneCamera.fieldOfView = originalFOV;
            sceneCamera.aspect = originalAspect;

            renderingSceneCubemap = false;
        }

        private void Start()
        {
            // these are consistent between all the render textures used, and depend on the active color space
            var readwrite = QualitySettings.activeColorSpace == ColorSpace.Linear ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default;
            var texFormat = QualitySettings.activeColorSpace == ColorSpace.Linear ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.Default;

            // initialize cubemap texture
            sceneCubemap = new RenderTexture((int)captureResolution, (int)captureResolution, 16, texFormat, readwrite);
            sceneCubemap.dimension = UnityEngine.Rendering.TextureDimension.Cube;
            sceneCubemap.hideFlags = HideFlags.HideAndDontSave;
            sceneCubemap.Create();

            // initialize scene face temp texture:
            // Unity doesn't allow us to set a single cubemap face
            // texture as a camera's target texture, so we use the
            // intermediate 'sceneFace' texture to render into, and
            // then copy it into each face of the cubemap. Not the 
            // most efficient, but no other way as far as I can find.
            sceneFace = new RenderTexture((int)captureResolution, (int)captureResolution, 16, texFormat, readwrite);
            sceneFace.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            sceneFace.hideFlags = HideFlags.HideAndDontSave;
            sceneFace.enableRandomWrite = true;
            sceneFace.Create();

            // initialize fisheye output texture
            fisheyeTexture = new RenderTexture((int)fisheyeResolution, (int)fisheyeResolution, 0, texFormat, readwrite);
            fisheyeTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            fisheyeTexture.hideFlags = HideFlags.HideAndDontSave;
            fisheyeTexture.enableRandomWrite = true;
            fisheyeTexture.Create();

            // initialize camera target texture (this is used 
            // so the attached camera will actually respect the
            // resolution that is specified)			
            cameraTargetTexture = new RenderTexture((int)fisheyeResolution, (int)fisheyeResolution, 16, texFormat, readwrite);
            cameraTargetTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            cameraTargetTexture.hideFlags = HideFlags.HideAndDontSave;
            cameraTargetTexture.name = "fisheye camera target";
            cameraTargetTexture.Create();
            fisheyeCamera.targetTexture = cameraTargetTexture;

            // initialize all fields for the fisheye shader
            InitializeShader();
        }

        private void OnEnable()
        {
            // get the fisheye camera (the camera attached to this renderer)
            fisheyeCamera = GetComponent<Camera>();

            // as a sanity check don't allow the attached camera to be set as the scene camera
            if (fisheyeCamera == sceneCamera)
            {
                Debug.LogError("FisheyeRenderer has the 'SceneCamera' field set to itself. This must be set to a different camera object!");
                enabled = false;
            }
            else
            {
                // disable the fisheye camera when this component is enabled.
                // this script will manually render that camera, so don't make
                // it render twice each frame.
                fisheyeCamera.enabled = false;
            }
        }

        private void OnDestroy()
        {
            // cleanup render textures

            if (sceneCubemap != null)
            {
                sceneCubemap.Release();
                sceneCubemap = null;
            }

            if (sceneFace != null)
            {
                sceneFace.Release();
                sceneFace = null;
            }

            if (fisheyeTexture != null)
            {
                fisheyeTexture.Release();
                fisheyeTexture = null;
            }

            if (cameraTargetTexture != null)
            {
                fisheyeCamera.targetTexture = null;
                cameraTargetTexture.Release();
                cameraTargetTexture = null;
            }
        }

        private void LateUpdate()
        {
            if ((sceneCamera == null || !sceneCamera.isActiveAndEnabled) && autoSelectCamera)
            {
                FindSourceCamera();
            }

            // update cubemap
            RenderSceneCubemap();

            // render the fisheye
            fisheyeCamera.Render();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // ensure this isn't being called while rendering the scene cubemap 
            // (will happen if this camera is set as the SceneCamera.
            if (renderingSceneCubemap)
            {
                Debug.LogError("FisheyeRenderer::OnRenderImage called while rendering scene cubemap. Make sure the 'SceneCamera' is not set to itself!");
                return;
            }

            // ignore the source texture, generate the destination 
            // from the scene cubemap rendered in LateUpdate
            int groups = (int)fisheyeResolution / 32;
            fisheyeShader.SetFloat(fisheyeAngleId, fisheyeAngle * 0.5f * Mathf.Deg2Rad);
            fisheyeShader.SetFloat(invTextureSizeId, 1.0f / (int)fisheyeResolution);
            fisheyeShader.SetTexture(fisheyeKernelId, sourceCubemapId, sceneCubemap);
            fisheyeShader.SetTexture(fisheyeKernelId, resultTextureId, fisheyeTexture);
            fisheyeShader.Dispatch(fisheyeKernelId, groups, groups, 1);
            Graphics.Blit(fisheyeTexture, destination);
        }
    }
}
