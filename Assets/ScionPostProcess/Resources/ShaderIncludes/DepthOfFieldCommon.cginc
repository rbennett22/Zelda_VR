#ifndef SCION_DOF_COMMON
#define SCION_DOF_COMMON

#include "../ShaderIncludes/VirtualCameraCommon.cginc" 
	
uniform float4 _CoCParams1;		
#define PrecalcCoCScale _CoCParams1.x
#define PrecalcCoCBias _CoCParams1.y
#define FocalDistance _CoCParams1.z
#define FocalRangeLength _CoCParams1.w

uniform float4 _CoCParams2;
#define MaxCoCRadius _CoCParams2.x

uniform float3 _FrustumCornerBottomLeftVP;
uniform float3 _FrustumCornerWidthVP;
uniform float3 _FrustumCornerHeightVP;

//This is done in half res, so its actually 20 pixels
//#define MAX_COC_RADIUS 10.0f
#define MAX_COC_RADIUS MaxCoCRadius

uniform sampler2D _MainTex;
uniform sampler2D _HalfResSourceTexture;	
uniform sampler2D _TiledNeighbourhoodData;
uniform sampler2D _ExclusionMask;
uniform sampler2D _HalfResSourceDepthTexture;
uniform sampler2D _PresortTexture;
uniform float4 _MainTex_TexelSize;	
uniform float _CoCUVOffset;		

float3 SamplePresortTexture(float2 uv)
{
	float3 presortSample = tex2Dlod(_PresortTexture, float4(uv, 0.0f, 0.0f)).xyz;
	presortSample.x = presortSample.x * 10.0f; //Scale from [0,1] to [0,10]
	return presortSample;
}

struct BlurTapOutput
{
	float4 	colorAndDepth	: SV_Target0;
	float4 	alpha			: SV_Target1;
};

uniform sampler2D _AvgCenterDepth;	
float GetFocalDistance()
{
	#ifdef SC_DOF_FOCUS_MANUAL
	return FocalDistance;
	#endif
	
	#ifdef SC_DOF_FOCUS_RANGE
	return FocalDistance;
	#endif	
	
	#ifdef SC_DOF_FOCUS_CENTER
	return tex2Dlod(_AvgCenterDepth, float4(0.5f, 0.5f, 0.0f, 0.0f)).x;
	#endif
	
	return 0.0f;
}

float2 PreCalculatedCoC()
{
	#ifdef SC_EXPOSURE_MANUAL
	return float2(PrecalcCoCScale, PrecalcCoCBias);
	#endif

	#ifdef SC_EXPOSURE_AUTO
	return tex2Dlod(_VirtualCameraTexture2, float4(0.5f, 0.5f, 0.0f, 0.0f)).yz;		
	#endif
	
	return 0.0f;
}

float2 CalculateCoC(float focalDistance)
{
	#ifdef SC_EXPOSURE_MANUAL
	return ComputeCoCScaleAndBias(ManualFNumber, VCFocalLength, focalDistance, CoCToPixels);	
	//CoCScaleAndBias = ComputeCoCScaleAndBiasFromAperture(ApertureDiameter, VCFocalLength, focalDistance, CoCToPixels);		
	#endif
	
	#ifdef SC_EXPOSURE_AUTO
	float fNumber = tex2Dlod(_VirtualCameraTexture1, float4(0.5f, 0.5f, 0.0f, 0.0f)).w;
	return ComputeCoCScaleAndBias(fNumber, VCFocalLength, focalDistance, CoCToPixels);
	#endif
	
	return 0.0f;
}

//The manual distance and the manual range are using a pre calculated CoC from the virtual camera pass
//This is actually the previous frames pass, however the data changes so slowly that its not noticable
//This is an optimization to effectivize the pipeline and a very acceptable tradeoff
float CoCFromDepthSigned(float depth, float focalDistance)
{			
	float2 CoCScaleAndBias = 0.0f; 
	
	#ifdef SC_DOF_FOCUS_MANUAL
	CoCScaleAndBias = PreCalculatedCoC();
	//CoCScaleAndBias = CalculateCoC(focalDistance);
	#endif
	
	#ifdef SC_DOF_FOCUS_RANGE
	CoCScaleAndBias = PreCalculatedCoC();
	//CoCScaleAndBias = CalculateCoC(focalDistance);
	depth = depth + FixedClamp(focalDistance - depth, -FocalRangeLength, FocalRangeLength);
	#endif
	
	#ifdef SC_DOF_FOCUS_CENTER
	CoCScaleAndBias = CalculateCoC(focalDistance);
	#endif
	
	//return abs(depth - focalDistance);
	//CoCScaleAndBias = 0.0f;
	
	float invDepth = 1.0f / depth;
	float CoC = invDepth * CoCScaleAndBias.x + CoCScaleAndBias.y;	
	return CoC;
}

float CoCFromDepth(float depth, float focalDistance)
{
	return abs(CoCFromDepthSigned(depth, focalDistance));
}

float CoCFromDepthClamped(float depth, float focalDistance)
{
	return min(CoCFromDepth(depth, focalDistance), MAX_COC_RADIUS);
}

//[-1.0, 1.0]
float2 RandomOffset(float2 uv)
{
	float2 randomOffset; 
	
//	#if (SHADER_API_D3D11 || SHADER_API_XBOXONE)
//	float scale = 0.25f;
//	float2 posMod = float2(uint2(uv) & 1);
//	randomOffset = (-scale + 2.0f * scale * posMod.x) * (-1.0f + 2.0f * posMod.y); 
//	#else
	float2 random = (1.0f / 4320.0f) * uv + float2(0.25f, 0.0f);
	random = frac(dot(random, 3571.0f) * random);
	randomOffset = random * 2.0f - 1.0f;
	//#endif
	
	return randomOffset;
}
		
float InvCircleArea(float radius)
{
	return 1.0f / (PI * radius * radius + 1e-5f);	
}

float InvCircleAreaClamped(float radius)
{
	//TODO: Calc inv area of MAX_COC_RADIUS on CPU
	//return clamp(InvCircleArea(radius), InvCircleArea(MAX_COC_RADIUS), InvCircleArea(1.0f));
	
	radius = max(radius, 0.5f);
	return 1.0f / (PI * radius * radius);
}

#endif //SCION_DOF_COMMON