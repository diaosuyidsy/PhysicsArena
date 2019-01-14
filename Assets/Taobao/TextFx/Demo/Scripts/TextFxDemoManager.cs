using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TextFx;

public class TextFxDemoManager : MonoBehaviour {

	public TextFxUGUI m_titleEffect;

	public RectTransform m_animEditorContent;
	public TextFxUGUI m_effect;

#if !UNITY_4_6 && !UNITY_4_7 && !UNITY_5_0 && !UNITY_5_1

	readonly Color SECTION_INACTIVE_COLOUR = new Color(1f, 1f, 1f, 100f / 255f);
	readonly Color SECTION_ACTIVE_COLOUR = new Color(193f / 255f, 247f / 255f, 255f / 255f, 255f / 255f);
	readonly Color SECTION_SELECTED_COLOUR = new Color(66f / 255f, 204f / 255f, 223f / 255f, 255f / 255f);

	public Image[] m_sectionHeaderImages;

	public RectTransform m_menuSectionsContainer;
	public RectTransform[] m_animSectionTransforms;
	public Dropdown[] m_animSectionDropdowns;

	public Button m_playButton;
	public Text m_playButtonText;
	public Button m_resetButton;
	public Button m_continueButton;

	public FloatVariableSetting m_floatVariableSettingPrefab;
	public Vector3VariableSetting m_vector3VariableSettingPrefab;
	public ToggleVariableSetting m_toggleVariableSettingPrefab;
	public ColourVariableSetting m_colourVariableSettingPrefab;

	public InputField m_textInput;

	public int m_introStartEffect = -1;
	public int m_mainStartEffect = -1;
	public int m_outroStartEffect = -1;

	public bool m_skipTitleEffectIntro = false;

	TextFxAnimationManager.PresetAnimationSection[] m_animationSections;

	ObjectPool<FloatVariableSetting> m_floatVariableSettingsPool;
	ObjectPool<Vector3VariableSetting> m_vector3VariableSettingsPool;
	ObjectPool<ToggleVariableSetting> m_toggleVariableSettingsPool;
	ObjectPool<ColourVariableSetting> m_colourVariableSettingsPool;

	List<FloatVariableSetting>[] m_floatVariableSettingsInUse;
	List<Vector3VariableSetting>[] m_vector3VariableSettingsInUse;
	List<ToggleVariableSetting>[] m_toggleVariableSettingsInUse;
	List<ColourVariableSetting>[] m_colourVariableSettingsInUse;

	bool[] m_sectionsActive;
	int m_activeSectionIndex = -1;
	Color m_playbackButtonHighlightColour = new Color(100 / 255f, 1f, 150 / 255f);

