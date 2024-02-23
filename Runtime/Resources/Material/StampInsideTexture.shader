// "Stamps" a second texture within the bounding box defined by the TopLeft and
// BotRight UV coordinates and draws the original texture everywhere outside
// that box.

Shader "MinVR/StampInsideTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StampTex ("Stamp Texture", 2D) = "white" {}
        _StampTopLeftU ("Stamp Top Left U Coord", Range(0.0, 1.0)) = 0.0
        _StampTopLeftV ("Stamp Top Left V Coord", Range(0.0, 1.0)) = 1.0
        _StampBotRightU ("Stamp Bot Right U Coord", Range(0.0, 1.0)) = 1.0
        _StampBotRightV ("Stamp Bot Right V Coord", Range(0.0, 1.0)) = 0.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _StampTex;
            float _StampTopLeftU;
            float _StampTopLeftV;
            float _StampBotRightU;
            float _StampBotRightV;

            fixed4 frag (v2f i) : SV_Target
            {
                if ((i.uv[0] >= _StampTopLeftU) && (i.uv[0] < _StampBotRightU) &&
                    (i.uv[1] >= _StampBotRightV) && (i.uv[1] < _StampTopLeftV)) {
                    float2 stampuv = float2(
                        (i.uv[0] - _StampTopLeftU) / (_StampBotRightU - _StampTopLeftU),
                        (i.uv[1] - _StampBotRightV) / (_StampTopLeftV - _StampBotRightV)
                    );
                    return tex2D(_StampTex, stampuv);
                } else {
                    return tex2D(_MainTex, i.uv);
                }
            }
            ENDCG
        }
    }
}
