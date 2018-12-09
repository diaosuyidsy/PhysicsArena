﻿
// AliyerEdon@gmail.com/
// Lighting Box is an "paid" asset. Don't share it for free

#if UNITY_EDITOR   
using UnityEngine;   
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;   
using UnityEngine.Rendering.PostProcessing;
using LightingBox.Effects;
using SharpConfig;
using cCharkes;

[ExecuteInEditMode]
public class LB_LightingBox : EditorWindow
{
	#region Variables

	public WindowMode winMode = WindowMode.Part1;
	public LB_LightingBoxHelper helper;
	public bool webGL_Mobile = false;

	// Sun Shaft
	public UnityStandardAssets.ImageEffects.SunShafts.SunShaftsResolution shaftQuality = UnityStandardAssets.ImageEffects.SunShafts.SunShaftsResolution.High;
	public float shaftDistance = 0.5f;
	public float shaftBlur = 4f;
	public Color shaftColor = new Color32 (255,189,146,255);

	// AA
	public AAMode aaMode;

	// AO
	public AOType aoType;
	public float aoRadius;
	public float aoIntensity = 1f;
	public bool ambientOnly = true;
	public Color aoColor = Color.black;
	public AmbientOcclusionQuality aoQuality = AmbientOcclusionQuality.Medium;

	// Bloom
	public float bIntensity = 1f;
	public float bThreshould = 0.5f;
	public Color bColor = Color.white;
	public Texture2D dirtTexture;
	public float dirtIntensity;
	public bool mobileOptimizedBloom;
	public float bRotation;

	public bool visualize;

	// Color settings
	public float exposureIntensity = 1.43f;
	public float contrastValue = 30f;
	public float temp = 0;
	public float eyeKeyValue = 0.17f;
	public ColorMode colorMode;
	public float gamma = 0;
	public Color colorGamma = Color.black;
	public Color colorLift = Color.black;
	public float saturation = 0;
	public Texture lut;

	// SSR
	public ScreenSpaceReflectionPreset ssrQuality = ScreenSpaceReflectionPreset.Lower;
	public float ssrAtten;
	public float ssrFade;

	// Stochastic SSR    
	public ResolutionMode resolutionMode = ResolutionMode.halfRes;
	public SSRDebugPass debugPass = SSRDebugPass.Combine;
	public int rayDistance = 70;
	public float screenFadeSize = 0;
	public float smoothnessRange = 1f;

	public float vignetteIntensity = 0.1f;
	public float CA_Intensity = 0.1f;
	public bool mobileOptimizedChromattic;

	public Render_Path renderPath;

	// Profiles
	public LB_LightingProfile LB_LightingProfile;
	public PostProcessProfile postProcessingProfile;

	public LightingMode lightingMode;
	public AmbientLight ambientLight;
	public LightSettings lightSettings;
	public LightProbeMode lightprobeMode;

	// Depth of field
	public float dofRange;
	public float dofBlur;
	public float falloff = 30f;
	public DOFQuality dofQuality;
	// Auto Focus
	public float afRange = 100f;
	public float afBlur = 30f;
	public float afSpeed = 100f;
	public float afUpdate = 0.001f;
	public float afRayLength = 10f;
	public LayerMask afLayer = 1;

	// Depth of Field Stack 2
	public float dof_Distance = 0;

	// Auto Focus stack 2
	public LB_Stack2_DOF_AutoFocus.DoFAFocusQuality dof_focusQuality = LB_Stack2_DOF_AutoFocus.DoFAFocusQuality.NORMAL;
	public LayerMask dof_hitLayer = 1;
	public float dof_maxDistance = 100.0f;
	public bool dof_interpolateFocus = false;
	public float dof_interpolationTime = 0.7f;

	// Sky and Sun
	public Material skyBox;
	public Light sunLight;
	public Flare sunFlare;
	public Color sunColor = Color.white;
	public float sunIntensity = 2.1f;
	public float indirectIntensity = 1.43f;
	public  Color ambientColor = new Color32(96,104,116,255);
	public Color skyColor;
	public Color equatorColor,groundColor;

	public bool autoMode;
	public MyColorSpace colorSpace;

	// Volumetric Light
	public VolumetricLightType vLight;
	public VLightLevel vLightLevel;

	// Volumetric Fog
	CustomFog vFog;
	float fDistance = 0;
	float fHeight = 30f;
	[Range(0,1)]
	float fheightDensity = 0.5f;
	Color fColor = Color.white;
	[Range(0,10)]
	float fogIntensity = 1f;

	public LightsShadow psShadow;
	public float bakedResolution = 10f;
	public bool helpBox;

	// Private variabled
	Color redColor;
	bool lightError;
	bool lightChecked;
	GUIStyle myFoldoutStyle;
	bool showLogs;
	// Display window elements (Lighting Box)   
	Vector2 scrollPos = Vector2.zero;

	// Camera
	public Camera mainCamera;

	// Foliage
	public float translucency;
	public float ambient;
	public float shadows;
	public Color tranColor;
	public float windSpeed;
	public float windScale;
	public string CustomShader = "Legacy Shaders/Transparent/Diffuse";

	// Snow
	public Texture2D snowAlbedo;
	public Texture2D snowNormal;
	public float snowIntensity = 0;
	public string customShaderSnow = "Legacy Shaders/Diffuse";

	// Material converter
	public MatType matType;
	#endregion

	#region Init()
	// Add menu named "My Window" to the Window menu
	[MenuItem("Window/Lighting Box 2 %E")]
	static void Init()
	{
		// Get existing open window or if none, make a new one:
	////	LB_LightingBox window = (LB_LightingBox)EditorWindow.GetWindow(typeof(LB_LightingBox));
		System.Type inspectorType = System.Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
		LB_LightingBox window = (LB_LightingBox)EditorWindow.GetWindow<LB_LightingBox>("Lighting Box 2", true, new System.Type[] {inspectorType} );

		window.Show();
		window.autoRepaintOnSceneChange = true;
		window.maxSize = new Vector2 (1000f, 1000f);
		window.minSize = new Vector2 (387f, 1000f);

	}
	#endregion

	#region Options
	// Internal Usage
	public bool LightingBoxState = true,OptionsState = true;
	public bool ambientState = true;
	public bool sunState = true;
	public bool lightSettingsState = true;
	public bool cameraState = true;
	public bool profileState = true;
	public bool buildState = true;
	public bool vLightState = true;
	public bool sunShaftState = true;
	public bool fogState = true;
	public bool dofState = true;
	public bool autoFocusState =  true;
	public bool colorState = true;
	public bool bloomState = true;
	public bool aaState = true;
	public bool aoState = true;
	public bool motionBlurState = true;
	public bool vignetteState = true;
	public bool chromatticState = true;
	public bool ssrState = true;
	public bool st_ssrState;
	public bool foliageState = true;
	public bool snowState = true;

	// Effects enabled
	public bool Ambient_Enabled = true;
	public bool Scene_Enabled = true;
	public bool Sun_Enabled = true;
	public bool VL_Enabled = false;
	public bool SunShaft_Enabled = false;
	public bool Fog_Enabled = false;
	public bool DOF_Enabled = true;
	public bool Bloom_Enabled = false;
	public bool AA_Enabled = true;
	public bool AO_Enabled = false;
	public bool MotionBlur_Enabled = true;
	public bool Vignette_Enabled = true;
	public bool Chromattic_Enabled = true;
	public bool SSR_Enabled = false;
	public bool AutoFocus_Enabled = false;
	public bool ST_SSR_Enabled = false;

	Texture2D arrowOn,arrowOff;

	#endregion

	void NewSceneInit()
	{
		if (EditorSceneManager.GetActiveScene ().name == "") 
		{
			LB_LightingProfile = Resources.Load ("DefaultSettings")as LB_LightingProfile;
			helper.Update_MainProfile(LB_LightingProfile);

			OnLoad ();
			currentScene = EditorSceneManager.GetActiveScene ().name;

		} 
		else
		{
			if (System.String.IsNullOrEmpty (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name))) 
			{
				LB_LightingProfile = Resources.Load ("DefaultSettings")as LB_LightingProfile;
				helper.Update_MainProfile(LB_LightingProfile);

			} else 
			{
				LB_LightingProfile = (LB_LightingProfile)AssetDatabase.LoadAssetAtPath (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name), typeof(LB_LightingProfile));
				helper.Update_MainProfile(LB_LightingProfile);

			}

