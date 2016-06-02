#pragma warning disable 0162

using UnityEngine;
using System.Collections;

namespace ScionEngine
{
	public class DepthOfField
	{
		private Material m_DoFMat;
		private Material m_DoFMatTemporal;
		private RenderTexture previousPointAverage;

		private Camera m_maskCamera;
		private Camera maskCamera
		{
			get
			{ 
				if (m_maskCamera == null) 
				{
					GameObject camGO = new GameObject();
					camGO.SetActive(false);
					camGO.hideFlags = HideFlags.HideAndDontSave;
					camGO.name = "ScionDoFMaskCamera";					
					m_maskCamera = camGO.AddComponent<Camera>();
					m_maskCamera.enabled = false;
					m_maskCamera.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_maskCamera;
			}
		}
		private static Shader m_maskShader;
		private static Shader maskShader
		{
			get
			{
				if (m_maskShader == null) m_maskShader = Shader.Find("Scion/ScionDepthOfFieldMask");
				return m_maskShader;
			}
		}
		private Transform m_maskCameraTransform;
		private Transform maskCameraTransform
		{
			get
			{
				if (m_maskCameraTransform == null) m_maskCameraTransform = maskCamera.transform;
				return m_maskCameraTransform;
			}
		}
		
		public DepthOfField()
		{
			m_DoFMat = new Material(Shader.Find("Hidden/ScionDepthOfField"));
			m_DoFMat.hideFlags = HideFlags.HideAndDontSave;

			m_DoFMatTemporal = new Material(Shader.Find("Hidden/ScionDepthOfFieldTemporal"));
			m_DoFMatTemporal.hideFlags = HideFlags.HideAndDontSave;
		}
		
		public bool PlatformCompatibility()
		{
			if (Shader.Find("Hidden/ScionDepthOfField").isSupported == false)  
			{ 
				Debug.LogWarning("Depth of Field shader not supported");
				return false;
			}
			if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8) == false)
			{ 
				Debug.LogWarning("R8 texture format not supported");
				return false;
			}
			return true;
		}
		
		public void EndOfFrameCleanup()
		{
			
		}

		private RenderTexture previousTapsTexture = null;
		private RenderTexture previousAlphaTexture = null;
		public RenderTexture RenderDepthOfField(PostProcessParameters postProcessParams, RenderTexture source, RenderTexture downsampledClrDepth, 
		                                        VirtualCamera virtualCamera, RenderTexture exclusionMask)
		{
			if (ShaderSettings.ExposureSettings.IsActive("SC_EXPOSURE_AUTO") == true)
			{
				virtualCamera.BindVirtualCameraTextures(m_DoFMat);
			}

			virtualCamera.BindVirtualCameraParams(m_DoFMat, postProcessParams.cameraParams, postProcessParams.DoFParams.focalDistance, postProcessParams.halfWidth, postProcessParams.isFirstRender);	

			RenderTexture depthCenterAverage = null;
			if (postProcessParams.DoFParams.depthFocusMode == DepthFocusMode.PointAverage)
			{
				depthCenterAverage = PrepatePointAverage(postProcessParams, downsampledClrDepth);
				//ScionPostProcess.ActiveDebug.RegisterTextureForVisualization(depthCenterAverage, false, false, false);
			}	

			RenderTexture tiledData = CreateTiledData(downsampledClrDepth, 
			                                          postProcessParams.preCalcValues.tanHalfFoV,
			                                          postProcessParams.cameraParams.fNumber,
			                                          postProcessParams.DoFParams.focalDistance,
			                                          postProcessParams.DoFParams.focalRange,
			                                          postProcessParams.cameraParams.apertureDiameter,
			                                          postProcessParams.cameraParams.focalLength,
			                                          postProcessParams.DoFParams.maxCoCRadius,
			                                          postProcessParams.cameraParams.nearPlane,
			                                          postProcessParams.cameraParams.farPlane);
			
			//ScionPostProcess.ActiveDebug.RegisterTextureForVisualization(exclusionMask, false, false, false);

			RenderTexture neighbourhoodData 	= TileNeighbourhoodDataGathering(tiledData);
			RenderTexture prefilteredSource 	= PrefilterSource(downsampledClrDepth);		
			RenderTexture.ReleaseTemporary(downsampledClrDepth);
			
			RenderTexture presortTexture		= Presort(prefilteredSource, neighbourhoodData);

			if (postProcessParams.DoFParams.useTemporal == true) UploadTemporalReprojectionVariables(postProcessParams);

			RenderTexture tapsTexture;
			RenderTexture alphaTexture;
			BlurTapPass(prefilteredSource, tiledData, neighbourhoodData, exclusionMask, depthCenterAverage, presortTexture,
			            postProcessParams.DoFParams.quality, out tapsTexture, out alphaTexture);

			const bool temporalAlpha = false;
			
			//Only do temporal super sampling in editor if its playing
			#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying == true && 
				    postProcessParams.DoFParams.useTemporal == true && 
				    previousTapsTexture != null) 
				{
					if (temporalAlpha == true) TemporalPass(ref tapsTexture, ref alphaTexture, previousTapsTexture, previousAlphaTexture);
					else TemporalPass(ref tapsTexture, previousTapsTexture);
				}
			#else
			if (postProcessParams.DoFParams.useTemporal == true && previousTapsTexture != null) 
			{
				if (temporalAlpha == true) TemporalPass(ref tapsTexture, ref alphaTexture, previousTapsTexture, previousAlphaTexture);
				else TemporalPass(ref tapsTexture, previousTapsTexture);
			}
			#endif

