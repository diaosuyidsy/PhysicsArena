using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TextFx;

public class ToggleVariableSetting : BaseVariableSetting {

	public Toggle m_toggleInput;

	public override void Setup(string labelText, List<PresetEffectSetting.VariableStateListener> varStateListener, System.Action valueChangedCallback, bool isSubSetting)
	{
		if (varStateListener == null || varStateListener.Count != 1)
			return;

		base.Setup (labelText, varStateListener, valueChangedCallback, isSubSetting);

		m_toggleInput.isOn = varStateListener[0].m_startToggleValue;
        m_toggleInput.onValueChanged.AddListener((bool state) =>
		{
			varStateListener[0].m_onToggleStateChangeCallback(state);

			if(valueChangedCallback != null)
				valueChangedCallback();
		});
    }
}
