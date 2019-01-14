/**
	TextFx Variable Progression Classes.
	Used in animation Actions to define either a constant value, or an ordered or random sequence of values within a given range.
**/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Boomlagoon.TextFx.JSON;
#if UNITY_EDITOR
using UnityEditor;
using System;
#endif

namespace TextFx
{
	public enum ValueProgression
	{
		Constant,
		Random,
		Eased,
		EasedCustom
	}

	public enum PROGRESSION_VALUE_STATE
	{
		UNIQUE,
		REFERENCE,
		OFFSET_FROM_REFERENCE
	}

	// Stores the information required to identify what this progression data is (ie. color, rotation, position etc)
	[System.Serializable]
	public struct ActionVariableProgressionReferenceData
	{
		public ANIMATION_DATA_TYPE m_data_type;
		public bool m_start_state;
		public int m_action_index;

		public ActionVector3Progression GetVector3Prog(List<LetterAction> letter_actions)
		{
			if (m_action_index >= letter_actions.Count)
				return null;

			LetterAction letter_action = letter_actions [m_action_index];

			switch (m_data_type) {
				case ANIMATION_DATA_TYPE.LOCAL_SCALE:
					return m_start_state ? letter_action.m_start_scale : letter_action.m_end_scale;
				case ANIMATION_DATA_TYPE.GLOBAL_SCALE:
					return m_start_state ? letter_action.m_global_start_scale : letter_action.m_global_end_scale;
				case ANIMATION_DATA_TYPE.LOCAL_ROTATION:
					return m_start_state ? letter_action.m_start_euler_rotation : letter_action.m_end_euler_rotation;
				case ANIMATION_DATA_TYPE.GLOBAL_ROTATION:
					return m_start_state ? letter_action.m_global_start_euler_rotation : letter_action.m_global_end_euler_rotation;
				case ANIMATION_DATA_TYPE.POSITION:
					return m_start_state ? letter_action.m_start_pos : letter_action.m_end_pos;
			}

			return null;
		}

		public ActionPositionVector3Progression GetPositionVector3Prog(List<LetterAction> letter_actions)
		{
			if (m_action_index >= letter_actions.Count)
				return null;
			
			LetterAction letter_action = letter_actions [m_action_index];
			
			switch (m_data_type) {
				case ANIMATION_DATA_TYPE.POSITION:
					return m_start_state ? letter_action.m_start_pos : letter_action.m_end_pos;
			}
			
			return null;
		}

		public ActionColorProgression GetColourProg(List<LetterAction> letter_actions, ActionColorProgression defaultColourProg)
		{
			if (m_action_index < 0 && m_data_type == ANIMATION_DATA_TYPE.COLOUR) {
				// Marked as using the default text colouring
				return defaultColourProg;
			}

			if (m_action_index >= letter_actions.Count)
				return null;
			
			LetterAction letter_action = letter_actions [m_action_index];
			
			switch (m_data_type) {
				case ANIMATION_DATA_TYPE.COLOUR:
					return m_start_state ? letter_action.m_start_colour : letter_action.m_end_colour;
			}
			
			return null;
		}
	}

	[System.Serializable]
	public abstract class ActionVariableProgression
	{
#if UNITY_EDITOR
		protected const float LINE_HEIGHT = 20;
		protected const float VECTOR_3_WIDTH = 300;
		protected const float PROGRESSION_HEADER_LABEL_WIDTH = 150;
		protected const float ACTION_INDENT_LEVEL_1 = 10;
		protected const float ENUM_SELECTOR_WIDTH = 300;
		protected const float ENUM_SELECTOR_WIDTH_MEDIUM = 120;
		protected const float ENUM_SELECTOR_WIDTH_SMALL = 70;
		
		protected static int[] PROGRESSION_ENUM_VALUES = new int[] {0,1,2,3};
#endif

		[SerializeField]
		protected ActionVariableProgressionReferenceData m_reference_data;
		public ActionVariableProgressionReferenceData ReferenceData { get { return m_reference_data; } }
		[SerializeField]
		protected ValueProgression m_progression = ValueProgression.Constant;		// Legacy field
		[SerializeField]
		protected int m_progression_idx = -1;
		public virtual string[] ProgressionExtraOptions { get { return null; } }
		public virtual int[] ProgressionExtraOptionIndexes { get { return null; } }
		
		[SerializeField]
		protected EasingEquation m_ease_type = EasingEquation.Linear;
		[SerializeField]
		protected bool m_is_offset_from_last = false;
		[SerializeField]
		protected bool m_to_to_bool = false;
		[SerializeField]
		protected bool m_unique_randoms = false;
		[SerializeField]
		protected AnimatePerOptions m_animate_per;
		[SerializeField]
		protected bool m_override_animate_per_option = false;
		[SerializeField]
		protected AnimationCurve m_custom_ease_curve = new AnimationCurve();
		
		public EasingEquation EaseType { get { return m_ease_type; } }
		public bool IsOffsetFromLast { get { return m_is_offset_from_last; } set { m_is_offset_from_last = value; } }
		public bool UsingThirdValue { get { return m_to_to_bool; } }
		public AnimatePerOptions AnimatePer { get { return m_animate_per; } set { m_animate_per = value; } }
		public bool OverrideAnimatePerOption { get { return m_override_animate_per_option; } set { m_override_animate_per_option = value; } }
		public AnimationCurve CustomEaseCurve { get { return m_custom_ease_curve; } }
		public virtual bool UniqueRandom { get { return Progression == (int) ValueProgression.Random && m_unique_randoms; } }
		public bool UniqueRandomRaw { get { return m_unique_randoms; } }
		public int Progression {
			get {
				if(m_progression_idx == -1)
					m_progression_idx = (int) m_progression;
				return m_progression_idx;
			}
		}

		public void SetReferenceData(int actionIdx, ANIMATION_DATA_TYPE data_type, bool startState)
		{
			m_reference_data.m_action_index = actionIdx;
			m_reference_data.m_data_type = data_type;
			m_reference_data.m_start_state = startState;
		}
		
		public int GetProgressionIndex(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default, bool consider_white_space = false)
		{
			return progression_variables.GetValue(m_override_animate_per_option ? m_animate_per : animate_per_default, consider_white_space);
		}

		protected void ExportBaseData(ref tfxJSONObject json_data)
		{
			json_data["m_progression"] = Progression;
			json_data["m_ease_type"] = (int) m_ease_type;
			json_data["m_is_offset_from_last"] = m_is_offset_from_last;
			json_data["m_to_to_bool"] = m_to_to_bool;
			json_data["m_unique_randoms"] = m_unique_randoms;
			json_data["m_animate_per"] = (int) m_animate_per;
			json_data["m_override_animate_per_option"] = m_override_animate_per_option;
			
			if(Progression == (int) ValueProgression.EasedCustom)
			{
				json_data["m_custom_ease_curve"] = m_custom_ease_curve.ExportData();
			}
		}

		protected void ImportBaseData(tfxJSONObject json_data)
		{
			m_progression_idx = (int) json_data["m_progression"].Number;
			m_ease_type = (EasingEquation) (int) json_data["m_ease_type"].Number;
			m_is_offset_from_last = json_data["m_is_offset_from_last"].Boolean;
			m_to_to_bool = json_data["m_to_to_bool"].Boolean;
			m_unique_randoms = json_data["m_unique_randoms"].Boolean;
			m_animate_per = (AnimatePerOptions) (int) json_data["m_animate_per"].Number;
			m_override_animate_per_option = json_data["m_override_animate_per_option"].Boolean;
			if(json_data.ContainsKey("m_custom_ease_curve"))
				m_custom_ease_curve = json_data["m_custom_ease_curve"].Array.JSONtoAnimationCurve();
		}

		public abstract tfxJSONValue ExportData();

		public abstract void ImportData(tfxJSONObject json_data);
		
		public void ImportBaseLagacyData(KeyValuePair<string, string> value_pair)
		{
			switch(value_pair.Key)
			{
				case "m_progression": m_progression_idx = int.Parse(value_pair.Value); break;
				case "m_ease_type": m_ease_type = (EasingEquation) int.Parse(value_pair.Value); break;
				case "m_is_offset_from_last": m_is_offset_from_last = bool.Parse(value_pair.Value); break;
				case "m_to_to_bool": m_to_to_bool = bool.Parse(value_pair.Value); break;
				case "m_unique_randoms": m_unique_randoms = bool.Parse(value_pair.Value); break;
				case "m_animate_per": m_animate_per = (AnimatePerOptions) int.Parse(value_pair.Value); break;
				case "m_override_animate_per_option": m_override_animate_per_option = bool.Parse(value_pair.Value); break;
				case "m_custom_ease_curve": m_custom_ease_curve = value_pair.Value.ToAnimationCurve(); break;
			}
		}
		
