#ifndef COLOR_UTILITIES
#define COLOR_UTILITIES

// From http://www.chilliant.com/rgb2hsv.html
float3 HUEtoRGB(in float H)
{
	float R = abs(H * 6 - 3) - 1;
	float G = 2 - abs(H * 6 - 2);
	float B = 2 - abs(H * 6 - 4);
	return saturate(float3(R, G, B));
}

// From http://www.chilliant.com/rgb2hsv.html
float3 HSVtoRGB(in float3 HSV)
{
	float3 RGB = HUEtoRGB(HSV.x);
	return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

// Generate a color based on a floating point seed (passed in x component).
// Adjacent integral values for the seed will always have visually different colors.
// The y and z components control the hue and saturation of the palette respectively.
// Based on http://martin.ankerl.com/2009/12/09/how-to-create-random-colors-programmatically/
float3 generateColor(in float3 seedSatVal)
{
	float golden_ratio_conjugate = 0.618033988749895;
	seedSatVal.x = frac(seedSatVal.x * golden_ratio_conjugate);
	return HSVtoRGB(seedSatVal);
}

#endif //COLOR_UTILITIES