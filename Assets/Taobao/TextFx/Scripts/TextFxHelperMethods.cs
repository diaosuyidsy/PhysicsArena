using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif
using System.IO;
using System;
using Boomlagoon.TextFx.JSON;

namespace TextFx
{
	public enum OBJ_POS
	{
		CENTER,
		TOP_LEFT,
		TOP_RIGHT,
		BOTTOM_RIGHT,
		BOTTOM_LEFT
	}
	
	public enum TRANSFORM_SPACE
	{
		LOCAL,
		WORLD
	}
	
	public enum LETTERS_TO_ANIMATE
	{
		ALL_LETTERS,
		FIRST_LETTER,
		FIRST_LETTER_WORDS,
		FIRST_LETTER_LINES,
		LAST_LETTER,
		LAST_LETTER_WORDS,
		LAST_LETTER_LINES,
		CUSTOM,
		FIRST_WORD,
		LAST_WORD,
		NTH_WORD,
		FIRST_LINE,
		LAST_LINE,
		NTH_LINE
	}
	
	public enum TextDisplayAxis
	{
		HORIZONTAL,
		VERTICAL
	}
	
	public enum AnimationTime
	{
		GAME_TIME,
		REAL_TIME
	}
	
	public enum AnimatePerOptions
	{
		LETTER,
		WORD,
		LINE
	}
	
	public enum LETTER_ANIMATION_STATE
	{
		PLAYING,
		STOPPED,
		WAITING,
		WAITING_INFINITE,
		CONTINUING,
		CONTINUING_FINISHED
	}
	
	public enum ON_FINISH_ACTION
	{
		NONE,
		DISABLE_OBJECT,
		DESTROY_OBJECT,
		RESET_ANIMATION
	}

	public enum ANIMATION_DATA_TYPE
	{
		NONE,
		ALL,
		TEXT_CHANGED,
		ANIMATE_ON,
		ACTION_TYPE,
		LETTER_ANCHOR,
		EASE_TYPE,
		COLOUR,
		POSITION,
		LOCAL_ROTATION,
		GLOBAL_ROTATION,
		LOCAL_SCALE,
		GLOBAL_SCALE,
		FORCE_SAME_START,
		WHITE_SPACES_DELAYS,
		DELAY,
		DURATION,
		AUDIO_EFFECTS,
		PARTICLE_EFFECTS,
		DELAY_EASED_RANDOM_SWITCH,
		NUM_LOOP_ITERATIONS
	}

	public enum ContinueType
	{
		None,
		Instant,
		EndOfLoop
//		,EndOfLoopSkipBreaks
	}

	[Serializable]
	public class TextSizeData
	{
		public float m_text_line_width;
		public float m_text_line_height;
		public float m_total_text_width;
		public float m_total_text_height;
		public float m_line_height_offset;
		public float m_y_max;
		
		public TextSizeData(float text_line_width, float text_line_height, float line_height_offset, float y_max)
		{
			m_text_line_width = text_line_width;
			m_text_line_height = text_line_height;
			m_line_height_offset = line_height_offset;
			m_y_max = y_max;
		}
	}

	[Serializable]
	public class AxisEasingOverrideData
	{
		public bool m_override_default = false;
		public EasingEquation m_x_ease = EasingEquation.Linear;
		public EasingEquation m_y_ease = EasingEquation.Linear;
		public EasingEquation m_z_ease = EasingEquation.Linear;
		
		public AxisEasingOverrideData Clone()
		{
			AxisEasingOverrideData axis_data = new AxisEasingOverrideData();
			axis_data.m_override_default = m_override_default;
			axis_data.m_x_ease = m_x_ease;
			axis_data.m_y_ease = m_y_ease;
			axis_data.m_z_ease = m_z_ease;
			return axis_data;
		}
		
		public tfxJSONValue ExportData()
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			json_data["m_override_default"] = m_override_default;
			json_data["m_x_ease"] = (int) m_x_ease;
			json_data["m_y_ease"] = (int) m_y_ease;
			json_data["m_z_ease"] = (int) m_z_ease;
			
			return new tfxJSONValue(json_data);
		}
		