	#if UNITY_EDITOR	
		public float DrawProgressionEditorHeader(GUIContent label, Rect position, bool offset_legal, bool unique_randoms_legal, bool bold_label = true, string[] extra_options = null, int[] extra_option_indexes = null)
		{
			float x_offset = position.x;
			float y_offset = position.y;
			if(bold_label)
			{
				EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), label, EditorStyles.boldLabel);
			}
			else
			{
				EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), label);
			}
			x_offset += PROGRESSION_HEADER_LABEL_WIDTH;
			
			string[] options = Enum.GetNames( typeof(ValueProgression) );
			int[] option_indexes = PROGRESSION_ENUM_VALUES;
			
			if(extra_options != null && extra_option_indexes != null && extra_options.Length > 0 && extra_options.Length == extra_option_indexes.Length)
			{
				int original_length = options.Length;
				Array.Resize<string>(ref options, options.Length + extra_options.Length);
				Array.Copy(extra_options, 0, options, original_length, extra_options.Length);
				
				original_length = option_indexes.Length;
				Array.Resize<int>(ref option_indexes, option_indexes.Length + extra_option_indexes.Length);
				Array.Copy(extra_option_indexes, 0, option_indexes, original_length, extra_option_indexes.Length);
			}
			
			m_progression_idx = EditorGUI.IntPopup(new Rect(x_offset, y_offset, ENUM_SELECTOR_WIDTH_SMALL + 18, LINE_HEIGHT), Progression, options, option_indexes);
			x_offset += ENUM_SELECTOR_WIDTH_SMALL + 25;
			
			if(m_progression_idx == (int) ValueProgression.Eased)
			{
				EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), new GUIContent("Function :", "Easing function used to lerp values between 'from' and 'to'."));
				x_offset += 65;
				m_ease_type = (EasingEquation) EditorGUI.EnumPopup(new Rect(x_offset, y_offset, ENUM_SELECTOR_WIDTH_MEDIUM, LINE_HEIGHT), m_ease_type);
				x_offset += ENUM_SELECTOR_WIDTH_MEDIUM + 10;
				
				EditorGUI.LabelField(new Rect(x_offset, y_offset, position.width, LINE_HEIGHT), new GUIContent("3rd?", "Option to add a third state to lerp values between."));
				x_offset += 35;
				m_to_to_bool = EditorGUI.Toggle(new Rect(x_offset, y_offset, ENUM_SELECTOR_WIDTH_MEDIUM, LINE_HEIGHT), m_to_to_bool);
			}
			else if(m_progression_idx == (int) ValueProgression.Random && unique_randoms_legal)
			{
				m_unique_randoms = EditorGUI.Toggle(new Rect(x_offset, y_offset, 200, LINE_HEIGHT), new GUIContent("Unique Randoms?", "Denotes whether a new random value will be picked each time this action is repeated (like when in a loop)."), m_unique_randoms);
			}
			y_offset += LINE_HEIGHT;
			
			if(offset_legal)
			{
				m_is_offset_from_last = EditorGUI.Toggle(new Rect(position.x + ACTION_INDENT_LEVEL_1, y_offset, 200, LINE_HEIGHT), new GUIContent("Offset From Last?", "Denotes whether this value will offset from whatever value it had in the last state. End states offset the start state. Start states offset the previous actions end state."), m_is_offset_from_last);
				y_offset += LINE_HEIGHT;
			}
			
			if((m_progression_idx == (int) ValueProgression.Eased || m_progression_idx == (int) ValueProgression.Random))
			{
				m_override_animate_per_option = EditorGUI.Toggle(new Rect(position.x + ACTION_INDENT_LEVEL_1, y_offset, 200, LINE_HEIGHT), new GUIContent("Override AnimatePer?", "Denotes whether this state value progression will use the global 'Animate Per' setting, or define its own."), m_override_animate_per_option);
				if(m_override_animate_per_option)
				{
					m_animate_per = (AnimatePerOptions) EditorGUI.EnumPopup(new Rect(position.x + ACTION_INDENT_LEVEL_1 + 200, y_offset, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), m_animate_per);
				}
				
				y_offset += LINE_HEIGHT;
			}
			else
			{
				m_override_animate_per_option = false;
			}
			
			return position.y + (y_offset - position.y);
		}
	#endif
	}

	[System.Serializable]
	public class ActionFloatProgression : ActionVariableProgression
	{
		[SerializeField]
		float[] m_values;
		[SerializeField]
		float m_from = 0;
		[SerializeField]
		float m_to = 0;
		[SerializeField]
		float m_to_to = 0;
		
		public float ValueFrom { get { return m_from; } }
		public float ValueTo { get { return m_to; } }
		public float ValueThen { get { return m_to_to; } }
		public float[] Values { get { return m_values; } set { m_values = value; } }
		
		public void SetConstant( float constant_value )
		{
			m_progression_idx = (int) ValueProgression.Constant;
			m_from = constant_value;
		}
		
		public void SetRandom( float random_min, float random_max, bool unique_randoms = false)
		{
			m_progression_idx = (int) ValueProgression.Random;
			m_from = random_min;
			m_to = random_max;
			m_unique_randoms = unique_randoms;
		}

		public void SetEased( float eased_from, float eased_to)
		{
			SetEased (eased_from, eased_to, m_ease_type);
		}

		public void SetEased( float eased_from, float eased_to, float eased_then)
		{
			SetEased (eased_from, eased_to, eased_then, m_ease_type);
		}

		public void SetEased( float eased_from, float eased_to, EasingEquation easing_function)
		{
			m_progression_idx = (int) ValueProgression.Eased;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to_bool = false;
			m_ease_type = easing_function;
		}
		
		public void SetEased( float eased_from, float eased_to, float eased_then, EasingEquation easing_function)
		{
			m_progression_idx = (int) ValueProgression.Eased;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to = eased_then;
			m_to_to_bool = true;
			m_ease_type = easing_function;
		}

		public void SetEasedCustom ( float eased_from, float eased_to)
		{
			SetEasedCustom (eased_from, eased_to, m_custom_ease_curve);
		}

		public void SetEasedCustom ( float eased_from, float eased_to, AnimationCurve easing_curve)
		{
			m_progression_idx = (int) ValueProgression.EasedCustom;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to_bool = false;
			m_custom_ease_curve = easing_curve;
		}
		
		
		public float GetValue(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default, bool consider_white_space = false)
		{
			return GetValue(GetProgressionIndex(progression_variables,animate_per_default, consider_white_space));
		}
		
		public float GetValue(int progression_idx)
		{
			int num_vals = m_values.Length;
			if(num_vals > 1 && progression_idx < num_vals)
			{
				return m_values[progression_idx];
			}
			else if(num_vals==1)
			{
				return m_values[0];
			}
			else
			{
				return 0;
			}
		}
		
		public ActionFloatProgression(float start_val)
		{
			m_from = start_val;
			m_to = start_val;
			m_to_to = start_val;
		}
		
		public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per)
		{
			m_values[GetProgressionIndex(progression_variables, animate_per)] = m_from + (m_to - m_from) * UnityEngine.Random.value;
		}
		
		public void CalculateProgressions(int num_progressions)
		{
			// Initialise array of values.
			m_values = new float[Progression == (int) ValueProgression.Eased || Progression == (int) ValueProgression.EasedCustom || Progression == (int) ValueProgression.Random
									? num_progressions
									: 1];
			
			if(Progression == (int) ValueProgression.Random) //  && (progression >= 0 || m_unique_randoms))
			{
				for(int idx=0; idx < num_progressions; idx++)
				{
					m_values[idx] = m_from + (m_to - m_from) * UnityEngine.Random.value;
				}
			}
			else if(Progression == (int) ValueProgression.Eased)
			{
				float progression;
				for(int idx=0; idx < num_progressions; idx++)
				{
					progression = num_progressions == 1 ? 0 : (float)idx / ((float)num_progressions - 1f);
					
					if(m_to_to_bool)
					{
						if(progression <= 0.5f)
						{
							m_values[idx] = m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression/0.5f);
						}
						else
						{
							progression -= 0.5f;
							m_values[idx] = m_to + (m_to_to - m_to) * EasingManager.GetEaseProgress(EasingManager.GetEaseTypeOpposite(m_ease_type), progression/0.5f);
						}
					}
					else
					{
						m_values[idx] = m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression);
					}
				}
			}
			else if(Progression == (int) ValueProgression.EasedCustom)
			{
				float progression;
				
				for(int idx=0; idx < num_progressions; idx++)
				{
					progression = num_progressions == 1 ? 0 : (float)idx / ((float)num_progressions - 1f);
					
					m_values[idx] += m_from + (m_to - m_from) * m_custom_ease_curve.Evaluate(progression);
				}
			}
			else if(Progression == (int) ValueProgression.Constant)
			{
				m_values[0] = m_from;
			}
		}
		
		public ActionFloatProgression Clone()
		{
			ActionFloatProgression float_progression = new ActionFloatProgression(0);
			
			float_progression.m_progression_idx = Progression;
			float_progression.m_ease_type = m_ease_type;
			float_progression.m_from = m_from;
			float_progression.m_to = m_to;
			float_progression.m_to_to = m_to_to;
			float_progression.m_to_to_bool = m_to_to_bool;
			float_progression.m_unique_randoms = m_unique_randoms;
			float_progression.m_override_animate_per_option = m_override_animate_per_option;
			float_progression.m_animate_per = m_animate_per;
			
			return float_progression;
		}

		public override tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			ExportBaseData(ref json_data);
			
			json_data["m_from"] = m_from;
			json_data["m_to"] = m_to;
			json_data["m_to_to"] = m_to_to;
			
			return new tfxJSONValue(json_data);
		}

		public override void ImportData(tfxJSONObject json_data)
		{
			m_from = (float) json_data["m_from"].Number;
			m_to = (float) json_data["m_to"].Number;
			m_to_to = (float) json_data["m_to_to"].Number;
			
			ImportBaseData(json_data);
		}
		
		public void ImportLegacyData(string data_string)
		{
			KeyValuePair<string, string> value_pair;
			List<object> obj_list = data_string.StringToList(';',':');
			
			foreach(object obj in obj_list)
			{
				value_pair = (KeyValuePair<string, string>) obj;
				
				switch(value_pair.Key)
				{
					case "m_from": m_from = float.Parse(value_pair.Value); break;
					case "m_to": m_to = float.Parse(value_pair.Value); break;
					case "m_to_to": m_to_to = float.Parse(value_pair.Value); break;

					default :
						ImportBaseLagacyData(value_pair); break;
				}
			}
		}
		
	#if UNITY_EDITOR	
		public float DrawEditorGUI(GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
		{
			float x_offset = position.x + ACTION_INDENT_LEVEL_1;
			float y_offset = DrawProgressionEditorHeader(label, position, offset_legal, unique_random_legal, bold_label, extra_options : ProgressionExtraOptions, extra_option_indexes : ProgressionExtraOptionIndexes);
			
			m_from = EditorGUI.FloatField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), Progression == (int) ValueProgression.Constant ? "Value" : "Value From", m_from);
			y_offset += LINE_HEIGHT;
			
			if(Progression != (int) ValueProgression.Constant)
			{
				m_to = EditorGUI.FloatField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Value To", m_to);
				y_offset += LINE_HEIGHT;
				
				if(Progression == (int) ValueProgression.Eased && m_to_to_bool)
				{
					m_to_to = EditorGUI.FloatField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Value Then", m_to_to);
					y_offset += LINE_HEIGHT;
				}
				
				
				if(Progression == (int) ValueProgression.EasedCustom)
				{
					m_custom_ease_curve = EditorGUI.CurveField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve", m_custom_ease_curve );
					y_offset += LINE_HEIGHT * 1.2f;
				}
			}
			
			return (y_offset) - position.y;
		}

		public bool DrawQuickEditorGUI(string settingName, float gui_x_offset, ref float gui_y_offset, bool ignore_gui_changes, float inputsXOffset = 200)
		{
			float valueFrom = 0, valueTo = 0, valueThen = 0;

			EditorGUI.LabelField (new Rect (gui_x_offset, gui_y_offset, 300, LINE_HEIGHT), settingName);

			valueFrom = EditorGUI.FloatField(new Rect(inputsXOffset, gui_y_offset, 30, LINE_HEIGHT), ValueFrom);
			
			if(Progression == (int) ValueProgression.Eased || Progression == (int) ValueProgression.EasedCustom || Progression == (int) ValueProgression.Random)
			{
				EditorGUI.LabelField (new Rect (inputsXOffset + 35, gui_y_offset, 20, LINE_HEIGHT), "->");

				valueTo = EditorGUI.FloatField(new Rect(inputsXOffset + 60, gui_y_offset, 30, LINE_HEIGHT), ValueTo);
				valueThen = 0;
				if(Progression == (int) ValueProgression.Eased && m_to_to_bool)
				{
					EditorGUI.LabelField (new Rect (inputsXOffset + 95, gui_y_offset, 20, LINE_HEIGHT), "->");
					valueThen = EditorGUI.FloatField(new Rect(inputsXOffset + 120, gui_y_offset, 30, LINE_HEIGHT), ValueThen);
				}
			}

			gui_y_offset += LINE_HEIGHT;

			if(GUI.changed && !ignore_gui_changes)
			{
				if(Progression == (int) ValueProgression.Constant)
					SetConstant(valueFrom);
				else if(Progression == (int) ValueProgression.Eased)
				{
					if(m_to_to_bool)
						SetEased(valueFrom, valueTo, valueThen);
					else
						SetEased(valueFrom, valueTo);
				}
				else if(Progression == (int) ValueProgression.Random)
					SetRandom(valueFrom, valueTo);
				else if(Progression == (int) ValueProgression.EasedCustom)
					SetEasedCustom(valueFrom, valueTo);
				
				return true;
			}
			
			return false;
		}
