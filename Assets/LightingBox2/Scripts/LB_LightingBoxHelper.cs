﻿// Use this script to get runtime access to the lighting box econtrolled effects
/// <summary>
/// example :
/// 
/// // Update bloom effect .
/// void Start ()
/// {
///   	GameObject.FindObjectOfType<LB_LightingBoxHelper> ().Update_Bloom (true, 1f, 0.5f, Color.white);
/// }
/// </summary>
using UnityEngine;   
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using LightingBox.Effects;
using cCharkes;

#if UNITY_EDITOR
using UnityEditor;
#endif

#region Emum Types
public enum FoliageMode
{
	Off,On
}
public enum MatType
{
	Translucency,
	TranslucencySnow,
	SpeedTree
}
public enum MaterialConverterType
{
	SpeedTree
}
public enum CameraMode
{
	Single,All,Custom
}
public enum WindowMode
{
	Part1,Part2,Part3,
	Finish
}
public enum AmbientLight
{
	Skybox,
	Color,
	Gradient
}
public enum LightingMode
{
	FullyRealtime,
	RealtimeGI,
	Baked,
	RealtimeGIandBakedGI
}
public enum LightSettings
{
	Default,
	Realtime,
	Mixed,
	Baked
}
public enum MyColorSpace
{
	Linear,
	Gamma
}
public enum VolumetricLightType
{
	OnlyDirectional,
	AllLightSources
}
public enum VLightLevel
{
	Level1,	Level2,Level3,Level4
}
public enum CustomFog
{
	Global,
	Height,
	Distance
}
public enum LightsShadow
{
	OnlyDirectionalSoft,OnlyDirectionalHard,
	AllLightsSoft,AllLightsHard,
	Off
}
public enum LightProbeMode
{
	Blend,
	Proxy
}
public enum Render_Path
{
	Default,Forward,Deferred
}

public enum DOFQuality
{
	Low,Medium,High
}

public enum AOType
{
	Classic,Modern
}

public enum ColorMode
{
	ACES,Neutral,LUT
}

public enum AAMode
{
	TAA,FXAA,SMAA
}

#endregion

public class LB_LightingBoxHelper : MonoBehaviour {

	public LB_LightingProfile mainLightingProfile;

	#region Runtime Update

	Light sunLight;
	Camera mainCamera;
	LB_LightingBoxHelper helper;

	void Start()
	{
		if (!mainCamera) {
			if (GameObject.Find (mainLightingProfile.mainCameraName))
				mainCamera = GameObject.Find (mainLightingProfile.mainCameraName).GetComponent<Camera> ();
			else
				mainCamera = GameObject.FindObjectOfType<Camera> ();
		}

		Update_SunRuntime (mainLightingProfile);
		UpdatePostEffects (mainLightingProfile);
		UpdateSettings (mainLightingProfile);
	}

	void UpdatePostEffects(LB_LightingProfile profile)
	{
		if(!helper)
			helper = GameObject.Find("LightingBox_Helper").GetComponent<LB_LightingBoxHelper> ();

		if (!profile)
			return;

		helper.UpdateProfiles (mainCamera, profile.postProcessingProfile);

		// MotionBlur
		if (profile.MotionBlur_Enabled)
			helper.Update_MotionBlur (true);
		else
			helper.Update_MotionBlur (false);

		// Vignette
		helper.Update_Vignette (profile.Vignette_Enabled,profile.vignetteIntensity);


		// _ChromaticAberration
		helper.Update_ChromaticAberration(profile.Chromattic_Enabled,profile.CA_Intensity,profile.mobileOptimizedChromattic);

		// Foliage
		helper.Update_Foliage (profile.translucency,profile. ambient, profile.shadows, profile.windSpeed, profile.windScale, profile.tranColor);

		// Snow
		helper.Update_Snow (profile.snowAlbedo,profile.snowNormal,profile.snowIntensity);

		helper.Update_Bloom(profile.Bloom_Enabled,profile.bIntensity,profile.bThreshould,profile.bColor,profile.dirtTexture,profile.dirtIntensity,profile.mobileOptimizedBloom,profile.bRotation);


		// Depth of Field
		helper.Update_DOF(mainCamera,profile.DOF_Enabled,profile.dofQuality,profile.dofBlur,profile.dofRange,profile.falloff,profile.visualize);
		helper.Update_AutoFocus (mainCamera, profile.AutoFocus_Enabled, profile.DOF_Enabled,profile.afLayer, profile.afRange, profile.afBlur, profile.afSpeed, profile.afUpdate, profile.afRayLength);

		// AO
		if (profile.AO_Enabled)
			helper.Update_AO(mainCamera,true,profile.aoType,profile.aoRadius,profile.aoIntensity,profile.ambientOnly,profile.aoColor, profile.aoQuality);
		else
			helper.Update_AO(mainCamera,false,profile.aoType,profile.aoRadius,profile.aoIntensity,profile.ambientOnly,profile.aoColor, profile.aoQuality);


		// Color Grading
		helper.Update_ColorGrading(profile.colorMode,profile.exposureIntensity,profile.contrastValue,profile.temp,profile.eyeKeyValue,profile.saturation,profile.colorGamma,profile.colorLift,profile.gamma,profile.lut);

		////-----------------------------------------------------------------------------
		/// 
		// Screen Space Reflections
		helper.Update_SSR(mainCamera, profile.SSR_Enabled,profile.ssrQuality,profile.ssrAtten,profile.ssrFade);

		helper.Update_StochasticSSR(mainCamera, profile.ST_SSR_Enabled,profile.resolutionMode,profile.debugPass,profile.rayDistance,profile.screenFadeSize,profile.smoothnessRange);

	}

	void UpdateSettings(LB_LightingProfile profile)
	{
		// Sun Light Update
		if (sunLight) {
			sunLight.color = profile.sunColor;
			sunLight.intensity = profile.sunIntensity;
			sunLight.bounceIntensity = profile.indirectIntensity;
		} else {
			Update_SunRuntime (profile);
		}

		if (profile.sunFlare)
		{
			if(sunLight)
				sunLight.flare = profile.sunFlare;
		}
		else
		{
			if(sunLight)
				sunLight.flare = null;
		}

		// Skybox
		helper.Update_SkyBox (profile.Ambient_Enabled, profile.skyBox);

		// Update Ambient
		helper.Update_Ambient (profile.Ambient_Enabled,profile.ambientLight, profile.ambientColor,profile.skyColor,profile.equatorColor,profile.groundColor);

		// Volumetric Light
		helper.Update_VolumetricLight(mainCamera,profile.VL_Enabled,profile.vLight,profile.vLightLevel);

		// Sun Shaft
		helper.Update_SunShaft(mainCamera,profile.SunShaft_Enabled, profile.shaftQuality,profile.shaftDistance,profile.shaftBlur,profile.shaftColor,sunLight.transform);

		// Global Fog
		helper.Update_GlobalFog(mainCamera,profile.Fog_Enabled,profile.fogMode,profile.fogDistance,profile.fogHeight,profile.fogHeightIntensity,profile.fogColor,profile.fogIntensity);

	}