		public void ImportData(tfxJSONObject json_data)
		{
			m_override_default = json_data["m_override_default"].Boolean;
			m_x_ease = (EasingEquation) (int) json_data["m_x_ease"].Number;
			m_y_ease = (EasingEquation) (int) json_data["m_y_ease"].Number;
			m_z_ease = (EasingEquation) (int) json_data["m_z_ease"].Number;
		}
	}

	[Serializable]
	public class AnimationProgressionVariables
	{
		[SerializeField]
		int m_letter_value;
		[SerializeField]
		int m_letter_value_inc_white_space;		// The letter index, with white space chars counted as well
		[SerializeField]
		int m_word_value;
		[SerializeField]
		int m_line_value;

		public int LetterValue { get { return m_letter_value; } }
		public int WordValue { get { return m_word_value; } set { m_word_value = value; } }
		public int LineValue { get { return m_line_value; } set { m_line_value = value; } }

		public AnimationProgressionVariables(int letter_val, int word_val, int line_val)
		{
			m_letter_value = letter_val;
			m_word_value = word_val;
			m_line_value = line_val;
		}

		public void SetLetterValue(int letter_idx, int letter_idx_inc_white_space = -1)
		{
			m_letter_value = letter_idx;

			m_letter_value_inc_white_space = letter_idx_inc_white_space < 0 ? letter_idx : letter_idx_inc_white_space;
		}
		
		public int GetValue(AnimatePerOptions animate_per, bool consider_white_space = false)
		{
			switch(animate_per)
			{
			case AnimatePerOptions.LETTER:
				return consider_white_space ? m_letter_value_inc_white_space : m_letter_value;
			case AnimatePerOptions.WORD:
				return m_word_value;
			case AnimatePerOptions.LINE:
				return m_line_value;
			}
			
			return m_letter_value;
		}
	}

	public class CustomCharacterInfo
	{
		public bool flipped = false;
		public Rect uv = new Rect();
		public Rect vert = new Rect();
		public float width = 0;

		public CustomCharacterInfo() {}

		public CustomCharacterInfo(CustomCharacterInfo char_info)
		{
			flipped = char_info.flipped;
			uv = char_info.uv;
			vert = char_info.vert;
			width = char_info.width;
		}

		public void ScaleClone(float scale, ref CustomCharacterInfo char_info)
		{
			char_info.flipped = flipped;
			char_info.uv = new Rect(uv);
			char_info.vert = new Rect(vert);
			char_info.width = width;
			
			// Scale char_info values
			char_info.vert.x /= scale;
			char_info.vert.y /= scale;
			char_info.vert.width /= scale;
			char_info.vert.height /= scale;
			char_info.width /= scale;
		}
	}

	public class CustomFontCharacterData
	{
		public Dictionary<int, CustomCharacterInfo> m_character_infos;
		
		public CustomFontCharacterData()
		{
			m_character_infos = new Dictionary<int, CustomCharacterInfo>();
		}
	}

	public static class TextFxHelperMethods
	{
		// Strips out all rich text code from a piece of text.
		public static string StripRichTextCode(string textString)
		{
			return Regex.Replace(textString, @"<[^>]*>", String.Empty);
		}

		public static tfxJSONArray ExportData (this List<int> list)
		{
			tfxJSONArray json_array = new tfxJSONArray();

			if(list != null)
			{
				foreach(int num in list)
				{
					json_array.Add(num);
				}
			}
			
			return json_array;
		}
		
		public static List<int> JSONtoListInt (this tfxJSONArray json_array)
		{
			List<int> int_list = new List<int>();
			
			foreach(tfxJSONValue int_val in json_array)
			{
				int_list.Add((int) int_val.Number);
			}
			
			return int_list;
		}

		public static List<object> StringToList (this string data_string, char delimiter = ',', char seperator = '=')
		{
			// Assuming the data string is book-ended with brackets
			data_string = data_string.Substring(1, data_string.Length - 2);
			
			if(data_string.Equals(""))
				return null;
			
			List<object> list = new List<object>();
			
			if(data_string.Contains("" + seperator))
			{
				// List of keyvaluepairs

				string[] value_pairs = data_string.Split(new char[]{ delimiter } );
				string[] data_parts;
				
				foreach( string value_pair in value_pairs)
				{
					data_parts = value_pair.Split(new char[]{seperator} );
					
					list.Add(new KeyValuePair<string,string>(data_parts[0], data_parts[1]));
				}
			}
			else
			{
				list = new List<object>(data_string.Split(new char[]{ delimiter } ));
			}
			
			return list;
		}

		public static string Vector2ToString (this Vector2 vec, char delimiter = ';', char seperator = ':')
		{
			return "x" + seperator + vec.x + delimiter + "y" + seperator + vec.y;
		}
		
		public static tfxJSONValue Vector2ToJSON (this Vector2 vec)
		{
			tfxJSONObject json_data = new tfxJSONObject();
			json_data["x"] = vec.x;
			json_data["y"] = vec.y;
			
			return new tfxJSONValue(json_data);
		}

		public static Vector2 StringToVector2 (this string data_string, char delimiter = ';', char seperator = ':')
		{
			string[] value_pairs = data_string.Split(new char[]{ delimiter } );
			string[] data_parts;
			Vector2 vec = new Vector2();
			
			foreach( string value_pair in value_pairs)
			{
				data_parts = value_pair.Split(new char[]{seperator} );

				if(data_parts[0].Equals("x"))
				{
					vec.x = float.Parse(data_parts[1]);
				}
				else if(data_parts[0].Equals("y"))
				{
					vec.y = float.Parse(data_parts[1]);
				}
			}

			return vec;
		}
		
		public static Vector2 JSONtoVector2 (this tfxJSONObject json_data)
		{
			return new Vector2(){ x = (float) json_data["x"].Number, y = (float) json_data["y"].Number };
		}

		public static tfxJSONValue ExportData (this Vector3 vec)
		{
			tfxJSONObject json_data = new tfxJSONObject();
			json_data["x"] = vec.x;
			json_data["y"] = vec.y;
			json_data["z"] = vec.z;
			return new tfxJSONValue(json_data);
		}
		
		public static Vector3 JSONtoVector3 (this tfxJSONObject json_data)
		{
			return new Vector3() {
				x = (float) json_data["x"].Number,
				y = (float) json_data["y"].Number,
				z = (float) json_data["z"].Number
			};
		}
		
		public static Vector3 StringToVector3 (this string data_string, char delimiter = ';', char seperator = ':')
		{
			string[] value_pairs = data_string.Split(new char[]{ delimiter } );
			string[] data_parts;
			Vector3 vec = new Vector3();
			
			foreach( string value_pair in value_pairs)
			{
				data_parts = value_pair.Split(new char[]{seperator} );
				
				if(data_parts[0].Equals("x"))
				{
					vec.x = float.Parse(data_parts[1]);
				}
				else if(data_parts[0].Equals("y"))
				{
					vec.y = float.Parse(data_parts[1]);
				}
				else if(data_parts[0].Equals("z"))
				{
					vec.z = float.Parse(data_parts[1]);
				}
			}
			
			return vec;
		}

		public static tfxJSONValue ExportData (this Color color)
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			json_data["r"] = color.r;
			json_data["g"] = color.g;
			json_data["b"] = color.b;
			json_data["a"] = color.a;
			
			return new tfxJSONValue(json_data);
		}
		
		public static Color JSONtoColor (this tfxJSONObject json_data)
		{
			return new Color()
			{
				r = (float) json_data["r"].Number,
				g = (float) json_data["g"].Number,
				b = (float) json_data["b"].Number,
				a = (float) json_data["a"].Number
			};
		}
		
		public static Color StringToColor (this string data_string, char delimiter = ';', char seperator = ':')
		{
			return StringDataToColor(data_string, delimiter, seperator);
		}

		public static tfxJSONValue ExportData (this VertexColour vert_color)
		{
			tfxJSONObject json_data = new tfxJSONObject();
			
			json_data["bottom_left"] = vert_color.bottom_left.ExportData();
			json_data["bottom_right"] = vert_color.bottom_right.ExportData();
			json_data["top_left"] = vert_color.top_left.ExportData();
			json_data["top_right"] = vert_color.top_right.ExportData();
			
			return new tfxJSONValue(json_data);
		}
		
		public static VertexColour JSONtoVertexColour (this tfxJSONObject json_data)
		{
			if(json_data.ContainsKey("r"))
			{
				// Legacy export data
				return new VertexColour( json_data.JSONtoColor());
			}

			return new VertexColour(){
				bottom_left = json_data["bottom_left"].Obj.JSONtoColor(),
				bottom_right = json_data["bottom_right"].Obj.JSONtoColor(),
				top_left = json_data["top_left"].Obj.JSONtoColor(),
				top_right = json_data["top_right"].Obj.JSONtoColor()
			};
		}

		public static VertexColour StringToVertexColor (this string data_string, char delimiter = ';', char seperator = ':', char color_seperator = '|')
		{
			string[] color_string_datas = data_string.Split(new char[]{ color_seperator } );

			return new VertexColour() {
							bottom_left = StringDataToColor(color_string_datas[0], delimiter, seperator),
							bottom_right = StringDataToColor(color_string_datas[1], delimiter, seperator),
							top_left = StringDataToColor(color_string_datas[2], delimiter, seperator),
							top_right = StringDataToColor(color_string_datas[3], delimiter, seperator)
			};
		}

		static Color StringDataToColor (string data_string, char delimiter = ';', char seperator = ':')
		{
			string[] value_pairs = data_string.Split(new char[]{ delimiter } );
			string[] data_parts;
			Color color = new Color();
			
			foreach( string value_pair in value_pairs)
			{
				data_parts = value_pair.Split(new char[]{seperator} );
				
				if(data_parts[0].Equals("r"))
				{
					color.r = float.Parse(data_parts[1]);
				}
				else if(data_parts[0].Equals("g"))
				{
					color.g = float.Parse(data_parts[1]);
				}
				else if(data_parts[0].Equals("b"))
				{
					color.b = float.Parse(data_parts[1]);
				}
				else if(data_parts[0].Equals("a"))
				{
					color.a = float.Parse(data_parts[1]);
				}
			}
			
			return color;
		}
		
		public static tfxJSONValue ExportData (this UnityEngine.Keyframe frame, AnimationCurve curve, int keyIndex)
		{
			tfxJSONObject json_data = new tfxJSONObject();
			json_data["inTangent"] = frame.inTangent;
			json_data["outTangent"] = frame.outTangent;
#if UNITY_2018_1_OR_NEWER && UNITY_EDITOR
			json_data["leftTangentMode"] = (int) AnimationUtility.GetKeyLeftTangentMode(curve, keyIndex);
			json_data["rightTangentMode"] = (int) AnimationUtility.GetKeyRightTangentMode(curve, keyIndex);
#elif !UNITY_2018_1_OR_NEWER
			json_data["tangentMode"] = frame.tangentMode;
#endif
			json_data["time"] = frame.time;
			json_data["value"] = frame.value;
			return new tfxJSONValue(json_data);
		}
		
		public static UnityEngine.Keyframe JSONtoKeyframe (this tfxJSONObject json_data)
		{
			return new UnityEngine.Keyframe ()
			{
				inTangent = (float) json_data["inTangent"].Number,
				outTangent = (float) json_data["outTangent"].Number,
#if !UNITY_2018_1_OR_NEWER
				tangentMode = (int) json_data["tangentMode"].Number,
#endif
				time = (float) json_data["time"].Number,
				value = (float) json_data["value"].Number
			};
		}
		
		public static tfxJSONValue ExportData (this AnimationCurve curve)
		{
			tfxJSONArray key_frame_data = new tfxJSONArray();
			int keyIndex = 0;

			foreach(UnityEngine.Keyframe key_frame in curve.keys)
			{
				key_frame_data.Add(key_frame.ExportData(curve, keyIndex));
				keyIndex++;
			}
			
			return key_frame_data;
		}
		
		public static AnimationCurve JSONtoAnimationCurve (this tfxJSONArray json_data)
		{
			AnimationCurve anim_curve = new AnimationCurve();
			anim_curve.keys = new Keyframe[0];

			UnityEngine.Keyframe keyframe;
			int frameIndex = 0;
			
			foreach(tfxJSONValue frame_data in json_data)
			{
				keyframe = new UnityEngine.Keyframe ()
				{
					inTangent = (float) frame_data.Obj["inTangent"].Number,
					outTangent = (float) frame_data.Obj["outTangent"].Number,
#if !UNITY_2018_1_OR_NEWER
					tangentMode = (int) frame_data.Obj["tangentMode"].Number,
#endif
					time = (float) frame_data.Obj["time"].Number,
					value = (float) frame_data.Obj["value"].Number
				};

#if UNITY_2018_1_OR_NEWER && UNITY_EDITOR
				AnimationUtility.SetKeyLeftTangentMode(anim_curve, frameIndex, (AnimationUtility.TangentMode) ((int)frame_data.Obj["tangentModeLeft"].Number));
				AnimationUtility.SetKeyRightTangentMode(anim_curve, frameIndex, (AnimationUtility.TangentMode) ((int)frame_data.Obj["tangentModeRight"].Number));
#endif

				anim_curve.AddKey(keyframe);

				frameIndex++;
			}
			
			return anim_curve;
		}
		
		public static AnimationCurve ToAnimationCurve (this string curve_data)
		{
			AnimationCurve curve = new AnimationCurve();
			string[] key_frame_data_parts = curve_data.Split('#');
				
			if(key_frame_data_parts.Length % 5 != 0)
				return curve;
			
			int idx=0;
			
			UnityEngine.Keyframe key_frame = new UnityEngine.Keyframe();
			
			for(idx=0; idx <key_frame_data_parts.Length; idx++)
			{
				if(idx % 5 == 0)
				{
					if(idx > 0)
						curve.AddKey(key_frame);
						
					key_frame = new Keyframe();
					key_frame.time = float.Parse(key_frame_data_parts[idx]);
				}
				if(idx % 5 == 1)
					key_frame.value = float.Parse(key_frame_data_parts[idx]);
				if(idx % 5 == 2)
					key_frame.inTangent = float.Parse(key_frame_data_parts[idx]);
				if(idx % 5 == 3)
					key_frame.outTangent = float.Parse(key_frame_data_parts[idx]);
#if !UNITY_2018_1_OR_NEWER
				if(idx % 5 == 4)
					key_frame.tangentMode = int.Parse(key_frame_data_parts[idx]);
#endif
			}
			
			if(idx > 0)
				curve.AddKey(key_frame);
			
			return curve;
		}

		public static string ToPath (this AudioClip clip)
		{
	#if UNITY_EDITOR
			return AssetDatabase.GetAssetPath(clip);
	#else
			return "";
	#endif
		}

		public static AudioClip PathToAudioClip (this string path)
		{
	#if UNITY_EDITOR	
			return AssetDatabase.LoadAssetAtPath(path, typeof(AudioClip)) as AudioClip;
	#else
			string audioclipName = "";
			int lastDashIndex = path.LastIndexOf('/');
			if(lastDashIndex >= 0)
			{
				audioclipName = path.Substring(lastDashIndex + 1);
				int fileExtensionPeriodIndex = audioclipName.LastIndexOf('.');
				if(fileExtensionPeriodIndex >= 0)
				{
					audioclipName = audioclipName.Substring(0,fileExtensionPeriodIndex);
					//Debug.Log("Load Resource: '" + ("TextFx/" + audioclipName) + "'");
					return Resources.Load<AudioClip>("TextFx/" + audioclipName);
				}
			}

			return null;
	#endif
		}