#endif


		public List<PresetEffectSetting.VariableStateListener> GetStateListeners()
		{
			List<PresetEffectSetting.VariableStateListener> variableListeners = new List<PresetEffectSetting.VariableStateListener> ();

			if(Progression == (int) ValueProgression.Constant)
			{
				variableListeners.Add (new PresetEffectSetting.VariableStateListener () {
					m_type = PresetEffectSetting.VariableStateListener.TYPE.FLOAT,
					m_startFloatValue = m_from,
					m_onFloatStateChangeCallback = (float value) => {
						SetConstant(value);
					}});
			}
			else
			{
				variableListeners.Add (new PresetEffectSetting.VariableStateListener () {
					m_type = PresetEffectSetting.VariableStateListener.TYPE.FLOAT,
					m_startFloatValue = m_from,
					m_onFloatStateChangeCallback = (float value) => {
						if(Progression == (int) ValueProgression.Eased)
							if(m_to_to_bool)
								SetEased(value, m_to, m_to_to);
							else
								SetEased(value, m_to);
						else if(Progression == (int) ValueProgression.EasedCustom)
							SetEasedCustom(value, m_to);
						else if(Progression == (int) ValueProgression.Random)
							SetRandom(value, m_to);
							
					}});
				variableListeners.Add (new PresetEffectSetting.VariableStateListener () {
					m_type = PresetEffectSetting.VariableStateListener.TYPE.FLOAT,
					m_startFloatValue = m_to,
					m_onFloatStateChangeCallback = (float value) => {
						if(Progression == (int) ValueProgression.Eased)
							if(m_to_to_bool)
								SetEased(m_from, value, m_to_to);
							else
								SetEased(m_from, value);
						else if(Progression == (int) ValueProgression.EasedCustom)
							SetEasedCustom(m_from, value);
						else if(Progression == (int) ValueProgression.Random)
							SetRandom(m_from, value);
					}});
				if(Progression == (int) ValueProgression.Eased && m_to_to_bool)
				{
					variableListeners.Add (new PresetEffectSetting.VariableStateListener () {
						m_type = PresetEffectSetting.VariableStateListener.TYPE.FLOAT,
						m_startFloatValue = m_to_to,
						m_onFloatStateChangeCallback = (float value) => {
							SetEased(m_from, m_to, value);
						}});
				}
			}

			return variableListeners;
		}
	}

	[System.Serializable]
	public class ActionPositionVector3Progression : ActionVector3Progression
	{
		[SerializeField]
		bool m_force_position_override = false;
		public bool ForcePositionOverride { get { return m_force_position_override; } set { m_force_position_override = value; } }
		
		public void SetConstant( Vector3 constant_value, bool force_this_position = false )
		{
			m_progression_idx = (int) ValueProgression.Constant;
			m_from = constant_value;
			m_force_position_override = force_this_position;
		}
		
		public ActionPositionVector3Progression(Vector3 start_vec)
		{
			m_from = start_vec;
			m_to = start_vec;
			m_to_to = start_vec;
		}
		
		public ActionPositionVector3Progression CloneThis()
		{
			ActionPositionVector3Progression progression = new ActionPositionVector3Progression(Vector3.zero);
			
			progression.m_progression_idx = Progression;
			progression.m_ease_type = m_ease_type;
			progression.m_from = m_from;
			progression.m_to = m_to;
			progression.m_to_to = m_to_to;
			progression.m_to_to_bool = m_to_to_bool;
			progression.m_is_offset_from_last = m_is_offset_from_last;
			progression.m_unique_randoms = m_unique_randoms;
			progression.m_force_position_override = m_force_position_override;
			progression.m_override_animate_per_option = m_override_animate_per_option;
			progression.m_animate_per = m_animate_per;
			progression.m_ease_curve_per_axis = m_ease_curve_per_axis;
			progression.m_custom_ease_curve = new AnimationCurve(m_custom_ease_curve.keys);
			progression.m_custom_ease_curve_y = new AnimationCurve(m_custom_ease_curve_y.keys);
			progression.m_custom_ease_curve_z = new AnimationCurve(m_custom_ease_curve_z.keys);
			
			return progression;
		}
		
		public void CalculatePositionProgressions(TextFxAnimationManager anim_manager,
		                                          LetterAnimation letter_animation,
		                                          LetterSetup[] letters,
		                                          int num_progressions,
		                                          ActionPositionVector3Progression offset_prog,
		                                          bool variableActive = true)
		{
			CalculateProgressions(num_progressions, offset_prog, variableActive);

			if((m_value_state == PROGRESSION_VALUE_STATE.REFERENCE ? GetOffsetReference().GetPositionVector3Prog(letter_animation.LetterActions).m_force_position_override : m_force_position_override))
			{
				Vector3 base_offset = Vector3.zero;

				if(m_values.Length == 1)
				{
					base_offset = m_values[0];
					m_values = new Vector3[letters.Length];
				}

				// Remove the base positioning offset for each letter
				for(int letter_idx =0; letter_idx < letters.Length; letter_idx++)
				{
					m_values[letter_idx] += base_offset;
					m_values[letter_idx] -= letters[letter_idx].BaseVerticesCenter / anim_manager.MovementScale;
				}
			}
		}

		public override tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = base.ExportData().Obj;
			
			json_data["m_force_position_override"] = m_force_position_override;
			
			return new tfxJSONValue(json_data);
		}
		
		public override void ImportData(tfxJSONObject json_data)
		{
			base.ImportData(json_data);
			
			m_force_position_override = json_data["m_force_position_override"].Boolean;
		}
		
