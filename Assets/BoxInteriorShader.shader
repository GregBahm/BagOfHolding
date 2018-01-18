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
				float3 objSpace : TEXCOORD1;
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
				o.objSpace = v.vertex;
				return o;
			}

			float _SwapColors;
			
			fixed4 frag (v2f i) : SV_Target
			{
				float xVal = abs(i.objSpace.x) * 2;
				xVal = pow(xVal, 10);
				float yVal = abs(i.objSpace.y) * 2;
				yVal = pow(yVal, 10);
				float val = min(xVal, yVal);
				float anotherVal = -i.objSpace.y + .5;
				anotherVal += -i.normal.y;
				anotherVal = pow(anotherVal, .5);
				val += -i.normal.y;
				val = lerp(val, anotherVal, .5);
				float zVal = .5 - i.objSpace.z;
				val = lerp(val, zVal, .5);
				float3 colA = float3(.75, 1, 1.5);
				float3 colB = float3(.5,0.25, 0);
				float selector = lerp(val, 1 - val, _SwapColors);
				float3 col = lerp(colA, colB, selector);
				return float4(col, 1);
			}
			ENDCG
		}
	}
		FallBack "Diffuse"
}