			OnLoad ();   
			currentScene = EditorSceneManager.GetActiveScene ().name;

		}


	}

	// Load and apply default settings when a new scene opened
	void OnNewSceneOpened()
	{
		NewSceneInit ();
	}

	void OnDisable()
	{
		EditorApplication.hierarchyWindowChanged -= OnNewSceneOpened;
	}

	void OnEnable()
	{
		arrowOn = Resources.Load ("arrowOn") as Texture2D;
		arrowOff = Resources.Load ("arrowOff") as Texture2D;

		if (!GameObject.Find ("LightingBox_Helper")) 
		{
			GameObject helperObject = new GameObject ("LightingBox_Helper");
			helperObject.AddComponent<LB_LightingBoxHelper> ();
			helper = helperObject.GetComponent<LB_LightingBoxHelper> ();
		}

		EditorApplication.hierarchyWindowChanged += OnNewSceneOpened;

		currentScene = EditorSceneManager.GetActiveScene().name;

		if (System.String.IsNullOrEmpty (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name)))
			LB_LightingProfile = Resources.Load ("DefaultSettings")as LB_LightingProfile;
		else
			LB_LightingProfile = (LB_LightingProfile)AssetDatabase.LoadAssetAtPath (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name), typeof(LB_LightingProfile));

		OnLoad ();

	}

	void OnGUI()
	{

		#region Styles
		GUIStyle redStyle = new GUIStyle (EditorStyles.label);
		redStyle.alignment = TextAnchor.MiddleLeft;
		redStyle.normal.textColor = Color.red;

		GUIStyle blueStyle = new GUIStyle (EditorStyles.label);
		blueStyle.alignment = TextAnchor.MiddleLeft;
		blueStyle.normal.textColor = Color.blue;


		GUIStyle stateButton = new GUIStyle ();
		stateButton = "Label";
		stateButton.alignment = TextAnchor.MiddleLeft;
		stateButton.fontStyle = FontStyle.Bold;

		GUIStyle buttonMain = new GUIStyle ();
		buttonMain = "Box";
		buttonMain.alignment = TextAnchor.MiddleCenter;
		buttonMain.fontStyle = FontStyle.Bold;

		#endregion

		#region GUI start implementation
		Undo.RecordObject (this, "lb");

		scrollPos = EditorGUILayout.BeginScrollView (scrollPos,
			false,
			false,
			GUILayout.Width (this.position.width),
			GUILayout.Height (this.position.height));

		EditorGUILayout.Space ();

		GUILayout.Label ("Lighting Box 2 - ALIyerEdon@gmail.com", EditorStyles.helpBox);


		EditorGUILayout.BeginHorizontal ();

		if (!helpBox) {
			if (GUILayout.Button ("Show Help", buttonMain, GUILayout.Width (177), GUILayout.Height (24f))) {
				helpBox = !helpBox;
			}
		} else {
			if (GUILayout.Button ("Hide Help", buttonMain, GUILayout.Width (177), GUILayout.Height (24f))) {
				helpBox = !helpBox;
			}
		}
		if (GUILayout.Button ("Refresh", buttonMain, GUILayout.Width (179), GUILayout.Height (24f))) {
			UpdateSettings ();
			UpdatePostEffects ();
		}

		EditorGUILayout.EndHorizontal ();

		if (EditorPrefs.GetInt ("RateLB") != 3) {

			if (GUILayout.Button ("Rate Lighting Box")) {
				EditorPrefs.SetInt ("RateLB", 3);
				Application.OpenURL ("http://u3d.as/Se9");
			}
		}

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();


		#endregion

		#region Tabs
		EditorGUILayout.BeginHorizontal ();
		//----------------------------------------------
		if (winMode == WindowMode.Part1)
			GUI.backgroundColor = Color.green;
		else
			GUI.backgroundColor = Color.white;
		//----------------------------------------------
		if (GUILayout.Button ("Scene", buttonMain, GUILayout.Width (87), GUILayout.Height (43)))
			winMode = WindowMode.Part1;
		//----------------------------------------------
		if (winMode == WindowMode.Part2)
			GUI.backgroundColor = Color.green;
		else
			GUI.backgroundColor = Color.white;
		//----------------------------------------------
		if (GUILayout.Button ("Effect", buttonMain, GUILayout.Width (87), GUILayout.Height (43)))
			winMode = WindowMode.Part2;
		//----------------------------------------------
		if (winMode == WindowMode.Part3)
			GUI.backgroundColor = Color.green;
		else
			GUI.backgroundColor = Color.white;
		//----------------------------------------------
		if (GUILayout.Button ("Color", buttonMain, GUILayout.Width (87), GUILayout.Height (43)))
			winMode = WindowMode.Part3;
		//----------------------------------------------
		if (winMode == WindowMode.Finish)
			GUI.backgroundColor = Color.green;
		else
			GUI.backgroundColor = Color.white;
		//----------------------------------------------
		if (GUILayout.Button ("Screen", buttonMain, GUILayout.Width (87), GUILayout.Height (43)))
			winMode = WindowMode.Finish;
		//----------------------------------------------
		GUI.backgroundColor = Color.white;
		//----------------------------------------------//----------------------------------------------//----------------------------------------------
		
	    EditorGUILayout.EndHorizontal ();

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		#endregion

		#region Toolbar

		EditorGUILayout.BeginHorizontal();
		if (LightingBoxState) 
		{
			if (GUILayout.Button ("Effects On", buttonMain, GUILayout.Width (177), GUILayout.Height (24))) {
				helper.Toggle_Effects ();

				LightingBoxState = !LightingBoxState;

				if(LB_LightingProfile)
					LB_LightingProfile.LightingBoxState = LightingBoxState;
			}
		} else {
			if (GUILayout.Button ("Effects Off", buttonMain, GUILayout.Width (177), GUILayout.Height (24))) {
				helper.Toggle_Effects ();
				LightingBoxState = !LightingBoxState;

				if(LB_LightingProfile)
					LB_LightingProfile.LightingBoxState = LightingBoxState;
			}
		}
		if(OptionsState)
		{
			if (GUILayout.Button ("Expand All", buttonMain, GUILayout.Width (179), GUILayout.Height (24))){
				ambientState = sunState = lightSettingsState = true;
				cameraState = profileState = buildState = true;
				vLightState = sunShaftState = fogState = true;
				dofState = autoFocusState = colorState = true;
				bloomState = aaState = aoState = true;
				motionBlurState = vignetteState = chromatticState = true;
				ssrState = foliageState = snowState = st_ssrState = true;
				OptionsState = !OptionsState;

				if(LB_LightingProfile)
				{
					LB_LightingProfile.ambientState = ambientState;
					LB_LightingProfile.sunState = sunState;
					LB_LightingProfile.lightSettingsState = lightSettingsState;
					LB_LightingProfile.cameraState = cameraState;
					LB_LightingProfile.profileState = profileState;
					LB_LightingProfile.buildState = buildState;
					LB_LightingProfile.vLightState = vLightState;
					LB_LightingProfile.sunShaftState = sunShaftState;
					LB_LightingProfile.fogState = fogState;
					LB_LightingProfile.dofState = dofState;
					LB_LightingProfile.autoFocusState = autoFocusState;
					LB_LightingProfile.colorState = colorState;
					LB_LightingProfile.bloomState = bloomState;
					LB_LightingProfile.aaState = aaState;
					LB_LightingProfile.aoState = aoState;
					LB_LightingProfile.motionBlurState = motionBlurState;
					LB_LightingProfile.vignetteState = vignetteState;
					LB_LightingProfile.chromatticState = chromatticState;
					LB_LightingProfile.ssrState = ssrState;
					LB_LightingProfile.st_ssrState = st_ssrState;
					LB_LightingProfile.foliageState = foliageState;
					LB_LightingProfile.snowState = snowState;
					LB_LightingProfile.OptionsState  = OptionsState;
					EditorUtility.SetDirty (LB_LightingProfile);
				}

			}
		}
		else
		{
			if (GUILayout.Button ("Close All", buttonMain, GUILayout.Width (179), GUILayout.Height (24))) {

				ambientState = sunState = lightSettingsState = false;
				cameraState = profileState = buildState = false;
				vLightState = sunShaftState = fogState = false;
				dofState = autoFocusState = colorState = false;
				bloomState = aaState = aoState = false;
				motionBlurState = vignetteState = chromatticState = false;
				ssrState = foliageState = snowState = st_ssrState = false;
				OptionsState = !OptionsState;

				if(LB_LightingProfile)
				{
					LB_LightingProfile.ambientState = ambientState;
					LB_LightingProfile.sunState = sunState;
					LB_LightingProfile.lightSettingsState = lightSettingsState;
					LB_LightingProfile.cameraState = cameraState;
					LB_LightingProfile.profileState = profileState;
					LB_LightingProfile.buildState = buildState;
					LB_LightingProfile.vLightState = vLightState;
					LB_LightingProfile.sunShaftState = sunShaftState;
					LB_LightingProfile.fogState = fogState;
					LB_LightingProfile.dofState = dofState;
					LB_LightingProfile.autoFocusState = autoFocusState;
					LB_LightingProfile.colorState = colorState;
					LB_LightingProfile.bloomState = bloomState;
					LB_LightingProfile.aaState = aaState;
					LB_LightingProfile.aoState = aoState;
					LB_LightingProfile.motionBlurState = motionBlurState;
					LB_LightingProfile.vignetteState = vignetteState;
					LB_LightingProfile.chromatticState = chromatticState;
					LB_LightingProfile.ssrState = ssrState;
					LB_LightingProfile.st_ssrState = st_ssrState;
					LB_LightingProfile.foliageState = foliageState;
					LB_LightingProfile.snowState = snowState;
					LB_LightingProfile.OptionsState  = OptionsState;
					EditorUtility.SetDirty (LB_LightingProfile);
				}

			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space ();
		EditorGUILayout.Space ();
		#endregion
		   
		if (winMode == WindowMode.Part1) {
			
			#region Toggle Settings


			#endregion

			#region Profiles

			//-----------Profile----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			EditorGUILayout.BeginHorizontal ();

			if(profileState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));

			var profileStateRef = profileState;

			if (GUILayout.Button ("Profile", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				profileState = !profileState;
			}

			if(profileStateRef != profileState)
			{
				
				if (LB_LightingProfile)
				{
					LB_LightingProfile.profileState = profileState;				
					EditorUtility.SetDirty (LB_LightingProfile);
				}

			}
			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			if (profileState) {
				if (helpBox)
					EditorGUILayout.HelpBox ("1. LB_LightingBox settings profile   2.Post Processing Stack Profile", MessageType.Info);

				var lightingProfileRef = LB_LightingProfile;
				var postProcessingProfileRef = postProcessingProfile;

				EditorGUILayout.BeginHorizontal ();
				LB_LightingProfile = EditorGUILayout.ObjectField ("Lighting Profile", LB_LightingProfile, typeof(LB_LightingProfile), true) as LB_LightingProfile;

				if (GUILayout.Button ("New", GUILayout.Width (43), GUILayout.Height (17))) {

					if (EditorSceneManager.GetActiveScene ().name == "")
						EditorSceneManager.SaveScene (EditorSceneManager.GetActiveScene ());

					string path = EditorUtility.SaveFilePanelInProject ("Save As ...", "Lighting_Profile_"+EditorSceneManager.GetActiveScene ().name, "asset", "");

					if (path != "")
					{
						LB_LightingProfile = new LB_LightingProfile ();

						AssetDatabase.CreateAsset (LB_LightingProfile, path);
						AssetDatabase.CopyAsset (AssetDatabase.GetAssetPath (Resources.Load ("DefaultSettings_LB")), path);
						LB_LightingProfile = (LB_LightingProfile)AssetDatabase.LoadAssetAtPath (path, typeof(LB_LightingProfile));
						helper.Update_MainProfile(LB_LightingProfile);

						AssetDatabase.Refresh ();

						string path2 = System.IO.Path.GetDirectoryName(path) + "/Post_Profile_"+EditorSceneManager.GetActiveScene ().name+".asset";
						// Create new post processing stack 2 profile
						postProcessingProfile = new PostProcessProfile ();
						AssetDatabase.CreateAsset (postProcessingProfile, path2);
						AssetDatabase.CopyAsset (AssetDatabase.GetAssetPath (Resources.Load ("Default_Post_Profile")), path2);
						postProcessingProfile = (PostProcessProfile)AssetDatabase.LoadAssetAtPath (path2, typeof(PostProcessProfile));
						LB_LightingProfile.postProcessingProfile = postProcessingProfile;

						AssetDatabase.Refresh ();
						    
					}
				}
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Space ();

				if (lightingProfileRef != LB_LightingProfile){
					
					helper.Update_MainProfile(LB_LightingProfile);
					OnLoad ();
					EditorPrefs.SetString (EditorSceneManager.GetActiveScene ().name, AssetDatabase.GetAssetPath (LB_LightingProfile));

					if (LB_LightingProfile)
						EditorUtility.SetDirty (LB_LightingProfile);
				}

				if (postProcessingProfileRef != postProcessingProfile)
				{
					if (LB_LightingProfile)
					{
						LB_LightingProfile.postProcessingProfile = postProcessingProfile;
						EditorUtility.SetDirty (LB_LightingProfile);
					}

					UpdatePostEffects ();

				}
				



				if (helpBox)
					EditorGUILayout.HelpBox ("Which camera should has effects", MessageType.Info);

				EditorGUILayout.BeginHorizontal();
				var mainCameraRef = mainCamera;

				mainCamera = EditorGUILayout.ObjectField ("Target Camera", mainCamera, typeof(Camera), true) as Camera;
				if (GUILayout.Button ("Save", GUILayout.Width (43), GUILayout.Height (17)))
				{
					if (LB_LightingProfile)
					{
						LB_LightingProfile.mainCameraName = mainCamera.name;
						EditorUtility.SetDirty (LB_LightingProfile);
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Space ();

				if (mainCameraRef != mainCamera) 
				{
					UpdatePostEffects ();
					UpdateSettings ();

					if (LB_LightingProfile)
					{
						LB_LightingProfile.mainCameraName = mainCamera.name;
						EditorUtility.SetDirty (LB_LightingProfile);
					}
				}

				var webGL_MobileRef = webGL_Mobile;

				webGL_Mobile = EditorGUILayout.Toggle ("WebGL 2.0 Target", webGL_Mobile);

				if (webGL_MobileRef != webGL_Mobile) {
					if (LB_LightingProfile)
					{
						LB_LightingProfile.webGL_Mobile = webGL_Mobile;
						EditorUtility.SetDirty (LB_LightingProfile);
					}
				}

				EditorGUILayout.Space ();
				EditorGUILayout.Space ();

			}

			#endregion

			#region Ambient

			//-----------Ambient----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			EditorGUILayout.BeginHorizontal ();

			if(ambientState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
			var Ambient_EnabledRef = Ambient_Enabled;
			var ambientStateRef = ambientState;

			if (GUILayout.Button ("Ambient", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				ambientState = !ambientState;
			}

			if(ambientStateRef != ambientState )
			{
				if (LB_LightingProfile)
				{
					LB_LightingProfile.ambientState = ambientState;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------


			if (ambientState) {
				if (helpBox)
					EditorGUILayout.HelpBox ("Assign scene skybox material here   ", MessageType.Info);

				var skyboxRef = skyBox;

				Ambient_Enabled = EditorGUILayout.Toggle("Enabled",Ambient_Enabled);
				EditorGUILayout.Space();

				skyBox = EditorGUILayout.ObjectField ("SkyBox Material", skyBox, typeof(Material), true) as Material;

				if (skyboxRef != skyBox) {

					helper.Update_SkyBox (Ambient_Enabled,skyBox);

					if (LB_LightingProfile)
					{
						LB_LightingProfile.skyBox = skyBox;
						EditorUtility.SetDirty (LB_LightingProfile);
					}
				}


				if (helpBox)
					EditorGUILayout.HelpBox ("Set ambient lighting source as Skybox(IBL) or a simple color", MessageType.Info);

				var ambientLightRef = ambientLight;
				var ambientColorRef = ambientColor;
				var skyBoxRef = skyBox;
				var skyColorRef = skyColor;
				var equatorColorRef = equatorColor;
				var groundColorRef = groundColor;

				// choose ambient lighting mode   (color or skybox)
				ambientLight = (AmbientLight)EditorGUILayout.EnumPopup ("Ambient Source", ambientLight, GUILayout.Width (343));

				if(ambientLight == AmbientLight.Color)
					ambientColor = EditorGUILayout.ColorField ("Color", ambientColor);
				if(ambientLight == AmbientLight.Gradient)
				{
					skyColor = EditorGUILayout.ColorField ("Sky Color", skyColor);
					equatorColor = EditorGUILayout.ColorField ("Equator Color", equatorColor);
					groundColor = EditorGUILayout.ColorField ("Ground Color", groundColor);
				}
				
				if (ambientLightRef != ambientLight || ambientColorRef != ambientColor
					|| skyBoxRef != skyBox || skyColorRef != skyColor
					|| equatorColorRef != equatorColor || groundColorRef != groundColor
					|| Ambient_EnabledRef != Ambient_Enabled  )
				{
					helper.Update_Ambient (Ambient_Enabled,ambientLight, ambientColor,skyColor,equatorColor,groundColor);

					if (LB_LightingProfile)
					{
						LB_LightingProfile.ambientColor = ambientColor;
						LB_LightingProfile.ambientLight = ambientLight;
						LB_LightingProfile.skyBox = skyBox;
						LB_LightingProfile.skyColor = skyColor;
						LB_LightingProfile.equatorColor = equatorColor;
						LB_LightingProfile.groundColor = groundColor;
						LB_LightingProfile.Ambient_Enabled = Ambient_Enabled;
						EditorUtility.SetDirty (LB_LightingProfile);
					}
				}
				//----------------------------------------------------------------------
			}
			#endregion

			#region Sun Light
			//-----------Sun----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			EditorGUILayout.BeginHorizontal ();

			if(sunState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));

			var sunStateRef = sunState;

			if (GUILayout.Button ("Sun", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				sunState = !sunState;
			}

			if(sunStateRef != sunState)
			{
				if (LB_LightingProfile)
				{
					LB_LightingProfile.sunState = sunState;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------


			if (sunState) {
				if (helpBox)
					EditorGUILayout.HelpBox ("Sun /  Moon light settings", MessageType.Info);
				
				var Sun_EnabledRef = Sun_Enabled;

				Sun_Enabled = EditorGUILayout.Toggle("Enabled",Sun_Enabled);

				EditorGUILayout.Space();

				EditorGUILayout.BeginHorizontal ();
				sunLight = EditorGUILayout.ObjectField ("Sun Light", sunLight, typeof(Light), true) as Light;
				if (!sunLight){
					if (GUILayout.Button ("Find"))
						Update_Sun ();
				}
				EditorGUILayout.EndHorizontal ();
				var sunColorRef = sunColor;

				sunColor = EditorGUILayout.ColorField ("Color", sunColor);

				var sunIntensityRef = sunIntensity;
				var indirectIntensityRef = indirectIntensity;

				sunIntensity = EditorGUILayout.Slider ("Intenity", sunIntensity, 0, 4f);
				indirectIntensity = EditorGUILayout.Slider ("Indirect Intensity", indirectIntensity, 0, 4f);

				var sunFlareRef = sunFlare;

				sunFlare = EditorGUILayout.ObjectField ("Lens Flare", sunFlare, typeof(Flare), true) as Flare;

				if (Sun_EnabledRef != Sun_Enabled)
				{					
					if (LB_LightingProfile)
					{
						LB_LightingProfile.Sun_Enabled = Sun_Enabled;
						EditorUtility.SetDirty (LB_LightingProfile);
					}
				}

				if(Sun_Enabled)
				{

					if (sunColorRef != sunColor || Sun_EnabledRef != Sun_Enabled) {
						
						if (sunLight)
							sunLight.color = sunColor;
						else
							Update_Sun ();		
						if (LB_LightingProfile)
						{
							LB_LightingProfile.sunColor = sunColor;
							EditorUtility.SetDirty (LB_LightingProfile);
						}
					}

					if (sunIntensityRef != sunIntensity || indirectIntensityRef != indirectIntensity
						|| Sun_EnabledRef != Sun_Enabled) {

						if (sunLight) {
							sunLight.intensity = sunIntensity;
							sunLight.bounceIntensity = indirectIntensity;
						} else
							Update_Sun ();
						if (LB_LightingProfile) {
							LB_LightingProfile.sunIntensity = sunIntensity;
							LB_LightingProfile.indirectIntensity = indirectIntensity;
							LB_LightingProfile.Sun_Enabled = Sun_Enabled;
						}
						if (LB_LightingProfile)
						{
							LB_LightingProfile.sunState = sunState;
							EditorUtility.SetDirty (LB_LightingProfile);
						}
					}
					if (sunFlareRef != sunFlare) {
						if (sunFlare) {
							if (sunLight)
								sunLight.flare = sunFlare;
						} else {
							if (sunLight)
								sunLight.flare = null;
						}

						if (LB_LightingProfile)
						{
							LB_LightingProfile.sunFlare = sunFlare;
							LB_LightingProfile.sunState = sunState;
							EditorUtility.SetDirty (LB_LightingProfile);
						}
					}
				}
			}
			#endregion

			#region Lighting Mode


			//-----------Light Settings----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			EditorGUILayout.BeginHorizontal ();

			if(lightSettingsState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
			var lightSettingsStateRef = lightSettingsState;
			var Scene_EnabledRef = Scene_Enabled;

			if (GUILayout.Button ("Scene", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				lightSettingsState = !lightSettingsState;
			}

			if(lightSettingsStateRef != lightSettingsState)
			{
				if (LB_LightingProfile)
				{
					LB_LightingProfile.lightSettingsState = lightSettingsState;  
					LB_LightingProfile.Scene_Enabled = Scene_Enabled;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------


			if (lightSettingsState) {
				if (helpBox)
					EditorGUILayout.HelpBox ("Fully realtime without GI, Enlighten Realtime GI or Baked Progressive Lightmapper", MessageType.Info);

				var lightingModeRef = lightingMode;

				Scene_Enabled = EditorGUILayout.Toggle("Enabled",Scene_Enabled);
				EditorGUILayout.Space();

				// Choose lighting mode (realtime GI or baked GI)
				lightingMode = (LightingMode)EditorGUILayout.EnumPopup ("Lighting Mode", lightingMode, GUILayout.Width (343));

				if (lightingMode == LightingMode.Baked) {
					EditorGUILayout.Space ();

					if (helpBox)
						EditorGUILayout.HelpBox ("Baked lightmapping resolution. Higher value needs more RAM and longer bake time. Check task manager about RAM usage during bake time", MessageType.Info);

					// Baked lightmapping resolution   
					bakedResolution = EditorGUILayout.FloatField ("Baked Resolution", bakedResolution);
					LightmapEditorSettings.bakeResolution = bakedResolution;
					if (LB_LightingProfile)
					{
						LB_LightingProfile.bakedResolution = bakedResolution;
						LB_LightingProfile.Scene_Enabled = Scene_Enabled;
						EditorUtility.SetDirty (LB_LightingProfile);
					}

				}
				if (webGL_Mobile) {
					if (lightingMode == LightingMode.RealtimeGI) {
						EditorGUILayout.Space ();
						EditorGUILayout.LabelField ("Enlighten's Realtime GI is not available for WebGL platform", redStyle);
						EditorGUILayout.Space ();

					}
				}

				if (lightingModeRef != lightingMode || Scene_EnabledRef != Scene_Enabled) {
					//----------------------------------------------------------------------
					// Update Lighting Mode
					helper.Update_LightingMode (Scene_Enabled,lightingMode);
					//----------------------------------------------------------------------
					if (LB_LightingProfile)
					{
						LB_LightingProfile.lightingMode = lightingMode; 
						LB_LightingProfile.Scene_Enabled = Scene_Enabled;
						EditorUtility.SetDirty (LB_LightingProfile);
					}
				}
				#endregion

			#region Color Space
			EditorGUILayout.Space ();

			if (helpBox)
				EditorGUILayout.HelpBox ("Choose between Linear or Gamma color space , default should be Linear for my settings and next-gen platfroms   ", MessageType.Info);

			var colorSpaceRef = colorSpace;

			// Choose color space (Linear,Gamma) i have used Linear in post effect setting s
			colorSpace = (MyColorSpace)EditorGUILayout.EnumPopup ("Color Space", colorSpace, GUILayout.Width (343));

			if (colorSpaceRef != colorSpace) {
				// Color Space
					helper.Update_ColorSpace (Scene_Enabled,colorSpace);

				if (LB_LightingProfile)
				{
					LB_LightingProfile.colorSpace = colorSpace;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}
			#endregion

			#region Render Path
			EditorGUILayout.Space ();

			if (helpBox)
				EditorGUILayout.HelpBox ("Choose between Forward and Deferred rendering path for cameras. Deferred needed for Screen Spacwe Reflection effect. Forward has better performance in unity", MessageType.Info);

			var renderPathRef = renderPath;

			renderPath = (Render_Path)EditorGUILayout.EnumPopup ("Render Path", renderPath, GUILayout.Width (343));

			if (renderPathRef != renderPath) {

					helper.Update_RenderPath (Scene_Enabled,renderPath, mainCamera);

				if (LB_LightingProfile)
				{
					LB_LightingProfile.renderPath = renderPath;
					EditorUtility.SetDirty (LB_LightingProfile);
				}

			}

			#endregion

			#region Light Types
			EditorGUILayout.Space ();

			if (helpBox)
				EditorGUILayout.HelpBox ("Changing the type of all light sources (Realtime,Baked,Mixed)", MessageType.Info);

			var lightSettingsRef = lightSettings;

			// Change file lightmapping type mixed,realtime baked
			lightSettings = (LightSettings)EditorGUILayout.EnumPopup ("Lights Type", lightSettings, GUILayout.Width (343));

			//----------------------------------------------------------------------
			// Light Types
			if (lightSettingsRef != lightSettings) {

					helper.Update_LightSettings (Scene_Enabled,lightSettings);

				if (LB_LightingProfile)
				{
					LB_LightingProfile.lightSettings = lightSettings;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}
			//----------------------------------------------------------------------
			#endregion

			#region Light Shadows Settings
			EditorGUILayout.Space ();

			if (helpBox)
				EditorGUILayout.HelpBox ("Activate shadows for point and spot lights   ", MessageType.Info);

			var pshadRef = psShadow;
			// Choose hard shadows state on off for spot and point lights
			psShadow = (LightsShadow)EditorGUILayout.EnumPopup ("Shadows", psShadow, GUILayout.Width (343));

			if (pshadRef != psShadow) {

				// Shadows
					helper.Update_Shadows (Scene_Enabled,psShadow);

				//----------------------------------------------------------------------
				if (LB_LightingProfile)
				{
					LB_LightingProfile.lightsShadow = psShadow;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}
			#endregion

			#region Light Probes
			EditorGUILayout.Space ();

			if (helpBox)
				EditorGUILayout.HelpBox ("Adjust light probes settings for non-static objects, Blend mode is more optimized", MessageType.Info);

			var lightprobeModeRef = lightprobeMode;

			lightprobeMode = (LightProbeMode)EditorGUILayout.EnumPopup ("Light Probes", lightprobeMode, GUILayout.Width (343));

			if (lightprobeModeRef != lightprobeMode) {

				// Light Probes
				helper.Update_LightProbes (Scene_Enabled, lightprobeMode);

				//----------------------------------------------------------------------
				if (LB_LightingProfile)
				{
					LB_LightingProfile.lightProbesMode = lightprobeMode;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}
		}
		#endregion

			#region Buttons
			//-----------Buttons----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			EditorGUILayout.BeginHorizontal ();

			if(buildState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
			var buildStateRef = buildState;

			if (GUILayout.Button ("Build", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				buildState = !buildState;
			}

			if(buildStateRef != buildState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.buildState = buildState;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			if (buildState) {
				var automodeRef = autoMode;

				if (helpBox)
					EditorGUILayout.HelpBox ("Automatic lightmap baking", MessageType.Info);


				autoMode = EditorGUILayout.Toggle ("Auto Mode", autoMode);

				if (automodeRef != autoMode) {
					// Auto Mode
					if (autoMode)
						Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.Iterative;
					else
						Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;
					//----------------------------------------------------------------------
					if (LB_LightingProfile)
					{
						LB_LightingProfile.automaticLightmap = autoMode;
						EditorUtility.SetDirty (LB_LightingProfile);
					}
				}

				// Start bake
				if (!Lightmapping.isRunning) {

					if (helpBox)
						EditorGUILayout.HelpBox ("Bake lightmap", MessageType.Info);

					if (GUILayout.Button ("Bake")) {
						if (!Lightmapping.isRunning) {
							Lightmapping.BakeAsync ();
						}
					}

					if (helpBox)
						EditorGUILayout.HelpBox ("Clear lightmap data", MessageType.Info);

					if (GUILayout.Button ("Clear")) {
						Lightmapping.Clear ();
					}
				} else {

					if (helpBox)
						EditorGUILayout.HelpBox ("Cancel lightmap baking", MessageType.Info);

					if (GUILayout.Button ("Cancel")) {
						if (Lightmapping.isRunning) {
							Lightmapping.Cancel ();
						}
					}
				}

				if (Input.GetKey (KeyCode.F)) {
					if (Lightmapping.isRunning)
						Lightmapping.Cancel ();
				}
				if (Input.GetKey (KeyCode.LeftControl) && Input.GetKey (KeyCode.E)) {
					if (!Lightmapping.isRunning)
						Lightmapping.BakeAsync ();
				}

				if (helpBox) {
					EditorGUILayout.HelpBox ("Bake : Shift + B", MessageType.Info);
					EditorGUILayout.HelpBox ("Cancel : Shift + C", MessageType.Info);
					EditorGUILayout.HelpBox ("Clear : Shift + E", MessageType.Info);

				}
				EditorGUILayout.Space ();
				EditorGUILayout.Space ();
				EditorGUILayout.Space ();
				EditorGUILayout.Space ();

				if (helpBox)
					EditorGUILayout.HelpBox ("Open unity Lighting Settings window", MessageType.Info);

				if (GUILayout.Button ("Lighting Window")) {

					EditorApplication.ExecuteMenuItem ("Window/Lighting/Settings");
				}

				EditorGUILayout.Space ();
				EditorGUILayout.Space ();

				if (GUILayout.Button ("Add Camera Move Script")) {
						if (!mainCamera.GetComponent<LB_CameraMove> ())
						mainCamera.gameObject.AddComponent<LB_CameraMove> ();
				}
				if (GUILayout.Button ("Add RenderBox")) {
					if (!GameObject.FindObjectOfType<LB_RenderBox> ()) {
						GameObject rb = new GameObject ("RenderBox");
						rb.AddComponent<LB_RenderBox> ();
						rb.GetComponent<LB_RenderBox> ().screenShotResolution = SelectResolution._1080P;
					}
				}

				#endregion

			}
		}

			if (winMode == WindowMode.Part2)
			{

				#region Volumetric Light

				//-----------Volumetric Lighting----------------------------------------------------------------------------
				GUILayout.BeginVertical ("Box");

				var VL_EnabledRef = VL_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(vLightState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
				VL_Enabled = EditorGUILayout.Toggle("",VL_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var vLightStateRef = vLightState;

				if (GUILayout.Button ("Volumetric Lighting", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
					vLightState = !vLightState;
				}

			if(vLightStateRef != vLightState)
			{
				if (LB_LightingProfile)
				{
					LB_LightingProfile.vLightState = vLightState;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}

				EditorGUILayout.EndHorizontal ();

				GUILayout.EndVertical ();
				//---------------------------------------------------------------------------------------


			if(vLightState)
			{
					if (helpBox)
						EditorGUILayout.HelpBox ("Activate Volumetric Lights For All Light Sources", MessageType.Info);
			}
					var vLightRef = vLight;
					var vLightLevelRef = vLightLevel;

			if(vLightState)
			{
					// Activate or deactivate volumetric lighting for all light sources
					vLight = (VolumetricLightType)EditorGUILayout.EnumPopup ("Volumetric Light", vLight, GUILayout.Width (343));


					vLightLevel = (VLightLevel)EditorGUILayout.EnumPopup ("Intensity", vLightLevel, GUILayout.Width (343));
			}
					if (vLightRef != vLight || vLightLevelRef != vLightLevel || VL_EnabledRef != VL_Enabled) {

						// Volumetric Light
						helper.Update_VolumetricLight (mainCamera, VL_Enabled, vLight, vLightLevel);
						//----------------------------------------------------------------------
				if (LB_LightingProfile)
				{
							LB_LightingProfile.vLight = vLight;
							LB_LightingProfile.vLightLevel = vLightLevel;
							LB_LightingProfile.VL_Enabled = VL_Enabled;
							EditorUtility.SetDirty (LB_LightingProfile);
				}
			}

			if (webGL_Mobile) 
			{
				if (VL_Enabled) 
					{
						if(vLightState)
						{
								EditorGUILayout.Space ();
								EditorGUILayout.LabelField ("Volumetric Light is not supported on WebGL platform", redStyle);
								EditorGUILayout.Space ();
						}
					}
				}
				
				#endregion

				#region Sun Shaft

			//-----------Sun Shaft----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			var SunShaft_EnabledRef = SunShaft_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(sunShaftState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
			SunShaft_Enabled = EditorGUILayout.Toggle("",SunShaft_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var sunShaftStateRef = sunShaftState;

			if (GUILayout.Button ("Sun Shaft", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				sunShaftState = !sunShaftState;
			}

			if(sunShaftStateRef != sunShaftState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.sunShaftState = sunShaftState;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			if(sunShaftState)
			{
				if (helpBox)
					EditorGUILayout.HelpBox ("Activate Sun Shaft for sun", MessageType.Info);
			}
				var shaftDistanceRef = shaftDistance;
				var shaftBlurRef = shaftBlur;
				var shaftColorRef = shaftColor;
				var shaftQualityRef = shaftQuality;

				// Activate Sun Shaft for sun
				///	shaftMode = (ShaftMode)EditorGUILayout.EnumPopup("Sun Shaft",shaftMode,GUILayout.Width(343));


				if(sunShaftState)
				{
					shaftQuality = (UnityStandardAssets.ImageEffects.SunShafts.SunShaftsResolution)EditorGUILayout.EnumPopup ("Quality", shaftQuality, GUILayout.Width (343));
					shaftDistance = 1.0f - EditorGUILayout.Slider ("Distance falloff", 1.0f - shaftDistance, 0.1f, 1.0f);
					shaftBlur = EditorGUILayout.Slider ("Blur", shaftBlur, 1f, 10f);
					shaftColor = (Color)EditorGUILayout.ColorField ("Color", shaftColor);
				}

			if (SunShaft_EnabledRef != SunShaft_Enabled || shaftDistanceRef != shaftDistance
				  || shaftBlurRef != shaftBlur || shaftColorRef != shaftColor || shaftQualityRef != shaftQuality) {

					// Sun Shaft
				if(sunLight)					
					helper.Update_SunShaft (mainCamera, SunShaft_Enabled, shaftQuality, shaftDistance, shaftBlur, shaftColor, sunLight.transform);

				//----------------------------------------------------------------------
				if (LB_LightingProfile)
				{
						LB_LightingProfile.SunShaft_Enabled = SunShaft_Enabled;
						LB_LightingProfile.shaftQuality = shaftQuality;
						LB_LightingProfile.shaftDistance = shaftDistance;
						LB_LightingProfile.shaftBlur = shaftBlur;
						LB_LightingProfile.shaftColor = shaftColor;
						EditorUtility.SetDirty (LB_LightingProfile);
				}
				}

				#endregion

				#region Global Fog


			//-----------Global Fog----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			var Fog_EnabledRef = Fog_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(fogState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
			Fog_Enabled = EditorGUILayout.Toggle("",Fog_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var fogStateRef = fogState;

			if (GUILayout.Button ("Global Fog", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				fogState = !fogState;
			}

			if(fogStateRef != fogState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.fogState = fogState;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}
			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			if(fogState)
			{
				if (helpBox)
					EditorGUILayout.HelpBox ("Activate Global Fog for the scene. Combined with unity Lighting Window Fog parameters", MessageType.Info);
			}

				var vfogRef = vFog;

			if(fogState)
			{
				vFog = (CustomFog)EditorGUILayout.EnumPopup ("Global Fog", vFog, GUILayout.Width (343));
			}

				if (Fog_EnabledRef != Fog_Enabled)
			{
					helper.Update_GlobalFog (mainCamera, Fog_Enabled, vFog, fDistance, fHeight, fheightDensity, fColor, fogIntensity);
				if (LB_LightingProfile)
					LB_LightingProfile.Fog_Enabled = Fog_Enabled;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}
			

				//-----Distance--------------------------------------------------------------------
				if (vFog == CustomFog.Distance) {
					float fDistanceRef = fDistance;
					Color fColorRef = fColor;
				float fogIntensityRef = fogIntensity;

				if(fogState)
				{
					fDistance = (float)EditorGUILayout.FloatField ("Start Distance", fDistance);
					fogIntensity = (float)EditorGUILayout.Slider ("Density", fogIntensity, 0, 30f);
					fColor = (Color)EditorGUILayout.ColorField ("Color", fColor);
				}
				if (fDistanceRef != fDistance || fColorRef != fColor || fogIntensityRef != fogIntensity) {
						helper.Update_GlobalFog (mainCamera, Fog_Enabled, vFog, fDistance, fHeight, fheightDensity, fColor, fogIntensity);
					if (LB_LightingProfile)
					{
							LB_LightingProfile.fogDistance = fDistance;
							LB_LightingProfile.fogColor = fColor;
							LB_LightingProfile.fogIntensity = fogIntensity;
							EditorUtility.SetDirty (LB_LightingProfile);
					}
				}
				}
				//-----Global--------------------------------------------------------------------
				if (vFog == CustomFog.Global) {
					Color fColorRef = fColor;
				float fogIntensityRef = fogIntensity;

				if(fogState)
				{
					fogIntensity = (float)EditorGUILayout.Slider ("Density", fogIntensity, 0, 40f);
					fColor = (Color)EditorGUILayout.ColorField ("Color", fColor);
				}
				if (fColorRef != fColor || fogIntensityRef != fogIntensity) {
					helper.Update_GlobalFog (mainCamera, Fog_Enabled, vFog, fDistance, fHeight, fheightDensity, fColor, fogIntensity);
						if (LB_LightingProfile)
					{
							LB_LightingProfile.fogColor = fColor;
						LB_LightingProfile.fogIntensity = fogIntensity;
						LB_LightingProfile.fogState = fogState;
							EditorUtility.SetDirty (LB_LightingProfile);
					}
					}
				}
				//-----Height--------------------------------------------------------------------
				if (vFog == CustomFog.Height) {
					float fDistanceRef = fDistance;
					float fHeightRef = fHeight;
					Color fColorRef = fColor;
					float fheightDensityRef = fheightDensity;
				if(fogState)
				{
					fDistance = (float)EditorGUILayout.FloatField ("Start Distance", fDistance);
					fHeight = (float)EditorGUILayout.FloatField ("Height", fHeight);
					fheightDensity = (float)EditorGUILayout.Slider ("Height Density", fheightDensity, 0, 1f);
					fColor = (Color)EditorGUILayout.ColorField ("Color", fColor);
				}
					if (fHeightRef != fHeight || fheightDensityRef != fheightDensity || fColorRef != fColor || fDistanceRef != fDistance) {
					helper.Update_GlobalFog (mainCamera, Fog_Enabled, vFog, fDistance, fHeight, fheightDensity, fColor, fogIntensity);
						if (LB_LightingProfile)
					{
							LB_LightingProfile.fogHeight = fHeight;
						LB_LightingProfile.fogHeightIntensity = fheightDensity;
							LB_LightingProfile.fogColor = fColor;
						LB_LightingProfile.fogState = fogState;
							LB_LightingProfile.fogDistance = fDistance;
							EditorUtility.SetDirty (LB_LightingProfile);
					}
					}
				}
				//-----Global Fog Type--------------------------------------------------------------------
				if (vfogRef != vFog) {

					helper.Update_GlobalFog (mainCamera, Fog_Enabled, vFog, fDistance, fHeight, fheightDensity, fColor, fogIntensity);

					//-------------------------------------------------------------------
					if (LB_LightingProfile)
				{
						LB_LightingProfile.fogMode = vFog;
					LB_LightingProfile.fogState = fogState;
						EditorUtility.SetDirty (LB_LightingProfile);
				}
				}

				#endregion

				#region Depth of Field Legacy    

			//-----------Depth of Field----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			var DOF_EnabledRef = DOF_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(dofState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
			DOF_Enabled = EditorGUILayout.Toggle("",DOF_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var dofStateRef = dofState;

			if (GUILayout.Button ("Depth of Field", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				dofState = !dofState;
			}

			if(dofStateRef != dofState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.dofState = dofState;
				if (LB_LightingProfile)					
					EditorUtility.SetDirty (LB_LightingProfile);
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			if(dofState)
			{
				if (helpBox)
					EditorGUILayout.HelpBox ("Activate Depth Of Field for the camera", MessageType.Info);
			}
				var dofRangeRef = dofRange;
				var dofBlurRef = dofBlur;
				var falloffRef = falloff;
				var dofQualityRef = dofQuality;
				var visualizeRef = visualize;

				// Auto focus
				var afRangeRef = afRange;
				var afBlurRef = afBlur;
				var afSpeedRef = afSpeed;
				var afUpdateRef = afUpdate;
				var afRayLengthRef = afRayLength;
				var afLayerRef = afLayer;
				var AutoFocus_EnabledRef = AutoFocus_Enabled;

			if(dofState)
			{
					dofQuality = (DOFQuality)EditorGUILayout.EnumPopup ("Quality", dofQuality, GUILayout.Width (343));
					dofRange = (float)EditorGUILayout.Slider ("Range", dofRange, 0, 100f);
					dofBlur = (float)EditorGUILayout.Slider ("Blur", dofBlur, 0, 100f);
					falloff = (float)EditorGUILayout.Slider ("Falloff", falloff, 0, 100f);
					visualize = (bool)EditorGUILayout.Toggle ("Visualize", visualize);
			}


			if(DOF_Enabled)
				{

				//-----------Auto Focus----------------------------------------------------------------------------
				GUILayout.BeginVertical ("Box");

				EditorGUILayout.BeginHorizontal ();

				if(autoFocusState)
					GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
				else
					GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
				
				AutoFocus_Enabled = EditorGUILayout.Toggle("",AutoFocus_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

				var autoFocusStateRef = autoFocusState;

				if (GUILayout.Button ("Auto Focus", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
					autoFocusState = !autoFocusState;
				}

				if(autoFocusStateRef != autoFocusState)
				{
					if (LB_LightingProfile)
					{
						LB_LightingProfile.autoFocusState = autoFocusState;
						EditorUtility.SetDirty (LB_LightingProfile);
					}
				}

				EditorGUILayout.EndHorizontal ();

				GUILayout.EndVertical ();
				//---------------------------------------------------------------------------------------

					if(autoFocusState)
					{
						ScriptableObject targetL = this;
						SerializedObject soL = new SerializedObject (targetL);
						SerializedProperty stringsProperty = soL.FindProperty ("afLayer");
						EditorGUILayout.PropertyField (stringsProperty, true); // True means show children
						soL.ApplyModifiedProperties (); // Remember to apply modified properties


						afRange = (float)EditorGUILayout.Slider ("Max Range", afRange, 0, 100f);
						afBlur = (float)EditorGUILayout.Slider ("Max Blur", afBlur, 0, 100f);
						afSpeed = (float)EditorGUILayout.Slider ("Speed", afSpeed, 0, 100f);
						afUpdate = (float)EditorGUILayout.Slider ("Raycst Update", afUpdate, 0, 1f);
						afRayLength = (float)EditorGUILayout.Slider ("Ray Length", afRayLength, 0, 1000f);
					}
				}
					
					if (afLayerRef != afLayer || afRangeRef != afRange || afBlurRef != afBlur
					    || afSpeedRef != afSpeed || afUpdateRef != afUpdate || afRayLengthRef != afRayLength
						|| AutoFocus_EnabledRef != AutoFocus_Enabled) {

				helper.Update_AutoFocus (mainCamera, AutoFocus_Enabled, DOF_Enabled,afLayer, afRange, afBlur, afSpeed, afUpdate, afRayLength);

						//----------------------------------------------------------------------
				if (LB_LightingProfile)
				{
							LB_LightingProfile.afLayer = afLayer;
							LB_LightingProfile.afRange = afRange;
							LB_LightingProfile.afBlur = afBlur;
							LB_LightingProfile.afSpeed = afSpeed;
							LB_LightingProfile.afUpdate = afUpdate;
							LB_LightingProfile.afRayLength = afRayLength;
							LB_LightingProfile.AutoFocus_Enabled = AutoFocus_Enabled;
							EditorUtility.SetDirty (LB_LightingProfile);
				}
			}


				if (DOF_EnabledRef != DOF_Enabled || dofRangeRef != dofRange || dofBlurRef != dofBlur
				  || dofQualityRef != dofQuality || falloffRef != falloff
				  || visualizeRef != visualize) {

					helper.Update_DOF (mainCamera, DOF_Enabled, dofQuality, dofBlur, dofRange, falloff, visualize);
				helper.Update_AutoFocus (mainCamera, AutoFocus_Enabled, DOF_Enabled,afLayer, afRange, afBlur, afSpeed, afUpdate, afRayLength);

					//----------------------------------------------------------------------
					if (LB_LightingProfile)
				{
						LB_LightingProfile.DOF_Enabled = DOF_Enabled;
						LB_LightingProfile.dofRange = dofRange;
						LB_LightingProfile.dofQuality = dofQuality;
						LB_LightingProfile.dofBlur = dofBlur;
						LB_LightingProfile.falloff = falloff;
						LB_LightingProfile.visualize = visualize;
						EditorUtility.SetDirty (LB_LightingProfile);
				}
				}

				#endregion

				#region Bloom

				//-----------Bloom----------------------------------------------------------------------------
				GUILayout.BeginVertical ("Box");

				var Bloom_EnabledRef = Bloom_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(bloomState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
				Bloom_Enabled = EditorGUILayout.Toggle("",Bloom_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var bloomStateRef = bloomState;

				if (GUILayout.Button ("Bloom", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
					bloomState = !bloomState;
				}

			if(bloomStateRef != bloomState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.bloomState = bloomState;			
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

				EditorGUILayout.EndHorizontal ();

				GUILayout.EndVertical ();
				//---------------------------------------------------------------------------------------
				if(bloomState)
				{
					if (helpBox)
						EditorGUILayout.HelpBox ("Activate Bloom for the camera", MessageType.Info);
				}
				var bIntensityRef = bIntensity;
				var bThreshouldRef = bThreshould;
				var bColorRef = bColor;
			var dirtTextureRef = dirtTexture;
			var dirtIntensityRef = dirtIntensity;
			var mobileOptimizedBloomRef = mobileOptimizedBloom;
			var bRotationRef = bRotation;
				if (bloomState)
				{
					bIntensity = (float)EditorGUILayout.Slider ("Intensity", bIntensity, 0, 3f);
					bThreshould = (float)EditorGUILayout.Slider ("Threshould", bThreshould, 0, 2f);
				    bRotation = (float)EditorGUILayout.Slider ("Rotation", bRotation, -1, 0.7f);

					bColor = (Color)EditorGUILayout.ColorField ("Color", bColor);
					mobileOptimizedBloom = EditorGUILayout.Toggle("Mobile Optimized",mobileOptimizedBloom);
					EditorGUILayout.Space();

				dirtTexture = EditorGUILayout.ObjectField ("Dirt Texture", dirtTexture, typeof(Texture2D), true) as Texture2D;
				dirtIntensity = (float)EditorGUILayout.Slider ("Dirt Intensity", dirtIntensity, 0, 10f);
				}

			if (dirtTextureRef != dirtTexture || dirtIntensityRef != dirtIntensity || Bloom_EnabledRef != Bloom_Enabled || bIntensityRef != bIntensity || bColorRef != bColor || bThreshouldRef != bThreshould || bIntensityRef != bIntensity
				|| mobileOptimizedBloomRef != mobileOptimizedBloom  || bRotationRef != bRotation) {


				helper.Update_Bloom(Bloom_Enabled,bIntensity,bThreshould,bColor,dirtTexture,dirtIntensity,mobileOptimizedBloom,bRotation);


					//----------------------------------------------------------------------

				if (LB_LightingProfile)
				{
						LB_LightingProfile.Bloom_Enabled = Bloom_Enabled;
					LB_LightingProfile.bIntensity = bIntensity;
					LB_LightingProfile.bRotation = bRotation;
					LB_LightingProfile.bThreshould = bThreshould;
						LB_LightingProfile.mobileOptimizedBloom = mobileOptimizedBloom;						
						LB_LightingProfile.bColor = bColor;		
						LB_LightingProfile.dirtTexture = dirtTexture;		
						LB_LightingProfile.dirtIntensity = dirtIntensity;	
						EditorUtility.SetDirty (LB_LightingProfile);
				}
				}

				#endregion

			}

			if (winMode == WindowMode.Part3) {

				#region Color Grading

			//-----------Color Grading----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			EditorGUILayout.BeginHorizontal ();

			if(colorState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			

			var colorStateRef = colorState;

			if (GUILayout.Button ("Color Grading", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				colorState = !colorState;
			}

			if(colorStateRef != colorState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.colorState = colorState;				
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			if(colorState)
			{
				if (helpBox)
					EditorGUILayout.HelpBox ("Color grading settings", MessageType.Info);
			}
				var colorModeRef = colorMode;
				var exposureIntensityRef = exposureIntensity;
				var contrastValueRef = contrastValue;
				var tempRef = temp;
				var eyeKeyValueRef = eyeKeyValue;
				var gammaRef = gamma;
				var colorGammaRef = colorGamma;
				var colorLiftRef = colorLift;
				var saturationRef = saturation;
				var lutRef = lut;

			if(colorState)
			{
				if (!webGL_Mobile)
					colorMode = (ColorMode)EditorGUILayout.EnumPopup ("Mode", colorMode, GUILayout.Width (343));
				
				if(colorMode == ColorMode.LUT)
				{
					lut = EditorGUILayout.ObjectField ("LUT Texture   ", lut, typeof(Texture), true) as Texture;


				}
				else
				{
					exposureIntensity = (float)EditorGUILayout.Slider ("Exposure", exposureIntensity, 0, 3f);
					contrastValue = (float)EditorGUILayout.Slider ("Contrast", contrastValue, 0, 1f);
					saturation = (float)EditorGUILayout.Slider ("Saturation", saturation, -1f, 0.3f);
					temp = (float)EditorGUILayout.Slider ("Temperature", temp, 0, 100f);


					if (!webGL_Mobile)
					{
						EditorGUILayout.Space();
						eyeKeyValue = (float)EditorGUILayout.Slider ("Auto Exposure", eyeKeyValue, 0, 1f);
					}

					EditorGUILayout.Space();

					colorGamma = (Color)EditorGUILayout.ColorField ("Gamma Color", colorGamma);
					colorLift = (Color)EditorGUILayout.ColorField ("Lift Color", colorLift);
					EditorGUILayout.Space();

					gamma = (float)EditorGUILayout.Slider ("Gamma", gamma, -1f, 1f);
				}
			}

				if (exposureIntensityRef != exposureIntensity || contrastValueRef != contrastValue || tempRef != temp || eyeKeyValueRef != eyeKeyValue
				  || colorModeRef != colorMode || gammaRef != gamma || colorGammaRef != colorGamma || colorLiftRef != colorLift || saturationRef != saturation
				|| lutRef != lut) {


				helper.Update_ColorGrading (colorMode, exposureIntensity, contrastValue, temp, eyeKeyValue, saturation, colorGamma, colorLift, gamma,lut);

					//----------------------------------------------------------------------
				if (LB_LightingProfile)
				{
						LB_LightingProfile.exposureIntensity = exposureIntensity;
						LB_LightingProfile.lut = lut;
						LB_LightingProfile.contrastValue = contrastValue;
						LB_LightingProfile.temp = temp;
						LB_LightingProfile.eyeKeyValue = eyeKeyValue;
						LB_LightingProfile.colorMode = colorMode;
						LB_LightingProfile.colorLift = colorLift;
						LB_LightingProfile.colorGamma = colorGamma;
						LB_LightingProfile.gamma = gamma;
						LB_LightingProfile.saturation = saturation;
						EditorUtility.SetDirty (LB_LightingProfile);
				}
			}

				#endregion

				#region Foliage


			//-----------Foliage----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");


			EditorGUILayout.BeginHorizontal ();

			if(foliageState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			

			var foliageStateRef = foliageState;

			if (GUILayout.Button ("Foliage", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				foliageState = !foliageState;
			}

			if(foliageStateRef != foliageState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.foliageState = foliageState;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			if(foliageState)
			{
				if (helpBox)
					EditorGUILayout.HelpBox ("Activate foliage shaders + wind settings", MessageType.Info);
			}

			var translucencyRef = translucency;
			var ambientRef = ambient;
			var shadowsRef = shadows;
			var tranColorRef = tranColor;
			var windSpeedRef = windSpeed;
			var windScaleRef = windScale;
			var matTypeRef = matType;
			var CustomShaderRef = CustomShader;

			if(foliageState)
			{
				GUILayout.Label ("Translucency", new GUILayoutOption[]{ GUILayout.Width (300), GUILayout.Height (20) });
				GUILayout.Box ("", new GUILayoutOption[]{ GUILayout.Width (300), GUILayout.Height (1) });

				tranColor = (Color)EditorGUILayout.ColorField ("Color", tranColor);
				translucency = (float)EditorGUILayout.Slider ("Intensity", translucency, 0, 1f);
				ambient = (float)EditorGUILayout.Slider ("Ambient", ambient, 0, 1f);
				shadows = (float)EditorGUILayout.Slider ("Shadow", shadows, 0, 1f);


				GUILayout.Label ("Wind", new GUILayoutOption[]{ GUILayout.Width (300), GUILayout.Height (20) });
				GUILayout.Box ("", new GUILayoutOption[]{ GUILayout.Width (300), GUILayout.Height (1) });

				EditorGUILayout.LabelField("Customize wind properties from each material");

				windSpeed = (float)EditorGUILayout.Slider ("Speed", windSpeed, 0, 10f);
				windScale = (float)EditorGUILayout.Slider ("Scale", windScale, 0, 100f);

				EditorGUILayout.Space ();
				EditorGUILayout.Space ();

				EditorGUILayout.Space ();
				GUILayout.Label ("Material Converter", new GUILayoutOption[]{ GUILayout.Width (300), GUILayout.Height (20) });
				GUILayout.Box ("", new GUILayoutOption[]{ GUILayout.Width (300), GUILayout.Height (1) });

				EditorGUILayout.LabelField("Speed Tree materials will be converted");

				CustomShader = EditorGUILayout.TextField ("Custom Material", CustomShader, GUILayout.Width (343));
				matType = (MatType)EditorGUILayout.EnumPopup ("Convert to", matType, GUILayout.Width (343));

				EditorGUILayout.Space ();

				if (GUILayout.Button ("Batch Convert"))
					helper.Update_ConvertMaterials (matType,CustomShader);

				EditorGUILayout.Space ();

			}
			if (translucencyRef != translucency || ambientRef != ambient
				|| shadowsRef != shadows || tranColorRef != tranColor || windSpeedRef != windSpeed || windScaleRef != windScale
				|| matTypeRef != matType || CustomShaderRef != CustomShader) {

				helper.Update_Foliage (translucency, ambient, shadows, windSpeed, windScale, tranColor);

				//----------------------------------------------------------------------
				if (LB_LightingProfile)
				{
					LB_LightingProfile.matType = matType;
					LB_LightingProfile.translucency = translucency;
					LB_LightingProfile.ambient = ambient;
					LB_LightingProfile.shadows = shadows;
					LB_LightingProfile.tranColor = tranColor;
					LB_LightingProfile.windSpeed = windSpeed;
					LB_LightingProfile.windScale = windScale;
					LB_LightingProfile.CustomShader = CustomShader;
					EditorUtility.SetDirty (LB_LightingProfile);
				}

			}

			#endregion

				#region Snow


			//-----------Snow----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");


			EditorGUILayout.BeginHorizontal ();

			if(snowState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			

			var snowStateRef = snowState;

			if (GUILayout.Button ("Snow", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				snowState = !snowState;
			}

			if(snowStateRef != snowState)
			{
				if (LB_LightingProfile)
				{
					LB_LightingProfile.snowState = snowState;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			if(snowState)
			{
				if (helpBox)
					EditorGUILayout.HelpBox ("Activate Snow shaders + Snow settings", MessageType.Info);
			}

			var snowAlbedoRef = snowAlbedo;
			var snowNormalRef = snowNormal;
			var snowIntensityRef = snowIntensity;

			if(snowState)
			{
				snowAlbedo = EditorGUILayout.ObjectField ("Snow Albedo", snowAlbedo, typeof(Texture2D), true) as Texture2D;
				snowNormal = EditorGUILayout.ObjectField ("Snow Normal", snowNormal, typeof(Texture2D), true) as Texture2D;
				snowIntensity = (float)EditorGUILayout.Slider ("Intensity", snowIntensity, 0, 3f);

				EditorGUILayout.Space ();
				GUILayout.Label ("Material Converter", new GUILayoutOption[]{ GUILayout.Width (300), GUILayout.Height (20) });
				GUILayout.Box ("", new GUILayoutOption[]{ GUILayout.Width (300), GUILayout.Height (1) });

				EditorGUILayout.LabelField("Standard materials will be converted");

				customShaderSnow = EditorGUILayout.TextField ("Custom Shader", customShaderSnow, GUILayout.Width (343));

				EditorGUILayout.Space ();

				if (GUILayout.Button ("Batch Convert"))
					helper.Update_ConvertSnowMaterials (customShaderSnow);

				EditorGUILayout.Space ();

			}
			if (snowAlbedoRef != snowAlbedo || snowNormalRef != snowNormal
				|| snowIntensityRef != snowIntensity) {

				helper.Update_Snow (snowAlbedo,snowNormal,snowIntensity);

				//----------------------------------------------------------------------
				if (LB_LightingProfile)
				{
					LB_LightingProfile.snowAlbedo = snowAlbedo;
					LB_LightingProfile.snowNormal = snowNormal;
					LB_LightingProfile.snowIntensity = snowIntensity;
					LB_LightingProfile.customShaderSnow = customShaderSnow;
					EditorUtility.SetDirty (LB_LightingProfile);
				}

			}
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();
			EditorGUILayout.Space ();


			#endregion

			}

			if (winMode == WindowMode.Finish) {

				#region Anti Aliasing

			//-----------Anti Aliasing----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			var AA_EnabledRef = AA_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(aaState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
			AA_Enabled = EditorGUILayout.Toggle("",AA_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var aaStateRef = aaState;

			if (GUILayout.Button ("Anti Aliasing", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				aaState = !aaState;
			}

			if(aaStateRef != aaState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.aaState = aaState;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			var aaModeRef = aaMode;

			if(aaState)
			{
				if (helpBox)
					EditorGUILayout.HelpBox ("Activate Antialiasing for the camera", MessageType.Info);

				aaMode = (AAMode)EditorGUILayout.EnumPopup ("Anti Aliasing", aaMode, GUILayout.Width (343));
			}
			if (aaModeRef != aaMode  || AA_EnabledRef != AA_Enabled) {

					helper.Update_AA (mainCamera, aaMode, AA_Enabled);

					//----------------------------------------------------------------------
				if (LB_LightingProfile)
				{
					LB_LightingProfile.aaMode = aaMode;
					LB_LightingProfile.AA_Enabled = AA_Enabled;
				}

				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);


				}

				#endregion

				#region AO

			//-----------Ambient Occlusion----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			var AO_EnabledRef = AO_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(aoState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
			AO_Enabled = EditorGUILayout.Toggle("",AO_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var aoStateRef = aoState;

			if (GUILayout.Button ("Ambient Occlusion", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				aoState = !aoState;
			}

			if(aoStateRef != aoState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.aoState = aoState;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			if(aoState)
			{
				if (helpBox)
					EditorGUILayout.HelpBox ("Activate AO for the camera", MessageType.Info);
			}
				var aoIntensityRef = aoIntensity;
				var ambientOnlyRef = ambientOnly;
				var aoTypeRef = aoType;
				var aoRadiusRef = aoRadius;
				var aoColorRef = aoColor;
				var aoQualityRef = aoQuality;


		

				if(aoState)
				{
					aoType = (AOType)EditorGUILayout.EnumPopup ("Type", aoType, GUILayout.Width (343));
				}
					if (aoType == AOType.Modern)
				{
					if(aoState)
					{
						aoIntensity = (float)EditorGUILayout.Slider ("Intensity", aoIntensity, 0, 2f);
						aoColor	= (Color)EditorGUILayout.ColorField ("Color", aoColor);
						ambientOnly = (bool)EditorGUILayout.Toggle ("Ambient Only", ambientOnly);
					}
					}
					if (aoType == AOType.Classic) {
					if(aoState)
					{
						aoRadius = (float)EditorGUILayout.Slider ("Radius", aoRadius, 0, 4.3f);
						aoIntensity = (float)EditorGUILayout.Slider ("Intensity", aoIntensity, 0, 4f);
						aoColor	= (Color)EditorGUILayout.ColorField ("Color", aoColor);
						aoQuality = (AmbientOcclusionQuality)EditorGUILayout.EnumPopup ("Quality", aoQuality, GUILayout.Width (343));
						ambientOnly = (bool)EditorGUILayout.Toggle ("Ambient Only", ambientOnly);
					}
					}


				if (AO_EnabledRef != AO_Enabled || aoIntensityRef != aoIntensity || ambientOnlyRef != ambientOnly
				  || aoTypeRef != aoType || aoRadiusRef != aoRadius || aoColorRef != aoColor || aoQualityRef != aoQuality) {

					if (AO_Enabled)
						helper.Update_AO (mainCamera, true, aoType, aoRadius, aoIntensity, ambientOnly, aoColor, aoQuality);
					if (!AO_Enabled)
						helper.Update_AO (mainCamera, false, aoType, aoRadius, aoIntensity, ambientOnly, aoColor, aoQuality);

					//----------------------------------------------------------------------
				if (LB_LightingProfile)
				{
						LB_LightingProfile.AO_Enabled = AO_Enabled;
						LB_LightingProfile.aoIntensity = aoIntensity;
						LB_LightingProfile.ambientOnly = ambientOnly;
						LB_LightingProfile.aoColor = aoColor;
						LB_LightingProfile.aoRadius = aoRadius;
						LB_LightingProfile.aoType = aoType;
						LB_LightingProfile.aoQuality = aoQuality;
						EditorUtility.SetDirty (LB_LightingProfile);
				}
			}

				#endregion

				#region Vignette


				//-----------Vignette----------------------------------------------------------------------------
				GUILayout.BeginVertical ("Box");

				var Vignette_EnabledRef = Vignette_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(vignetteState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
				Vignette_Enabled = EditorGUILayout.Toggle("",Vignette_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var vignetteStateRef = vignetteState;

			if (GUILayout.Button ("Vignette", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
					vignetteState = !vignetteState;
				}

			if(vignetteStateRef != vignetteState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.vignetteState = vignetteState;	
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

				EditorGUILayout.EndHorizontal ();

				GUILayout.EndVertical ();
				//---------------------------------------------------------------------------------------

				if(vignetteState)
				{
					if (helpBox)
						EditorGUILayout.HelpBox ("Activate Vignette effect for your camera", MessageType.Info);				
				}

				var vignetteIntensityRef = vignetteIntensity;

				if(vignetteState)
					vignetteIntensity = EditorGUILayout.Slider("Intensity",vignetteIntensity,0,0.3f);


				if(Vignette_EnabledRef != Vignette_Enabled || vignetteIntensityRef != vignetteIntensity)
				{
					helper.Update_Vignette(Vignette_Enabled,vignetteIntensity);
				}

				if (LB_LightingProfile)
			{
					LB_LightingProfile.Vignette_Enabled = Vignette_Enabled;
					LB_LightingProfile.vignetteIntensity = vignetteIntensity;
					EditorUtility.SetDirty (LB_LightingProfile);
			}

				#endregion

				#region Motion Blur


				//-----------Motion Blur----------------------------------------------------------------------------
				GUILayout.BeginVertical ("Box");

				var MotionBlur_EnabledRef = MotionBlur_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(motionBlurState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
				MotionBlur_Enabled = EditorGUILayout.Toggle("",MotionBlur_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var motionBlurStateRef = motionBlurState;

			if (GUILayout.Button ("Motion Blur", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
					motionBlurState = !motionBlurState;
				}

			if(motionBlurStateRef != motionBlurState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.motionBlurState = motionBlurState;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

				EditorGUILayout.EndHorizontal ();

				GUILayout.EndVertical ();
				//---------------------------------------------------------------------------------------

				if(motionBlurState)
				{
					if (helpBox)
						EditorGUILayout.HelpBox ("Activate Motion Blur effect for your camera", MessageType.Info);				
				}


				if(MotionBlur_EnabledRef != MotionBlur_Enabled)
				{
					helper.Update_MotionBlur(MotionBlur_Enabled);
				}

				if (LB_LightingProfile)
					LB_LightingProfile.MotionBlur_Enabled = MotionBlur_Enabled;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);

				#endregion

				#region Chromattic Aberration


				//-----------Chromattic Aberration----------------------------------------------------------------------------
				GUILayout.BeginVertical ("Box");

				var Chromattic_EnabledRef = Chromattic_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(chromatticState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
				Chromattic_Enabled = EditorGUILayout.Toggle("",Chromattic_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var chromatticStateRef = chromatticState;

			if (GUILayout.Button ("Chromattic Aberration", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
					chromatticState = !chromatticState;
				}

			if(chromatticStateRef != chromatticState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.chromatticState = chromatticState;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

				EditorGUILayout.EndHorizontal ();

				GUILayout.EndVertical ();
				//---------------------------------------------------------------------------------------

				if(chromatticState)
				{
					if (helpBox)
						EditorGUILayout.HelpBox ("Activate Chromattic Aberration effect for your camera", MessageType.Info);				
				}

				var CA_IntensityRef = CA_Intensity;
				var mobileOptimizedChromatticRef = mobileOptimizedChromattic;

			if(chromatticState)
			{
				CA_Intensity = EditorGUILayout.Slider("Intensity", CA_Intensity, 0,1f ) ;
				mobileOptimizedChromattic = EditorGUILayout.Toggle("Mobile Optimized",mobileOptimizedChromattic);
			}

				if(Chromattic_EnabledRef != Chromattic_Enabled || CA_IntensityRef != CA_Intensity 
				|| mobileOptimizedChromatticRef != mobileOptimizedChromattic)
				{
				helper.Update_ChromaticAberration(Chromattic_Enabled,CA_Intensity,mobileOptimizedChromattic);
				}

			if (LB_LightingProfile)
			{
				LB_LightingProfile.Chromattic_Enabled = Chromattic_Enabled;
				LB_LightingProfile.CA_Intensity = CA_Intensity;
				LB_LightingProfile.mobileOptimizedChromattic = mobileOptimizedChromattic;
				EditorUtility.SetDirty (LB_LightingProfile);
			}

				#endregion

				#region Screen Space Reflections


				//-----------Screen Space Reflections----------------------------------------------------------------------------
				GUILayout.BeginVertical ("Box");

				var SSR_EnabledRef = SSR_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(ssrState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			
				SSR_Enabled = EditorGUILayout.Toggle("",SSR_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var ssrStateRef = ssrState;

			if (GUILayout.Button ("Screen Space Reflections", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
					ssrState = !ssrState ;
				}

			if(ssrStateRef != ssrState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.ssrState = ssrState;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

				EditorGUILayout.EndHorizontal ();

				GUILayout.EndVertical ();
				//---------------------------------------------------------------------------------------

				if(ssrState )
				{
					if (helpBox)
						EditorGUILayout.HelpBox ("Activate Screen Space Reflections effect for your camera", MessageType.Info);				
				}

			var ssrQualityRef = ssrQuality;
			var ssrAttenRef = ssrAtten;
			var ssrFadeRef = ssrFade;

			if(ssrState)
			{
				ssrQuality = (ScreenSpaceReflectionPreset)EditorGUILayout.EnumPopup ("Quality", ssrQuality, GUILayout.Width (343));
				ssrAtten  = EditorGUILayout.Slider("Attention",ssrAtten,0,1);
				ssrFade  = EditorGUILayout.Slider("Fade Distance",ssrFade,0,1);
			}

			if(SSR_EnabledRef != SSR_Enabled || ssrQualityRef != ssrQuality || ssrAttenRef != ssrAtten || ssrFadeRef != ssrFade)
			{
				helper.Update_SSR(mainCamera, SSR_Enabled,ssrQuality,ssrAtten,ssrFade);

				if (LB_LightingProfile)
				{
					LB_LightingProfile.SSR_Enabled = SSR_Enabled;
					LB_LightingProfile.ssrQuality = ssrQuality;
					LB_LightingProfile.ssrFade = ssrFade;
					LB_LightingProfile.ssrAtten = ssrAtten;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}

			#endregion

				#region Stochastic Screen Space Reflections

			//-----------Stochastic Screen Space Reflections----------------------------------------------------------------------------
			GUILayout.BeginVertical ("Box");

			var ST_SSR_EnabledRef = ST_SSR_Enabled;

			EditorGUILayout.BeginHorizontal ();

			if(st_ssrState)
				GUILayout.Label(arrowOn,stateButton,GUILayout.Width(20),GUILayout.Height(14));
			else
				GUILayout.Label(arrowOff,stateButton,GUILayout.Width(20),GUILayout.Height(14));

			ST_SSR_Enabled = EditorGUILayout.Toggle("",ST_SSR_Enabled ,GUILayout.Width(30f),GUILayout.Height(17f));

			var st_ssrStateRef = st_ssrState;

			if (GUILayout.Button ("Stochastic SSR", stateButton, GUILayout.Width (300), GUILayout.Height (17f))) {
				st_ssrState = !st_ssrState ;
			}

			if(st_ssrStateRef != st_ssrState)
			{
				if (LB_LightingProfile)
					LB_LightingProfile.st_ssrState = st_ssrState;
				if (LB_LightingProfile)
					EditorUtility.SetDirty (LB_LightingProfile);
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.EndVertical ();
			//---------------------------------------------------------------------------------------

			if(st_ssrState )
			{
				if (helpBox)
					EditorGUILayout.HelpBox ("Activate Stochastic Screen Space Reflections effect for your camera", MessageType.Info);				
			}

			var resolutionModeRef = resolutionMode;
			var debugPassRef = debugPass;
			var rayDistanceRef = rayDistance;
			var screenFadeSizeRef = screenFadeSize;
			var smoothnessRangeRef = smoothnessRange;

			if(st_ssrState)
			{
				resolutionMode = (ResolutionMode)EditorGUILayout.EnumPopup ("Resolution", resolutionMode, GUILayout.Width (343));
				rayDistance  = EditorGUILayout.IntSlider("Ray Distance",rayDistance,0,100);
				screenFadeSize  = EditorGUILayout.Slider("Fade Distance",screenFadeSize,0,1);
				smoothnessRange = EditorGUILayout.Slider("Smoothness Range",smoothnessRange,0,1);
				debugPass = (SSRDebugPass)EditorGUILayout.EnumPopup ("DebugMode", debugPass, GUILayout.Width (343));
			}

			if(ST_SSR_EnabledRef != ST_SSR_Enabled || resolutionModeRef != resolutionMode
				|| debugPassRef != debugPass || screenFadeSizeRef != screenFadeSize
				|| rayDistanceRef != rayDistance || smoothnessRangeRef != smoothnessRange)
			{
				helper.Update_StochasticSSR(mainCamera, ST_SSR_Enabled,resolutionMode,debugPass,rayDistance,screenFadeSize,smoothnessRange);

				if (LB_LightingProfile)
				{
					LB_LightingProfile.ST_SSR_Enabled = ST_SSR_Enabled;
					LB_LightingProfile.resolutionMode = resolutionMode;
					LB_LightingProfile.screenFadeSize = screenFadeSize;
					LB_LightingProfile.smoothnessRange = smoothnessRange;
					LB_LightingProfile.debugPass = debugPass;
					LB_LightingProfile.rayDistance = rayDistance;
					EditorUtility.SetDirty (LB_LightingProfile);
				}
			}

			#endregion

				#region Check for updates

				if (GUILayout.Button ("Check for updates")) {
				
					EditorApplication.ExecuteMenuItem ("Assets/Lighting Box Updates");
				}

				EditorGUILayout.Space ();
				EditorGUILayout.Space ();
				EditorGUILayout.Space ();
				EditorGUILayout.Space ();

				#endregion

			}

			EditorGUILayout.EndScrollView ();
		}


		#region Update Settings
		void UpdateSettings()
		{
		
			// Sun Light Update
			if (sunLight) {
				sunLight.color = sunColor;
				sunLight.intensity = sunIntensity;
				sunLight.bounceIntensity = indirectIntensity;
			} else {
				Update_Sun ();
			}

			if (sunFlare)
			{
				if(sunLight)
					sunLight.flare = sunFlare;
			}
			else
			{
				if(sunLight)
					sunLight.flare = null;
			}

			// Skybox
		helper.Update_SkyBox (Ambient_Enabled,skyBox);

			// Update Lighting Mode
		helper.Update_LightingMode (Scene_Enabled,lightingMode);

			// Update Ambient
		helper.Update_Ambient (Ambient_Enabled,ambientLight, ambientColor,skyColor,equatorColor,groundColor);

			// Lights settings
		helper.Update_LightSettings (Scene_Enabled,lightSettings);

			// Color Space
		helper.Update_ColorSpace(Scene_Enabled,colorSpace);

			// Render Path
		helper.Update_RenderPath(Scene_Enabled,renderPath,mainCamera);

			// Volumetric Light
		helper.Update_VolumetricLight(mainCamera,VL_Enabled,vLight,vLightLevel);

			// Sun Shaft
		if(sunLight)
			helper.Update_SunShaft(mainCamera,SunShaft_Enabled, shaftQuality,shaftDistance,shaftBlur,shaftColor,sunLight.transform);

			// Shadows
		helper.Update_Shadows(Scene_Enabled,psShadow);

			// Light Probes
		helper.Update_LightProbes(Scene_Enabled,lightprobeMode);

			// Auto Mode
			helper.Update_AutoMode(autoMode);

			// Global Fog
		helper.Update_GlobalFog(mainCamera,Fog_Enabled,vFog,fDistance,fHeight,fheightDensity,fColor,fogIntensity);

		}
		#endregion

		#region On Load
		// load saved data based on project and scene name
		void OnLoad()
		{

		if (!mainCamera) {
			if (GameObject.Find (LB_LightingProfile.mainCameraName))
				mainCamera = GameObject.Find (LB_LightingProfile.mainCameraName).GetComponent<Camera> ();
			else
				mainCamera = GameObject.FindObjectOfType<Camera> ();
		}
		
		if (!GameObject.Find ("LightingBox_Helper")){
			GameObject helperObject = new GameObject ("LightingBox_Helper");
			helperObject.AddComponent<LB_LightingBoxHelper> ();
			helper = helperObject.GetComponent<LB_LightingBoxHelper> ();
		}


		if (LB_LightingProfile) {

			lightingMode = LB_LightingProfile.lightingMode;
			if (LB_LightingProfile.skyBox)
				skyBox = LB_LightingProfile.skyBox;
			else
				skyBox = RenderSettings.skybox;
			sunFlare = LB_LightingProfile.sunFlare;
			ambientLight = LB_LightingProfile.ambientLight;
			renderPath = LB_LightingProfile.renderPath;
			lightSettings = LB_LightingProfile.lightSettings;
			sunColor = LB_LightingProfile.sunColor;

			// Color Space
			colorSpace = LB_LightingProfile.colorSpace;

			// Volumetric Light
			vLight = LB_LightingProfile.vLight;
			vLightLevel = LB_LightingProfile.vLightLevel;

			lightprobeMode = LB_LightingProfile.lightProbesMode;

			// Shadows
			psShadow = LB_LightingProfile.lightsShadow;

			// Fog
			vFog = LB_LightingProfile.fogMode;
			fDistance = LB_LightingProfile.fogDistance;
			fHeight = LB_LightingProfile.fogHeight;
			fheightDensity = LB_LightingProfile.fogHeightIntensity;
			fColor = LB_LightingProfile.fogColor;
			fogIntensity = LB_LightingProfile.fogIntensity;

			// DOF Legacy
			dofRange = LB_LightingProfile.dofRange;
			dofBlur = LB_LightingProfile.dofBlur;
			falloff = LB_LightingProfile.falloff;
			dofQuality = LB_LightingProfile.dofQuality;
			visualize = LB_LightingProfile.visualize;
			// Auto Focus
			afRange = LB_LightingProfile.afRange;
			afBlur = LB_LightingProfile.afBlur;
			afSpeed = LB_LightingProfile.afSpeed;
			afUpdate = LB_LightingProfile.afUpdate;
			afRayLength = LB_LightingProfile.afRayLength;
			afLayer = LB_LightingProfile.afLayer;

			// AA
			aaMode = LB_LightingProfile.aaMode;

			// AO
			aoIntensity = LB_LightingProfile.aoIntensity;
			aoColor = LB_LightingProfile.aoColor;
			ambientOnly = LB_LightingProfile.ambientOnly;
			aoRadius = LB_LightingProfile.aoRadius;
			aoType = LB_LightingProfile.aoType;
			aoQuality = LB_LightingProfile.aoQuality;

			// Bloom
			bIntensity = LB_LightingProfile.bIntensity;
			bColor = LB_LightingProfile.bColor;
			bThreshould = LB_LightingProfile.bThreshould;
			dirtTexture = LB_LightingProfile.dirtTexture;
			dirtIntensity = LB_LightingProfile.dirtIntensity;
			mobileOptimizedBloom = LB_LightingProfile.mobileOptimizedBloom;
			bRotation = LB_LightingProfile.bRotation;

			// Color Grading
			exposureIntensity = LB_LightingProfile.exposureIntensity;
			contrastValue = LB_LightingProfile.contrastValue;
			temp = LB_LightingProfile.temp;
			eyeKeyValue = LB_LightingProfile.eyeKeyValue;
			colorMode = LB_LightingProfile.colorMode;
			colorGamma = LB_LightingProfile.colorGamma;
			colorLift = LB_LightingProfile.colorLift;
			gamma = LB_LightingProfile.gamma;
			saturation = LB_LightingProfile.saturation;
			lut = LB_LightingProfile.lut;

			// Effects
			MotionBlur_Enabled = LB_LightingProfile.MotionBlur_Enabled;
			Vignette_Enabled = LB_LightingProfile.Vignette_Enabled;
			vignetteIntensity = LB_LightingProfile.vignetteIntensity;
			Chromattic_Enabled = LB_LightingProfile.Chromattic_Enabled;
			CA_Intensity = LB_LightingProfile.CA_Intensity;
			mobileOptimizedChromattic = LB_LightingProfile.mobileOptimizedChromattic;

			// SSR
			SSR_Enabled = LB_LightingProfile.SSR_Enabled;
			ssrQuality = LB_LightingProfile.ssrQuality;
			ssrAtten = LB_LightingProfile.ssrAtten;
			ssrFade = LB_LightingProfile.ssrFade;
			SSR_Enabled = LB_LightingProfile.SSR_Enabled;

			// Stochastic SSR
			resolutionMode = LB_LightingProfile.resolutionMode;
			debugPass = LB_LightingProfile.debugPass;
			rayDistance = LB_LightingProfile.rayDistance;
			screenFadeSize = LB_LightingProfile.screenFadeSize;
			smoothnessRange = LB_LightingProfile.smoothnessRange;
			ST_SSR_Enabled = LB_LightingProfile.ST_SSR_Enabled;


			// Lightmap
			bakedResolution = LB_LightingProfile.bakedResolution;
			sunIntensity = LB_LightingProfile.sunIntensity;
			indirectIntensity = LB_LightingProfile.indirectIntensity;

			ambientColor = LB_LightingProfile.ambientColor;
			ambientLight = LB_LightingProfile.ambientLight;
			skyBox = LB_LightingProfile.skyBox;
			skyColor = LB_LightingProfile.skyColor;
			equatorColor = LB_LightingProfile.equatorColor;
			groundColor = LB_LightingProfile.groundColor;

			// Auto lightmap
			autoMode = LB_LightingProfile.automaticLightmap;

			// WebGL
			webGL_Mobile = LB_LightingProfile.webGL_Mobile;

			// Sun Shaft
			shaftDistance = LB_LightingProfile.shaftDistance;
			shaftBlur = LB_LightingProfile.shaftBlur;
			shaftColor = LB_LightingProfile.shaftColor;
			shaftQuality = LB_LightingProfile.shaftQuality;


			// Foliage
			matType = LB_LightingProfile.matType;
			translucency = LB_LightingProfile.translucency;
			ambient = LB_LightingProfile.ambient;
			shadows = LB_LightingProfile.shadows;
			tranColor = LB_LightingProfile.tranColor;
			windSpeed = LB_LightingProfile.windSpeed;
			windScale = LB_LightingProfile.windScale;
			CustomShader = LB_LightingProfile.CustomShader;

			// Snow
			snowAlbedo = LB_LightingProfile.snowAlbedo;
			snowNormal = LB_LightingProfile.snowNormal;
			snowIntensity = LB_LightingProfile.snowIntensity;
			customShaderSnow = LB_LightingProfile.customShaderSnow;

			Ambient_Enabled = LB_LightingProfile.Ambient_Enabled;
			Scene_Enabled = LB_LightingProfile.Scene_Enabled;
			Sun_Enabled = LB_LightingProfile.Sun_Enabled;
			VL_Enabled = LB_LightingProfile.VL_Enabled;
			SunShaft_Enabled = LB_LightingProfile.SunShaft_Enabled;
			Fog_Enabled = LB_LightingProfile.Fog_Enabled;
			AutoFocus_Enabled = LB_LightingProfile.AutoFocus_Enabled;
			DOF_Enabled = LB_LightingProfile.DOF_Enabled;
			Bloom_Enabled = LB_LightingProfile.Bloom_Enabled;
			AA_Enabled = LB_LightingProfile.AA_Enabled;
			AO_Enabled = LB_LightingProfile.AO_Enabled;

			buildState = LB_LightingProfile.buildState;
			profileState = LB_LightingProfile.profileState;
			cameraState = LB_LightingProfile.cameraState;
			lightSettingsState = LB_LightingProfile.lightSettingsState;
			sunState = LB_LightingProfile.sunState;
			ambientState = LB_LightingProfile.ambientState;
			ssrState = LB_LightingProfile.ssrState;
			st_ssrState = LB_LightingProfile.st_ssrState;


			chromatticState = LB_LightingProfile.chromatticState;
			vignetteState = LB_LightingProfile.vignetteState;
			motionBlurState = LB_LightingProfile.motionBlurState;
			aoState = LB_LightingProfile.aoState;
			aaState = LB_LightingProfile.aaState;
			bloomState = LB_LightingProfile.bloomState;
			colorState = LB_LightingProfile.colorState;
			autoFocusState = LB_LightingProfile.autoFocusState;
			dofState = LB_LightingProfile.dofState;
			fogState = LB_LightingProfile.fogState;
			sunShaftState = LB_LightingProfile.sunShaftState;
			vLightState = LB_LightingProfile.vLightState;
			foliageState = LB_LightingProfile.foliageState;
			snowState = LB_LightingProfile.snowState;
			OptionsState = LB_LightingProfile.OptionsState;
			LightingBoxState = LB_LightingProfile.LightingBoxState;

			mainCamera.allowHDR = true;
			mainCamera.allowMSAA = false;

			if (LB_LightingProfile.postProcessingProfile)
				postProcessingProfile = LB_LightingProfile.postProcessingProfile;
		}

			UpdatePostEffects ();

			UpdateSettings ();

			Update_Sun();

	}
		#endregion

		#region Update Post Effects Settings

		public void UpdatePostEffects()
		{

			if(!helper)
				helper = GameObject.Find("LightingBox_Helper").GetComponent<LB_LightingBoxHelper> ();

			if (!postProcessingProfile)
				return;

			helper.UpdateProfiles (mainCamera, postProcessingProfile);

			// MotionBlur
		if (MotionBlur_Enabled)
				helper.Update_MotionBlur (true);
			else
				helper.Update_MotionBlur (false);

			// Vignette
			helper.Update_Vignette (Vignette_Enabled,vignetteIntensity);


		// _ChromaticAberration
		helper.Update_ChromaticAberration(Chromattic_Enabled,CA_Intensity,mobileOptimizedChromattic);

		// Foliage
		helper.Update_Foliage (translucency, ambient, shadows, windSpeed, windScale, tranColor);

		// Snow
		helper.Update_Snow (snowAlbedo,snowNormal,snowIntensity);

		helper.Update_Bloom(Bloom_Enabled,bIntensity,bThreshould,bColor,dirtTexture,dirtIntensity,mobileOptimizedBloom,bRotation);



			// Depth of Field 1 
		helper.Update_DOF(mainCamera,DOF_Enabled,dofQuality,dofBlur,dofRange,falloff,visualize);
		helper.Update_AutoFocus (mainCamera, AutoFocus_Enabled, DOF_Enabled,afLayer, afRange, afBlur, afSpeed, afUpdate, afRayLength);

			// AO
		if (AO_Enabled)
				helper.Update_AO(mainCamera,true,aoType,aoRadius,aoIntensity,ambientOnly,aoColor, aoQuality);
			else
				helper.Update_AO(mainCamera,false,aoType,aoRadius,aoIntensity,ambientOnly,aoColor, aoQuality);


			// Color Grading
		helper.Update_ColorGrading(colorMode,exposureIntensity,contrastValue,temp,eyeKeyValue,saturation,colorGamma,colorLift,gamma,lut);

			////-----------------------------------------------------------------------------
			/// 
			// Screen Space Reflections
		helper.Update_SSR(mainCamera, SSR_Enabled,ssrQuality,ssrAtten,ssrFade);

		helper.Update_StochasticSSR(mainCamera, ST_SSR_Enabled,resolutionMode,debugPass,rayDistance,screenFadeSize,smoothnessRange);

	}
		#endregion

		#region Scene Delegate

		string currentScene;    
		void SceneChanging ()
	{
		if (currentScene != EditorSceneManager.GetActiveScene ().name) {
			if (System.String.IsNullOrEmpty (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name)))
				LB_LightingProfile = Resources.Load ("DefaultSettings")as LB_LightingProfile;
			else
				LB_LightingProfile = (LB_LightingProfile)AssetDatabase.LoadAssetAtPath (EditorPrefs.GetString (EditorSceneManager.GetActiveScene ().name), typeof(LB_LightingProfile));

			helper.Update_MainProfile (LB_LightingProfile);

			OnLoad ();
			currentScene = EditorSceneManager.GetActiveScene ().name;
		}

	}
		#endregion

		#region Sun Light
		public void Update_Sun()
		{
		if (Sun_Enabled) {
			if (!RenderSettings.sun) {
				Light[] lights = GameObject.FindObjectsOfType<Light> ();
				foreach (Light l in lights) {
					if (l.type == LightType.Directional) {
						sunLight = l;

						if (sunColor != Color.clear)
							sunColor = l.color;
						else
							sunColor = Color.white;

						//sunLight.shadowNormalBias = 0.05f;  
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

				//	sunLight.shadowNormalBias = 0.05f;  
				sunLight.color = sunColor;
				if (sunLight.bounceIntensity == 1f)
					sunLight.bounceIntensity = indirectIntensity;
			}
		}
	}

		#endregion

		#region On Download Completed
		void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
	{
		if (e.Error != null)
			Debug.Log (e.Error);
	}
		#endregion
}
#endif