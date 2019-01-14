//#define ANIM_DESIGN_DEV_TOOLS

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using TFXBoomlagoon.JSON;

namespace TextFx
{
	public class TextEffectsManager : EditorWindow
	{
		static TextEffectsManager m_instance;
		public static TextEffectsManager Instance
		{
			get
			{
				if(m_instance == null)
				{
					m_instance = (TextEffectsManager)EditorWindow.GetWindow (typeof (TextEffectsManager));
				}
				
				return m_instance;
			}
		}
		
		// Gui layout variables
		const float WINDOW_BORDER_LEFT = 10;
		const float WINDOW_BORDER_RIGHT = 10;
		const float WINDOW_BORDER_TOP = 10;
		const float WINDOW_BORDER_BOTTOM = 10;
		const float TOOLBAR_LETTER_WIDTH = 40;
		const float TOOLBAR_LETTER_HEIGHT = 30;
		const float LINE_HEIGHT = 20;
		const float INPUT_FIELD_HEIGHT = 18;
		const float TEXT_AREA_LINE_HEIGHT = 14;
		const float HEADER_HEIGHT = 30;
		const float ENUM_SELECTOR_WIDTH = 300;
		const float ENUM_SELECTOR_WIDTH_MEDIUM = 120;
		const float ENUM_SELECTOR_WIDTH_SMALL = 70;
		const float ACTION_NODE_MARGIN = 5;
		
		const float PROGRESSION_HEADER_LABEL_WIDTH = 150;
		const float ACTION_INDENT_LEVEL_1 = 10;
		const float ACTION_INDENT_LEVEL_2 = 30;
		
		const float VECTOR_3_WIDTH = 300;
		
		const float ACTION_NODE_SPACING = 60;
		const int LOOP_LIST_WIDTH = 360;
		const int BASE_LOOP_LIST_POSITION_OFFSET = 80;
		const int LOOP_LINE_OFFSET = 35;

		static Color LOOP_COLOUR = Color.blue;
		static Color LOOP_REVERSE_COLOUR = Color.red;
		
		static GUIStyle m_foldout_header_gui_style;
		static GUIStyle FoldOutHeaderGUIStyle
		{
			get
			{
				if(m_foldout_header_gui_style == null)
				{
					m_foldout_header_gui_style = new GUIStyle(EditorStyles.foldout);
					m_foldout_header_gui_style.fontSize = 18;
					m_foldout_header_gui_style.padding.top = -2;
					m_foldout_header_gui_style.padding.left = 16;
				}
				
				return m_foldout_header_gui_style;
			}
		}


		static GUIStyle m_header_gui_style;
		static GUIStyle HeaderGUIStyle
		{
			get
			{
				if(m_header_gui_style == null)
				{
					m_header_gui_style = new GUIStyle(EditorStyles.label );
					m_header_gui_style.fontSize = 18;
					m_header_gui_style.padding.top = -2;
					m_header_gui_style.padding.left = 16;
				}
				
				return m_header_gui_style;
			}
		}

		static GUIStyle m_big_header_gui_style;
		static GUIStyle BigHeaderGUIStyle
		{
			get
			{
				if(m_big_header_gui_style == null)
				{
					m_big_header_gui_style = new GUIStyle(EditorStyles.boldLabel );
					m_big_header_gui_style.fontSize = 20;
				}
				
				return m_big_header_gui_style;
			}
		}

		static GUIStyle m_popup_header_gui_style;
		static GUIStyle PopupHeaderGUIStyle
		{
			get
			{
				if(m_popup_header_gui_style == null)
				{
					m_popup_header_gui_style = new GUIStyle(EditorStyles.popup);
					m_popup_header_gui_style.fontSize = 13;
					m_popup_header_gui_style.fixedHeight = 22;
					m_popup_header_gui_style.padding.top = -2;
				}
				
				return m_popup_header_gui_style;
			}
		}

		static GUIStyle m_button_image_only_gui_style;
		static GUIStyle ButtonImageOnlyGUIStyle
		{
			get{
				if(m_button_image_only_gui_style == null)
				{
					m_button_image_only_gui_style = new GUIStyle( EditorStyles.miniButton );

					m_button_image_only_gui_style.normal.background = null;
				}

				return m_button_image_only_gui_style;
			}
		}

		static GUIStyle m_windowHeaderStyle;
		static GUIStyle WindowHeaderStyle
		{
			get{
				if(m_windowHeaderStyle == null)
				{
					m_windowHeaderStyle = new GUIStyle( EditorStyles.label );

					m_windowHeaderStyle.fontSize = 9;
					m_windowHeaderStyle.richText = true;
				}

				return m_windowHeaderStyle;
			}
		}

		// Gui Interaction variables
		Vector2 MAIN_SCROLL_POS = Vector2.zero;
		Vector2 LOOP_SCROLL_POS = Vector2.zero;
		Vector2 CUSTOM_LETTERS_LIST_POS = Vector2.zero;
		Vector2 QUICK_SETUP_SCROLL_POS = Vector2.zero;
		float m_quick_setup_panel_height = 3000;
		float m_main_editor_panel_height = 3000;
		
		int ANIMATION_IDX = 0;
		bool m_display_loops_tree = false;
		
		Rect[] m_state_node_rects;
		const float state_overview_x = 5;
		int main_action_editor_x = 100;
		int state_overview_width = 0;
		int loop_tree_width = 60;
		
		int MainEditorX { get { return state_overview_width < main_action_editor_x ? main_action_editor_x : state_overview_width; } }
		
		int m_mouse_down_node_idx = -1;
		Vector2 m_mouse_down_pos;
		Vector2 m_mouse_drag_pos;

		TextFxAnimationInterface m_textfx_animation_interface;
		TextFxAnimationManager m_animation_manager;
		
		bool ignore_gui_change = false;
		bool noticed_gui_change = false;
		int edited_action_idx = -1;
		ANIMATION_DATA_TYPE m_edited_data = ANIMATION_DATA_TYPE.NONE;
		bool editing_start_state = true;
		int m_editor_section_idx = 0;
		string[] m_animation_config_titles;
		string[] m_animation_config_intro_titles;
		string[] m_animation_config_main_titles;
		string[] m_animation_config_outro_titles;
#if ANIM_DESIGN_DEV_TOOLS
		int m_selected_animation_idx;
		bool m_show_raw_import_settings = false;
#endif

		Texture2D 	m_copy_button_texture,
					m_paste_button_texture,
//					m_save_button_texture,
					m_play_button_texture,
					m_pause_button_texture,
					m_reset_button_texture,
					m_continue_button_texture,
					m_continue_past_loop_button_texture,
					m_repeat_off_button_texture,
					m_repeat_on_button_texture,
					m_infinity_texture;
		
		// New Loop Vars
		int new_loop_from = 0;
		int new_loop_to = 0;

		static EditorWindow m_window;

		static EditorWindow AnimEditorWindow {
			get {
				if (m_window == null)
					m_window = EditorWindow.GetWindow (typeof(TextEffectsManager));
				return m_window;
			}
		}
	    
