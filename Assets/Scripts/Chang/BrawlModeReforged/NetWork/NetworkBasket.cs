using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class NetworkBasket : NetworkBehaviour
{
    private enum ScoreTextState
    {
        Hide,
        Appear,
        Show,
        Disappear
    }

    public int TeamNumber;

    public GameObject Canvas;
    public GameObject ScoreText;

    public float DetectRadius;
    public float SuckRadius;
    public float SuckSpeed;

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
        if (!isServer)
        {
            return;
        }

        RpcCheckScoreText();

        CheckCharacter();
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

    [ClientRpc]
    private void RpcCheckScoreText()
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

    private void CheckCharacter() //Add/remove bagel holder to camera
    {

        Collider[] AllHits = Physics.OverlapSphere(transform.position, DetectRadius);
        for (int i = 0; i < AllHits.Length; i++)
        {
            if (AllHits[i].gameObject.CompareTag("Team2Resource") && AllHits[i].gameObject.GetComponent<NetworkBagel>().Hold)
            {
                InCameraTimer = 0;

                if (!InCamera)
                {
                    RpcCameraAddRemove(true);

                    InCamera = true;
                }
                return;
            }
        }

        if (Bagel == null)
        {
            InCameraTimer += Time.deltaTime;

            if (InCameraTimer >= InCameraTime)
            {
                RpcCameraAddRemove(false);

                InCamera = false;
            }
        }
    }

    [ClientRpc]
    private void RpcCameraAddRemove(bool Add)
    {
        if (Add)
        {
            InCamera = true;
            EventManager.Instance.TriggerEvent(new OnAddCameraTargets(gameObject, 1));
        }
        else
        {
            InCamera = false;
            EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(gameObject));
        }
    }

    private void CheckBagel()// Suck Bagel
    {
        if (Bagel == null)
        {
            Collider[] AllHits = Physics.OverlapSphere(transform.position, SuckRadius);
            for (int i = 0; i < AllHits.Length; i++)
            {
                if (AllHits[i].gameObject.CompareTag("Team2Resource") && !AllHits[i].gameObject.GetComponent<NetworkBagel>().Hold)
                {
                    Bagel = AllHits[i].gameObject;
                    Bagel.GetComponent<BoxCollider>().enabled = false;
                    AllHits[i].gameObject.GetComponent<NetworkBagel>().OnSucked();

                    RpcSuckBagel(AllHits[i].gameObject);

                    break;
                }
            }
        }
        else
        {
            Vector3 Offset = transform.position - Bagel.transform.position;
            Bagel.transform.position += SuckSpeed * Offset.normalized * Time.deltaTime;
            if (Vector3.Dot(Offset, transform.position - Bagel.transform.position) < 0)
            {
                ScoreTextTimer = 0;
                ScoreText.GetComponent<TextMeshProUGUI>().enabled = true;
                TextState = ScoreTextState.Appear;

                EventManager.Instance.TriggerEvent(new BagelSent(gameObject,Bagel.GetComponent<Bagel>().GetLastOwner()));

                NetworkServer.UnSpawn(Bagel);
                Destroy(Bagel);

                RpcSendBagel();
            }
        }
    }

    [ClientRpc]
    private void RpcSendBagel()
    {
        ScoreTextTimer = 0;
        ScoreText.GetComponent<TextMeshProUGUI>().enabled = true;
        TextState = ScoreTextState.Appear;
    }

    [ClientRpc]
    private void RpcSuckBagel(GameObject Obj)
    {
        Bagel = Obj;
        Bagel.GetComponent<BoxCollider>().enabled = false;
        Obj.gameObject.GetComponent<NetworkBagel>().OnSucked();
    }


    private bool SameTeam(GameObject Player)
    {
        return Player.tag.Contains("1") && TeamNumber == 1 || Player.tag.Contains("2") && TeamNumber == 2;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer)
        {
            return;
        }

        if (other.GetComponentInParent<PlayerControllerMirror>() || other.GetComponent<PlayerControllerMirror>())
        {
            if (other.GetComponentInParent<PlayerControllerMirror>() && SameTeam(other.GetComponentInParent<PlayerControllerMirror>().gameObject))
            {
                other.GetComponentInParent<PlayerControllerMirror>().gameObject.GetComponent<PlayerControllerMirror>().ForceDropHandObject(Vector3.zero);
                RpcDrop(other.GetComponentInParent<PlayerControllerMirror>().gameObject, Vector3.zero);
            }
            else if (other.GetComponent<PlayerControllerMirror>() && SameTeam(other.gameObject))
            {
                other.GetComponent<PlayerControllerMirror>().ForceDropHandObject(Vector3.zero);
                RpcDrop(other.gameObject, Vector3.zero);
            }
        }
    }

    [ClientRpc]
    private void RpcDrop(GameObject Player, Vector3 force)
    {
        Player.GetComponent<PlayerControllerMirror>().ForceDropHandObject(force);
    }
}
