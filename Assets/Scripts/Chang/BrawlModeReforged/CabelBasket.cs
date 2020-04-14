using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CabelBasket : MonoBehaviour
{
    private enum ScoreTextState
    {
        Hide,
        Appear,
        Show,
        Disappear
    }

    public int TeamNumber;

    public GameObject ScoreText;

    public GameObject Up;
    public GameObject Down;

    public float DropAngleTolerance;

    public float DoorOpenTime;

    public float DetectRadius;
    public float AutoDropRadius;
    public float SuckRadius;

    public float SuckHoriSpeed;
    public float SuckVerSpeed;

    public float VerticalSuckPos;

    public float ScoreTextStartAlpha;
    public float ScoreTextEndAlpha;
    public float ScoreTextStartScale;
    public float ScoreTextEndScale;

    public float ScoreTextAppearTime;
    public float ScoreTextShowTime;
    public float ScoreTextDisappearTime;

    public float InCameraTime;

    private bool InCamera;

    private GameObject Bagel;
    private float ScoreTextTimer;
    private ScoreTextState TextState;

    private float InCameraTimer;

    private Vector3 SuckRotationSpeed;

    private Vector3 Target;


    // Start is called before the first frame update
    void Start()
    {
        TextState = ScoreTextState.Hide;
    }

    // Update is called once per frame
    void Update()
    {

        CheckCharacter();
        CheckAutoDrop();
        CheckBagel();
        CheckScoreText();
    }

    private void CheckScoreText() //Score text pop up
    {
        Color color = ScoreText.GetComponent<TextMeshProUGUI>().color;
        switch (TextState)
        {
            case ScoreTextState.Appear:
                ScoreTextTimer += Time.deltaTime;
                ScoreText.GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, Mathf.Lerp(ScoreTextStartAlpha, ScoreTextEndAlpha, ScoreTextTimer / ScoreTextAppearTime));
                ScoreText.transform.localScale = Vector3.Lerp(Vector3.one * ScoreTextStartScale, Vector3.one * ScoreTextEndScale, ScoreTextTimer / ScoreTextAppearTime);
                if (ScoreTextTimer >= ScoreTextAppearTime)
                {
                    TextState = ScoreTextState.Show;
                    ScoreTextTimer = 0;
                }
                break;
            case ScoreTextState.Disappear:
                ScoreTextTimer += Time.deltaTime;
                ScoreText.GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, Mathf.Lerp(ScoreTextEndAlpha, 0, ScoreTextTimer / ScoreTextDisappearTime));
                if (ScoreTextTimer >= ScoreTextDisappearTime)
                {
                    ScoreText.GetComponent<TextMeshProUGUI>().enabled = false;
                    TextState = ScoreTextState.Hide;
                    ScoreTextTimer = 0;
                }
                break;
            case ScoreTextState.Show:
                ScoreTextTimer += Time.deltaTime;
                if (ScoreTextTimer >= ScoreTextShowTime)
                {
                    TextState = ScoreTextState.Disappear;
                    ScoreTextTimer = 0;
                    ScoreText.transform.localScale = Vector3.zero;
                }
                break;
        }
    }

    private void CheckAutoDrop()
    {
        Collider[] AllHits = Physics.OverlapSphere(transform.position, AutoDropRadius);

        for (int i = 0; i < AllHits.Length; i++)
        {
            GameObject other = AllHits[i].gameObject;
            if (other.GetComponentInParent<PlayerController>() || other.GetComponent<PlayerController>())
            {
                if (other.GetComponentInParent<PlayerController>() && SameTeam(other.GetComponentInParent<PlayerController>().gameObject))
                {
                    PlayerController Pc = other.GetComponentInParent<PlayerController>().gameObject.GetComponent<PlayerController>();

                    if (Pc.HandObject!=null && Pc.HandObject.CompareTag("Team2Resource"))
                    {
                        Vector3 Offset = transform.position - Pc.transform.position;
                        Offset.y = 0;
                        Vector3 Forward = Pc.HandObject.transform.forward;
                        Forward.y = 0;

                        if (Vector3.Angle(Offset, Forward) < DropAngleTolerance)
                        {
                            Pc.ForceDropHandObject(new Vector3(0, 2, 2));
                        }
                        return;
                    }
                }
                else if (other.GetComponent<PlayerController>() && SameTeam(other.gameObject))
                {
                    PlayerController Pc = other.GetComponent<PlayerController>();

                    if (Pc.HandObject != null && Pc.HandObject.CompareTag("Team2Resource"))
                    {
                        Vector3 Offset = transform.position - Pc.transform.position;
                        Offset.y = 0;
                        Vector3 Forward = Pc.HandObject.transform.forward;
                        Forward.y = 0;

                        if (Vector3.Angle(Offset, Forward) < DropAngleTolerance)
                        {
                            Pc.ForceDropHandObject(new Vector3(0, 2, 2));
                        }
                        return;
                    }
                }
            }
        }

    }

    private void CheckCharacter() //Add/remove bagel holder to camera
    {

        Collider[] AllHits = Physics.OverlapSphere(transform.position, DetectRadius);
        for (int i = 0; i < AllHits.Length; i++)
        {
            if (AllHits[i].gameObject.CompareTag("Team2Resource") && AllHits[i].gameObject.GetComponent<Bagel>().Hold)
            {
                InCameraTimer = 0;

                if (!InCamera)
                {
                    InCamera = true;

                    EventManager.Instance.TriggerEvent(new OnAddCameraTargets(gameObject, 1));
                }
                return;
            }
        }

        if (Bagel == null)
        {
            InCameraTimer += Time.deltaTime;

            if (InCameraTimer >= InCameraTime)
            {
                InCamera = false;
                EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(gameObject));
            }
        }

    }

    private void CheckBagel()// Suck Bagel
    {
        if (Bagel == null)
        {
            Collider[] AllHits = Physics.OverlapSphere(transform.position, SuckRadius);
            for (int i = 0; i < AllHits.Length; i++)
            {
                if (AllHits[i].gameObject.CompareTag("Team2Resource") && !AllHits[i].gameObject.GetComponent<Bagel>().Hold)
                {
                    Bagel = AllHits[i].gameObject;
                    Bagel.GetComponent<BoxCollider>().enabled = false;
                    Bagel.GetComponent<Bagel>().OnSucked();

                    Target = transform.position;
                    Target.y = VerticalSuckPos;

                    Vector3 Offset= Target- Bagel.transform.position;

                    SuckRotationSpeed = -Bagel.transform.eulerAngles * SuckVerSpeed / Offset.y;

                    StartCoroutine(DoorOpenClose(true));
                    break;


                }
            }
        }
        else
        {
            Vector3 Offset = Target - Bagel.transform.position;
            Vector3 HoriOffset = Offset;
            HoriOffset.y = 0;

            Bagel.transform.position += HoriOffset.normalized * SuckHoriSpeed * Time.deltaTime;
            Bagel.transform.position += Vector3.down * SuckVerSpeed * Time.deltaTime;

            Vector3 NewHoriOffset = Target - Bagel.transform.position;
            NewHoriOffset.y = 0;
            if (Vector3.Dot(HoriOffset, NewHoriOffset) < 0)
            {
                Bagel.transform.position = new Vector3(Target.x, Bagel.transform.position.y, Target.z);
            }

            if(Bagel.transform.position.y < Target.y)
            {
                Bagel.transform.eulerAngles = Vector3.zero;
                ScoreTextTimer = 0;
                ScoreText.GetComponent<TextMeshProUGUI>().enabled = true;
                TextState = ScoreTextState.Appear;

                StartCoroutine(DoorOpenClose(false));
                EventManager.Instance.TriggerEvent(new BagelSent(gameObject));

                Bagel = null;
            }


            Bagel.transform.eulerAngles += SuckRotationSpeed * Time.deltaTime;


            /*if (Vector3.Dot(Offset, transform.position - Bagel.transform.position) < 0)
            {
                Bagel.transform.eulerAngles = Vector3.zero;
                ScoreTextTimer = 0;
                ScoreText.GetComponent<TextMeshProUGUI>().enabled = true;
                TextState = ScoreTextState.Appear;

                StartCoroutine(DoorOpenClose(false));
                EventManager.Instance.TriggerEvent(new BagelSent(gameObject));

                Bagel = null;
            }*/
        }
    }

    private bool SameTeam(GameObject Player)
    {
        return Player.tag.Contains("1") && TeamNumber == 1 || Player.tag.Contains("2") && TeamNumber == 2;
    }

    public IEnumerator DoorOpenClose(bool Open)
    {
        float Angle = 0;

        float DoorOpenSpeed = 90f/DoorOpenTime;

        while (Angle <= 90)
        {
            Angle += DoorOpenSpeed * Time.deltaTime;
            if (Open)
            {
                Up.transform.localEulerAngles = new Vector3(Angle ,0, 0);
                Down.transform.localEulerAngles = new Vector3(-Angle, 0,  0);
            }
            else
            {
                Up.transform.localEulerAngles = new Vector3(90-Angle, 0,  0);
                Down.transform.localEulerAngles = new Vector3(Angle-90, 0,  0);
            }

            yield return null;
        }


    }
}
