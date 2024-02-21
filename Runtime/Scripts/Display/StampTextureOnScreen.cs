using UnityEngine;


namespace IVLab.MinVR3
{
    /// <summary>
    /// 
    /// </summary>
    public class StampTextureOnScreen : MonoBehaviour
    {
        public RenderTexture stampTexture;
        public Vector2 topLeftCornerUV;
        public Vector2 botRightCornerUV;

        private Material stampInsideTextureMaterial;

        private void Reset()
        {
            stampTexture = null;
            topLeftCornerUV = new Vector2(0, 1);
            botRightCornerUV = new Vector2(1, 0);
        }

        private void Start()
        {
            Shader minVRStampShader = Shader.Find("MinVR/StampInsideTexture");
            Debug.Assert(minVRStampShader != null, "Cannot find shader named MinVR/StampInsideTexture");
            stampInsideTextureMaterial = new Material(minVRStampShader);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if ((stampTexture != null) && (stampInsideTextureMaterial != null)) {
                stampInsideTextureMaterial.SetTexture("_StampTex", stampTexture);
                stampInsideTextureMaterial.SetFloat("_StampTopLeftU", topLeftCornerUV[0]);
                stampInsideTextureMaterial.SetFloat("_StampTopLeftV", topLeftCornerUV[1]);
                stampInsideTextureMaterial.SetFloat("_StampBotRightU", botRightCornerUV[0]);
                stampInsideTextureMaterial.SetFloat("_StampBotRightV", botRightCornerUV[1]);
                Graphics.Blit(source, destination, stampInsideTextureMaterial);
            }
        }
    }
}
