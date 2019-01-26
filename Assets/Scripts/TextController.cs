using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour
{
    private Text _text;
    private void Awake()
    {
        _text = GetComponent<Text>();
    }
    public void UpdateText(float number)
    {
        _text.text = Mathf.FloorToInt(number).ToString();
    }

    public void UpdateFloatText(float number)
    {
        _text.text = number.ToString("F1");
    }

}
