using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace TextFx
{
	[InitializeOnLoad]
	public class TextFxPluginsCheck
	{
		static TextFxPluginsCheck()
		{
			CheckForPlugins ();
		}

		[MenuItem("Tools/TextFx/Check For Plugin Support")]
		private static void CheckForPlugins()
		{
			if (CheckIfAssetClassesArePresent ("TextMeshPro"))
			{
				if (AddFlagToScriptingDefines ("TMP"))
				{
					Debug.Log ("Detected 'Text Mesh Pro' plugin in Project. Auto added scripting defines 'TMP' to enable additional TextFx functionality.");
				}
			}

			if (CheckIfAssetClassesArePresent ("NGUIMenu"))
			{
				if (AddFlagToScriptingDefines ("NGUI"))
				{
					Debug.Log ("Detected 'NGUI' plugin in Project. Auto added scripting defines 'NGUI' to enable additional TextFx functionality.");
				}
			}

			if (CheckIfAssetClassesArePresent ("PlayMakerFSM"))
			{
				if (AddFlagToScriptingDefines ("PLAYMAKER"))
				{
					Debug.Log ("Detected 'PlayMaker' plugin in Project. Auto added scripting defines 'PLAYMAKER' to enable additional TextFx functionality.");
				}
			}
		}

		private static bool CheckIfAssetClassesArePresent( string a_pluginClassName)
		{
			var pluginClassType = (	from assembly in AppDomain.CurrentDomain.GetAssemblies()
									from type in assembly.GetTypes()
									where type.Name == a_pluginClassName
									select type).FirstOrDefault();

			return pluginClassType != null;
		}

		private static bool AddFlagToScriptingDefines(string a_flag)
		{
			IEnumerable<BuildTargetGroup> buildTargets = Enum.GetValues (typeof(BuildTargetGroup)).Cast<BuildTargetGroup> ();
			string defineSymbolsString;
			bool changeMade = false;

			var moduleManager = System.Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
			var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTargetGroup", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

			foreach(BuildTargetGroup target in buildTargets)
			{
				bool isPlatformLoaded = target == BuildTargetGroup.Standalone ||
										(bool)isPlatformSupportLoaded.Invoke(null,new object[] {(string)getTargetStringFromBuildTarget.Invoke(null, new object[] {target})});

				// Check for deprecated or uninstalled build targets
				if (!isPlatformLoaded)
				{
					// Debug.LogWarning ("Excluding platform #" + ((int)target) + ", " + target);
					continue;
				}

				defineSymbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup (target);

				if (!defineSymbolsString.Contains (a_flag))
				{
					defineSymbolsString += ";" + a_flag;

					PlayerSettings.SetScriptingDefineSymbolsForGroup (target, defineSymbolsString);

					changeMade = true;
				}
			}

			if (changeMade)
			{
				AssetDatabase.SaveAssets ();
			}

			return changeMade;
		}
	}
}