	void Update_SunRuntime(LB_LightingProfile profile)
	{
		if (profile.Sun_Enabled) {
			if (!RenderSettings.sun) {
				Light[] lights = GameObject.FindObjectsOfType<Light> ();
				foreach (Light l in lights) {
					if (l.type == LightType.Directional) {
						sunLight = l;

						if (profile.sunColor != Color.clear)
							profile.sunColor = l.color;
						else
							profile.sunColor = Color.white;

						//sunLight.shadowNormalBias = 0.05f;  
						sunLight.color = profile.sunColor;
						if (sunLight.bounceIntensity == 1f)
							sunLight.bounceIntensity = profile.indirectIntensity;
					}
				}
			} else {		
				sunLight = RenderSettings.sun;

				if (profile.sunColor != Color.clear)
					profile.sunColor = sunLight.color;
				else
					profile.sunColor = Color.white;

				//	sunLight.shadowNormalBias = 0.05f;  
				sunLight.color = profile.sunColor;
				if (sunLight.bounceIntensity == 1f)
					sunLight.bounceIntensity = profile.indirectIntensity;
			}
		}


	}

	#endregion

	public void Update_MainProfile(LB_LightingProfile profile)
	{
		if(profile)
			mainLightingProfile = profile;
	}

	public void UpdateProfiles(Camera mainCamera,PostProcessProfile profile)
	{
		if (!profile)
			return;
		
		if (profile)
		{
			if (!mainCamera.GetComponent<PostProcessLayer> ())
			{
				mainCamera.gameObject.AddComponent<PostProcessLayer> ();
				mainCamera.gameObject.GetComponent<PostProcessLayer> ().antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
				mainCamera.gameObject.GetComponent<PostProcessLayer> ().volumeLayer = LayerMask.NameToLayer("Everything");
				mainCamera.gameObject.GetComponent<PostProcessLayer> ().fog.enabled = true;
				mainCamera.gameObject.GetComponent<PostProcessLayer> ().Init(null);
			}
			
		}

		if (!GameObject.Find ("Global Volume")) {
			GameObject gVolume = new GameObject ();
			gVolume.name = "Global Volume";
			gVolume.AddComponent<PostProcessVolume> ();
			gVolume.GetComponent<PostProcessVolume> ().isGlobal = true;
			gVolume.GetComponent<PostProcessVolume> ().priority = 1f;
			if (profile)
				gVolume.GetComponent<PostProcessVolume> ().sharedProfile = profile;
		} else {
			if (profile)
				GameObject.Find ("Global Volume").GetComponent<PostProcessVolume> ().sharedProfile = profile;
		}

	}

	public void Update_MotionBlur(bool enabled)
	{
		MotionBlur mb;
		GameObject.Find("Global Volume").GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings(out mb);
		mb.enabled.value = enabled; 
	}

	public void Update_Vignette(bool enabled, float intensity)
	{
		Vignette vi;
		GameObject.Find("Global Volume").GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings(out vi);

		vi.enabled.value = enabled; 

		vi.intensity.overrideState = true;
		vi.intensity.value = intensity;

		vi.smoothness.overrideState = true;
		vi.smoothness.value = 1f;

		vi.roundness.overrideState = true;
		vi.roundness.value = 1f;

	}

	public void Update_ChromaticAberration(bool enabled,float intensity,bool mobileOptimized)
	{
		ChromaticAberration ca;
		GameObject.Find("Global Volume").GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings(out ca);

		ca.intensity.overrideState = true;
		ca.intensity.value = intensity;

		ca.fastMode.overrideState = true;
		ca.fastMode.value = mobileOptimized;

		ca.enabled.value = enabled; 
	}

	public void Update_Bloom(bool enabled,float intensity,float threshold,Color color,Texture2D dirtTexture,float dirtIntensity,bool mobileOptimized,float bRotation)
	{
		if(enabled)
		{
			Bloom b;
			GameObject.Find("Global Volume").GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings(out b);
			b.intensity.overrideState = true;
			b.intensity.value = intensity;
			b.threshold.overrideState = true;
			b.threshold.value = threshold;
			b.color.overrideState = true;
			b.color.value = color;

			b.anamorphicRatio.overrideState = true;
			b.anamorphicRatio.value = bRotation;

			b.fastMode.overrideState = true;
			b.fastMode.value = mobileOptimized;

			b.dirtTexture.overrideState = true;
			b.dirtTexture.value = dirtTexture;

			b.dirtIntensity.overrideState = true;
			b.dirtIntensity.value = dirtIntensity;

			b.enabled.value = true;
		}
		else
		{
			Bloom b;
			GameObject.Find("Global Volume").GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings(out b);
			b.intensity.overrideState = true;
			b.intensity.value = intensity;
			b.threshold.overrideState = true;
			b.threshold.value = threshold;
			b.color.overrideState = true;
			b.color.value = color;

			b.dirtTexture.overrideState = true;
			b.dirtTexture.value = dirtTexture;

			b.dirtIntensity.overrideState = true;
			b.dirtIntensity.value = dirtIntensity;

			b.anamorphicRatio.overrideState = true;
			b.anamorphicRatio.value = bRotation;

			b.enabled.value = false;
		}
	}

