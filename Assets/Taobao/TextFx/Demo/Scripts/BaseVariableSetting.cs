using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TextFx;

public class BaseVariableSetting : MonoBehaviour {

	readonly Color SETTING_COLOUR = new Color(163f / 255f, 228f / 255f, 1, 100f / 255f);
	readonly Color SUB_SETTING_COLOUR = new Color(0, 160f / 255f, 1, 65f / 255f);

	public Text m_labelText;
	public Image m_bgImage;

	bool m_subSetting = false;
	bool m_subSettingActive = true;

	public bool IsSubSetting { get { return m_subSetting; } }
	public bool SubSettingActive { get { return m_subSettingActive; } }

	public void SubSettingSetActive(bool state)
	{
		m_subSettingActive = state;

		gameObject.SetActive (state);
	}

	public virtual void Setup(PresetEffectSetting settingData, List<PresetEffectSetting.VariableStateListener> varStateListener, System.Action valueChangedCallback, bool isSubSetting)
	{
		Setup(settingData.m_setting_name, varStateListener, valueChangedCallback, isSubSetting);
	}

	public virtual void Setup(string labelName, List<PresetEffectSetting.VariableStateListener> varStateListeners, System.Action valueChangedCallback, bool isSubSetting)
	{
		m_subSetting = isSubSetting;

		m_bgImage.color = m_subSetting ? SUB_SETTING_COLOUR : SETTING_COLOUR;
		m_labelText.text = labelName;
	}
}