	    [MenuItem ("Window/TextFX/Animation Editor", false, 0)]
	    public static void Init ()
		{
	        // Get existing open window or if none, make a new one:
			m_window = EditorWindow.GetWindow (typeof (TextEffectsManager));

#if UNITY_4 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			m_window.title = "TextFx Editor";
#else
			m_window.titleContent = new GUIContent( "TextFx Editor",  AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/EditorTitleIcon.png", typeof(Texture2D)) as Texture2D);
#endif
		}

		[MenuItem("Tools/TextFx/Links/Unity Forum Post", false, 100)]
		static void OpenForumWebsite()
		{
			Application.OpenURL("http://forum.unity3d.com/threads/released-textfx-a-text-animation-plugin-for-unity-available-now-in-the-assetstore.184822/");
		}

		[MenuItem("Tools/TextFx/Links/Documentation", false, 101)]
		static void OpenDocumentationWebsite()
		{
			Application.OpenURL("http://www.codeimmunity.co.uk/TextFx/plugin_setup.php");
		}


		//		[MenuItem ("Window/TextFX/Copy &c")]
		//		static void Copy ()
		//		{
		//			if(Selection.activeTransform != null)
		//			{
		//				TextFxAnimationInterface effect = Selection.activeTransform.GetComponent(typeof(TextFxAnimationInterface)) as TextFxAnimationInterface;
		//
		//				if(effect != null)
		//				{
		//					EditorPrefs.SetString("EffectExport", effect.AnimationManager.ExportData());
		//					Debug.Log("Soft Copied " + Selection.activeTransform.name);
		//				}
		//			}
		//		}
		//
		//		[MenuItem ("Window/TextFX/Copy Hard #&c")]
		//		static void CopyHard ()
		//		{
		//			if(Selection.activeTransform != null)
		//			{
		//				TextFxAnimationInterface effect = Selection.activeTransform.GetComponent(typeof(TextFxAnimationInterface)) as TextFxAnimationInterface;
		//				
		//				if(effect != null)
		//				{
		//					EditorPrefs.SetString("EffectExport", effect.AnimationManager.ExportData(hard_copy: true));
		//					Debug.Log("Hard Copied " + Selection.activeTransform.name);
		//				}
		//			}
		//		}
		//
		//		[MenuItem ("Window/TextFX/Paste &v")]
		//		static void Paste ()
		//		{
		//			if(EditorPrefs.HasKey("EffectExport") && Selection.activeTransform != null)
		//			{
		//				TextFxAnimationInterface effect = Selection.activeTransform.GetComponent(typeof(TextFxAnimationInterface)) as TextFxAnimationInterface;
		//				
		//				if(effect != null)
		//				{
		//					effect.AnimationManager.ImportData(EditorPrefs.GetString("EffectExport"));
		//					Debug.Log("Pasted " + Selection.activeTransform.name);
		//				}
		//			}
		//		}

		static string m_textFxRootDirectoryPath;

		void OnEnable()
	    {
			// Get TextFx relative folder paths
			// Assumes that this script is located in 'TextFx/Editor'
			MonoScript monoScript = MonoScript.FromScriptableObject (this);
			string managerScriptPath = AssetDatabase.GetAssetPath (monoScript);
			m_textFxRootDirectoryPath = managerScriptPath.Replace ("Editor/" + monoScript.name + ".cs", "");

			EditorApplication.update += UpdateManager;

#if ANIM_DESIGN_DEV_TOOLS
			m_selected_animation_idx = EditorPrefs.GetInt("SelectedTextFxAnim", 0);
#endif

			if(m_animation_config_intro_titles == null)
			{
				m_animation_config_intro_titles = TextFxQuickSetupAnimConfigs.IntroAnimNames;
				m_animation_config_main_titles = TextFxQuickSetupAnimConfigs.MainAnimNames;
				m_animation_config_outro_titles = TextFxQuickSetupAnimConfigs.OutroAnimNames;
			}



			// Load editor icon textures
#if UNITY_PRO_LICENSE
			m_copy_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/CopyButton_pro.png", typeof(Texture2D)) as Texture2D;
			m_paste_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/PasteButton_pro.png", typeof(Texture2D)) as Texture2D;
//			m_save_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/SaveButton_pro.png", typeof(Texture2D)) as Texture2D;
			m_play_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/PlayButton_pro.png", typeof(Texture2D)) as Texture2D;
			m_pause_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/PauseButton_pro.png", typeof(Texture2D)) as Texture2D;
			m_reset_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ResetButton_pro.png", typeof(Texture2D)) as Texture2D;
			m_continue_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ContinueButton_pro.png", typeof(Texture2D)) as Texture2D;
			m_continue_past_loop_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ContinueFromLoop_pro.png", typeof(Texture2D)) as Texture2D;
			m_repeat_off_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/RepeatButton2.png", typeof(Texture2D)) as Texture2D;
			m_repeat_on_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/RepeatButton_pro2.png", typeof(Texture2D)) as Texture2D;
			m_infinity_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/InfinitySymbol_pro.png", typeof(Texture2D)) as Texture2D;
#else

			m_copy_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/CopyButton.png", typeof(Texture2D)) as Texture2D;
			m_paste_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/PasteButton.png", typeof(Texture2D)) as Texture2D;
//			m_save_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/SaveButton.png", typeof(Texture2D)) as Texture2D;
			m_play_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/PlayButton.png", typeof(Texture2D)) as Texture2D;
			m_pause_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/PauseButton.png", typeof(Texture2D)) as Texture2D;
			m_reset_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ResetButton.png", typeof(Texture2D)) as Texture2D;
			m_continue_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ContinueButton.png", typeof(Texture2D)) as Texture2D;
			m_continue_past_loop_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ContinueFromLoop.png", typeof(Texture2D)) as Texture2D;
			m_repeat_off_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/RepeatButton_pro2.png", typeof(Texture2D)) as Texture2D;
			m_repeat_on_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/RepeatButton2.png", typeof(Texture2D)) as Texture2D;
			m_infinity_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/InfinitySymbol.png", typeof(Texture2D)) as Texture2D;
#endif
	    }
		
		void OnDisable()
		{
			EditorApplication.update -= UpdateManager;
		}

		private void UpdateManager()
		{	
			if(m_animation_manager != null && m_animation_manager.PreviewingAnimationInEditor && !m_animation_manager.PreviewAnimationPaused)
			{
				if(m_animation_manager == null || m_textfx_animation_interface == null || m_textfx_animation_interface.Equals(null))
				{
					m_animation_manager.PreviewingAnimationInEditor = false;
					return;
				}

				if(!m_animation_manager.UpdateAnimation() && m_animation_manager.ParticleEffectManagers.Count == 0)
				{
					m_animation_manager.PreviewingAnimationInEditor = false;
				}

				m_textfx_animation_interface.UpdateTextFxMesh();
				
				if(m_animation_manager.ParticleEffectManagers.Count > 0)
					SceneView.RepaintAll();
			}
		}
	    
	    void OnGUI ()
		{
			// Check whether a unique TextFx object is selected 
			if(Selection.gameObjects.Length == 1  && ((m_textfx_animation_interface == null || m_textfx_animation_interface.Equals(null)) || !Selection.gameObjects[0].Equals(m_textfx_animation_interface.GameObject)))
			{
				TextFxAnimationInterface textfx_animation_interface = Selection.gameObjects[0].GetComponent(typeof(TextFxAnimationInterface)) as TextFxAnimationInterface;
				if(textfx_animation_interface != null)
				{
					m_textfx_animation_interface = textfx_animation_interface;
					m_animation_manager = textfx_animation_interface.AnimationManager;

					if (m_animation_manager.m_master_animations == null)
						m_animation_manager.m_master_animations = new List<LetterAnimation>();
				}
			}
			
			if(m_animation_manager == null || m_animation_manager.m_master_animations == null || m_textfx_animation_interface == null || m_textfx_animation_interface.AnimationManager == null || m_textfx_animation_interface.AnimationManager.GameObject == null)
			{
				GUI.Label (new Rect (10, 10, AnimEditorWindow.position.width, LINE_HEIGHT * 2), "No TextFx instance linked!\nClick on a TextFx instance to populate this panel.");

				return;
			}

			// Display the currently linked TextFx instance object
			GUI.Label (new Rect (10, 3, AnimEditorWindow.position.width, LINE_HEIGHT), "Editing <b>'" + m_textfx_animation_interface.GameObject.name + "'</b>", WindowHeaderStyle);

			int edited_action = -1;
			bool start_of_action = true;
			ANIMATION_DATA_TYPE edited_data = ANIMATION_DATA_TYPE.ALL;

			DrawEffectEditorPanel(20);
			
			if(!ignore_gui_change)
			{
				start_of_action = editing_start_state;
				edited_action = edited_action_idx;
				edited_data = m_edited_data;
			}


			if (edited_action >= 0)
			{
				if(m_animation_manager.Playing && edited_data != ANIMATION_DATA_TYPE.NONE)
				{
					m_animation_manager.PrepareAnimationData(edited_data);
				}
				else
				{
					m_animation_manager.SetAnimationState(edited_action, start_of_action ? 0 : 1, update_action_values: true, edited_data: edited_data);
					
					m_textfx_animation_interface.UpdateTextFxMesh( );
				}
			}

			if(GUI.changed)
			{
				EditorUtility.SetDirty( m_textfx_animation_interface.ObjectInstance );

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
				// All Unity Editors between Unity 5.0 - Unity 5.2
				EditorApplication.MarkSceneDirty ();
#elif !UNITY_4_5 && !UNITY_4_6
				// All Unity Editors beyond version Unity 5.2
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty ( UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene() );
#endif
			}
	    }
		
		void OnInspectorUpdate()
		{
			Instance.Repaint();
		}
		
		void DrawLoopTree(LetterAnimation selected_animation)
		{
			float main_editor_height = Instance.position.height - WINDOW_BORDER_TOP - WINDOW_BORDER_BOTTOM;
			int num_actions = selected_animation.NumActions;
			float gui_y_offset = 110;
			int action_idx = 0;
			
			// Draw A's
			m_state_node_rects = new Rect[num_actions];
			
			EditorGUI.LabelField(new Rect(state_overview_x, gui_y_offset, 100, LINE_HEIGHT*2), "Loops [" + selected_animation.NumLoops + "]", EditorStyles.boldLabel);
			gui_y_offset += LINE_HEIGHT*2;
			
			if(GUI.Button(new Rect(state_overview_x, gui_y_offset, 60, LINE_HEIGHT), new GUIContent(m_display_loops_tree ? "Hide" : "Show", (m_display_loops_tree ? "Hide" : "Show") + " the loops setup menu.")))
			{
				m_display_loops_tree = !m_display_loops_tree;
			}
			gui_y_offset += LINE_HEIGHT * 2;
			
			
			LOOP_SCROLL_POS = GUI.BeginScrollView(
				new Rect(state_overview_x, gui_y_offset, MainEditorX - state_overview_x - 14, main_editor_height - (gui_y_offset - WINDOW_BORDER_TOP)),
				LOOP_SCROLL_POS,
				new Rect(0,0, MainEditorX - 35, selected_animation.NumActions * ACTION_NODE_SPACING)
			);
			
			gui_y_offset = 0;
			
			for(action_idx = 0; action_idx < selected_animation.NumActions; action_idx++)
			{
				GUI.Label(new Rect(20, gui_y_offset, 40, 20), "A" + action_idx, EditorStyles.boldLabel);

				if(selected_animation.GetAction(action_idx).m_action_type == ACTION_TYPE.BREAK)
					GUI.DrawTexture(new Rect(5, gui_y_offset, 15, 15), m_pause_button_texture);

				if(m_display_loops_tree)
				{
					m_state_node_rects[action_idx] = new Rect(20 - ACTION_NODE_MARGIN, gui_y_offset - ACTION_NODE_MARGIN, 20 + (2 * ACTION_NODE_MARGIN), 20 + (2 * ACTION_NODE_MARGIN));
				}
				
				gui_y_offset += ACTION_NODE_SPACING;
			}
			
			
			if(m_display_loops_tree)
			{
				// Draw existing loops
				Vector2 point1, point2;
				
				// Draw Loop assignment-in-progress line, triggered by users click and drag on action nodes
				if(m_mouse_down_node_idx >= 0)
				{
					TextFxHelperMethods.DrawGUILine(m_mouse_down_pos, new Vector2(m_mouse_down_pos.x, m_mouse_drag_pos.y), Color.gray, 3);
				}
				
				state_overview_width = 0;
				
				int num_loops = selected_animation.NumLoops;
				GUIStyle my_style = EditorStyles.miniButton;
				my_style.alignment = TextAnchor.MiddleLeft;
				my_style.normal.textColor = Color.black;
				
				
				// Loop through loop list, drawing loop line representations on loop timeline and adding loop list entries
				float loops_list_x;
				ActionLoopCycle loop_cycle;
				int last_span_width = -1;
				int indent_num = 0;
				float line_offset_x = 0;
				float loop_list_line_y = 0;
				
				GUI.SetNextControlName ("LoopsTitle");
				
				// Display loop list header
				EditorGUI.LabelField(new Rect(loop_tree_width + 60, loop_list_line_y, 100, LINE_HEIGHT), "Active Loops", EditorStyles.boldLabel);
				loop_list_line_y += LINE_HEIGHT;
				
				EditorGUI.LabelField(new Rect(loop_tree_width + 52, loop_list_line_y, 30, LINE_HEIGHT), new GUIContent("From","The index of the action to start this loop."), EditorStyles.miniBoldLabel);
				EditorGUI.LabelField(new Rect(loop_tree_width + 87, loop_list_line_y, 20, LINE_HEIGHT), new GUIContent("To", "The index of the action to end this loop."), EditorStyles.miniBoldLabel);
				EditorGUI.LabelField(new Rect(loop_tree_width + 110, loop_list_line_y, 42, LINE_HEIGHT), new GUIContent("#Loops", "The number of times to run through this loop. Enter zero to run loop infinitely."), EditorStyles.miniBoldLabel);
				EditorGUI.LabelField(new Rect(loop_tree_width + 160, loop_list_line_y, 40, LINE_HEIGHT), new GUIContent("Type", "The type/behaviour of this loop."), EditorStyles.miniBoldLabel);
				EditorGUI.LabelField(new Rect(loop_tree_width + 230, loop_list_line_y, 70, LINE_HEIGHT), new GUIContent("FAE", "'Finish At End' - Applied to Loop Reverse loops only. Forces the animation to finish in the END state of the loop reverse, instead of continuing from the start, which is the default."), EditorStyles.miniBoldLabel);
				EditorGUI.LabelField(new Rect(loop_tree_width + 262, loop_list_line_y, 70, LINE_HEIGHT), new GUIContent("DFO", "'Delay First Only' - Letter action delays (non-constant) will only be applied for the first forward pass through the loop. This stops the letters getting more and more out of sequence with every loop interation."), EditorStyles.miniBoldLabel);

				loop_list_line_y += LINE_HEIGHT;
				
				
				for(int loop_idx=0; loop_idx < num_loops; loop_idx++)
				{
					loop_cycle = selected_animation.GetLoop(loop_idx);
					
					if(last_span_width == -1)
					{
						last_span_width = loop_cycle.SpanWidth;
					}
					
					// Check for invalid loops
					if(loop_cycle.m_start_action_idx >= num_actions || loop_cycle.m_end_action_idx >= num_actions)
					{
						// invalid loop. Delete it.
						selected_animation.RemoveLoop(loop_idx);
						loop_idx--;
						num_loops--;
						continue;
					}
					
					// Represent loop as a line on the Action timeline
					if(last_span_width != loop_cycle.SpanWidth)
					{
						last_span_width = loop_cycle.SpanWidth;
						indent_num++;
					}
					
					line_offset_x = 20 + (indent_num * LOOP_LINE_OFFSET);
					
					if(loop_cycle.m_start_action_idx != loop_cycle.m_end_action_idx)
					{
						point1 = m_state_node_rects[loop_cycle.m_start_action_idx].center + new Vector2(line_offset_x, 0);
						point2 = m_state_node_rects[loop_cycle.m_end_action_idx].center + new Vector2(line_offset_x, 0);
						
						TextFxHelperMethods.DrawGUILine(point1, point2, loop_cycle.m_loop_type == LOOP_TYPE.LOOP ? LOOP_COLOUR : LOOP_REVERSE_COLOUR, 2);
						
						TextFxHelperMethods.DrawGUILine(point1 + new Vector2(0, -2), point1 + new Vector2(0, 2), loop_cycle.m_loop_type == LOOP_TYPE.LOOP ? LOOP_COLOUR : LOOP_REVERSE_COLOUR, 6);
						TextFxHelperMethods.DrawGUILine(point2 + new Vector2(0, -2), point2 + new Vector2(0, 2), loop_cycle.m_loop_type == LOOP_TYPE.LOOP ? LOOP_COLOUR : LOOP_REVERSE_COLOUR, 6);
					}
					else
					{
						point1 = m_state_node_rects[loop_cycle.m_start_action_idx].center + new Vector2(line_offset_x, -2);
						point2 = m_state_node_rects[loop_cycle.m_end_action_idx].center + new Vector2(line_offset_x, 2);
						
						TextFxHelperMethods.DrawGUILine(point1, point2, loop_cycle.m_loop_type == LOOP_TYPE.LOOP ? LOOP_COLOUR : LOOP_REVERSE_COLOUR, 6);
					}
					
					// Display loop number
					my_style.normal.textColor = loop_cycle.m_loop_type == LOOP_TYPE.LOOP ? LOOP_COLOUR : LOOP_REVERSE_COLOUR;
					EditorGUI.LabelField(new Rect(point1.x, (point1.y + point2.y) / 2 - 10, loop_cycle.m_number_of_loops > 9 ? 30 : 20, 20),  loop_cycle.m_number_of_loops <= 0 ? "~" : "" + loop_cycle.m_number_of_loops, my_style); //EditorStyles.miniButton);
					
					bool gui_changed = GUI.changed;

					// Display list view of loop cycle
					loops_list_x = loop_tree_width;
					EditorGUI.LabelField(new Rect(loops_list_x, loop_list_line_y, 100, LINE_HEIGHT), "Loop " + (loop_idx+1));
					loops_list_x += 58;
					EditorGUI.LabelField(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), "" + loop_cycle.m_start_action_idx);
					loops_list_x += 30;
					EditorGUI.LabelField(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), "" + loop_cycle.m_end_action_idx);
					loops_list_x += 32;
					loop_cycle.m_number_of_loops = Mathf.Max( EditorGUI.IntField(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), loop_cycle.m_number_of_loops), 0);
					loops_list_x += 30;
					gui_changed = GUI.changed;
					loop_cycle.m_loop_type = (LOOP_TYPE) EditorGUI.EnumPopup(new Rect(loops_list_x, loop_list_line_y, 70, LINE_HEIGHT), loop_cycle.m_loop_type);
					if(!gui_changed && GUI.changed)
						m_animation_manager.PrepareAnimationData();
					loops_list_x += 85;
					if(loop_cycle.m_loop_type == LOOP_TYPE.LOOP_REVERSE)
					{
						gui_changed = GUI.changed;
						loop_cycle.m_finish_at_end = EditorGUI.Toggle(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), loop_cycle.m_finish_at_end);
						if(!gui_changed && GUI.changed)
							m_animation_manager.PrepareAnimationData();
					}
					loops_list_x += 35;
					if(loop_cycle.m_loop_type == LOOP_TYPE.LOOP_REVERSE)
						loop_cycle.m_delay_first_only = EditorGUI.Toggle(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), loop_cycle.m_delay_first_only);
					loops_list_x += 30;
					if(GUI.Button(new Rect(loops_list_x, loop_list_line_y, 24, LINE_HEIGHT), "x") && m_animation_manager.WipeQuickSetupData(user_confirm: true))
					{
						selected_animation.RemoveLoop(loop_idx);
						num_loops--;

						m_animation_manager.PrepareAnimationData();
						continue;
					}
					
