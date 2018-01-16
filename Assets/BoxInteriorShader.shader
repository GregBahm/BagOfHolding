Shader "Unlit/BoxInteriorShader"
{
	Properties
	{
	}
	SubShader
	{
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1"}
		LOD 100
		Cull Front

		Pass
		{
			Stencil 
			{
                Ref 2
                Comp Equal
                Pass keep
            }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normal = v.normal;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(i.uv.x, i.uv.y, 0, 1);
			}
			ENDCG
		}
	}
}