	public void Update_AutoFocus(Camera mainCamera , bool enabled, bool dofEnabled,LayerMask afLayer,float afRange,float afBlur,float afSpeed, float afUpdate, float afRayLength)
	{
		
		if (enabled && dofEnabled) 
		{
			
			if (enabled)
			{
				if (!mainCamera.GetComponent<LB_AutoFocus> ()) {
					mainCamera.gameObject.AddComponent<LB_AutoFocus> ();
					LB_AutoFocus dofAF = mainCamera.GetComponent<LB_AutoFocus> ();

					dofAF.layerMask = afLayer;
					dofAF.maxRange = afRange;
					dofAF.maxBlur = afBlur;
					dofAF.speed = afSpeed;
					dofAF.updateInterval = afUpdate;
					dofAF.rayLength = afRayLength;

				}
				else 
				{
					LB_AutoFocus dofAF = mainCamera.GetComponent<LB_AutoFocus> ();

					dofAF.layerMask = afLayer;
					dofAF.maxRange = afRange;
					dofAF.maxBlur = afBlur;
					dofAF.speed = afSpeed;
					dofAF.updateInterval = afUpdate;
					dofAF.rayLength = afRayLength;
				}
			}
			if (!enabled || !dofEnabled) {
				if (mainCamera.GetComponent<LB_AutoFocus> ()) {
					if(Application.isPlaying)
						Destroy(mainCamera.GetComponent<LB_AutoFocus> ());						
					else
						DestroyImmediate (mainCamera.GetComponent<LB_AutoFocus> ());
				}

			}
			
		} else {
			
			if (mainCamera.GetComponent<LB_AutoFocus> ()) {
				if(Application.isPlaying)
					Destroy (mainCamera.GetComponent<LB_AutoFocus> ());					
				else
					DestroyImmediate (mainCamera.GetComponent<LB_AutoFocus> ());
			}
		}


	}

	public void Update_DOF(Camera mainCamera ,bool enabled,DOFQuality quality,float blur,float range,float falloff, bool visualize)
	{
		if (enabled) {
			if (!mainCamera.GetComponent<LightingBox.Effects.DepthOfField> ()) {
				mainCamera.gameObject.AddComponent<LightingBox.Effects.DepthOfField> ();
				LightingBox.Effects.DepthOfField dof = mainCamera.GetComponent<LightingBox.Effects.DepthOfField> ();
				dof.settings.tweakMode = LightingBox.Effects.DepthOfField.TweakMode.Range;

				if(quality == DOFQuality.Low)
					dof.settings.filteringQuality = LightingBox.Effects.DepthOfField.QualityPreset.Low;
				if(quality == DOFQuality.Medium)
					dof.settings.filteringQuality = LightingBox.Effects.DepthOfField.QualityPreset.Medium;
				if(quality == DOFQuality.High)
					dof.settings.filteringQuality = LightingBox.Effects.DepthOfField.QualityPreset.High;
				
				dof.settings.apertureShape = LightingBox.Effects.DepthOfField.ApertureShape.Octogonal;
				dof.focus.focusPlane = 0f;
				dof.focus.farFalloff = falloff;
				dof.focus.range = range;
				dof.focus.farBlurRadius = blur;
				dof.settings.visualizeFocus = visualize;
			} else {
				LightingBox.Effects.DepthOfField dof = mainCamera.GetComponent<LightingBox.Effects.DepthOfField> ();
				dof.settings.tweakMode = LightingBox.Effects.DepthOfField.TweakMode.Range;

				if(quality == DOFQuality.Low)
					dof.settings.filteringQuality = LightingBox.Effects.DepthOfField.QualityPreset.Low;
				if(quality == DOFQuality.Medium)
					dof.settings.filteringQuality = LightingBox.Effects.DepthOfField.QualityPreset.Medium;
				if(quality == DOFQuality.High)
					dof.settings.filteringQuality = LightingBox.Effects.DepthOfField.QualityPreset.High;

				dof.settings.apertureShape = LightingBox.Effects.DepthOfField.ApertureShape.Octogonal;
				dof.focus.focusPlane = 0f;
				dof.focus.farFalloff = falloff;
				dof.focus.range = range;
				dof.focus.farBlurRadius = blur;
				dof.settings.visualizeFocus = visualize;
			}
		}
		if (!enabled) {
			if (mainCamera.GetComponent<LightingBox.Effects.DepthOfField> ()) {
				if(Application.isPlaying)
					Destroy (mainCamera.GetComponent<LightingBox.Effects.DepthOfField> ());					
				else
					DestroyImmediate (mainCamera.GetComponent<LightingBox.Effects.DepthOfField> ());
			}

		}
	}

	public void Update_AA(Camera mainCamera ,AAMode aaMode, bool  enabled)
	{
		if (enabled) {
			if (aaMode == AAMode.TAA) {
				mainCamera.GetComponent<PostProcessLayer> ().antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
				mainCamera.GetComponent<PostProcessLayer> ().Init (null);
			}
			if (aaMode == AAMode.FXAA) {
				mainCamera.GetComponent<PostProcessLayer> ().antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
				mainCamera.GetComponent<PostProcessLayer> ().Init (null);
			}
			if (aaMode == AAMode.SMAA) {
				mainCamera.GetComponent<PostProcessLayer> ().antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
				mainCamera.GetComponent<PostProcessLayer> ().Init (null);
			}
		} else {
			mainCamera.GetComponent<PostProcessLayer> ().antialiasingMode = PostProcessLayer.Antialiasing.None;
			mainCamera.GetComponent<PostProcessLayer> ().Init (null);
		}
	}

	public void Update_Foliage(float translucency, float ambient,float shadows, float windSpeed
		,float windScale, Color transColor)
	{
		Shader.SetGlobalFloat ("_WindScale", windScale);
		Shader.SetGlobalFloat ("_WindSpeed", windSpeed);

		Shader.SetGlobalColor ("_TranslucencyColor", transColor);
		Shader.SetGlobalFloat ("_TranslucencyIntensity", translucency);
		Shader.SetGlobalFloat ("_TransAmbient", ambient);
		Shader.SetGlobalFloat ("_TransShadow", shadows);
	}

	public void Update_ConvertMaterials(MatType matType,string customShader)
	{
		System.Collections.Generic.List<Material> mats = new System.Collections.Generic.List<Material>();

		MeshRenderer[] mr = GameObject.FindObjectsOfType<MeshRenderer> ();

		foreach (MeshRenderer m in mr) {

			Material[] mat = m.sharedMaterials;

			for (int a = 0; a < mat.Length; a++)
				mats.Add (mat [a]);
			
		}

		for (int a = 0; a < mats.Count; a++) 
		{
			// Convert speed tree shaders  to translucency  
			if (mats [a].shader == Shader.Find("Nature/SpeedTree")) 
			{
				if (matType == MatType.Translucency)					
					mats [a].shader = Shader.Find ("LightingBox/Nature/Leave Standard (Wind Support)");
				if (matType == MatType.TranslucencySnow)					
					mats [a].shader = Shader.Find ("LightingBox/Nature/Snow-Leave Standard");
			}

			// Convert custom shaders to translucency
			if (mats [a].shader == Shader.Find(customShader)) 
			{
				if (matType == MatType.Translucency)					
					mats [a].shader = Shader.Find ("LightingBox/Nature/Leave Standard (Wind Support)");
				if (matType == MatType.TranslucencySnow)					
					mats [a].shader = Shader.Find ("LightingBox/Nature/Snow-Leave Standard");
			}

			if (mats [a].shader == Shader.Find ("LightingBox/Nature/Leave Standard (Wind Support)") && matType == MatType.TranslucencySnow)				
				mats [a].shader = Shader.Find ("LightingBox/Nature/Snow-Leave Standard");

			if (mats [a].shader == Shader.Find ("LightingBox/Nature/Snow-Leave Standard") && matType == MatType.Translucency)				
				mats [a].shader = Shader.Find ("LightingBox/Nature/Leave Standard (Wind Support)");

			if (matType == MatType.SpeedTree) {
				if (mats [a].shader == Shader.Find ("LightingBox/Nature/Snow-Leave Standard")
				   || mats [a].shader == Shader.Find ("LightingBox/Nature/Leave Standard (Wind Support)")
					|| mats [a].shader == Shader.Find (customShader))
					mats [a].shader = Shader.Find ("Nature/SpeedTree");		
			}
		}

	}