#if UNITY_EDITOR
		public float DrawPositionEditorGUI(GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
		{
			float x_offset = position.x + ACTION_INDENT_LEVEL_1;
			float y_offset = DrawProgressionEditorHeader(label, position, offset_legal, unique_random_legal, bold_label, extra_options : ProgressionExtraOptions, extra_option_indexes : ProgressionExtraOptionIndexes);
			
			if(Progression == (int) ValueProgression.Constant)
			{
				Rect toggle_pos = new Rect();
				if(offset_legal)
				{
					toggle_pos = new Rect(x_offset + 190, y_offset - LINE_HEIGHT, 200, LINE_HEIGHT);
				}
				else
				{
					toggle_pos = new Rect(x_offset, y_offset, 200, LINE_HEIGHT);
					
					y_offset += LINE_HEIGHT;
				}
				m_force_position_override = EditorGUI.Toggle(toggle_pos, "Force This Position?", m_force_position_override);
			}
			else
			{
				// Force false
				m_force_position_override = false;
			}
			
			m_from = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), Progression == (int) ValueProgression.Constant ? "Vector" : "Vector From", m_from);
			y_offset += LINE_HEIGHT*2;
			
			if(Progression != (int) ValueProgression.Constant)
			{
				m_to = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Vector To", m_to);
				y_offset += LINE_HEIGHT*2;
				
				if(Progression == (int) ValueProgression.Eased && m_to_to_bool)
				{
					m_to_to = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Vector Then", m_to_to);
					y_offset += LINE_HEIGHT*2;
				}
				
				y_offset = DrawVector3CustomEaseCurveSettings(x_offset, y_offset);
			}
			
			return (y_offset) - position.y;
		}
#endif

	}

	[System.Serializable]
	public class ActionVector3Progression : ActionVariableProgression
	{
		[SerializeField]
		protected Vector3[] m_values;
		[SerializeField]
		protected Vector3 m_from = Vector3.zero;
		[SerializeField]
		protected Vector3 m_to = Vector3.zero;
		[SerializeField]
		protected Vector3 m_to_to = Vector3.zero;
		
		[SerializeField]
		protected bool m_ease_curve_per_axis = false;
		[SerializeField]
		protected AnimationCurve m_custom_ease_curve_y = new AnimationCurve();
		[SerializeField]
		protected AnimationCurve m_custom_ease_curve_z = new AnimationCurve();

		[SerializeField]
		protected PROGRESSION_VALUE_STATE m_value_state = PROGRESSION_VALUE_STATE.UNIQUE;
		[SerializeField]
		protected ActionVariableProgressionReferenceData m_offset_progression;

		public Vector3 ValueFrom { get { return m_from; } }
		public Vector3 ValueTo { get { return m_to; } }
		public Vector3 ValueThen { get { return m_to_to; } }
		public Vector3[] Values { get { return m_values; } set { m_values = value; } }

		public bool EaseCurvePerAxis { get { return m_ease_curve_per_axis; } }
		public AnimationCurve CustomEaseCurveY { get { return m_custom_ease_curve_y; } }
		public AnimationCurve CustomEaseCurveZ { get { return m_custom_ease_curve_z; } }

		public override bool UniqueRandom { get { return Progression == (int) ValueProgression.Random && m_unique_randoms && m_value_state != PROGRESSION_VALUE_STATE.REFERENCE; } }

		public ActionVariableProgressionReferenceData GetOffsetReference()
		{
			if(m_value_state == PROGRESSION_VALUE_STATE.UNIQUE)
				return m_reference_data;
			else
				return m_offset_progression;
		}

		public void SetValueReference(ActionVector3Progression progression)
		{
			m_value_state = PROGRESSION_VALUE_STATE.REFERENCE;
			m_offset_progression = progression.ReferenceData;
		}
		
		public void SetConstant( Vector3 constant_value )
		{
			m_progression_idx = (int) ValueProgression.Constant;
			m_from = constant_value;
		}
		
		public void SetRandom( Vector3 random_min, Vector3 random_max, bool unique_randoms = false)
		{
			m_progression_idx = (int) ValueProgression.Random;
			m_from = random_min;
			m_to = random_max;
			m_unique_randoms = unique_randoms;
		}

		public void SetEased( Vector3 eased_from, Vector3 eased_to)
		{
			SetEased (eased_from, eased_to, m_ease_type);
		}

		public void SetEased( Vector3 eased_from, Vector3 eased_to, Vector3 eased_then)
		{
			SetEased (eased_from, eased_to, eased_then, m_ease_type);
		}
		
		public void SetEased( Vector3 eased_from, Vector3 eased_to, EasingEquation easing_function)
		{
			m_progression_idx = (int) ValueProgression.Eased;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to_bool = false;
			m_ease_type = easing_function;
		}
		
		public void SetEased( Vector3 eased_from, Vector3 eased_to, Vector3 eased_then, EasingEquation easing_function)
		{
			m_progression_idx = (int) ValueProgression.Eased;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to = eased_then;
			m_to_to_bool = true;
			m_ease_type = easing_function;
		}

		public void SetEasedCustom ( Vector3 eased_from, Vector3 eased_to)
		{
			m_progression_idx = (int) ValueProgression.EasedCustom;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to_bool = false;
		}

		public void SetEasedCustom ( Vector3 eased_from, Vector3 eased_to, AnimationCurve easing_curve)
		{
			m_progression_idx = (int) ValueProgression.EasedCustom;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to_bool = false;
			
			m_ease_curve_per_axis = false;
			m_custom_ease_curve = easing_curve;
		}
		
		public void SetEasedCustom ( Vector3 eased_from, Vector3 eased_to, AnimationCurve easing_curve_x, AnimationCurve easing_curve_y, AnimationCurve easing_curve_z)
		{
			m_progression_idx = (int) ValueProgression.EasedCustom;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to_bool = false;
			
			m_ease_curve_per_axis = true;
			m_custom_ease_curve = easing_curve_x;
			m_custom_ease_curve_y = easing_curve_y;
			m_custom_ease_curve_z = easing_curve_z;
		}
		
		
		
		public Vector3 GetValue(List<LetterAction> all_letter_actions, AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default, bool consider_white_space = false)
		{
			int prog_index = GetProgressionIndex (progression_variables, animate_per_default, consider_white_space);

			return GetValue(all_letter_actions, prog_index);
		}
		
		Vector3 GetValue(List<LetterAction> all_letter_actions, int progression_idx)
		{
			Vector3 vecValue = Vector3.zero;

			if((m_value_state == PROGRESSION_VALUE_STATE.OFFSET_FROM_REFERENCE || m_value_state == PROGRESSION_VALUE_STATE.REFERENCE) && all_letter_actions != null)
			{
				ActionVector3Progression prog = m_offset_progression.GetVector3Prog(all_letter_actions);

				if(prog.m_reference_data.m_action_index == m_reference_data.m_action_index && prog.m_reference_data.m_start_state == m_reference_data.m_start_state)
				{
					// Referencing itself. Not good. Infinite Loop.

				}
				else
					vecValue = prog.GetValue(all_letter_actions, progression_idx);
			}

			if(m_value_state == PROGRESSION_VALUE_STATE.OFFSET_FROM_REFERENCE || m_value_state == PROGRESSION_VALUE_STATE.UNIQUE)
			{
				int num_vals = m_values.Length;
				if(num_vals > 1 && progression_idx < num_vals && progression_idx >= 0)
				{
					vecValue += m_values[progression_idx];
				}
				else if(num_vals==1)
				{
					vecValue += m_values[0];
				}
				else
				{
					vecValue += Vector3.zero;
				}
			}

			return vecValue;
		}
		
		public ActionVector3Progression()
		{
			
		}
		
		public ActionVector3Progression(Vector3 start_vec)
		{
			m_from = start_vec;
			m_to = start_vec;
			m_to_to = start_vec;
		}
		
		public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per, Vector3[] offset_vec)
		{
			int progression_idx = GetProgressionIndex(progression_variables, animate_per);
			bool constant_offset = offset_vec != null && offset_vec.Length == 1;
				
			m_values[progression_idx] = m_is_offset_from_last ? offset_vec[constant_offset ? 0 : progression_idx] : Vector3.zero;
			m_values[progression_idx] += new Vector3(m_from.x + (m_to.x - m_from.x) * UnityEngine.Random.value, m_from.y + (m_to.y - m_from.y) * UnityEngine.Random.value, m_from.z + (m_to.z - m_from.z) * UnityEngine.Random.value);
		}
		
		public void CalculateRotationProgressions (int num_progressions,
		                                           ActionVector3Progression offset_prog,
		                                           bool variableActive = true)
		{
			CalculateProgressions(num_progressions, offset_prog, variableActive);
		}


		public virtual void CalculateProgressions(int num_progressions,
		                                          ActionVector3Progression offset_prog,
		                                          bool variableActive = true)
		{
			if(!variableActive)
			{
				SetValueReference(offset_prog);
				return;
			}
			else if(m_is_offset_from_last && offset_prog != null)
			{
				m_value_state = PROGRESSION_VALUE_STATE.OFFSET_FROM_REFERENCE;
				m_offset_progression = offset_prog.GetOffsetReference();
			}
			else
				m_value_state = PROGRESSION_VALUE_STATE.UNIQUE;

			// Initialise the array of values. Array of only one if all progressions share the same constant value.
			m_values = new Vector3[Progression == (int) ValueProgression.Eased || Progression == (int) ValueProgression.EasedCustom || Progression == (int) ValueProgression.Random ? num_progressions : 1];

			// Calculate progression values
			if(Progression == (int) ValueProgression.Random)
			{
				for(int idx=0; idx < num_progressions; idx++)
				{
					m_values[idx] = new Vector3(m_from.x + (m_to.x - m_from.x) * UnityEngine.Random.value, m_from.y + (m_to.y - m_from.y) * UnityEngine.Random.value, m_from.z + (m_to.z - m_from.z) * UnityEngine.Random.value);
				}
			}
			else if(Progression == (int) ValueProgression.Eased)
			{
				float progression;
				
				for(int idx=0; idx < num_progressions; idx++)
				{
					progression = num_progressions == 1 ? 0 : (float)idx / ((float)num_progressions - 1f);
					
					if(m_to_to_bool)
					{
						if(progression <= 0.5f)
						{
							m_values[idx] = m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression/0.5f);
						}
						else
						{
							progression -= 0.5f;
							m_values[idx] = m_to + (m_to_to - m_to) * EasingManager.GetEaseProgress(EasingManager.GetEaseTypeOpposite(m_ease_type), progression/0.5f);
						}
					}
					else
					{
						m_values[idx] = m_from + (m_to - m_from) * EasingManager.GetEaseProgress(m_ease_type, progression);
					}
				}
				
			}
			else if(Progression == (int) ValueProgression.EasedCustom)
			{
				float progression;
				
				for(int idx=0; idx < num_progressions; idx++)
				{
					progression = num_progressions == 1 ? 0 : (float)idx / ((float)num_progressions - 1f);
					
					if(m_ease_curve_per_axis)
					{
						m_values[idx].x = m_from.x + (m_to.x - m_from.x) * m_custom_ease_curve.Evaluate(progression);
						m_values[idx].y = m_from.y + (m_to.y - m_from.y) * m_custom_ease_curve_y.Evaluate(progression);
						m_values[idx].z = m_from.z + (m_to.z - m_from.z) * m_custom_ease_curve_z.Evaluate(progression);
					}
					else
						m_values[idx] = m_from + (m_to - m_from) * m_custom_ease_curve.Evaluate(progression);
				}
			}
			else if(Progression == (int) ValueProgression.Constant)
			{
				for(int idx=0; idx < m_values.Length; idx++)
				{
					m_values[idx] = m_from;
				}
			}
		}
		
		public ActionVector3Progression Clone()
		{
			ActionVector3Progression vector3_progression = new ActionVector3Progression(Vector3.zero);
			
			vector3_progression.m_progression_idx = Progression;
			vector3_progression.m_ease_type = m_ease_type;
			vector3_progression.m_from = m_from;
			vector3_progression.m_to = m_to;
			vector3_progression.m_to_to = m_to_to;
			vector3_progression.m_to_to_bool = m_to_to_bool;
			vector3_progression.m_is_offset_from_last = m_is_offset_from_last;
			vector3_progression.m_unique_randoms = m_unique_randoms;
			vector3_progression.m_override_animate_per_option = m_override_animate_per_option;
			vector3_progression.m_animate_per = m_animate_per;
			vector3_progression.m_ease_curve_per_axis = m_ease_curve_per_axis;
			vector3_progression.m_custom_ease_curve = new AnimationCurve(m_custom_ease_curve.keys);
			vector3_progression.m_custom_ease_curve_y = new AnimationCurve(m_custom_ease_curve_y.keys);
			vector3_progression.m_custom_ease_curve_z = new AnimationCurve(m_custom_ease_curve_z.keys);
			
			return vector3_progression;
		}

		public override tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			ExportBaseData(ref json_data);
			
			json_data["m_from"] = m_from.ExportData();
			json_data["m_to"] = m_to.ExportData();
			json_data["m_to_to"] = m_to_to.ExportData();
			json_data["m_ease_curve_per_axis"] = m_ease_curve_per_axis;
			
			if(Progression == (int) ValueProgression.EasedCustom && m_ease_curve_per_axis)
			{
				json_data["m_custom_ease_curve_y"] = m_custom_ease_curve_y.ExportData();
				json_data["m_custom_ease_curve_z"] = m_custom_ease_curve_z.ExportData();
			}
			
			return new tfxJSONValue(json_data);
		}
		
		public override void ImportData(tfxJSONObject json_data)
		{
			m_from = json_data["m_from"].Obj.JSONtoVector3();
			m_to = json_data["m_to"].Obj.JSONtoVector3();
			m_to_to = json_data["m_to_to"].Obj.JSONtoVector3();
			m_ease_curve_per_axis = json_data["m_ease_curve_per_axis"].Boolean;
			if(json_data.ContainsKey("m_custom_ease_curve_y"))
			{
				m_custom_ease_curve_y = json_data["m_custom_ease_curve_y"].Array.JSONtoAnimationCurve();
				m_custom_ease_curve_z = json_data["m_custom_ease_curve_z"].Array.JSONtoAnimationCurve();
			}
			
			ImportBaseData(json_data);
			
		}
		
		public void ImportLegacyData(string data_string)
		{
			KeyValuePair<string, string> value_pair;
			List<object> obj_list = data_string.StringToList(';',':');
			
			foreach(object obj in obj_list)
			{
				value_pair = (KeyValuePair<string, string>) obj;
				
				switch(value_pair.Key)
				{
					case "m_from": m_from = value_pair.Value.StringToVector3('|','<'); break;
					case "m_to": m_to = value_pair.Value.StringToVector3('|','<'); break;
					case "m_to_to": m_to_to = value_pair.Value.StringToVector3('|','<'); break;
					case "m_ease_curve_per_axis": m_ease_curve_per_axis = bool.Parse(value_pair.Value); break;
					case "m_custom_ease_curve_y": m_custom_ease_curve_y = value_pair.Value.ToAnimationCurve(); break;
					case "m_custom_ease_curve_z": m_custom_ease_curve_z = value_pair.Value.ToAnimationCurve(); break;
					
					default :
						ImportBaseLagacyData(value_pair); break;
				}
			}
		}
		
