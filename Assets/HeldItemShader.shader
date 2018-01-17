Shader "Unlit/HeldItemShader"
{
	Properties
	{
	}
	SubShader
	{
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1"}
		LOD 100
		
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

			float _IsOutOfContainer;
			
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
				//clip(.5 - _IsOutOfContainer);
				return fixed4(i.normal, 1) / 2 + .5;
			}
			ENDCG
		}
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
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				float3 normal : NORMAL;
			};

			float3 _LidPlaneNormal;
			float3 _LidPlanePoint;
			float _IsOutOfContainer;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.normal = v.normal;
				return o;
			}
			
			float GetDistToPlane(float3 worldPos)
			{
				return dot(worldPos, -_LidPlaneNormal) - dot(-_LidPlaneNormal, _LidPlanePoint);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float distToPlane = GetDistToPlane(i.worldPos);
				float clipValue = lerp(distToPlane, 1, _IsOutOfContainer);
				clip(clipValue);
				return fixed4(i.normal.zxy, 1) / 2 + .5;
			}
			ENDCG
		}
	}
}
