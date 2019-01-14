using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.TextFx.JSON;

namespace TextFx
{

	public enum ACTION_TYPE
	{
		ANIM_SEQUENCE,
		BREAK
	}

	public enum PLAY_ITEM_EVENTS
	{
		ON_START,
		ON_FINISH
	}

	public enum PARTICLE_EFFECT_TYPE
	{
		SHURIKEN,
		LEGACY
	}

	public enum PLAY_ITEM_ASSIGNMENT
	{
		PER_LETTER,
		CUSTOM
	}

	[System.Serializable]
	public class ParticleEffectSetup : EffectItemSetup
	{
		public PARTICLE_EFFECT_TYPE m_effect_type;
#if !UNITY_5_4_OR_NEWER
		public ParticleEmitter m_legacy_particle_effect;
#endif
		public ParticleSystem m_shuriken_particle_effect;
		public ActionFloatProgression m_duration = new ActionFloatProgression(0);
		public bool m_follow_mesh = false;
		public ActionVector3Progression m_position_offset = new ActionVector3Progression(Vector3.zero);
		public ActionVector3Progression m_rotation_offset = new ActionVector3Progression(Vector3.zero);
		public bool m_rotate_relative_to_letter = true;
		
		
		public tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			ExportBaseData(ref json_data);
			
			json_data["m_effect_type"] = (int) m_effect_type;
#if !UNITY_5_4_OR_NEWER
			if(m_effect_type == PARTICLE_EFFECT_TYPE.LEGACY)
				json_data["m_legacy_particle_effect"] = m_legacy_particle_effect.ToPath();
			else
#endif
				json_data["m_shuriken_particle_effect"] = m_shuriken_particle_effect.ToPath();
			json_data["m_duration"] = m_duration.ExportData();
			json_data["m_follow_mesh"] = m_follow_mesh;
			json_data["m_position_offset"] = m_position_offset.ExportData();
			json_data["m_rotation_offset"] = m_rotation_offset.ExportData();
			json_data["m_rotate_relative_to_letter"] = m_rotate_relative_to_letter;
			
			return new tfxJSONValue(json_data);
		}
		