			//alphaTexture = BilateralAlphaFilter(alphaTexture, tapsTexture);
			//alphaTexture = MedianFilterPass(alphaTexture);
			
			if (postProcessParams.DoFParams.useMedianFilter == true) tapsTexture = MedianFilterPass(tapsTexture);
			
			if (postProcessParams.DoFParams.visualizeFocalDistance == true) VisualizeFocalDistance(tapsTexture);
			
			//ScionPostProcess.ActiveDebug.RegisterTextureForVisualization(presort, true, false, false);
			//ScionPostProcess.ActiveDebug.RegisterTextureForVisualization(alphaTexture, false, false, false);
			//ScionPostProcess.ActiveDebug.RegisterTextureForVisualization(previousTapsTexture, false, false, false);

			RenderTexture compositedDoF = UpsampleDepthOfField(source, tapsTexture, alphaTexture, neighbourhoodData, exclusionMask);
			RenderTexture.ReleaseTemporary(tiledData);
			RenderTexture.ReleaseTemporary(neighbourhoodData);
			RenderTexture.ReleaseTemporary(prefilteredSource);
			RenderTexture.ReleaseTemporary(presortTexture);

			if (copiedDepthBuffer != null) 		{ RenderTexture.ReleaseTemporary(copiedDepthBuffer); copiedDepthBuffer = null; }			
			if (previousTapsTexture != null) 	{ RenderTexture.ReleaseTemporary(previousTapsTexture); previousTapsTexture = null; }
			if (previousAlphaTexture != null) 	{ RenderTexture.ReleaseTemporary(previousAlphaTexture); previousAlphaTexture = null; }

			if (postProcessParams.DoFParams.useTemporal == true) 
			{
				previousTapsTexture = tapsTexture;
				previousAlphaTexture = alphaTexture;
			}
			else 
			{
				RenderTexture.ReleaseTemporary(tapsTexture);
				RenderTexture.ReleaseTemporary(alphaTexture);
			}

