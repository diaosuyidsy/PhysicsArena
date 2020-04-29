using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorePlusScreen : MonoBehaviour
{
    public GameObject Holder;

    public float StartScale;
    public float BigScale;
    public float HugeScale;
    public float EndScale;

    public float MoveTime;
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

        transform.localScale = StartScale * Vector3.one/transform.parent.localScale.x;



        CurrentOffset = 1;

        GameUI = GameObject.Find("GameUI").gameObject;
    }

    // Update is called once per frame
    void Update()
    {

        Timer += Time.deltaTime;

        if (Timer <= ScaleUpTime)
        {
            Vector3 Pos = Camera.main.WorldToScreenPoint(Holder.transform.position);
            Pos.z = 0;
            transform.position = Pos;
            transform.localScale = Mathf.Lerp(StartScale, BigScale, Timer / ScaleUpTime) * Vector3.one/ transform.parent.localScale.x;
        }
        else if (Timer <= ScaleUpTime + ScaleDownTime)
        {
            Vector3 Pos = Camera.main.WorldToScreenPoint(Holder.transform.position);
            Pos.z = 0;
            transform.position = Pos;

            float t = (Timer - ScaleUpTime) / ScaleDownTime;
            transform.localScale = Mathf.Lerp(BigScale, EndScale, t) * Vector3.one / transform.parent.localScale.x;

        }
        else if (Timer <= ScaleUpTime + ScaleDownTime + StayTime)
        {
            Vector3 Pos = Camera.main.WorldToScreenPoint(Holder.transform.position);
            Pos.z = 0;
            transform.position = Pos;

            transform.localScale = Vector3.one * EndScale / transform.parent.localScale.x;
        }
        else if (Timer <= ScaleUpTime + ScaleDownTime + StayTime + MoveTime)
        {
            if(Timer - Time.deltaTime <= ScaleUpTime + ScaleDownTime + StayTime)
            {
                MoveStart = transform.position;

                if (Team1)
                {
                    MoveTarget = GameUI.transform.Find("Team1Background").gameObject.transform.position;
                }
                else
                {
                    MoveTarget = GameUI.transform.Find("Team2Background").gameObject.transform.position;
                }
            }

            float t = (Timer - ScaleUpTime - ScaleDownTime - StayTime) / MoveTime;

            //Color color = GetComponent<TextMeshProUGUI>().color;
            //GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, 1 - t);

            CurrentOffset = Mathf.Lerp(0, 1, t);

            transform.position = Vector3.Lerp(MoveStart, MoveTarget, t);

            //transform.parent.position = OriOffset + FadeOffset * CurrentOffset + Holder.transform.position;
        }
        else
        {
            EventManager.Instance.TriggerEvent(new ScoreEffectTrigger(Team1));
            Destroy(transform.gameObject);
        }
    }
}
