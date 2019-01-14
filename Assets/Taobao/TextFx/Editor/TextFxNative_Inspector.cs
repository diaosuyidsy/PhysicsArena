using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace TextFx
{
	[CustomEditor(typeof(TextFxNative))]
	public class TextFxNative_Inspector : Editor
	{
		TextFxNative textfx_instance;
		
		string m_old_text;
		TextDisplayAxis m_old_display_axis;
		TextAnchor m_old_text_anchor;
		TextAlignment m_old_text_alignment;
		float m_old_char_size;
		Color m_old_textColour;
		VertexColour m_old_vertexColour;
		bool m_old_use_gradients;
		float m_old_line_height;
		Vector2 m_old_px_offset;
		bool m_old_baseline_override;
		float m_old_font_baseline;
		float m_old_max_width;

		TextFxAnimationManager animationManager;

		void OnEnable()
		{
			TextFxNative effectInstance = (TextFxNative) target;
			animationManager = effectInstance.AnimationManager;

			effectInstance.GetComponent<MeshRenderer>().hideFlags = HideFlags.HideInInspector;
			effectInstance.GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;

			EditorApplication.update += UpdateManager;
		}

		void OnDisable()
		{
			EditorApplication.update -= UpdateManager;
		}

		void UpdateManager()
		{
			TextFxBaseInspector.UpdateManager (animationManager);
		}

		public override void OnInspectorGUI ()
		{
			// Draw TextFx inspector section
			TextFxBaseInspector.DrawTextFxInspectorSection(this, animationManager, () => {
				RefreshTextCurveData();
			});

//			DrawDefaultInspector();
			
			textfx_instance = (TextFxNative)target;
			
			m_old_text = textfx_instance.m_text;
			m_old_display_axis = textfx_instance.m_display_axis;
			m_old_text_anchor = textfx_instance.m_text_anchor;
			m_old_text_alignment = textfx_instance.m_text_alignment;
			m_old_char_size = textfx_instance.m_character_size;
			m_old_textColour = textfx_instance.m_textColour;
			m_old_vertexColour = textfx_instance.m_textColourGradient.Clone();
			m_old_use_gradients = textfx_instance.m_use_colour_gradient;
			m_old_textColour = textfx_instance.m_textColour;
			m_old_line_height = textfx_instance.m_line_height_factor;
			m_old_px_offset = textfx_instance.m_px_offset;
			m_old_baseline_override = textfx_instance.m_override_font_baseline;
			m_old_font_baseline = textfx_instance.m_font_baseline_override;
			m_old_max_width = textfx_instance.m_max_width;
			
			if(GUI.changed)
			{
				return;
			}

			
			EditorGUILayout.LabelField("Font Setup Data", EditorStyles.boldLabel);
			
	#if !UNITY_3_5
			textfx_instance.m_font = EditorGUILayout.ObjectField(new GUIContent("Font (.ttf, .dfont, .otf)", "Your font file to use for this text."), textfx_instance.m_font, typeof(Font), true) as Font;
			if(GUI.changed && textfx_instance.m_font != null)
			{
				textfx_instance.gameObject.GetComponent<Renderer>().material = textfx_instance.m_font.material;
				textfx_instance.m_font_material = textfx_instance.m_font.material;
				textfx_instance.SetText(textfx_instance.m_text);
			}
	#endif
			
			textfx_instance.m_font_data_file = EditorGUILayout.ObjectField(new GUIContent("Font Data File", "Your Bitmap font text data file."), textfx_instance.m_font_data_file, typeof(TextAsset), true) as TextAsset;
			if(GUI.changed && textfx_instance.m_font_data_file != null && textfx_instance.m_font_material != null)
			{
				// Wipe the old character data hashtable
				textfx_instance.ClearFontCharacterData();
				textfx_instance.SetText(textfx_instance.m_text);
				return;
			}
			textfx_instance.m_font_material = EditorGUILayout.ObjectField(new GUIContent("Font Material", "Your Bitmap font material"), textfx_instance.m_font_material, typeof(Material), true) as Material;
			if(GUI.changed && textfx_instance.m_font_data_file != null && textfx_instance.m_font_material != null)
			{
				// Reset the text with the new material assigned.
				textfx_instance.gameObject.GetComponent<Renderer>().material = textfx_instance.m_font_material;
				textfx_instance.SetText(textfx_instance.m_text);
				return;
			}
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField(new GUIContent("Text", "The text to display."), EditorStyles.boldLabel);
			textfx_instance.m_text = EditorGUILayout.TextArea(textfx_instance.m_text, GUILayout.Width(Screen.width - 25));
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField("Text Settings", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal ();

			if(textfx_instance.m_use_colour_gradient)
			{
				EditorGUILayout.BeginVertical ();
				EditorGUILayout.BeginHorizontal ();
				textfx_instance.m_textColourGradient.top_left = EditorGUILayout.ColorField("Colour", textfx_instance.m_textColourGradient.top_left);
				textfx_instance.m_textColourGradient.top_right = EditorGUILayout.ColorField(textfx_instance.m_textColourGradient.top_right, GUILayout.Width(53));
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				textfx_instance.m_textColourGradient.bottom_left = EditorGUILayout.ColorField(" ", textfx_instance.m_textColourGradient.bottom_left);
				textfx_instance.m_textColourGradient.bottom_right = EditorGUILayout.ColorField(textfx_instance.m_textColourGradient.bottom_right, GUILayout.Width(53));
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.EndVertical ();
			}
			else
			{
				textfx_instance.m_textColour = EditorGUILayout.ColorField("Colour", textfx_instance.m_textColour);//, GUILayout.Width(260));
			}
			GUILayout.FlexibleSpace ();
			EditorGUILayout.LabelField ("Use Gradient?", GUILayout.Width(100));
			textfx_instance.m_use_colour_gradient = EditorGUILayout.Toggle (textfx_instance.m_use_colour_gradient, GUILayout.Width(20));
			GUILayout.FlexibleSpace ();
			EditorGUILayout.EndHorizontal ();

			textfx_instance.m_display_axis = (TextDisplayAxis) EditorGUILayout.EnumPopup(new GUIContent("Display Axis", "Denotes whether to render the text horizontally or vertically."), textfx_instance.m_display_axis);
			textfx_instance.m_text_anchor = (TextAnchor) EditorGUILayout.EnumPopup(new GUIContent("Text Anchor", "Defines the anchor point about which the text is rendered"), textfx_instance.m_text_anchor);
			textfx_instance.m_text_alignment = (TextAlignment) EditorGUILayout.EnumPopup(new GUIContent("Text Alignment", "Defines the alignment of the text, just like your favourite word processor."), textfx_instance.m_text_alignment);
			textfx_instance.m_character_size = EditorGUILayout.FloatField(new GUIContent("Character Size", "Specifies the size of the text."), textfx_instance.m_character_size);
			textfx_instance.m_line_height_factor = EditorGUILayout.FloatField(new GUIContent("Line Height", "Defines the height of the text lines, based on the tallest line. If value is 2, the lines will be spaced at double the height of the tallest line."), textfx_instance.m_line_height_factor);

			EditorGUILayout.BeginHorizontal();
			textfx_instance.m_override_font_baseline = EditorGUILayout.Toggle(new GUIContent("Override Font Baseline?", "Allows you to manually set a baseline y-offset for the font to be rendered to."), textfx_instance.m_override_font_baseline);
			if(textfx_instance.m_override_font_baseline)
			{
				textfx_instance.m_font_baseline_override = EditorGUILayout.FloatField(new GUIContent("Font Baseline Offset", ""), textfx_instance.m_font_baseline_override);
			}
			EditorGUILayout.EndHorizontal();

			textfx_instance.m_px_offset = EditorGUILayout.Vector2Field("Letter Spacing Offset", textfx_instance.m_px_offset);
			textfx_instance.m_max_width = EditorGUILayout.FloatField(new GUIContent("Max Width", "Defines the maximum width of the text, and breaks the text onto new lines to keep it within this maximum."), textfx_instance.m_max_width);

			if (GUI.changed)
			{
				EditorUtility.SetDirty(textfx_instance);
			}
			
			if(m_old_char_size != textfx_instance.m_character_size ||
			   m_old_textColour != textfx_instance.m_textColour ||
			   !m_old_vertexColour.Equals(textfx_instance.m_textColourGradient) ||
			   m_old_use_gradients != textfx_instance.m_use_colour_gradient ||
				m_old_display_axis != textfx_instance.m_display_axis ||
			   	m_old_line_height != textfx_instance.m_line_height_factor ||
				m_old_max_width != textfx_instance.m_max_width ||
				!m_old_text.Equals(textfx_instance.m_text)	||
				m_old_text_alignment != textfx_instance.m_text_alignment ||
				m_old_text_anchor != textfx_instance.m_text_anchor ||
				m_old_px_offset != textfx_instance.m_px_offset ||
				m_old_baseline_override != textfx_instance.m_override_font_baseline || 
				(textfx_instance.m_override_font_baseline && m_old_font_baseline != textfx_instance.m_font_baseline_override))
			{
				textfx_instance.SetText(textfx_instance.m_text);
			}
		}

		void OnSceneGUI()
		{
			if (textfx_instance != null && textfx_instance.RenderToCurve && textfx_instance.BezierCurve.EditorVisible)
			{
				textfx_instance.OnSceneGUIBezier (animationManager.Transform.position, animationManager.Scale * animationManager.AnimationInterface.MovementScale);

				if (GUI.changed)
				{
					RefreshTextCurveData ();
				}
			}
		}

		void RefreshTextCurveData()
		{
			animationManager.CheckCurveData ();

			// Update mesh values to latest using new curve offset values
			textfx_instance.ForceUpdateCachedVertData();


			textfx_instance.UpdateTextFxMesh();
		}
	}
}