#if !UNITY_5_4_OR_NEWER
		public static string ToPath (this ParticleEmitter emitter)
		{
	#if UNITY_EDITOR
			return AssetDatabase.GetAssetPath(emitter);
	#else
			return "";
	#endif
		}
#endif
	
#if !UNITY_5_4_OR_NEWER
		public static ParticleEmitter PathToParticleEmitter (this string path, string assetNameSuffix = "")
		{
	#if UNITY_EDITOR
			if(assetNameSuffix != "")
			{
				// Attempt to find Suffixed version first, and then default to non-suffixed
				string suffixedPath = path.Insert(path.LastIndexOf('.'), assetNameSuffix);

				ParticleEmitter emitter = AssetDatabase.LoadAssetAtPath(suffixedPath, typeof(ParticleEmitter)) as ParticleEmitter;

				if(emitter != null)
					return emitter;
			}

			return AssetDatabase.LoadAssetAtPath(path, typeof(ParticleEmitter)) as ParticleEmitter;
	#else
			return null;
	#endif
		}
#endif
		
		public static string ToPath (this ParticleSystem p_system)
		{
#if UNITY_EDITOR
			UnityEngine.Object prefabObject = PrefabUtility.GetCorrespondingObjectFromSource(p_system);

			if(prefabObject != null)
			{
				return AssetDatabase.GetAssetPath(prefabObject);
			}
			else
			{
				Debug.LogWarning("ParticleSystem '" + p_system.name + "' need to be a prefab in order for it to be referenced by this TextFx animation copy data.");
				return null;
			}
#else
			return "";
#endif
		}
		
		public static ParticleSystem PathToParticleSystem (this string path, string assetNameSuffix = "")
		{
	#if UNITY_EDITOR
			if(path == String.Empty)
				return null;

			if(assetNameSuffix != "")
			{
				// Attempt to find Suffixed version first, and then default to non-suffixed
				string suffixedPath = path.Insert(path.LastIndexOf('.'), assetNameSuffix);

				ParticleSystem pSystem = AssetDatabase.LoadAssetAtPath(suffixedPath, typeof(ParticleSystem)) as ParticleSystem;
				
				if(pSystem != null)
					return pSystem;
			}

			return AssetDatabase.LoadAssetAtPath(path, typeof(ParticleSystem)) as ParticleSystem;
	#else
			return null;
	#endif
		}

		public static string[] GetArrayOfFirstEntries(this string[,] two_d_array)
		{
			string[] result_array = new string[two_d_array.GetLength(0)];

			for(int idx=0; idx < two_d_array.GetLength(0); idx++)
			{
				result_array[idx] = two_d_array[idx,0];
			}

			return result_array;
		}
		
		
		
		public static void ImportLegacyData (this TextFxAnimationManager effect_manager, string data)
		{
			List<object> data_list = data.StringToList();

			KeyValuePair<string, string> value_pair;
			string key, value;
			int anim_idx=0;

			effect_manager.m_master_animations = new List<LetterAnimation> ();

			for(int idx = 0; idx < data_list.Count; idx++)
			{
				value_pair = (KeyValuePair<string, string>) data_list[idx];
				key = value_pair.Key;
				value = value_pair.Value;

				switch(key)
				{
					case "m_animate_per":
						effect_manager.m_animate_per = (AnimatePerOptions) int.Parse(value); break;
					case "m_begin_delay":
						effect_manager.m_begin_delay = float.Parse(value); break;
					case "m_begin_on_start":
						effect_manager.m_begin_on_start = bool.Parse(value); break;
					case "m_on_finish_action":
						effect_manager.m_on_finish_action = (ON_FINISH_ACTION) int.Parse(value); break;
					case "m_time_type":
						effect_manager.m_time_type = (AnimationTime) int.Parse(value); break;

					case "ANIM_DATA_START":
						if(anim_idx == effect_manager.NumAnimations)
						{
							effect_manager.AddAnimation();
						}
						idx = effect_manager.GetAnimation(anim_idx).ImportLegacyData(data_list, idx+1);
						anim_idx ++;
						break;
				}
			}
		}
		
		public static int ImportLegacyData(this LetterAnimation letter_anim, List<object> data_list, int index_offset = 0)
		{
			KeyValuePair<string, string> value_pair;
			string key, value;
			int idx;
			int loop_idx = 0, action_idx = 0;

			for(idx = index_offset; idx < data_list.Count; idx++)
			{
				value_pair = (KeyValuePair<string, string>) data_list[idx];
				key = value_pair.Key;
				value = value_pair.Value;

				if(key.Equals("ANIM_DATA_END"))
				{
					// reached end of this animations import data
					break;
				}

				switch(key)
				{
					case "m_letters_to_animate":
						List<object> letter_list = value.StringToList(';');
						letter_anim.m_letters_to_animate = new List<int>();
						if(letter_list != null)
							foreach(object obj in letter_list)
							{
								letter_anim.m_letters_to_animate.Add(int.Parse (obj.ToString()));
							}
						break;
					case "m_letters_to_animate_custom_idx":
						letter_anim.m_letters_to_animate_custom_idx = int.Parse(value); break;
					case "m_letters_to_animate_option":
						letter_anim.m_letters_to_animate_option = (LETTERS_TO_ANIMATE) int.Parse(value); break;


					// LOOP DATA IMPORT
					case "LOOP_DATA_START":
						if(loop_idx == letter_anim.NumLoops)
						{
							letter_anim.AddLoop();
						}
						break;
					case "LOOP_DATA_END":
						loop_idx++; break;
					case "m_delay_first_only":
						letter_anim.GetLoop(loop_idx).m_delay_first_only = bool.Parse(value); break;
					case "m_end_action_idx":
						letter_anim.GetLoop(loop_idx).m_end_action_idx = int.Parse(value); break;
					case "m_loop_type":
						letter_anim.GetLoop(loop_idx).m_loop_type = (LOOP_TYPE) int.Parse(value); break;
					case "m_number_of_loops":
						letter_anim.GetLoop(loop_idx).m_number_of_loops = int.Parse(value); break;
					case "m_start_action_idx":
						letter_anim.GetLoop(loop_idx).m_start_action_idx = int.Parse(value); break;


					// ACTION DATA IMPORT
					case "ACTION_DATA_START":
						if(action_idx == letter_anim.NumActions)
						{
							letter_anim.AddAction();
						}
						idx = letter_anim.GetAction(action_idx).ImportLegacyData(data_list, idx+1);
						action_idx ++;
						break;
				}
			}

			// Remove any extra LoopData or LetterAction instances that existed prior to importing
			if(letter_anim.NumLoops > loop_idx)
			{
				letter_anim.RemoveLoops(loop_idx, letter_anim.NumLoops - loop_idx);
			}

			if(letter_anim.NumActions > action_idx)
			{
				letter_anim.RemoveActions(action_idx, letter_anim.NumActions - action_idx);
			}

			return idx;
		}
		
		public static int ImportLegacyData (this LetterAction letter_action, List<object> data_list, int index_offset = 0)
		{
			KeyValuePair<string, string> value_pair;
			string key, value;
			int idx;
			
			letter_action.ClearAudioEffectSetups();
			letter_action.ClearParticleEffectSetups();
			
			AudioEffectSetup audio_setup = null;
			ParticleEffectSetup effect_setup = null;
			
			for(idx = index_offset; idx < data_list.Count; idx++)
			{
				value_pair = (KeyValuePair<string, string>) data_list[idx];
				key = value_pair.Key;
				value = value_pair.Value;
				
				if(key.Equals("ACTION_DATA_END"))
				{
					// reached end of this Actions import data

					letter_action.m_colour_transition_active = true;
					letter_action.m_position_transition_active = true;
					letter_action.m_local_scale_transition_active = true;
					letter_action.m_local_rotation_transition_active = true;
					letter_action.m_global_scale_transition_active = true;
					letter_action.m_global_rotation_transition_active = true;
					break;
				}

				switch(key)
				{
					case "m_action_type":
						letter_action.m_action_type = (ACTION_TYPE) int.Parse(value); break;
					case "m_ease_type":
						letter_action.m_ease_type = (EasingEquation) int.Parse(value); break;
					case "m_force_same_start_time":
						letter_action.m_force_same_start_time = bool.Parse(value); break;
					// Legacy letter anchor import support
					case "m_letter_anchor":
						letter_action.m_letter_anchor_start = int.Parse(value);
						letter_action.m_letter_anchor_2_way = false;	
						break;
					
					// New letter anchor import support
					case "m_letter_anchor_start":
						letter_action.m_letter_anchor_start = int.Parse(value);break;
					case "m_letter_anchor_end":
						letter_action.m_letter_anchor_end = int.Parse(value);break;
					case "m_letter_anchor_2_way":
						letter_action.m_letter_anchor_2_way = bool.Parse(value); break;
					
					
					case "m_offset_from_last":
						letter_action.m_offset_from_last = bool.Parse(value); break;
					case "m_position_axis_ease_data":
						letter_action.m_position_axis_ease_data.ImportLegacyData(value); break;
					case "m_rotation_axis_ease_data":
						letter_action.m_rotation_axis_ease_data.ImportLegacyData(value); break;
					case "m_scale_axis_ease_data":
						letter_action.m_scale_axis_ease_data.ImportLegacyData(value); break;


					case "m_start_colour":
						letter_action.m_start_colour.ImportLegacyData(value); break;
					case "m_end_colour":
						letter_action.m_end_colour.ImportLegacyData(value); break;
					case "m_start_euler_rotation":
						letter_action.m_start_euler_rotation.ImportLegacyData(value); break;
					case "m_end_euler_rotation":
						letter_action.m_end_euler_rotation.ImportLegacyData(value); break;
					case "m_start_pos":
						letter_action.m_start_pos.ImportLegacyData(value); break;
					case "m_end_pos":
						letter_action.m_end_pos.ImportLegacyData(value); break;
					case "m_start_scale":
						letter_action.m_start_scale.ImportLegacyData(value); break;
					case "m_end_scale":
						letter_action.m_end_scale.ImportLegacyData(value); break;
					case "m_delay_progression":
						letter_action.m_delay_progression.ImportLegacyData(value); break;
					case "m_duration_progression":
						letter_action.m_duration_progression.ImportLegacyData(value); break;
						
					
					case "m_audio_on_start":
						if(value.PathToAudioClip() != null)
						{
							audio_setup = new AudioEffectSetup() { m_audio_clip = value.PathToAudioClip(), m_play_when = PLAY_ITEM_EVENTS.ON_START, m_effect_assignment = PLAY_ITEM_ASSIGNMENT.PER_LETTER, m_loop_play_once = false };
						}
						break;
					case "m_audio_on_start_delay":
						if(audio_setup != null)
							audio_setup.m_delay.ImportLegacyData(value); break;
					case "m_audio_on_start_offset":
						if(audio_setup != null)
							audio_setup.m_offset_time.ImportLegacyData(value); break;
					case "m_audio_on_start_pitch":
						if(audio_setup != null)
							audio_setup.m_pitch.ImportLegacyData(value); break;
					case "m_audio_on_start_volume":
						if(audio_setup != null)
						{
							audio_setup.m_volume.ImportLegacyData(value);
							letter_action.AddAudioEffectSetup(audio_setup);
							audio_setup = null;
						}
						break;
					
					case "m_audio_on_finish":
						if(value.PathToAudioClip() != null)
						{
							audio_setup = new AudioEffectSetup() { m_audio_clip = value.PathToAudioClip(), m_play_when = PLAY_ITEM_EVENTS.ON_FINISH, m_effect_assignment = PLAY_ITEM_ASSIGNMENT.PER_LETTER, m_loop_play_once = false };
						}
						break;
					case "m_audio_on_finish_delay":
						if(audio_setup != null)
							audio_setup.m_delay.ImportLegacyData(value);
						break;
					case "m_audio_on_finish_offset":
						if(audio_setup != null)
							audio_setup.m_offset_time.ImportLegacyData(value);
						break;
					case "m_audio_on_finish_pitch":
						if(audio_setup != null)
							audio_setup.m_pitch.ImportLegacyData(value);
						break;
					case "m_audio_on_finish_volume":
						if(audio_setup != null)
						{
							audio_setup.m_volume.ImportLegacyData(value);
							letter_action.AddAudioEffectSetup(audio_setup);
							audio_setup = null;
						}
						break;
					
					
					// BACKWARDS COMPATIBILITY PARTICLE IMPORT
#if !UNITY_5_4_OR_NEWER
					case "m_emitter_on_start":
						if(value.PathToParticleEmitter() != null)
						{
							effect_setup = new ParticleEffectSetup()
							{
								m_legacy_particle_effect = value.PathToParticleEmitter(),
								m_play_when = PLAY_ITEM_EVENTS.ON_START,
								m_loop_play_once = false,
								m_rotation_offset = new ActionVector3Progression(new Vector3(0,180,0)),
								m_rotate_relative_to_letter = true,
								m_effect_type = PARTICLE_EFFECT_TYPE.LEGACY
							};
						}
						break;
#endif
					case "m_emitter_on_start_delay":
						if(effect_setup != null)
							effect_setup.m_delay.ImportLegacyData(value);
						break;
					case "m_emitter_on_start_duration":
						if(effect_setup != null)
							effect_setup.m_duration.ImportLegacyData(value);
						break;
					case "m_emitter_on_start_follow_mesh":
						if(effect_setup != null)
							effect_setup.m_follow_mesh = bool.Parse(value);
						break;
					case "m_emitter_on_start_offset":
						if(effect_setup != null)
							effect_setup.m_position_offset.ImportLegacyData(value);
						break;
					case "m_emitter_on_start_per_letter":
						if(effect_setup != null)
						{
							effect_setup.m_effect_assignment = bool.Parse(value) ? PLAY_ITEM_ASSIGNMENT.PER_LETTER : PLAY_ITEM_ASSIGNMENT.CUSTOM;
							if(effect_setup.m_effect_assignment == PLAY_ITEM_ASSIGNMENT.CUSTOM)
								effect_setup.m_effect_assignment_custom_letters = new List<int>(){0};
						
							letter_action.AddParticleEffectSetup(effect_setup);
							effect_setup = null;
						}
						break;
#if !UNITY_5_4_OR_NEWER
					case "m_emitter_on_finish":
						if(value.PathToParticleEmitter() != null)
						{
							effect_setup = new ParticleEffectSetup()
							{
								m_legacy_particle_effect = value.PathToParticleEmitter(),
								m_play_when = PLAY_ITEM_EVENTS.ON_FINISH,
								m_loop_play_once = false,
								m_rotation_offset = new ActionVector3Progression(new Vector3(0,180,0)),
								m_rotate_relative_to_letter = true,
								m_effect_type = PARTICLE_EFFECT_TYPE.LEGACY
							};
						}
						break;
#endif
					case "m_emitter_on_finish_delay":
						if(effect_setup != null)
							effect_setup.m_delay.ImportLegacyData(value);
						break;
					case "m_emitter_on_finish_duration":
						if(effect_setup != null)
							effect_setup.m_duration.ImportLegacyData(value);
						break;
					case "m_emitter_on_finish_follow_mesh":
						if(effect_setup != null)
							effect_setup.m_follow_mesh = bool.Parse(value);
						break;
					case "m_emitter_on_finish_offset":
						if(effect_setup != null)
							effect_setup.m_position_offset.ImportLegacyData(value);
						break;
					case "m_emitter_on_finish_per_letter":
						if(effect_setup != null)
						{
							effect_setup.m_effect_assignment = bool.Parse(value) ? PLAY_ITEM_ASSIGNMENT.PER_LETTER : PLAY_ITEM_ASSIGNMENT.CUSTOM;
							if(effect_setup.m_effect_assignment == PLAY_ITEM_ASSIGNMENT.CUSTOM)
								effect_setup.m_effect_assignment_custom_letters = new List<int>(){0};
						
							letter_action.AddParticleEffectSetup(effect_setup);
							effect_setup = null;
						}
						break;
				}
			}

			return idx;
		}
		
		public static void ImportLegacyData(this AxisEasingOverrideData axis_data, string data_string)
		{
			string[] data_parts = data_string.Split(new char[]{ '|' } );
			if(int.Parse( data_parts[0] ) == 1)
			{
				axis_data.m_override_default = true;
				axis_data.m_x_ease = (EasingEquation) int.Parse(data_parts[1]);
				axis_data.m_y_ease = (EasingEquation) int.Parse(data_parts[2]);
				axis_data.m_z_ease = (EasingEquation) int.Parse(data_parts[3]);
				
			}
			else
			{
				axis_data.m_override_default = false;
			}
		}

