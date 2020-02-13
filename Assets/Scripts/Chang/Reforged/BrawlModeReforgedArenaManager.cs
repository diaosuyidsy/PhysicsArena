using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CanonState
{
    Unactivated,
    Cooldown,
    Firing
}

public enum MiddleState
{
    Neutral,
    Red,
    Blue
}

public class BrawlModeReforgedArenaManager : MonoBehaviour
{
    private class CanonInfo
    {
        public GameObject Entity;
        public CanonState State;
        public float Timer;
        public int FireCount;
        public GameObject CooldownMark;
        public GameObject Mark;
        public GameObject LockedPlayer;


        public CanonInfo(GameObject canon, CanonState state, GameObject cdmark)
        {
            Entity = canon;
            State = state;
            CooldownMark = cdmark;
            CooldownMark.SetActive(false);

            FireCount = 0;
            Timer = 0;
            Mark = null;
            LockedPlayer = null;
        }

        public void TransitionTo(CanonState state)
        {
            State = state;
            Timer = 0;

            switch (State)
            {
                case CanonState.Unactivated:
                    CooldownMark.SetActive(false);
                    FireCount = 0;
                    break;
                case CanonState.Cooldown:
                    CooldownMark.SetActive(true);
                    break;
                case CanonState.Firing:
                    CooldownMark.SetActive(true);
                    break;
            }
        }
    }

    public BrawlModeReforgedModeData Data;

    public GameObject Team1Canon;
    public GameObject Team2Canon;
    public GameObject Team1CanonCooldownMark;
    public GameObject Team2CanonCooldownMark;

    public GameObject CanonObject;
    public GameObject Lever;

    private CanonInfo Team1CanonInfo;
    private CanonInfo Team2CanonInfo;

    public LayerMask PlayerLayer;
    public LayerMask GroundLayer;
    public GameObject Players;

    public GameObject BagelPrefab;
    public GameObject MarkPrefab;
    public GameObject ExplosionVFX;
    public Color MarkDefaultColor;
    public Color MarkAlertColor;

    public Vector3 BagelGenerationPos;
    public Vector3 BagelGenerationPosLeft;
    public Vector3 BagelGenerationPosRight;

    public float SwtichingTime;
    public float LeverAngle;

    private GameObject Bagel;

    private MiddleState CurrentMid;
    private MiddleState PreviousMid;