	void Start ()
	{
		List<string[]> configAnimNames = new List<string[]> { TextFxQuickSetupAnimConfigs.IntroAnimNames, TextFxQuickSetupAnimConfigs.MainAnimNames, TextFxQuickSetupAnimConfigs.OutroAnimNames };

		// Calculate starting animations
		int introAnimIndex = Mathf.Clamp(m_introStartEffect, 1, configAnimNames[0].Length);
		int mainAnimIndex = Mathf.Clamp(m_mainStartEffect, 1, configAnimNames[1].Length);
		int outroAnimIndex = Mathf.Clamp(m_outroStartEffect, 1, configAnimNames[2].Length);

		if (m_introStartEffect < 1)
		{
			// Pick a random intro anim, which has no audio
			do
			{
				introAnimIndex = Random.Range (1, configAnimNames [0].Length);
			}
			while(configAnimNames[0][introAnimIndex] == "Epic" || configAnimNames[0][introAnimIndex] == "Mental House" || configAnimNames[0][introAnimIndex] == "Type Writer");
		}
		if (m_mainStartEffect < 1)
		{
			// Pick a random main anim, which has no audio
			do
			{
				mainAnimIndex = Random.Range (1, configAnimNames [1].Length);
			}
			while(configAnimNames[1][mainAnimIndex] == "Windy");
		}
		if (m_outroStartEffect < 1)
		{
			// Pick a random starting intro anim, which isn't a noisey one
			do
			{
				outroAnimIndex = Random.Range (1, configAnimNames [2].Length);
			}
			while(configAnimNames[2][outroAnimIndex] == "Shot Drop");
		}

		List<Dropdown.OptionData> dropdownOptions;
		int[] animIndex = new int[]{ introAnimIndex, mainAnimIndex, outroAnimIndex };

		for(int idx=0; idx < 3; idx++)
		{
			// Populate dropdown values
			dropdownOptions = new List<Dropdown.OptionData>();
			foreach (string animName in configAnimNames[idx])
				dropdownOptions.Add(new Dropdown.OptionData(animName));
			m_animSectionDropdowns[idx].options = dropdownOptions;

			TextFxAnimationManager.PRESET_ANIMATION_SECTION animSection = (TextFxAnimationManager.PRESET_ANIMATION_SECTION)idx;

			m_animSectionDropdowns [idx].value = animIndex[idx];

			// Assign dropdown listeners
			m_animSectionDropdowns[idx].onValueChanged.AddListener((int index) => { AnimationSectionSet(animSection, index, true); });

			// Set header images to inactive colours
			m_sectionHeaderImages[idx].color = SECTION_INACTIVE_COLOUR;
        }

		m_animationSections = new TextFxAnimationManager.PresetAnimationSection[] { m_effect.AnimationManager.m_preset_intro, m_effect.AnimationManager.m_preset_main, m_effect.AnimationManager.m_preset_outro };

		m_floatVariableSettingsPool = new ObjectPool<FloatVariableSetting>(m_floatVariableSettingPrefab.gameObject, 5);
		m_vector3VariableSettingsPool = new ObjectPool<Vector3VariableSetting>(m_vector3VariableSettingPrefab.gameObject, 5);
		m_toggleVariableSettingsPool = new ObjectPool<ToggleVariableSetting>(m_toggleVariableSettingPrefab.gameObject, 5);
		m_colourVariableSettingsPool = new ObjectPool<ColourVariableSetting>(m_colourVariableSettingPrefab.gameObject, 5);

		m_floatVariableSettingsInUse = new List<FloatVariableSetting>[3];
		m_vector3VariableSettingsInUse = new List<Vector3VariableSetting>[3];
		m_toggleVariableSettingsInUse = new List<ToggleVariableSetting>[3];
		m_colourVariableSettingsInUse = new List<ColourVariableSetting>[3];


		// Add playback button callbacks
		m_playButton.onClick.AddListener (()=> {

			if(!m_effect.AnimationManager.Playing)
			{
				PlayAnimation();
			}
			else
			{
				if(m_effect.AnimationManager.Paused)
				{
					m_effect.AnimationManager.Paused = false;
					m_playButtonText.text = "Pause";
				}
				else
				{
					m_effect.AnimationManager.Paused = true;
					m_playButtonText.text = "Play";
				}
			}
		});

		m_resetButton.onClick.AddListener (()=> {
			m_effect.AnimationManager.ResetAnimation();

			m_playButtonText.text = "Play";

			HideContinueButton();
		});

		m_continueButton.onClick.AddListener (()=> {

			if(!m_effect.AnimationManager.Paused)
			{
				LetterAnimation letterAnim = m_effect.AnimationManager.GetAnimation(0);

				if(letterAnim != null && (letterAnim.CurrentAnimationState == LETTER_ANIMATION_STATE.WAITING || letterAnim.CurrentAnimationState == LETTER_ANIMATION_STATE.WAITING_INFINITE))
					m_effect.AnimationManager.ContinuePastBreak();
				else
					m_effect.AnimationManager.ContinuePastLoop();
			}
		});
		m_continueButton.gameObject.SetActive (false);

		m_sectionsActive = new bool[3];


		m_textInput.text = "Hello World!";


#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
		m_textInput.onValueChange.AddListener ((string new_string) => {
#else
		m_textInput.onValueChanged.AddListener ((string new_string) => {
#endif

			m_effect.SetText(new_string);

		});
			

		// Preset the animation sections, so it's not empty on start
		AnimationSectionSet (TextFxAnimationManager.PRESET_ANIMATION_SECTION.INTRO, introAnimIndex);
		AnimationSectionSet (TextFxAnimationManager.PRESET_ANIMATION_SECTION.MAIN, mainAnimIndex);
		AnimationSectionSet (TextFxAnimationManager.PRESET_ANIMATION_SECTION.OUTRO, outroAnimIndex);

		// Select Intro section
		AnimationSectionSelected (0);

		// Play intro sequence
		StartCoroutine(DoIntro(6, m_skipTitleEffectIntro));
    }

	IEnumerator DoIntro(float waitTime, bool skipTitleEffect = false)
	{
		yield return false;

		m_animEditorContent.gameObject.SetActive (false);

		// play intro title effect
		m_titleEffect.GameObject.SetActive (true);
		m_titleEffect.AnimationManager.PlayAnimation (1f);

		if(!skipTitleEffect)
			yield return new WaitForSeconds(waitTime);

		m_titleEffect.GameObject.SetActive (false);

		float timer = 0;

		Quaternion rotation = Quaternion.Euler (0, 90, 0);
		Vector3 rotationVec = new Vector3 (0, 90, 0);

		m_animEditorContent.rotation = Quaternion.Euler (0,90,0);
		m_animEditorContent.gameObject.SetActive (true);

		while (timer <= 0.5f)
		{
			rotationVec.y = Mathf.Lerp (90, 0, timer / 0.5f);

			rotation.eulerAngles = rotationVec;

			m_animEditorContent.rotation = rotation;

			timer += Time.deltaTime;

			yield return false;
		}

		rotationVec.y = 0;

		rotation.eulerAngles = rotationVec;

		m_animEditorContent.rotation = rotation;

		PlayAnimation (0.5f);
	}

	void PlayAnimation(float delay = 0)
	{
		m_effect.AnimationManager.PlayAnimation(delay, AnimationFinished);
		m_playButtonText.text = "Pause";

		ShowContinueButton();

		StartCoroutine ("HighlightContinueButton");
	}

	void ShowContinueButton()
	{
		m_continueButton.image.color = Color.white;

		m_continueButton.gameObject.SetActive (true);

		m_playButton.image.color = Color.white;
	}

	void HideContinueButton()
	{
		m_continueButton.gameObject.SetActive (false);

		StopCoroutine ("HighlightContinueButton");

		m_playButton.image.color = m_playbackButtonHighlightColour;
	}

	IEnumerator HighlightContinueButton()
	{
		yield return new WaitForSeconds (4f);

		m_continueButton.image.color = m_playbackButtonHighlightColour;
	}


	void AnimationFinished()
	{
		m_playButtonText.text = "Play";

		HideContinueButton();
	}

	void RecycleSectionSettingComponents(int sectionIndex)
	{
		if (m_floatVariableSettingsInUse[sectionIndex] != null)
			foreach (FloatVariableSetting fVar in m_floatVariableSettingsInUse[sectionIndex])
				m_floatVariableSettingsPool.Recycle(fVar);
		if (m_vector3VariableSettingsInUse[sectionIndex] != null)
			foreach (Vector3VariableSetting vVar in m_vector3VariableSettingsInUse[sectionIndex])
				m_vector3VariableSettingsPool.Recycle(vVar);
		if (m_toggleVariableSettingsInUse[sectionIndex] != null)
			foreach (ToggleVariableSetting tVar in m_toggleVariableSettingsInUse[sectionIndex])
				m_toggleVariableSettingsPool.Recycle(tVar);
		if (m_colourVariableSettingsInUse[sectionIndex] != null)
			foreach (ColourVariableSetting cVar in m_colourVariableSettingsInUse[sectionIndex])
				m_colourVariableSettingsPool.Recycle(cVar);
	}

	public void AnimationSectionSelected(int index)
	{
		if(m_sectionsActive[index])
		{
			if (m_activeSectionIndex != index)
			{
				if(m_activeSectionIndex >= 0)
				{
					m_sectionHeaderImages[m_activeSectionIndex].color = SECTION_ACTIVE_COLOUR;
					CloseAnimationSection(m_activeSectionIndex);
				}

				m_sectionHeaderImages [index].color = SECTION_SELECTED_COLOUR;
				
				m_activeSectionIndex = index;

				OpenAnimationSection(m_activeSectionIndex);
			}
		}
	}

	void CloseAnimationSection(int sectionIndex)
	{
		// Hide all settings associated to this section
		foreach (FloatVariableSetting fVar in m_floatVariableSettingsInUse[sectionIndex]) fVar.gameObject.SetActive(false);
		foreach (Vector3VariableSetting vVar in m_vector3VariableSettingsInUse[sectionIndex]) vVar.gameObject.SetActive(false);
		foreach (ToggleVariableSetting tVar in m_toggleVariableSettingsInUse[sectionIndex]) tVar.gameObject.SetActive(false);
		foreach (ColourVariableSetting cVar in m_colourVariableSettingsInUse[sectionIndex]) cVar.gameObject.SetActive(false);
	}

	void OpenAnimationSection(int sectionIndex)
	{
		// Show all settings associated to this section
		foreach (FloatVariableSetting fVar in m_floatVariableSettingsInUse[sectionIndex]) fVar.gameObject.SetActive(fVar.IsSubSetting ? fVar.SubSettingActive : true);
		foreach (Vector3VariableSetting vVar in m_vector3VariableSettingsInUse[sectionIndex]) vVar.gameObject.SetActive(vVar.IsSubSetting ? vVar.SubSettingActive : true);
		foreach (ToggleVariableSetting tVar in m_toggleVariableSettingsInUse[sectionIndex]) tVar.gameObject.SetActive(tVar.IsSubSetting ? tVar.SubSettingActive : true);
		foreach (ColourVariableSetting cVar in m_colourVariableSettingsInUse[sectionIndex]) cVar.gameObject.SetActive(cVar.IsSubSetting ? cVar.SubSettingActive : true);
	}

	void AnimationSectionSet(TextFxAnimationManager.PRESET_ANIMATION_SECTION section, int animIndex, bool restartAnimation = false)
	{
		if(m_activeSectionIndex >= 0 && m_activeSectionIndex != (int) section)
			CloseAnimationSection(m_activeSectionIndex);


		// Disable any previous setting section objects for this animaction Section
		RecycleSectionSettingComponents((int)section);

		m_floatVariableSettingsInUse[(int)section] = new List<FloatVariableSetting>();
		m_vector3VariableSettingsInUse[(int)section] = new List<Vector3VariableSetting>();
		m_toggleVariableSettingsInUse[(int)section] = new List<ToggleVariableSetting>();
		m_colourVariableSettingsInUse[(int)section] = new List<ColourVariableSetting>();


		// Setup the animation section on the effect instance
		m_effect.AnimationManager.SetQuickSetupSection(section, m_animSectionDropdowns[(int) section].options[animIndex].text, forceClearOldAudioParticles: false);


		// Set the section as focused/selected
		if(m_activeSectionIndex >= 0)
			m_sectionHeaderImages[m_activeSectionIndex].color = SECTION_ACTIVE_COLOUR;

		if(animIndex > 0)
		{
			m_sectionHeaderImages[(int)section].color = SECTION_SELECTED_COLOUR;

			m_activeSectionIndex = (int)section;
			m_sectionsActive[m_activeSectionIndex] = true;
		}
		else
		{
			m_sectionHeaderImages[(int)section].color = SECTION_INACTIVE_COLOUR;

			m_sectionsActive[(int) section] = false;

			if (m_activeSectionIndex >= 0)
			{
				if(m_activeSectionIndex == (int) section)
					m_activeSectionIndex = -1;
			}
		}


		TextFxAnimationManager.PresetAnimationSection animation_section = m_animationSections[(int)section];
		int sectionSiblingIndexOffset = m_animSectionTransforms[(int)section].GetSiblingIndex();
		int settingIdx = 0;

		RectTransform settingTransform;

		foreach (PresetEffectSetting effectSetting in animation_section.m_preset_effect_settings)
		{
			settingTransform = null;

			switch (effectSetting.m_data_type)
			{
				case ANIMATION_DATA_TYPE.DELAY:
				case ANIMATION_DATA_TYPE.DURATION:
					// Grab a float variable component
					FloatVariableSetting floatVarSetting = m_floatVariableSettingsPool.GetObject();
					m_floatVariableSettingsInUse[(int)section].Add(floatVarSetting);

					floatVarSetting.transform.SetParent(m_menuSectionsContainer);

					ANIMATION_DATA_TYPE dataType = effectSetting.m_data_type;

					floatVarSetting.Setup(effectSetting, effectSetting.GetSettingData(m_effect.AnimationManager, animation_section.m_start_action, animation_section.m_start_loop), ()=> {
						m_effect.AnimationManager.PrepareAnimationData(dataType);

						if(dataType == ANIMATION_DATA_TYPE.DURATION)
							m_effect.AnimationManager.PrepareAnimationData(ANIMATION_DATA_TYPE.DELAY);
					}, false);

					settingTransform = (floatVarSetting.transform as RectTransform);

					break;

				case ANIMATION_DATA_TYPE.GLOBAL_ROTATION:
				case ANIMATION_DATA_TYPE.LOCAL_ROTATION:
				case ANIMATION_DATA_TYPE.POSITION:
				case ANIMATION_DATA_TYPE.GLOBAL_SCALE:
				case ANIMATION_DATA_TYPE.LOCAL_SCALE:
					// Grab a Vector3 variable component
					Vector3VariableSetting vector3VarSetting = m_vector3VariableSettingsPool.GetObject();
					m_vector3VariableSettingsInUse[(int)section].Add(vector3VarSetting);

					vector3VarSetting.transform.SetParent(m_menuSectionsContainer);

					ANIMATION_DATA_TYPE vecDataType = effectSetting.m_data_type;

					vector3VarSetting.Setup(effectSetting, effectSetting.GetSettingData(m_effect.AnimationManager, animation_section.m_start_action, animation_section.m_start_loop), () => {
						m_effect.AnimationManager.PrepareAnimationData(vecDataType);
					}, false);

					settingTransform = (vector3VarSetting.transform as RectTransform);

					break;
				case ANIMATION_DATA_TYPE.COLOUR:

					ColourVariableSetting colourVarSetting = m_colourVariableSettingsPool.GetObject();
					m_colourVariableSettingsInUse[(int)section].Add(colourVarSetting);

					colourVarSetting.transform.SetParent(m_menuSectionsContainer);
					settingTransform = (colourVarSetting.transform as RectTransform);

					ANIMATION_DATA_TYPE colDataType = ANIMATION_DATA_TYPE.COLOUR;

					colourVarSetting.Setup(effectSetting, effectSetting.GetSettingData(m_effect.AnimationManager, animation_section.m_start_action, animation_section.m_start_loop), () => {
						m_effect.AnimationManager.PrepareAnimationData(colDataType);
					}, false);

					break;
				case ANIMATION_DATA_TYPE.DELAY_EASED_RANDOM_SWITCH:
					ToggleVariableSetting toggleVarSetting = m_toggleVariableSettingsPool.GetObject();
					m_toggleVariableSettingsInUse[(int)section].Add(toggleVarSetting);

					toggleVarSetting.transform.SetParent(m_menuSectionsContainer);

					effectSetting.m_setting_name = "Randomised?";
                    toggleVarSetting.Setup(effectSetting, effectSetting.GetSettingData(m_effect.AnimationManager, animation_section.m_start_action, animation_section.m_start_loop), null, false);

					settingTransform = (toggleVarSetting.transform as RectTransform);

					break;
				default:
					settingIdx--;
					break;
			}

			if (settingTransform != null)
			{
				settingTransform.SetSiblingIndex (sectionSiblingIndexOffset + 1 + settingIdx);

				settingTransform.localPosition = new Vector3 (settingTransform.localPosition.x, settingTransform.localPosition.y, 0);
				settingTransform.localScale = Vector3.one;
			}

			settingIdx++;
        }


		// Add in the Exit Delay option
		if(animIndex > 0)
		{
			ToggleVariableSetting toggleVarSetting = m_toggleVariableSettingsPool.GetObject();
			m_toggleVariableSettingsInUse[(int)section].Add(toggleVarSetting);

			FloatVariableSetting floatVarSetting = m_floatVariableSettingsPool.GetObject();
			m_floatVariableSettingsInUse[(int)section].Add(floatVarSetting);

			toggleVarSetting.transform.SetParent(m_menuSectionsContainer);

			RectTransform exitDelayTransform = (toggleVarSetting.transform as RectTransform);
			exitDelayTransform.SetSiblingIndex(sectionSiblingIndexOffset + 1 + settingIdx);

			exitDelayTransform.localPosition = new Vector3 (exitDelayTransform.localPosition.x, exitDelayTransform.localPosition.y, 0);
			exitDelayTransform.localScale = Vector3.one;

			List<PresetEffectSetting.VariableStateListener> stateListeners =
										new List<PresetEffectSetting.VariableStateListener>() {
											new PresetEffectSetting.VariableStateListener() {
												m_startToggleValue = animation_section.m_exit_pause,
												m_onToggleStateChangeCallback = (bool state)=> {
													animation_section.SetExitPauseState(m_effect.AnimationManager, state);

													floatVarSetting.SubSettingSetActive(state);
												}
											}
										};

			toggleVarSetting.Setup("Exit Delay", stateListeners, null, false);

			settingIdx++;


			floatVarSetting.transform.SetParent(m_menuSectionsContainer);

			exitDelayTransform = (floatVarSetting.transform as RectTransform);
			exitDelayTransform.SetSiblingIndex(sectionSiblingIndexOffset + 1 + settingIdx);

			exitDelayTransform.localPosition = new Vector3 (exitDelayTransform.localPosition.x, exitDelayTransform.localPosition.y, 0);
			exitDelayTransform.localScale = Vector3.one;

			floatVarSetting.Setup("Duration", new List<PresetEffectSetting.VariableStateListener>() {
													new PresetEffectSetting.VariableStateListener() {
														m_startFloatValue = animation_section.m_exit_pause_duration,
														m_onFloatStateChangeCallback = (float fVal) => {
															animation_section.SetExitPauseDuration(m_effect.AnimationManager, fVal);
														}
													}}, null, true);

			floatVarSetting.SubSettingSetActive(animation_section.m_exit_pause);
        }

		// Reset Play button text
		m_playButtonText.text = "Play";

		HideContinueButton ();

		if(restartAnimation)
			PlayAnimation ();
	}

#else

	void Start()
	{
		StartCoroutine (NonDemoIntro ());
	}

	IEnumerator NonDemoIntro()
	{
		m_titleEffect.GameObject.SetActive (true);

		m_titleEffect.AnimationManager.PlayAnimation (0.2f);

		m_animEditorContent.gameObject.SetActive (false);

		yield return new WaitForSeconds (5);

		m_animEditorContent.gameObject.SetActive (true);

		m_effect.SetText ("Demo only compatible with Unity 5.2 and above...Sorry!");

		m_effect.AnimationManager.SetQuickSetupSection(TextFxAnimationManager.PRESET_ANIMATION_SECTION.INTRO, "Blink");

		m_effect.AnimationManager.PlayAnimation (0.2f);
	}

#endif
}
