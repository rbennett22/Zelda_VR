Shader "VisualizeNormals"
{
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert addshadow

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

			o.Albedo = (IN.volumeNormal + float3(1.0, 1.0, 1.0)) * 0.5;
			o.Alpha = 1.0;
		}
		ENDCG
	}
		FallBack "Diffuse"
}