    // Start is called before the first frame update
    void Start()
    {
        CurrentMid = MiddleState.Neutral;
        PreviousMid = MiddleState.Neutral;

        Team1CanonInfo = new CanonInfo(Team1Canon, CanonState.Unactivated,Team1CanonCooldownMark);
        Team2CanonInfo = new CanonInfo(Team2Canon, CanonState.Unactivated,Team2CanonCooldownMark);

        GenerateBagel();

        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);
        EventManager.Instance.AddHandler<BagelSent>(OnBagelSent);
        EventManager.Instance.AddHandler<BagelDespawn>(OnBagelDespawn);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<PlayerDied>(OnPlayerDied);
        EventManager.Instance.RemoveHandler<BagelSent>(OnBagelSent);
        EventManager.Instance.RemoveHandler<BagelDespawn>(OnBagelDespawn);
    }

    // Update is called once per frame
    void Update()
    {
        SetObject();
        CheckCanon();
    }

    private float To180(float eulerangle)
    {
        if (eulerangle > 180)
        {
            eulerangle -= 360;
        }

        return eulerangle;
    }

    private void SetObject()
    {
        float LeverRotateSpeed = 2 * LeverAngle / SwtichingTime;
        float CanonRotateSpeed = 180 / SwtichingTime;

        if(CurrentMid == MiddleState.Neutral)
        {
            if (PreviousMid == MiddleState.Neutral)
            {
                Lever.transform.localEulerAngles = Vector3.zero;
                CanonObject.transform.localEulerAngles = new Vector3(0, 0, -45);
            }
            else if(PreviousMid == MiddleState.Red)
            {
                Lever.transform.Rotate(new Vector3(1, 0, 0), -LeverRotateSpeed * Time.deltaTime);

                Lever.transform.localEulerAngles = new Vector3(Mathf.Clamp(To180(Lever.transform.localEulerAngles.x), 0, 45), 0, 0);

                CanonObject.transform.Rotate(new Vector3(-45, 0, 0), Space.World);
                CanonObject.transform.Rotate(new Vector3(0, 1, 0), CanonRotateSpeed * Time.deltaTime);
                CanonObject.transform.localEulerAngles = new Vector3(0, Mathf.Clamp(To180(CanonObject.transform.localEulerAngles.y), -90, 0),0);
                CanonObject.transform.Rotate(new Vector3(45, 0, 0), Space.World);
            }
            else
            {
                Lever.transform.Rotate(new Vector3(1, 0, 0), LeverRotateSpeed * Time.deltaTime);

                Lever.transform.localEulerAngles = new Vector3(Mathf.Clamp(To180(Lever.transform.localEulerAngles.x), -45, 0), 0, 0);

                CanonObject.transform.Rotate(new Vector3(-45, 0, 0), Space.World);
                CanonObject.transform.Rotate(new Vector3(0, 1, 0), -CanonRotateSpeed * Time.deltaTime);
                CanonObject.transform.localEulerAngles = new Vector3(0, Mathf.Clamp(To180(CanonObject.transform.localEulerAngles.y), 0, 90), 0);
                CanonObject.transform.Rotate(new Vector3(45, 0, 0), Space.World);
            }
        }
        else if(CurrentMid == MiddleState.Red)
        {
            if (PreviousMid == MiddleState.Neutral)
            {
                Lever.transform.Rotate(new Vector3(1, 0, 0), LeverRotateSpeed * Time.deltaTime);
                Lever.transform.localEulerAngles = new Vector3(Mathf.Clamp(To180(Lever.transform.localEulerAngles.x), 0, 45), 0, 0);

                CanonObject.transform.Rotate(new Vector3(-45, 0, 0), Space.World);
                CanonObject.transform.Rotate(new Vector3(0, 1, 0), -CanonRotateSpeed * Time.deltaTime);

                CanonObject.transform.localEulerAngles = new Vector3(0, Mathf.Clamp(To180(CanonObject.transform.localEulerAngles.y), -90, 0), 0);
                CanonObject.transform.Rotate(new Vector3(45, 0, 0), Space.World);
            }
            else if (PreviousMid == MiddleState.Red)
            {
                Lever.transform.localEulerAngles = new Vector3(0, 0, 45);

                CanonObject.transform.Rotate(new Vector3(-45, 0, 0), Space.World);

                CanonObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                CanonObject.transform.Rotate(new Vector3(0, 1, 0), -90);
                CanonObject.transform.Rotate(new Vector3(45, 0, 0), Space.World);
            }
            else
            {

                Lever.transform.Rotate(new Vector3(1, 0, 0), LeverRotateSpeed * Time.deltaTime);

                Lever.transform.localEulerAngles = new Vector3(Mathf.Clamp(To180(Lever.transform.localEulerAngles.x), 0, 45), 0, 0);

                CanonObject.transform.Rotate(new Vector3(-45, 0, 0), Space.World);
                CanonObject.transform.Rotate(new Vector3(0, 1, 0), -CanonRotateSpeed * Time.deltaTime);

                CanonObject.transform.localEulerAngles = new Vector3(0, Mathf.Clamp(To180(CanonObject.transform.localEulerAngles.y), -90, 0), 0);
                CanonObject.transform.Rotate(new Vector3(45, 0, 0), Space.World);
            }
        }
        else
        {

            if (PreviousMid == MiddleState.Neutral)
            {
                Lever.transform.Rotate(new Vector3( 0, 0, -LeverRotateSpeed * Time.deltaTime), Space.World);

                Lever.transform.localEulerAngles = new Vector3(Mathf.Clamp(To180(Lever.transform.localEulerAngles.x), -45, 0), 0, 0);

                CanonObject.transform.Rotate(new Vector3(-45, 0, 0), Space.World);
                CanonObject.transform.Rotate(new Vector3(0, 1, 0), CanonRotateSpeed * Time.deltaTime);
                CanonObject.transform.localEulerAngles = new Vector3(0, Mathf.Clamp(To180(CanonObject.transform.localEulerAngles.y), 0, 90), 0);

                CanonObject.transform.Rotate(new Vector3(45, 0, 0), Space.World);
            }
            else if (PreviousMid == MiddleState.Red)
            {
                Lever.transform.Rotate(new Vector3(1, 0, 0), -LeverRotateSpeed * Time.deltaTime);

                Lever.transform.localEulerAngles = new Vector3(Mathf.Clamp(To180(Lever.transform.localEulerAngles.x), -45, 0), 0, 0);

                CanonObject.transform.Rotate(new Vector3(-45, 0, 0), Space.World);
                CanonObject.transform.Rotate(new Vector3(0, 1, 0), CanonRotateSpeed * Time.deltaTime);
                CanonObject.transform.localEulerAngles = new Vector3(0, Mathf.Clamp(To180(CanonObject.transform.localEulerAngles.y), 0, 90), 0);

                CanonObject.transform.Rotate(new Vector3(45, 0, 0), Space.World);
            }
            else
            {
                Lever.transform.localEulerAngles = new Vector3(-45, 0, 0);

                CanonObject.transform.Rotate(new Vector3(-45, 0, 0), Space.World);

                CanonObject.transform.localEulerAngles = new Vector3(0, 0, 0);
                CanonObject.transform.Rotate(new Vector3(0, 1, 0), 90);
                CanonObject.transform.Rotate(new Vector3(45, 0, 0), Space.World);


            }
        }
    }

    private void CheckCanon()
    {
        if(Team1CanonInfo.State != CanonState.Unactivated)
        {
            if(Team1CanonInfo.State == CanonState.Cooldown)
            {
                CanonCooldown(Team1CanonInfo);
            }
            else
            {
                CanonFiring(Team1CanonInfo);
            }
        }
        else if(Team2CanonInfo.State != CanonState.Unactivated)
        {
            if (Team2CanonInfo.State == CanonState.Cooldown)
            {
                CanonCooldown(Team2CanonInfo);
            }
            else
            {
                CanonFiring(Team2CanonInfo);
            }
        }
    }

    private void CanonCooldown(CanonInfo Info)
    {
        Info.Timer += Time.deltaTime;

        Info.CooldownMark.GetComponent<Image>().fillAmount = Info.Timer / Data.CanonCooldown;

        if(Info.Timer >= Data.CanonCooldown)
        {
            if (LockPlayer(Info))
            {
                Info.TransitionTo(CanonState.Firing);
            }
            else
            {
                Destroy(Info.Mark);
            }
        }

    }

    private void CanonFiring(CanonInfo Info)
    {
        Info.Timer += Time.deltaTime;
        if(Info.Timer >= Data.CanonFireTime)
        {
            BagelFall(Info);
            Info.FireCount++;
            if (Info.FireCount >= Data.MaxCanonFireCount)
            {
                Info.TransitionTo(CanonState.Unactivated);
                PreviousMid = CurrentMid;
                CurrentMid = MiddleState.Neutral;
            }
            else
            {
                Info.TransitionTo(CanonState.Cooldown);
            }

        }
        else if(Info.Timer >= Data.CanonFireTime - Data.CanonFireAlertTime)
        {
            Info.Mark.GetComponent<SpriteRenderer>().color = MarkAlertColor;
        }

        MarkFollow(Info);
    }

    private void MarkFollow(CanonInfo Info)
    {
        Vector3 Offset = Info.LockedPlayer.transform.position - Info.Mark.transform.position;
        Offset.y = 0;
        float Dis = Offset.magnitude;

        if (Dis >= 0.01f)
        {
            if (Dis >= Data.FollowSpeed * Time.deltaTime)
            {
                Info.Mark.transform.position += Data.FollowSpeed * Time.deltaTime * Offset.normalized;
            }
            else
            {
                Info.Mark.transform.position += Dis * Offset.normalized;
            }
        }
    }

    private bool LockPlayer(CanonInfo Info)
    {
        List<GameObject> AvailablePlayer = new List<GameObject>();
        List<Vector3> AvailablePoint = new List<Vector3>();

        foreach (Transform child in Players.transform)
        {
            if (child.GetComponent<PlayerController>().enabled && (child.tag.Contains("1") && Info.Entity == Team2Canon || child.tag.Contains("2") && Info.Entity == Team1Canon))
            {
                RaycastHit hit;
                if (Physics.Raycast(child.position, Vector3.down, out hit, 5, GroundLayer))
                {
                    if (hit.collider != null)
                    {
                        AvailablePlayer.Add(child.gameObject);
                        AvailablePoint.Add(hit.point);

                    }
                }
            }

        }

        if (AvailablePlayer.Count > 0)
        {
            int index = Random.Range(0, AvailablePlayer.Count);

            GameObject Mark = GameObject.Instantiate(MarkPrefab);
            Mark.GetComponent<SpriteRenderer>().color = MarkDefaultColor;
            Mark.transform.position = AvailablePoint[index] + Vector3.up * 0.01f;

            Info.Mark = Mark;
            Info.LockedPlayer = AvailablePlayer[index];

            return true;
        }
        else
        {
            return false;
        }
    }

    private void BagelFall(CanonInfo Info)
    {


        RaycastHit[] AllHits = Physics.SphereCastAll(Info.Mark.transform.position, Data.CanonRadius, Vector3.up, 0, PlayerLayer);

        List<GameObject> HitPlayer = new List<GameObject>();

        for (int i = 0; i < AllHits.Length; i++)
        {
            GameObject Player;

            if (AllHits[i].collider.gameObject.GetComponent<PlayerController>())
            {
                Player = AllHits[i].collider.gameObject;
            }
            else if (AllHits[i].collider.gameObject.GetComponentInParent<PlayerController>())
            {
                Player = AllHits[i].collider.gameObject.GetComponentInParent<PlayerController>().gameObject;
            }
            else
            {
                Player = AllHits[i].collider.gameObject.GetComponent<WeaponBase>().Owner;
            }

            if (!HitPlayer.Contains(Player))
            {
                HitPlayer.Add(Player);
                Vector3 Offset = Player.transform.position - Info.Mark.transform.position;
                Offset.y = 0;

                if (Mathf.Abs(Offset.x) < 0.01f && Mathf.Abs(Offset.z) < 0.01f)
                {
                    float angle = Random.Range(0, 2 * Mathf.PI);

                    Offset.x = Mathf.Sin(angle);
                    Offset.z = Mathf.Cos(angle);
                }

                Player.GetComponent<IHittable>().OnImpact(Data.CanonPower * Offset.normalized, ForceMode.Impulse, Player, ImpactType.BazookaGun);
            }
        }

        if (Bagel == null)
        {
            GenerateBagel();
        }
        GameObject.Instantiate(ExplosionVFX, Info.Mark.transform.position, ExplosionVFX.transform.rotation);
        Destroy(Info.Mark);
    }

    private void GenerateBagel()
    {
        Vector3 Pos = BagelGenerationPos;

        switch (CurrentMid)
        {
            case MiddleState.Neutral:
                Pos = BagelGenerationPos;
                break;
            case MiddleState.Blue:
                Pos = BagelGenerationPosLeft;
                break;
            case MiddleState.Red:
                Pos = BagelGenerationPosRight;
                break;
        }

        Bagel = GameObject.Instantiate(BagelPrefab, Pos, new Quaternion(0, 0, 0, 0));
    }

    private void OnBagelDespawn(BagelDespawn e)
    {
        GenerateBagel();
    }

    private void OnBagelSent(BagelSent e)
    {
        Bagel = null;
        if(e.Canon == Team1Canon)
        {
            PreviousMid = CurrentMid;
            CurrentMid = MiddleState.Red;

            if (Team1CanonInfo.State == CanonState.Unactivated)
            {
                Team1CanonInfo.TransitionTo(CanonState.Cooldown);
                Team1CanonInfo.Timer = Data.CanonCooldown;
                Team2CanonInfo.TransitionTo(CanonState.Unactivated);
                Destroy(Team2CanonInfo.Mark);
            }
        }
        else
        {
            PreviousMid = CurrentMid;
            CurrentMid = MiddleState.Blue;

            if (Team2CanonInfo.State == CanonState.Unactivated)
            {
                Team2CanonInfo.TransitionTo(CanonState.Cooldown);
                Team2CanonInfo.Timer = Data.CanonCooldown;
                Team1CanonInfo.TransitionTo(CanonState.Unactivated);
                Destroy(Team1CanonInfo.Mark);
            }
        }
    }



    private void OnPlayerDied(PlayerDied e)
    {
        if(e.Player == Team1CanonInfo.LockedPlayer && Team1CanonInfo.State == CanonState.Firing)
        {
            Destroy(Team1CanonInfo.Mark);
            Team1CanonInfo.TransitionTo(CanonState.Cooldown);
        }
        else if(e.Player == Team2CanonInfo.LockedPlayer && Team2CanonInfo.State == CanonState.Firing)
        {
            Destroy(Team2CanonInfo.Mark);
            Team2CanonInfo.TransitionTo(CanonState.Cooldown);
        }
    }
}
