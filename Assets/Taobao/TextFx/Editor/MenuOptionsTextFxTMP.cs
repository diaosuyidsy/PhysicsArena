#if TMP

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5 && UNITY_EDITOR

#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1

#define UGUI_VERSION_1

#endif

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// This script adds the UI menu options to the Unity Editor.
/// </summary>
	
namespace TextFx
{
	public static class MenuOptionsTextFxTMP
	{
		public const string NEW_INSTANCE_NAME_TMP = "TextMeshPro TextFx";
		public const string NEW_INSTANCE_NAME_TMP_UGUI = "TextMeshPro UI TextFx";

		/// <summary>
		/// Create a TextMeshPro object that works with the CanvasRenderer
		/// </summary>
		/// <param name="command"></param>
#if UGUI_VERSION_1
		[MenuItem("GameObject/UI/TextMeshPro - TextFx", false, 2003)]
#else
		[MenuItem("GameObject/UI/TextMeshPro - TextFx", false, 2001)]
#endif
		public static void CreateTextMeshProGuiObjectPerform(MenuCommand command)
		{
			// Check if there is a Canvas in the scene
			Canvas canvas = Object.FindObjectOfType<Canvas>();
			if (canvas == null)
			{
				// Create new Canvas since none exists in the scene.
				GameObject canvasObject = new GameObject("Canvas");
				canvas = canvasObject.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;

				// Add a Graphic Raycaster Component as well
				canvas.gameObject.AddComponent<GraphicRaycaster>();

				Undo.RegisterCreatedObjectUndo(canvasObject, "Create " + canvasObject.name);
			}


			// Create the TextFxTextMeshProUGUI Object
			GameObject go = new GameObject(NEW_INSTANCE_NAME_TMP_UGUI);
			RectTransform goRectTransform = go.AddComponent<RectTransform>();

			Undo.RegisterCreatedObjectUndo((Object)go, "Create " + go.name);

			// Check if object is being create with left or right click
			GameObject contextObject = command != null ? command.context as GameObject : null;
			if (contextObject == null)
			{
				//goRectTransform.sizeDelta = new Vector2(200f, 50f);
				GameObjectUtility.SetParentAndAlign(go, canvas.gameObject);

				TextFxTextMeshProUGUI textMeshPro = go.AddComponent<TextFxTextMeshProUGUI>();
				textMeshPro.text = "New Text";
				textMeshPro.alignment = TextAlignmentOptions.TopLeft;
			}
			else
			{
				if (contextObject.GetComponent<Button>() != null)
				{
					goRectTransform.sizeDelta = Vector2.zero;
					goRectTransform.anchorMin = Vector2.zero;
					goRectTransform.anchorMax = Vector2.one;

					GameObjectUtility.SetParentAndAlign(go, contextObject);

					TextFxTextMeshProUGUI textMeshPro = go.AddComponent<TextFxTextMeshProUGUI>();
					textMeshPro.text = "Button";
					textMeshPro.fontSize = 24;
					textMeshPro.alignment = TextAlignmentOptions.Center;
				}
				else
				{
					//goRectTransform.sizeDelta = new Vector2(200f, 50f);

					GameObjectUtility.SetParentAndAlign(go, contextObject);

					TextFxTextMeshProUGUI textMeshPro = go.AddComponent<TextFxTextMeshProUGUI>();
					textMeshPro.text = "New Text";
					textMeshPro.alignment = TextAlignmentOptions.TopLeft;
				}
			}


			// Check if an event system already exists in the scene
			if (!Object.FindObjectOfType<EventSystem>())
			{
				GameObject eventObject = new GameObject("EventSystem", typeof(EventSystem));
				eventObject.AddComponent<StandaloneInputModule>();
#if UNITY_5_3_OR_NEWER
				// Nothing
#else
eventObject.AddComponent<TouchInputModule>();
#endif
				Undo.RegisterCreatedObjectUndo(eventObject, "Create " + eventObject.name);
			}

			Selection.activeGameObject = go;
		}


		/// <summary>
		/// Create a TextMeshPro object that works with the Mesh Renderer
		/// </summary>
		/// <param name="command"></param>
		[MenuItem("GameObject/3D Object/TextMeshPro - TextFx", false, 30)]
		static public void CreateTextMeshProInstance(MenuCommand command)
		{
			GameObject go = new GameObject(NEW_INSTANCE_NAME_TMP);

			TextFxTextMeshPro textMeshPro = go.AddComponent<TextFxTextMeshPro>();
			textMeshPro.text = "Sample text";
			textMeshPro.alignment = TextAlignmentOptions.TopLeft;
			textMeshPro.rectTransform.sizeDelta = new Vector2(20, 5);

			Undo.RegisterCreatedObjectUndo((Object)go, "Create " + go.name);

			GameObject contextObject = command != null ? command.context as GameObject : null;
			if (contextObject != null)
			{
				GameObjectUtility.SetParentAndAlign(go, contextObject);
				Undo.SetTransformParent(go.transform, contextObject.transform, "Parent " + go.name);
			}

			Selection.activeGameObject = go;
		}
	}
}
#endif
#endif