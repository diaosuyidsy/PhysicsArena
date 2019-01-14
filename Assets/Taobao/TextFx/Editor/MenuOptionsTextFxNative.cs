using UnityEngine;
using UnityEditor;

namespace TextFx
{
	public static class MenuOptionsTextFxNative
	{
		public const string NEW_INSTANCE_NAME = "TextFx Text";

		[MenuItem("GameObject/TextFx/Text", false)]
		static public void AddTextFxNativeInstance ()
		{
			GameObject go = new GameObject (NEW_INSTANCE_NAME);
			
			TextFxNative textfxComp = go.AddComponent<TextFxNative>();
			textfxComp.SetText("New TextFx");
			
			Selection.activeGameObject = go;
		}
	}
}
