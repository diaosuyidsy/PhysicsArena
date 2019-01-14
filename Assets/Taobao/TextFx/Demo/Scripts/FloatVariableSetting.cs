using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TextFx;

public class FloatVariableSetting : BaseVariableSetting {

	public InputField m_fromValue;
	public InputField m_toValue;
	public InputField m_thenValue;

	public GameObject m_toValueObject;
	public GameObject m_thenValueObject;

	public override void Setup(string labelName, List<PresetEffectSetting.VariableStateListener> varStateListeners, System.Action valueChangedCallback, bool isSubSetting)
	{
		base.Setup (labelName, varStateListeners, valueChangedCallback, isSubSetting);

		// Activate the input objects we'll need
		if (varStateListeners.Count == 1)
		{
			m_toValueObject.SetActive (false);
			m_thenValueObject.SetActive (false);
		}
		else if (varStateListeners.Count == 2)
		{
			m_toValueObject.SetActive (true);
			m_thenValueObject.SetActive (false);
		}
		else if (varStateListeners.Count == 3)
		{
			m_toValueObject.SetActive (true);
			m_thenValueObject.SetActive (true);
		}

		m_fromValue.text = varStateListeners[0].m_startFloatValue.ToString();
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        m_fromValue.onValueChange.AddListener ((string newValue) => {
#else
		m_fromValue.onValueChanged.AddListener ((string newValue) => {
#endif
            varStateListeners[0].m_onFloatStateChangeCallback( float.Parse(newValue));
			if(valueChangedCallback != null)
				valueChangedCallback();
        });

		if(varStateListeners.Count > 1)
		{
			m_toValue.text = varStateListeners[1].m_startFloatValue.ToString();
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			m_toValue.onValueChange.AddListener ((string newValue) => {
#else
			m_toValue.onValueChanged.AddListener ((string newValue) => {
#endif
				varStateListeners[1].m_onFloatStateChangeCallback( float.Parse(newValue));
				if (valueChangedCallback != null)
					valueChangedCallback();
			});
		}
		if(varStateListeners.Count > 2)
		{
			m_thenValue.text = varStateListeners[2].m_startFloatValue.ToString();
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			m_thenValue.onValueChange.AddListener ((string newValue) => {
#else
			m_thenValue.onValueChanged.AddListener ((string newValue) => {
#endif
				varStateListeners[2].m_onFloatStateChangeCallback( float.Parse(newValue));
				if (valueChangedCallback != null)
					valueChangedCallback();
			});
		}
    }
}
