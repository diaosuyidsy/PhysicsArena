#if NGUI
using UnityEngine;
using UnityEditor;

/// <summary>
/// This script adds the NGUI menu options to the Unity Editor.
/// </summary>
	
namespace TextFx
{
	static public class MenuOptionsTextFxNGUI
	{
		public const string NEW_INSTANCE_NAME = "TextFx Label";

		// NGUI Stuff

		[MenuItem("NGUI/Create/TextFx Label", false, 6)]
		static public void AddLabel ()
		{
			GameObject go = NGUIEditorTools.SelectedRoot(true);
			
			if (go != null)
			{
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				Undo.RegisterSceneUndo("Add a Label");
#endif

				TextFxNGUI w = NGUITools.AddWidget<TextFxNGUI>(go);
				w.name = NEW_INSTANCE_NAME;
				w.ambigiousFont = NGUISettings.ambigiousFont;
				w.text = "New Label";
				w.pivot = NGUISettings.pivot;
				w.width = 120;
				w.height = Mathf.Max(20, NGUISettings.GetInt("NGUI Font Height", 16));
				w.fontStyle = NGUISettings.fontStyle;
				w.fontSize = NGUISettings.fontSize;
				w.applyGradient = true;
				w.gradientBottom = new Color(0.7f, 0.7f, 0.7f);
				w.AssumeNaturalSize();
	//				return w;

				if(w.bitmapFont == null && w.trueTypeFont == null)
					w.trueTypeFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;

				Selection.activeGameObject = w.gameObject;
			}
			else
			{
				Debug.Log("You must select a game object first.");
			}
		}
	}
}
#endif