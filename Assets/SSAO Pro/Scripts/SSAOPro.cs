// SSAO Pro - Unity Asset
// Copyright (c) 2015 - Thomas Hourdel
// http://www.thomashourdel.com

using UnityEngine;
using SSAOProUtils;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_5_4_OR_NEWER
[ImageEffectAllowedInSceneView]
#endif
[HelpURL("http://www.thomashourdel.com/ssaopro/doc/")]
[ExecuteInEditMode, AddComponentMenu("Image Effects/SSAO Pro")]
[RequireComponent(typeof(Camera))]
public class SSAOPro : MonoBehaviour
{
	public enum BlurMode
	{
		None,
		Gaussian,
		HighQualityBilateral
	}

	public enum SampleCount
	{
		VeryLow,
		Low,
		Medium,
		High,
		Ultra
	}

	public Texture2D NoiseTexture;

	public bool UseHighPrecisionDepthMap = false;

	public SampleCount Samples = SampleCount.Medium;

	[Range(1, 4)]
	public int Downsampling = 1;

	[Range(0.01f, 1.25f)]
	public float Radius = 0.12f;

	[Range(0f, 16f)]
	public float Intensity = 2.5f;

	[Range(0f, 10f)]
	public float Distance = 1f;

	[Range(0f, 1f)]
	public float Bias = 0.1f;

	[Range(0f, 1f)]
	public float LumContribution = 0.5f;

	[ColorUsage(false)]
	public Color OcclusionColor = Color.black;

	public float CutoffDistance = 150f;
	public float CutoffFalloff = 50f;

	public BlurMode Blur = BlurMode.HighQualityBilateral;
	public bool BlurDownsampling = false;

	[Range(1, 4)]
	public int BlurPasses = 1;

	[Range(1f, 20f)]
	public float BlurBilateralThreshold = 10f;

	public bool DebugAO = false;

	protected Shader m_ShaderSSAO;
	protected Material m_Material;
	protected Camera m_Camera;

	protected enum Pass
	{
		Clear = 0,
		GaussianBlur = 5,
		HighQualityBilateralBlur = 6,
		Composite = 7
	}

	public Material Material
	{
		get
		{
			if (m_Material == null)
				m_Material = new Material(ShaderSSAO) { hideFlags = HideFlags.HideAndDontSave };

			return m_Material;
		}
	}

	public Shader ShaderSSAO
	{
		get
		{
			if (m_ShaderSSAO == null)
				m_ShaderSSAO = Shader.Find("Hidden/SSAO Pro V2");

			return m_ShaderSSAO;
		}
	}

	void OnEnable()
	{
		m_Camera = GetComponent<Camera>();

	#if UNITY_EDITOR
        // Don't check for shader compatibility while it's building as it would disable most effects
        // on build farms without good-enough gaming hardware.
        if (!BuildPipeline.isBuildingPlayer)
        {
	#endif
			// Disable if we don't support image effects
			if (!SystemInfo.supportsImageEffects)
			{
				Debug.LogWarning("Image Effects are not supported on this device.");
				enabled = false;
				return;
			}

			// Disable if we don't support render textures
			if (!SystemInfo.supportsRenderTextures)
			{
				Debug.LogWarning("RenderTextures are not supported on this platform.");
				enabled = false;
				return;
			}

			// Disable the image effect if the shader is missing
			if (ShaderSSAO == null)
			{
				Debug.LogWarning("Missing shader (SSAO).");
				enabled = false;
				return;
			}

			// Disable the image effect if the shader can't run on the users graphics card
			if (!ShaderSSAO.isSupported)
			{
				Debug.LogWarning("Unsupported shader (SSAO).");
				enabled = false;
				return;
			}

			if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
			{
				Debug.LogWarning("Depth textures aren't supported on this device.");
				enabled = false;
				return;
			}
	#if UNITY_EDITOR
		}
	#endif

		// Depth & normal maps
		m_Camera.depthTextureMode |= DepthTextureMode.Depth;
		m_Camera.depthTextureMode |= DepthTextureMode.DepthNormals;
	}

	void OnDisable()
	{
		if (m_Material != null)
			DestroyImmediate(m_Material);

		m_Material = null;
	}

