
Shader "Unlit/ContainedItemBoxShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		ZWrite Off
		Blend OneMinusDstColor  One
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
				float3 worldPos : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			float3 _ColliderPosition;

			float _Collision;
			 
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float uDist = abs(i.uv.x - .5) * 2;
				float vDist = abs(i.uv.y - .5) * 2;
				float uvDist = max(uDist, vDist);
				float uvAlpha = pow(uvDist, 5);
				float dist = length(i.worldPos - _ColliderPosition);
				dist = saturate(1 - dist * 5);
				dist = pow(dist, 4);
				float finalAlpha = _Collision * dist * uvAlpha;
				return float4(0.25, 1, 4 , 1) * finalAlpha;
			}
			ENDCG
		}
	}
}
