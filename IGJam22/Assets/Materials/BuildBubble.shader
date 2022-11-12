Shader "Unlit/BuildBubble"
{
    Properties
    {
        _GlowColor("Glow Color", Color) = (1,1,1,1)
        _GlowStrength("Glow Strength", Float) = 2.0
        _GlowPower("Glow Power", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Front
            Blend One OneMinusSrcAlpha
            BlendOp Add

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 viewNormal : TEXCOORD0;
                float4 viewPosition : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _GlowColor;
            float _GlowStrength;
            float _GlowPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.viewNormal = mul(UNITY_MATRIX_MV, float4(v.normal, 0.0f));
                o.viewPosition = mul(UNITY_MATRIX_MV, float4(v.vertex, 1.0f));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = _GlowColor;

                col.a = 1.0f - (dot(normalize(i.viewNormal.xyz), normalize(i.viewPosition.xyz)) * 0.5f + 0.5f);
                col.a = saturate(pow(col.a * _GlowStrength, _GlowPower));

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
}
