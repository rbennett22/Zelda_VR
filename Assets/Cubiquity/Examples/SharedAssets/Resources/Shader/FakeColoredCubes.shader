// This shader can be applied to a cube to make it look like a 'colored cube' voxel.
Shader "FakeColoredCubes"
{
	SubShader
	{
		// Set up for transparent rendering.
		Tags { "RenderType" = "Opaque" }

		CGPROGRAM

		// We include this file so we can access functionality such as noise from the colored cubes shader.
		// Using the relative path here is slightly error-prone, but it seems a Unity bug prevents us from using
		// the absolute path: http://forum.unity3d.com/threads/custom-cginc-relative-to-assets-folder-in-dx11.163271/
		#include "../../../../Resources/Shaders/ColoredCubesUtilities.cginc"

		#pragma multi_compile DIFFUSE_TEXTURE_OFF DIFFUSE_TEXTURE_ON
		#pragma surface surf Lambert addshadow
		#pragma target 3.0
		#pragma glsl

		// These are the same parameters that the real colored cube has.
		sampler2D _DiffuseMap;
		sampler2D _NormalMap;
		float _NoiseStrength;

		// These properties are specific to the fake colored cube.
		float4 _CubeColor;
		float4 _CubePosition; // In volume space

		struct Input
		{
			float2 uv_NormalMap;
			float4 localPosition;
		};

		void surf(Input IN, inout SurfaceOutput o)
		{
			// Add noise using the same input as is used by the real colored cubes voxel shader.
			float noise = positionBasedNoise(float4(_CubePosition.xyz, _NoiseStrength));

			// Sample the same surface maps that are used by the real colored cubes voxel shader.
			// So far it hasn't seemed necessary to wrap this in an '#if' (like for the diffuse texture),
			// presumably because the default texture (when one is not set) is ok. Would still be more
			// effcient to wrap this of course but it's just an example.
			float3 unpackedNormal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));

			// Sample the diffuse texture map if it's being used
#if DIFFUSE_TEXTURE_ON
			float3 diffuseVal = tex2D(_DiffuseMap, IN.uv_NormalMap);
#else
			float3 diffuseVal = float3(1.0, 1.0, 1.0);
#endif

			// Set the appropriate attributes of the output struct.
			o.Albedo = (_CubeColor.rgb + float3(noise, noise, noise)) * diffuseVal;
			o.Alpha = 1.0f;
			o.Normal = unpackedNormal;
		}
		ENDCG
	}
}