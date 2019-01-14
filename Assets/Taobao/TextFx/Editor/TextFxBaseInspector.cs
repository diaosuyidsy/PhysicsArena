using UnityEngine;
using UnityEditor;
using System.Collections;

namespace TextFx
{
	public static class TextFxBaseInspector {

		static Texture2D m_play_button_texture,
		m_pause_button_texture,
		m_reset_button_texture,
		m_continue_button_texture,
		m_continue_past_loop_button_texture,
		m_inspector_section_bg_texture;

		static GUIStyle m_horizontalLayoutBoxStyle;
		static GUIStyle m_textFxInspectorBoxTitleStyle;
		static GUIStyle m_wrapButton;

		const float m_inspectorButtonDims = 25f;


		static void CheckAssetReferences(ScriptableObject objRef)
		{
			if (m_play_button_texture == null)
			{
				string baseInspectorClassBase = typeof(TextFxBaseInspector).ToString().Replace("TextFx.", "");
				string[] searchResults = AssetDatabase.FindAssets (baseInspectorClassBase);
				string m_textFxRootDirectoryPath = AssetDatabase.GUIDToAssetPath( searchResults [0]).Replace ("Editor/" + baseInspectorClassBase + ".cs", "");

				// Load editor icon textures
	#if UNITY_PRO_LICENSE
				m_play_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/PlayButton_pro.png", typeof(Texture2D)) as Texture2D;
				m_pause_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/PauseButton_pro.png", typeof(Texture2D)) as Texture2D;
				m_reset_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ResetButton_pro.png", typeof(Texture2D)) as Texture2D;
				m_continue_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ContinueButton_pro.png", typeof(Texture2D)) as Texture2D;
				m_continue_past_loop_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ContinueFromLoop_pro.png", typeof(Texture2D)) as Texture2D;
				m_inspector_section_bg_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/bg_texture_pro.png", typeof(Texture2D)) as Texture2D;
	#else

				m_play_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/PlayButton.png", typeof(Texture2D)) as Texture2D;
				m_pause_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/PauseButton.png", typeof(Texture2D)) as Texture2D;
				m_reset_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ResetButton.png", typeof(Texture2D)) as Texture2D;
				m_continue_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ContinueButton.png", typeof(Texture2D)) as Texture2D;
				m_continue_past_loop_button_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/ContinueFromLoop.png", typeof(Texture2D)) as Texture2D;
				m_inspector_section_bg_texture = AssetDatabase.LoadAssetAtPath(m_textFxRootDirectoryPath + "Editor/Icons/bg_texture.png", typeof(Texture2D)) as Texture2D;
	#endif


				// Setup custom GUIStyles
#if !UNITY_4_6 && !UNITY_4_7 && !UNITY_4
				m_horizontalLayoutBoxStyle = new GUIStyle(EditorStyles.helpBox);
#else
				m_horizontalLayoutBoxStyle = new GUIStyle(EditorStyles.textArea);
#endif
				m_horizontalLayoutBoxStyle.normal.background = m_inspector_section_bg_texture;

				m_textFxInspectorBoxTitleStyle = new GUIStyle(EditorStyles.boldLabel);
				m_textFxInspectorBoxTitleStyle.fontSize = 17;
				m_textFxInspectorBoxTitleStyle.padding.top = 2;


				m_wrapButton = new GUIStyle (EditorStyles.miniButton);
				m_wrapButton.wordWrap = true;
				m_wrapButton.fontSize = 11;
				m_wrapButton.padding.top = 0;
			}
		}


