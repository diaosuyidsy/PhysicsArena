﻿using System.Collections;
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

    public float DetectRadius;
    public float AutoDropRadius;
    public float SuckRadius;
    public float SuckSpeed;
    public float VerticalOffset;

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
                        Pc.ForceDropHandObject();
                        return;
                    }
                }
                else if (other.GetComponent<PlayerController>() && SameTeam(other.gameObject))
                {
                    PlayerController Pc = other.GetComponent<PlayerController>();

                    if (Pc.HandObject != null && Pc.HandObject.CompareTag("Team2Resource"))
                    {
                        Pc.ForceDropHandObject();
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
                    Services.GameStateManager.CameraTargets.Add(transform);
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
                Services.GameStateManager.CameraTargets.Remove(transform);
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
                    AllHits[i].gameObject.GetComponent<Bagel>().OnSucked();
                    break;
                }
            }
        }
        else
        {
            Vector3 Offset = transform.position +Vector3.up * VerticalOffset - Bagel.transform.position;
            Bagel.transform.position += SuckSpeed * Offset.normalized * Time.deltaTime;
            if (Vector3.Dot(Offset, transform.position - Bagel.transform.position) < 0)
            {
                ScoreTextTimer = 0;
                ScoreText.GetComponent<TextMeshProUGUI>().enabled = true;
                TextState = ScoreTextState.Appear;

                EventManager.Instance.TriggerEvent(new BagelSent(gameObject));

                Bagel = null;
            }
        }
    }

    private bool SameTeam(GameObject Player)
    {
        return Player.tag.Contains("1") && TeamNumber == 1 || Player.tag.Contains("2") && TeamNumber == 2;
    }
}
