Shader "Custom/StandardContainedItemShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader 
	{
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1"}
		LOD 200
		
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;          
			float3 worldPos;
		};

		float3 _LidPlaneNormal;
		float3 _LidPlanePoint;
		float _IsOutOfContainer;

		half _Glossiness;
		half _Metallic; 
		fixed4 _Color;

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)  

		float GetDistToPlane(float3 worldPos)
		{
			return dot(worldPos, -_LidPlaneNormal) - dot(-_LidPlaneNormal, _LidPlanePoint);
		}

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float distToPlane = GetDistToPlane(IN.worldPos);
			float clipValue = lerp(distToPlane, 1, _IsOutOfContainer);
			clip(clipValue);

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Emission = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
		Stencil
		{
			Ref 2
			Comp Equal
			Pass keep
		}
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb * float3(.5, 1, 2);
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Emission = c.rgb* float3(.5, 1, 2);
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