					loop_list_line_y += LINE_HEIGHT;
					
				}
				
				loop_list_line_y += 5;
				
				// "Add new loop" line
				loops_list_x = loop_tree_width;
				EditorGUI.LabelField(new Rect(loops_list_x, loop_list_line_y, 100, LINE_HEIGHT), "New", EditorStyles.boldLabel);
				loops_list_x += 58;
				new_loop_from = EditorGUI.IntField(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), new_loop_from);
				loops_list_x += 30;
				new_loop_to = EditorGUI.IntField(new Rect(loops_list_x, loop_list_line_y, 20, LINE_HEIGHT), new_loop_to);
				loops_list_x += 32;
				if(GUI.Button(new Rect(loops_list_x, loop_list_line_y, 40, LINE_HEIGHT), "Add") && m_animation_manager.WipeQuickSetupData(user_confirm: true))
				{
					selected_animation.AddLoop(new_loop_from, new_loop_to, false);
					m_animation_manager.PrepareAnimationData();
					
					new_loop_from = 0;
					new_loop_to = 0;
					
					// Force keyboard focus loss from any of the loop adding fields.
					GUI.FocusControl("LoopsTitle");
				}
				
				// Set the width of the loop tree segment
				loop_tree_width = (int) line_offset_x + BASE_LOOP_LIST_POSITION_OFFSET;
				
				// Add additional width of loop list menu
				state_overview_width = loop_tree_width + LOOP_LIST_WIDTH;
				
				
				EventType eventType = Event.current.type;
				Vector2 mousePos = Event.current.mousePosition;
				
			    if (eventType == EventType.MouseDown && mousePos.x < MainEditorX)
			    {
					int rect_idx = 0;
					foreach(Rect node_rect in m_state_node_rects)
					{
						if(node_rect.Contains(mousePos))
						{
							m_mouse_down_node_idx= rect_idx;
							m_mouse_down_pos = node_rect.center;
							m_mouse_drag_pos = m_mouse_down_pos;
						}
						
						rect_idx ++;
					}
			    }
				else if(eventType == EventType.MouseDrag)
				{
					if(m_mouse_down_node_idx >= 0)
					{
						m_mouse_drag_pos = mousePos;
						
						Instance.Repaint();
					}
				}
				else if(eventType == EventType.MouseUp)
				{
					if(m_mouse_down_node_idx >= 0 && mousePos.x < MainEditorX)
					{
						int rect_idx = 0;
						foreach(Rect node_rect in m_state_node_rects)
						{
							if(node_rect.Contains(mousePos))
							{
	//							Debug.LogError("you joined : " + m_mouse_down_node_idx + " and " + rect_idx + " with button " + Event.current.button);
								
								int start, end;
								if(m_mouse_down_node_idx < rect_idx)
								{
									start = m_mouse_down_node_idx;
									end = rect_idx;
								}
								else
								{
									start = rect_idx;
									end = m_mouse_down_node_idx;
								}
								
								selected_animation.AddLoop(start, end, Event.current.button == 1);
	//							m_animation_manager.PrepareAnimationData();
								
								break;
							}
							
							rect_idx ++;
						}
					}
					
					m_mouse_down_node_idx = -1;
					Instance.Repaint();
				}
			}
			else
			{
				state_overview_width = 0;
			}
			
			GUI.EndScrollView();
		}


		float HEIGHT_OF_DEFAULT_SETTINGS_GUI_PANEL = 215;
		const float PLAYBACK_SECTION_HEIGHT = 65;
		const float PLAYBACK_BUTTONS_WIDTH = 40;
		const float PLAYBACK_BUTTONS_HEIGHT = 40;
		const float COPY_PASTE_SECTION_WIDTH = 110;
		Vector2 m_playbackButtonsScrollPosition;
		float m_playbackButtonsAreaWidth = PLAYBACK_BUTTONS_WIDTH * 2 + 20;
		// Debug options
		ContinueType m_editor_continue_type = ContinueType.EndOfLoop;
		float m_editor_continue_lerp_duration = 0;
		int m_editor_continue_animation_index = 0;
		bool m_editor_continue_pass_next_infinite_loop = true;
		float m_editor_continue_anim_speed_override = 1;
		bool m_editor_continue_shorten_interim_loops = true;


		void DrawEffectEditorPanel(float y_offset = 0)
		{
			float gui_x_offset = 10;
			float gui_y_offset = WINDOW_BORDER_TOP + y_offset;

			m_playbackButtonsScrollPosition = GUI.BeginScrollView (new Rect (0, gui_y_offset, Instance.position.width - COPY_PASTE_SECTION_WIDTH, PLAYBACK_SECTION_HEIGHT), m_playbackButtonsScrollPosition, new Rect (0, 0, m_playbackButtonsAreaWidth, 40));

			if(GUI.Button(new Rect(gui_x_offset, 0, 40, PLAYBACK_BUTTONS_HEIGHT), !m_animation_manager.PreviewingAnimationInEditor || m_animation_manager.PreviewAnimationPaused ? m_play_button_texture : m_pause_button_texture))
			{
				if(m_animation_manager.PreviewingAnimationInEditor)
				{
					m_animation_manager.PreviewAnimationPaused = !m_animation_manager.PreviewAnimationPaused;
				}
				else
				{
					m_animation_manager.PlayEditorAnimation();
				}
			}

			gui_x_offset += PLAYBACK_BUTTONS_WIDTH + 10;

			if(GUI.Button(new Rect(gui_x_offset, 0, PLAYBACK_BUTTONS_WIDTH, PLAYBACK_BUTTONS_HEIGHT), m_reset_button_texture))
			{
				m_animation_manager.ResetEditorAnimation();
            }

			gui_x_offset = (PLAYBACK_BUTTONS_WIDTH * 3);




			if(m_animation_manager.Playing)
			{
				// Draw divider line
				TextFxHelperMethods.DrawGUILine(new Rect(PLAYBACK_BUTTONS_WIDTH * 3 - 10, 0, 0, PLAYBACK_BUTTONS_HEIGHT), Color.gray, 3);

				m_playbackButtonsAreaWidth = PLAYBACK_BUTTONS_WIDTH * 2 + 20;

				// Continue Past Break Button
				if(GUI.Button(new Rect(gui_x_offset, 0, PLAYBACK_BUTTONS_WIDTH, PLAYBACK_BUTTONS_HEIGHT), m_continue_button_texture))
				{
					m_animation_manager.ContinuePastBreak(m_editor_continue_animation_index);
//					Debug.Log("Editor ContinuePastBreak");
				}

				gui_x_offset += PLAYBACK_BUTTONS_WIDTH + 10;

				// Continue Past Loop Button
				if(GUI.Button(new Rect(gui_x_offset, 0, PLAYBACK_BUTTONS_WIDTH, PLAYBACK_BUTTONS_HEIGHT), m_continue_past_loop_button_texture))
				{
					m_animation_manager.ContinuePastLoop(m_editor_continue_animation_index, m_editor_continue_type, m_editor_continue_lerp_duration, m_editor_continue_pass_next_infinite_loop, m_editor_continue_shorten_interim_loops, m_editor_continue_anim_speed_override);
//					Debug.Log("Editor ContinuePastLoop");

//					PauseEditorAnimation();
				}
				
				gui_x_offset += PLAYBACK_BUTTONS_WIDTH + 10;

				m_playbackButtonsAreaWidth +=  PLAYBACK_BUTTONS_WIDTH + 10;

			}
			else
				m_playbackButtonsAreaWidth = PLAYBACK_BUTTONS_WIDTH * 2 + 20;

#if ANIM_DESIGN_DEV_TOOLS
			// TODO: Display this info in a sensible non-cluttered/confusing way.
			gui_x_offset += 10;
			
			string[] anim_idx_select_array = new string[m_animation_manager.m_master_animations.Count];
			for(int anim_idx=0; anim_idx < anim_idx_select_array.Length; anim_idx++)
				anim_idx_select_array[anim_idx] = "" + anim_idx;
			EditorGUI.LabelField (new Rect (gui_x_offset, WINDOW_BORDER_TOP - 5, 40, LINE_HEIGHT), "Anim");
			m_editor_continue_animation_index = EditorGUI.Popup (new Rect (gui_x_offset + 45, WINDOW_BORDER_TOP - 5, 35, LINE_HEIGHT), m_editor_continue_animation_index, anim_idx_select_array);
			
			
			if(m_editor_continue_type == ContinueType.None)
				m_editor_continue_type = ContinueType.EndOfLoop;
			
			m_editor_continue_type = (ContinueType) (1 + EditorGUI.Popup (new Rect (gui_x_offset + 90, WINDOW_BORDER_TOP - 5, 80, LINE_HEIGHT), ((int) m_editor_continue_type) - 1, new string[]{"Instant", "End Of Loop"}));
			
			EditorGUI.LabelField (new Rect (gui_x_offset, WINDOW_BORDER_TOP + 12, 70, LINE_HEIGHT), "Exit Infinite");
			m_editor_continue_pass_next_infinite_loop = EditorGUI.Toggle (new Rect (gui_x_offset + 74, WINDOW_BORDER_TOP + 12, 20, LINE_HEIGHT), m_editor_continue_pass_next_infinite_loop);
			
			
			if(m_editor_continue_type == ContinueType.Instant)
			{
				EditorGUI.LabelField (new Rect (gui_x_offset, WINDOW_BORDER_TOP + LINE_HEIGHT + 8, 60, LINE_HEIGHT), "Lerp Time");
				m_editor_continue_lerp_duration = EditorGUI.FloatField (new Rect (gui_x_offset + 65, WINDOW_BORDER_TOP + LINE_HEIGHT + 8, 30, 15), m_editor_continue_lerp_duration);
			}
			else if(m_editor_continue_type == ContinueType.EndOfLoop)
			{
				EditorGUI.LabelField (new Rect (gui_x_offset, WINDOW_BORDER_TOP + LINE_HEIGHT + 8, 74, LINE_HEIGHT), "Anim Speed");
				m_editor_continue_anim_speed_override = EditorGUI.FloatField (new Rect (gui_x_offset + 75, WINDOW_BORDER_TOP + LINE_HEIGHT + 8, 20, 15), m_editor_continue_anim_speed_override);
				
				EditorGUI.LabelField (new Rect (gui_x_offset + 100, WINDOW_BORDER_TOP + LINE_HEIGHT + 8, 70, LINE_HEIGHT), "Trim Loops");
				m_editor_continue_shorten_interim_loops = EditorGUI.Toggle (new Rect (gui_x_offset + 175, WINDOW_BORDER_TOP + LINE_HEIGHT + 8, 20, 15), m_editor_continue_shorten_interim_loops);
			}
#endif




			GUI.EndScrollView ();

			TextFxHelperMethods.DrawGUILine(new Rect(0, y_offset, Instance.position.width, 0), Color.gray, 5);

			// Draw copy/paste section divider line
			TextFxHelperMethods.DrawGUILine(new Rect(Instance.position.width - COPY_PASTE_SECTION_WIDTH + 5, y_offset, 0, PLAYBACK_BUTTONS_HEIGHT + 20), Color.gray, 3);


			GUIStyle style = new GUIStyle(EditorStyles.miniButton);

			style.fontSize = 10;

			// COPY_PASTE_SECTION_WIDTH
			gui_x_offset = Instance.position.width - COPY_PASTE_SECTION_WIDTH + 13;

//			if(GUI.Button(new Rect(gui_x_offset, 3, 70, 20), new GUIContent("Copy [S]", "Soft Copy this TextEffect animation configuration, not including any Text settings."), style))
//			{
//				string json_data = m_animation_manager.ExportData();
//				EditorGUIUtility.systemCopyBuffer = json_data;
//				EditorPrefs.SetString("EffectExport", json_data);
//				Debug.Log("Soft Copied " + m_textfx_animation_interface.GameObject.name);
//			}
			if(GUI.Button(new Rect(gui_x_offset, gui_y_offset, 40, 40), m_copy_button_texture))
			{
				string json_data = m_animation_manager.ExportData(hard_copy: true);
				EditorGUIUtility.systemCopyBuffer = json_data;
				EditorPrefs.SetString("EffectExport", json_data);
				Debug.Log("Hard Copied " + m_textfx_animation_interface.GameObject.name);
			}
			if(GUI.Button(new Rect(gui_x_offset + 45, gui_y_offset, 40, 40), m_paste_button_texture))
			{
				if(EditorPrefs.HasKey("EffectExport"))
				{
					m_animation_manager.ImportData(EditorPrefs.GetString("EffectExport"), true);
					Debug.Log("Pasted onto " + m_textfx_animation_interface.GameObject.name);
				}
			}
//			if(GUI.Button(new Rect(gui_x_offset + 90, WINDOW_BORDER_TOP, 40, 40), m_save_button_texture))
//			{
//				// SAVE animations data to a txt file
//				string json_data = m_animation_manager.ExportData(hard_copy: true);
//
//				m_animation_manager.m_effect_name = TextFxHelperMethods.SaveEffect(m_animation_manager.m_effect_name, json_data);
//
//				EditorToolsHelper.RewriteImportEffectNames();
//
//				AssetDatabase.Refresh();
//
//				LoadInPresetAnimationNames();
//
//				GUI.FocusControl("");
//			}



			// Draw Playback button section divider
			TextFxHelperMethods.DrawGUILine(new Rect(0, y_offset + PLAYBACK_SECTION_HEIGHT - 3, Instance.position.width, 0), Color.gray, 5);

			gui_y_offset += 55;

			// Add a notice about any available update packages
			if(!TextFxMenuItem.TextFxMenuItem.IgnoringCurrentUpdate)
			{
				bool currentRichTextState = GUI.skin.box.richText;
				GUI.skin.box.richText = true;

				GUI.Box(
					new Rect(0, gui_y_offset, Instance.position.width, LINE_HEIGHT * 4),
					new GUIContent("<color=red><b>TextFx Update v" + TextFxMenuItem.TextFxMenuItem.AvailabePackageVersionString + " is available now!</b></color>\n"));

				if(GUI.Button(new Rect(Instance.position.width/2 - 100, gui_y_offset + LINE_HEIGHT, 200, LINE_HEIGHT), "View Changelog"))
				{
					TextFxMenuItem.TextFxMenuItem.ShowUpdateChangelog();
				}
				if (GUI.Button(new Rect(Instance.position.width / 2 - 100, gui_y_offset + 10 + LINE_HEIGHT * 2, 200, LINE_HEIGHT), "Update Package"))
				{
					TextFxMenuItem.TextFxMenuItem.UpdateTextFxPackage();
				}

				gui_y_offset += LINE_HEIGHT * 4;

				GUI.skin.box.richText = currentRichTextState;
			}



			m_editor_section_idx = GUI.Toolbar(new Rect(0, gui_y_offset, Instance.position.width, LINE_HEIGHT), m_editor_section_idx, new string[]{"Quick Setup", "Full Editor"}); //, EditorStyles.toolbarButton);

			if(GUI.changed)
				return;

			gui_y_offset += 25;



			ignore_gui_change = false;
			noticed_gui_change = false;
			edited_action_idx = -1;
			m_edited_data = ANIMATION_DATA_TYPE.NONE;

			if(m_editor_section_idx == 0)
			{
				DrawQuickSetupPanel(gui_y_offset);
			}
			else
			{
				DrawFullEditorPanel(gui_y_offset);
			}
		}


		void DrawQuickSetupPanel(float gui_y_offset)
		{
#if ANIM_DESIGN_DEV_TOOLS
			m_show_raw_import_settings = EditorGUI.Toggle (new Rect (Instance.position.width - 20, gui_y_offset, 20, 20), m_show_raw_import_settings);

			if(m_show_raw_import_settings)
			{
				float xOffset = 100;
				
				// Handle Effect imports
				GUI.Label(new Rect(xOffset, gui_y_offset, 200, 20), "Import Preset Effect", EditorStyles.boldLabel);
				gui_y_offset += LINE_HEIGHT;
				
				m_selected_animation_idx = EditorGUI.Popup (new Rect (xOffset, gui_y_offset, 250, 20), m_selected_animation_idx, m_animation_config_titles);
				
				if(GUI.changed)
				{
					EditorPrefs.SetInt("SelectedTextFxAnim", m_selected_animation_idx);
				}
				
				m_animation_manager.m_import_as_section = EditorGUI.Toggle (new Rect (xOffset + 260, gui_y_offset - 20, 170, LINE_HEIGHT), "Import as Section?", m_animation_manager.m_import_as_section);
				
				if(GUI.Button(new Rect(xOffset + 260, gui_y_offset, 80, 20), "Apply") && m_animation_manager.WipeQuickSetupData(user_confirm: true))
				{
					//
					string path = m_textFxRootDirectoryPath + "AnimationConfigs/" + m_animation_config_titles[m_selected_animation_idx] + ".txt";

					path = path.Replace("\n", "");
					path = path.Replace("\r", "");
					
					TextAsset animation_config_data = AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset;

					Debug.Log("Importing from : " + path);
					
					if(animation_config_data != null)
					{
						if(m_animation_manager.m_import_as_section
						   //						m_animation_config_titles[m_selected_animation_idx].Contains("Intro") ||
						   //					   m_animation_config_titles[m_selected_animation_idx].Contains("Main") ||
						   //					   m_animation_config_titles[m_selected_animation_idx].Contains("Outro")
						   )
						{
							// Importing a preset animation section
							m_animation_manager.ImportPresetAnimationSectionData(animation_config_data.text, true);
						}
						else
						{
							// Importing a full effect
							m_animation_manager.ImportData(animation_config_data.text, true);
						}
						
						//					Debug.Log("TextFx animation '" + m_animation_config_titles[m_selected_animation_idx] + "' imported");
						
						m_animation_manager.m_effect_name = (m_animation_config_titles[m_selected_animation_idx].Split('/'))[1];
						
						// Wipe all Quick Setup data
						m_animation_manager.WipeQuickSetupData();
					}
				}
				
				if(GUI.Button(new Rect(xOffset + 350, gui_y_offset, 120, 20), "Refresh List"))
				{
					EditorToolsHelper.RewriteImportEffectNames();
					
					AssetDatabase.Refresh();
					
					LoadInPresetAnimationNames();
				}
				
				gui_y_offset += LINE_HEIGHT;
				gui_y_offset += LINE_HEIGHT;
				
				m_animation_manager.m_effect_name = EditorGUI.TextField (new Rect (xOffset, gui_y_offset, 320, 20), "Name", m_animation_manager.m_effect_name);
				
				gui_y_offset += LINE_HEIGHT;
				gui_y_offset += LINE_HEIGHT;
				
				if(m_animation_manager.m_preset_effect_settings != null)
				{
					foreach(PresetEffectSetting effect_setting in m_animation_manager.m_preset_effect_settings)
					{
						effect_setting.DrawGUISetting(m_animation_manager, 10, ref gui_y_offset, GUI.changed);
					}
				}
				
				// Draw copy/paste section divider line
				TextFxHelperMethods.DrawGUILine(new Rect(0, gui_y_offset, Instance.position.width, 0), Color.gray, 3);
			}
#endif


			// Play On Start option
			gui_y_offset += 10;
			m_animation_manager.m_begin_on_start = EditorGUI.Toggle(new Rect(10, gui_y_offset, 180, LINE_HEIGHT), "Play On Start", m_animation_manager.m_begin_on_start);
			gui_y_offset += LINE_HEIGHT;
			if(m_animation_manager.m_begin_on_start)
			{
				m_animation_manager.m_begin_delay = Mathf.Max( EditorGUI.FloatField(new Rect(25, gui_y_offset, 220, INPUT_FIELD_HEIGHT), "Delay", m_animation_manager.m_begin_delay), 0 );
				gui_y_offset += LINE_HEIGHT;
			}
			m_animation_manager.m_time_type = (AnimationTime) EditorGUI.EnumPopup (new Rect (10, gui_y_offset, 280, LINE_HEIGHT), "Time", m_animation_manager.m_time_type);
			gui_y_offset += LINE_HEIGHT;
			m_animation_manager.m_animation_speed_factor = EditorGUI.FloatField (new Rect (10, gui_y_offset, 180, INPUT_FIELD_HEIGHT), "Speed Factor", m_animation_manager.m_animation_speed_factor);
			gui_y_offset += LINE_HEIGHT;

			m_animation_manager.m_on_finish_action = (ON_FINISH_ACTION) EditorGUI.EnumPopup (new Rect (10, gui_y_offset, 280, INPUT_FIELD_HEIGHT), "On Finish", m_animation_manager.m_on_finish_action);
			gui_y_offset += LINE_HEIGHT;


			gui_y_offset += LINE_HEIGHT;
			EditorGUI.LabelField (new Rect (10, gui_y_offset, 250, 30), "Animation Sections", BigHeaderGUIStyle);
			gui_y_offset += 30;


			QUICK_SETUP_SCROLL_POS = GUI.BeginScrollView (new Rect (0, gui_y_offset, Instance.position.width, Instance.position.height - gui_y_offset), QUICK_SETUP_SCROLL_POS, new Rect(0,0,Instance.position.width - 20,m_quick_setup_panel_height) );

			bool gui_changed = GUI.changed;

			gui_y_offset = 0;


			// Draw global loop option
			if(m_animation_manager.m_preset_main.m_active && !m_animation_manager.m_preset_intro.m_active && !m_animation_manager.m_preset_outro.m_active)
			{
				GUI.color = Color.gray;
				GUI.enabled = false;
			}


			if(GUI.Button (new Rect (310, gui_y_offset - 7, 40, 40), m_animation_manager.m_repeat_all_sections ? m_repeat_on_button_texture : m_repeat_off_button_texture, ButtonImageOnlyGUIStyle))
			{
				m_animation_manager.m_repeat_all_sections = !m_animation_manager.m_repeat_all_sections;
			}

			if(!gui_changed && GUI.changed)
			{
				// Update section loop settings
				if(!m_animation_manager.m_repeat_all_sections)
				{
					// Remove global repeat loop
					m_animation_manager.m_master_animations[0].RemoveLoop(m_animation_manager.GlobalRepeatLoopStartIndex);
				}
				else
				{
					// Add in a repeat loop
					ActionLoopCycle new_loop = new ActionLoopCycle();
					new_loop.m_start_action_idx = 0;
					new_loop.m_end_action_idx = m_animation_manager.m_preset_outro.m_start_action + (m_animation_manager.m_preset_outro.m_active ? m_animation_manager.m_preset_outro.m_num_actions : -1);
					new_loop.m_number_of_loops = m_animation_manager.m_repeat_all_sections_count;
					
					m_animation_manager.m_master_animations[0].InsertLoop(m_animation_manager.GlobalRepeatLoopStartIndex, new_loop);
				}
			}
			
			if(m_animation_manager.m_repeat_all_sections)
			{
				// display repeat num field
				m_animation_manager.m_repeat_all_sections_count = Mathf.Max(EditorGUI.IntField(new Rect(355, gui_y_offset + 2, 30, LINE_HEIGHT), m_animation_manager.m_repeat_all_sections_count), 0);
				
				if(m_animation_manager.m_repeat_all_sections_count == 0)
				{
					GUI.DrawTexture(new Rect(390, gui_y_offset - 8, 40, 40), m_infinity_texture);
				}
			}

			// Return GUI to active state again
			GUI.color = Color.white;
			GUI.enabled = true;

			gui_y_offset += 45;


			// Handle drawing the animation sections
			DrawGUIAnimationSettings (TextFxAnimationManager.PRESET_ANIMATION_SECTION.INTRO, TextFxAnimationManager.ANIM_INTROS_FOLDER_NAME, ref gui_y_offset, ref m_animation_manager.m_intro_animation_foldout, ref m_animation_manager.m_selected_intro_animation_idx, ref m_animation_manager.m_preset_intro, m_animation_config_intro_titles);
			DrawGUIAnimationSettings (TextFxAnimationManager.PRESET_ANIMATION_SECTION.MAIN, TextFxAnimationManager.ANIM_MAINS_FOLDER_NAME, ref gui_y_offset, ref m_animation_manager.m_main_animation_foldout, ref m_animation_manager.m_selected_main_animation_idx, ref m_animation_manager.m_preset_main, m_animation_config_main_titles);
			DrawGUIAnimationSettings (TextFxAnimationManager.PRESET_ANIMATION_SECTION.OUTRO, TextFxAnimationManager.ANIM_OUTROS_FOLDER_NAME, ref gui_y_offset, ref m_animation_manager.m_outro_animation_foldout, ref m_animation_manager.m_selected_outro_animation_idx, ref m_animation_manager.m_preset_outro, m_animation_config_outro_titles);


			GUI.EndScrollView ();

			// Check that current scrollview height matches the height of the content within it. Else update the value and call to redraw.
			if(m_quick_setup_panel_height != gui_y_offset)
			{
				m_quick_setup_panel_height = gui_y_offset;
				Instance.Repaint();
			}
		}

		void DrawGUIAnimationSettings(TextFxAnimationManager.PRESET_ANIMATION_SECTION section, string folder_name, ref float gui_y_offset, ref bool foldout, ref int selected_index, ref TextFxAnimationManager.PresetAnimationSection preset_anim_section, string[] anim_titles)
		{
			if(selected_index > 0)
				foldout = EditorGUI.Foldout (new Rect (20, gui_y_offset, 80, 20), foldout, TextFxAnimationManager.m_animation_section_names[(int) section], true, FoldOutHeaderGUIStyle);
			else
				EditorGUI.LabelField(new Rect (20, gui_y_offset, 80, 20), TextFxAnimationManager.m_animation_section_names[(int) section], HeaderGUIStyle);

			bool gui_changed = GUI.changed;

			// Draw Section loop options
			if(section == TextFxAnimationManager.PRESET_ANIMATION_SECTION.MAIN && selected_index > 0)
			{
				if(GUI.Button (new Rect (310, gui_y_offset - 7, 40, 40), preset_anim_section.m_repeat ? m_repeat_on_button_texture : m_repeat_off_button_texture, ButtonImageOnlyGUIStyle))
				{
					preset_anim_section.m_repeat = !preset_anim_section.m_repeat;
				}

				if(!gui_changed && GUI.changed)
				{
					// Update section loop settings
					if(!preset_anim_section.m_repeat)
					{
						// Remove section repeat loop
						m_animation_manager.m_master_animations[0].RemoveLoop(GetSectionRepeatLoopIndex(section));
					}
					else
					{
						// Add in a repeat loop
						ActionLoopCycle new_loop = new ActionLoopCycle();
						new_loop.m_start_action_idx = preset_anim_section.m_start_action;
						new_loop.m_end_action_idx = preset_anim_section.m_start_action + preset_anim_section.m_num_actions;
						new_loop.m_number_of_loops = preset_anim_section.m_repeat_count;

						m_animation_manager.m_master_animations[0].InsertLoop(GetSectionRepeatLoopIndex(section), new_loop);
					}
				}

				if(preset_anim_section.m_repeat)
				{
					gui_changed = GUI.changed;

					// display repeat num field
					preset_anim_section.m_repeat_count = Mathf.Max(EditorGUI.IntField(new Rect(355, gui_y_offset + 2, 30, LINE_HEIGHT), preset_anim_section.m_repeat_count), 0);
					
					if(preset_anim_section.m_repeat_count == 0)
					{
						GUI.DrawTexture(new Rect(390, gui_y_offset - 8, 40, 40), m_infinity_texture);
					}

					if(!gui_changed && GUI.changed)
					{
						// Update loop with current repeat count
						ActionLoopCycle repeat_loop = m_animation_manager.m_master_animations[0].GetLoop(GetSectionRepeatLoopIndex(section));
						repeat_loop.m_number_of_loops = preset_anim_section.m_repeat_count;
					}
				}
			}
			

			gui_changed = GUI.changed;

			selected_index = EditorGUI.Popup (new Rect (120, gui_y_offset, 180, 20), selected_index, anim_titles, PopupHeaderGUIStyle);
			
			if(!gui_changed && GUI.changed)
			{
				if(m_animation_manager.WipeFullEditorData(user_confirm: true))
				{
					// Apply newly selected animation section
					m_animation_manager.SetQuickSetupSection(section, anim_titles[selected_index]);
				}
				else
				{
					// Set selected anim index back to NONE
					selected_index = 0;
				}
			}
			
			gui_y_offset += 35;
			
			if(foldout && selected_index > 0)
			{
				if(preset_anim_section.m_preset_effect_settings == null || preset_anim_section.m_preset_effect_settings.Count == 0)
				{
					return;
				}

				bool setting_changed = false;

				float gui_x_offset = 60;

				GUI.Label(new Rect(gui_x_offset - 5, gui_y_offset, 120, LINE_HEIGHT), "Section Settings", EditorStyles.boldLabel);

				if(GUI.Button(new Rect(gui_x_offset + 150, gui_y_offset, 100, LINE_HEIGHT), "Play Section"))
					setting_changed = true;

				gui_y_offset += 30;

				foreach(PresetEffectSetting effect_setting in preset_anim_section.m_preset_effect_settings)
				{
					if(effect_setting.DrawGUISetting(m_animation_manager, gui_x_offset, ref gui_y_offset, GUI.changed, preset_anim_section.m_start_action, preset_anim_section.m_start_loop))
						setting_changed = true;
				}

				// Display Exit Pause setting
				gui_changed = GUI.changed;
				preset_anim_section.m_exit_pause = EditorGUI.Toggle(new Rect(gui_x_offset, gui_y_offset, 200, LINE_HEIGHT), "Exit Delay", preset_anim_section.m_exit_pause);
				gui_y_offset += LINE_HEIGHT;

				if(!gui_changed && GUI.changed)
				{
					preset_anim_section.SetExitPauseState(m_animation_manager, preset_anim_section.m_exit_pause);
				}

				if(preset_anim_section.m_exit_pause)
				{
					LetterAction exit_pause_action = m_animation_manager.m_master_animations[0].GetAction(preset_anim_section.ExitPauseIndex);

					// Force to be Constant progression type
					if(exit_pause_action.m_duration_progression.Progression != (int) ValueProgression.Constant)
						exit_pause_action.m_duration_progression.SetConstant(exit_pause_action.m_duration_progression.ValueFrom);

					gui_changed = GUI.changed;

					float exit_pause_duration = Mathf.Max(EditorGUI.FloatField(new Rect(120, gui_y_offset, 200, LINE_HEIGHT), "Duration", exit_pause_action.m_duration_progression.ValueFrom), 0);
					gui_y_offset += LINE_HEIGHT;

					if(!gui_changed && GUI.changed)
					{
						exit_pause_action.m_duration_progression.SetConstant(exit_pause_duration);

						m_animation_manager.PrepareAnimationData(ANIMATION_DATA_TYPE.DURATION);

						setting_changed = true;
					}
				}


				if(setting_changed)
				{
					m_animation_manager.PlayEditorAnimation(preset_anim_section.m_start_action);
				}
			}

			gui_y_offset += LINE_HEIGHT;
		}

		int GetSectionRepeatLoopIndex(TextFxAnimationManager.PRESET_ANIMATION_SECTION section)
		{
			if(section == TextFxAnimationManager.PRESET_ANIMATION_SECTION.INTRO)
				return m_animation_manager.IntroRepeatLoopStartIndex;
			else if(section == TextFxAnimationManager.PRESET_ANIMATION_SECTION.MAIN)
				return m_animation_manager.MainRepeatLoopStartIndex;
			else if(section == TextFxAnimationManager.PRESET_ANIMATION_SECTION.OUTRO)
				return m_animation_manager.OutroRepeatLoopStartIndex;

			return 0;
		}


		void DrawFullEditorPanel(float y_offset)
		{
			float gui_y_offset = y_offset;
			float main_editor_width = Instance.position.width - MainEditorX - WINDOW_BORDER_RIGHT;
			float main_editor_height = Instance.position.height - WINDOW_BORDER_TOP - WINDOW_BORDER_BOTTOM - HEIGHT_OF_DEFAULT_SETTINGS_GUI_PANEL;

			// Draw Loop Tree/Action Editor divider line
			TextFxHelperMethods.DrawGUILine(new Rect(MainEditorX - 10, gui_y_offset, 0, Instance.position.height - WINDOW_BORDER_TOP - WINDOW_BORDER_BOTTOM), Color.gray, 3);



			if(m_animation_manager.LetterAnimations == null)
			{
				return;
			}

			

			
			IgnoreChanges();
			

//			if(m_animation_manager.HasAudioParticleChildInstances)
//			{
//				if (GUI.Button(new Rect(MainEditorX, gui_y_offset, 200, 20), "Clear Audio/Particle Instances"))
//				{
//					m_animation_manager.ClearCachedAudioParticleInstances();
//				}
//
//				gui_y_offset += LINE_HEIGHT;
//				gui_y_offset += LINE_HEIGHT;
//			}


			gui_y_offset += 10;

			m_animation_manager.m_begin_on_start = EditorGUI.Toggle(new Rect(MainEditorX, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Play On Start?", "Should this text animation play automatically when the object is first active in the scene?"), m_animation_manager.m_begin_on_start);
			
			if(m_animation_manager.m_begin_on_start)
			{
				gui_y_offset += LINE_HEIGHT;
				m_animation_manager.m_begin_delay = EditorGUI.FloatField(new Rect(MainEditorX + 20, gui_y_offset, ENUM_SELECTOR_WIDTH, INPUT_FIELD_HEIGHT), new GUIContent("Delay", "How much of a delay should there be before automatically playing this animation?"), m_animation_manager.m_begin_delay);
			}
			gui_y_offset += LINE_HEIGHT;

			m_animation_manager.m_time_type = (AnimationTime) EditorGUI.EnumPopup(new Rect(MainEditorX, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Time", "Sets whether the animation will use in-game time, or real world time. There's only a difference if Time.timescale != 1."), m_animation_manager.m_time_type);
			gui_y_offset += LINE_HEIGHT;

			m_animation_manager.m_animation_speed_factor = EditorGUI.FloatField (new Rect (MainEditorX, gui_y_offset, ENUM_SELECTOR_WIDTH, INPUT_FIELD_HEIGHT), "Speed Factor", m_animation_manager.m_animation_speed_factor);
			gui_y_offset += LINE_HEIGHT;

			m_animation_manager.m_on_finish_action = (ON_FINISH_ACTION) EditorGUI.EnumPopup (new Rect (MainEditorX, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), "On Finish", m_animation_manager.m_on_finish_action);
			gui_y_offset += LINE_HEIGHT;

			m_animation_manager.m_animate_per = (AnimatePerOptions) EditorGUI.EnumPopup(new Rect(MainEditorX, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Animate Per", "Sets whether to calculate state values on a per letter, per word or per line basis."), m_animation_manager.m_animate_per);
			gui_y_offset += LINE_HEIGHT;

			gui_y_offset += LINE_HEIGHT;
			
			
			if(GUI.Button(new Rect(MainEditorX, gui_y_offset, 140, LINE_HEIGHT), new GUIContent("Add Animation")) && m_animation_manager.WipeQuickSetupData(user_confirm: true))
			{
				m_animation_manager.AddAnimation();
				
				return;
			}
			if(GUI.Button(new Rect(MainEditorX + 150, gui_y_offset, 210, LINE_HEIGHT), new GUIContent("Delete Selected Animation")) && m_animation_manager.WipeQuickSetupData(user_confirm: true))
			{
				m_animation_manager.RemoveAnimation(ANIMATION_IDX);
				
				if(ANIMATION_IDX >= m_animation_manager.NumAnimations)
				{
					ANIMATION_IDX = m_animation_manager.NumAnimations - 1;
				}
				
				return;
			}
			gui_y_offset += LINE_HEIGHT + 5;
			
			// Check if any animations exist to draw
			if(m_animation_manager.NumAnimations == 0)
			{
				return;
			}
			
			int num_actions;
			LetterAnimation selected_animation;
			
			// Draw animation selection toolbar
			string[] animation_labels = new string[m_animation_manager.NumAnimations];
			for(int anim_idx=0; anim_idx < animation_labels.Length; anim_idx++)
			{
				animation_labels[anim_idx] = "Anim " + (anim_idx + 1);
			}
			ANIMATION_IDX = GUI.Toolbar(new Rect(MainEditorX, gui_y_offset, main_editor_width, LINE_HEIGHT), ANIMATION_IDX, animation_labels);
			gui_y_offset += LINE_HEIGHT * 1.5f;
			
			
			ANIMATION_IDX = Mathf.Clamp(ANIMATION_IDX, 0, m_animation_manager.NumAnimations - 1);
			
			selected_animation = m_animation_manager.GetAnimation(ANIMATION_IDX);
			num_actions = selected_animation.NumActions;
			
			selected_animation.m_letters_to_animate_option = (LETTERS_TO_ANIMATE) EditorGUI.EnumPopup(new Rect(MainEditorX, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Animate On", "Specifies which letters in the text this animation will affect."), selected_animation.m_letters_to_animate_option);
			
			if(selected_animation.m_letters_to_animate_option == LETTERS_TO_ANIMATE.CUSTOM)
			{
				if (selected_animation.m_letters_to_animate == null)
					selected_animation.m_letters_to_animate = new List<int>();

				gui_y_offset += LINE_HEIGHT;
				
				CUSTOM_LETTERS_LIST_POS = GUI.BeginScrollView(new Rect(MainEditorX,gui_y_offset,main_editor_width,LINE_HEIGHT*3), CUSTOM_LETTERS_LIST_POS,  new Rect(0,0, m_animation_manager.CurrentText.Length * 20,40));

				int idx = 0;
				float spacing_offset = 0;
				foreach(char character in m_animation_manager.CurrentText)
				{
					if(character.Equals(' ') || character.Equals('\n'))
					{
						if (selected_animation.m_letters_to_animate.Contains (idx))
							selected_animation.m_letters_to_animate.Remove (idx);

						idx++;

						continue;
					}
					GUI.Label(new Rect(idx*20 + spacing_offset,0,20,50), "" + character);
					bool toggle_state = false;
					if(selected_animation.m_letters_to_animate.Contains(idx))
					{
						toggle_state = true;
					}
					
					if(GUI.Toggle(new Rect(idx*20 + spacing_offset,20,20,50), toggle_state, "") != toggle_state)
					{
						if(toggle_state)
						{
							// Letter removed from list
							selected_animation.m_letters_to_animate.Remove(idx);
						}
						else
						{
							// Adding letter to list
							selected_animation.m_letters_to_animate.Add(idx);
						}
					}
					idx++;
				}
				
				selected_animation.m_letters_to_animate.Sort();
				
				GUI.EndScrollView();
				
				gui_y_offset += LINE_HEIGHT * 2;
				
				main_editor_height -= LINE_HEIGHT * 3;
			}
			else if(selected_animation.m_letters_to_animate_option == LETTERS_TO_ANIMATE.NTH_WORD || selected_animation.m_letters_to_animate_option == LETTERS_TO_ANIMATE.NTH_LINE)
			{
				gui_y_offset += LINE_HEIGHT;
				selected_animation.m_letters_to_animate_custom_idx = EditorGUI.IntField(
					new Rect(MainEditorX + 10,gui_y_offset,main_editor_width - 10,LINE_HEIGHT),
					new GUIContent((selected_animation.m_letters_to_animate_option == LETTERS_TO_ANIMATE.NTH_WORD ? "Word" : "Line") + " Number"),
					selected_animation.m_letters_to_animate_custom_idx);
				
				main_editor_height -= LINE_HEIGHT * 1;
			}
			
			gui_y_offset += TOOLBAR_LETTER_HEIGHT;
			
			CheckGUIChange (0, true, ANIMATION_DATA_TYPE.ALL); //.ANIMATE_ON);
			
			if(GUI.Button(new Rect(MainEditorX, gui_y_offset, 110, LINE_HEIGHT), new GUIContent("Add Action", "Add a new action state to this animation.")) && m_animation_manager.WipeQuickSetupData(user_confirm: true))
			{
				if(m_animation_manager.Playing)
					m_animation_manager.ResetAnimation();

				if(selected_animation.NumActions > 0)
				{
					selected_animation.AddAction(selected_animation.GetAction(selected_animation.NumActions-1).ContinueActionFromThis());
				}
				else
				{
					selected_animation.AddAction();
				}
				
				m_animation_manager.PrepareAnimationData();
				
				return;
			}
			gui_y_offset += LINE_HEIGHT + 5;



			MAIN_SCROLL_POS = GUI.BeginScrollView(new Rect(MainEditorX, gui_y_offset, main_editor_width + 5, Instance.position.height - gui_y_offset), MAIN_SCROLL_POS, new Rect(0,0, main_editor_width - 13, m_main_editor_panel_height + 10));
			
			gui_y_offset = 0;
			
			IgnoreChanges();
			
			LetterAction action;
			for(int action_idx=0; action_idx < selected_animation.NumActions; action_idx++)
			{
				action = selected_animation.GetAction(action_idx);
				
				action.FoldedInEditor = EditorGUI.Foldout(new Rect(0, gui_y_offset, 100, HEADER_HEIGHT), action.FoldedInEditor, "Action " + action_idx, true, FoldOutHeaderGUIStyle);
				
				if(action_idx > 0)
				{
					IgnoreChanges();

					EditorGUI.LabelField(new Rect(100, gui_y_offset, 100, LINE_HEIGHT), "Cont. Prev?");
					action.m_offset_from_last = EditorGUI.Toggle(new Rect(175, gui_y_offset, 18, LINE_HEIGHT), action.m_offset_from_last);

					if(CheckGUIChange(ANIMATION_DATA_TYPE.ALL) && !m_animation_manager.WipeQuickSetupData(user_confirm: true))
					{
						// Revert value back to what it was.
						action.m_offset_from_last = !action.m_offset_from_last;
					}
				}
				else
				{
					action.m_offset_from_last = false;
				}
				
				if(GUI.Button(new Rect(200, gui_y_offset, 50, LINE_HEIGHT), new GUIContent("Add", "Add a new action after this action."), EditorStyles.toolbarButton) && m_animation_manager.WipeQuickSetupData(user_confirm: true))
				{
					if(m_animation_manager.Playing)
						m_animation_manager.ResetAnimation();

					selected_animation.InsertAction(action_idx+1, action.ContinueActionFromThis());
					m_animation_manager.PrepareAnimationData();
					break;
				}
				if(GUI.Button(new Rect(250, gui_y_offset, 60, LINE_HEIGHT), new GUIContent("Delete", "Delete this action."), EditorStyles.toolbarButton) && m_animation_manager.WipeQuickSetupData(user_confirm: true))
				{
					if(m_animation_manager.Playing)
						m_animation_manager.ResetAnimation();

					selected_animation.RemoveAction(action_idx);
					m_animation_manager.PrepareAnimationData();
					break;
				}
				if(action_idx > 0 && GUI.Button(new Rect(320, gui_y_offset, 40, LINE_HEIGHT), new GUIContent("Up", "Move this action one position upwards."), EditorStyles.toolbarButton) && m_animation_manager.WipeQuickSetupData(user_confirm: true))
				{
					if(m_animation_manager.Playing)
						m_animation_manager.ResetAnimation();

					selected_animation.RemoveAction(action_idx);
					selected_animation.InsertAction(action_idx-1, action);
					m_animation_manager.PrepareAnimationData();
					break;
				}
				if(action_idx < num_actions - 1 && GUI.Button(new Rect(360, gui_y_offset, 40, LINE_HEIGHT), new GUIContent("Down", "Move this action one position downwards."), EditorStyles.toolbarButton) && m_animation_manager.WipeQuickSetupData(user_confirm: true))
				{
					if(m_animation_manager.Playing)
						m_animation_manager.ResetAnimation();

					selected_animation.RemoveAction(action_idx);
					selected_animation.InsertAction(action_idx+1, action);
					m_animation_manager.PrepareAnimationData();
					break;
				}
				
				IgnoreChanges();
				
				if(GUI.Button(new Rect(410, gui_y_offset, 55, LINE_HEIGHT), new GUIContent("Reset To", "Reset the state of the animation text to the start of this actions state."), EditorStyles.toolbarButton))
				{
					m_animation_manager.SetAnimationState(action_idx, 0);

					m_textfx_animation_interface.UpdateTextFxMesh( );
					return;
				}
	        	gui_y_offset += HEADER_HEIGHT;
				
				if(action.FoldedInEditor)
				{
					action.m_action_type = (ACTION_TYPE) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Action Type", "Denotes whether this action is for animating the text (ANIM_SEQUENCE), or for pausing the animation (BREAK)."), action.m_action_type);
					gui_y_offset += LINE_HEIGHT;
					IgnoreChanges();
					
					if(action.m_action_type == ACTION_TYPE.ANIM_SEQUENCE)
					{
						EditorGUI.LabelField(new Rect(ACTION_INDENT_LEVEL_1 + ENUM_SELECTOR_WIDTH + 15, gui_y_offset, 70, LINE_HEIGHT),  new GUIContent("From/To?", "Option to define a separate letter anchor for this actions Start/End states."));
						action.m_letter_anchor_2_way = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1 + ENUM_SELECTOR_WIDTH + 75, gui_y_offset, 20, LINE_HEIGHT), action.m_letter_anchor_2_way);
						action.m_letter_anchor_start = (int) (TextfxTextAnchor) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent((action.m_letter_anchor_2_way ? "Start " : "") + "Letter Anchor", "Defines the anchor point on each letter which the rotation and scale state values are based around."), (TextfxTextAnchor) action.m_letter_anchor_start);
						gui_y_offset += LINE_HEIGHT;
						if(action.m_letter_anchor_2_way)
						{	
							action.m_letter_anchor_end = (int) (TextfxTextAnchor) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("End Letter Anchor", "Defines the anchor point on each letter which the rotation and scale state values are based around."), (TextfxTextAnchor) action.m_letter_anchor_end);
							gui_y_offset += LINE_HEIGHT;
						}
						CheckGUIChange(ANIMATION_DATA_TYPE.LETTER_ANCHOR);
						
						action.m_ease_type = (EasingEquation) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), new GUIContent("Ease Type", "Defines which easing function to use when lerping from the action start to end states."), action.m_ease_type);
						gui_y_offset += LINE_HEIGHT;
						IgnoreChanges();


						// Colour section
						DrawActionVariableSection(new GUIContent("Colour", "Define a start/end colour transition for the text during this Action."),
						                          main_editor_width,
						                          ref gui_y_offset,
						                          ref action.m_colour_section_foldout,
						                          ref action.m_colour_transition_active,
						                          action_idx == 0,
						                          () => { CheckGUIChange(action_idx, m_animation_manager.EditorActionProgress != 0, ANIMATION_DATA_TYPE.COLOUR, true); },
						                          (bool gui_enabled) => {

							GUI.enabled = gui_enabled;
							
							if(!action.m_offset_from_last)
							{
								gui_y_offset += action.m_start_colour.DrawEditorGUI(new GUIContent("Start Colour"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), true, true);
								CheckGUIChange(action_idx, true, ANIMATION_DATA_TYPE.COLOUR);
							}
							
							gui_y_offset += action.m_end_colour.DrawEditorGUI(new GUIContent("End Colour"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), true, true);
							CheckGUIChange(action_idx, false, ANIMATION_DATA_TYPE.COLOUR);
							
							GUI.enabled = true;

						});


						// Position section
						DrawActionVariableSection(new GUIContent("Position", "Define a start/end positional transition for the text during this Action."),
						                          main_editor_width,
						                          ref gui_y_offset,
						                          ref action.m_position_section_foldout,
						                          ref action.m_position_transition_active,
						                          action_idx == 0,
						                          () => { CheckGUIChange(action_idx, m_animation_manager.EditorActionProgress != 0, ANIMATION_DATA_TYPE.POSITION, true); },
						                          (bool gui_enabled) => {


							GUI.enabled = gui_enabled;
							gui_y_offset += DrawAxisEaseOverrideGUI(action.m_position_axis_ease_data, new GUIContent("Set Position Axis Ease?", "Allows you to override the action 'Ease Type' for each axis individually to create a more unique movement."), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0));
							
							if(!action.m_offset_from_last)
							{
								gui_y_offset += action.m_start_pos.DrawPositionEditorGUI(new GUIContent("Start Position"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), action_idx > 0, true);
									if(CheckGUIChange(action_idx, true))
									{
										m_animation_manager.PrepareAnimationData(ANIMATION_DATA_TYPE.POSITION);
									}
							}
							gui_y_offset += action.m_end_pos.DrawPositionEditorGUI(new GUIContent("End Position"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), true, true);
							CheckGUIChange(action_idx, false, ANIMATION_DATA_TYPE.POSITION);
							
							GUI.enabled = true;
						});


						// Local Rotation section
						DrawActionVariableSection(new GUIContent("Letter Rotation", "Define a start/end rotational transition for each letter about their local anchor points during this Action."),
						                          main_editor_width,
						                          ref gui_y_offset,
						                          ref action.m_local_rotation_section_foldout,
						                          ref action.m_local_rotation_transition_active,
						                          action_idx == 0,
						                          () => { CheckGUIChange(action_idx, m_animation_manager.EditorActionProgress != 0, ANIMATION_DATA_TYPE.LOCAL_ROTATION, true); },
						                          (bool gui_enabled) => {

							GUI.enabled = gui_enabled;

							gui_y_offset += DrawAxisEaseOverrideGUI(action.m_rotation_axis_ease_data, new GUIContent("Set Rotation Axis Ease?", "Allows you to override the action 'Ease Type' for each axis individually to create a more unique rotation."), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0));
							
							if(!action.m_offset_from_last)
							{
								gui_y_offset += action.m_start_euler_rotation.DrawEditorGUI( new GUIContent("Start Euler Rotation"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), action_idx > 0, true);
								CheckGUIChange(action_idx, true, ANIMATION_DATA_TYPE.LOCAL_ROTATION);
							}
							gui_y_offset += action.m_end_euler_rotation.DrawEditorGUI(new GUIContent("End Euler Rotation"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), true, true);
							CheckGUIChange(action_idx, false, ANIMATION_DATA_TYPE.LOCAL_ROTATION);

							GUI.enabled = true;
						});


						// Local Scale section
						DrawActionVariableSection(new GUIContent("Letter Scale", "Define a start/end scaling transition for each letter about their local anchor points during this Action."),
						                          main_editor_width,
						                          ref gui_y_offset,
						                          ref action.m_local_scale_section_foldout,
						                          ref action.m_local_scale_transition_active,
						                          action_idx == 0,
						                          () => { CheckGUIChange(action_idx, m_animation_manager.EditorActionProgress != 0, ANIMATION_DATA_TYPE.LOCAL_SCALE, true); },
						                          (bool gui_enabled) => {

							GUI.enabled = gui_enabled;
							gui_y_offset += DrawAxisEaseOverrideGUI(action.m_scale_axis_ease_data, new GUIContent("Set Scale Axis Ease?", "Allows you to override the action 'Ease Type' for each axis individually to create a more unique scaling."), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0));
							
							if(!action.m_offset_from_last)
							{
								gui_y_offset += action.m_start_scale.DrawEditorGUI( new GUIContent("Start Scale"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), action_idx > 0, true);
								CheckGUIChange(action_idx, true, ANIMATION_DATA_TYPE.LOCAL_SCALE);
							}
							gui_y_offset += action.m_end_scale.DrawEditorGUI( new GUIContent("End Scale"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), true, true);
							CheckGUIChange(action_idx, false, ANIMATION_DATA_TYPE.LOCAL_SCALE);

							GUI.enabled = true;
						});


						// Global Rotation section
						DrawActionVariableSection(new GUIContent("Text Rotation", "Define a start/end rotational transition for the text as a whole, about the objects pivot point, during this Action."),
						                          main_editor_width,
						                          ref gui_y_offset,
						                          ref action.m_global_rotation_section_foldout,
						                          ref action.m_global_rotation_transition_active,
						                          action_idx == 0,
						                          () => { CheckGUIChange(action_idx, m_animation_manager.EditorActionProgress != 0, ANIMATION_DATA_TYPE.GLOBAL_ROTATION, true); },
						(bool gui_enabled) => {
							
							GUI.enabled = gui_enabled;
							gui_y_offset += DrawAxisEaseOverrideGUI(action.m_global_rotation_axis_ease_data, new GUIContent("Set Rotation Axis Ease?", "Allows you to override the action 'Ease Type' for each axis individually to create a more unique rotation."), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0));
							
							if(!action.m_offset_from_last)
							{
								gui_y_offset += action.m_global_start_euler_rotation.DrawEditorGUI( new GUIContent("Start Euler Rotation"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), action_idx > 0, true);
								CheckGUIChange(action_idx, true, ANIMATION_DATA_TYPE.GLOBAL_ROTATION);
							}
							gui_y_offset += action.m_global_end_euler_rotation.DrawEditorGUI(new GUIContent("End Euler Rotation"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), true, true);
							CheckGUIChange(action_idx, false, ANIMATION_DATA_TYPE.GLOBAL_ROTATION);
							
							GUI.enabled = true;
						});

						// Global Scale section
						DrawActionVariableSection(new GUIContent("Text Scale", "Define a start/end scaling transition for the text as a whole during this Action."),
						                          main_editor_width,
						                          ref gui_y_offset,
						                          ref action.m_global_scale_section_foldout,
						                          ref action.m_global_scale_transition_active,
						                          action_idx == 0,
						                          () => { CheckGUIChange(action_idx, m_animation_manager.EditorActionProgress != 0, ANIMATION_DATA_TYPE.GLOBAL_SCALE, true); },
						                          (bool gui_enabled) => {

							GUI.enabled = gui_enabled;

							gui_y_offset += DrawAxisEaseOverrideGUI(action.m_global_scale_axis_ease_data, new GUIContent("Set Scale Axis Ease?", "Allows you to override the action 'Ease Type' for each axis individually to create a more unique scaling."), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0));
							
							if(!action.m_offset_from_last)
							{
								gui_y_offset += action.m_global_start_scale.DrawEditorGUI( new GUIContent("Start Scale"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), action_idx > 0, true);
								CheckGUIChange(action_idx, true, ANIMATION_DATA_TYPE.GLOBAL_SCALE);
							}
							gui_y_offset += action.m_global_end_scale.DrawEditorGUI( new GUIContent("End Scale"), new Rect(ACTION_INDENT_LEVEL_2, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_2, 0), true, true);
							CheckGUIChange(action_idx, false, ANIMATION_DATA_TYPE.GLOBAL_SCALE);

							GUI.enabled = true;
						});

						gui_y_offset += 8;

						action.m_force_same_start_time = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, LINE_HEIGHT), new GUIContent("Force Same Start?", "Forces all letters in this animation to start animating this action at the same time."), action.m_force_same_start_time);
						gui_y_offset += LINE_HEIGHT;

						if(selected_animation.m_letters_to_animate_option == LETTERS_TO_ANIMATE.ALL_LETTERS && (((ValueProgression) action.m_delay_progression.Progression) == ValueProgression.Eased || ((ValueProgression) action.m_delay_progression.Progression) == ValueProgression.EasedCustom))
						{
							action.m_delay_with_white_space_influence = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, LINE_HEIGHT), new GUIContent("White Space Delays?", "Whether or not to take white space into account in letter delay calculations."), action.m_delay_with_white_space_influence);
							gui_y_offset += LINE_HEIGHT;
						}
						
						gui_y_offset += action.m_delay_progression.DrawEditorGUI(new GUIContent("Delay"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true);
						if(CheckGUIChange(-1, true))
						{
							m_animation_manager.PrepareAnimationData(ANIMATION_DATA_TYPE.DELAY);
						}
					}
					
					gui_y_offset += action.m_duration_progression.DrawEditorGUI(new GUIContent("Duration"), new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true);
					if(CheckGUIChange(-1,true))
					{
						m_animation_manager.PrepareAnimationData(ANIMATION_DATA_TYPE.DURATION);
					}
					
					// Give a little bit of extra spacing before audio/particle settings
					gui_y_offset += 5;
					
					
					
					action.AudioEffectsEditorDisplay = EditorGUI.Foldout(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, 150, LINE_HEIGHT), action.AudioEffectsEditorDisplay, "", true);
					EditorGUI.LabelField(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, 150, LINE_HEIGHT), "Audio Effects " + (action.NumAudioEffectSetups == 0 ? "" : "[" + action.NumAudioEffectSetups + "]"), EditorStyles.boldLabel);
					gui_y_offset += LINE_HEIGHT;
					
					if(action.AudioEffectsEditorDisplay)
					{
						if(GUI.Button(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, 90, LINE_HEIGHT), new GUIContent("Add New", "Add a new particle effect setup for this animation action.")))
						{
							action.AddAudioEffectSetup();
						}
						gui_y_offset += LINE_HEIGHT;
						
						AudioEffectSetup audio_setup;
						for(int idx=0; idx < action.NumAudioEffectSetups; idx++)
						{
							audio_setup = action.GetAudioEffectSetup(idx);
							
							audio_setup.m_editor_display = EditorGUI.Foldout(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, 150, LINE_HEIGHT), audio_setup.m_editor_display, "", true);
							EditorGUI.LabelField(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, 150, LINE_HEIGHT), "Effect " + (idx+1), EditorStyles.boldLabel);
							if(GUI.Button(new Rect(ACTION_INDENT_LEVEL_1 + 220, gui_y_offset + 2, 30, LINE_HEIGHT - 4), new GUIContent("X", "Remove particle effect setup.")))
							{
								action.RemoveAudioEffectSetup(idx);
								idx--;
								continue;
							}
							gui_y_offset += LINE_HEIGHT;
								
							if(audio_setup.m_editor_display)
							{
								audio_setup.m_audio_clip = (AudioClip) EditorGUI.ObjectField(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 40, LINE_HEIGHT), new GUIContent("Audio Clip", ""), audio_setup.m_audio_clip, typeof(AudioClip), true);
								gui_y_offset += LINE_HEIGHT;
								
								audio_setup.m_play_when = (PLAY_ITEM_EVENTS) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 40, LINE_HEIGHT), new GUIContent("Play When?"), audio_setup.m_play_when);
								gui_y_offset += LINE_HEIGHT;
								
								audio_setup.m_effect_assignment = (PLAY_ITEM_ASSIGNMENT) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 40, LINE_HEIGHT), new GUIContent("Effect Assignment"), audio_setup.m_effect_assignment);
								gui_y_offset += LINE_HEIGHT;
								
								if(audio_setup.m_effect_assignment == PLAY_ITEM_ASSIGNMENT.CUSTOM)
								{
									audio_setup.CUSTOM_LETTERS_LIST_POS = GUI.BeginScrollView(new Rect(MainEditorX,gui_y_offset,main_editor_width,LINE_HEIGHT*3), audio_setup.CUSTOM_LETTERS_LIST_POS,  new Rect(0,0,m_animation_manager.CurrentText.Length * 20,40));
									
									if(audio_setup.m_effect_assignment_custom_letters == null)
										audio_setup.m_effect_assignment_custom_letters = new System.Collections.Generic.List<int>();
									
									int letter_idx = 0;
									float spacing_offset = 0;
									bool toggle_state;
									foreach(char character in m_animation_manager.CurrentText)
									{
										if(character.Equals(' ') || character.Equals('\n'))
										{
											spacing_offset += 20;
											continue;
										}
										GUI.Label(new Rect(letter_idx*20 + spacing_offset,0,20,50), "" + character);
										toggle_state = false;
										
										if(audio_setup.m_effect_assignment_custom_letters.Contains(letter_idx))
										{
											toggle_state = true;
										}
										
										if(GUI.Toggle(new Rect(letter_idx*20 + spacing_offset,20,20,20), toggle_state, "") != toggle_state)
										{
											if(toggle_state)
											{
												// Letter removed from list
												audio_setup.m_effect_assignment_custom_letters.Remove(letter_idx);
											}
											else
											{
												// Adding letter to list
												audio_setup.m_effect_assignment_custom_letters.Add(letter_idx);
											}
										}
										letter_idx++;
									}
									
									audio_setup.m_effect_assignment_custom_letters.Sort();
									
									GUI.EndScrollView();
									
									gui_y_offset += LINE_HEIGHT * 2;
								}
								
								gui_y_offset += audio_setup.m_delay.DrawEditorGUI(new GUIContent("Delay", "How much this particle effect should be delayed from the end of the action."), new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
								
								gui_y_offset += audio_setup.m_offset_time.DrawEditorGUI(new GUIContent("Offset Time", "How far into the audio clip should it start playing."), new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
								
								gui_y_offset += audio_setup.m_volume.DrawEditorGUI(new GUIContent("Volume", "What volume should the audio clip be played at."), new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
								
								gui_y_offset += audio_setup.m_pitch.DrawEditorGUI(new GUIContent("Pitch", "What pitch should the audio clip be played at."), new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
								
								audio_setup.m_loop_play_once = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 20, LINE_HEIGHT), new GUIContent("Loop Play Once?", "Should the particle effect only play for the first interation of loop?"), audio_setup.m_loop_play_once);
								gui_y_offset += LINE_HEIGHT;
							}
						}
					}
					
					
					action.ParticleEffectsEditorDisplay = EditorGUI.Foldout(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, 150, LINE_HEIGHT), action.ParticleEffectsEditorDisplay, "", true);
					EditorGUI.LabelField(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, 150, LINE_HEIGHT), "Particle Effects " + (action.NumParticleEffectSetups == 0 ? "" : "[" + action.NumParticleEffectSetups + "]"), EditorStyles.boldLabel);
					gui_y_offset += LINE_HEIGHT;
					
					if(action.ParticleEffectsEditorDisplay)
					{
						if(GUI.Button(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, 90, LINE_HEIGHT), new GUIContent("Add New", "Add a new particle effect setup for this animation action.")))
						{
							action.AddParticleEffectSetup();
						}
						gui_y_offset += LINE_HEIGHT;
						
						ParticleEffectSetup particle_setup;
						for(int idx=0; idx < action.NumParticleEffectSetups; idx++)
						{
							particle_setup = action.GetParticleEffectSetup(idx);
							
							particle_setup.m_editor_display = EditorGUI.Foldout(new Rect(ACTION_INDENT_LEVEL_1 + 10, gui_y_offset, 150, LINE_HEIGHT), particle_setup.m_editor_display, "", true);
							EditorGUI.LabelField(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, 150, LINE_HEIGHT), "Effect " + (idx+1), EditorStyles.boldLabel);
							if(GUI.Button(new Rect(ACTION_INDENT_LEVEL_1 + 220, gui_y_offset + 2, 30, LINE_HEIGHT - 4), new GUIContent("X", "Remove particle effect setup.")))
							{
								action.RemoveParticleEffectSetup(idx);
								idx--;
								continue;
							}
							gui_y_offset += LINE_HEIGHT;
								
							if(particle_setup.m_editor_display)
							{
								particle_setup.m_effect_type = (PARTICLE_EFFECT_TYPE) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 40, LINE_HEIGHT), new GUIContent("Effect Type"), particle_setup.m_effect_type);
								gui_y_offset += LINE_HEIGHT;