	[ImageEffectOpaque]
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		// Fail checks
		if (ShaderSSAO == null || Mathf.Approximately(Intensity, 0f))
		{
			Graphics.Blit(source, destination);
			return;
		}

		// Shader keywords & pass ID
		Material.shaderKeywords = null;

		switch (Samples)
		{
			case SampleCount.Low:
				Material.EnableKeyword("SAMPLES_LOW");
				break;
			case SampleCount.Medium:
				Material.EnableKeyword("SAMPLES_MEDIUM");
				break;
			case SampleCount.High:
				Material.EnableKeyword("SAMPLES_HIGH");
				break;
			case SampleCount.Ultra:
				Material.EnableKeyword("SAMPLES_ULTRA");
				break;
			default:
				break;
		}

		// SSAO pass ID
		int ssaoPass = 0;

		if (NoiseTexture != null)
			ssaoPass = 1;

		if (!Mathf.Approximately(LumContribution, 0f))
			ssaoPass += 2;

		ssaoPass++;

		// Uniforms
		Material.SetMatrix("_InverseViewProject", (m_Camera.projectionMatrix * m_Camera.worldToCameraMatrix).inverse);
		Material.SetMatrix("_CameraModelView", m_Camera.cameraToWorldMatrix);
		Material.SetTexture("_NoiseTex", NoiseTexture);
		Material.SetVector("_Params1", new Vector4(NoiseTexture == null ? 0f : NoiseTexture.width, Radius, Intensity, Distance));
		Material.SetVector("_Params2", new Vector4(Bias, LumContribution, CutoffDistance, CutoffFalloff));
		Material.SetColor("_OcclusionColor", OcclusionColor);

		// Render !
		if (Blur == BlurMode.None)
		{
			RenderTexture rt = RenderTexture.GetTemporary(source.width / Downsampling, source.height / Downsampling, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(rt, rt, Material, (int)Pass.Clear);

			if (DebugAO)
			{
				Graphics.Blit(source, rt, Material, ssaoPass);
				Graphics.Blit(rt, destination);
				RenderTexture.ReleaseTemporary(rt);
				return;
			}

			Graphics.Blit(source, rt, Material, ssaoPass);
			Material.SetTexture("_SSAOTex", rt);
			Graphics.Blit(source, destination, Material, (int)Pass.Composite);
			RenderTexture.ReleaseTemporary(rt);
		}
		else
		{
			// Pass ID
			Pass blurPass = (Blur == BlurMode.HighQualityBilateral)
				? Pass.HighQualityBilateralBlur
				: Pass.GaussianBlur;

			// Prep work
			int d = BlurDownsampling ? Downsampling : 1;
			RenderTexture rt1 = RenderTexture.GetTemporary(source.width / d, source.height / d, 0, RenderTextureFormat.ARGB32);
			RenderTexture rt2 = RenderTexture.GetTemporary(source.width / Downsampling, source.height / Downsampling, 0, RenderTextureFormat.ARGB32);
			Graphics.Blit(rt1, rt1, Material, (int)Pass.Clear);

			// SSAO
			Graphics.Blit(source, rt1, Material, ssaoPass);

			Material.SetFloat("_BilateralThreshold", BlurBilateralThreshold * 5f);

			for (int i = 0; i < BlurPasses; i++)
			{
				// Horizontal blur
				Material.SetVector("_Direction", new Vector2(1f / source.width, 0f));
				Graphics.Blit(rt1, rt2, Material, (int)blurPass);
				rt1.DiscardContents();

				// Vertical blur
				Material.SetVector("_Direction", new Vector2(0f, 1f / source.height));
				Graphics.Blit(rt2, rt1, Material, (int)blurPass);
				rt2.DiscardContents();
			}

			if (!DebugAO)
			{
				Material.SetTexture("_SSAOTex", rt1);
				Graphics.Blit(source, destination, Material, (int)Pass.Composite);
			}
			else
			{
				Graphics.Blit(rt1, destination);
			}

			RenderTexture.ReleaseTemporary(rt1);
			RenderTexture.ReleaseTemporary(rt2);
		}
	}
}
