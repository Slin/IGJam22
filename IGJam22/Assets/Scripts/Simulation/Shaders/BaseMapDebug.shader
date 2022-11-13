Shader "Unlit/BaseMapDebug"
{
	Properties
	{
		_Population ("Population", 2D) = "white" {}
		_Spirit ("Spirit", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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

			sampler2D _Population;
			sampler2D _Spirit;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = fixed4(tex2D(_Population, float2(1, 1) - i.uv).r / 1000.0f,
					tex2D(_Spirit, float2(1, 1) - i.uv).r / 1000.0f, -tex2D(_Spirit, float2(1, 1) - i.uv).r / 1000.0f, 1.0f);
				// fixed4 col = fixed4(tex2D(_Population, float2(1, 1) - i.uv).r / 1000.0f,
				// 	0.0f, 0.0f, 1.0f);
				return col;
			}
			ENDCG
		}
	}
}