#if UNITY_EDITOR
		public static T GetCopyOf<T>(this Component comp, T other) where T : Component
		{
			Type type = comp.GetType();
			if (type != other.GetType()) return null; // type mis-match
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			PropertyInfo[] pinfos = type.GetProperties(flags);

			foreach (PropertyInfo propertyInfo in pinfos)
			{
				if(!propertyInfo.CanWrite || !propertyInfo.CanRead)
					continue;
				
				if(propertyInfo.GetGetMethod(false) == null || propertyInfo.GetSetMethod(false) == null)
					continue;
				
				object currentPropertyValue = propertyInfo.GetValue(other, null);
				propertyInfo.SetValue(comp, currentPropertyValue, null);
			}

			FieldInfo[] finfos = type.GetFields(flags);
			foreach (var finfo in finfos) {
				finfo.SetValue(comp, finfo.GetValue(other));
			}
			return comp as T;
		}

		public static void GetCloneOf<T,M>(this T cloningObj, ref M targetObj, string[] fieldExceptions = null)
		{
			Type type = cloningObj.GetType();
			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			PropertyInfo[] pinfos = type.GetProperties(flags);
			
			foreach (PropertyInfo propertyInfo in pinfos)
			{
				if(!propertyInfo.CanWrite || !propertyInfo.CanRead)
					continue;
				
				if(propertyInfo.GetGetMethod(false) == null || propertyInfo.GetSetMethod(false) == null)
					continue;

				if(fieldExceptions != null)
				{
					bool exceptionFound = false;
					foreach(string exceptionName in fieldExceptions)
					{
						if(exceptionName.Equals( propertyInfo.Name ))
						{
							exceptionFound = true;
							continue;
						}
					}

					if(exceptionFound)
						continue;
				}

				object currentPropertyValue = propertyInfo.GetValue(cloningObj, null);
				propertyInfo.SetValue(targetObj, currentPropertyValue, null);
			}
			
			FieldInfo[] finfos = type.GetFields(flags);
			foreach (FieldInfo finfo in finfos) {

				if(fieldExceptions != null)
				{
					foreach(string exceptionName in fieldExceptions)
					{
						if(exceptionName == finfo.Name)
							continue;
					}
				}

				finfo.SetValue(targetObj, finfo.GetValue(cloningObj));
			}
		}

		public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
		{
			return go.AddComponent<T>().GetCopyOf(toAdd) as T;
		}


		public static Texture2D lineTex;
		public static void DrawGUILine(Rect rect) { DrawGUILine(rect, GUI.contentColor, 1.0f); }
		public static void DrawGUILine(Rect rect, Color color) { DrawGUILine(rect, color, 1.0f); }
		public static void DrawGUILine(Rect rect, float width) { DrawGUILine(rect, GUI.contentColor, width); }
		public static void DrawGUILine(Rect rect, Color color, float width) { DrawGUILine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width); }
		public static void DrawGUILine(Vector2 pointA, Vector2 pointB) { DrawGUILine(pointA, pointB, GUI.contentColor, 1.0f); }
		public static void DrawGUILine(Vector2 pointA, Vector2 pointB, Color color) { DrawGUILine(pointA, pointB, color, 1.0f); }
		public static void DrawGUILine(Vector2 pointA, Vector2 pointB, float width) { DrawGUILine(pointA, pointB, GUI.contentColor, width); }
		public static void DrawGUILine(Vector2 pointA, Vector2 pointB, Color color, float width)
		{
			if(pointA.Equals(pointB))
			{
				// points are the same.
				return;
			}
			
			// Save the current GUI matrix, since we're going to make changes to it.
			Matrix4x4 matrix = GUI.matrix;
			
			// Generate a single pixel texture if it doesn't exist
			if (!lineTex)
			{
				lineTex = new Texture2D(1, 1);

				// Setting to never save this texture to the scene, so that the editor doesn't complain about an unreferenced texture in memory.
				lineTex.hideFlags = HideFlags.DontSave;
			}
			
			// Store current GUI color, so we can switch it back later,
			// and set the GUI color to the color parameter
			Color savedColor = GUI.color;
			GUI.color = color;
			
			// Determine the angle of the line.
			float angle = Vector3.Angle(pointB - pointA, Vector2.right);
			
			// Vector3.Angle always returns a positive number.
			// If pointB is above pointA, then angle needs to be negative.
			if (pointA.y > pointB.y) { angle = -angle; }
			
			// Use ScaleAroundPivot to adjust the size of the line.
			// We could do this when we draw the texture, but by scaling it here we can use
			//  non-integer values for the width and length (such as sub 1 pixel widths).
			// Note that the pivot point is at +.5 from pointA.y, this is so that the width of the line
			//  is centered on the origin at pointA.
			GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));
			
			// Set the rotation for the line.
			//  The angle was calculated with pointA as the origin.
			GUIUtility.RotateAroundPivot(angle, pointA);
			
			// Finally, draw the actual line.
			// We're really only drawing a 1x1 texture from pointA.
			// The matrix operations done with ScaleAroundPivot and RotateAroundPivot will make this
			//  render with the proper width, length, and angle.
			GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTex);
			
			// We're done.  Restore the GUI matrix and GUI color to whatever they were before.
			GUI.matrix = matrix;
			GUI.color = savedColor;
		}
#endif
	}

}