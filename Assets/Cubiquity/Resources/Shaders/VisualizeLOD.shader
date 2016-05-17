Shader "VisualizeLOD"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert addshadow

		#include "ColorUtilities.cginc"

		float _height;

		struct Input
		{
			float4 color : COLOR;
			float3 worldPos : POSITION;
			float3 volumeNormal;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);

			o.volumeNormal = v.normal;
		}



		void surf(Input IN, inout SurfaceOutput o)
		{
			IN.volumeNormal = normalize(IN.volumeNormal);

			o.Albedo = generateColor(float3(_height, 0.5, 1.0));

			o.Alpha = 1.0;
		}
		ENDCG
	}
		FallBack "Diffuse"
}