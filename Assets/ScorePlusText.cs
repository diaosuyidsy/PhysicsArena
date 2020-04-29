using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorePlusText : MonoBehaviour
{
    public GameObject Holder;

    public float StartScale;
    public float BigScale;
    public float HugeScale;
    public float EndScale;

    public float MoveTime;
    public float FadeTime;
    public float ScaleUpTime;
    public float ScaleDownTime;
    public float StayTime;

    public bool Team1;
    public Color RedColor;
    public Color BlueColor;

    public Vector3 FadeOffset;

    private float Timer;

    private float CurrentOffset;
    private Vector3 OriOffset;
    private GameObject GameUI;
    private Vector3 MoveTarget;
    private Vector3 MoveStart;

    // Start is called before the first frame update
    void Start()
    {
        if (Team1)
        {
            GetComponent<TextMeshProUGUI>().color = RedColor;
        }
        else
        {
            GetComponent<TextMeshProUGUI>().color = BlueColor;
        }

        //Color color = GetComponent<TextMeshProUGUI>().color;
        //GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, 0);

        transform.localScale = StartScale * Vector3.one;

        OriOffset = transform.parent.position - Holder.transform.position;

        CurrentOffset = 1;

        GameUI = GameObject.Find("GameUI").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        transform.parent.LookAt(Camera.main.transform,Vector3.up);

        Timer += Time.deltaTime;

        if (Timer <= ScaleUpTime)
        {
            transform.parent.position = OriOffset + Holder.transform.position;
            transform.localScale = Mathf.Lerp(StartScale, BigScale, Timer / ScaleUpTime) * Vector3.one;
        }
        else if (Timer <= ScaleUpTime + ScaleDownTime)
        {
            float t = (Timer - ScaleUpTime) / ScaleDownTime;
            transform.localScale = Mathf.Lerp(BigScale, EndScale, t) * Vector3.one;

        }
        else if (Timer <= ScaleUpTime + ScaleDownTime + StayTime)
        {
            transform.localScale = Vector3.one * EndScale;
        }
        else if (Timer <= ScaleUpTime + ScaleDownTime + StayTime + FadeTime)
        {
            float t = (Timer - ScaleUpTime - ScaleDownTime - StayTime) / FadeTime;

            Color color = GetComponent<TextMeshProUGUI>().color;
            GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, 1 - t);

            CurrentOffset = Mathf.Lerp(0, 1, t);
            transform.parent.position = OriOffset + FadeOffset * CurrentOffset + Holder.transform.position;
        }
        else
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
