using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TextFx;
using HSVPickerDemo;

public class ColourVariableSetting : BaseVariableSetting {

	public Image m_fromValue;
	public Image m_toValue;
	public Image m_thenValue;
	public GameObject m_fromHighlight;
	public GameObject m_toHighlight;
	public GameObject m_thenHighlight;

	public GameObject m_toValueObject;
	public GameObject m_thenValueObject;

	public GameObject m_colourPickerSection;
	public HSVPicker m_colourPicker;
	public LayoutElement m_sectionLayoutElement;

	bool m_colourSelected = false;
	int m_currentColourIndex = 0;
	List<System.Action<Color>> m_stateListenerCallbacks;
	System.Action m_valueChangedCallback;
	Image[] m_colours;

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

		m_valueChangedCallback = valueChangedCallback;
		m_stateListenerCallbacks = new List<System.Action<Color>>();

		m_fromValue.color = varStateListeners[0].m_startColorValue;
		m_stateListenerCallbacks.Add(varStateListeners[0].m_onColorStateChangeCallback);

		if (varStateListeners.Count > 1)
		{
			m_toValue.color = varStateListeners[1].m_startColorValue;
			m_stateListenerCallbacks.Add(varStateListeners[1].m_onColorStateChangeCallback);
		}
		if (varStateListeners.Count > 2)
		{
			m_thenValue.color = varStateListeners[2].m_startColorValue;
			m_stateListenerCallbacks.Add(varStateListeners[2].m_onColorStateChangeCallback);
		}

		m_colourPicker.onValueChanged.AddListener(OnColourChanged);

		m_colours = new Image[] { m_fromValue, m_toValue, m_thenValue };
	}

	public void ColourSelected(int index)
	{
		GameObject[] highlights = new GameObject[] { m_fromHighlight, m_toHighlight, m_thenHighlight };

		m_colourSelected = !highlights[index].activeSelf;

		for (int idx = 0; idx < 3; idx++)
		{
			if (idx == index)
				highlights[idx].SetActive(m_colourSelected);
			else
				highlights[idx].SetActive(false);
		}

		if (m_colourSelected)
		{
			m_sectionLayoutElement.minHeight = 130;

			Color cachedTargetColour = m_colours[index].color;

            m_colourPickerSection.SetActive(true);

			m_colourPicker.AssignColor(cachedTargetColour);

			m_currentColourIndex = index;
		}
		else
		{
			m_sectionLayoutElement.minHeight = 30;

			m_colourPickerSection.SetActive(false);
		}
	}

	void OnColourChanged(Color newColour)
	{
		m_colours[m_currentColourIndex].color = newColour;

		m_stateListenerCallbacks[m_currentColourIndex](newColour);

		if (m_valueChangedCallback != null)
			m_valueChangedCallback();
    }
}