			return compositedDoF;
		}

		private void VisualizeFocalDistance(RenderTexture downsampledClrDepth)
		{
			const int visualizationPassID = 13;

			RenderTexture visualized = RenderTexture.GetTemporary(downsampledClrDepth.width, downsampledClrDepth.height, 0, RenderTextureFormat.ARGB32);
			visualized.filterMode = FilterMode.Bilinear;
			visualized.wrapMode = TextureWrapMode.Clamp;
			
			m_DoFMat.SetTexture("_HalfResSourceDepthTexture", downsampledClrDepth);
			
			ScionGraphics.Blit(visualized, m_DoFMat, visualizationPassID);
			ScionPostProcessBase.ActiveDebug.RegisterTextureForVisualization(visualized, true);
		}

		private void UploadTemporalReprojectionVariables(PostProcessParameters postProcessParams)//Transform cameraTransform, float tanHalfFoV, float aspect, float farClipPlane)
		{
			float dy = postProcessParams.preCalcValues.tanHalfFoV;
			float dx = dy * postProcessParams.cameraParams.aspect;
			
			Vector3 centerVP 	= postProcessParams.cameraTransform.forward;
			Vector3 rightVP 	= postProcessParams.cameraTransform.right * dx;
			Vector3 upVP		= postProcessParams.cameraTransform.up * dy;
			
			m_DoFMatTemporal.SetVector("_FrustumCornerBottomLeftVP", centerVP - rightVP - upVP);
			m_DoFMatTemporal.SetVector("_FrustumCornerWidthVP", rightVP * 2.0f);
			m_DoFMatTemporal.SetVector("_FrustumCornerHeightVP", upVP * 2.0f);
			m_DoFMatTemporal.SetMatrix("_PreviousViewProjection", postProcessParams.cameraParams.previousViewProjection);

			float temporalBlend = postProcessParams.DoFParams.temporalBlend;
			m_DoFMatTemporal.SetFloat("_TemporalBlendFactor", temporalBlend);

			const float temporalIncrement = 1.372f;
			int temporalSteps = postProcessParams.DoFParams.temporalSteps;
			if (temporalSteps > 0) 
			{
				temporalUVOffset = temporalUVOffset + temporalIncrement;
				while (temporalUVOffset > temporalIncrement*temporalSteps - 0.01f) temporalUVOffset -= temporalIncrement*temporalSteps;
			}
			else temporalUVOffset = 0.0f;

			m_DoFMat.SetFloat("_TemporalUVOffset", temporalUVOffset);
		}
		
		private float Min(float val1, float val2) { return val1 > val2 ? val2 : val1; } 
		private float Max(float val1, float val2) { return val1 < val2 ? val2 : val1; } 
		private int Min(int val1, int val2) { return val1 > val2 ? val2 : val1; } 
		private int Max(int val1, int val2) { return val1 < val2 ? val2 : val1; } 
		
		private RenderTexture PrepatePointAverage(PostProcessParameters postProcessParams, RenderTexture downsampledClrDepth)
		{				
			const int weightedDownsamplePassID = 7;
			const int finalPassID = 8;
			const int visualizationPassID = 9;
			const int downsamplePassID = 10;
			const RenderTextureFormat format = RenderTextureFormat.RGHalf;
			const RenderTextureFormat format2 = RenderTextureFormat.RHalf;
			
			//Force at least 10x10 pixels to always effect the depth
			//Because stuff goes south if the circle is too small
			float range = Max(10.0f/postProcessParams.halfWidth, postProcessParams.DoFParams.pointAverageRange);
			
			Vector4 weightedDownsampleParams = new Vector4();
			weightedDownsampleParams.x = Mathf.Clamp01(postProcessParams.DoFParams.pointAveragePosition.x);
			weightedDownsampleParams.y = Mathf.Clamp01(postProcessParams.DoFParams.pointAveragePosition.y);
			weightedDownsampleParams.z = range*range;
			weightedDownsampleParams.w = 1.0f / (range*range);
			m_DoFMat.SetVector("_DownsampleWeightedParams", weightedDownsampleParams);
			
			if (previousPointAverage != null && postProcessParams.isFirstRender == false)
			{
#if UNITY_EDITOR
				if (UnityEditor.EditorApplication.isPlaying == false) m_DoFMat.SetFloat("_DownsampleWeightedAdaptionSpeed", 1.0f);
				else
#endif
				{
					m_DoFMat.SetFloat("_DownsampleWeightedAdaptionSpeed", 1.0f - Mathf.Exp(-Time.deltaTime * postProcessParams.DoFParams.depthAdaptionSpeed));
				}
				m_DoFMat.SetTexture("_PreviousWeightedResult", previousPointAverage);
			}
			else
			{
				m_DoFMat.SetFloat("_DownsampleWeightedAdaptionSpeed", 1.0f);
				m_DoFMat.SetTexture("_PreviousWeightedResult", null);
			}
			
			downsampledClrDepth.filterMode = FilterMode.Bilinear;
			
			const int maxSize = 1;
			int texWidth = Max(postProcessParams.halfWidth / 2, maxSize);
			int texHeight = Max(postProcessParams.halfHeight / 2, maxSize);
			
			RenderTexture weightedDownsample = RenderTexture.GetTemporary(texWidth, texHeight, 0, format);
			weightedDownsample.filterMode = FilterMode.Bilinear;
			weightedDownsample.wrapMode = TextureWrapMode.Clamp;
			
			Graphics.Blit(downsampledClrDepth, weightedDownsample, m_DoFMat, weightedDownsamplePassID);
			
			if (postProcessParams.DoFParams.visualizePointFocus == true)
			{
				RenderTexture visTexture = RenderTexture.GetTemporary(texWidth, texHeight, 0, RenderTextureFormat.ARGB32);
				m_DoFMat.SetTexture("_HalfResSourceDepthTexture", downsampledClrDepth);
				Graphics.Blit(weightedDownsample, visTexture, m_DoFMat, visualizationPassID);
				//Graphics.Blit(visTexture, dest);
				//RenderTexture.ReleaseTemporary(visTexture);
				ScionPostProcess.ActiveDebug.RegisterTextureForVisualization(visTexture, true, true, false);
			}
			
			RenderTexture input = weightedDownsample;
			int largestSide = Max(texWidth, texHeight);
			
			while (largestSide > maxSize)
			{
				texWidth = Max(maxSize, texWidth/2 + texWidth%2);
				texHeight = Max(maxSize, texHeight/2 + texHeight%2);
				largestSide = largestSide / 2 + largestSide%2;
				
				RenderTexture downsample;
				if (largestSide > maxSize)
				{
					downsample = RenderTexture.GetTemporary(texWidth, texHeight, 0, format);
					downsample.filterMode = FilterMode.Bilinear;
					downsample.wrapMode = TextureWrapMode.Clamp;
					Graphics.Blit(input, downsample, m_DoFMat, downsamplePassID);
				}
				else //Final pass
				{
					downsample = RenderTexture.GetTemporary(texWidth, texHeight, 0, format2);
					downsample.filterMode = FilterMode.Bilinear;
					downsample.wrapMode = TextureWrapMode.Clamp;
					Graphics.Blit(input, downsample, m_DoFMat, finalPassID);
				}
				
				RenderTexture.ReleaseTemporary(input);
				input = downsample;
			}
			
			RenderTexture pointAverage = input;
			if (previousPointAverage != null) RenderTexture.ReleaseTemporary(previousPointAverage);
			previousPointAverage = pointAverage;

			downsampledClrDepth.filterMode = FilterMode.Point;
			
			return pointAverage;
		}

		private RenderTexture copiedDepthBuffer = null;
		
		public RenderTexture RenderExclusionMask(int width, int height, Camera camera, Transform cameraTransform, LayerMask mask)
		{
			//First copy entire depth buffer (it's horrible I know, but no other choice)
			copiedDepthBuffer = RenderTexture.GetTemporary (width, height, 0, RenderTextureFormat.RHalf);
			ScionGraphics.Blit(copiedDepthBuffer, m_DoFMat, 15);
			Shader.SetGlobalTexture("_ScionCopiedFullResDepth", copiedDepthBuffer);

			RenderTexture exclusionMask = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.R8);
			exclusionMask.filterMode = FilterMode.Point;
			exclusionMask.wrapMode = TextureWrapMode.Clamp;

			maskCameraTransform.position = cameraTransform.position;
			maskCameraTransform.rotation = cameraTransform.rotation;

			maskCamera.CopyFrom(camera);
			maskCamera.cullingMask = mask;
			maskCamera.SetTargetBuffers(exclusionMask.colorBuffer, exclusionMask.depthBuffer);
			maskCamera.clearFlags = CameraClearFlags.SolidColor;
			maskCamera.backgroundColor = Color.white;
			maskCamera.renderingPath = RenderingPath.Forward;
			maskCamera.hdr = false;
			maskCamera.RenderWithShader(maskShader, "RenderType");

			return exclusionMask;
		}

		private RenderTexture CreateTiledData(RenderTexture downsampledClrDepth, float tanHalfFoV, float fNumber, float focalDistance, float focalRange,
		                                      float apertureDiameter, float focalLength, float maxCoCRadius, float nearPlane, float farPlane)
		{
			int tileWidth = (downsampledClrDepth.width + 9) / 10;
			int tileHeight = (downsampledClrDepth.height + 9) / 10;		

			float CoCScale = apertureDiameter * focalLength * focalDistance / (focalDistance - focalLength);
			float CoCBias = -apertureDiameter * focalLength / (focalDistance - focalLength);

			float toPixels = ScionUtility.CoCToPixels(downsampledClrDepth.width);
			CoCScale *= toPixels;
			CoCBias *= toPixels;

			Vector4 CoCParams1 = new Vector4();
			CoCParams1.x = CoCScale;
			CoCParams1.y = CoCBias;
			CoCParams1.z = focalDistance;
			CoCParams1.w = focalRange * 0.5f;
			m_DoFMat.SetVector("_CoCParams1", CoCParams1);
			
			Vector4 CoCParams2 = new Vector4();
			CoCParams2.x = maxCoCRadius * 0.5f; //We're in half res, so halve it
			m_DoFMat.SetVector("_CoCParams2", CoCParams2);

			m_DoFMat.SetFloat("_CoCUVOffset", 1.0f / downsampledClrDepth.width); //Width for horizontal
			m_DoFMat.SetTexture("_HalfResSourceDepthTexture", downsampledClrDepth);

			RenderTexture tiledDataHorizontal = RenderTexture.GetTemporary(tileWidth, downsampledClrDepth.height, 0, RenderTextureFormat.RGHalf);
			tiledDataHorizontal.filterMode = FilterMode.Point;
			tiledDataHorizontal.wrapMode = TextureWrapMode.Clamp;

			RenderTexture tiledData = RenderTexture.GetTemporary(tileWidth, tileHeight, 0, RenderTextureFormat.RGHalf);
			tiledData.filterMode = FilterMode.Point;
			tiledData.wrapMode = TextureWrapMode.Clamp;

			downsampledClrDepth.filterMode = FilterMode.Point;

			ScionGraphics.Blit(tiledDataHorizontal, m_DoFMat, 0);

			m_DoFMat.SetTexture("_HorizontalTileResult", tiledDataHorizontal);
			m_DoFMat.SetFloat("_CoCUVOffset", 1.0f / downsampledClrDepth.height); //Height for vertical

			ScionGraphics.Blit(tiledData, m_DoFMat, 1);
			RenderTexture.ReleaseTemporary(tiledDataHorizontal);

			return tiledData;
		}

		private RenderTexture TileNeighbourhoodDataGathering(RenderTexture tiledData)
		{
			Vector4 neighbourhoodParams = new Vector4();
			neighbourhoodParams.x = 1.0f / tiledData.width;
			neighbourhoodParams.y = 1.0f / tiledData.height;
			m_DoFMat.SetVector("_NeighbourhoodParams", neighbourhoodParams);

			RenderTexture neighbourhoodData = RenderTexture.GetTemporary(tiledData.width, tiledData.height, 0, RenderTextureFormat.RGHalf);
			neighbourhoodData.filterMode = FilterMode.Point;
			neighbourhoodData.wrapMode = TextureWrapMode.Clamp;

			m_DoFMat.SetTexture("_TiledData", tiledData);
			ScionGraphics.Blit(neighbourhoodData, m_DoFMat, 2);

			return neighbourhoodData;
		}

		private RenderTexture Presort(RenderTexture downsampledClrDepth, RenderTexture neighbourhoodData)
		{
			RenderTexture presort = RenderTexture.GetTemporary(downsampledClrDepth.width, downsampledClrDepth.height, 0, RenderTextureFormat.ARGB2101010);
			presort.filterMode = FilterMode.Point;
			presort.wrapMode = TextureWrapMode.Clamp;
			
			m_DoFMat.SetTexture("_HalfResSourceDepthTexture", downsampledClrDepth);
			m_DoFMat.SetTexture("_TiledNeighbourhoodData", neighbourhoodData);

			ScionGraphics.Blit(presort, m_DoFMat, 11);

			return presort;
		}

		private RenderTexture PrefilterSource(RenderTexture downsampledClrDepth)
		{
			m_DoFMat.SetTexture("_HalfResSourceDepthTexture", downsampledClrDepth);
			downsampledClrDepth.filterMode = FilterMode.Point;
			
			RenderTexture prefilteredSource = RenderTexture.GetTemporary(downsampledClrDepth.width, downsampledClrDepth.height, 0, downsampledClrDepth.format);
			prefilteredSource.filterMode = FilterMode.Point;
			prefilteredSource.wrapMode = TextureWrapMode.Clamp;
			
			ScionGraphics.Blit(prefilteredSource, m_DoFMat, 4);
			return prefilteredSource;
		}

		private float temporalUVOffset = 0.0f;
		private RenderBuffer[] renderBuffers = new RenderBuffer[2];
		private void BlurTapPass(RenderTexture prefilteredSource, RenderTexture tiledData, RenderTexture neighbourhoodData, 
		                         RenderTexture exclusionMask, RenderTexture depthCenterAverage, RenderTexture presortTexture, 
		                         DepthOfFieldSamples qualityLevel, out RenderTexture tapsTexture, out RenderTexture alphaTexture)
		{
			m_DoFMat.SetTexture("_TiledData", tiledData);
			m_DoFMat.SetTexture("_TiledNeighbourhoodData", neighbourhoodData);
			m_DoFMat.SetTexture("_HalfResSourceDepthTexture", prefilteredSource); //Actually the prefiltered half res
			m_DoFMat.SetTexture("_PresortTexture", presortTexture); 

			if (exclusionMask != null) 		m_DoFMat.SetTexture("_ExclusionMask", exclusionMask); 
			if (depthCenterAverage != null) m_DoFMat.SetTexture("_AvgCenterDepth", depthCenterAverage); 
			prefilteredSource.filterMode = FilterMode.Point;

			tapsTexture = RenderTexture.GetTemporary(prefilteredSource.width, prefilteredSource.height, 0, prefilteredSource.format);
			tapsTexture.filterMode = FilterMode.Point;
			tapsTexture.wrapMode = TextureWrapMode.Clamp;
			
			alphaTexture = RenderTexture.GetTemporary(prefilteredSource.width, prefilteredSource.height, 0, RenderTextureFormat.R8);
			alphaTexture.filterMode = FilterMode.Point;
			alphaTexture.wrapMode = TextureWrapMode.Clamp;

			renderBuffers[0] = tapsTexture.colorBuffer;
			renderBuffers[1] = alphaTexture.colorBuffer;
			Graphics.SetRenderTarget(renderBuffers, tapsTexture.depthBuffer);

			if (qualityLevel == DepthOfFieldSamples.Normal_25) ScionGraphics.Blit(m_DoFMat, 12);
			if (qualityLevel == DepthOfFieldSamples.High_49) ScionGraphics.Blit(m_DoFMat, 5);
		}

		private RenderTexture MedianFilterPass(RenderTexture inputTexture)
		{
			RenderTexture medianFiltered = RenderTexture.GetTemporary(inputTexture.width, inputTexture.height, 0, inputTexture.format);
			medianFiltered.filterMode = FilterMode.Point;
			medianFiltered.wrapMode = TextureWrapMode.Clamp;
			
			inputTexture.filterMode = FilterMode.Point;
			Graphics.Blit(inputTexture, medianFiltered, m_DoFMat, 3);
			RenderTexture.ReleaseTemporary(inputTexture);
			return medianFiltered;
		}
		
		private void TemporalPass(ref RenderTexture tapsTexture, RenderTexture previousTapsTexture)
		{
			RenderTexture temporalTaps = RenderTexture.GetTemporary(tapsTexture.width, tapsTexture.height, 0, tapsTexture.format);
			temporalTaps.filterMode = FilterMode.Point;
			temporalTaps.wrapMode = TextureWrapMode.Clamp;

			previousTapsTexture.filterMode = FilterMode.Bilinear;
			
			m_DoFMatTemporal.SetTexture("_TapsCurrentTexture", tapsTexture);
			m_DoFMatTemporal.SetTexture("_TapsHistoryTexture", previousTapsTexture);
			
			ScionGraphics.Blit(temporalTaps, m_DoFMatTemporal, 0);
			RenderTexture.ReleaseTemporary(tapsTexture);
			tapsTexture = temporalTaps;
		}

		private void TemporalPass(ref RenderTexture tapsTexture, ref RenderTexture alphaTexture, RenderTexture previousTapsTexture, RenderTexture previousAlphaTexture)
		{
			RenderTexture temporalTaps = RenderTexture.GetTemporary(tapsTexture.width, tapsTexture.height, 0, tapsTexture.format);
			temporalTaps.filterMode = FilterMode.Point;
			temporalTaps.wrapMode = TextureWrapMode.Clamp;

			RenderTexture temporalAlpha = RenderTexture.GetTemporary(alphaTexture.width, alphaTexture.height, 0, alphaTexture.format);
			temporalAlpha.filterMode = FilterMode.Point;
			temporalAlpha.wrapMode = TextureWrapMode.Clamp;
			
			previousTapsTexture.filterMode = FilterMode.Bilinear;
			previousAlphaTexture.filterMode = FilterMode.Bilinear;
			
			m_DoFMatTemporal.SetTexture("_TapsCurrentTexture", tapsTexture);
			m_DoFMatTemporal.SetTexture("_AlphaCurrentTexture", alphaTexture);
			m_DoFMatTemporal.SetTexture("_TapsHistoryTexture", previousTapsTexture);
			m_DoFMatTemporal.SetTexture("_AlphaHistoryTexture", previousAlphaTexture);

			renderBuffers[0] = temporalTaps.colorBuffer;
			renderBuffers[1] = temporalAlpha.colorBuffer;

			Graphics.SetRenderTarget(renderBuffers, temporalTaps.depthBuffer);
			
			ScionGraphics.Blit(m_DoFMatTemporal, 1);
			RenderTexture.ReleaseTemporary(tapsTexture);
			RenderTexture.ReleaseTemporary(alphaTexture);
			tapsTexture = temporalTaps;
			alphaTexture = temporalAlpha;
		}

		private RenderTexture UpsampleDepthOfField(RenderTexture source, RenderTexture tapsTexture, RenderTexture alphaTexture, RenderTexture neighbourhoodData, RenderTexture exclusionMask)
		{
			m_DoFMat.SetTexture("_TapsTexture", tapsTexture);
			m_DoFMat.SetTexture("_AlphaTexture", alphaTexture);
			m_DoFMat.SetTexture("_FullResolutionSource", source);
			m_DoFMat.SetTexture("_TiledNeighbourhoodData", neighbourhoodData);
			if (exclusionMask != null) m_DoFMat.SetTexture("_ExclusionMask", exclusionMask); 

			neighbourhoodData.filterMode = FilterMode.Bilinear;
			tapsTexture.filterMode = FilterMode.Point;
			alphaTexture.filterMode = FilterMode.Point;

			RenderTexture compositedDoF = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
			source.filterMode = FilterMode.Point;
			source.wrapMode = TextureWrapMode.Clamp;

			ScionGraphics.Blit(compositedDoF, m_DoFMat, 6);
			return compositedDoF;
		}

		private RenderTexture BilateralAlphaFilter(RenderTexture alphaTexture, RenderTexture tapsTexture)
		{
			RenderTexture bilateralFiltered = RenderTexture.GetTemporary(alphaTexture.width, alphaTexture.height, 0, alphaTexture.format);
			bilateralFiltered.filterMode = FilterMode.Point;
			bilateralFiltered.wrapMode = TextureWrapMode.Clamp;
			
			alphaTexture.filterMode = FilterMode.Point;
			m_DoFMat.SetTexture("_AlphaTexture", alphaTexture);
			tapsTexture.filterMode = FilterMode.Point;
			m_DoFMat.SetTexture("_TapsTexture", tapsTexture);

			Graphics.Blit(alphaTexture, bilateralFiltered, m_DoFMat, 14);
			RenderTexture.ReleaseTemporary(alphaTexture);
			return bilateralFiltered;
		}
	}
}