	public void Update_Snow(Texture2D albedo, Texture2D normal, float intensity)
	{
		Shader.SetGlobalTexture ("_SnowAlbedo", albedo);
		Shader.SetGlobalTexture ("_SnowNormal", normal);
		Shader.SetGlobalFloat ("_SnowIntensity", intensity);
	}

	public void Update_ConvertSnowMaterials(string customShader)
	{
		System.Collections.Generic.List<Material> mats = new System.Collections.Generic.List<Material>();

		MeshRenderer[] mr = GameObject.FindObjectsOfType<MeshRenderer> ();

		foreach (MeshRenderer m in mr) {

			Material[] mat = m.sharedMaterials;

			for (int a = 0; a < mat.Length; a++)
				mats.Add (mat [a]);

		}

		for (int a = 0; a < mats.Count; a++) 
		{
			
			// Convert speed tree shaders  to translucency  
			if (mats [a].shader == Shader.Find("Standard") || mats [a].shader == Shader.Find("Standard (Specular setup)")
				|| mats [a].shader == Shader.Find(customShader)) 
			{			
				mats [a].shader = Shader.Find ("LightingBox/Snow Standard (Specular)");
			}
		}

	}

	public void Update_AO(Camera mainCamera ,bool enabled,AOType aoType,float aoRadius,float aoIntensity,bool ambientOnly,Color aoColor, AmbientOcclusionQuality aoQuality)
	{

		AmbientOcclusion ao;
		GameObject.Find("Global Volume").GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings(out ao);

		if (enabled) {
			if (aoType == AOType.Classic) {
				ao.enabled.overrideState = true;
				ao.enabled.value = true;
				ao.mode.overrideState = true;
				ao.mode.value = AmbientOcclusionMode.ScalableAmbientObscurance;
				ao.radius.overrideState = true;
				ao.radius.value = aoRadius;
				ao.ambientOnly.overrideState = true;
				ao.ambientOnly.value = ambientOnly;
				ao.color.overrideState = true;
				ao.color.value = aoColor;
				ao.intensity.overrideState = true;
				ao.intensity.value = aoIntensity;
				ao.quality.overrideState = true;
				ao.quality.value = aoQuality;
			}
			if (aoType == AOType.Modern) {		
				ao.enabled.overrideState = true;
				ao.enabled.value = true;
				ao.mode.overrideState = true;
				ao.mode.value = AmbientOcclusionMode.MultiScaleVolumetricObscurance;
				ao.radius.overrideState = true;
				ao.radius.value = aoRadius;
				ao.ambientOnly.overrideState = true;
				ao.ambientOnly.value = ambientOnly;
				ao.color.overrideState = true;
				ao.color.value = aoColor;
				ao.intensity.overrideState = true;
				ao.intensity.value = aoIntensity;
			}
		} else {
			ao.enabled.overrideState = true;
			ao.enabled.value = false;
		}
	}

	public void Update_ColorGrading(ColorMode colorMode,float exposureIntensity,float contrastValue,float temp,float eyeKeyValue
		,float saturation,Color colorGamma,Color colorLift,float gamma,Texture lut)
	{
		ColorGrading cg;
		GameObject.Find("Global Volume").GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings(out cg);

		AutoExposure ae;
		GameObject.Find("Global Volume").GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings(out ae);

			if (colorMode == ColorMode.ACES) {
				cg.tonemapper.overrideState = true;
				cg.tonemapper.value = Tonemapper.ACES;
				cg.gradingMode.value = GradingMode.HighDefinitionRange;

			}
			if (colorMode == ColorMode.Neutral) {
				cg.tonemapper.overrideState = true;
				cg.tonemapper.value = Tonemapper.Neutral;
				cg.gradingMode.value = GradingMode.HighDefinitionRange;

			}

		if (colorMode == ColorMode.LUT) {
			cg.tonemapper.overrideState = true;
			cg.tonemapper.value = Tonemapper.Custom;
			cg.gradingMode.value = GradingMode.External;
			if (lut != null) {
				cg.externalLut.overrideState = true;
				cg.externalLut.value = lut;
			}
		} else {

			cg.lift.overrideState = true;
			cg.lift.value.Set (colorLift.r, colorLift.g, colorLift.b, 0);

			cg.gamma.overrideState = true;
			cg.gamma.value.Set (colorGamma.r, colorGamma.g, colorGamma.b, gamma);

			cg.gain.overrideState = true;
			cg.gain.value.Set (cg.gain.value.x, cg.gain.value.y, cg.gain.value.z, 0);

			cg.saturation.overrideState = true;
			cg.saturation.value = saturation * 100;

			cg.saturation.overrideState = true;
			cg.saturation.value = saturation * 100;
			cg.postExposure.overrideState = true;
			cg.postExposure.value = exposureIntensity;
			cg.contrast.overrideState = true;
			cg.contrast.value = contrastValue * 100;
			cg.temperature.overrideState = true;
			cg.temperature.value = temp;
			cg.enabled.value = true;

			ae.keyValue.value = eyeKeyValue;
			ae.enabled.value = true;
		}
	}

	public void Update_SSR(Camera mainCamera ,bool enabled,ScreenSpaceReflectionPreset preset,float atten,float fade)
	{

		ScreenSpaceReflections ssr;
		GameObject.Find("Global Volume").GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings(out ssr);

		ssr.enabled.overrideState = true;
		ssr.enabled.value = enabled;

		ssr.preset.overrideState = true;
		ssr.preset.value = preset;

		ssr.vignette.overrideState = true;
		ssr.vignette.value = atten;

		ssr.distanceFade.overrideState = true;
		ssr.distanceFade.value = fade;


	}

