using UnityEngine;
using UnityEditor;
using System.Collections;

/* 	Class to listen for reimported Font files (caused by font size change, font type change and other setting changes).
	Calls to all EffectManager instances in scene to let them know the font that's changed. */

namespace TextFx
{
#if UNITY_4_6 || UNITY_4_5 || UNITY_4_3 || UNITY_4_2 || UNITY_4_1 || UNITY_4_0_1 || UNITY_4_0
	class TextfxFontChangeListener : AssetPostprocessor
	{
		static void OnPostprocessAllAssets (
			string[] importedAssets,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			string asset_path;
			foreach (string str in importedAssets)
			{
				asset_path = str.ToLower();

				string[] parts = asset_path.Split('.');
				string file_extension = parts[parts.Length - 1];
				
				if(file_extension.Equals("ttf") || file_extension.Equals("dfont") || file_extension.Equals("otf"))
				{
					// Imported a font file. Tell all EffectManager instances, to update text accordingly
					parts = asset_path.Split('/');
					string font_name = parts[parts.Length - 1];
					font_name = font_name.Replace(".ttf", "");
					font_name = font_name.Replace(".dfont", "");
					font_name = font_name.Replace(".otf", "");
					
					TextFxNative[] effects = GameObject.FindObjectsOfType(typeof(TextFxNative)) as TextFxNative[];
					
					foreach(TextFxNative effect in effects)
					{
						effect.FontImportDetected(font_name);
					}
				}
			}
		}
	}
#endif
}