#if UNITY_EDITOR
		public float DrawEditorGUI(GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
		{
			float x_offset = position.x + ACTION_INDENT_LEVEL_1;
			float y_offset = DrawProgressionEditorHeader(label, position, offset_legal, unique_random_legal, bold_label, extra_options : ProgressionExtraOptions, extra_option_indexes : ProgressionExtraOptionIndexes);
			
			m_from = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), Progression == (int) ValueProgression.Constant ? "Vector" : "Vector From", m_from);
			y_offset += LINE_HEIGHT*2;
			
			if(Progression != (int) ValueProgression.Constant)
			{
				m_to = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Vector To", m_to);
				y_offset += LINE_HEIGHT*2;
				
				if(Progression == (int) ValueProgression.Eased && m_to_to_bool)
				{
					m_to_to = EditorGUI.Vector3Field(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Vector Then", m_to_to);
					y_offset += LINE_HEIGHT*2;
				}
				
				y_offset = DrawVector3CustomEaseCurveSettings(x_offset, y_offset);
			}
			
			return (y_offset) - position.y;
		}

		public bool DrawQuickEditorGUI(PresetEffectSetting effect_setting, float gui_x_offset, ref float gui_y_offset, bool ignore_gui_changes, float inputsXOffset = 200)
		{
			Vector3 vector3_from = Vector3.zero, vector3_to = Vector3.zero, vector3_then = Vector3.zero;

			EditorGUI.LabelField (new Rect (gui_x_offset, gui_y_offset, 300, LINE_HEIGHT), effect_setting.m_setting_name);

			gui_y_offset += LINE_HEIGHT;

			vector3_from = EditorGUI.Vector3Field(new Rect(gui_x_offset + 15, gui_y_offset, 135, LINE_HEIGHT),"", ValueFrom);


			if(Progression == (int) ValueProgression.Eased || Progression == (int) ValueProgression.EasedCustom || Progression == (int) ValueProgression.Random)
			{
				EditorGUI.LabelField (new Rect (gui_x_offset + 160, gui_y_offset, 20, LINE_HEIGHT), "->", EditorStyles.boldLabel);
				
				vector3_to = EditorGUI.Vector3Field(new Rect(gui_x_offset + 190, gui_y_offset, 135, LINE_HEIGHT), "", ValueTo);

				if(Progression == (int) ValueProgression.Eased && m_to_to_bool)
				{
					EditorGUI.LabelField (new Rect (gui_x_offset + 335, gui_y_offset, 20, LINE_HEIGHT), "->", EditorStyles.boldLabel);
					vector3_then = EditorGUI.Vector3Field(new Rect(gui_x_offset + 365, gui_y_offset, 135, LINE_HEIGHT), "", ValueThen);
				}
			}

			gui_y_offset += LINE_HEIGHT;

			if(GUI.changed && !ignore_gui_changes)
			{
				if(Progression == (int) ValueProgression.Constant)
					SetConstant(vector3_from);
				else if(Progression == (int) ValueProgression.Eased)
				{
					if(m_to_to_bool)
						SetEased(vector3_from, vector3_to, vector3_then);
					else
						SetEased(vector3_from, vector3_to);
				}
				else if(Progression == (int) ValueProgression.Random)
					SetRandom(vector3_from, vector3_to);
				else if(Progression == (int) ValueProgression.EasedCustom)
					SetEasedCustom(vector3_from, vector3_to);
				
				return true;
			}
			
			return false;
		}
		
		protected float DrawVector3CustomEaseCurveSettings(float x_offset, float y_offset)
		{
			if(Progression == (int) ValueProgression.EasedCustom)
			{
				EditorGUI.LabelField(new Rect(x_offset + VECTOR_3_WIDTH + 5, y_offset+1, 70, LINE_HEIGHT), new GUIContent("Per Axis?", "Enables the definition of a custom animation easing curve for each axis (x,y,z)."));
				m_ease_curve_per_axis = EditorGUI.Toggle(new Rect(x_offset + VECTOR_3_WIDTH + 75, y_offset, 20, LINE_HEIGHT), m_ease_curve_per_axis);
				m_custom_ease_curve = EditorGUI.CurveField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve" + (m_ease_curve_per_axis ? " (x)" : ""), m_custom_ease_curve );
				y_offset += LINE_HEIGHT * 1.2f;
				
				if(m_ease_curve_per_axis)
				{
					m_custom_ease_curve_y = EditorGUI.CurveField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve (y)", m_custom_ease_curve_y );
					y_offset += LINE_HEIGHT * 1.2f;
					m_custom_ease_curve_z = EditorGUI.CurveField(new Rect(x_offset, y_offset, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve (z)", m_custom_ease_curve_z );
					y_offset += LINE_HEIGHT * 1.2f;
				}
			}
			
			return y_offset;
		}
#endif

		public List<PresetEffectSetting.VariableStateListener> GetStateListeners()
		{
			List<PresetEffectSetting.VariableStateListener> variableListeners = new List<PresetEffectSetting.VariableStateListener>();

			if (Progression == (int)ValueProgression.Constant)
			{
				variableListeners.Add(new PresetEffectSetting.VariableStateListener()
				{
					m_type = PresetEffectSetting.VariableStateListener.TYPE.VECTOR3,
					m_startVector3Value = m_from,
					m_onVector3StateChangeCallback = (Vector3 value) => {
						SetConstant(value);
					}
				});
			}
			else
			{
				variableListeners.Add(new PresetEffectSetting.VariableStateListener()
				{
					m_type = PresetEffectSetting.VariableStateListener.TYPE.VECTOR3,
					m_startVector3Value = m_from,
					m_onVector3StateChangeCallback = (Vector3 value) => {
						if (Progression == (int)ValueProgression.Eased)
							if (m_to_to_bool)
								SetEased(value, m_to, m_to_to);
							else
								SetEased(value, m_to);
						else if (Progression == (int)ValueProgression.EasedCustom)
							SetEasedCustom(value, m_to);
						else if (Progression == (int)ValueProgression.Random)
							SetRandom(value, m_to);

					}
				});
				variableListeners.Add(new PresetEffectSetting.VariableStateListener()
				{
					m_type = PresetEffectSetting.VariableStateListener.TYPE.VECTOR3,
					m_startVector3Value = m_to,
					m_onVector3StateChangeCallback = (Vector3 value) => {
						if (Progression == (int)ValueProgression.Eased)
							if (m_to_to_bool)
								SetEased(m_from, value, m_to_to);
							else
								SetEased(m_from, value);
						else if (Progression == (int)ValueProgression.EasedCustom)
							SetEasedCustom(m_from, value);
						else if (Progression == (int)ValueProgression.Random)
							SetRandom(m_from, value);
					}
				});
				if (Progression == (int)ValueProgression.Eased && m_to_to_bool)
				{
					variableListeners.Add(new PresetEffectSetting.VariableStateListener()
					{
						m_type = PresetEffectSetting.VariableStateListener.TYPE.VECTOR3,
						m_startVector3Value = m_to_to,
						m_onVector3StateChangeCallback = (Vector3 value) => {
							SetEased(m_from, m_to, value);
						}
					});
				}
			}

			return variableListeners;
		}
	}


	[System.Serializable]
	public class ActionColorProgression : ActionVariableProgression
	{
		[SerializeField]
		VertexColour[] m_values;
		[SerializeField]
		VertexColour m_from = new VertexColour();
		[SerializeField]
		VertexColour m_to = new VertexColour();
		[SerializeField]
		VertexColour m_to_to = new VertexColour();
		[SerializeField]
		bool m_override_alpha = false;
		[SerializeField]
		bool m_use_colour_gradients = false;

		[SerializeField]
		protected PROGRESSION_VALUE_STATE m_value_state = PROGRESSION_VALUE_STATE.UNIQUE;
		[SerializeField]
		protected ActionVariableProgressionReferenceData m_offset_progression;
		
		public VertexColour ValueFrom { get { return m_use_colour_gradients ? m_from : m_from.FlatColour; } }
		public VertexColour ValueTo { get { return m_use_colour_gradients ? m_to : m_to.FlatColour; } }
		public VertexColour ValueThen { get { return m_use_colour_gradients ? m_to_to : m_to_to.FlatColour; } }
		public VertexColour[] Values { get { return m_values; } set { m_values = value; } }
		public bool UseColourGradients { get { return m_use_colour_gradients; } set { m_use_colour_gradients = value; } }

		public override bool UniqueRandom { get { return Progression == (int) ValueProgression.Random && m_unique_randoms && m_value_state != PROGRESSION_VALUE_STATE.REFERENCE; } }

		ActionColorProgression cachedColourProgression;

		public ActionColorProgression (VertexColour start_colour)
		{
			m_from = start_colour.Clone();
			m_to = start_colour.Clone();
			m_to_to = start_colour.Clone();
		}

		public ActionColorProgression (VertexColour start_colour, bool offsetFromLast)
		{
			m_from = start_colour.Clone();
			m_to = start_colour.Clone();
			m_to_to = start_colour.Clone();

			m_is_offset_from_last = offsetFromLast;
		}

		public ActionVariableProgressionReferenceData GetOffsetReference()
		{
			if(m_value_state == PROGRESSION_VALUE_STATE.UNIQUE)
				return m_reference_data;
			else
				return m_offset_progression;
		}
		
		public void SetValueReference(ActionColorProgression progression)
		{
			m_value_state = PROGRESSION_VALUE_STATE.REFERENCE;
			m_offset_progression = progression.ReferenceData;
		}

        public void SetConstant(Color constant_value)
        {
            m_progression_idx = (int)ValueProgression.Constant;
            m_from = new VertexColour(constant_value);
            m_use_colour_gradients = false;
        }

        public void SetConstant( VertexColour constant_value )
		{
			m_progression_idx = (int) ValueProgression.Constant;
			m_from = constant_value;
            m_use_colour_gradients = true;
		}

        public void SetRandom(Color random_min, Color random_max, bool unique_randoms = false)
        {
            m_progression_idx = (int)ValueProgression.Random;
            m_from = new VertexColour( random_min );
            m_to = new VertexColour( random_max );
            m_unique_randoms = unique_randoms;
            m_use_colour_gradients = false;
        }

        public void SetRandom( VertexColour random_min, VertexColour random_max, bool unique_randoms = false)
		{
			m_progression_idx = (int) ValueProgression.Random;
			m_from = random_min;
			m_to = random_max;
			m_unique_randoms = unique_randoms;
            m_use_colour_gradients = true;
        }

		public void SetEased( VertexColour eased_from, VertexColour eased_to)
		{
			SetEased (eased_from, eased_to, m_ease_type);
		}

		public void SetEased( VertexColour eased_from, VertexColour eased_to, VertexColour eased_then)
		{
			SetEased (eased_from, eased_to, eased_then, m_ease_type);
		}

		public void SetEased( VertexColour eased_from, VertexColour eased_to, EasingEquation easing_function)
		{
			m_progression_idx = (int) ValueProgression.Eased;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to_bool = false;
			m_ease_type = easing_function;
            m_use_colour_gradients = true;
        }
		
		public void SetEased( VertexColour eased_from, VertexColour eased_to, VertexColour eased_then, EasingEquation easing_function)
		{
			m_progression_idx = (int) ValueProgression.Eased;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to = eased_then;
			m_to_to_bool = true;
			m_ease_type = easing_function;
            m_use_colour_gradients = true;
        }

		public void SetEasedCustom ( VertexColour eased_from, VertexColour eased_to)
		{
			SetEasedCustom (eased_from, eased_to, m_custom_ease_curve);
		}

		public void SetEasedCustom ( VertexColour eased_from, VertexColour eased_to, AnimationCurve easing_curve)
		{
			m_progression_idx = (int) ValueProgression.EasedCustom;
			m_from = eased_from;
			m_to = eased_to;
			m_to_to_bool = false;
			
			m_custom_ease_curve = easing_curve;
            m_use_colour_gradients = true;
        }

		public void SetValues( VertexColour[] colValues)
		{
			m_values = colValues;
			m_progression_idx = (int) ValueProgression.Eased;
            m_use_colour_gradients = true;
        }

		public void GetValue(ref VertexColour colValue, List<LetterAction> all_letter_actions, AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per_default, ActionColorProgression defaultAnimColourProg)
		{
			GetValue(ref colValue, all_letter_actions, GetProgressionIndex(progression_variables,animate_per_default), defaultAnimColourProg);
		}

		public void GetValue(ref VertexColour colValue, List<LetterAction> all_letter_actions, int progression_idx, ActionColorProgression defaultAnimColourProg) 
		{
			colValue.Clear();
			
			if((m_value_state == PROGRESSION_VALUE_STATE.OFFSET_FROM_REFERENCE || m_value_state == PROGRESSION_VALUE_STATE.REFERENCE))
			{
				cachedColourProgression = m_offset_progression.GetColourProg(all_letter_actions, defaultAnimColourProg);
				
				if(cachedColourProgression == null || cachedColourProgression.m_reference_data.m_action_index == m_reference_data.m_action_index && cachedColourProgression.m_reference_data.m_start_state == m_reference_data.m_start_state)
				{
					// Referencing itself. Not good. Infinite Loop.
					
				}
				else
				{
					cachedColourProgression.GetValue(ref colValue, all_letter_actions, progression_idx, defaultAnimColourProg);
					
					if(m_value_state == PROGRESSION_VALUE_STATE.OFFSET_FROM_REFERENCE && m_override_alpha)
						colValue.ClearAlpha();
				}
			}
			
			if(m_value_state == PROGRESSION_VALUE_STATE.OFFSET_FROM_REFERENCE || m_value_state == PROGRESSION_VALUE_STATE.UNIQUE)
			{
				if(m_values.Length > 1 && progression_idx < m_values.Length)
				{
					colValue.AddInLine( m_values[progression_idx] );
				}
				else if(m_values.Length==1)
				{
					colValue.AddInLine( m_values[0] );
				}
//				else
//				{
//					colValue = colValue.Add(new VertexColour(Color.clear) );
//				}
			}
		}
		
		public void ConvertFromFlatColourProg(ActionColorProgression flat_colour_progression)
		{
			m_progression_idx = flat_colour_progression.Progression;
			m_ease_type = flat_colour_progression.EaseType;
			m_from = new VertexColour(flat_colour_progression.ValueFrom);
			m_to = new VertexColour(flat_colour_progression.ValueTo);
			m_to_to = new VertexColour(flat_colour_progression.ValueThen);
			m_to_to_bool = flat_colour_progression.UsingThirdValue;
			m_is_offset_from_last = flat_colour_progression.IsOffsetFromLast;
			m_unique_randoms = flat_colour_progression.UniqueRandom;
		}
		
		public void CalculateUniqueRandom(AnimationProgressionVariables progression_variables, AnimatePerOptions animate_per, VertexColour[] offset_colours)
		{
			int progression_idx = GetProgressionIndex(progression_variables, animate_per);
			bool constant_offset = offset_colours != null && offset_colours.Length == 1;
				
			m_values[progression_idx] = m_is_offset_from_last ? offset_colours[constant_offset ? 0 : progression_idx].Clone() : new VertexColour(new Color(0,0,0,0));
			m_values[progression_idx] = m_values[progression_idx].Add(m_from.Add(m_to.Sub(m_from).Multiply(UnityEngine.Random.value)));
		}
		
		public void CalculateProgressions(int num_progressions, ActionColorProgression offset_prog, bool variableActive = true)
		{
			if(!variableActive)
			{
				SetValueReference(offset_prog);
				return;
			}
			else if(m_is_offset_from_last)
			{
				m_value_state = PROGRESSION_VALUE_STATE.OFFSET_FROM_REFERENCE;
				m_offset_progression = offset_prog.GetOffsetReference();
			}
			else
				m_value_state = PROGRESSION_VALUE_STATE.UNIQUE;
			
			
			// Initialise the array of values. Array of only one if all progressions share the same constant value.
			m_values = new VertexColour[Progression == (int) ValueProgression.Eased || Progression == (int) ValueProgression.EasedCustom || Progression == (int) ValueProgression.Random ? num_progressions : 1];
			
			// Calculate progression values
			if(Progression == (int) ValueProgression.Random)
			{
				for(int idx=0; idx < num_progressions; idx++)
				{
					m_values[idx] = ValueFrom.Add(ValueTo.Sub(ValueFrom).Multiply(UnityEngine.Random.value));
				}
			}
			else if(Progression == (int) ValueProgression.Eased)
			{
				float progression;
				
				for(int idx=0; idx < num_progressions; idx++)
				{
					progression = num_progressions == 1 ? 0 : (float)idx / ((float)num_progressions - 1f);
				
					if(m_to_to_bool)
					{
						if(progression  <= 0.5f)
						{
							m_values[idx] = ValueFrom.Add((ValueTo.Sub(ValueFrom)).Multiply(EasingManager.GetEaseProgress(m_ease_type, progression/0.5f)));
						}
						else
						{
							progression -= 0.5f;
							m_values[idx] = ValueTo.Add((ValueThen.Sub(ValueTo)).Multiply(EasingManager.GetEaseProgress(m_ease_type, progression/0.5f)));
						}
					}
					else
					{
						m_values[idx] = ValueFrom.Add((ValueTo.Sub(ValueFrom)).Multiply(EasingManager.GetEaseProgress(m_ease_type, progression)));
					}
				}
			}
			else if(Progression == (int) ValueProgression.EasedCustom)
			{
				float progression;
				
				for(int idx=0; idx < num_progressions; idx++)
				{
					progression = num_progressions == 1 ? 0 : (float)idx / ((float)num_progressions - 1f);
					
					m_values[idx] = ValueFrom.Add((ValueTo.Sub(ValueFrom)).Multiply(m_custom_ease_curve.Evaluate(progression)));
				}
			}
			else if(Progression == (int) ValueProgression.Constant)
			{
				for(int idx=0; idx < m_values.Length; idx++)
				{
					m_values[idx] = ValueFrom;
				}
			}
		}
		
		public ActionColorProgression Clone()
		{
			ActionColorProgression color_progression = new ActionColorProgression(new VertexColour());
			
			color_progression.m_progression_idx = Progression;
			color_progression.m_ease_type = m_ease_type;
			color_progression.m_from = m_from.Clone();
			color_progression.m_to = m_to.Clone();
			color_progression.m_to_to = m_to_to.Clone();
			color_progression.m_to_to_bool = m_to_to_bool;
			color_progression.m_is_offset_from_last = m_is_offset_from_last;
			color_progression.m_unique_randoms = m_unique_randoms;
			color_progression.m_override_animate_per_option = m_override_animate_per_option;
			color_progression.m_animate_per = m_animate_per;
			color_progression.m_use_colour_gradients = m_use_colour_gradients;
			color_progression.m_override_alpha = m_override_alpha;
			
			return color_progression;
		}

		public override tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			ExportBaseData(ref json_data);
			
			json_data["m_from"] = m_from.ExportData();
			json_data["m_to"] = m_to.ExportData();
			json_data["m_to_to"] = m_to_to.ExportData();
			json_data["m_use_colour_gradients"] = m_use_colour_gradients;
			json_data ["m_override_alpha"] = m_override_alpha;

			return new tfxJSONValue(json_data);
		}
		
		public override void ImportData(tfxJSONObject json_data)
		{
			m_from = json_data["m_from"].Obj.JSONtoVertexColour();
			m_to = json_data["m_to"].Obj.JSONtoVertexColour();
			m_to_to = json_data["m_to_to"].Obj.JSONtoVertexColour();

			if(json_data.ContainsKey("m_use_colour_gradients"))
				m_use_colour_gradients = json_data["m_use_colour_gradients"].Boolean;
			else
				m_use_colour_gradients = false;

			if(json_data.ContainsKey("m_override_alpha"))
				m_override_alpha = json_data["m_override_alpha"].Boolean;
			else
				m_override_alpha = false;
			
			ImportBaseData(json_data);
		}
		
//		public void ImportLegacyData(string data_string)
//		{
//			KeyValuePair<string, string> value_pair;
//			List<object> obj_list = data_string.StringToList(';',':');
//			
//			foreach(object obj in obj_list)
//			{
//				value_pair = (KeyValuePair<string, string>) obj;
//				
//				switch(value_pair.Key)
//				{
//				case "m_from": m_from = value_pair.Value.StringToVertexColor('|','<','^'); break;
//				case "m_to": m_to = value_pair.Value.StringToVertexColor('|','<','^'); break;
//				case "m_to_to": m_to_to = value_pair.Value.StringToVertexColor('|','<','^'); break;
//					
//				default :
//					ImportBaseLagacyData(value_pair); break;
//				}
//			}
//		}

		public void ImportLegacyData(string data_string)
		{
			KeyValuePair<string, string> value_pair;
			List<object> obj_list = data_string.StringToList(';',':');
			
			foreach(object obj in obj_list)
			{
				value_pair = (KeyValuePair<string, string>) obj;
				
				switch(value_pair.Key)
				{
				case "m_from": m_from = new VertexColour( value_pair.Value.StringToColor('|','<')); break;
				case "m_to": m_to = new VertexColour( value_pair.Value.StringToColor('|','<')); break;
				case "m_to_to": m_to_to = new VertexColour(value_pair.Value.StringToColor('|','<')); break;
					
				default :
					ImportBaseLagacyData(value_pair); break;
				}
			}
		}

		
	#if UNITY_EDITOR
		public float DrawEditorGUI(GUIContent label, Rect position, bool offset_legal, bool unique_random_legal = false, bool bold_label = true)
		{
			float x_offset = position.x + ACTION_INDENT_LEVEL_1;
			float y_offset = DrawProgressionEditorHeader(label, position, offset_legal, unique_random_legal, bold_label, extra_options : ProgressionExtraOptions, extra_option_indexes : ProgressionExtraOptionIndexes);

			EditorGUI.LabelField (new Rect (x_offset + 190, y_offset - LINE_HEIGHT, 100, LINE_HEIGHT), "Override Alpha?");
			m_override_alpha = EditorGUI.Toggle(new Rect (x_offset + 300, y_offset - LINE_HEIGHT, 20, LINE_HEIGHT), m_override_alpha);

			m_use_colour_gradients = EditorGUI.Toggle (new Rect (x_offset, y_offset, 200, LINE_HEIGHT), "Colour Gradients?", m_use_colour_gradients);
			y_offset += LINE_HEIGHT * 1.1f;

			EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT * 2), Progression == (int) ValueProgression.Constant ? "Colour" : "Colour\nFrom", EditorStyles.miniBoldLabel);
			x_offset += 60;

			if(!m_use_colour_gradients)
			{
				m_from.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), m_from.top_left);
			}
			else
			{
				m_from.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), m_from.top_left);
				m_from.bottom_left = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), m_from.bottom_left);
				x_offset += 45;
				m_from.top_right = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), m_from.top_right);
				m_from.bottom_right = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), m_from.bottom_right);
			}
			
			
			if(Progression != (int) ValueProgression.Constant)
			{
				x_offset += 65;
				
				EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT*2), "Colour\nTo", EditorStyles.miniBoldLabel);
				x_offset += 60;


				if(!m_use_colour_gradients)
				{
					m_to.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), m_to.top_left);
				}
				else
				{
					m_to.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), m_to.top_left);
					m_to.bottom_left = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), m_to.bottom_left);
					x_offset += 45;
					m_to.top_right = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), m_to.top_right);
					m_to.bottom_right = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), m_to.bottom_right);
				}
				
				
				if(Progression == (int) ValueProgression.Eased && m_to_to_bool)
				{
					x_offset += 65;
				
					EditorGUI.LabelField(new Rect(x_offset, y_offset, 50, LINE_HEIGHT*2), "Colour\nThen To", EditorStyles.miniBoldLabel);
					x_offset += 60;

					if(!m_use_colour_gradients)
					{
						m_to_to.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), m_to_to.top_left);
					}
					else
					{
						m_to_to.top_left = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), m_to_to.top_left);
						m_to_to.bottom_left = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), m_to_to.bottom_left);
						x_offset += 45;
						m_to_to.top_right = EditorGUI.ColorField(new Rect(x_offset, y_offset, LINE_HEIGHT*2, LINE_HEIGHT), m_to_to.top_right);
						m_to_to.bottom_right = EditorGUI.ColorField(new Rect(x_offset, y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), m_to_to.bottom_right);
					}
				}
				
				if(Progression == (int) ValueProgression.EasedCustom)
				{
					m_custom_ease_curve = EditorGUI.CurveField(new Rect(position.x + ACTION_INDENT_LEVEL_1, y_offset + LINE_HEIGHT * 2 + 10, VECTOR_3_WIDTH, LINE_HEIGHT), "Ease Curve", m_custom_ease_curve );
					y_offset += LINE_HEIGHT * 1.2f;
				}
			}
			
			return (y_offset + LINE_HEIGHT * (m_use_colour_gradients ? 2 : 1) + 10) - position.y;
		}

		public bool DrawQuickEditorGUI(PresetEffectSetting effect_setting, float gui_x_offset, ref float gui_y_offset, bool ignore_gui_changes)
		{
			VertexColour colorFrom = new VertexColour (), colorTo = new VertexColour (), colorThen = new VertexColour ();

			EditorGUI.LabelField (new Rect (gui_x_offset, gui_y_offset, 300, LINE_HEIGHT), effect_setting.m_setting_name);
			
			gui_y_offset += LINE_HEIGHT;
			gui_x_offset += 15;

			if(!m_use_colour_gradients)
			{
				colorFrom.top_left = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset, LINE_HEIGHT*2, LINE_HEIGHT), ValueFrom.top_left);
				gui_x_offset += 45;
			}
			else
			{
				colorFrom.top_left = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset, LINE_HEIGHT*2, LINE_HEIGHT), ValueFrom.top_left);
				colorFrom.bottom_left = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), ValueFrom.bottom_left);
				gui_x_offset += 45;
				colorFrom.top_right = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset, LINE_HEIGHT*2, LINE_HEIGHT), ValueFrom.top_right);
				colorFrom.bottom_right = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), ValueFrom.bottom_right);

				gui_x_offset += 45;
			}


			if(Progression == (int) ValueProgression.Eased || Progression == (int) ValueProgression.EasedCustom || Progression == (int) ValueProgression.Random)
			{
				EditorGUI.LabelField (new Rect (gui_x_offset + 5, gui_y_offset + (m_use_colour_gradients ? 10 : 0), 20, LINE_HEIGHT), "->", EditorStyles.boldLabel);

				gui_x_offset += 35;
				
				if(!m_use_colour_gradients)
				{
					colorTo.top_left = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset, LINE_HEIGHT*2, LINE_HEIGHT), ValueTo.top_left);
					gui_x_offset += 45;
				}
				else
				{
					colorTo.top_left = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset, LINE_HEIGHT*2, LINE_HEIGHT), ValueTo.top_left);
					colorTo.bottom_left = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), ValueTo.bottom_left);
					gui_x_offset += 45;
					colorTo.top_right = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset, LINE_HEIGHT*2, LINE_HEIGHT), ValueTo.top_right);
					colorTo.bottom_right = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), ValueTo.bottom_right);
					gui_x_offset += 45;
				}
				
				if(Progression == (int) ValueProgression.Eased && m_to_to_bool)
				{
					EditorGUI.LabelField (new Rect (gui_x_offset + 5, gui_y_offset + (m_use_colour_gradients ? 10 : 0), 20, LINE_HEIGHT), "->", EditorStyles.boldLabel);
					
					gui_x_offset += 35;

					if(!m_use_colour_gradients)
					{
						colorThen.top_left = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset, LINE_HEIGHT*2, LINE_HEIGHT), ValueThen.top_left);
						gui_x_offset += 45;
					}
					else
					{
						colorThen.top_left = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset, LINE_HEIGHT*2, LINE_HEIGHT), ValueThen.top_left);
						colorThen.bottom_left = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), ValueThen.bottom_left);
						gui_x_offset += 45;
						colorThen.top_right = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset, LINE_HEIGHT*2, LINE_HEIGHT), ValueThen.top_right);
						colorThen.bottom_right = EditorGUI.ColorField(new Rect(gui_x_offset, gui_y_offset + LINE_HEIGHT, LINE_HEIGHT*2, LINE_HEIGHT), ValueThen.bottom_right);
						gui_x_offset += 45;
					}
				}
			}

			if(m_use_colour_gradients)
				gui_y_offset += 2 * LINE_HEIGHT;
			else
				gui_y_offset += LINE_HEIGHT;


			if(GUI.changed && !ignore_gui_changes)
			{
				if(Progression == (int) ValueProgression.Constant)
					SetConstant(colorFrom);
				else if(Progression == (int) ValueProgression.Eased)
				{
					if(m_to_to_bool)
						SetEased(colorFrom, colorTo, colorThen);
					else
						SetEased(colorFrom, colorTo);
				}
				else if(Progression == (int) ValueProgression.Random)
					SetRandom(colorFrom, colorTo);
				else if(Progression == (int) ValueProgression.EasedCustom)
					SetEasedCustom(colorFrom, colorTo);
				
				return true;
			}
			
			return false;
		}