	public void Update_StochasticSSR(Camera mainCamera ,bool enabled,ResolutionMode resolutionMode,SSRDebugPass debugPass
		,int rayDistance ,float screenFadeSize, float smoothnessRange )
	{
		if (enabled) {
			if (!mainCamera.GetComponent<StochasticSSR> ()) {
				mainCamera.gameObject.AddComponent<StochasticSSR> ();
				StochasticSSR ssr = mainCamera.GetComponent<StochasticSSR> ();

				ssr.depthMode = resolutionMode;
				ssr.rayMode = resolutionMode;
				ssr.screenFadeSize = screenFadeSize;
				ssr.debugPass = debugPass;
				ssr.rayDistance = rayDistance;
				ssr.smoothnessRange = smoothnessRange;

			} else {
				StochasticSSR ssr = mainCamera.GetComponent<StochasticSSR> ();

				ssr.depthMode = resolutionMode;
				ssr.rayMode = resolutionMode;
				ssr.screenFadeSize = screenFadeSize;
				ssr.debugPass = debugPass;
				ssr.rayDistance = rayDistance;
				ssr.smoothnessRange = smoothnessRange;

			}
		}
		if (!enabled) {
			if (mainCamera.GetComponent<StochasticSSR> ()) {
				if(Application.isPlaying)
					Destroy (mainCamera.GetComponent<StochasticSSR> ());					
				else
					DestroyImmediate (mainCamera.GetComponent<StochasticSSR> ());
			}

		}
	}

	public void Update_SkyBox(bool enabled ,Material material)
	{
		if (enabled) {
			if (material)
				RenderSettings.skybox = material;
		}
		
	}

	public void Update_LightingMode(bool enabled, LightingMode lightingMode)
	{
		if (enabled) {
			#if UNITY_EDITOR
			if (lightingMode == LightingMode.RealtimeGI) {
				Lightmapping.realtimeGI = true;
				Lightmapping.bakedGI = false;
				LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.Enlighten;
			}
			if (lightingMode == LightingMode.Baked) {
				Lightmapping.realtimeGI = false;
				Lightmapping.bakedGI = true;
				LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveCPU;
			}
			if (lightingMode == LightingMode.FullyRealtime) {
				Lightmapping.realtimeGI = false;
				Lightmapping.bakedGI = false;
			}
			if (lightingMode == LightingMode.RealtimeGIandBakedGI) {
				Lightmapping.realtimeGI = true;
				Lightmapping.bakedGI = true;
				LightmapEditorSettings.lightmapper = LightmapEditorSettings.Lightmapper.ProgressiveCPU;
			}
			#endif
		}
	}

	public void Update_Ambient(bool enabled,AmbientLight ambientLight,Color ambientColor,Color skyColor,Color equatorColor
		,Color groundColor)
	{
		if (enabled) {
			if (ambientLight == AmbientLight.Color) {
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
				RenderSettings.ambientLight = ambientColor;
			}
			if (ambientLight == AmbientLight.Skybox)
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
			if (ambientLight == AmbientLight.Gradient) {
				RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
				RenderSettings.ambientSkyColor = skyColor;
				RenderSettings.ambientEquatorColor = equatorColor;
				RenderSettings.ambientGroundColor = groundColor;
			}


		}
	}

