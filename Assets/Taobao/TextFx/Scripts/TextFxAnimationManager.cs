using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.TextFx.JSON;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TextFx
{
	public enum TEXTFX_IMPLEMENTATION
	{
		ALL = -1,
		NONE = 0,
		UGUI,
		NGUI,
		NATIVE,
		TMP,
		TMP_UGUI
	}

	[System.Serializable]
	public class TextFxAnimationManager
	{
		public enum PRESET_ANIMATION_SECTION
		{
			NONE = -1,
			INTRO = 0,
			MAIN,
			OUTRO
		}

		[System.Serializable]
		public class PresetAnimationSection
		{
			public List<PresetEffectSetting> m_preset_effect_settings;
			public bool m_active = false;
			public int m_start_action = 0;
			public int m_num_actions = 0;
			public int m_start_loop = 0;
			public int m_num_loops = 0;
			public bool m_exit_pause = false;
			public float m_exit_pause_duration = 1;
			public bool m_repeat = false;
			public int m_repeat_count = 0;

			public int ExitPauseIndex { get { return m_start_action + m_num_actions; } }

			public void SetExitPauseState(TextFxAnimationManager animationManager, bool state)
			{
				m_exit_pause = state;

				LetterAction exitPauseAction = animationManager.m_master_animations[0].GetAction(ExitPauseIndex);

				exitPauseAction.m_duration_progression.SetConstant(m_exit_pause ? m_exit_pause_duration : TextFxAnimationManager.INACTIVE_EXIT_PAUSE_DURATION);

				animationManager.PrepareAnimationData(ANIMATION_DATA_TYPE.DURATION);

				animationManager.UpdatePresetAnimSectionActionIndexes();
			}

			public void SetExitPauseDuration(TextFxAnimationManager animationManager, float duration)
			{
				m_exit_pause_duration = duration;

				LetterAction exitPauseAction = animationManager.m_master_animations[0].GetAction(ExitPauseIndex);

				exitPauseAction.m_duration_progression.SetConstant(duration);

				animationManager.PrepareAnimationData(ANIMATION_DATA_TYPE.DURATION);
			}

			public void Reset()
			{
				m_preset_effect_settings = new List<PresetEffectSetting> ();
				m_start_action = 0;
				m_num_actions = 0;
				m_start_loop = 0;
				m_num_loops = 0;
				m_exit_pause = false;
				m_exit_pause_duration = 1;
				m_repeat = false;
				m_repeat_count = 0;
				m_active = false;
			}
		}

		public static string[] m_animation_section_names = new string[]{"Intro", "Main", "Outro"};
		public static string[] m_animation_section_folders = new string[]{"Intros", "Mains", "Outros"};

		public interface GuiTextDataHandler
		{
			int NumVerts { get; }
			int NumVertsPerLetter { get; }
			int ExtraVertsPerLetter { get; }
			Vector3[] GetLetterBaseVerts(int letterIndex);
			Color[] GetLetterBaseCols(int letterIndex);
			Vector3[] GetLetterExtraVerts(int letterIndex);
			Color[] GetLetterExtraCols(int letterIndex);
		}

		public const float INACTIVE_EXIT_PAUSE_DURATION = 0.000001f;

		public const string ANIM_INTROS_FOLDER_NAME = "Intros";
		public const string ANIM_MAINS_FOLDER_NAME = "Mains";
		public const string ANIM_OUTROS_FOLDER_NAME = "Outros";

		const int JSON_EXPORTER_VERSION = 1;

		public List<LetterAnimation> m_master_animations;

		public bool m_begin_on_start = false;
		public ON_FINISH_ACTION m_on_finish_action = ON_FINISH_ACTION.NONE;
		public float m_animation_speed_factor = 1;
		public float m_begin_delay = 0;
		public AnimatePerOptions m_animate_per = AnimatePerOptions.LETTER;
		public AnimationTime m_time_type = AnimationTime.GAME_TIME;

		[SerializeField]
		LetterSetup[] m_letters;
		public LetterSetup[] Letters { get { return m_letters; } }
		[SerializeField]
		List<AudioSource> m_audio_sources;			// List of AudioSources used for sound effects
#if !UNITY_5_4_OR_NEWER
		[SerializeField]
		List<ParticleEmitter> m_particle_emitters;
#endif
		[SerializeField]
		List<ParticleSystem> m_particle_systems;
		[SerializeField]
		List<ParticleEffectInstanceManager> m_particle_effect_managers;
		[SerializeField]
		GameObject m_gameObect;
		[SerializeField]
		Transform m_transform;
		[SerializeField]
		TextFxAnimationInterface m_animation_interface_reference;
		[SerializeField]
		MonoBehaviour m_monobehaviour;
		[SerializeField]
		int m_num_letters = 0;
		[SerializeField]
		int m_num_extra_verts_per_letter = 0;
		[SerializeField]
		List<float> m_textWidths;
		[SerializeField]
		List<float> m_textHeights;

		public float TextWidth(int lineIndex) { return m_textWidths[lineIndex]; }
		public float TextHeight(int lineIndex) { return m_textHeights[lineIndex]; }
		public float TextWidthScaled(int lineIndex) { return m_textWidths[lineIndex] / MovementScale; }
		public float TextHeightScaled(int lineIndex) { return m_textHeights[lineIndex] / MovementScale; }


		[SerializeField]
		Vector3[] m_current_mesh_verts;
		[SerializeField]
		Color[] m_current_mesh_colours;

		[SerializeField]
		string m_current_text = "";
		public string CurrentText { get { return m_current_text; } }

		[SerializeField]
		int m_num_words = 0;
		[SerializeField]
		int m_num_lines = 0;
		
		List<int> m_letters_not_animated = null;
		float m_last_time = 0;
		float m_animation_timer = 0;
		int m_lowest_action_progress = 0;
		float m_runtime_animation_speed_factor = 1;		// Set by continue anim speed override option
		bool m_running = false;
		bool m_paused = false;
		System.Action m_animation_callback = null;	// Callback called after animation has finished
		ANIMATION_DATA_TYPE m_what_just_changed = ANIMATION_DATA_TYPE.NONE;		// A record of the last thing to have been updated in this LetterAction
		System.Action<int> m_animation_continue_callback = null;

		int m_dataRebuildCallFrame = -1;

		public void SetDataRebuildCallFrame()
		{
			m_dataRebuildCallFrame = Time.frameCount;
		}

		public PresetAnimationSection m_preset_intro;
		public PresetAnimationSection m_preset_main;
		public PresetAnimationSection m_preset_outro;
		
		public int IntroRepeatLoopStartIndex { get { return m_preset_outro.m_start_loop + m_preset_outro.m_num_loops; } }
		public int MainRepeatLoopStartIndex { get { return IntroRepeatLoopStartIndex + (m_preset_intro.m_active && m_preset_intro.m_repeat ? 1  : 0); } }
		public int OutroRepeatLoopStartIndex { get { return MainRepeatLoopStartIndex + (m_preset_main.m_active && m_preset_main.m_repeat ? 1  : 0); } }
		public int GlobalRepeatLoopStartIndex { get { return OutroRepeatLoopStartIndex + (m_preset_outro.m_active && m_preset_outro.m_repeat ? 1  : 0); } }

		public bool m_repeat_all_sections = false;
		public int m_repeat_all_sections_count = 0;

#if UNITY_EDITOR

		// Editor only variables
		bool m_previewing_anim = false;			// Denotes whether the animation is currently being previewed in the editor
		bool m_preview_paused = false;
		int m_editor_action_idx = 0;
		float m_editor_action_progress = 0;

		public bool PreviewingAnimationInEditor { get { return m_previewing_anim; } set { m_previewing_anim = value; } }
		public bool PreviewAnimationPaused { get { return m_preview_paused; } set { m_preview_paused = value; m_paused = value; } }
		public int EditorActionIdx { get { return m_editor_action_idx; } }
		public float EditorActionProgress { get { return m_editor_action_progress; } }

		public List<PresetEffectSetting> m_preset_effect_settings;

		public bool m_using_quick_setup = false;
		public int m_selected_intro_animation_idx;
		public int m_selected_main_animation_idx;
		public int m_selected_outro_animation_idx;
		public bool m_intro_animation_foldout = false;
		public bool m_main_animation_foldout = false;
		public bool m_outro_animation_foldout = false;

		public string m_effect_name = "";		// A human readible name given to the effect when saved.
		public bool m_import_as_section = false;

		public void PlayEditorAnimation(int starting_action = 0)
		{
			if (m_master_animations == null || m_master_animations.Count == 0)
			{
				Debug.LogWarning("PlayAnimation() called, but no animations defined on '" + GameObject.name + "'");
				return;
			}

			PreviewingAnimationInEditor = true;
			PreviewAnimationPaused = false;

			PlayAnimation(starting_action_index: starting_action);
		}

		public void ResetEditorAnimation()
		{
			if (AnimationInterface == null)
				return;

			PreviewAnimationPaused = false;
			PreviewingAnimationInEditor = false;
			ResetAnimation();

			m_animation_interface_reference.UpdateTextFxMesh();

			SceneView.RepaintAll();
		}

		public bool WipeQuickSetupData(bool user_confirm = false)
		{
			if (!m_using_quick_setup)
				// quick setup data already wiped
				return true;

			if(user_confirm && m_using_quick_setup && !EditorUtility.DisplayDialog("Sure?", "This will break your current Quick Setup pairing, do you still want to continue?", "Yes", "No"))
			{
				return false;
			}

			m_preset_intro.Reset();
			m_preset_main.Reset();
			m_preset_outro.Reset();
			m_selected_intro_animation_idx = 0;
			m_selected_main_animation_idx = 0;
			m_selected_outro_animation_idx = 0;
			m_intro_animation_foldout = false;
			m_main_animation_foldout = false;
			m_outro_animation_foldout = false;
			m_using_quick_setup = false;
			
			Debug.Log("Wipe all Quick Setup data");

			return true;
		}

		public bool WipeFullEditorData(bool user_confirm = false)
		{
			if(m_using_quick_setup)
				// Already wiped Full Editor data
				return true;

			if(user_confirm &&
			   !m_using_quick_setup &&
			   m_master_animations != null &&
			   m_master_animations.Count > 0 &&
			   m_master_animations[0].NumActions > 0 &&
			   !EditorUtility.DisplayDialog("Sure?", "This will delete your existing animation setup in the Full Editor, do you still want to continue?", "Yes", "No"))
			{
				return false;
			}

			m_master_animations = new List<LetterAnimation> () {new LetterAnimation()};
			m_using_quick_setup = true;

			return true;
		}

		static bool m_exitingPlayMode = false;
		static bool m_assignedPlaymodeCallback = false;
		
		public static bool ExitingPlayMode { get { return m_exitingPlayMode; } }

#if UNITY_2017_2_OR_NEWER
		static void PlayModeStateChangedCallback(PlayModeStateChange a_stateChange)
		{
			if(a_stateChange == PlayModeStateChange.ExitingPlayMode)
			{
				m_exitingPlayMode = true;
			}
			else
			{
				m_exitingPlayMode = false;
			}
		}
#else
		static void PlayModeStateChangedCallback()
		{
			if(!EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying)
			{
				m_exitingPlayMode = true;
			}
			else
			{
				m_exitingPlayMode = false;
			}
		}
#endif

#endif

		public TextFxAnimationInterface AnimationInterface {
			get {
				return m_animation_interface_reference;
			}
		}

		public float MovementScale {
			get {
				return m_animation_interface_reference != null ? m_animation_interface_reference.MovementScale : 1f;
			}
		}

		public Transform Transform { get { return m_transform; } }
		public GameObject GameObject { get { return m_gameObect; } }
		public Vector3[] MeshVerts { get { return m_current_mesh_verts; } }
		public Color[] MeshColours { get { return m_current_mesh_colours; } }
		public int NumAnimations { get { return m_master_animations == null ? 0 : m_master_animations.Count; } }
		public ANIMATION_DATA_TYPE WhatJustChanged { get { return m_what_just_changed; } set { m_what_just_changed = value; } }
		public List<LetterAnimation> LetterAnimations { get { 	if(m_master_animations == null)
				m_master_animations = new List<LetterAnimation>();
				return m_master_animations; } }
		
		public bool HasAudioParticleChildInstances {
			get {
				return (
							m_audio_sources != null
#if !UNITY_5_4_OR_NEWER
							&& m_particle_emitters != null
#endif
							&& m_particle_systems != null
						)
						&& (
							m_audio_sources.Count > 0
#if !UNITY_5_4_OR_NEWER
							|| m_particle_emitters.Count > 0
#endif
							|| m_particle_systems.Count > 0
						);
			}
		}
		public List<ParticleEffectInstanceManager> ParticleEffectManagers { get { return m_particle_effect_managers; } }

		public int NumVertsPerLetter { get { return 4 + m_num_extra_verts_per_letter; } }

		public Vector3 Position { get { return m_transform.position; } }
		public Vector3 Scale { get { return m_transform.lossyScale; } }

		public float AnimationTimer { get { return m_animation_timer; } }

		public bool Playing { get { return m_running; } }
		public bool Paused
		{
			get
			{
				return m_paused;
			}
			set
			{
				m_paused = value;
				
				PauseAllParticleEffects(m_paused);
			}
		}

		public TextFxAnimationManager()
		{
		}

		void Init(MonoBehaviour monoInstance)
		{
			if(m_master_animations == null)
				m_master_animations = new List<LetterAnimation>();

#if UNITY_EDITOR
			if(m_preset_intro == null)
			{
				// Initialise all anim section objects
				m_preset_intro = new PresetAnimationSection();
				m_preset_main = new PresetAnimationSection();
				m_preset_outro = new PresetAnimationSection();
			}

			if(!m_assignedPlaymodeCallback)
			{
				// Assign a callback method for the playmodeStateChanged action
#if UNITY_2017_2_OR_NEWER
				EditorApplication.playModeStateChanged += PlayModeStateChangedCallback;
#else	
				EditorApplication.playmodeStateChanged += PlayModeStateChangedCallback;
#endif

				m_assignedPlaymodeCallback= true;
			}
#endif
		}

		public void OnStart()
		{
			// Force text into animation starting state
			if(m_master_animations != null && m_master_animations.Count > 0)
			{
				SetAnimationState(0, 0, false, ANIMATION_DATA_TYPE.NONE, update_mesh: true);
			} 

			if(m_begin_on_start)
			{
				PlayAnimation(m_begin_delay);
			}
		}

		public void SetParentObjectReferences(GameObject gameObject, Transform transform, TextFxAnimationInterface anim_interface)
		{
			m_gameObect = gameObject;
			m_transform = transform;
			m_animation_interface_reference = anim_interface;
			m_monobehaviour = (MonoBehaviour) anim_interface;

			Init (m_monobehaviour);
		}




//#if UNITY_EDITOR

		// Cycles through preset anim sections to update the start/end action index data
		public void UpdatePresetAnimSectionActionIndexes()
		{
			int action_idx = 0;
			int loop_idx = 0;
			
			m_preset_intro.m_start_action = 0;
			if(m_preset_intro.m_active)
				action_idx += m_preset_intro.m_num_actions + 1;		// Plus one for the exit pause action
			
			m_preset_main.m_start_action = action_idx;
			if(m_preset_main.m_active)
				action_idx += m_preset_main.m_num_actions + 1;		// Plus one for the exit pause action
			
			m_preset_outro.m_start_action = action_idx;
			
			
			// Calculate Loop indexes
			m_preset_intro.m_start_loop = 0;
			loop_idx += m_preset_intro.m_num_loops;
			
			m_preset_main.m_start_loop = loop_idx;
			loop_idx += m_preset_main.m_num_loops;
			
			m_preset_outro.m_start_loop = loop_idx;
		}

		PresetAnimationSection GetPresetAnimationSection(PRESET_ANIMATION_SECTION section)
		{
			switch (section)
			{
				case PRESET_ANIMATION_SECTION.INTRO:
					return m_preset_intro;
				case PRESET_ANIMATION_SECTION.MAIN:
					return m_preset_main;
				case PRESET_ANIMATION_SECTION.OUTRO:
					return m_preset_outro;
				default:
					return null;
			}
		}

		public void SetQuickSetupSection(PRESET_ANIMATION_SECTION section, string animationName = "None", bool setEditorSectionIndex = false, bool forceClearOldAudioParticles = true)
		{
			PresetAnimationSection preset_anim_section = GetPresetAnimationSection (section);

			// Handle removing any existing section anim
			if(preset_anim_section.m_num_actions > 0
			   && m_master_animations != null
			   && m_master_animations.Count > 0
			   && m_master_animations[0].NumActions >= preset_anim_section.m_num_actions + 1)
			{
				m_master_animations[0].RemoveActions(preset_anim_section.m_start_action, preset_anim_section.m_num_actions + 1);
				
				m_master_animations[0].RemoveLoops(preset_anim_section.m_start_loop, preset_anim_section.m_num_loops);
			}
			
			// Animation selection changed. Update animation to reflect this
			if(animationName != "None")
			{
				preset_anim_section.m_repeat = false;
				preset_anim_section.m_repeat_count = 0;

				string effectJsonDataString = TextFxQuickSetupAnimConfigs.GetConfig(section, animationName);

				if(effectJsonDataString != "")
				{
					preset_anim_section.m_active = true;
					
					ImportData(effectJsonDataString, preset_anim_section, section, forceClearOldAudioParticles);
					
					// Add in an Exit Pause action after the end of this section
					LetterAction exit_pause_action = new LetterAction();
					exit_pause_action.m_action_type = ACTION_TYPE.BREAK;
					
					// Initialise delay duration based on existing settings
					exit_pause_action.m_duration_progression.SetConstant(preset_anim_section.m_exit_pause ? preset_anim_section.m_exit_pause_duration : INACTIVE_EXIT_PAUSE_DURATION);
					
					m_master_animations[0].InsertAction(preset_anim_section.ExitPauseIndex, exit_pause_action);
					
					PrepareAnimationData (ANIMATION_DATA_TYPE.DURATION);
				}
			}
			else
			{
				preset_anim_section.Reset();

				PrepareAnimationData ();

				ResetAnimation();

				// Update mesh
				m_animation_interface_reference.UpdateTextFxMesh();

#if UNITY_EDITOR
				SceneView.RepaintAll();
#endif
			}
			
			UpdatePresetAnimSectionActionIndexes();
			
			// Update global loop settings if active
			if(m_repeat_all_sections)
			{
				ActionLoopCycle global_loop = m_master_animations[0].GetLoop(GlobalRepeatLoopStartIndex);
				
				if(global_loop == null)
				{
					// Global loop was removed during section rearranging. Re-add one in.
					global_loop = new ActionLoopCycle();
					global_loop.m_number_of_loops = m_repeat_all_sections_count;
					m_master_animations[0].InsertLoop(GlobalRepeatLoopStartIndex, global_loop, true);
				}
				
				global_loop.m_start_action_idx = 0;
				global_loop.m_end_action_idx = m_preset_outro.m_start_action + (m_preset_outro.m_active ? m_preset_outro.m_num_actions : -1);
			}
		}
//#endif

        public string GetRawTextString(string text)
        {
            string rawText = text.Replace("\n", "");
            rawText = text.Replace("\r", "");
            rawText = text.Replace("\t", "");
            rawText = text.Replace(" ", "");

            return rawText;
        }

        // Sets up LetterSetup instances for each letter in the current text
        public void UpdateText(string text_string, GuiTextDataHandler textData, bool white_space_meshes)
		{
			List<LetterSetup> letter_list = new List<LetterSetup> ();
			LetterSetup new_letter_setup;
			char current_char = '\n';
			int letter_index = 0;
			int word_idx = 0;
			int line_idx = 0;
			bool visible_character;
			bool last_quad_visible = false;
			bool newLine = true;
			float lineWidth;

			float textMinX = 0, textMaxX = 0, textMinY = 0, textMaxY = 0;

			m_num_letters = textData.NumVertsPerLetter > 0 ? textData.NumVerts / textData.NumVertsPerLetter : 0;
			m_num_extra_verts_per_letter = textData.ExtraVertsPerLetter;

			if (m_textWidths == null)
			{
				m_textWidths = new List<float> ();
				m_textHeights = new List<float> ();
			}
			else
			{
				m_textWidths.Clear();
				m_textHeights.Clear();
			}

			LetterSetup lastExistingVisibleLetter = null;

			for (int char_idx = 0; char_idx < text_string.Length; char_idx++)
			{
				current_char = text_string[char_idx];

				visible_character = ! ( current_char.Equals(' ') || current_char.Equals('\n') || current_char.Equals('\r') || current_char.Equals('\t'));

				if (current_char.Equals ('\n') || current_char.Equals ('\r'))
				{
					lineWidth = textMaxX - textMinX;
					m_textWidths.Add(lineWidth);
					m_textHeights.Add(textMaxY - textMinY);

					line_idx++;
					
					newLine = true;
				}

				if(last_quad_visible && !visible_character)
					word_idx++;

				last_quad_visible = visible_character;


				// Reuse an existing letter mesh or create a new one if none available
				if(m_letters != null && char_idx < m_letters.Length && !m_letters[char_idx].StubInstance && m_letters[char_idx].VisibleCharacter)
				{
					new_letter_setup = m_letters[char_idx];

					if(visible_character)
					{
						lastExistingVisibleLetter = new_letter_setup;
					}
				}
				else
					// Setup a new LetterSetup instance based off of the last letter from previous text, if available
					new_letter_setup = new LetterSetup(this, lastExistingVisibleLetter);


				new_letter_setup.SetWordLineIndex(word_idx, line_idx);


				if(!white_space_meshes && !visible_character)
				{
					// Creating a white space character LetterSetup stub
					new_letter_setup.SetAsStubInstance();
				}
				else
				{
					if(letter_index >= m_num_letters)
					{
						// No mesh available for this letter; which can be caused by it being clipped off
						break;
					}

					new_letter_setup.SetLetterData(	textData.GetLetterBaseVerts(letter_index),
					                               	textData.GetLetterBaseCols(letter_index), 
													textData.GetLetterExtraVerts(letter_index),
													textData.GetLetterExtraCols(letter_index),
													letter_index);

					if(visible_character)
					{
						if(newLine || new_letter_setup.BaseVerticesBL.x < textMinX)
							textMinX = new_letter_setup.BaseVerticesBL.x;
						if(newLine || new_letter_setup.BaseVerticesBR.x > textMaxX)
							textMaxX = new_letter_setup.BaseVerticesBR.x;
						
						if(newLine || new_letter_setup.BaseVerticesBL.y < textMinY)
							textMinY = new_letter_setup.BaseVerticesBL.y;
						if(newLine || new_letter_setup.BaseVerticesTL.y > textMaxY)
							textMaxY = new_letter_setup.BaseVerticesTL.y;

						newLine = false;
					}

					letter_index++;
				}

				// Set whether letter is a visible character or not
				new_letter_setup.VisibleCharacter = visible_character;

				letter_list.Add(new_letter_setup);
			}

			lineWidth = textMaxX - textMinX;
			m_textWidths.Add(lineWidth);
			m_textHeights.Add(textMaxY - textMinY);


			m_num_words = word_idx + (last_quad_visible ? 1 : 0);
			m_num_lines = line_idx + (current_char != '\n' && current_char != '\r' ? 1 : 0);

			int prev_num_letters = m_letters != null ? m_letters.Length : 0;

		
			// Update letters array
			m_letters = letter_list.ToArray ();


			// Check to see if animation data needs to be re-calculated
			if(m_current_text != text_string || prev_num_letters != m_letters.Length)
			{
				// Calculate action progression values for letters 
				PrepareAnimationData();
			}
			else
			{
				PrepareAnimationDefaultColour();
			}

			// Populate the default mesh data
			PopulateDefaultMeshData(true);

			// Keep track of the current text
			m_current_text = text_string;
		}

		bool m_curveDataApplied = false;

		public bool UsingBezierCurve
		{
			get
			{
				return m_animation_interface_reference == null ? false : m_animation_interface_reference.RenderToCurve;
			}
		}

		public bool CheckCurveData()
		{
			// Handle curve position offset
			if (UsingBezierCurve)
			{
				TextFxBezierCurve bezierCurve = m_animation_interface_reference.BezierCurve;

				float[] curveLetterProgressions = bezierCurve.GetLetterProgressions (this, ref m_letters, m_animation_interface_reference.TextAlignment);
				LetterSetup letter;
				Vector3 curveBaseLetterPosition = Vector3.zero;
				Vector3 curveBaseLetterRotation = Vector3.zero;

				for (int letter_idx = 0; letter_idx < m_letters.Length; letter_idx++)
				{
					letter = m_letters [letter_idx];

					if (!letter.StubInstance)
					{
						curveBaseLetterPosition = Vector3.zero;
						curveBaseLetterRotation = Vector3.zero;

						if (curveLetterProgressions != null)
						{
							curveBaseLetterPosition = bezierCurve.GetCurvePoint (	curveLetterProgressions [letter_idx],
																					yOffset: (bezierCurve.m_baselineOffset + letter.BaseVerticesCenter.y) * (1f / m_animation_interface_reference.MovementScale));
							curveBaseLetterRotation = bezierCurve.GetCurvePointRotation (curveLetterProgressions [letter_idx]);

							letter.OffsetLetterData (curveBaseLetterPosition * m_animation_interface_reference.MovementScale, curveBaseLetterRotation);
						}
					}
				}

				m_curveDataApplied = true;

				return true;
			}
			else if(m_curveDataApplied)
				ClearCurveData();

			

			return false;
		}

		public void ClearCurveData()
		{
			if (!m_curveDataApplied)
				return;

			for (int letter_idx = 0; letter_idx < m_letters.Length; letter_idx++)
			{
				m_letters [letter_idx].ClearOffsetData ();
			}

			m_curveDataApplied = false;
		}


		// Calculates values for all animation state progressions using current field values.
		public void PrepareAnimationData(ANIMATION_DATA_TYPE what_to_update = ANIMATION_DATA_TYPE.ALL)
		{
//			Debug.Log ("PrepareAnimationData () " + what_to_update);

			if(m_master_animations != null)
			{
				if (m_letters_not_animated == null)
					m_letters_not_animated = new List<int> ();
				m_letters_not_animated.Clear ();

				// Initialise list with all letters
				for (int letterIdx = 0; letterIdx < m_letters.Length; letterIdx++)
				{
					if(m_letters[letterIdx].VisibleCharacter)
						m_letters_not_animated.Add (letterIdx);
				}


				// Prepare animation data
				foreach(LetterAnimation animation in m_master_animations)
				{
					animation.PrepareData(this, m_letters, what_to_update, m_num_words, m_num_lines, m_animate_per);	// Requires info about number letters, words, lines etc

					for (int idx = 0; idx < animation.m_letters_to_animate.Count; idx++)
					{
						// Remove animated letter index from the list
						m_letters_not_animated.Remove (animation.m_letters_to_animate [idx]);
					}
				}



				// Prepare mesh data for any non-animated letters in their default states
				if (m_letters_not_animated != null && m_letters_not_animated.Count > 0)
				{
					// Fill in default text mesh data, before overriding with any animated mesh data
					PopulateDefaultMeshData ();

					LetterSetup letter_setup;
					
					foreach (int letterIdx in m_letters_not_animated)
					{
						letter_setup = m_letters [letterIdx];

						Vector3[] letter_verts = new Vector3[NumVertsPerLetter];
						for(int lIdx=0; lIdx < NumVertsPerLetter; lIdx++)
							letter_verts[lIdx] = m_current_mesh_verts[lIdx < m_num_extra_verts_per_letter
																		? letter_setup.MeshIndex * m_num_extra_verts_per_letter + lIdx
																		: (m_num_letters * m_num_extra_verts_per_letter) + (letter_setup.MeshIndex * 4) + (lIdx - m_num_extra_verts_per_letter)];
						
						
						Color[] letter_colours = new Color[NumVertsPerLetter];
						for(int lIdx=0; lIdx < NumVertsPerLetter; lIdx++)
							letter_colours[lIdx] = m_current_mesh_colours[lIdx < m_num_extra_verts_per_letter
																		? letter_setup.MeshIndex * m_num_extra_verts_per_letter + lIdx
																		: (m_num_letters * m_num_extra_verts_per_letter) + (letter_setup.MeshIndex * 4) + (lIdx - m_num_extra_verts_per_letter)];


						m_letters[letterIdx].SetMeshState(this, -1, 0, null, m_animate_per, ref letter_verts, ref letter_colours);


						// Update the verts for this letter
						for(int idx=0; idx < NumVertsPerLetter; idx++)
						{
							m_current_mesh_verts[ idx < m_num_extra_verts_per_letter
								? letter_setup.MeshIndex * m_num_extra_verts_per_letter + idx
								: (m_num_letters * m_num_extra_verts_per_letter) + (letter_setup.MeshIndex * 4) + (idx - m_num_extra_verts_per_letter)] = letter_verts[idx];
						}

						// Set Colours
						int currentMeshColourIdx;
						for(int idx=0; idx < NumVertsPerLetter; idx++)
						{
							currentMeshColourIdx = idx < m_num_extra_verts_per_letter
								? (letter_setup.MeshIndex * m_num_extra_verts_per_letter) + idx
								: (m_num_letters * m_num_extra_verts_per_letter) + (letter_setup.MeshIndex * 4) + (idx - m_num_extra_verts_per_letter);

							m_current_mesh_colours[ currentMeshColourIdx ] = letter_colours[idx];
						}
					}
				}



				if(Playing)
				{
					m_what_just_changed = what_to_update;
				}
			}
		}

		void PrepareAnimationDefaultColour()
		{
			if(m_master_animations != null)
			{
				foreach(LetterAnimation animation in m_master_animations)
				{
					animation.RefreshDefaultTextColour(m_letters);
				}
			}
		}


		public void PlayAnimation(System.Action animation_callback)
		{
			m_animation_callback = animation_callback;
			
			PlayAnimation();
		}
		
		public void PlayAnimation(float delay, System.Action animation_callback)
		{
			m_animation_callback = animation_callback;
			
			PlayAnimation(delay);
		}

		public void PlayAnimation(float delay = 0, int starting_action_index = 0)
		{
			if (m_master_animations == null || m_master_animations.Count == 0)
			{
				Debug.LogWarning ("PlayAnimation() called on '" + m_gameObect.name + "', but no animation defined.");
				return;
			}
			else if (m_gameObect == null)
			{
				// Lost reference to the gameObject
				return;
			}
			else if (m_gameObect.activeSelf == false)
			{
				Debug.LogWarning ("PlayAnimation() called on '" + m_gameObect.name + "', but the gameObject is inactive.");
				return;
			}
			
			int num_letters = m_letters.Length;

			m_audio_sources = new List<AudioSource>(m_gameObect.GetComponentsInChildren<AudioSource>());
#if !UNITY_5_4_OR_NEWER
			m_particle_emitters = new List<ParticleEmitter>(m_gameObect.GetComponentsInChildren<ParticleEmitter>());
#endif
			m_particle_systems = new List<ParticleSystem>(m_gameObect.GetComponentsInChildren<ParticleSystem>());
			m_particle_effect_managers = new List<ParticleEffectInstanceManager>();
			
			// Stop all audio sources and particle effects
			foreach(AudioSource a_source in m_audio_sources)
			{
				a_source.Stop();
			}

#if !UNITY_5_4_OR_NEWER
			foreach(ParticleEmitter p_emitter in m_particle_emitters)
			{
				p_emitter.emit = false;
				p_emitter.particles = null;
				p_emitter.enabled = false;
			}
#endif
			
			foreach(ParticleSystem p_system in m_particle_systems)
			{
				p_system.Stop();
				p_system.Clear();
			}
			
			// Prepare Master Animations data and check for particle effect onstart reset
			bool reset_mesh = false;
			foreach(LetterAnimation animation in m_master_animations)
			{
				animation.CurrentAnimationState = LETTER_ANIMATION_STATE.PLAYING;

				foreach(int letter_idx in animation.m_letters_to_animate)
				{
					if(letter_idx < num_letters)
					{
						m_letters[letter_idx].Reset(animation, starting_action_index);
						m_letters[letter_idx].Active = true;
					}
				}

				// Force letter start positions reset before playing, to avoid onStart particle effect positioning errors
				if(!reset_mesh && animation.NumActions > 0 && animation.GetAction(starting_action_index).NumParticleEffectSetups > 0 )
				{
					foreach(ParticleEffectSetup effect_setup in animation.GetAction(starting_action_index).ParticleEffectSetups)
					{
						if(effect_setup.m_play_when == PLAY_ITEM_EVENTS.ON_START)
						{
							UpdateMesh(false, true, starting_action_index, 0);
							
							reset_mesh = true;
						}
					}
				}
			}
			
			m_lowest_action_progress = 0;
			m_animation_timer = 0;
			m_runtime_animation_speed_factor = 1;
			m_animation_continue_callback = null;

			if (m_dataRebuildCallFrame > -1)
			{
				if (m_dataRebuildCallFrame == Time.frameCount)
					// Delay by a fraction of time (one frame), in order to allow all rendering data to catch up
					delay = 0.00001f;
				
				m_dataRebuildCallFrame = -1;
			}

			if(delay > 0)
			{
				m_monobehaviour.StartCoroutine(PlayAnimationAfterDelay(delay));
			}
			else
			{
				if (m_time_type == AnimationTime.REAL_TIME || !Application.isPlaying)
				{
					m_last_time = Time.realtimeSinceStartup;
				}
				else
				{
					m_last_time = Time.time;
				}
				
				m_running = true;
				m_paused = false;
			}
		}

		IEnumerator PlayAnimationAfterDelay(float delay)
		{
			yield return m_monobehaviour.StartCoroutine(TimeDelay(delay, m_time_type));
			
			if (m_time_type == AnimationTime.REAL_TIME || !Application.isPlaying)
			{
				m_last_time = Time.realtimeSinceStartup;
			}
			else
			{
				m_last_time = Time.time;
			}
			
			m_running = true;
			m_paused = false;
		}
		
		// Reset animation to starting state
		public void ResetAnimation()
		{
			UpdateMesh(false, true, 0, 0);
			
			foreach(LetterSetup letter in m_letters)
			{
				letter.AnimStateVars.Reset();
			}
			
			m_running = false;
			m_paused = false;
			m_lowest_action_progress = 0;
			m_animation_timer = 0;
			m_runtime_animation_speed_factor = 1;
			m_animation_continue_callback = null;
			
			StopAllParticleEffects(true);

			// Force the mesh to update
			if(m_animation_interface_reference != null)
				m_animation_interface_reference.UpdateTextFxMesh();
		}

		// Set Text Effect to its end state
		public void SetEndState()
		{
			//if (m_master_animations == null)
			//	return;

			int longest_action_list = 0;
			int chosenAnimIdx = 0;

			for (int idx=0; idx < m_master_animations.Count; idx++)
			{
				if (m_master_animations[idx].NumActions > longest_action_list)
				{
					longest_action_list = m_master_animations[idx].NumActions;
					chosenAnimIdx = idx;
                }
			}

			int endActionIndex = 0;

			LetterAnimation anim = m_master_animations[chosenAnimIdx];
			

			// Check for BREAK action type, and if so, get first non-break action before it.
			for (int idx=longest_action_list - 1; idx >= 0; idx--)
			{
                if (anim.GetAction(idx).m_action_type == ACTION_TYPE.ANIM_SEQUENCE)
				{
					endActionIndex = idx;
					break;
                }
            }

			SetAnimationState(endActionIndex, 1);

			m_running = false;
			m_paused = false;
			m_lowest_action_progress = 0;
			m_animation_timer = 0;
			m_runtime_animation_speed_factor = 1;
			m_animation_continue_callback = null;

			StopAllParticleEffects(true);

			// Force the mesh to update
			m_animation_interface_reference.UpdateTextFxMesh();
		}


		public bool UpdateAnimation(float deltaTimeOverride = -1)
		{
			float delta_time;

			if (deltaTimeOverride > 0)
				delta_time = deltaTimeOverride;
			else
				delta_time= m_time_type == AnimationTime.GAME_TIME && Application.isPlaying ? Time.time - m_last_time : Time.realtimeSinceStartup - m_last_time;

			// Catch massive delta_time values caused by pauses/breaks in playback, resulting in an old m_last_time being used.
			if(delta_time > 0.25f && deltaTimeOverride < 0)
			{
				delta_time = 1f / 30f;
			}

			if (m_time_type == AnimationTime.REAL_TIME || !Application.isPlaying)
			{
				m_last_time = Time.realtimeSinceStartup;
			}
			else
			{
				m_last_time = Time.time;
			}

			if(m_paused && deltaTimeOverride < 0)
				return m_running;

			// Adjust by animation speed factor value
			delta_time *= (m_runtime_animation_speed_factor * m_animation_speed_factor);

			m_animation_timer += delta_time;
			
			if(m_running && UpdateMesh(true, false, 0,0, delta_time))
			{
				m_running = false;

				// Call to the animation-complete callback if assigned
				if(m_animation_callback != null)
				{
					m_animation_callback();
				}
				
				// Execute on finish action requested
				if(Application.isPlaying)
				{
					if(m_on_finish_action == ON_FINISH_ACTION.DESTROY_OBJECT)
					{
						GameObject.Destroy(m_gameObect);
					}
					else if(m_on_finish_action == ON_FINISH_ACTION.DISABLE_OBJECT)
					{
	#if !UNITY_3_5
						m_gameObect.SetActive(false);
	#else
						m_gameObect.SetActiveRecursively(false);
	#endif
					}
					else if(m_on_finish_action == ON_FINISH_ACTION.RESET_ANIMATION)
					{
						ResetAnimation();
					}
				}
			}
			
			if(m_particle_effect_managers.Count > 0)
			{
				for(int idx=0; idx < m_particle_effect_managers.Count; idx++)
				{
					if(m_particle_effect_managers[idx].Update(delta_time))
					{
						// particle effect instance is complete
						// Remove from list
						
						m_particle_effect_managers.RemoveAt(idx);
						idx --;
					}
				}
			}
			
			return m_running;
		}


		
		public void SetAnimationState(int action_idx, float action_progress, bool update_action_values = false, ANIMATION_DATA_TYPE edited_data = ANIMATION_DATA_TYPE.ALL, bool update_mesh = false)
		{
			if(update_action_values)
			{
				// Calculate action progression values
				PrepareAnimationData(edited_data);
			}

			UpdateMesh(false, true, action_idx, action_progress);

			if(update_mesh)
			{
				m_animation_interface_reference.UpdateTextFxMesh( );
			}
		}

		// Continues current animation past any WAITING states
		public void ContinuePastBreak(bool onlyIfAllLettersWaiting = false)
		{
			ContinuePastBreak (0, onlyIfAllLettersWaiting);
		}

		public void ContinuePastBreak(int animationIndex, bool onlyIfAllLettersWaiting = false)
		{
			if (animationIndex < 0 || m_master_animations == null || animationIndex >= m_master_animations.Count)
				return;

			LetterAnimation animation = m_master_animations[animationIndex];

			if(onlyIfAllLettersWaiting && animation.CurrentAnimationState != LETTER_ANIMATION_STATE.WAITING && animation.CurrentAnimationState != LETTER_ANIMATION_STATE.WAITING_INFINITE)
				// Not all letters are in a waiting state at a BREAK action, so don't continue
				return;

			// Continue each waiting letter
			foreach(int letter_idx in animation.m_letters_to_animate)
			{
				// letter is in a waiting state. Continue it beyond this wait state.
				if(m_letters[letter_idx].CurrentAnimationState == LETTER_ANIMATION_STATE.WAITING || m_letters[letter_idx].CurrentAnimationState == LETTER_ANIMATION_STATE.WAITING_INFINITE)
					m_letters[letter_idx].ContinueAction(m_animation_timer, animation, m_animate_per);
			}
			
			return;
		}


		/// <summary>Continues the current animation out of a current loop</summary>
		/// <param name="continueType">Denotes the action to be taken in order to continue the animation.</param>
		/// <param name="lerpSyncDuration"> The duration of the Instant continue lerp into the next state. Ignored if <paramref name="continueType"/> is set to EndOfLoop</param>
		/// <param name="passNextInfiniteLoop"> If muliple loops are currently active, then the next Infinite loop will be the loop that is continued</para>
		/// <param name="trimInterimLoops"> Any loop within the infinite loop that is being skipped, will be set to it's last iteration </param>
		/// <param name="animationSpeedOverride"> If continuing once all letters are at the end of the loop, this can be used to override the animation speed during this period. Default value is 1. </param>
		public void ContinuePastLoop(ContinueType continueType = ContinueType.EndOfLoop,
		                             float lerpSyncDuration = 0.5f,
		                             bool passNextInfiniteLoop = true,
		                             bool trimInterimLoops = true,
		                             float animationSpeedOverride = 1,
		                             System.Action<int> finishedCallback = null)
		{
			ContinuePastLoop (0, continueType, lerpSyncDuration, passNextInfiniteLoop, trimInterimLoops, animationSpeedOverride, finishedCallback);
		}


		/// <summary>Continues the current animation out of a current loop</summary>
		/// <param name="animation_index">Index of the animation to continue. Most the time this is 0</param>
		/// <param name="continueType">Denotes the action to be taken in order to continue the animation.</param>
		/// <param name="lerpSyncDuration"> The duration of the Instant continue lerp into the next state. Ignored if <paramref name="continueType"/> is set to EndOfLoop</param>
		/// <param name="passNextInfiniteLoop"> If muliple loops are currently active, then the next Infinite loop will be the loop that is continued</para>
		/// <param name="trimInterimLoops"> Any loop within the infinite loop that is being skipped, will be set to it's last iteration </param>
		/// <param name="animationSpeedOverride"> If continuing once all letters are at the end of the loop, this can be used to override the animation speed during this period. Default value is 1. </param>
		public void ContinuePastLoop(int animation_index,
		                             ContinueType continueType = ContinueType.EndOfLoop,
		                             float lerpSyncDuration = 0.5f,
		                             bool passNextInfiniteLoop = true,
		                             bool trimInterimLoops = true,
		                             float animationSpeedOverride = 1,
		                             System.Action<int> finishedCallback = null)
		{
			if (animation_index < 0 || m_master_animations == null || animation_index >= m_master_animations.Count)
				return;

			LetterAnimation animation = m_master_animations[animation_index];

			int deepestLoopDepth = -1;
			ActionLoopCycle deepestLoopCycle = null;
			LetterSetup deepestLoopLetter = null;
			int furthestActionIndex = -1;
			int furthestActionIndexProgress = -1;
			int[] lowestActiveLoopIterations = new int[animation.ActionLoopCycles.Count];
			bool allLettersHaveLoops = true;

			LetterSetup letter;
			foreach(int letter_idx in animation.m_letters_to_animate)
			{
				letter = m_letters[letter_idx];

				if(furthestActionIndex == -1 || letter.ActionIndex > furthestActionIndex)
					furthestActionIndex = letter.ActionIndex;

				if(furthestActionIndexProgress == -1 || letter.ActionProgress > furthestActionIndexProgress)
					furthestActionIndexProgress = letter.ActionProgress;

				if(letter.ActiveLoopCycles.Count > 0)
				{
					// Record lowest active loop iteration counts for later syncing
					for(int loop_cycle_index = 0; loop_cycle_index < letter.ActiveLoopCycles.Count; loop_cycle_index++)
					{
						ActionLoopCycle loop_cycle = letter.ActiveLoopCycles[loop_cycle_index];

						if(lowestActiveLoopIterations[loop_cycle.m_active_loop_index] == 0 || loop_cycle.m_number_of_loops < lowestActiveLoopIterations[loop_cycle.m_active_loop_index])
						{
							lowestActiveLoopIterations[loop_cycle.m_active_loop_index] = loop_cycle.m_number_of_loops;
						}
					}
				}

				if(letter.ActiveLoopCycles.Count > 0 && (deepestLoopDepth == -1 || letter.ActiveLoopCycles.Count < deepestLoopDepth))
				{
					// Record this new deepest loop cycle
					deepestLoopDepth = letter.ActiveLoopCycles.Count;
					deepestLoopCycle = letter.ActiveLoopCycles[0];
					deepestLoopLetter = letter;
				}
				else if (letter.ActiveLoopCycles.Count == 0)
					allLettersHaveLoops = false;
			}

			int action_index_to_continue_to = -1;
			int action_progress = 0;

			if(deepestLoopCycle == null)
			{
				// These letters are not currently in any loops. Nothing to Continue Past
				return;
			}
			else
			{
				if(deepestLoopCycle.m_end_action_idx + 1 < animation.LetterActions.Count)
				{
					int offsetIdx = 1;
					while(animation.GetAction(deepestLoopCycle.m_end_action_idx + offsetIdx).m_action_type == ACTION_TYPE.BREAK)
					{
						if(deepestLoopCycle.m_end_action_idx + offsetIdx + 1 >= animation.LetterActions.Count)
						{
							break;
						}
						else
							offsetIdx++;

					}

					if(animation.GetAction(deepestLoopCycle.m_end_action_idx + offsetIdx).m_action_type != ACTION_TYPE.BREAK)
					{
						// There's a LetterAction defined beyond the end of this deepest loop.
						// Continue to the start of that letterAction
						action_index_to_continue_to = deepestLoopCycle.m_end_action_idx + offsetIdx;
						action_progress = 0;
					}
				}

				if(deepestLoopCycle.m_end_action_idx + 1 == animation.LetterActions.Count || action_index_to_continue_to == -1)
				{
					// This loop finishes with the last LetterAction, or it failed to find a non-break action beyond the end of the loop

					int start_action_index = (deepestLoopCycle.m_loop_type == LOOP_TYPE.LOOP || deepestLoopCycle.m_finish_at_end) ? deepestLoopCycle.m_end_action_idx : deepestLoopCycle.m_start_action_idx;
					int direction = (deepestLoopCycle.m_loop_type == LOOP_TYPE.LOOP || deepestLoopCycle.m_finish_at_end) ? -1 : 1;
					int idx = 0;

					while(start_action_index + (direction * idx) >= deepestLoopCycle.m_start_action_idx &&
					      start_action_index + (direction * idx) <= deepestLoopCycle.m_end_action_idx &&
					      animation.GetAction(start_action_index + (direction * idx)).m_action_type == ACTION_TYPE.BREAK)
					{
						idx++;
					}

					if(start_action_index + (direction * idx) >= deepestLoopCycle.m_start_action_idx &&
					   start_action_index + (direction * idx) <= deepestLoopCycle.m_end_action_idx)
					{
						action_index_to_continue_to = start_action_index + (direction * idx);
						action_progress = (deepestLoopCycle.m_loop_type == LOOP_TYPE.LOOP || deepestLoopCycle.m_finish_at_end) ? 1 : 0;
					}
				}

				if(!allLettersHaveLoops && furthestActionIndex > ((deepestLoopCycle.m_loop_type == LOOP_TYPE.LOOP || deepestLoopCycle.m_finish_at_end) ? deepestLoopCycle.m_end_action_idx : deepestLoopCycle.m_start_action_idx))
				{
					// Some letters are without loops and are beyond the letters with loops
					action_index_to_continue_to = furthestActionIndex;
					action_progress = 1;

					deepestLoopDepth = 0;
				}
				else
				{
					// Remove any loops that have been passed by stepping through to find an appropriate action to continue to
					for(int loop_idx = 1; loop_idx < deepestLoopLetter.ActiveLoopCycles.Count; loop_idx++)
					{
						if(action_index_to_continue_to > deepestLoopLetter.ActiveLoopCycles[loop_idx].m_end_action_idx)
						{
							// remove this loop cycle too
							deepestLoopDepth --;
						}
					}
				}

			
				if(passNextInfiniteLoop)// && deepestLoopCycle.m_number_of_loops > 0 && deepestLoopDepth > 1)
				{
					bool infiniteLoopAlreadyPassed = false;

					// Check if already passed an infinite loop
					for(int loopDepth = 0; loopDepth < (deepestLoopLetter.ActiveLoopCycles.Count - deepestLoopDepth) + 1; loopDepth ++)
					{
						if(deepestLoopLetter.ActiveLoopCycles[loopDepth].m_number_of_loops <= 0)
						{
							infiniteLoopAlreadyPassed = true;

							break;
						}
					}


					if(!infiniteLoopAlreadyPassed)
					{
						// Supposed to be exiting the next infinite loop, but deepest active loop found isn't infinite
						// Search for deeper infinite active loop
						for(int loopDepth = m_letters[0].ActiveLoopCycles.Count - deepestLoopDepth; loopDepth < m_letters[0].ActiveLoopCycles.Count; loopDepth++)
						{
							if(m_letters[0].ActiveLoopCycles[loopDepth].m_number_of_loops <= 0)
							{
								// Found an infinite loop deeper in the active loops list
								// Set as new deepestLoop
								deepestLoopCycle = m_letters[0].ActiveLoopCycles[loopDepth];
								deepestLoopDepth -= (loopDepth - (m_letters[0].ActiveLoopCycles.Count - deepestLoopDepth));
								deepestLoopLetter = m_letters[0];
							}
						}
					}
				}
			}


//			Debug.Log("action_index_to_continue_to : " +action_index_to_continue_to + ", action_progress : " + action_progress + ", furthestActionIndexProgress : " + furthestActionIndexProgress + ", deepestLoopDepth : "+ deepestLoopDepth);

			// Setup each letter to continue
			foreach(int letter_idx in animation.m_letters_to_animate)
			{
				letter = m_letters[letter_idx];

//				lerpSyncDuration = 0f;
				letter.ContinueFromCurrentToAction(animation,
				                                   action_index_to_continue_to,
				                                   action_progress == 0,
				                                   deepestLoopCycle == null ? action_index_to_continue_to : deepestLoopCycle.m_end_action_idx + 1,
				                                   m_animate_per,
				                                   m_animation_timer,
				                                   lerpSyncDuration,
				                                   furthestActionIndexProgress,
				                                   deepestLoopDepth,
				                                   continueType,
				                                   trimInterimLoops,
				                                   lowestActiveLoopIterations);
			}

			// Set animation speed override
			if(continueType == ContinueType.EndOfLoop)
				m_runtime_animation_speed_factor = animationSpeedOverride;
			
			if(finishedCallback != null)
				m_animation_continue_callback = finishedCallback;


			// Set state to CONTINUING
			animation.CurrentAnimationState = LETTER_ANIMATION_STATE.CONTINUING;
		}

		// Used to fill the MeshVerts and MeshColours data with the default text state
		public void PopulateDefaultMeshData(bool forcePopulate = false)
		{
			if(forcePopulate || m_current_mesh_verts == null || m_current_mesh_verts.Length != m_num_letters * NumVertsPerLetter)
			{

				m_current_mesh_verts = new Vector3[m_num_letters * NumVertsPerLetter];
				m_current_mesh_colours = new Color[m_num_letters * NumVertsPerLetter];


                // Initialise values
                LetterSetup letter;
				Vector3[] base_verts;
				Vector3[] extraVerts;
				Color[] extraVertColours;
				int mesh_idx = 0;
				int vertStartIdx = 0;
				for (int letter_idx = 0; letter_idx < m_letters.Length; letter_idx++)
				{
                    letter = m_letters[letter_idx];

                    if (!letter.StubInstance)
					{
                        base_verts = letter.BaseVertices;
						extraVerts = letter.BaseExtraVertices;
						extraVertColours = letter.BaseExtraColours;
						vertStartIdx = mesh_idx * m_num_extra_verts_per_letter;

						for(int extraVertIdx = 0; extraVertIdx < m_num_extra_verts_per_letter; extraVertIdx++)
						{
							m_current_mesh_verts[vertStartIdx + extraVertIdx] = extraVerts[extraVertIdx];
						}
						for(int extraVertIdx = 0; extraVertIdx < m_num_extra_verts_per_letter; extraVertIdx++)
						{
							m_current_mesh_colours[vertStartIdx + extraVertIdx] = extraVertColours[extraVertIdx];
						}

						vertStartIdx = (m_num_letters * m_num_extra_verts_per_letter) + (mesh_idx * 4);
						m_current_mesh_verts[vertStartIdx] = base_verts[0];
						m_current_mesh_verts[vertStartIdx + 1] = base_verts[1];
						m_current_mesh_verts[vertStartIdx + 2] = base_verts[2];
						m_current_mesh_verts[vertStartIdx + 3] = base_verts[3];


                        m_current_mesh_colours[vertStartIdx + 0] = letter.BaseMeshColour(0);
                        m_current_mesh_colours[vertStartIdx + 1] = letter.BaseMeshColour(1);
                        m_current_mesh_colours[vertStartIdx + 2] = letter.BaseMeshColour(2);
                        m_current_mesh_colours[vertStartIdx + 3] = letter.BaseMeshColour(3);



                        mesh_idx++;
					}
				}
			}
		}

		bool all_letter_anims_finished = true;
		bool all_letter_anims_waiting;
		bool all_letter_anims_waiting_infinitely;
		bool all_letter_anims_continuing_finished;
		int lowest_action_progress = -1;
		int last_letter_idx;

		LetterSetup letter_setup;
		LetterAnimation letterAnimation;
		Vector3[] letter_verts;
		Color[] letter_colours;

		public bool UpdateMesh(bool use_timer, bool force_render, int action_idx = 0, float action_progress = 0, float delta_time = 0)
		{
			all_letter_anims_finished = true;

			// Fill in default text mesh data, before overriding with any animated mesh data
			PopulateDefaultMeshData ();

			if(m_master_animations != null)
			{
				
				lowest_action_progress = -1;

				for(int aIdx = 0; aIdx < m_master_animations.Count; aIdx++)
				{
					letterAnimation = m_master_animations [aIdx];

					last_letter_idx = -1;

					all_letter_anims_waiting = true;
					all_letter_anims_waiting_infinitely = true;
					all_letter_anims_continuing_finished = true;

					if(letterAnimation.m_letters_to_animate == null)
						letterAnimation.m_letters_to_animate = new List<int>();

					int letter_idx;

					for(int lIdx = 0; lIdx < letterAnimation.m_letters_to_animate.Count; lIdx++)
					{
						letter_idx = letterAnimation.m_letters_to_animate [lIdx];

						// two of the same letter index next to each other. Or idx out of bounds.
						if(letter_idx == last_letter_idx || letter_idx >= m_letters.Length)
						{
							continue;
						}

						letter_setup = m_letters[letter_idx];
						
						if(lowest_action_progress == -1 || letter_setup.ActionProgress < lowest_action_progress)
						{
							lowest_action_progress = letter_setup.ActionProgress;
						}

						// Initialise values with existing mesh data
						if(letter_verts == null || letter_verts.Length != NumVertsPerLetter)
							letter_verts = new Vector3[NumVertsPerLetter];
						
						if(letter_colours == null || letter_colours.Length != NumVertsPerLetter)
							letter_colours = new Color[NumVertsPerLetter];
						
						for (int vIdx = 0; vIdx < NumVertsPerLetter; vIdx++)
						{
							letter_verts [vIdx] = m_current_mesh_verts [vIdx < m_num_extra_verts_per_letter
								                                          ? letter_setup.MeshIndex * m_num_extra_verts_per_letter + vIdx
								                                          : (m_num_letters * m_num_extra_verts_per_letter) + (letter_setup.MeshIndex * 4) + (vIdx - m_num_extra_verts_per_letter)];

							letter_colours[vIdx] = m_current_mesh_colours[vIdx < m_num_extra_verts_per_letter
																			? letter_setup.MeshIndex * m_num_extra_verts_per_letter + vIdx
																			: (m_num_letters * m_num_extra_verts_per_letter) + (letter_setup.MeshIndex * 4) + (vIdx - m_num_extra_verts_per_letter)];
						}
						

						if(use_timer)
						{
							letter_setup.AnimateMesh(this,
							                         force_render,
							                         m_animation_timer,
							                         m_lowest_action_progress,
													 letterAnimation,
							                         m_animate_per,
							                         delta_time,
							                         ref letter_verts,
							                         ref letter_colours);

							LETTER_ANIMATION_STATE anim_state = letter_setup.CurrentAnimationState;

							if(anim_state != LETTER_ANIMATION_STATE.CONTINUING_FINISHED)
								all_letter_anims_continuing_finished = false;

							if(anim_state == LETTER_ANIMATION_STATE.STOPPED)
							{
								lowest_action_progress = letter_setup.ActionProgress;
							}
							else
							{
								all_letter_anims_finished = false;
							}

							if(anim_state != LETTER_ANIMATION_STATE.WAITING_INFINITE)
							{
								all_letter_anims_waiting_infinitely = false;
							}

							if(anim_state != LETTER_ANIMATION_STATE.WAITING && anim_state != LETTER_ANIMATION_STATE.WAITING_INFINITE)
							{
								all_letter_anims_waiting = false;
							}
						}
						else
						{
							letter_setup.SetMeshState(this, Mathf.Clamp(action_idx, 0, letterAnimation.NumActions-1), action_progress, letterAnimation, m_animate_per, ref letter_verts, ref letter_colours);
						}

						// Update the verts for this letter
						for(int idx=0; idx < NumVertsPerLetter; idx++)
						{
							m_current_mesh_verts[ idx < m_num_extra_verts_per_letter
							                     	? letter_setup.MeshIndex * m_num_extra_verts_per_letter + idx
							                     	: (m_num_letters * m_num_extra_verts_per_letter) + (letter_setup.MeshIndex * 4) + (idx - m_num_extra_verts_per_letter)] = letter_verts[idx];
						}


						// Set Colours
						int currentMeshColourIdx;
						for(int idx=0; idx < NumVertsPerLetter; idx++)
						{
							currentMeshColourIdx = idx < m_num_extra_verts_per_letter
														? (letter_setup.MeshIndex * m_num_extra_verts_per_letter) + idx
														: (m_num_letters * m_num_extra_verts_per_letter) + (letter_setup.MeshIndex * 4) + (idx - m_num_extra_verts_per_letter);

                            m_current_mesh_colours[ currentMeshColourIdx ] = letter_colours[idx];
						}

						last_letter_idx = letter_idx;
					}
					
					// Set animation state
					if(letterAnimation.m_letters_to_animate.Count > 0)
					{
						if(use_timer)
						{
							if(letterAnimation.CurrentAnimationState == LETTER_ANIMATION_STATE.CONTINUING)
							{
								if(all_letter_anims_continuing_finished)
								{
									letterAnimation.CurrentAnimationState = LETTER_ANIMATION_STATE.PLAYING;

									m_runtime_animation_speed_factor = 1;

									// Set all letters to PLAYING state
									for(int lIdx = 0; lIdx < letterAnimation.m_letters_to_animate.Count; lIdx++)
									{
										letter_idx = letterAnimation.m_letters_to_animate [lIdx];

										m_letters[letter_idx].SetPlayingState();
									}

									// Important to update the lowest action progress to the same consistent value, so that subsequent ForceSameStart actions will work properly
									m_lowest_action_progress = m_letters[0].ActionProgress;

									// Fire off callback
									if(m_animation_continue_callback != null)
										m_animation_continue_callback(m_letters[0].ActionIndex);

									m_animation_continue_callback = null;
								}
							}
							else if(all_letter_anims_waiting_infinitely)
								letterAnimation.CurrentAnimationState = LETTER_ANIMATION_STATE.WAITING_INFINITE;
							else if(all_letter_anims_waiting)
								letterAnimation.CurrentAnimationState = LETTER_ANIMATION_STATE.WAITING;
							else if(!all_letter_anims_finished)
								letterAnimation.CurrentAnimationState = LETTER_ANIMATION_STATE.PLAYING;
						}
					}
					else
					{
						// No letters in this animation, so mark as STOPPED
						letterAnimation.CurrentAnimationState = LETTER_ANIMATION_STATE.STOPPED;
					}
					
					if(lowest_action_progress > m_lowest_action_progress)
					{
						m_lowest_action_progress = lowest_action_progress;
					}
				}


            }
            

			m_what_just_changed = ANIMATION_DATA_TYPE.NONE;

			return all_letter_anims_finished;
		}




		void PauseAllParticleEffects(bool paused)
		{
			if(m_particle_effect_managers != null)
			{
				foreach(ParticleEffectInstanceManager particle_effect in m_particle_effect_managers)
				{
					particle_effect.Pause(paused);
				}
			}
		}
		
		void StopAllParticleEffects(bool force_stop = false)
		{
			if(m_particle_effect_managers != null)
			{
				foreach(ParticleEffectInstanceManager particle_effect in m_particle_effect_managers)
				{
					particle_effect.Stop(force_stop);
				}
				
				m_particle_effect_managers = new List<ParticleEffectInstanceManager>();
			}
			
			if(m_particle_systems != null)
			{
				foreach(ParticleSystem p_system in m_particle_systems)
				{
					if(p_system == null)
						continue;
					
					p_system.Stop();
					p_system.Clear();
				}
			}

#if !UNITY_5_4_OR_NEWER
			if(m_particle_emitters != null)
			{
				foreach(ParticleEmitter p_emit in m_particle_emitters)
				{
					if(p_emit == null)
						continue;
					
					p_emit.emit = false;
					p_emit.ClearParticles();
				}
			}
#endif
		}
		
		public void ClearCachedAudioParticleInstances()
		{
			m_audio_sources = new List<AudioSource>(m_gameObect.GetComponentsInChildren<AudioSource>());
#if !UNITY_5_4_OR_NEWER
			m_particle_emitters = new List<ParticleEmitter>(m_gameObect.GetComponentsInChildren<ParticleEmitter>());
#endif
			m_particle_systems = new List<ParticleSystem>(m_gameObect.GetComponentsInChildren<ParticleSystem>());
			
			foreach(AudioSource a_source in m_audio_sources)
			{
				if(a_source != null && a_source.gameObject != null)
					GameObject.DestroyImmediate(a_source.gameObject);
			}
			m_audio_sources = new List<AudioSource>();

#if !UNITY_5_4_OR_NEWER
			foreach(ParticleEmitter p_emitter in m_particle_emitters)
			{
				if(p_emitter != null && p_emitter.gameObject != null)
					GameObject.DestroyImmediate(p_emitter.gameObject);
			}
			m_particle_emitters = new List<ParticleEmitter>();
#endif
			
			foreach(ParticleSystem p_system in m_particle_systems)
			{
				if(p_system != null && p_system.gameObject != null)
					GameObject.DestroyImmediate(p_system.gameObject);
			}
			m_particle_systems = new List<ParticleSystem>();
			
			m_particle_effect_managers = new List<ParticleEffectInstanceManager>();
		}


		AudioSource AddNewAudioChild()
		{
			GameObject new_audio_source = new GameObject("TextFx_AudioSource");
			new_audio_source.transform.parent = m_transform;
			
			AudioSource a_source = new_audio_source.AddComponent<AudioSource>();
			
			a_source.playOnAwake = false;
			
			if(m_audio_sources == null)
			{
				m_audio_sources = new List<AudioSource>();
			}
			
			m_audio_sources.Add(a_source);
			
			return a_source;
		}
		
		void PlayClip(AudioSource a_source, AudioClip clip, float delay, float start_time, float volume, float pitch)
		{
			a_source.clip = clip;
			a_source.time = start_time;
			a_source.volume = volume;
			a_source.pitch = pitch;
			
	#if !UNITY_3_5 && !UNITY_4_0
			a_source.PlayDelayed(delay);
	#else
			a_source.Play((ulong)( delay * 44100));
	#endif
		}
		
		public void PlayAudioClip(AudioEffectSetup effect_setup, AnimationProgressionVariables progression_vars, AnimatePerOptions animate_per)
		{
			bool sound_played = false;
			AudioSource source = null;
			
			if(m_audio_sources != null)
			{
				foreach(AudioSource a_source in m_audio_sources)
				{
					if(!a_source.isPlaying)
					{
						// audio source free to play a sound
						source= a_source;
						
						sound_played = true;
						break;
					}
				}
				
				if(!sound_played)
				{
					source = AddNewAudioChild();
				}
			}
			else
			{
				source = AddNewAudioChild();
			}
			
			PlayClip(
				source,
				effect_setup.m_audio_clip,
				effect_setup.m_delay.GetValue(progression_vars, animate_per),
				effect_setup.m_offset_time.GetValue(progression_vars, animate_per),
				effect_setup.m_volume.GetValue(progression_vars, animate_per),
				effect_setup.m_pitch.GetValue(progression_vars, animate_per));
		}
		
		public void PlayParticleEffect(LetterSetup letter_setup, ParticleEffectSetup effect_setup, AnimationProgressionVariables progression_vars, AnimatePerOptions animate_per)
		{
			bool effect_played = false;

#if !UNITY_5_4_OR_NEWER
			if(effect_setup.m_legacy_particle_effect != null)
			{
				if(m_particle_emitters == null)
				{
					m_particle_emitters = new List<ParticleEmitter>();
				}
				
				foreach(ParticleEmitter p_emitter in m_particle_emitters)
				{
					if(!p_emitter.emit && p_emitter.particleCount == 0 && p_emitter.name.Equals(effect_setup.m_legacy_particle_effect.name + "(Clone)"))
					{
						m_particle_effect_managers.Add(new ParticleEffectInstanceManager(this, letter_setup, effect_setup, progression_vars, animate_per, particle_emitter : p_emitter));
						
						effect_played = true;
						break;
					}
				}
				
				if(!effect_played)
				{
					ParticleEmitter p_emitter = GameObject.Instantiate(effect_setup.m_legacy_particle_effect) as ParticleEmitter;
					m_particle_emitters.Add(p_emitter);
	#if !UNITY_3_5
					p_emitter.gameObject.SetActive(true);
	#else
					p_emitter.gameObject.SetActiveRecursively(true);
	#endif
					p_emitter.emit = false;
					p_emitter.transform.parent = m_transform;
					
					m_particle_effect_managers.Add(new ParticleEffectInstanceManager(this, letter_setup, effect_setup, progression_vars, animate_per, particle_emitter : p_emitter));
				}
			}
			else
#endif
				if(effect_setup.m_shuriken_particle_effect != null)
			{
				if(m_particle_systems == null)
					m_particle_systems = new List<ParticleSystem>();
				
				foreach(ParticleSystem p_system in m_particle_systems)
				{
					// check if particle system instance is currently not being used, and if it's the same type of effect that we're looking for.
					if(!p_system.isPlaying && p_system.particleCount == 0 && p_system.name.Equals(effect_setup.m_shuriken_particle_effect.name + "(Clone)"))
					{
						m_particle_effect_managers.Add(new ParticleEffectInstanceManager(this, letter_setup, effect_setup, progression_vars, animate_per, particle_system : p_system));
						
						effect_played = true;
						break;
					}
				}
				
				if(!effect_played)
				{
					// Make a new instance of the particleSystem effect and add to pool
					ParticleSystem p_system = GameObject.Instantiate(effect_setup.m_shuriken_particle_effect) as ParticleSystem;
					m_particle_systems.Add(p_system);
#if !UNITY_3_5
					p_system.gameObject.SetActive(true);
#else
					p_system.gameObject.SetActiveRecursively(true);
#endif

#if UNITY_5_5_OR_NEWER
					ParticleSystem.MainModule mainMod = p_system.main;
					mainMod.playOnAwake = false;
#else
					p_system.playOnAwake = false;
#endif
					p_system.Stop();
					p_system.transform.parent = m_transform;
					
					m_particle_effect_managers.Add(new ParticleEffectInstanceManager(this, letter_setup, effect_setup, progression_vars, animate_per, particle_system : p_system));
				}
			}
		}





		public LetterAnimation AddAnimation()
		{
			if(m_master_animations == null)
				m_master_animations = new List<LetterAnimation>();

			LetterAnimation newAnim = new LetterAnimation ();

			m_master_animations.Add(newAnim);

			return newAnim;
		}
		
		public void RemoveAnimation(int index)
		{
			if(m_master_animations != null && index >= 0 && index < NumAnimations)
				m_master_animations.RemoveAt(index);
		}
		
		public LetterAnimation GetAnimation(int index)
		{
			if(m_master_animations != null && m_master_animations.Count > index && index >= 0)
				return m_master_animations[index];
			else
				return null;
		}

		public LetterSetup GetLetter(int letterIdx)
		{
			if(m_letters != null && letterIdx < m_letters.Length)
				return m_letters[letterIdx];
			return null;
		}

		public Vector3 GetLetterPosition(int letter_idx, OBJ_POS position_requested = OBJ_POS.CENTER, TRANSFORM_SPACE transform_space = TRANSFORM_SPACE.WORLD)
		{
			if(m_letters == null || m_letters.Length == 0)
				return Vector3.zero;
			
			letter_idx = Mathf.Clamp(letter_idx, 0, m_letters.Length - 1);
			
			switch(position_requested)
			{
			case OBJ_POS.CENTER:
				return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[letter_idx].Center : m_letters[letter_idx].CenterLocal;
			case OBJ_POS.BOTTOM_LEFT:
				return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[letter_idx].BottomLeft : m_letters[letter_idx].BottomLeftLocal;
			case OBJ_POS.BOTTOM_RIGHT:
				return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[letter_idx].BottomRight : m_letters[letter_idx].BottomRightLocal;
			case OBJ_POS.TOP_LEFT:
				return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[letter_idx].TopLeft : m_letters[letter_idx].TopLeftLocal;
			case OBJ_POS.TOP_RIGHT:
				return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[letter_idx].TopRight : m_letters[letter_idx].TopRightLocal;
				
			default:
				return Vector3.zero;
			}
		}
		
		public Quaternion GetLetterRotation(int letter_idx, TRANSFORM_SPACE transform_space = TRANSFORM_SPACE.WORLD)
		{
			if(m_letters == null || m_letters.Length == 0)
				return Quaternion.identity;
			
			letter_idx = Mathf.Clamp(letter_idx, 0, m_letters.Length - 1);
			
			return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[letter_idx].Rotation : m_letters[letter_idx].RotationLocal;
		}
		
		public Vector3 GetLetterScale(int letter_idx, TRANSFORM_SPACE transform_space = TRANSFORM_SPACE.WORLD)
		{
			if(m_letters == null || m_letters.Length == 0)
				return Vector3.one;
			
			letter_idx = Mathf.Clamp(letter_idx, 0, m_letters.Length - 1);
			
			return transform_space == TRANSFORM_SPACE.WORLD ? m_letters[letter_idx].Scale : m_letters[letter_idx].ScaleLocal;
		}


		IEnumerator TimeDelay(float delay, AnimationTime time_type)
		{
			if(time_type == AnimationTime.GAME_TIME)
			{
				yield return new WaitForSeconds(delay);
			}
			else
			{
				float timer = 0;
				float last_time = Time.realtimeSinceStartup;
				float delta_time;
				while(timer < delay)
				{
					delta_time = Time.realtimeSinceStartup - last_time;
					if(delta_time > 0.1f)
					{
						delta_time = 0.1f;
					}
					timer += delta_time;
					last_time = Time.realtimeSinceStartup;
					yield return false;
				}
			}
		}



#if UNITY_EDITOR
		public string ExportDataAsPresetSection(TextFxAnimationManager.PRESET_ANIMATION_SECTION section, bool saveSampleTextInfo = true)
		{
			if(m_master_animations == null || m_master_animations.Count == 0)
			{
				Debug.LogError("There's no animation to export");
				return "";
			}

			tfxJSONObject json_data = new tfxJSONObject();
			
			json_data["TEXTFX_EXPORTER_VERSION"] = JSON_EXPORTER_VERSION;
			json_data["SECTION_TYPE"] = "" + section;
            json_data["LETTER_ANIMATIONS_DATA"] = m_master_animations[0].ExportDataAsPresetSection(saveSampleTextInfo);
			
			
			// Handle exporting quick setup configuration fields
			tfxJSONArray effect_settings_options = new tfxJSONArray();
			if(m_preset_effect_settings != null)
			{
				foreach(PresetEffectSetting effect_setting in m_preset_effect_settings)
				{
					effect_settings_options.Add(effect_setting.ExportData());
				}
			}
			json_data["PRESET_EFFECT_SETTINGS"] = effect_settings_options;
			
			return json_data.ToString();
		}

		public string ExportData(bool hard_copy = false)
		{
			tfxJSONObject json_data = new tfxJSONObject();

			json_data["TEXTFX_EXPORTER_VERSION"] = JSON_EXPORTER_VERSION;
			json_data["m_animate_per"] = (int) m_animate_per;
			
			if (hard_copy)
			{
				json_data["m_begin_delay"] = m_begin_delay;
				json_data["m_begin_on_start"] = m_begin_on_start;
				json_data["m_on_finish_action"] = (int) m_on_finish_action;
				json_data["m_time_type"] = (int) m_time_type;
			}
			
			tfxJSONArray letter_animations_data = new tfxJSONArray();
			if(m_master_animations != null)
			{
				foreach(LetterAnimation anim in m_master_animations)
				{
					letter_animations_data.Add(anim.ExportData());
				}
			}
			json_data["LETTER_ANIMATIONS_DATA"] = letter_animations_data;


			// Handle exporting quick setup configuration fields
			tfxJSONArray effect_settings_options = new tfxJSONArray();
			if(m_preset_effect_settings != null)
			{
				foreach(PresetEffectSetting effect_setting in m_preset_effect_settings)
				{
					effect_settings_options.Add(effect_setting.ExportData());
				}
			}
			json_data["PRESET_EFFECT_SETTINGS"] = effect_settings_options;

			
			return json_data.ToString();
		}


		// Used for importing a preset animation section's data, to have its animation edited in the editor, or have its quick-edit settings edited
		public void ImportPresetAnimationSectionData(string data, bool force_clear_old_audio_particles = false)
		{
			if(force_clear_old_audio_particles)
				ClearCachedAudioParticleInstances();
			
			tfxJSONObject json_data = tfxJSONObject.Parse(data, true);
			
			if(json_data != null)
			{
				m_master_animations = new List<LetterAnimation>();
				LetterAnimation letter_anim = new LetterAnimation();
				letter_anim.ImportPresetSectionData(json_data["LETTER_ANIMATIONS_DATA"].Obj, m_letters, m_animation_interface_reference.AssetNameSuffix);

				m_master_animations.Add(letter_anim);
				
				m_preset_effect_settings = new List<PresetEffectSetting>();
				
				// Import any Quick setup settings info
				if(json_data.ContainsKey("PRESET_EFFECT_SETTINGS"))
				{
					PresetEffectSetting effectSetting;
					foreach(tfxJSONValue effectSettingData in json_data["PRESET_EFFECT_SETTINGS"].Array)
					{
						effectSetting = new PresetEffectSetting();
						effectSetting.ImportData(effectSettingData.Obj);
						m_preset_effect_settings.Add(effectSetting);
					}
				}
			}
			else
			{
				// Import string is not valid JSON, therefore assuming it is in the legacy data import format.
				Debug.LogError("TextFx animation import failed. Non-valid JSON data provided");
			}
			
			if(!Application.isPlaying && m_current_text.Equals(""))
				m_animation_interface_reference.SetText("TextFx");
			
			PrepareAnimationData ();
			
			ResetAnimation();
			
			// Update mesh
			m_animation_interface_reference.UpdateTextFxMesh( );
			
			SceneView.RepaintAll();
		}
		
#endif

		public void ImportData(string data, TextFxAnimationManager.PresetAnimationSection animationSection, PRESET_ANIMATION_SECTION section, bool force_clear_old_audio_particles = false)
		{
			if(force_clear_old_audio_particles)
				ClearCachedAudioParticleInstances();
			
			int num_actions_added = 0;
			int num_loops_added = 0;
			int insert_action_index = 0;
			int insert_loop_index = 0;
			
			if(section == PRESET_ANIMATION_SECTION.MAIN)
			{
				insert_action_index = m_preset_intro.m_start_action + m_preset_intro.m_num_actions + (m_preset_intro.m_active ? 1 : 0);
				insert_loop_index = m_preset_intro.m_start_loop + m_preset_intro.m_num_loops;
			}
			else if(section == PRESET_ANIMATION_SECTION.OUTRO)
			{
				insert_action_index = m_preset_main.m_start_action + m_preset_main.m_num_actions + (m_preset_main.m_active ? 1 : 0);
				insert_loop_index = m_preset_main.m_start_loop + m_preset_main.m_num_loops;
			}
			
			
			tfxJSONObject json_data = tfxJSONObject.Parse(data, true);
			
			if(json_data != null)
			{
				if(m_master_animations == null || m_master_animations.Count == 0)
					m_master_animations = new List<LetterAnimation>(){ new LetterAnimation() };

				m_master_animations[0].ImportPresetSectionData(json_data["LETTER_ANIMATIONS_DATA"].Obj, m_letters, insert_action_index, insert_loop_index, ref num_actions_added, ref num_loops_added, m_animation_interface_reference != null ? m_animation_interface_reference.AssetNameSuffix : "");
				
				animationSection.m_preset_effect_settings = new List<PresetEffectSetting>();

#if UNITY_EDITOR
				m_preset_effect_settings = new List<PresetEffectSetting>();
#endif
				
				// Import any Quick setup settings info
				if(json_data.ContainsKey("PRESET_EFFECT_SETTINGS"))
				{
					PresetEffectSetting effectSetting;
					foreach(tfxJSONValue effectSettingData in json_data["PRESET_EFFECT_SETTINGS"].Array)
					{
						effectSetting = new PresetEffectSetting();
						effectSetting.ImportData(effectSettingData.Obj);
						animationSection.m_preset_effect_settings.Add(effectSetting);

#if UNITY_EDITOR
						m_preset_effect_settings.Add(effectSetting);
#endif
					}
				}
			}
			else
			{
				// Import string is not valid JSON, therefore assuming it is in the legacy data import format.
				Debug.LogError("TextFx animation import failed. Non-valid JSON data provided");
				
				//				this.ImportLegacyData(data);
				//				
				//				m_preset_effect_settings = new List<PresetEffectSetting>();
			}
			
			
			if(!Application.isPlaying && m_current_text.Equals(""))
				m_animation_interface_reference.SetText("TextFx");
			
			PrepareAnimationData ();
			
			ResetAnimation();

			// Update mesh
			if(m_animation_interface_reference != null)
				m_animation_interface_reference.UpdateTextFxMesh();

#if UNITY_EDITOR
			SceneView.RepaintAll();
#endif
			
			animationSection.m_num_actions = num_actions_added;
			animationSection.m_num_loops = num_loops_added;
		}




		public void ImportData(string data, bool force_clear_old_audio_particles = false)
		{
			if(force_clear_old_audio_particles)
				ClearCachedAudioParticleInstances();
			
			tfxJSONObject json_data = tfxJSONObject.Parse(data, true);
			
			if(json_data != null)
			{
				m_animate_per = (AnimatePerOptions) (int) json_data["m_animate_per"].Number;
				
				if(json_data.ContainsKey("m_begin_delay")) m_begin_delay = (float) json_data["m_begin_delay"].Number;
				if(json_data.ContainsKey("m_begin_on_start")) m_begin_on_start = json_data["m_begin_on_start"].Boolean;
				if(json_data.ContainsKey("m_on_finish_action")) m_on_finish_action = (ON_FINISH_ACTION) (int) json_data["m_on_finish_action"].Number;
				if(json_data.ContainsKey("m_time_type")) m_time_type = (AnimationTime) (int) json_data["m_time_type"].Number;
				
				m_master_animations = new List<LetterAnimation>();
				LetterAnimation letter_anim;
				foreach(tfxJSONValue animation_data in json_data["LETTER_ANIMATIONS_DATA"].Array)
				{
					letter_anim = new LetterAnimation();
					letter_anim.ImportData(animation_data.Obj, m_animation_interface_reference.AssetNameSuffix);
					m_master_animations.Add(letter_anim);
				}

#if UNITY_EDITOR
				m_preset_effect_settings = new List<PresetEffectSetting>();

				// Import any Quick setup settings info
				if(json_data.ContainsKey("PRESET_EFFECT_SETTINGS"))
				{
					PresetEffectSetting effectSetting;
					foreach(tfxJSONValue effectSettingData in json_data["PRESET_EFFECT_SETTINGS"].Array)
					{
						effectSetting = new PresetEffectSetting();
						effectSetting.ImportData(effectSettingData.Obj);
						m_preset_effect_settings.Add(effectSetting);
					}
				}
#endif
			}
			else
			{
				// Import string is not valid JSON, therefore assuming it is in the legacy data import format.
				Debug.LogError("TextFx animation import failed. Non-valid JSON data provided");

				this.ImportLegacyData(data);

#if UNITY_EDITOR
				m_preset_effect_settings = new List<PresetEffectSetting>();
#endif
			}


			if(!Application.isPlaying && m_current_text.Equals(""))
				m_animation_interface_reference.SetText("TextFx");

			PrepareAnimationData ();

			ResetAnimation();

			// Update mesh
			m_animation_interface_reference.UpdateTextFxMesh();

#if UNITY_EDITOR
			SceneView.RepaintAll();
#endif
		}
	}
}