#endif

		public List<PresetEffectSetting.VariableStateListener> GetStateListeners()
		{
			List<PresetEffectSetting.VariableStateListener> variableListeners = new List<PresetEffectSetting.VariableStateListener>();

			if (Progression == (int)ValueProgression.Constant)
			{
				variableListeners.Add(new PresetEffectSetting.VariableStateListener()
				{
					m_startColorValue = m_from.top_left,
					m_onColorStateChangeCallback = (Color value) => {
						SetConstant(value);
					}
				});
			}
			else
			{
				variableListeners.Add(new PresetEffectSetting.VariableStateListener()
				{
					m_startColorValue = m_from.top_left,
					m_onColorStateChangeCallback = (Color value) => {
						if (Progression == (int)ValueProgression.Eased)
							if (m_to_to_bool)
								SetEased(new VertexColour(value), m_to, m_to_to);
							else
								SetEased(new VertexColour(value), m_to);
						else if (Progression == (int)ValueProgression.EasedCustom)
							SetEasedCustom(new VertexColour(value), m_to);
						else if (Progression == (int)ValueProgression.Random)
							SetRandom(value, m_to.top_left);

					}
				});
				variableListeners.Add(new PresetEffectSetting.VariableStateListener()
				{
					m_startColorValue = m_to.top_left,
					m_onColorStateChangeCallback = (Color value) => {
						if (Progression == (int)ValueProgression.Eased)
							if (m_to_to_bool)
								SetEased(m_from, new VertexColour(value), m_to_to);
							else
								SetEased(m_from, new VertexColour(value));
						else if (Progression == (int)ValueProgression.EasedCustom)
							SetEasedCustom(m_from, new VertexColour(value));
						else if (Progression == (int)ValueProgression.Random)
							SetRandom(m_from.top_left, value);
					}
				});
				if (Progression == (int)ValueProgression.Eased && m_to_to_bool)
				{
					variableListeners.Add(new PresetEffectSetting.VariableStateListener()
					{
						m_startColorValue = m_to_to.top_left,
						m_onColorStateChangeCallback = (Color value) => {
							SetEased(m_from, m_to, new VertexColour(value));
						}
					});
				}
			}

			return variableListeners;
		}
    }
}