	#if UNITY_EDITOR
	public void Update_LightSettings(bool enabled, LightSettings lightSettings)
	{
		if(enabled)
		{
			if (lightSettings == LightSettings.Baked) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					SerializedObject serialLightSource = new SerializedObject(l);
					SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
					SerialProperty.intValue = 2;
					serialLightSource.ApplyModifiedProperties ();
				}
			} 
			if (lightSettings == LightSettings.Realtime) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					SerializedObject serialLightSource = new SerializedObject(l);
					SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
					SerialProperty.intValue = 4;
					serialLightSource.ApplyModifiedProperties ();
				}
			}
			if (lightSettings == LightSettings.Mixed) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					SerializedObject serialLightSource = new SerializedObject(l);
					SerializedProperty SerialProperty  = serialLightSource.FindProperty("m_Lightmapping");
					SerialProperty.intValue = 1;
					serialLightSource.ApplyModifiedProperties ();
				}

			}
		}
	}


	public void Update_ColorSpace(bool enabled, MyColorSpace colorSpace)
	{
		if (enabled) {
			if (colorSpace == MyColorSpace.Gamma)
				PlayerSettings.colorSpace = ColorSpace.Gamma;
			else
				PlayerSettings.colorSpace = ColorSpace.Linear;
		}
	}

	public void Update_AutoMode(bool enabled)
	{
		if(enabled)
			Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.Iterative;
		else
			Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
	}
	public void Update_LightProbes(bool enabled, LightProbeMode lightProbesMode)
	{
		if (enabled) {
			if (lightProbesMode == LightProbeMode.Blend) {

				MeshRenderer[] renderers = GameObject.FindObjectsOfType<MeshRenderer> ();

				foreach (MeshRenderer mr in renderers) {
					if (!mr.gameObject.isStatic) {
						if (mr.gameObject.GetComponent<LightProbeProxyVolume> ()) {
							if (Application.isPlaying)
								Destroy (mr.gameObject.GetComponent<LightProbeProxyVolume> ());
							else
								DestroyImmediate (mr.gameObject.GetComponent<LightProbeProxyVolume> ());
						}
						mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
					}
				}
			}
			if (lightProbesMode == LightProbeMode.Proxy) {

				MeshRenderer[] renderers = GameObject.FindObjectsOfType<MeshRenderer> ();

				foreach (MeshRenderer mr in renderers) {

					if (!mr.gameObject.isStatic) {
						if (!mr.gameObject.GetComponent<LightProbeProxyVolume> ())
							mr.gameObject.AddComponent<LightProbeProxyVolume> ();
						mr.gameObject.GetComponent<LightProbeProxyVolume> ().resolutionMode = LightProbeProxyVolume.ResolutionMode.Custom;
						mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.UseProxyVolume;
					}
				}
			}
		}
	}

	public void Update_Shadows(bool enabled, LightsShadow lightsShadow)
	{
		if (enabled) {
			if (lightsShadow == LightsShadow.AllLightsSoft) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.type == LightType.Directional)
						l.shadows = LightShadows.Soft;

					if (l.type == LightType.Spot)
						l.shadows = LightShadows.Soft;

					if (l.type == LightType.Point)
						l.shadows = LightShadows.Soft;
				}
			}
			if (lightsShadow == LightsShadow.AllLightsHard) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.type == LightType.Directional)
						l.shadows = LightShadows.Hard;

					if (l.type == LightType.Spot)
						l.shadows = LightShadows.Hard;

					if (l.type == LightType.Point)
						l.shadows = LightShadows.Hard;
				}
			}
			if (lightsShadow == LightsShadow.OnlyDirectionalSoft) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.type == LightType.Directional)
						l.shadows = LightShadows.Soft;

					if (l.type == LightType.Spot)
						l.shadows = LightShadows.None;

					if (l.type == LightType.Point)
						l.shadows = LightShadows.None;
				}
			}
			if (lightsShadow == LightsShadow.OnlyDirectionalHard) {

				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.type == LightType.Directional)
						l.shadows = LightShadows.Hard;

					if (l.type == LightType.Spot)
						l.shadows = LightShadows.None;

					if (l.type == LightType.Point)
						l.shadows = LightShadows.None;
				}
			}
			if (lightsShadow == LightsShadow.Off) {
				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights)
					l.shadows = LightShadows.None;



			}
		}
	}

	#endif

	public void Update_RenderPath(bool enabled, Render_Path renderPath,Camera mainCamera)
	{
		if (enabled) {
			if (renderPath == Render_Path.Forward)
				mainCamera.renderingPath = RenderingPath.Forward;
			if (renderPath == Render_Path.Deferred)
				mainCamera.renderingPath = RenderingPath.DeferredShading;
			if (renderPath == Render_Path.Default)
				mainCamera.renderingPath = RenderingPath.UsePlayerSettings;

			mainCamera.allowHDR = true;
			mainCamera.allowMSAA = false;
			mainCamera.useOcclusionCulling = true;
		}
	}

	public void Update_SunShaft(Camera mainCamera,bool enabled, UnityStandardAssets.ImageEffects.SunShafts.SunShaftsResolution shaftQuality,float shaftDistance,float shaftBlur, Color shaftColor, Transform sun)
	{
		if (!sun) {
			Debug.Log ("Couldn't find sun for Sun Shaft effect");
			if (mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ()) {
				if (Application.isPlaying)
					Destroy (mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ());
				else
					DestroyImmediate (mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ());
			}
			return;
		}

		if (enabled) {
			if (!mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ())
				mainCamera.gameObject.AddComponent<UnityStandardAssets.ImageEffects.SunShafts> ();

			mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().sunShaftsShader = Shader.Find
			("Hidden/SunShaftsComposite");
			mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().simpleClearShader = Shader.Find
			("Hidden/SimpleClear");
			mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().resolution = shaftQuality;
			mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().screenBlendMode = UnityStandardAssets.ImageEffects.SunShafts.ShaftsScreenBlendMode.Screen;
			mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().sunShaftIntensity = 1f;
			mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().sunThreshold = Color.black;
			mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().sunColor = shaftColor;
			mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().sunShaftBlurRadius = shaftBlur;
			mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().radialBlurIterations = 2;
			mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().maxRadius = shaftDistance;
			if (!GameObject.Find ("Shaft Caster")) {
				GameObject shaftCaster = new GameObject ("Shaft Caster");
				shaftCaster.transform.parent = sun;
				shaftCaster.transform.localPosition = new Vector3 (0, 0, -7000f);
				mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().sunTransform = shaftCaster.transform;
			} else {
				GameObject.Find ("Shaft Caster").transform.parent = sun;
				GameObject.Find ("Shaft Caster").transform.localPosition = new Vector3 (0, 0, -7000f);
				mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ().sunTransform = GameObject.Find ("Shaft Caster").transform;
			}
		} else {
			if (mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ()) {
				if (Application.isPlaying)
					Destroy (mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ());
				else
					DestroyImmediate (mainCamera.gameObject.GetComponent<UnityStandardAssets.ImageEffects.SunShafts> ());
			}
		}
	}

	public void Update_VolumetricLight(Camera mainCamera, bool enabled, VolumetricLightType volumetricLight,VLightLevel vLightLevel)
	{
		if (enabled) {

			if (!mainCamera.gameObject.GetComponent<VolumetricLightRenderer> ())
			{
				mainCamera.gameObject.AddComponent<VolumetricLightRenderer> ();
				mainCamera.gameObject.GetComponent<VolumetricLightRenderer> ().Resolution = VolumetricLightRenderer.VolumtericResolution.Quarter;
				mainCamera.gameObject.GetComponent<VolumetricLightRenderer> ().DefaultSpotCookie = Resources.Load ("spot_Cookie_") as Texture;
			}

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) {
				// Only directional
				if (volumetricLight == VolumetricLightType.OnlyDirectional) {
					if (l.type == LightType.Directional) {
						if (!l.gameObject.GetComponent<VolumetricLight> ())
							l.gameObject.AddComponent<VolumetricLight> ();
					
						l.gameObject.GetComponent<VolumetricLight> ().SampleCount = 8;

						if (vLightLevel == VLightLevel.Level1)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0007f;
						if (vLightLevel == VLightLevel.Level2)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.001f;
						if (vLightLevel == VLightLevel.Level3)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.003f;
						if (vLightLevel == VLightLevel.Level4)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0043f;
				
						l.gameObject.GetComponent<VolumetricLight> ().ExtinctionCoef = 0;
						l.gameObject.GetComponent<VolumetricLight> ().SkyboxExtinctionCoef = 0.864f;
						l.gameObject.GetComponent<VolumetricLight> ().MieG = 0.675f;
						l.gameObject.GetComponent<VolumetricLight> ().HeightFog = false;
						l.gameObject.GetComponent<VolumetricLight> ().HeightScale = 0.1f;
						l.gameObject.GetComponent<VolumetricLight> ().GroundLevel = 0;

						l.gameObject.GetComponent<VolumetricLight> ().Noise = false;

						l.gameObject.GetComponent<VolumetricLight> ().NoiseScale = 0.015f;
						l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensity = 1f;
						l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensityOffset = 0.3f;
						l.gameObject.GetComponent<VolumetricLight> ().NoiseVelocity = new Vector2 (3f, 3f);
						l.gameObject.GetComponent<VolumetricLight> ().MaxRayLength = 400;
					}
					if (l.type == LightType.Spot || l.type == LightType.Point) {
						
						if (l.gameObject.GetComponent<VolumetricLight> ())
						{
							if(Application.isPlaying)
								Destroy (l.gameObject.GetComponent<VolumetricLight> ());					
							else
								DestroyImmediate (l.gameObject.GetComponent<VolumetricLight> ());
						}
					}
				}

				//-------------------------------------------------------------------------------

				// All light sources
				if (volumetricLight == VolumetricLightType.AllLightSources)
				{
					
					if (!l.gameObject.GetComponent<VolumetricLight> ())
						l.gameObject.AddComponent<VolumetricLight> ();

					l.gameObject.GetComponent<VolumetricLight> ().SampleCount = 8;

					if (l.type == LightType.Directional) 
					{
						if (vLightLevel == VLightLevel.Level1)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0007f;
						if (vLightLevel == VLightLevel.Level2)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.001f;
						if (vLightLevel == VLightLevel.Level3)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.003f;
						if (vLightLevel == VLightLevel.Level4)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.0043f;
					} else {
						if (vLightLevel == VLightLevel.Level1)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.021f;
						if (vLightLevel == VLightLevel.Level2)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.073f;
						if (vLightLevel == VLightLevel.Level3)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.1f;
						if (vLightLevel == VLightLevel.Level4)
							l.gameObject.GetComponent<VolumetricLight> ().ScatteringCoef = 0.21f;
					}

					l.gameObject.GetComponent<VolumetricLight> ().ExtinctionCoef = 0;
					l.gameObject.GetComponent<VolumetricLight> ().SkyboxExtinctionCoef = 0.864f;
					l.gameObject.GetComponent<VolumetricLight> ().MieG = 0.675f;
					l.gameObject.GetComponent<VolumetricLight> ().HeightFog = false;
					l.gameObject.GetComponent<VolumetricLight> ().HeightScale = 0.1f;
					l.gameObject.GetComponent<VolumetricLight> ().GroundLevel = 0;

					if (l.type == LightType.Directional)
						l.gameObject.GetComponent<VolumetricLight> ().Noise = false;
					else {
						l.gameObject.GetComponent<VolumetricLight> ().Noise = true;

						if (l.type == LightType.Spot) {
							if (l.range == 10f)
								l.range = 43f;
							if (l.spotAngle == 30f)
								l.spotAngle = 43f;
						}
					}

					l.gameObject.GetComponent<VolumetricLight> ().NoiseScale = 0.015f;
					l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensity = 1f;
					l.gameObject.GetComponent<VolumetricLight> ().NoiseIntensityOffset = 0.3f;
					l.gameObject.GetComponent<VolumetricLight> ().NoiseVelocity = new Vector2 (3f, 3f);
					l.gameObject.GetComponent<VolumetricLight> ().MaxRayLength = 400;
				}
			}
		}
		if (!enabled) 
		{

			if (mainCamera.gameObject.GetComponent<VolumetricLightRenderer> ()) {
				if(Application.isPlaying)
					Destroy (mainCamera.gameObject.GetComponent<VolumetricLightRenderer> ());					
				else
					DestroyImmediate (mainCamera.gameObject.GetComponent<VolumetricLightRenderer> ());
			}

			Light[] lights = GameObject.FindObjectsOfType<Light> ();

			foreach (Light l in lights) 
			{
				if (l.gameObject.GetComponent<VolumetricLight> ()) {
					if(Application.isPlaying)
						Destroy (l.gameObject.GetComponent<VolumetricLight> ());					
					else
						DestroyImmediate (l.gameObject.GetComponent<VolumetricLight> ());
				}
			}
		}
	}

	public void Update_GlobalFog(Camera mainCamera, bool enabled, CustomFog fogMode,float fogDistance,float fogHeight,float fogHeightDensity,Color fogColor,float fogDensity)
	{
		if (enabled)
		{
			if (fogMode == CustomFog.Distance)
				UpdateFog (mainCamera, 1, fogDistance, fogHeight, fogHeightDensity, fogColor, fogDensity);
			if (fogMode == CustomFog.Global)
				UpdateFog (mainCamera, 2, fogDistance, fogHeight, fogHeightDensity, fogColor, fogDensity);
			//-----Height--------------------------------------------------------------------
			if (fogMode == CustomFog.Height)
				UpdateFog (mainCamera, 0, fogDistance, fogHeight, fogHeightDensity, fogColor, fogDensity);
			//-----Global Fog Type--------------------------------------------------------------------

			if (fogMode == CustomFog.Height)
				UpdateFog (mainCamera, 0, fogDistance, fogHeight, fogHeightDensity, fogColor, fogDensity);
			if (fogMode == CustomFog.Distance)
				UpdateFog (mainCamera, 1, fogDistance, fogHeight, fogHeightDensity, fogColor, fogDensity);
			if (fogMode == CustomFog.Global)
				UpdateFog (mainCamera, 2, fogDistance, fogHeight, fogHeightDensity, fogColor, fogDensity);
		}
		if (!enabled)
			UpdateFog(mainCamera,3,fogDistance,fogHeight,fogHeightDensity,fogColor,fogDensity);
	}

	private void UpdateFog(Camera mainCamera,int fogType,float fogDistance,float fogHeight,float fogHeightDensity,Color fogColor,float fogDensity) // 0 Height , 1 Distance , 2 Global , 3 Off
	{

		//-------Height---------------------------------------------------------------------
		if (fogType == 0) {

			if (!mainCamera.gameObject.GetComponent<GlobalFog> ())
			{
				mainCamera.gameObject.AddComponent<GlobalFog> ();
				mainCamera.gameObject.GetComponent<GlobalFog> ().fogShader = Shader.Find ("Hidden/GlobalFog");
				mainCamera.gameObject.GetComponent<GlobalFog> ().distanceFog = false;
				mainCamera.gameObject.GetComponent<GlobalFog> ().heightFog = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().startDistance = fogDistance;

				mainCamera.gameObject.GetComponent<GlobalFog> ().heightDensity = fogHeightDensity;
				mainCamera.gameObject.GetComponent<GlobalFog> ().height = fogHeight;
				mainCamera.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
				RenderSettings.fog = false;
				RenderSettings.fogColor = fogColor;
				RenderSettings.fogMode = FogMode.ExponentialSquared;
				RenderSettings.fogDensity = fogDensity/1000;

			} else {
				mainCamera.gameObject.GetComponent<GlobalFog> ().distanceFog = false;
				mainCamera.gameObject.GetComponent<GlobalFog> ().heightFog = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().startDistance = fogDistance;

				mainCamera.gameObject.GetComponent<GlobalFog> ().heightDensity = fogHeightDensity;
				mainCamera.gameObject.GetComponent<GlobalFog> ().height = fogHeight;
				mainCamera.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
				RenderSettings.fog = false;
				RenderSettings.fogColor = fogColor;
				RenderSettings.fogMode = FogMode.ExponentialSquared;
				RenderSettings.fogDensity = fogDensity/1000;

			}
		}
		//----------------------------------------------------------------------------

		//-------Distance---------------------------------------------------------------------
		if (fogType == 1) {

			if (!mainCamera.gameObject.GetComponent<GlobalFog> ())
			{
				mainCamera.gameObject.AddComponent<GlobalFog> ();
				mainCamera.gameObject.GetComponent<GlobalFog> ().fogShader = Shader.Find ("Hidden/GlobalFog");
				mainCamera.gameObject.GetComponent<GlobalFog> ().distanceFog = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().heightFog = false;
				mainCamera.gameObject.GetComponent<GlobalFog> ().startDistance = fogDistance;
				mainCamera.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
				RenderSettings.fog = true;
				RenderSettings.fogColor = fogColor;
				RenderSettings.fogMode = FogMode.ExponentialSquared;
				RenderSettings.fogDensity = fogDensity/1000;
			} else {
				mainCamera.gameObject.GetComponent<GlobalFog> ().distanceFog = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().heightFog = false;
				mainCamera.gameObject.GetComponent<GlobalFog> ().startDistance = fogDistance;
				mainCamera.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
				RenderSettings.fog = true;
				RenderSettings.fogColor = fogColor;
				RenderSettings.fogMode = FogMode.ExponentialSquared;
				RenderSettings.fogDensity = fogDensity/1000;
			}
		}
		//----------------------------------------------------------------------------

		//-------Global---------------------------------------------------------------------
		if (fogType == 2) {

			if (!mainCamera.gameObject.GetComponent<GlobalFog> ())
			{
				mainCamera.gameObject.GetComponent<PostProcessLayer> ().fog.enabled = true;
				mainCamera.gameObject.GetComponent<PostProcessLayer> ().fog.excludeSkybox = true;


				mainCamera.gameObject.AddComponent<GlobalFog> ();
				mainCamera.gameObject.GetComponent<GlobalFog> ().fogShader = Shader.Find ("Hidden/GlobalFog");
				mainCamera.gameObject.GetComponent<GlobalFog> ().distanceFog = false;
				mainCamera.gameObject.GetComponent<GlobalFog> ().heightFog = false;
				mainCamera.gameObject.GetComponent<GlobalFog> ().startDistance = fogDistance;
				mainCamera.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
				RenderSettings.fog = true;
				RenderSettings.fogColor = fogColor;
				RenderSettings.fogMode = FogMode.ExponentialSquared;
				RenderSettings.fogDensity = fogDensity/1000;
			} else {
				mainCamera.gameObject.GetComponent<PostProcessLayer> ().fog.enabled = true;
				mainCamera.gameObject.GetComponent<PostProcessLayer> ().fog.excludeSkybox = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().distanceFog = false;
				mainCamera.gameObject.GetComponent<GlobalFog> ().heightFog = false;
				mainCamera.gameObject.GetComponent<GlobalFog> ().startDistance = fogDistance;
				mainCamera.gameObject.GetComponent<GlobalFog> ().useRadialDistance = true;
				mainCamera.gameObject.GetComponent<GlobalFog> ().excludeFarPixels = true;
				RenderSettings.fog = true;
				RenderSettings.fogColor = fogColor;
				RenderSettings.fogMode = FogMode.ExponentialSquared;
				RenderSettings.fogDensity = fogDensity/1000;
			}
		}
		//----------------------------------------------------------------------------

		//-------Off---------------------------------------------------------------------
		if (fogType == 3) {

			if (mainCamera.gameObject.GetComponent<GlobalFog> ()) {
				if(Application.isPlaying)
					Destroy (mainCamera.gameObject.GetComponent<GlobalFog> ());					
				else
					DestroyImmediate (mainCamera.gameObject.GetComponent<GlobalFog> ());
				RenderSettings.fog = false;
			}
		}
		//----------------------------------------------------------------------------
	}

	public void Update_Sun(bool enabled,Light sunLight,Color sunColor,float indirectIntensity)
	{
		if (enabled) {
			if (!RenderSettings.sun) {
				Light[] lights = GameObject.FindObjectsOfType<Light> ();

				foreach (Light l in lights) {
					if (l.type == LightType.Directional) {
						sunLight = l;

						if (sunColor != Color.clear)
							sunColor = sunLight.color;
						else
							sunColor = Color.white;

						//	sunLight.shadowNormalBias = 0.05f;  
						sunLight.color = sunColor;
						if (sunLight.bounceIntensity == 1f)
							sunLight.bounceIntensity = indirectIntensity;
					}
				}
			} else {
				sunLight = RenderSettings.sun;

				if (sunColor != Color.clear)
					sunColor = sunLight.color;
				else
					sunColor = Color.white;

				//sunLight.shadowNormalBias = 0.05f;  
				sunLight.color = sunColor;
				if (sunLight.bounceIntensity == 1f)
					sunLight.bounceIntensity = indirectIntensity;
			}
		}
	}

	bool effectsIsOn = true;

	public void Toggle_Effects()
	{
		effectsIsOn = !effectsIsOn;

		// Post layers
		PostProcessLayer[] postLayers = GameObject.FindObjectsOfType<PostProcessLayer> ();
		for (int a = 0; a < postLayers.Length; a++)
			postLayers [a].enabled = effectsIsOn;

		// Depth of Field
		LightingBox.Effects.DepthOfField[] dofEffects = GameObject.FindObjectsOfType<LightingBox.Effects.DepthOfField> ();
		for (int a = 0; a < dofEffects.Length; a++)
			dofEffects [a].enabled = effectsIsOn;

		// Stochastic SSR
		StochasticSSR[] stSSR = GameObject.FindObjectsOfType<StochasticSSR> ();
		for (int a = 0; a < stSSR.Length; a++)
			stSSR [a].enabled = effectsIsOn;
		
		// Global fog
		LightingBox.Effects.GlobalFog[] gFogS = GameObject.FindObjectsOfType<LightingBox.Effects.GlobalFog> ();
		for (int a = 0; a < gFogS.Length; a++)
			gFogS [a].enabled = effectsIsOn;

		// Sun Shaft
		UnityStandardAssets.ImageEffects.SunShafts[] sunShaftS = GameObject.FindObjectsOfType<UnityStandardAssets.ImageEffects.SunShafts> ();
		for (int a = 0; a < sunShaftS.Length; a++)
			sunShaftS [a].enabled = effectsIsOn;

		// Volumetric Light RendererS
		VolumetricLightRenderer[] vlRendererS = GameObject.FindObjectsOfType<VolumetricLightRenderer> ();
		for (int a = 0; a < vlRendererS.Length; a++)
			vlRendererS [a].enabled = effectsIsOn;
	}
}