		public static void DrawTextFxInspectorSection(ScriptableObject objRef, TextFxAnimationManager animationManager, System.Action curveSettingChangeCallback)
		{
			// Check that required assets have been loaded
			CheckAssetReferences (objRef);



			EditorGUILayout.BeginHorizontal (m_horizontalLayoutBoxStyle, GUILayout.Height(35));

			Color cachedColour = GUI.contentColor;
#if UNITY_PRO_LICENSE
			GUI.contentColor = Color.black;
#endif
			GUILayout.Label ("  TextFx", m_textFxInspectorBoxTitleStyle, GUILayout.Width(90));
#if UNITY_PRO_LICENSE
			GUI.contentColor = cachedColour;
#endif

			EditorGUILayout.BeginHorizontal(GUILayout.Width(120));

			if (GUILayout.Button(!animationManager.PreviewingAnimationInEditor || animationManager.PreviewAnimationPaused ? m_play_button_texture : m_pause_button_texture, GUILayout.Width(m_inspectorButtonDims), GUILayout.Height(m_inspectorButtonDims)))
			{
				if(animationManager.PreviewingAnimationInEditor)
				{
					animationManager.PreviewAnimationPaused = !animationManager.PreviewAnimationPaused;
				}
				else
				{
					if (animationManager.m_master_animations == null || animationManager.m_master_animations.Count == 0)
					{
						// No animation defined yet. Open the animation editor window to point user in the right direction.
						Debug.LogWarning("You need to setup an animation before you can play anything...");
						TextEffectsManager.Init();
					}
					else
					{
						animationManager.PlayEditorAnimation();
					}
				}
			}

			if(GUILayout.Button(m_reset_button_texture, GUILayout.Width(m_inspectorButtonDims), GUILayout.Height(m_inspectorButtonDims)))
			{
				animationManager.ResetEditorAnimation();
			}

			if(animationManager.Playing)
			{
				// Continue Past Break Button
				if(GUILayout.Button(m_continue_button_texture, GUILayout.Width(m_inspectorButtonDims), GUILayout.Height(m_inspectorButtonDims)))
				{
					animationManager.ContinuePastBreak();
				}

				// Continue Past Loop Button
				if(GUILayout.Button(m_continue_past_loop_button_texture, GUILayout.Width(m_inspectorButtonDims), GUILayout.Height(m_inspectorButtonDims)))
				{
					animationManager.ContinuePastLoop();
				}
			}

			EditorGUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

			GUI.contentColor = cachedColour;

#if UNITY_4_6 || UNITY_4_7
			if (GUILayout.Button("Open Animation Editor", m_wrapButton, GUILayout.MaxWidth(150), GUILayout.MinWidth(100), GUILayout.Height(29)))
#else
			if (GUILayout.Button("Open Animation Editor", m_wrapButton, GUILayout.MaxWidth(150), GUILayout.MinWidth(100), GUILayout.Height(32)))
#endif
			{
				TextEffectsManager.Init();
			}

			EditorGUILayout.EndHorizontal ();

			GUILayout.Space(10);

			if (animationManager.AnimationInterface != null && animationManager.AnimationInterface.CurvePositioningEnabled)
			{
				bool guiChanged = GUI.changed;
				
				animationManager.AnimationInterface.RenderToCurve = EditorGUILayout.Toggle ("Render To Curve?", animationManager.AnimationInterface.RenderToCurve);
				
				if (animationManager.AnimationInterface.RenderToCurve)
				{
					animationManager.AnimationInterface.DrawBezierInspector();
				}
				
				if (!guiChanged && GUI.changed && curveSettingChangeCallback != null)
					curveSettingChangeCallback ();
				
				GUILayout.Space(10);
			}
		}

		public static void UpdateManager(TextFxAnimationManager animationManager)
		{
			if(animationManager.PreviewingAnimationInEditor && !animationManager.PreviewAnimationPaused)
			{
				if(animationManager == null)
				{
					animationManager.PreviewingAnimationInEditor = false;
					return;
				}

				if(!animationManager.UpdateAnimation() && animationManager.ParticleEffectManagers.Count == 0)
				{
					animationManager.PreviewingAnimationInEditor = false;
				}

				animationManager.AnimationInterface.UpdateTextFxMesh();

				if(animationManager.ParticleEffectManagers.Count > 0)
					SceneView.RepaintAll();
			}
		}
	}
}