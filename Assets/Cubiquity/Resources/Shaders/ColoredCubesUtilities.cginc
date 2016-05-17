#ifndef COLORED_CUBES_VOLUME_UTILITIES
#define COLORED_CUBES_VOLUME_UTILITIES

float positionBasedNoise(float4 positionAndStrength)
{
	//'floor' is more widely supported than 'round'. Offset consists of:
	//  - 0.5 to perform the rounding
	float3 roundedPos = floor(positionAndStrength.xyz + float3(0.5, 0.5, 0.5));

	// The noise function below seems to do remarkably well even for large inputs. None-the-less, it is
	// better for small inputs so we limit the input range to within a few hundred voxels of the origin.
	roundedPos += float3(256.0, 256.0, 256.0);
	roundedPos += fmod(roundedPos, float3(512.0, 512.0, 512.0));

	// Based on this: http://byteblacksmith.com/improvements-to-the-canonical-one-liner-glsl-rand-for-opengl-es-2-0/
	// I've modified it to use a 3D seed with the third coefficient being a number I picked at random. Because it is
	// using a 3D seed the magnitude of the dot product could be much larger, so I've reduced each of the coefficients
	// by a factor of 10 to limit precision problems for high seed values. We can tweak these further in the future.
	float noise = frac(sin(fmod(dot(roundedPos.xyz, float3(1.29898, 7.8233, 4.26546)), 3.14)) * 43758.5453);

	//Scale the noise
	float halfNoiseStrength = positionAndStrength.w * 0.5;
	noise = -halfNoiseStrength + positionAndStrength.w * noise; //http://www.opengl.org/wiki/GLSL_Optimizations#Get_MAD

	return noise;
}

#endif //COLORED_CUBES_VOLUME_UTILITIES