using UnityEngine;

namespace MinVR
{
    [RequireComponent(typeof(Camera))]
    public class BlitFisheye : MonoBehaviour
    {
        [SerializeField, Tooltip("Camera that renders the fisheye cubemap view (FisheyeCamera prefab)")]
        private Camera fisheyeCamera;

        public bool overrideBlit = false;

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (overrideBlit)
                Graphics.Blit(fisheyeCamera.targetTexture, destination);
        }
    }
}