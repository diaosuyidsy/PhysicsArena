using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TextFx;
using Boomlagoon.TextFx.JSON;

public class TextFxQuickSetupAnimConfigs : MonoBehaviour {

	const string INTRO_ANIM_FOLDER_NAME = "Intros";
	const string MAIN_ANIM_FOLDER_NAME = "Mains";
	const string OUTRO_ANIM_FOLDER_NAME = "Outros";

	static string[] m_introAnimNames;
	static string[] m_mainAnimNames;
	static string[] m_outroAnimNames;

	public static string[] IntroAnimNames {
		get {
			GetLatestEffectNameLists();

			return m_introAnimNames;
		}
	}

	public static string[] MainAnimNames {
		get {
			GetLatestEffectNameLists();

			return m_mainAnimNames;
		}
	}

	public static string[] OutroAnimNames {
		get {
			GetLatestEffectNameLists();

			return m_outroAnimNames;
		}
	}

	public static void GetLatestEffectNameLists(bool force = false)
	{
		if(m_introAnimNames == null || force)
		{
			tfxJSONObject animNamesData = tfxJSONObject.Parse((Resources.Load<TextAsset>("textfxAnimNames")).text);

			m_introAnimNames = new string[animNamesData[INTRO_ANIM_FOLDER_NAME].Array.Length + 1];
			m_introAnimNames[0] = "None";
			int idx = 1;
			foreach (tfxJSONValue animVal in animNamesData[INTRO_ANIM_FOLDER_NAME].Array)
			{
				m_introAnimNames[idx] = animVal.Str;
				idx++;
			}

			m_mainAnimNames = new string[animNamesData[MAIN_ANIM_FOLDER_NAME].Array.Length + 1];
			m_mainAnimNames[0] = "None";
			idx = 1;
			foreach (tfxJSONValue animVal in animNamesData[MAIN_ANIM_FOLDER_NAME].Array)
			{
				m_mainAnimNames[idx] = animVal.Str;
				idx++;
			}

			m_outroAnimNames = new string[animNamesData[OUTRO_ANIM_FOLDER_NAME].Array.Length + 1];
			m_outroAnimNames[0] = "None";
			idx = 1;
			foreach (tfxJSONValue animVal in animNamesData[OUTRO_ANIM_FOLDER_NAME].Array)
			{
				m_outroAnimNames[idx] = animVal.Str;
				idx++;
			}
		}
	}


	public static string GetConfig(TextFxAnimationManager.PRESET_ANIMATION_SECTION section, string animName)
	{
		string[] animFolderNames = new string[] { INTRO_ANIM_FOLDER_NAME, MAIN_ANIM_FOLDER_NAME, OUTRO_ANIM_FOLDER_NAME };

		animName = animName.Trim();

		TextAsset animData = Resources.Load<TextAsset>(animFolderNames[(int)section] + "/" + animName);

		return animData != null ? animData.text : "";
	}
}