		public void ImportData(tfxJSONObject json_data, string assetNameSuffix = "")
		{
			m_effect_type = (PARTICLE_EFFECT_TYPE) (int) json_data["m_effect_type"].Number;
#if !UNITY_5_4_OR_NEWER
			if(m_effect_type == PARTICLE_EFFECT_TYPE.LEGACY)
				m_legacy_particle_effect = json_data["m_legacy_particle_effect"].Str.PathToParticleEmitter(assetNameSuffix);
			else
#endif
				m_shuriken_particle_effect = json_data["m_shuriken_particle_effect"].Str.PathToParticleSystem(assetNameSuffix);
			m_duration.ImportData(json_data["m_duration"].Obj);
			m_follow_mesh = json_data["m_follow_mesh"].Boolean;
			m_position_offset.ImportData(json_data["m_position_offset"].Obj);
			m_rotation_offset.ImportData(json_data["m_rotation_offset"].Obj);
			m_rotate_relative_to_letter = json_data["m_rotate_relative_to_letter"].Boolean;
			
			ImportBaseData(json_data);
		}
	}

	[System.Serializable]
	public class AudioEffectSetup : EffectItemSetup
	{
		public AudioClip m_audio_clip;
		public ActionFloatProgression m_offset_time = new ActionFloatProgression(0);
		public ActionFloatProgression m_volume = new ActionFloatProgression(1);
		public ActionFloatProgression m_pitch = new ActionFloatProgression(1);
		
		public tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			ExportBaseData(ref json_data);
			
			json_data["m_audio_clip"] = m_audio_clip.ToPath();
			json_data["m_offset_time"] = m_offset_time.ExportData();
			json_data["m_volume"] = m_volume.ExportData();
			json_data["m_pitch"] = m_pitch.ExportData();
			
			return new tfxJSONValue(json_data);
		}
		
		public void ImportData(tfxJSONObject json_data)
		{
			m_audio_clip = json_data["m_audio_clip"].Str.PathToAudioClip();
			m_offset_time.ImportData(json_data["m_offset_time"].Obj);
			m_volume.ImportData(json_data["m_volume"].Obj);
			m_pitch.ImportData(json_data["m_pitch"].Obj);
			
			ImportBaseData(json_data);
		}
	}

	[System.Serializable]
	public class EffectItemSetup
	{
		public bool m_editor_display;
		public PLAY_ITEM_EVENTS m_play_when;
		public PLAY_ITEM_ASSIGNMENT m_effect_assignment;
		public bool m_loop_play_once = false;
		public Vector2 CUSTOM_LETTERS_LIST_POS = Vector2.zero;
		public List<int> m_effect_assignment_custom_letters;
		public ActionFloatProgression m_delay = new ActionFloatProgression(0);
		
		public void ExportBaseData(ref tfxJSONObject json_data)
		{
			json_data["m_play_when"] = (int) m_play_when;
			json_data["m_effect_assignment"] = (int) m_effect_assignment;
			json_data["m_loop_play_once"] = m_loop_play_once;
			json_data["m_effect_assignment_custom_letters"] = m_effect_assignment_custom_letters.ExportData();
			json_data["m_delay"] = m_delay.ExportData();
		}
		
		public void ImportBaseData(tfxJSONObject json_data)
		{
			m_play_when = (PLAY_ITEM_EVENTS) (int) json_data["m_play_when"].Number;
			m_effect_assignment = (PLAY_ITEM_ASSIGNMENT) (int) json_data["m_effect_assignment"].Number;
			m_loop_play_once = json_data["m_loop_play_once"].Boolean;
			m_delay.ImportData(json_data["m_delay"].Obj);
			
			m_effect_assignment_custom_letters = json_data["m_effect_assignment_custom_letters"].Array.JSONtoListInt();
			m_loop_play_once = json_data["m_loop_play_once"].Boolean;
		}
	}

	[System.Serializable]
	public class LetterAction
	{
#if UNITY_EDITOR
		public bool m_colour_section_foldout = false;
		public bool m_position_section_foldout = false;
		public bool m_global_rotation_section_foldout = false;
		public bool m_local_rotation_section_foldout = false;
		public bool m_global_scale_section_foldout = false;
		public bool m_local_scale_section_foldout = false;
#endif

		bool m_editor_folded = false;
		public bool FoldedInEditor { get { return m_editor_folded; } set { m_editor_folded = value; } }
		
		public bool m_offset_from_last = false;
		
		public ACTION_TYPE m_action_type = ACTION_TYPE.ANIM_SEQUENCE;

		public bool m_colour_transition_active = false;
		public ActionColorProgression m_start_colour = new ActionColorProgression(new VertexColour(Color.black), true);
		public ActionColorProgression m_end_colour = new ActionColorProgression(new VertexColour(Color.black), true);

		public bool m_position_transition_active = false;
		public AxisEasingOverrideData  m_position_axis_ease_data = new AxisEasingOverrideData();
		public ActionPositionVector3Progression m_start_pos = new ActionPositionVector3Progression(Vector3.zero);
		public ActionPositionVector3Progression m_end_pos = new ActionPositionVector3Progression(Vector3.zero);

		public bool m_global_rotation_transition_active = false;
		public AxisEasingOverrideData m_global_rotation_axis_ease_data = new AxisEasingOverrideData();
		public ActionVector3Progression m_global_start_euler_rotation = new ActionVector3Progression(Vector3.zero);
		public ActionVector3Progression m_global_end_euler_rotation = new ActionVector3Progression(Vector3.zero);

		public bool m_local_rotation_transition_active = false;
		public AxisEasingOverrideData m_rotation_axis_ease_data = new AxisEasingOverrideData();
		public ActionVector3Progression m_start_euler_rotation = new ActionVector3Progression(Vector3.zero);
		public ActionVector3Progression m_end_euler_rotation = new ActionVector3Progression(Vector3.zero);

		public bool m_global_scale_transition_active = false;
		public AxisEasingOverrideData m_global_scale_axis_ease_data = new AxisEasingOverrideData();
		public ActionVector3Progression m_global_start_scale = new ActionVector3Progression(Vector3.one);
		public ActionVector3Progression m_global_end_scale = new ActionVector3Progression(Vector3.one);

		public bool m_local_scale_transition_active = false;
		public AxisEasingOverrideData m_scale_axis_ease_data = new AxisEasingOverrideData();
		public ActionVector3Progression m_start_scale = new ActionVector3Progression(Vector3.one);
		public ActionVector3Progression m_end_scale = new ActionVector3Progression(Vector3.one);
		
		public bool m_force_same_start_time = false;

		public bool m_delay_with_white_space_influence = false;

		public ActionFloatProgression m_delay_progression = new ActionFloatProgression(0);
		public ActionFloatProgression m_duration_progression = new ActionFloatProgression(1);
		
		public EasingEquation m_ease_type = EasingEquation.Linear;
		public int m_letter_anchor_start = (int) TextfxTextAnchor.MiddleCenter;
		public int m_letter_anchor_end = (int) TextfxTextAnchor.MiddleCenter;
		public bool m_letter_anchor_2_way = false;
		
		[SerializeField]
		List<ParticleEffectSetup> m_particle_effects = new List<ParticleEffectSetup>();
		[SerializeField]
		List<AudioEffectSetup> m_audio_effects = new List<AudioEffectSetup>();
		
		[SerializeField]
		Vector3 m_anchor_offset;
		[SerializeField]
		Vector3 m_anchor_offset_end;

		public Vector3 AnchorOffsetStart { get { return m_anchor_offset; } }
		public Vector3 AnchorOffsetEnd { get { return m_anchor_offset_end; } }
		public int NumParticleEffectSetups { get { return m_particle_effects != null ? m_particle_effects.Count : 0; } }
		public int NumAudioEffectSetups { get { return m_audio_effects != null ? m_audio_effects.Count : 0; } }
		public List<ParticleEffectSetup> ParticleEffectSetups { get { return m_particle_effects; } }
		public List<AudioEffectSetup> AudioEffectSetups { get { return m_audio_effects; } }
		public bool ParticleEffectsEditorDisplay { get; set; }
		public bool AudioEffectsEditorDisplay { get; set; }
		
		public ParticleEffectSetup GetParticleEffectSetup(int index)
		{
			if(index >= 0 && index < m_particle_effects.Count)
				return m_particle_effects[index];
			else
				return null;
		}
		
		public ParticleEffectSetup AddParticleEffectSetup()
		{
			if(m_particle_effects == null)
				m_particle_effects = new List<ParticleEffectSetup>();
			
			ParticleEffectSetup new_particle_effect = new ParticleEffectSetup();
			m_particle_effects.Add(new_particle_effect);
			
			return new_particle_effect;
		}
		
		public void AddParticleEffectSetup(ParticleEffectSetup particle_setup)
		{
			if(m_particle_effects == null)
				m_particle_effects = new List<ParticleEffectSetup>();
			
			m_particle_effects.Add(particle_setup);
		}
		
		public void RemoveParticleEffectSetup(int index)
		{
			if(m_particle_effects != null && index >= 0 && index < m_particle_effects.Count)
				m_particle_effects.RemoveAt(index);
		}
		
		public void ClearParticleEffectSetups()
		{
			m_particle_effects.Clear();
		}
		
		public AudioEffectSetup GetAudioEffectSetup(int index)
		{
			if(index >= 0 && index < m_audio_effects.Count)
				return m_audio_effects[index];
			else
				return null;
		}
		
		public AudioEffectSetup AddAudioEffectSetup()
		{
			if(m_audio_effects == null)
				m_audio_effects = new List<AudioEffectSetup>();
			
			AudioEffectSetup new_audio_effect = new AudioEffectSetup();
			m_audio_effects.Add(new_audio_effect);
			
			return new_audio_effect;
		}
		
		public void AddAudioEffectSetup(AudioEffectSetup audio_setup)
		{
			if(m_audio_effects == null)
				m_audio_effects = new List<AudioEffectSetup>();
			
			m_audio_effects.Add(audio_setup);
		}
		
		public void RemoveAudioEffectSetup(int index)
		{
			if(m_audio_effects != null && index >= 0 && index < m_audio_effects.Count)
				m_audio_effects.RemoveAt(index);
		}
		
		public void ClearAudioEffectSetups()
		{
			m_audio_effects.Clear();
		}

		AudioEffectSetup _audio_effect_setup;
		ParticleEffectSetup _particle_effect_setup;

		public void SoftReset(LetterAction prev_action, AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per, bool first_action = false)
		{
			if(!m_offset_from_last && !first_action)
			{
				if(m_start_colour.UniqueRandom)
				{
					m_start_colour.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_end_colour.Values : null);
				}
				if(m_start_pos.UniqueRandom)
				{
					m_start_pos.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_end_pos.Values : null);
				}
				if(m_start_euler_rotation.UniqueRandom)
				{
					m_start_euler_rotation.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_end_euler_rotation.Values : null);
				}
				if(m_global_start_euler_rotation.UniqueRandom)
				{
					m_global_start_euler_rotation.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_global_end_euler_rotation.Values : null);
				}
				if(m_start_scale.UniqueRandom)
				{
					m_start_scale.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_end_scale.Values : null);
				}
				if(m_global_start_scale.UniqueRandom)
				{
					m_global_start_scale.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_global_end_scale.Values : null);
				}
			}
			
			// End State Unique Randoms
			if(m_end_colour.UniqueRandom)
			{
				m_end_colour.CalculateUniqueRandom(progression_variables, animate_per, m_start_colour.Values);
			}
			if(m_end_pos.UniqueRandom)
			{
				m_end_pos.CalculateUniqueRandom(progression_variables, animate_per, m_start_pos.Values);
			}
			if(m_end_euler_rotation.UniqueRandom)
			{
				m_end_euler_rotation.CalculateUniqueRandom(progression_variables, animate_per, m_start_euler_rotation.Values);
			}
			if(m_end_scale.UniqueRandom)
			{
				m_end_scale.CalculateUniqueRandom(progression_variables, animate_per, m_start_scale.Values);
			}
			if(m_global_end_euler_rotation.UniqueRandom)
			{
				m_global_end_euler_rotation.CalculateUniqueRandom(progression_variables, animate_per, m_global_start_euler_rotation.Values);
			}
			if(m_global_end_scale.UniqueRandom)
			{
				m_global_end_scale.CalculateUniqueRandom(progression_variables, animate_per, m_global_start_scale.Values);
			}
			
			
			// Timing unique randoms
			if(m_delay_progression.UniqueRandom)
			{
				m_delay_progression.CalculateUniqueRandom(progression_variables, animate_per);
			}
			if(m_duration_progression.UniqueRandom)
			{
				m_duration_progression.CalculateUniqueRandom(progression_variables, animate_per);
			}
			
			if(m_audio_effects != null)
			{
				for(int aIdx=0; aIdx < m_audio_effects.Count; aIdx++)
				{
					_audio_effect_setup = m_audio_effects[aIdx];

					if(_audio_effect_setup.m_delay.UniqueRandom)
						_audio_effect_setup.m_delay.CalculateUniqueRandom(progression_variables, animate_per);
					if(_audio_effect_setup.m_offset_time.UniqueRandom)
						_audio_effect_setup.m_offset_time.CalculateUniqueRandom(progression_variables, animate_per);
					if(_audio_effect_setup.m_volume.UniqueRandom)
						_audio_effect_setup.m_volume.CalculateUniqueRandom(progression_variables, animate_per);
					if(_audio_effect_setup.m_pitch.UniqueRandom)
						_audio_effect_setup.m_pitch.CalculateUniqueRandom(progression_variables, animate_per);
				}
			}
			
			if(m_particle_effects != null)
			{
				for(int pIdx=0; pIdx < m_particle_effects.Count; pIdx++)
				{
					_particle_effect_setup = m_particle_effects [pIdx];

					if(_particle_effect_setup.m_position_offset.UniqueRandom)
						_particle_effect_setup.m_position_offset.CalculateUniqueRandom(progression_variables, animate_per, null);
					if(_particle_effect_setup.m_rotation_offset.UniqueRandom)
						_particle_effect_setup.m_rotation_offset.CalculateUniqueRandom(progression_variables, animate_per, null);
					if(_particle_effect_setup.m_delay.UniqueRandom)
						_particle_effect_setup.m_delay.CalculateUniqueRandom(progression_variables, animate_per);
					if(_particle_effect_setup.m_duration.UniqueRandom)
						_particle_effect_setup.m_duration.CalculateUniqueRandom(progression_variables, animate_per);
				}
			}
		}


		public void SoftResetStarts(LetterAction prev_action, AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per)
		{
			if(!m_offset_from_last && m_start_colour.UniqueRandom)
			{
				m_start_colour.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_end_colour.Values : null);
			}
			
			if(!m_offset_from_last)
			{
				if(m_start_pos.UniqueRandom)
				{
					m_start_pos.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_end_pos.Values : null);
				}
				if(m_start_euler_rotation.UniqueRandom)
				{
					m_start_euler_rotation.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_end_euler_rotation.Values : null);
				}
				if(m_start_scale.UniqueRandom)
				{
					m_start_scale.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_end_scale.Values : null);
				}
				if(m_global_start_euler_rotation.UniqueRandom)
				{
					m_global_start_euler_rotation.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_global_end_euler_rotation.Values : null);
				}
				if(m_global_start_scale.UniqueRandom)
				{
					m_global_start_scale.CalculateUniqueRandom(progression_variables, animate_per, prev_action != null ? prev_action.m_global_end_scale.Values : null);
				}
			}
		}
		
		public LetterAction ContinueActionFromThis()
		{
			LetterAction letter_action = new LetterAction();
			
			// Default to offset from previous and not be folded in editor
			letter_action.m_offset_from_last = true;
			letter_action.m_editor_folded = true;
			
			letter_action.m_position_axis_ease_data = m_position_axis_ease_data.Clone();
			letter_action.m_rotation_axis_ease_data = m_rotation_axis_ease_data.Clone();
			letter_action.m_scale_axis_ease_data = m_scale_axis_ease_data.Clone();
			
			letter_action.m_start_colour = m_end_colour.Clone();
			letter_action.m_end_colour = m_end_colour.Clone();
			
			letter_action.m_start_pos = m_end_pos.CloneThis();
			letter_action.m_end_pos = m_end_pos.CloneThis();
			
			letter_action.m_start_euler_rotation = m_end_euler_rotation.Clone();
			letter_action.m_end_euler_rotation = m_end_euler_rotation.Clone();
			
			letter_action.m_start_scale = m_end_scale.Clone();
			letter_action.m_end_scale = m_end_scale.Clone();

			letter_action.m_global_start_euler_rotation = m_global_end_euler_rotation.Clone();
			letter_action.m_global_end_euler_rotation = m_global_end_euler_rotation.Clone();
			
			letter_action.m_global_start_scale = m_global_end_scale.Clone();
			letter_action.m_global_end_scale = m_global_end_scale.Clone();
			
			letter_action.m_delay_progression = new ActionFloatProgression(0);
			letter_action.m_duration_progression = new ActionFloatProgression(1);
			
			letter_action.m_letter_anchor_start = m_letter_anchor_2_way ? m_letter_anchor_end : m_letter_anchor_start;
			
			letter_action.m_ease_type = m_ease_type;
			
			return letter_action;
		}
		
		int GetProgressionTotal(int num_letters, int num_words, int num_lines, AnimatePerOptions animate_per_default, AnimatePerOptions animate_per_override, bool overriden)
		{
			switch(overriden ? animate_per_override : animate_per_default)
			{
				case AnimatePerOptions.LETTER:
					return num_letters;
				case AnimatePerOptions.WORD:
					return num_words;
				case AnimatePerOptions.LINE:
					return num_lines;
			}
			
			return num_letters;
		}

		public void PrepareData(TextFxAnimationManager anim_manager,
								ref LetterSetup[] letters,
		                        LetterAnimation animation_ref,
		                        int action_idx,
		                        ANIMATION_DATA_TYPE what_to_update,
		                        int num_letters,
		                        int num_white_space_chars_to_include,
		                        int num_words,
		                        int num_lines,
		                        LetterAction prev_action,
		                        AnimatePerOptions animate_per,
		                        ActionColorProgression defaultTextColour,
		                        bool prev_action_end_state = true)
		{
			// Set progression reference datas 
			m_start_colour.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.COLOUR, true);
			m_start_euler_rotation.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.LOCAL_ROTATION, true);
			m_start_pos.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.POSITION, true);
			m_start_scale.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.LOCAL_SCALE, true);
			m_global_start_euler_rotation.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.GLOBAL_ROTATION, true);
			m_global_start_scale.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.GLOBAL_SCALE, true);
			m_end_colour.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.COLOUR, false);
			m_end_euler_rotation.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.LOCAL_ROTATION, false);
			m_end_pos.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.POSITION, false);
			m_end_scale.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.LOCAL_SCALE, false);
			m_global_end_euler_rotation.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.GLOBAL_ROTATION, false);
			m_global_end_scale.SetReferenceData(action_idx, ANIMATION_DATA_TYPE.GLOBAL_SCALE, false);


			// Prepare Data

			if(what_to_update == ANIMATION_DATA_TYPE.DURATION || what_to_update == ANIMATION_DATA_TYPE.ALL)
				m_duration_progression.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, m_duration_progression.AnimatePer, m_duration_progression.OverrideAnimatePerOption));


			if((what_to_update == ANIMATION_DATA_TYPE.AUDIO_EFFECTS || what_to_update == ANIMATION_DATA_TYPE.ALL) && m_audio_effects != null)
			{
				foreach(AudioEffectSetup effect_setup in m_audio_effects)
				{
					effect_setup.m_delay.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, effect_setup.m_delay.AnimatePer, effect_setup.m_delay.OverrideAnimatePerOption));
					effect_setup.m_offset_time.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, effect_setup.m_offset_time.AnimatePer, effect_setup.m_offset_time.OverrideAnimatePerOption));
					effect_setup.m_volume.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, effect_setup.m_volume.AnimatePer, effect_setup.m_volume.OverrideAnimatePerOption));
					effect_setup.m_pitch.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, effect_setup.m_pitch.AnimatePer, effect_setup.m_pitch.OverrideAnimatePerOption));
				}
			}

			if((what_to_update == ANIMATION_DATA_TYPE.PARTICLE_EFFECTS || what_to_update == ANIMATION_DATA_TYPE.ALL) && m_particle_effects != null)
			{
				foreach(ParticleEffectSetup effect_setup in m_particle_effects)
				{
					effect_setup.m_position_offset.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, effect_setup.m_position_offset.AnimatePer, effect_setup.m_position_offset.OverrideAnimatePerOption), null);
					effect_setup.m_rotation_offset.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, effect_setup.m_rotation_offset.AnimatePer, effect_setup.m_rotation_offset.OverrideAnimatePerOption), null);
					effect_setup.m_delay.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, effect_setup.m_delay.AnimatePer, effect_setup.m_delay.OverrideAnimatePerOption));
					effect_setup.m_duration.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, effect_setup.m_duration.AnimatePer, effect_setup.m_duration.OverrideAnimatePerOption));
				}
			}
			
			if(m_action_type == ACTION_TYPE.BREAK)
			{
				if(prev_action != null)
				{
					m_start_colour.SetValueReference(prev_action_end_state ? prev_action.m_end_colour : prev_action.m_start_colour);
					m_start_pos.SetValueReference( prev_action_end_state ? prev_action.m_end_pos : prev_action.m_start_pos );
					m_start_euler_rotation.SetValueReference( prev_action_end_state ? prev_action.m_end_euler_rotation : prev_action.m_start_euler_rotation );
					m_start_scale.SetValueReference( prev_action_end_state ? prev_action.m_end_scale : prev_action.m_start_scale );
					m_global_start_euler_rotation.SetValueReference( prev_action_end_state ? prev_action.m_global_end_euler_rotation : prev_action.m_global_start_euler_rotation );
					m_global_start_scale.SetValueReference( prev_action_end_state ? prev_action.m_global_end_scale : prev_action.m_global_start_scale );

					m_end_colour.SetValueReference(prev_action_end_state ? prev_action.m_end_colour : prev_action.m_start_colour);
					m_end_pos.SetValueReference( prev_action_end_state ? prev_action.m_end_pos : prev_action.m_start_pos );
					m_end_euler_rotation.SetValueReference(prev_action_end_state ? prev_action.m_end_euler_rotation : prev_action.m_start_euler_rotation );
					m_end_scale.SetValueReference( prev_action_end_state ? prev_action.m_end_scale : prev_action.m_start_scale );
					m_global_end_euler_rotation.SetValueReference( prev_action_end_state ? prev_action.m_global_end_euler_rotation : prev_action.m_global_start_euler_rotation );
					m_global_end_scale.SetValueReference( prev_action_end_state ? prev_action.m_global_end_scale : prev_action.m_global_start_scale );
				}

				return;
			}

			if (animation_ref.m_letters_to_animate_option != LETTERS_TO_ANIMATE.ALL_LETTERS || (((ValueProgression)m_delay_progression.Progression) != ValueProgression.Eased && ((ValueProgression)m_delay_progression.Progression) != ValueProgression.EasedCustom))
				m_delay_with_white_space_influence = false;


			if(what_to_update == ANIMATION_DATA_TYPE.DELAY || what_to_update == ANIMATION_DATA_TYPE.ALL)
				m_delay_progression.CalculateProgressions(GetProgressionTotal(num_letters + (m_delay_with_white_space_influence ? num_white_space_chars_to_include : 0), num_words, num_lines, animate_per, m_delay_progression.AnimatePer, m_delay_progression.OverrideAnimatePerOption));
			

			if(what_to_update == ANIMATION_DATA_TYPE.COLOUR || what_to_update == ANIMATION_DATA_TYPE.ALL)
			{
				if(m_offset_from_last && prev_action != null)
				{
					m_start_colour.SetValueReference(prev_action_end_state ? prev_action.m_end_colour : prev_action.m_start_colour );
				}
				else
				{
					m_start_colour.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, m_start_colour.AnimatePer, m_start_colour.OverrideAnimatePerOption), 
					                                     prev_action != null ? prev_action.m_end_colour : defaultTextColour,
					                                     prev_action == null || m_colour_transition_active);
				}

				m_end_colour.CalculateProgressions(GetProgressionTotal(num_letters, num_words, num_lines, animate_per, m_end_colour.AnimatePer, m_end_colour.OverrideAnimatePerOption),
				                                   m_start_colour,
				                                   prev_action == null || m_colour_transition_active);
			}
			
			
			if(m_offset_from_last && prev_action != null)
			{
				if(what_to_update == ANIMATION_DATA_TYPE.POSITION || what_to_update == ANIMATION_DATA_TYPE.ALL)
					m_start_pos.SetValueReference( prev_action_end_state ? prev_action.m_end_pos : prev_action.m_start_pos );

				if(what_to_update == ANIMATION_DATA_TYPE.LOCAL_ROTATION || what_to_update == ANIMATION_DATA_TYPE.ALL)
					m_start_euler_rotation.SetValueReference( prev_action_end_state ? prev_action.m_end_euler_rotation : prev_action.m_start_euler_rotation );

				if(what_to_update == ANIMATION_DATA_TYPE.LOCAL_SCALE || what_to_update == ANIMATION_DATA_TYPE.ALL)
					m_start_scale.SetValueReference( prev_action_end_state ? prev_action.m_end_scale : prev_action.m_start_scale );

				if(what_to_update == ANIMATION_DATA_TYPE.GLOBAL_ROTATION || what_to_update == ANIMATION_DATA_TYPE.ALL)
					m_global_start_euler_rotation.SetValueReference( prev_action_end_state ? prev_action.m_global_end_euler_rotation : prev_action.m_global_start_euler_rotation );

				if(what_to_update == ANIMATION_DATA_TYPE.GLOBAL_SCALE || what_to_update == ANIMATION_DATA_TYPE.ALL)
					m_global_start_scale.SetValueReference( prev_action_end_state ? prev_action.m_global_end_scale : prev_action.m_global_start_scale );
			}
			else
			{
				if(what_to_update == ANIMATION_DATA_TYPE.POSITION || what_to_update == ANIMATION_DATA_TYPE.ALL)
					m_start_pos.CalculatePositionProgressions(	anim_manager,
					                                          	animation_ref,
					                                          	letters,
					                                          	GetProgressionTotal(num_letters + num_white_space_chars_to_include, num_words, num_lines, animate_per, m_start_pos.AnimatePer, m_start_pos.OverrideAnimatePerOption),
																prev_action != null ? prev_action.m_end_pos : null,
					                                          	prev_action == null || m_position_transition_active);

				if(what_to_update == ANIMATION_DATA_TYPE.LOCAL_ROTATION || what_to_update == ANIMATION_DATA_TYPE.ALL)
					m_start_euler_rotation.CalculateRotationProgressions(	GetProgressionTotal(num_letters + num_white_space_chars_to_include, num_words, num_lines, animate_per, m_start_euler_rotation.AnimatePer, m_start_euler_rotation.OverrideAnimatePerOption),
																			prev_action != null ? prev_action.m_end_euler_rotation : null,
					                                                     	prev_action == null || m_local_rotation_transition_active);

				if(what_to_update == ANIMATION_DATA_TYPE.LOCAL_SCALE || what_to_update == ANIMATION_DATA_TYPE.ALL)
					m_start_scale.CalculateProgressions(GetProgressionTotal(num_letters + num_white_space_chars_to_include, num_words, num_lines, animate_per, m_start_scale.AnimatePer, m_start_scale.OverrideAnimatePerOption),
														prev_action != null ? prev_action.m_end_scale : null,
					                                    prev_action == null || m_local_scale_transition_active);

				if(what_to_update == ANIMATION_DATA_TYPE.GLOBAL_ROTATION || what_to_update == ANIMATION_DATA_TYPE.ALL)
					m_global_start_euler_rotation.CalculateRotationProgressions(GetProgressionTotal(num_letters + num_white_space_chars_to_include, num_words, num_lines, animate_per, m_global_start_euler_rotation.AnimatePer, m_global_start_euler_rotation.OverrideAnimatePerOption),
					                                                            prev_action != null ? prev_action.m_global_end_euler_rotation : null,
					                                                            prev_action == null || m_global_rotation_transition_active);

				if(what_to_update == ANIMATION_DATA_TYPE.GLOBAL_SCALE || what_to_update == ANIMATION_DATA_TYPE.ALL)
					m_global_start_scale.CalculateProgressions(	GetProgressionTotal(num_letters + num_white_space_chars_to_include, num_words, num_lines, animate_per, m_global_start_scale.AnimatePer, m_global_start_scale.OverrideAnimatePerOption),
				                                           		prev_action != null ? prev_action.m_global_end_scale : null,
					                                           	prev_action == null || m_global_scale_transition_active);
			}


			if(what_to_update == ANIMATION_DATA_TYPE.POSITION || what_to_update == ANIMATION_DATA_TYPE.ALL)
				m_end_pos.CalculatePositionProgressions(anim_manager,
				                                        animation_ref,
				                                        letters,
				                                        GetProgressionTotal(num_letters + num_white_space_chars_to_include, num_words, num_lines, animate_per, m_end_pos.AnimatePer, m_end_pos.OverrideAnimatePerOption),
				                                        m_start_pos,
				                                        prev_action == null || m_position_transition_active);

			if(what_to_update == ANIMATION_DATA_TYPE.LOCAL_ROTATION || what_to_update == ANIMATION_DATA_TYPE.ALL)
				m_end_euler_rotation.CalculateRotationProgressions(GetProgressionTotal(num_letters + num_white_space_chars_to_include, num_words, num_lines, animate_per, m_end_euler_rotation.AnimatePer, m_end_euler_rotation.OverrideAnimatePerOption),
				                                                   m_start_euler_rotation,
				                                                   prev_action == null || m_local_rotation_transition_active);

			if(what_to_update == ANIMATION_DATA_TYPE.LOCAL_SCALE || what_to_update == ANIMATION_DATA_TYPE.ALL)
				m_end_scale.CalculateProgressions(GetProgressionTotal(num_letters + num_white_space_chars_to_include, num_words, num_lines, animate_per, m_end_scale.AnimatePer, m_end_scale.OverrideAnimatePerOption),
				                                  m_start_scale,
				                                  prev_action == null || m_local_scale_transition_active);

			if(what_to_update == ANIMATION_DATA_TYPE.GLOBAL_ROTATION || what_to_update == ANIMATION_DATA_TYPE.ALL)
				m_global_end_euler_rotation.CalculateRotationProgressions(GetProgressionTotal(num_letters + num_white_space_chars_to_include, num_words, num_lines, animate_per, m_global_end_euler_rotation.AnimatePer, m_global_end_euler_rotation.OverrideAnimatePerOption),
				                                                          m_global_start_euler_rotation,
				                                                          prev_action == null || m_global_rotation_transition_active);

			if(what_to_update == ANIMATION_DATA_TYPE.GLOBAL_SCALE || what_to_update == ANIMATION_DATA_TYPE.ALL)
				m_global_end_scale.CalculateProgressions(GetProgressionTotal(num_letters + num_white_space_chars_to_include, num_words, num_lines, animate_per, m_global_end_scale.AnimatePer, m_global_end_scale.OverrideAnimatePerOption),
				                                         m_global_start_scale,
				                                         prev_action == null || m_global_scale_transition_active);

			if(what_to_update == ANIMATION_DATA_TYPE.POSITION
			   || what_to_update == ANIMATION_DATA_TYPE.POSITION
			   || what_to_update == ANIMATION_DATA_TYPE.LETTER_ANCHOR
			   || what_to_update == ANIMATION_DATA_TYPE.ALL)
				CalculateLetterAnchorOffset();
		}
		
		public void CalculateLetterAnchorOffset()
		{
			// Calculate letters anchor offset vector
			m_anchor_offset = AnchorOffsetToVector3((TextfxTextAnchor) m_letter_anchor_start);
			
			m_anchor_offset_end = m_letter_anchor_2_way ? AnchorOffsetToVector3((TextfxTextAnchor) m_letter_anchor_end) : m_anchor_offset;
		}
		
		Vector3 AnchorOffsetToVector3(TextfxTextAnchor anchor)
		{
			Vector3 anchor_vec = Vector3.zero;
			if(anchor == TextfxTextAnchor.UpperRight || anchor == TextfxTextAnchor.MiddleRight || anchor == TextfxTextAnchor.LowerRight)
			{
				anchor_vec.x = 1;
			}
			else if(anchor == TextfxTextAnchor.UpperCenter || anchor == TextfxTextAnchor.MiddleCenter || anchor == TextfxTextAnchor.LowerCenter)
			{
				anchor_vec.x = 0.5f;
			}
			
			// handle letter anchor y-offset
			if(anchor == TextfxTextAnchor.MiddleLeft || anchor == TextfxTextAnchor.MiddleCenter || anchor == TextfxTextAnchor.MiddleRight)
			{
				anchor_vec.y = 0.5f;
			}
			else if(anchor == TextfxTextAnchor.LowerLeft || anchor == TextfxTextAnchor.LowerCenter || anchor == TextfxTextAnchor.LowerRight)
			{
				anchor_vec.y = 1;
			}
			return anchor_vec;
		}

		public tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			json_data["m_action_type"] = (int) m_action_type;
			json_data["m_ease_type"] = (int) m_ease_type;
			json_data["m_force_same_start_time"] = m_force_same_start_time;
			json_data["m_letter_anchor_start"] = m_letter_anchor_start;
			json_data["m_letter_anchor_end"] = m_letter_anchor_end;
			json_data["m_letter_anchor_2_way"] = m_letter_anchor_2_way;
			json_data["m_offset_from_last"] = m_offset_from_last;
			json_data["m_position_axis_ease_data"] = m_position_axis_ease_data.ExportData();
			json_data["m_rotation_axis_ease_data"] = m_rotation_axis_ease_data.ExportData();
			json_data["m_scale_axis_ease_data"] = m_scale_axis_ease_data.ExportData();
			
			json_data ["m_colour_transition_active"] = m_colour_transition_active;
			json_data["m_start_colour"] = m_start_colour.ExportData();
			json_data ["m_position_transition_active"] = m_position_transition_active;
			json_data["m_start_pos"] = m_start_pos.ExportData();
			json_data ["m_local_rotation_transition_active"] = m_local_rotation_transition_active;
			json_data["m_start_euler_rotation"] = m_start_euler_rotation.ExportData();
			json_data ["m_local_scale_transition_active"] = m_local_scale_transition_active;
			json_data["m_start_scale"] = m_start_scale.ExportData();
			json_data ["m_global_rotation_transition_active"] = m_global_rotation_transition_active;
			json_data["m_global_start_euler_rotation"] = m_global_start_euler_rotation.ExportData();
			json_data ["m_global_scale_transition_active"] = m_global_scale_transition_active;
			json_data["m_global_start_scale"] = m_global_start_scale.ExportData();
			
			json_data["m_end_colour"] = m_end_colour.ExportData();
			json_data["m_end_pos"] = m_end_pos.ExportData();
			json_data["m_end_euler_rotation"] = m_end_euler_rotation.ExportData();
			json_data["m_end_scale"] = m_end_scale.ExportData();
			json_data["m_global_end_euler_rotation"] = m_global_end_euler_rotation.ExportData();
			json_data["m_global_end_scale"] = m_global_end_scale.ExportData();
			
			json_data["m_delay_progression"] = m_delay_progression.ExportData();
			json_data["m_duration_progression"] = m_duration_progression.ExportData();
			
			
			tfxJSONArray audio_effects_data = new tfxJSONArray();
			foreach(AudioEffectSetup effect_setup in m_audio_effects)
			{
				if(effect_setup.m_audio_clip == null)
					continue;
				
				audio_effects_data.Add(effect_setup.ExportData());
			}
			json_data["AUDIO_EFFECTS_DATA"] = audio_effects_data;
			
			tfxJSONArray particle_effects_data = new tfxJSONArray();
			foreach(ParticleEffectSetup effect_setup in m_particle_effects)
			{
				if(
#if !UNITY_5_4_OR_NEWER
					effect_setup.m_legacy_particle_effect == null && 
#endif
					effect_setup.m_shuriken_particle_effect == null)
					continue;
				
				particle_effects_data.Add(effect_setup.ExportData());
			}
			json_data["PARTICLE_EFFECTS_DATA"] = particle_effects_data;
			
			return new tfxJSONValue(json_data);
		}

		public void ImportData(tfxJSONObject json_data, string assetNameSuffix = "", float timing_scale = -1)
		{	
			m_action_type = (ACTION_TYPE) (int) json_data["m_action_type"].Number;
			m_ease_type = (EasingEquation) (int) json_data["m_ease_type"].Number;
			m_force_same_start_time = json_data["m_force_same_start_time"].Boolean;
			m_letter_anchor_start = (int) json_data["m_letter_anchor_start"].Number;
			m_letter_anchor_end = (int) json_data["m_letter_anchor_end"].Number;
			m_letter_anchor_2_way = json_data["m_letter_anchor_2_way"].Boolean;
			m_offset_from_last = json_data["m_offset_from_last"].Boolean;
			m_position_axis_ease_data.ImportData(json_data["m_position_axis_ease_data"].Obj);
			m_rotation_axis_ease_data.ImportData(json_data["m_rotation_axis_ease_data"].Obj);
			m_scale_axis_ease_data.ImportData(json_data["m_scale_axis_ease_data"].Obj);

			m_colour_transition_active = json_data.ContainsKey("m_colour_transition_active") ? json_data ["m_colour_transition_active"].Boolean : true;
			m_position_transition_active = json_data.ContainsKey("m_position_transition_active") ? json_data ["m_position_transition_active"].Boolean : true;
			m_local_rotation_transition_active = json_data.ContainsKey("m_local_rotation_transition_active") ? json_data ["m_local_rotation_transition_active"].Boolean : true;
			m_local_scale_transition_active = json_data.ContainsKey("m_local_scale_transition_active") ? json_data ["m_local_scale_transition_active"].Boolean : true;
			m_global_rotation_transition_active = json_data.ContainsKey("m_global_rotation_transition_active") ? json_data ["m_global_rotation_transition_active"].Boolean : true;
			m_global_scale_transition_active = json_data.ContainsKey("m_global_scale_transition_active") ? json_data ["m_global_scale_transition_active"].Boolean : true;


			if(json_data.ContainsKey("m_start_colour"))
				m_start_colour.ImportData(json_data["m_start_colour"].Obj);
			if(json_data.ContainsKey("m_end_colour"))
				m_end_colour.ImportData(json_data["m_end_colour"].Obj);

			if(json_data.ContainsKey("m_start_vertex_colour"))
				m_start_colour.ImportData(json_data["m_start_vertex_colour"].Obj);
			if(json_data.ContainsKey("m_end_vertex_colour"))
				m_start_colour.ImportData(json_data["m_end_vertex_colour"].Obj);

			if(json_data.ContainsKey("m_use_gradient_start"))
				// Legacy setting. Need to check for it for backwards compatibility
				m_start_colour.UseColourGradients = json_data["m_use_gradient_start"].Boolean;

			if(json_data.ContainsKey("m_use_gradient_end"))
				// Legacy setting. Need to check for it for backwards compatibility
				m_end_colour.UseColourGradients = json_data["m_use_gradient_end"].Boolean;

			
			m_start_pos.ImportData(json_data["m_start_pos"].Obj);
			m_end_pos.ImportData(json_data["m_end_pos"].Obj);

			m_start_euler_rotation.ImportData(json_data["m_start_euler_rotation"].Obj);
			m_end_euler_rotation.ImportData(json_data["m_end_euler_rotation"].Obj);
			m_start_scale.ImportData(json_data["m_start_scale"].Obj);
			m_end_scale.ImportData(json_data["m_end_scale"].Obj);

			if (json_data.ContainsKey ("m_global_start_euler_rotation"))
			{
				m_global_start_euler_rotation.ImportData(json_data["m_global_start_euler_rotation"].Obj);
				m_global_end_euler_rotation.ImportData(json_data["m_global_end_euler_rotation"].Obj);
			}
			if (json_data.ContainsKey ("m_global_start_scale"))
			{
				m_global_start_scale.ImportData (json_data ["m_global_start_scale"].Obj);
				m_global_end_scale.ImportData (json_data ["m_global_end_scale"].Obj);
			}

			m_duration_progression.ImportData(json_data["m_duration_progression"].Obj);
			m_delay_progression.ImportData(json_data["m_delay_progression"].Obj);

			if(timing_scale != -1)
			{
				// Scale delay easing by the provided timing scale
				if(m_delay_progression.Progression != (int) ValueProgression.Constant)
				{
					float from = m_delay_progression.ValueFrom;
					float to = m_delay_progression.ValueTo;
					float then = m_delay_progression.ValueThen;

					if(m_delay_progression.Progression == (int) ValueProgression.Eased)
					{
						if(m_delay_progression.UsingThirdValue)
							m_delay_progression.SetEased( from * timing_scale, to * timing_scale, then * timing_scale);
						else
							m_delay_progression.SetEased( from * timing_scale, to * timing_scale);
					}
					else if(m_delay_progression.Progression == (int) ValueProgression.EasedCustom)
					{
						m_delay_progression.SetEasedCustom( from * timing_scale, to * timing_scale);
					}
					else if(m_delay_progression.Progression == (int) ValueProgression.Random)
					{
						m_delay_progression.SetRandom( from * timing_scale, to * timing_scale, m_delay_progression.UniqueRandomRaw);
					}
				}
			}
			
			m_audio_effects = new List<AudioEffectSetup>();
			AudioEffectSetup audio_effect;
			foreach(tfxJSONValue audio_data in json_data["AUDIO_EFFECTS_DATA"].Array)
			{
				audio_effect = new AudioEffectSetup();
				audio_effect.ImportData(audio_data.Obj);
				m_audio_effects.Add(audio_effect);
			}
			
			m_particle_effects = new List<ParticleEffectSetup>();
			ParticleEffectSetup particle_effect;
			foreach(tfxJSONValue particle_data in json_data["PARTICLE_EFFECTS_DATA"].Array)
			{
				particle_effect = new ParticleEffectSetup();
				particle_effect.ImportData(particle_data.Obj, assetNameSuffix);
				m_particle_effects.Add(particle_effect);
			}
		}
	}
}