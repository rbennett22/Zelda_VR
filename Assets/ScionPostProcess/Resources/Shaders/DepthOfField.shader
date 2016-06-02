Shader "Hidden/ScionDepthOfField" 
{	   	
 	Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
       	
	CGINCLUDE
	#include "UnityCG.cginc" 
	#include "../ShaderIncludes/ScionCommon.cginc" 
	#include "../ShaderIncludes/MedianFilter.cginc" 
	#include "../ShaderIncludes/VirtualCameraCommon.cginc" 
	#include "../ShaderIncludes/DepthOfFieldCommon.cginc"
    	
	struct v2f
	{
		float4 pos 	: SV_POSITION;
		float2 uv 	: TEXCOORD0;
	};	
		
	v2f vert(appdata_img v)
	{
		v2f o;
		
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;
		#if UNITY_UV_STARTS_AT_TOP
        //if (_MainTex_TexelSize.y < 0.0f) o.uv.y = 1.0f - o.uv.y; 
		#endif
		
		return o; 
	}
	
	uniform float _ScionForwardMSAAFix;
	//This makes little sense but everything seems to be working now
	//Leave this just in case
	float2 FixForwardMSAAUV(float2 uv)
	{
		return uv;
		//return float2(uv.x, lerp(uv.y, 1.0f - uv.y, _ScionForwardMSAAFix));
	}
	
	float4 TiledDataHorizontal(v2f i) : SV_Target0
	{		
		i.uv = FixForwardMSAAUV(i.uv);
	
		float maxDepth;
		float minDepth;
					
		SCION_UNROLL for (int k = -5; k < 5; k++)
		{
			float kWithHalfPixOffset = k + 0.5f; //[-4.5, 4.5]
			float2 uv = float2(i.uv.x + kWithHalfPixOffset * _CoCUVOffset, i.uv.y);
			
			float depth = tex2Dlod(_HalfResSourceDepthTexture, float4(uv, 0.0f, 0.0f)).w;
			
			//Removed by compiler, no runtime branch
			if (k == -5) 
			{
				maxDepth = depth;
				minDepth = depth;
			}
			else
			{
				maxDepth = max(maxDepth, depth);
				minDepth = min(minDepth, depth);
			}
		}
				
		return float4(maxDepth, minDepth, 0.0f, 0.0f);
	}	
	
	uniform sampler2D _HorizontalTileResult;

	float4 TiledDataVertical(v2f i) : SV_Target0
	{		
		float maxDepth;
		float minDepth;
					
		SCION_UNROLL for (int k = -5; k < 5; k++)
		{
			float kWithHalfPixOffset = k + 0.5f; //[-4.5, 4.5]
			float2 uv = float2(i.uv.x , i.uv.y + kWithHalfPixOffset * _CoCUVOffset);
			
			float2 maxMinDepth = tex2Dlod(_HorizontalTileResult, float4(uv, 0.0f, 0.0f)).xy;
			
			//Removed by compiler, no runtime branch
			if (k == -5) 
			{
				maxDepth = maxMinDepth.x;
				minDepth = maxMinDepth.y;
			}
			else
			{
				maxDepth = max(maxDepth, maxMinDepth.x);
				minDepth = min(minDepth, maxMinDepth.y);
			}
		}
				
		return float4(maxDepth, minDepth, 0.0f, 0.0f);
	}	
	
	uniform float4 _NeighbourhoodParams;
	#define TileDataTexelWidth _NeighbourhoodParams.x
	#define TileDataTexelHeight _NeighbourhoodParams.y
	
	uniform sampler2D _TiledData;

	float4 NeighbourhoodDataGather(v2f i) : SV_Target0
	{		
		float maxDepth;
		float minDepth;
					
		SCION_UNROLL for (int v = -1; v < 2; v++)
		{
			SCION_UNROLL for (int u = -1; u < 2; u++)
			{
				float2 uv = float2(i.uv.x + u * TileDataTexelWidth, i.uv.y + v * TileDataTexelHeight);				
				float2 maxMinDepth = tex2Dlod(_TiledData, float4(uv, 0.0f, 0.0f)).xy;
				
				//Removed by compiler, no runtime branch
				if (u == -1 && v == -1) 
				{
					maxDepth = maxMinDepth.x;
					minDepth = maxMinDepth.y;
				}
				else
				{
					maxDepth = max(maxDepth, maxMinDepth.x);
					minDepth = min(minDepth, maxMinDepth.y);
				}
			}
		}
		
		float focalDistance = GetFocalDistance();
		float maxDepthCoC = CoCFromDepth(maxDepth, focalDistance);		
		float minDepthCoC = CoCFromDepth(minDepth, focalDistance);		
		float maxCoC = min(max(maxDepthCoC, minDepthCoC), MAX_COC_RADIUS);	
		
		//float colorOnlyLoop = maxDepth - minDepth
		
		return float4(maxCoC, minDepth, 0.0f, 0.0f);
	}	
							
	float4 MedianFilter(v2f i) : SV_Target0
	{
		return MedianFilter4(i.uv, _MainTex, InvHalfResSize);
	}	
	
//	float2 DepthCmp2( float depth, float closestDepth )
//	{
//		float d = DOF_DEPTH_SCALE_FOREGROUND * ( depth - closestDepth);
//		float2 depthCmp;
//		depthCmp.x = smoothstep( 0.0, 1.0, d ); // Background
//		depthCmp.y = 1.0 - depthCmp.x; // Foreground
//		return depthCmp;
//	}

	uniform float _ScionTestValue;
	float2 DepthComparison(float depth, float referenceDepth)
	{		
//		#if 0
		float d = 0.5f * (depth - referenceDepth);		
		float2 depthCmp; 
		depthCmp.y = smoothstep(0.0f, 1.0f, d); // Background
		depthCmp.x = 1.0f - depthCmp.y; 		// Foreground		
		return depthCmp;
//		#else
//		float background = saturate(depth - referenceDepth + 1.0f);
//		float foreground = 1.0f - background;		
//		return float2(foreground, background);	
//		#endif
		
//		float foreground = saturate(referenceDepth - depth + 2.0f);	
//		float background = 1.0f - foreground;
//		return float2(foreground, background);	
	}
							
	float4 Presort(v2f i) : SV_Target0
	{	
		i.uv = FixForwardMSAAUV(i.uv);
		
		float4 sampleLightingDepth 	= tex2Dlod(_HalfResSourceDepthTexture, float4(i.uv, 0.0f, 0.0f));
		float sampleDepth 			= sampleLightingDepth.w;	
		
		float4 tiledNeighbourhood 	= tex2Dlod(_TiledNeighbourhoodData, float4(i.uv, 0.0f, 0.0f));	
		float neighborhoodMinDepth 	= tiledNeighbourhood.y;	
		
		float2 depthWeights = DepthComparison(sampleDepth, neighborhoodMinDepth);	
		float CoC = CoCFromDepth(sampleDepth, GetFocalDistance());
		CoC = clamp(CoC, 0.5f, MAX_COC_RADIUS); //Force at least half pixel radius
		
		//Premultiply with inverse CoC area
		depthWeights = depthWeights * (1.0f / (CoC*CoC*3.1415f));
		
		return float4(CoC * 0.1f, depthWeights, 0.0f);
	}	
	
	//If black fine, otherwise not
	float2 HalfPixelOffsetTest(float2 uv)
	{	
		float2 fixedUV = floor((uv * HalfResSize) + 0.1f) + 0.5f;
		return float2(abs(uv * HalfResSize - fixedUV));
	}

	static const float2 PoissonTaps8[8] = { float2( -0.7456541f, 0.1131393f), 
		float2( 0.08293837f, -0.8036098f), float2( 0.2584362f, 0.1864142f), float2( -0.7107184f, -0.6010008f), 
		float2( 0.08933985f, 0.9051569f), float2( -0.6178224f, 0.7624108f), float2( 0.7340344f, -0.4169394f), 
		float2( 0.922537f, 0.2814612f) };		

	static const float2 PoissonTaps16[16] = { float2( 0.1213936f, -0.6422687f), 
		float2( -0.0200316f, -0.1517201f), float2( -0.595705f, -0.7857988f), float2( 0.488985f, -0.392335f), 
		float2( -0.4554596f, -0.1692587f), float2( -0.1923808f, -0.9592055f), float2( 0.5479098f, 0.07066441f), 
		float2( 0.06035915f, 0.3289492f), float2( 0.9886969f, 0.008187056f), float2( -0.8500692f, -0.4246502f), 
		float2( 0.6955848f, 0.6919048f), float2( 0.3017962f, 0.8908848f), float2( -0.2407158f, 0.6789985f), 
		float2( -0.9108973f, 0.1838938f), float2( -0.4678481f, 0.3011438f), float2( -0.7653541f, 0.6193144f) };
	
	float4 PrefilterSource(v2f i) : SV_Target0
	{				
		const float weightEpsilon = 1e-2f;
		const float maxWeight = 10.0f;
	
		float4 sourceSample = tex2Dlod(_HalfResSourceDepthTexture, float4(i.uv, 0.0f, 0.0f));
		float centerDepth = sourceSample.w;
		
		#ifdef SC_DOF_MASK_ON	
		float sampleValidity = tex2Dlod(_ExclusionMask, float4(i.uv, 0.0f, 0.0f)).x;
		SCION_BRANCH if (sampleValidity < 0.5f) return sourceSample;
		#endif
		
		//return centerDepth.xxxx / 30.0f;
		//return sourceSample;
		
		float4 tiledNeighbourhood = tex2Dlod(_TiledNeighbourhoodData, float4(i.uv, 0.0f, 0.0f));
		float2 centerForegroundBackground = DepthComparison(centerDepth, tiledNeighbourhood.y);		
		float centerCoC = CoCFromDepthClamped(centerDepth, GetFocalDistance());
		float sampleRadius = max(centerCoC * 0.35f, 1.0f);	
		
		//return float4(tiledNeighbourhood.x,0,0,0) / 20.0f;
		//return float4(tiledNeighbourhood.y,0,0,0);
		//return tiledNeighbourhood;
		//return float4(centerForegroundBackground,0,0);
		
		#if 1
		float4 totalAccumulated = float4(sourceSample.xyz, 1.0f);
		#else
		float sourceWeight = min((1.0f / weightEpsilon), maxWeight);
		float4 totalAccumulated = float4(sourceSample.xyz * sourceWeight, sourceWeight);
		#endif
		
		#define PRE_ITER 8	
		SCION_UNROLL for (int k = 0; k < PRE_ITER; k++)
		{
			#if PRE_ITER == 8
			float2 uv = i.uv + PoissonTaps8[k] * InvHalfResSize * sampleRadius;
			#endif 
			
			#if PRE_ITER == 16
			float2 uv = i.uv + PoissonTaps16[k] * InvHalfResSize * sampleRadius;
			#endif			

			float4 sampleLightingDepth = tex2Dlod(_HalfResSourceDepthTexture, float4(uv, 0.0f, 0.0f));
			float sampleDepth = sampleLightingDepth.w;	
			float weight = 1.0f / (abs(centerDepth - sampleDepth) + weightEpsilon);
			
			#if 1
			//TODO: Test this with full pipeline active
			const float biasIntensity = 10.0f;			
			float4 sampleTiledNeighbourhood = tex2Dlod(_TiledNeighbourhoodData, float4(uv, 0.0f, 0.0f));
			float2 sampleForegroundBackground = DepthComparison(sampleDepth, sampleTiledNeighbourhood.y);	
			weight = weight + dot(sampleForegroundBackground, centerForegroundBackground) * biasIntensity;
			#endif
			
			weight = min(weight, maxWeight);
					
			float4 accumulated = float4(sampleLightingDepth.xyz * weight, weight);			
			totalAccumulated += accumulated;
		}
		
		//return totalAccumulated.wwww * 1e-4f;
		return float4(totalAccumulated.xyz / totalAccumulated.w, centerDepth);
	}
	
	uniform sampler2D _TapsTexture;
	uniform sampler2D _AlphaTexture;
	uniform sampler2D _CameraDepthTexture;
	uniform sampler2D _FullResolutionSource;
	uniform sampler2D _ScionCopiedFullResDepth;
	
	float4 UpsamplePass(v2f i) : SV_Target0
	{		
		//i.uv = FixForwardMSAAUV(i.uv);
		
		//return tex2Dlod(_ExclusionMask, float4(i.uv, 0.0f, 0.0f)).xxxx;
		//return tex2Dlod(_AlphaTexture, float4(i.uv, 0.0f, 0.0f)).xxxx;
		//return tex2Dlod(_TapsTexture, float4(i.uv, 0.0f, 0.0f));
		#ifdef DEBUG_OPTIMIZATIONS
		return tex2Dlod(_TapsTexture, float4(i.uv, 0.0f, 0.0f));
		#endif
		
		#ifdef SC_DOF_MASK_ON	 
		float centerDepth = tex2Dlod(_ScionCopiedFullResDepth, float4(i.uv, 0.0f, 0.0f));
		#else
		float centerDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
		#endif
		
		float2 halfResUV = (floor(i.uv * HalfResSize) + 0.5f) * InvHalfResSize;
		float2 UVDiffDirection = sign(i.uv - halfResUV) * InvHalfResSize;
		
		float2 sampleUV[4];	
		sampleUV[0] = halfResUV;
		sampleUV[1] = halfResUV + UVDiffDirection;
		sampleUV[2] = float2(halfResUV.x + UVDiffDirection.x, halfResUV.y);
		sampleUV[3] = float2(halfResUV.x, halfResUV.y + UVDiffDirection.y);	
		
		float bilinearWeight[4];
		bilinearWeight[0] = 9.0f/16.0f;
		bilinearWeight[1] = 1.0f/16.0f;
		bilinearWeight[2] = 3.0f/16.0f;
		bilinearWeight[3] = 3.0f/16.0f; 
		
		#ifdef SC_DOF_MASK_ON	 
		float exclusionMaskAlpha = 0.0f;
		#endif
		
		float4 upsampledDoF = 0.0f;
		float totalWeight = 0.0f;
		SCION_UNROLL for (int j = 0; j < 4; j++)
		{
			float4 tapsSample = tex2Dlod(_TapsTexture, float4(sampleUV[j], 0.0f, 0.0f));
			float alpha = tex2Dlod(_AlphaTexture, float4(sampleUV[j], 0.0f, 0.0f));			
			float depth = tapsSample.w;
			float3 taps = tapsSample.xyz;
			float4 DoFSample = float4(taps, alpha);			
			
			float weight = bilinearWeight[j] / (abs(centerDepth - depth) + 1e-5f);			
			upsampledDoF = upsampledDoF + DoFSample * weight; 
			totalWeight = totalWeight + weight;
			
			#ifdef SC_DOF_MASK_ON
			float sampleValidity = tex2Dlod(_ExclusionMask, float4(sampleUV[j], 0.0f, 0.0f)).x;
			exclusionMaskAlpha = exclusionMaskAlpha + sampleValidity * weight;
			#endif
		}	
		
		#ifdef SC_DOF_MASK_ON
		float invWeight = 1.0f / totalWeight;
		exclusionMaskAlpha = exclusionMaskAlpha * invWeight;
		upsampledDoF = upsampledDoF * invWeight;
		#else
		upsampledDoF = upsampledDoF / totalWeight;	
		#endif
		
		float tiledNeighbourhood = tex2Dlod(_TiledNeighbourhoodData, float4(i.uv, 0.0f, 0.0f)).x;
		i.uv = FixForwardMSAAUV(i.uv);
		float3 fullResSource = tex2Dlod(_FullResolutionSource, float4(i.uv, 0.0f, 0.0f)).xyz;	
				
		float backgroundFactor = sqrt(CoCFromDepthClamped(centerDepth, GetFocalDistance()) * 0.35f);
		float foregroundFactor = tiledNeighbourhood.x * 0.25f;
		float upsampleFactor = smoothstep(0.0f, 1.0f, upsampledDoF.w);
		//float upsampleFactor = saturate(upsampledDoF.w);
		
		float lerpFactor = saturate(lerp(backgroundFactor, foregroundFactor, upsampleFactor));
		
		//return centerDepth.xxxx * 0.1f;
		//return CoCFromDepthClamped(centerDepth, GetFocalDistance()).xxxx;
		
		#ifdef SC_DOF_MASK_ON
		lerpFactor *= exclusionMaskAlpha;
		#endif
		 
		float3 finalResult = lerp(fullResSource, upsampledDoF.xyz, lerpFactor);		 
		return float4(finalResult, 1.0f);
	} 
	  
	uniform float4 _DownsampleWeightedParams; 
	#define DownsampleWeightedCenter float2(_DownsampleWeightedParams.xy)
	#define DownsampleWeightedRangeSqrd _DownsampleWeightedParams.z
	#define InvDownsampleWeightedRangeSqrd _DownsampleWeightedParams.w
	
	float4 DownsampleWeighted(v2f i) : SV_Target0
	{
		float depth = tex2Dlod(_MainTex, float4(i.uv, 0.0f, 0.0f)).w;
		
		float2 direction = i.uv - DownsampleWeightedCenter;
		direction.x *= AspectRatio;
		float distSqrd = direction.x*direction.x + direction.y*direction.y;
		float difference = DownsampleWeightedRangeSqrd - distSqrd;
		float weight = saturate(difference * InvDownsampleWeightedRangeSqrd);
				
		return float4(depth * weight, weight, 0.0f, 0.0f);
	}
	
	uniform sampler2D _PreviousWeightedResult;	
	uniform float _DownsampleWeightedAdaptionSpeed;
	
	float4 DownsampleWeightedFinalPass(v2f i) : SV_Target0
	{
		float2 depthAndWeight = tex2Dlod(_MainTex, float4(i.uv, 0.0f, 0.0f)).xy;		
		float depth = depthAndWeight.x / (depthAndWeight.y+1e-5f);
		float previousDepth = tex2Dlod(_PreviousWeightedResult, float4(0.5f, 0.5f, 0.0f, 0.0f));
		
		depth = previousDepth + (depth - previousDepth) * _DownsampleWeightedAdaptionSpeed;	
		depth = -min(-depth, 0.0f);
		
		return float4(depth, 0.0f, 0.0f, 0.0f);
	}
	
	float4 Downsample(v2f i) : SV_Target0
	{
		float2 depthAndWeight = tex2Dlod(_MainTex, float4(i.uv, 0.0f, 0.0f)).xy;
		//depthAndWeight *= 4.0f;		
		return float4(depthAndWeight, 0.0f, 0.0f);
	}
	
	float4 PointRangeVisualization(v2f i) : SV_Target0 
	{
		float4 mainTex = tex2Dlod(_MainTex, float4(i.uv, 0.0f, 0.0f));		
		float4 src = tex2Dlod(_HalfResSourceDepthTexture, float4(i.uv, 0.0f, 0.0f));	
		return float4(src.xyz * mainTex.y, 1.0f);
	}
	 
	float4 FocalDistanceVisualization(v2f i) : SV_Target0
	{
		float4 sampleDepth 	= tex2Dlod(_HalfResSourceDepthTexture, float4(i.uv, 0.0f, 0.0f)).w;
		float CoC = CoCFromDepth(sampleDepth, GetFocalDistance());		
		return float4(CoC.xxx / MAX_COC_RADIUS, 1.0f);
	}
	
	float4 BilateralAlpha(v2f i) : SV_Target0
	{
		float centerDepth = tex2Dlod(_TapsTexture, float4(i.uv, 0.0f, 0.0f)).w;
		
		float2 uvOffsets[5];
		uvOffsets[0] = 0.0f;
		uvOffsets[1] = float2(-InvHalfResSize.x, -InvHalfResSize.y);
		uvOffsets[2] = float2(-InvHalfResSize.x, InvHalfResSize.y);
		uvOffsets[3] = float2(InvHalfResSize.x, -InvHalfResSize.y);
		uvOffsets[4] = float2(InvHalfResSize.x, InvHalfResSize.y);
		
		float alphaAccum = 0.0f;
		float weightAccum = 0.0f;
		
		SCION_UNROLL for (int k = 0; k < 5; k++)
		{
			float2 uv = i.uv + uvOffsets[0];
			float sampleDepth;
			
			if (k == 0) sampleDepth = centerDepth;
			else sampleDepth = tex2Dlod(_TapsTexture, float4(uv, 0.0f, 0.0f)).w;
			
			float alphaSample = tex2Dlod(_AlphaTexture, float4(uv, 0.0f, 0.0f)).x;
			float weight = 1.0f;// / (abs(centerDepth - sampleDepth) + 1e-3f);
			
			alphaAccum = alphaAccum + alphaSample * weight;
			weightAccum = weightAccum + weight;
		}
		
		return (alphaAccum / weightAccum).xxxx;
	}
	
	float4 DepthBufferCopy(v2f i) : SV_Target0
	{
		float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));
		return depth.xxxx;
	}
	 
	ENDCG	

	Category 
	{
		Subshader 
		{
		    ZTest Off
		    Cull Off
		    ZWrite Off
		    Blend Off
		    Fog { Mode off }
			Pass 
			{
				Name "TiledDataHorizontal" //Pass 0
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment TiledDataHorizontal
				#pragma fragmentoption ARB_precision_hint_fastest 		
				ENDCG	
			}
			Pass 
			{
				Name "TiledDataVertical" //Pass 1
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment TiledDataVertical
				#pragma fragmentoption ARB_precision_hint_fastest 			
				ENDCG	
			}
			Pass 
			{
				Name "NeighbourhoodDataGather" //Pass 2
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment NeighbourhoodDataGather
				#pragma fragmentoption ARB_precision_hint_fastest 	
				#pragma multi_compile SC_EXPOSURE_AUTO SC_EXPOSURE_MANUAL	
				#pragma multi_compile SC_DOF_FOCUS_MANUAL SC_DOF_FOCUS_RANGE SC_DOF_FOCUS_CENTER			
				ENDCG	
			}
			Pass 
			{
				Name "MedianFilter" //Pass 3
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment MedianFilter
				#pragma fragmentoption ARB_precision_hint_fastest 			
				ENDCG	
			}
			Pass 
			{
				Name "PrefilterSource" //Pass 4
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment PrefilterSource
				#pragma fragmentoption ARB_precision_hint_fastest 
				#pragma multi_compile SC_EXPOSURE_AUTO SC_EXPOSURE_MANUAL	
				#pragma multi_compile SC_DOF_FOCUS_MANUAL SC_DOF_FOCUS_RANGE SC_DOF_FOCUS_CENTER		
				#pragma multi_compile __ SC_DOF_MASK_ON		
				ENDCG	
			}
			Pass 
			{
				Name "BlurTapPass49" //Pass 5
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment BlurTapPass
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile SC_EXPOSURE_AUTO SC_EXPOSURE_MANUAL	
				#pragma multi_compile SC_DOF_FOCUS_MANUAL SC_DOF_FOCUS_RANGE SC_DOF_FOCUS_CENTER	
				#pragma multi_compile __ SC_DOF_MASK_ON
				
				#define TAPS_ARRAY ConcentricTapsNormalized49
				#define TAPS_ARRAY_SIZE 49
				#define SCION_DOF_49_SAMPLES
				
				#include "DepthOfFieldImpl.cginc"
				
				#undef SCION_DOF_49_SAMPLES
						
				ENDCG	
			}
			Pass 
			{
				Name "UpsamplePass" //Pass 6
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment UpsamplePass
				#pragma fragmentoption ARB_precision_hint_fastest 	
				#pragma multi_compile SC_EXPOSURE_AUTO SC_EXPOSURE_MANUAL	
				#pragma multi_compile SC_DOF_FOCUS_MANUAL SC_DOF_FOCUS_RANGE SC_DOF_FOCUS_CENTER	
				#pragma multi_compile __ SC_DOF_MASK_ON		
				ENDCG	
			}	
			Pass 
			{
				Name "DownsampleWeighted" //Pass 7 
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment DownsampleWeighted
				#pragma fragmentoption ARB_precision_hint_fastest 			
				ENDCG	
			}
			Pass 
			{
				Name "DownsampleWeightedFinalPass" //Pass 8
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment DownsampleWeightedFinalPass
				#pragma fragmentoption ARB_precision_hint_fastest 			
				ENDCG	
			}
			Pass 
			{
				Name "PointRangeVisualization" //Pass 9
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment PointRangeVisualization
				#pragma fragmentoption ARB_precision_hint_fastest 			
				ENDCG	
			}	
			Pass 
			{
				Name "Downsample" //Pass 10
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment Downsample
				#pragma fragmentoption ARB_precision_hint_fastest 			
				ENDCG	
			}	
			Pass 
			{
				Name "Presort" //Pass 11
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment Presort
				#pragma fragmentoption ARB_precision_hint_fastest 	
				#pragma multi_compile SC_EXPOSURE_AUTO SC_EXPOSURE_MANUAL	
				#pragma multi_compile SC_DOF_FOCUS_MANUAL SC_DOF_FOCUS_RANGE SC_DOF_FOCUS_CENTER			
				ENDCG	
			}
			Pass 
			{
				Name "BlurTapPass25" //Pass 12
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment BlurTapPass
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile SC_EXPOSURE_AUTO SC_EXPOSURE_MANUAL	
				#pragma multi_compile SC_DOF_FOCUS_MANUAL SC_DOF_FOCUS_RANGE SC_DOF_FOCUS_CENTER	
				#pragma multi_compile __ SC_DOF_MASK_ON
				
				#define TAPS_ARRAY ConcentricTapsNormalized49
				#define TAPS_ARRAY_SIZE 49
				#define SCION_DOF_25_SAMPLES
				
				#include "DepthOfFieldImpl.cginc"
				
				#undef SCION_DOF_25_SAMPLES
						
				ENDCG	
			}
			Pass 
			{
				Name "FocalDistanceVisualization" //Pass 13
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment FocalDistanceVisualization
				#pragma fragmentoption ARB_precision_hint_fastest 	
				#pragma multi_compile SC_EXPOSURE_AUTO SC_EXPOSURE_MANUAL	
				#pragma multi_compile SC_DOF_FOCUS_MANUAL SC_DOF_FOCUS_RANGE SC_DOF_FOCUS_CENTER	
				ENDCG	
			}		
			Pass 
			{
				Name "BilateralAlpha" //Pass 14
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment BilateralAlpha
				#pragma fragmentoption ARB_precision_hint_fastest 	
				ENDCG	
			}		
			Pass 
			{
				Name "DepthBufferCopy" //Pass 15
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment DepthBufferCopy
				#pragma fragmentoption ARB_precision_hint_fastest 	
				ENDCG	
			}			
		}
	}	
	Fallback Off	
}





