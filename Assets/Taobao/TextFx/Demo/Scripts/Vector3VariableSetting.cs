using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TextFx;

public class Vector3VariableSetting : BaseVariableSetting {

	public InputField m_fromXValue;
	public InputField m_fromYValue;
	public InputField m_fromZValue;

	public GameObject m_toValueObject;
	public GameObject m_thenValueObject;

	Vector3 m_fromVec;

	public override void Setup(string labelName, List<PresetEffectSetting.VariableStateListener> varStateListeners, System.Action valueChangedCallback, bool isSubSetting)
	{
		base.Setup (labelName, varStateListeners, valueChangedCallback, isSubSetting);

		//// Activate the input objects we'll need
		//if (varStateListeners.Count == 1)
		//{
		//	m_toValueObject.SetActive (false);
		//	m_thenValueObject.SetActive (false);
		//}
		//else if (varStateListeners.Count == 2)
		//{
		//	m_toValueObject.SetActive (true);
		//	m_thenValueObject.SetActive (false);
		//}
		//else if (varStateListeners.Count == 3)
		//{
		//	m_toValueObject.SetActive (true);
		//	m_thenValueObject.SetActive (true);
		//}

		m_fromVec = varStateListeners[0].m_startVector3Value;

		m_fromXValue.text = m_fromVec.x.ToString();
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
		m_fromXValue.onValueChange.AddListener ((string newValue) => {
#else
		m_fromXValue.onValueChanged.AddListener ((string newValue) => {
#endif
			m_fromVec.x = float.Parse(newValue);
            varStateListeners[0].m_onVector3StateChangeCallback(m_fromVec);
			valueChangedCallback();
        });

		m_fromYValue.text = m_fromVec.y.ToString();
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
		m_fromYValue.onValueChange.AddListener((string newValue) => {
#else
		m_fromYValue.onValueChanged.AddListener((string newValue) => {
#endif
			m_fromVec.y = float.Parse(newValue);
			varStateListeners[0].m_onVector3StateChangeCallback(m_fromVec);
			valueChangedCallback();
		});

		m_fromZValue.text = m_fromVec.z.ToString();
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
		m_fromZValue.onValueChange.AddListener((string newValue) => {
#else
		m_fromZValue.onValueChanged.AddListener((string newValue) => {
#endif
			m_fromVec.z = float.Parse(newValue);
			varStateListeners[0].m_onVector3StateChangeCallback(m_fromVec);
			valueChangedCallback();
		});

		//if(varStateListeners.Count > 1)
		//{
		//	m_toValue.text = varStateListeners[1].m_startFloatValue.ToString();
		//	m_toValue.onValueChange.AddListener ((string newValue) => {
		//		varStateListeners[1].m_onFloatStateChangeCallback( float.Parse(newValue));
		//		valueChangedCallback();
		//	});
		//}
		//if(varStateListeners.Count > 2)
		//{
		//	m_thenValue.text = varStateListeners[2].m_startFloatValue.ToString();
		//	m_thenValue.onValueChange.AddListener ((string newValue) => {
		//		varStateListeners[2].m_onFloatStateChangeCallback( float.Parse(newValue));
		//		valueChangedCallback();
		//	});
		//}
	}
}
