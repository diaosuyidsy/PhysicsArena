using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Textblink : MonoBehaviour
{
    public float TextBlinkSpeed = 5f;
    private Text _text;
    private Text _subtext;
    private float _timeForBlink = 0f;

    private void Awake()
    {
        _text = GetComponent<Text>();
        if (transform.childCount >= 1)
            _subtext = transform.GetChild(0).GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        _updateAlpha(_text);
        if (_subtext != null)
            _updateAlpha(_subtext);
    }

    private void _updateAlpha(Text t)
    {
        float BlinkAlpha = (0.5f + 0.5f * Mathf.Cos(_timeForBlink));
        Color textColor1 = t.color;
        textColor1.a = BlinkAlpha;
        t.color = textColor1;
        _timeForBlink += Time.deltaTime * TextBlinkSpeed;
    }
}
