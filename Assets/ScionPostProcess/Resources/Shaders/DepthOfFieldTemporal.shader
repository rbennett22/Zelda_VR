// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/ScionDepthOfFieldTemporal" 
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
		float4 pos 		: SV_POSITION;
		float2 uv 		: TEXCOORD0;
		float3 worldRay : TEXCOORD1;
	};	
		
	v2f vert(appdata_img v)
	{
		v2f o;
		
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		#if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0.0f) o.uv.y = 1.0f - o.uv.y; 
		#endif
		
		o.worldRay = _FrustumCornerBottomLeftVP + o.uv.x * _FrustumCornerWidthVP + o.uv.y * _FrustumCornerHeightVP;
	
		return o; 
	}
	
	uniform sampler2D _TapsHistoryTexture;
	uniform sampler2D _AlphaHistoryTexture;
	uniform sampler2D _TapsCurrentTexture;
	uniform sampler2D _AlphaCurrentTexture;
	uniform float4x4 _PreviousViewProjection;
	uniform float _TemporalBlendFactor;
	
	void GetTemporalVariables(float currentDepth, v2f i, out float2 prevUV, out bool offScreen)
	{
		float3 worldPosition = CameraPosition + i.worldRay * currentDepth;					
		float4 prevProj = mul(_PreviousViewProjection, float4(worldPosition, 1.0f));
		float2 prevClip = prevProj.xy / prevProj.w;		
		
		prevUV = prevClip * 0.5f + 0.5f;	
		offScreen = max(abs(prevClip.x), abs(prevClip.y)) >= 1.0f ? true : false;
	}
	
	float4 TemporalBlend(v2f i) : SV_Target0
	{		
		float4 currentSample = tex2Dlod(_TapsCurrentTexture, float4(i.uv, 0.0f, 0.0f));
		float currentDepth = currentSample.w;
		
		float2 prevUV;
		bool offScreen;
		GetTemporalVariables(currentDepth, i, /*out*/ prevUV, /*out*/ offScreen);
		//return float4(prevUV,0,0);
		//return float4(saturate(worldPosition*0.1f),0);
		
		//Samples laid out like this
		//012
		//345
		//678	
		float2 uv[9];
		uv[0] = float2(i.uv.x - InvHalfResSize.x, 	i.uv.y + InvHalfResSize.y);
		uv[1] = float2(i.uv.x, 						i.uv.y + InvHalfResSize.y);
		uv[2] = float2(i.uv.x + InvHalfResSize.x, 	i.uv.y + InvHalfResSize.y);		
		uv[3] = float2(i.uv.x - InvHalfResSize.x, 	i.uv.y);
		uv[4] = float2(i.uv.x, 						i.uv.y);
		uv[5] = float2(i.uv.x + InvHalfResSize.x, 	i.uv.y);		
		uv[6] = float2(i.uv.x - InvHalfResSize.x, 	i.uv.y - InvHalfResSize.y);
		uv[7] = float2(i.uv.x, 						i.uv.y - InvHalfResSize.y);
		uv[8] = float2(i.uv.x + InvHalfResSize.x, 	i.uv.y - InvHalfResSize.y);					
											
		float3 currentTextureSamples[9];
		
		int k = 0;
		SCION_UNROLL for (k = 0; k < 9; k++)
		{
			//Compiler should flatten branch
			if (k == 4) currentTextureSamples[4] = currentSample.xyz;
			else currentTextureSamples[k] = tex2Dlod(_TapsCurrentTexture, float4(uv[k], 0.0f, 0.0f)).xyz;			
		}
		
		//Plus includes center	
		// 1 
		//345
		// 7	
		float3 sampleMinPlus = min(min(min(currentTextureSamples[1], currentTextureSamples[3]), min(currentTextureSamples[4], currentTextureSamples[5])), currentTextureSamples[7]);		
		float3 sampleMaxPlus = max(max(max(currentTextureSamples[1], currentTextureSamples[3]), max(currentTextureSamples[4], currentTextureSamples[5])), currentTextureSamples[7]);	
			
		//Cross does not include center
		//0 2
		//   
		//6 8	
		float3 sampleMinCross = min(min(currentTextureSamples[0], currentTextureSamples[2]), min(currentTextureSamples[6], currentTextureSamples[8]));		
		float3 sampleMaxCross = max(max(currentTextureSamples[0], currentTextureSamples[2]), max(currentTextureSamples[6], currentTextureSamples[8]));	
		
		float3 totalMin = min(sampleMinCross, sampleMinPlus); 
		float3 totalMax = max(sampleMaxCross, sampleMaxPlus); 
				
		float3 fragmentHistory = tex2Dlod(_TapsHistoryTexture, float4(prevUV, 0.0f, 0.0f)).xyz;		
		//return float4(fragmentHistory, currentDepth);
			
		float3 clampedHistory = clamp(fragmentHistory, totalMin, totalMax);			
		clampedHistory = lerp(currentTextureSamples[4], clampedHistory, _TemporalBlendFactor);		
			
		float3 finalColor = offScreen == true ? currentTextureSamples[4] : clampedHistory;
		finalColor = -min(-finalColor, 0.0);	
		
		return float4(finalColor, currentDepth);
	}	
	
	struct WithAlphaOutput
	{
		float4 clr0 : SV_Target0;
		float4 clr1 : SV_Target1;
	};
	
	WithAlphaOutput TemporalBlendWithAlpha(v2f i)
	{		
		float4 currentSample = tex2Dlod(_TapsCurrentTexture, float4(i.uv, 0.0f, 0.0f));
		float currentDepth = currentSample.w;
		
		float2 prevUV;
		bool offScreen;
		GetTemporalVariables(currentDepth, i, /*out*/ prevUV, /*out*/ offScreen);
		//return float4(prevUV,0,0);
		//return float4(saturate(worldPosition*0.1f),0);
		
		//Samples laid out like this
		//012
		//345
		//678	
		float2 uv[9];
		uv[0] = float2(i.uv.x - InvHalfResSize.x, 	i.uv.y + InvHalfResSize.y);
		uv[1] = float2(i.uv.x, 						i.uv.y + InvHalfResSize.y);
		uv[2] = float2(i.uv.x + InvHalfResSize.x, 	i.uv.y + InvHalfResSize.y);		
		uv[3] = float2(i.uv.x - InvHalfResSize.x, 	i.uv.y);
		uv[4] = float2(i.uv.x, 						i.uv.y);
		uv[5] = float2(i.uv.x + InvHalfResSize.x, 	i.uv.y);		
		uv[6] = float2(i.uv.x - InvHalfResSize.x, 	i.uv.y - InvHalfResSize.y);
		uv[7] = float2(i.uv.x, 						i.uv.y - InvHalfResSize.y);
		uv[8] = float2(i.uv.x + InvHalfResSize.x, 	i.uv.y - InvHalfResSize.y);					
											
		float4 currentTextureSamples[9];
		
		int k = 0;
		SCION_UNROLL for (k = 0; k < 9; k++)
		{
			//Compiler should flatten branch
			if (k == 4) currentTextureSamples[4] = float4(currentSample.xyz, tex2Dlod(_AlphaCurrentTexture, float4(uv[k], 0.0f, 0.0f)).x);
			else currentTextureSamples[k] = float4(tex2Dlod(_TapsCurrentTexture, float4(uv[k], 0.0f, 0.0f)).xyz, tex2Dlod(_AlphaCurrentTexture, float4(uv[k], 0.0f, 0.0f)).x);			
		}
		
		//Plus includes center	
		// 1 
		//345
		// 7	
		float4 sampleMinPlus = min(min(min(currentTextureSamples[1], currentTextureSamples[3]), min(currentTextureSamples[4], currentTextureSamples[5])), currentTextureSamples[7]);		
		float4 sampleMaxPlus = max(max(max(currentTextureSamples[1], currentTextureSamples[3]), max(currentTextureSamples[4], currentTextureSamples[5])), currentTextureSamples[7]);	
			
		//Cross does not include center
		//0 2
		//   
		//6 8	
		float4 sampleMinCross = min(min(currentTextureSamples[0], currentTextureSamples[2]), min(currentTextureSamples[6], currentTextureSamples[8]));		
		float4 sampleMaxCross = max(max(currentTextureSamples[0], currentTextureSamples[2]), max(currentTextureSamples[6], currentTextureSamples[8]));	
		
		float4 totalMin = min(sampleMinCross, sampleMinPlus); 
		float4 totalMax = max(sampleMaxCross, sampleMaxPlus); 
				
		float4 fragmentHistory = float4(tex2Dlod(_TapsHistoryTexture, float4(prevUV, 0.0f, 0.0f)).xyz, tex2Dlod(_AlphaHistoryTexture, float4(prevUV, 0.0f, 0.0f)).x);			
		//return float4(fragmentHistory, currentDepth);
			
		float4 clampedHistory = clamp(fragmentHistory, totalMin, totalMax);			
		//clampedHistory.xyz = lerp(currentTextureSamples[4].xyz, clampedHistory.xyz, _TemporalBlendFactor);
		clampedHistory = lerp(currentTextureSamples[4], clampedHistory, _TemporalBlendFactor);	
			
		float4 finalColor = offScreen == true ? currentTextureSamples[4] : clampedHistory;
		finalColor = -min(-finalColor, 0.0);	
		
		//finalColor = currentTextureSamples[4];
		//finalColor = clampedHistory;
		
		WithAlphaOutput output;
		output.clr0.xyz = finalColor.xyz;
		output.clr0.w = currentDepth;
		output.clr1.x = finalColor.w;
		output.clr1.yzw = 0.0f;
		
		return output;
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
				Name "TemporalBlend" //Pass 0
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment TemporalBlend
				#pragma fragmentoption ARB_precision_hint_fastest 		
				ENDCG	
			}
			Pass 
			{
				Name "TemporalBlendWithAlpha" //Pass 1
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment TemporalBlendWithAlpha
				#pragma fragmentoption ARB_precision_hint_fastest 		
				ENDCG	
			}
		}
	}	
	Fallback Off	
}