#if !UNITY_5_4_OR_NEWER
								if(particle_setup.m_effect_type == PARTICLE_EFFECT_TYPE.LEGACY)
								{
									particle_setup.m_legacy_particle_effect = (ParticleEmitter) EditorGUI.ObjectField(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 40, LINE_HEIGHT), new GUIContent("ParticleEmitter Prefab", ""), particle_setup.m_legacy_particle_effect, typeof(ParticleEmitter), true);
									particle_setup.m_shuriken_particle_effect = null;
								}
								else
#endif
								{
									particle_setup.m_shuriken_particle_effect = (ParticleSystem) EditorGUI.ObjectField(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 40, LINE_HEIGHT), new GUIContent("ParticleSystem Prefab", ""), particle_setup.m_shuriken_particle_effect, typeof(ParticleSystem), true);
#if !UNITY_5_4_OR_NEWER
									particle_setup.m_legacy_particle_effect = null;
#endif
								}
								gui_y_offset += LINE_HEIGHT;
								
								particle_setup.m_play_when = (PLAY_ITEM_EVENTS) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 40, LINE_HEIGHT), new GUIContent("Play When?"), particle_setup.m_play_when);
								gui_y_offset += LINE_HEIGHT;
								
								particle_setup.m_effect_assignment = (PLAY_ITEM_ASSIGNMENT) EditorGUI.EnumPopup(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 40, LINE_HEIGHT), new GUIContent("Effect Assignment"), particle_setup.m_effect_assignment);
								gui_y_offset += LINE_HEIGHT;
								
								if(particle_setup.m_effect_assignment == PLAY_ITEM_ASSIGNMENT.CUSTOM)
								{
									if(particle_setup.m_effect_assignment_custom_letters == null)
										particle_setup.m_effect_assignment_custom_letters = new System.Collections.Generic.List<int>();
									
									particle_setup.CUSTOM_LETTERS_LIST_POS = GUI.BeginScrollView(new Rect(MainEditorX,gui_y_offset,main_editor_width,LINE_HEIGHT*3), particle_setup.CUSTOM_LETTERS_LIST_POS,  new Rect(0,0,m_animation_manager.CurrentText.Length * 20,40));
									
									int letter_idx = 0;
									float spacing_offset = 0;
									bool toggle_state;
									foreach(char character in m_animation_manager.CurrentText)
									{
										if(character.Equals(' ') || character.Equals('\n'))
										{
											spacing_offset += 20;
											continue;
										}
										GUI.Label(new Rect(letter_idx*20 + spacing_offset,0,20,50), "" + character);
										toggle_state = false;
										if(particle_setup.m_effect_assignment_custom_letters.Contains(letter_idx))
										{
											toggle_state = true;
										}
										
										if(GUI.Toggle(new Rect(letter_idx*20 + spacing_offset,20,20,20), toggle_state, "") != toggle_state)
										{
											if(toggle_state)
											{
												// Letter removed from list
												particle_setup.m_effect_assignment_custom_letters.Remove(letter_idx);
											}
											else
											{
												// Adding letter to list
												particle_setup.m_effect_assignment_custom_letters.Add(letter_idx);
											}
										}
										letter_idx++;
									}
									
									particle_setup.m_effect_assignment_custom_letters.Sort();
									
									GUI.EndScrollView();
									
									gui_y_offset += LINE_HEIGHT * 2;
								}
								
								gui_y_offset += particle_setup.m_delay.DrawEditorGUI(new GUIContent("Delay", "How much this particle effect should be delayed from the end of the action."), new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
								
								EditorGUI.HelpBox(
									new Rect(ACTION_INDENT_LEVEL_1 + 340, gui_y_offset + LINE_HEIGHT, particle_setup.m_effect_type == PARTICLE_EFFECT_TYPE.LEGACY ? 150 : 230, LINE_HEIGHT),
									particle_setup.m_effect_type == PARTICLE_EFFECT_TYPE.LEGACY ? "zero == \"One Shot\"" : "zero == non-looping single playthrough",
									MessageType.Info);
								gui_y_offset += particle_setup.m_duration.DrawEditorGUI(new GUIContent("Duration", "How long this particle effect should be played for."), new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
								
								particle_setup.m_follow_mesh = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 20, LINE_HEIGHT), new GUIContent("Follow Mesh?", "Should the particle effect move and rotate with the letter mesh its assigned to?"), particle_setup.m_follow_mesh);
								gui_y_offset += LINE_HEIGHT;
								
								particle_setup.m_loop_play_once = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 20, LINE_HEIGHT), new GUIContent("Loop Play Once?", "Should the particle effect only play for the first interation of loop?"), particle_setup.m_loop_play_once);
								gui_y_offset += LINE_HEIGHT;
								
								gui_y_offset += particle_setup.m_position_offset.DrawEditorGUI(new GUIContent("Position Offset", "The positional offset of the particle effect from the mid-point of the letter mesh."), new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
								gui_y_offset += particle_setup.m_rotation_offset.DrawEditorGUI(new GUIContent("Rotation Offset", "The rotation offset of the particle effect."), new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, 0), false, true, false);
								
								particle_setup.m_rotate_relative_to_letter = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1 - 20, LINE_HEIGHT), new GUIContent("Rotate With Letter?", "Should the particle effect be rotated relative to the letters rotation?"), particle_setup.m_rotate_relative_to_letter);
								gui_y_offset += LINE_HEIGHT;
							}
						}
					}
					
					if(CheckGUIChange(-1,true))
					{
						m_animation_manager.PrepareAnimationData();
					}
					
					// Display help box
					if(action.m_action_type == ACTION_TYPE.BREAK)
					{
						EditorGUI.HelpBox(new Rect(ACTION_INDENT_LEVEL_1, gui_y_offset, main_editor_width - ACTION_INDENT_LEVEL_1, LINE_HEIGHT*2), "Enter a delay of zero for an infinite break.\nUse the Continue() function for progressing past an animation break.", MessageType.Info);
						gui_y_offset += LINE_HEIGHT * 2;
					}
					
					IgnoreChanges();
				}
			}
			
			GUI.EndScrollView();

			if(gui_y_offset != m_main_editor_panel_height)
			{
				m_main_editor_panel_height = gui_y_offset;
				Instance.Repaint();
			}

			DrawLoopTree(selected_animation);
		}

		void DrawActionVariableSection(GUIContent label, float section_width, ref float gui_y_offset, ref bool foldout, ref bool section_active, bool firstAction, System.Action activeStatusChangeCallback, System.Action<bool> foldoutCallback)
		{
			bool active_status_changed = false;

			GUI.color = section_active || firstAction ? new Color(0.3f, 0.7f, 0.3f) : Color.grey;
			bool gui_changed = GUI.changed;
			GUI.Button(new Rect(ACTION_INDENT_LEVEL_1 + (firstAction ? 0 : 20), gui_y_offset, section_width - ACTION_INDENT_LEVEL_1 - 20, LINE_HEIGHT), "");
			if(!gui_changed && GUI.changed)
				foldout = !foldout;
			GUI.color = Color.white;
			if(!firstAction)
			{
				gui_changed = GUI.changed;
				section_active = EditorGUI.Toggle(new Rect(ACTION_INDENT_LEVEL_1 + 3, gui_y_offset + 2, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), section_active);
				if(!gui_changed && GUI.changed)
					active_status_changed = true;
			}
			EditorGUI.LabelField( new Rect(ACTION_INDENT_LEVEL_1 + 20, gui_y_offset + 2, ENUM_SELECTOR_WIDTH, LINE_HEIGHT), label, EditorStyles.boldLabel);
			gui_y_offset += LINE_HEIGHT;
			
			IgnoreChanges();
			
			if(foldout)
				foldoutCallback(section_active || firstAction);


			if(active_status_changed)
			{
				activeStatusChangeCallback();
			}

		}

		bool CheckGUIChange(ANIMATION_DATA_TYPE edited_data_type = ANIMATION_DATA_TYPE.NONE, bool forceChange = false)
		{
			return CheckGUIChange(m_animation_manager.EditorActionIdx, m_animation_manager.EditorActionProgress == 0 ? true : false, edited_data_type, forceChange);
		}
		
		bool CheckGUIChange(int action_idx, bool start_state, ANIMATION_DATA_TYPE edited_data_type = ANIMATION_DATA_TYPE.NONE, bool forceChange = false)
		{
			if (forceChange || (!ignore_gui_change && !noticed_gui_change && GUI.changed))
			{
				if(forceChange)
					ignore_gui_change = false;

				noticed_gui_change = true;
				edited_action_idx = action_idx;
				editing_start_state = start_state;

				m_edited_data = edited_data_type;

				return true;
			}
			
			return false;
		}
		
		void IgnoreChanges()
		{
			if(!ignore_gui_change && !noticed_gui_change && GUI.changed)
			{
				ignore_gui_change = true;
			}
		}
		
		float DrawAxisEaseOverrideGUI(AxisEasingOverrideData axis_data, GUIContent label, Rect position)
		{
			axis_data.m_override_default = EditorGUI.Toggle(new Rect(position.x, position.y, 200, LINE_HEIGHT), label, axis_data.m_override_default);
			
			if(axis_data.m_override_default)
			{
				EditorGUI.LabelField(new Rect(position.x + 180, position.y, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), "x :");
				EditorGUI.LabelField(new Rect(position.x + 180, position.y + LINE_HEIGHT, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), "y :");
				EditorGUI.LabelField(new Rect(position.x + 180, position.y + LINE_HEIGHT * 2, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), "z :");
				axis_data.m_x_ease = (EasingEquation) EditorGUI.EnumPopup(new Rect(position.x + 200, position.y, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), axis_data.m_x_ease);
				axis_data.m_y_ease = (EasingEquation) EditorGUI.EnumPopup(new Rect(position.x + 200, position.y + LINE_HEIGHT, ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), axis_data.m_y_ease);
				axis_data.m_z_ease = (EasingEquation) EditorGUI.EnumPopup(new Rect(position.x + 200, position.y + (LINE_HEIGHT * 2), ENUM_SELECTOR_WIDTH_SMALL, LINE_HEIGHT), axis_data.m_z_ease);
				return LINE_HEIGHT * 3;
			}
			else
			{
				return LINE_HEIGHT;
			}
		}
	}
}