Shader "Hidden/ScionDownsampling" 
{	    	
 	Properties 
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
    
	CGINCLUDE
	#include "UnityCG.cginc" 
	#include "../ShaderIncludes/ScionCommon.cginc" 
	
	uniform sampler2D _MainTex;
	uniform float4 _MainTex_TexelSize;
    	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};	
		
	v2f vert(appdata_img v)
	{
		v2f o;
		
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;
		#if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0.0f) o.uv.y = 1.0f - o.uv.y; 
		#endif
		
		return o; 
	}

	float4 DownsampleWeighted(v2f i) : SV_Target0
	{	
		const float2 texel = _MainTex_TexelSize.xy;		
			
		float4 center00 = LumaWeighted(tex2D(_MainTex, i.uv - texel));
		float4 center01 = LumaWeighted(tex2D(_MainTex, i.uv + float2(-texel.x, texel.y)));
		float4 center10 = LumaWeighted(tex2D(_MainTex, i.uv + float2(texel.x, -texel.y)));
		float4 center11 = LumaWeighted(tex2D(_MainTex, i.uv + texel));
		
		float4 box00_00 = LumaWeighted(tex2D(_MainTex, i.uv - 2.0f * texel));
		float4 box00_01 = LumaWeighted(tex2D(_MainTex, i.uv - 2.0f * float2(texel.x, 0.0f)));
		float4 box00_10 = LumaWeighted(tex2D(_MainTex, i.uv - 2.0f * float2(0.0f, texel.y)));
		float4 box00_11 = LumaWeighted(tex2D(_MainTex, i.uv));
		
		float4 box01_00 = box00_01;
		float4 box01_01 = LumaWeighted(tex2D(_MainTex, i.uv + 2.0f * float2(-texel.x, texel.y)));
		float4 box01_10 = box00_11;
		float4 box01_11 = LumaWeighted(tex2D(_MainTex, i.uv + 2.0f * float2(0.0f, texel.y)));
		
		float4 box10_00 = box00_10;
		float4 box10_01 = box00_11;
		float4 box10_10 = LumaWeighted(tex2D(_MainTex, i.uv + 2.0f * float2(texel.x, -texel.y)));
		float4 box10_11 = LumaWeighted(tex2D(_MainTex, i.uv + 2.0f * float2(texel.x, 0.0f)));
		
		float4 box11_00 = box00_11;
		float4 box11_01 = box01_11;
		float4 box11_10 = box10_11;
		float4 box11_11 = LumaWeighted(tex2D(_MainTex, i.uv + 2.0f * texel));
		
		float4 center = (center00 + center01 + center10 + center11) * 0.25f;		
		float4 box00 = (box00_00 + box00_01 + box00_10 + box00_11) * 0.25f;		
		float4 box01 = (box01_00 + box01_01 + box01_10 + box01_11) * 0.25f;		
		float4 box10 = (box10_00 + box10_01 + box10_10 + box10_11) * 0.25f;		
		float4 box11 = (box11_00 + box11_01 + box11_10 + box11_11) * 0.25f;
		
		center.xyz = center.xyz / center.w;
		box00.xyz = box00.xyz / box00.w;
		box01.xyz = box01.xyz / box01.w;
		box10.xyz = box10.xyz / box10.w;
		box11.xyz = box11.xyz / box11.w;
		
		#if 0
		float3 final = center.xyz * 0.5f + (box00.xyz + box01.xyz + box10.xyz + box11.xyz) * 0.125f;
		#else
		float3 final = LumaWeightedLerp(center.xyz, (box00.xyz + box01.xyz + box10.xyz + box11.xyz) * 0.25f, 0.5f);
		#endif
		final = -min(-final, 0.0f);
		
		return float4(final, 1.0f);
	}	
	
	uniform sampler2D _HalfResDepth;
	uniform sampler2D _CameraDepthTexture;
	
	float BilateralWeight(float centerDepth, float sampleDepth)
	{
		return 1.0f / (abs(centerDepth - sampleDepth) + 1e-2f);
	}
	
	float BilateralWeight(float centerDepth, float2 uv)
	{
		float sampleDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
		return BilateralWeight(centerDepth, sampleDepth);
	}
		
	float4 DoFLumaWeighted(float4 clr, bool useLumaWeight)
	{
		//Compiler should flatten
		if (useLumaWeight == false) return float4(clr.xyz, 1.0f);
		
		float weight = LumaWeight(clr);
		return float4(clr.xyz * weight, weight);
	}

	float4 DownsampleForDepthOfField(v2f i) : SV_Target0
	{					
		//How samples are done
		// xx		   (0)(1)
		//xxxx		(2)(3)(4)(5)
		//xxxx		(6)(7)(8)(9)
		// xx		  (10)(11)
		
		const float2 texel = InvFullResSize;	
		const bool USE_LUMA_WEIGHT = true;
		
		//Samples 3, 4, 7 and 8
		float centerDepths[4]; 
		float2 centerUV[4];
		centerUV[0] = i.uv + float2(-texel.x, texel.y) * 0.5f;	//Sample 3
		centerUV[1] = i.uv + float2(texel.x, texel.y) * 0.5f;	//Sample 4
		centerUV[2] = i.uv + float2(-texel.x, -texel.y) * 0.5f;	//Sample 7
		centerUV[3] = i.uv + float2(texel.x, -texel.y) * 0.5f;	//Sample 8
		
		SCION_UNROLL for (int k = 0; k < 4; k++)
		{
			centerDepths[k] = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, centerUV[k]);
		}
		
		//Outputs color from same pixel as chosen depth if this is one (furthest depth always)
		//Useful for debugging (removing variables), but has less quality
		#if 0
		float4 output = centerDepths[0] > centerDepths[1] ? 
						float4(tex2D(_MainTex, centerUV[0]).xyz, centerDepths[0]) : 
						float4(tex2D(_MainTex, centerUV[1]).xyz, centerDepths[1]);
						
		output = output.w > centerDepths[2] ? output : float4(tex2D(_MainTex, centerUV[2]).xyz, centerDepths[2]);
		output = output.w > centerDepths[3] ? output : float4(tex2D(_MainTex, centerUV[3]).xyz, centerDepths[3]);
		output.w = LinearEyeDepth(output.w);
		return output;
		#endif
		
		#if 1 //Furthest
		float chosenDepth = VectorMax(float4(centerDepths[0], centerDepths[1], centerDepths[2], centerDepths[3]));
		#else //Closest
		float chosenDepth = VectorMin(float4(centerDepths[0], centerDepths[1], centerDepths[2], centerDepths[3]));
		#endif		
		chosenDepth = LinearEyeDepth(chosenDepth);
		
		float2 uv;
		
		//First use the 4 depths we've already sampled (compiler should optimize it this way anyway, but this is more explicit)
		uv = i.uv + texel * float2(-0.5f, 0.5f);
		float4 sample3 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, centerDepths[0]);
		
		uv = i.uv + texel * float2(0.5f, 0.5f);
		float4 sample4 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, centerDepths[1]);
		
		uv = i.uv + texel * float2(-0.5f, -0.5f);
		float4 sample7 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, centerDepths[2]);
		
		uv = i.uv + texel * float2(0.5f, -0.5f);
		float4 sample8 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, centerDepths[3]);
				
		//Then do the rest of the reads		
		uv = i.uv + texel * float2(-0.5f, 1.5f);
		float4 sample0 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, uv);
		
		uv = i.uv + texel * float2(0.5f, 1.5f);
		float4 sample1 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, uv);
		
		uv = i.uv + texel * float2(-1.5f, 0.5f);
		float4 sample2 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, uv);
		
		uv = i.uv + texel * float2(1.5f, 0.5f);
		float4 sample5 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, uv);
			
		uv = i.uv + texel * float2(-1.5f, -0.5f);
		float4 sample6 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, uv);
		
		uv = i.uv + texel * float2(1.5f, -0.5f);
		float4 sample9 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, uv);
		
		uv = i.uv + texel * float2(-0.5f, -1.5f);
		float4 sample10 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, uv);
		
		uv = i.uv + texel * float2(0.5f, -1.5f);
		float4 sample11 = DoFLumaWeighted(tex2D(_MainTex, uv), USE_LUMA_WEIGHT) * BilateralWeight(chosenDepth, uv);
		
		//box0 = 0,1,3,4
		//box1 = 2,3,6,7
		//box2 = 4,5,8,9
		//box3 = 7,8,10,11		
		float4 box0 = sample0 + sample1 + sample2 + sample3; 	
		float4 box1 = sample2 + sample3 + sample6 + sample7; 		
		float4 box2 = sample4 + sample5 + sample8 + sample9; 	
		float4 box3 = sample7 + sample8 + sample10 + sample11; 	
		
		#if 0 //Emphasis on luma ignoring depth, WILL cause fringing	
		box0 = box0 / box0.w;
		box1 = box1 / box1.w;	
		box2 = box2 / box2.w;
		box3 = box3 / box3.w;
		
		float3 boxLerp0 = LumaWeightedLerp(box0, box3, 0.5f);
		float3 boxLerp1 = LumaWeightedLerp(box1, box2, 0.5f);		
		float3 final = LumaWeightedLerp(boxLerp0, boxLerp1, 0.5f);
		#else
		float4 sum = box0 + box1 + box2 + box3;
		float3 final = sum.xyz / sum.w;
		#endif
		
		final = -min(-final, 0.0f);		
		return float4(final, chosenDepth);	
	}		
	 
	float4 DownsampleDepth(v2f i) : SV_Target0
	{
		float2 halfTexel = InvFullResSize * 0.5f;
		
		float depth00 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, float2(i.uv.x - halfTexel.x, i.uv.y - halfTexel.y));
		float depth10 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, float2(i.uv.x + halfTexel.x, i.uv.y - halfTexel.y));
		float depth01 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, float2(i.uv.x - halfTexel.x, i.uv.y + halfTexel.y));
		float depth11 = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, float2(i.uv.x + halfTexel.x, i.uv.y + halfTexel.y));
				
		//Linearize the depth
		//return Linear01Depth(depth00);
		//return Linear01Depth(VectorMax(float4(depth00, depth10, depth01, depth11)));
		return Linear01Depth(VectorMin(float4(depth00, depth10, depth01, depth11)));
	}		
	 
	float4 DownsampleMinFilter(v2f i) : SV_Target0
	{
		float2 halfTexel = InvFullResSize * 0.5f;
		
		float sample00 = tex2Dlod(_MainTex, float4(i.uv.x - halfTexel.x, i.uv.y - halfTexel.y, 0.0f, 0.0f)).x;
		float sample10 = tex2Dlod(_MainTex, float4(i.uv.x + halfTexel.x, i.uv.y - halfTexel.y, 0.0f, 0.0f)).x;
		float sample01 = tex2Dlod(_MainTex, float4(i.uv.x - halfTexel.x, i.uv.y + halfTexel.y, 0.0f, 0.0f)).x;
		float sample11 = tex2Dlod(_MainTex, float4(i.uv.x + halfTexel.x, i.uv.y + halfTexel.y, 0.0f, 0.0f)).x;
		
		return VectorMin(float4(sample00, sample10, sample01, sample11));
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
				Name "DownsampleWeighted" //Pass 0
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment DownsampleWeighted
				#pragma fragmentoption ARB_precision_hint_fastest 			
				ENDCG	
			}
			Pass 
			{
				Name "DownsampleForDepthOfField" //Pass 1
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment DownsampleForDepthOfField
				#pragma fragmentoption ARB_precision_hint_fastest 			
				ENDCG	
			}
			Pass 
			{
				Name "DownsampleDepth" //Pass 2
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment DownsampleDepth
				#pragma fragmentoption ARB_precision_hint_fastest 			
				ENDCG	
			}
			Pass 
			{
				Name "DownsampleMinFilter" //Pass 3
			
				CGPROGRAM	
				#pragma target 3.0		
				#pragma vertex vert
				#pragma fragment DownsampleMinFilter
				#pragma fragmentoption ARB_precision_hint_fastest 			
				ENDCG	
			}
		}
	}	
	Fallback Off	
}





