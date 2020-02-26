using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CanonState
{
    Unactivated,
    Cooldown,
    Swtiching,
    Firing
}

public enum CanonSide
{
    Neutral,
    Red,
    Blue
}

public class BrawlModeReforgedArenaManager : MonoBehaviour
{
    private class CanonInfo
    {
        public GameObject Pipe;
        public GameObject Lever;

        public CanonState State;
        public CanonSide CurrentSide;
        public CanonSide LastSide;
        public float Timer;
        public int FireCount;

        public GameObject Rocket;
        public GameObject Mark;
        public GameObject LockedPlayer;

        private float PipeAngle;
        private float LeverAngle;

        public bool ReadyToFire;


        public CanonInfo(GameObject pipe, GameObject lever)
        {
            Pipe = pipe;
            Lever = lever;
            State = CanonState.Unactivated;
            CurrentSide = LastSide = CanonSide.Neutral;

            FireCount = 0;
            Timer = 0;

            Mark = null;
            LockedPlayer = null;

            PipeAngle = 0;
            LeverAngle = 0;

            ReadyToFire = false;
        }

        public void TransitionTo(CanonState state)
        {
            State = state;
            Timer = 0;

            switch (State)
            {
                case CanonState.Unactivated:
                    FireCount = 0;
                    break;
                case CanonState.Cooldown:
                    break;
                case CanonState.Swtiching:
                    break;
                case CanonState.Firing:
                    break;
            }
        }



        public void SetCanon(float MaxPipeAngle, float MaxLeverAngle, float PipeRotateSpeed, float LeverRotateSpeed)
        {
            ReadyToFire = false;

            switch (CurrentSide)
            {
                case CanonSide.Neutral:

                    switch (LastSide)
                    {
                        case CanonSide.Neutral:
                            Lever.transform.Rotate(new Vector3(-LeverAngle, 0, 0));
                            Pipe.transform.Rotate(new Vector3(0, -PipeAngle, 0));

                            LeverAngle = 0;
                            PipeAngle = 0;
                            break;
                        case CanonSide.Blue:
                            if (LeverAngle < 0)
                            {
                                LeverAngle += LeverRotateSpeed * Time.deltaTime;
                                Lever.transform.Rotate(new Vector3(LeverRotateSpeed * Time.deltaTime, 0, 0));
                            }
                            else
                            {
                                Lever.transform.Rotate(new Vector3(-LeverAngle, 0, 0));
                                LeverAngle = 0;
                            }
                            if (PipeAngle > 0)
                            {
                                PipeAngle += -PipeRotateSpeed * Time.deltaTime;
                                Pipe.transform.Rotate(new Vector3(0, -PipeRotateSpeed * Time.deltaTime, 0));
                            }
                            else
                            {
                                Pipe.transform.Rotate(new Vector3(0, -PipeAngle, 0));
                                PipeAngle = 0;
                            }
                            break;
                        case CanonSide.Red:
                            if (LeverAngle > 0)
                            {
                                LeverAngle += -LeverRotateSpeed * Time.deltaTime;
                                Lever.transform.Rotate(new Vector3(-LeverRotateSpeed * Time.deltaTime, 0, 0));
                            }
                            else
                            {
                                Lever.transform.Rotate(new Vector3(-LeverAngle, 0, 0));
                                LeverAngle = 0;
                            }
                            if (PipeAngle < 0)
                            {
                                PipeAngle += PipeRotateSpeed * Time.deltaTime;
                                Pipe.transform.Rotate(new Vector3(0, PipeRotateSpeed * Time.deltaTime, 0));
                            }
                            else
                            {
                                Pipe.transform.Rotate(new Vector3(0, -PipeAngle, 0));
                                PipeAngle = 0;
                            }
                            break;
                    }
                    break;
                case CanonSide.Blue:
                    switch (LastSide)
                    {
                        case CanonSide.Neutral:
                            if(LeverAngle > -MaxLeverAngle)
                            {
                                LeverAngle += -LeverRotateSpeed * Time.deltaTime;
                                Lever.transform.Rotate(new Vector3(-LeverRotateSpeed * Time.deltaTime, 0, 0));
                            }
                            else
                            {
                                Lever.transform.Rotate(new Vector3(-MaxLeverAngle - LeverAngle, 0, 0));
                                LeverAngle = -MaxLeverAngle;

                                ReadyToFire = true;
                            }
                            if(PipeAngle < MaxPipeAngle)
                            {
                                PipeAngle += PipeRotateSpeed * Time.deltaTime;
                                Pipe.transform.Rotate(new Vector3(0, PipeRotateSpeed * Time.deltaTime, 0));
                            }
                            else
                            {
                                Pipe.transform.Rotate(new Vector3(0, MaxPipeAngle - PipeAngle, 0));
                                PipeAngle = MaxPipeAngle;

                                ReadyToFire = true;
                            }
                            break;
                        case CanonSide.Blue:
                            Lever.transform.Rotate(new Vector3(-MaxLeverAngle - LeverAngle, 0, 0));
                            Pipe.transform.Rotate(new Vector3(0, MaxPipeAngle - PipeAngle, 0));

                            LeverAngle = -MaxLeverAngle;
                            PipeAngle = MaxPipeAngle;

                            ReadyToFire = true;
                            break;
                        case CanonSide.Red:
                            if (LeverAngle > -MaxLeverAngle)
                            {
                                LeverAngle += -LeverRotateSpeed * Time.deltaTime;
                                Lever.transform.Rotate(new Vector3(-LeverRotateSpeed * Time.deltaTime, 0, 0));
                            }
                            else
                            {
                                Lever.transform.Rotate(new Vector3(-MaxLeverAngle - LeverAngle, 0, 0));
                                LeverAngle = -MaxLeverAngle;

                                ReadyToFire = true;

                            }
                            if (PipeAngle < MaxPipeAngle)
                            {
                                PipeAngle += PipeRotateSpeed * Time.deltaTime;
                                Pipe.transform.Rotate(new Vector3(0, PipeRotateSpeed * Time.deltaTime, 0));
                            }
                            else
                            {
                                Pipe.transform.Rotate(new Vector3(0, MaxPipeAngle - PipeAngle, 0));
                                PipeAngle = MaxPipeAngle;

                                ReadyToFire = true;
                            }
                            break;
                    }
                    break;
                case CanonSide.Red:
                    switch (LastSide)
                    {
                        case CanonSide.Neutral:
                            if (LeverAngle < MaxLeverAngle)
                            {
                                LeverAngle += LeverRotateSpeed * Time.deltaTime;
                                Lever.transform.Rotate(new Vector3(LeverRotateSpeed * Time.deltaTime, 0, 0));
                            }
                            else
                            {
                                Lever.transform.Rotate(new Vector3(MaxLeverAngle - LeverAngle, 0, 0));
                                LeverAngle = MaxLeverAngle;

                                ReadyToFire = true;
                            }
                            if (PipeAngle > -MaxPipeAngle)
                            {
                                PipeAngle += -PipeRotateSpeed * Time.deltaTime;
                                Pipe.transform.Rotate(new Vector3(0, -PipeRotateSpeed * Time.deltaTime, 0));
                            }
                            else
                            {
                                Pipe.transform.Rotate(new Vector3(0, -MaxPipeAngle - PipeAngle, 0));
                                PipeAngle = -MaxPipeAngle;

                                ReadyToFire = true;
                            }
                            break;
                        case CanonSide.Blue:
                            if (LeverAngle < MaxLeverAngle)
                            {
                                LeverAngle += LeverRotateSpeed * Time.deltaTime;
                                Lever.transform.Rotate(new Vector3(LeverRotateSpeed * Time.deltaTime, 0, 0));
                            }
                            else
                            {
                                Lever.transform.Rotate(new Vector3(MaxLeverAngle - LeverAngle, 0, 0));
                                LeverAngle = MaxLeverAngle;

                                ReadyToFire = true;
                            }
                            if (PipeAngle > -MaxPipeAngle)
                            {
                                PipeAngle += -PipeRotateSpeed * Time.deltaTime;
                                Pipe.transform.Rotate(new Vector3(0, -PipeRotateSpeed * Time.deltaTime, 0));
                            }
                            else
                            {
                                Pipe.transform.Rotate(new Vector3(0, -MaxPipeAngle - PipeAngle, 0));
                                PipeAngle = -MaxPipeAngle;

                                ReadyToFire = true;
                            }
                            break;
                        case CanonSide.Red:
                            Lever.transform.Rotate(new Vector3(MaxLeverAngle - LeverAngle, 0, 0));
                            Pipe.transform.Rotate(new Vector3(0, -MaxPipeAngle - PipeAngle, 0));

                            LeverAngle = MaxLeverAngle;
                            PipeAngle = -MaxPipeAngle;

                            ReadyToFire = true;
                            break;
                    }
                    break;
            }
        }
    }



    public BrawlModeReforgedModeData Data_2Player;
    public BrawlModeReforgedModeData Data_MorePlayer;

    public CanonFeelData FeelData;

    public GameObject PipeObject;
    public GameObject PipeEndObject;
    public GameObject LeverObject;

    public GameObject Team1Basket;
    public GameObject Team2Basket;

    public LayerMask PlayerLayer;
    public LayerMask GroundLayer;
    public GameObject Players;

    public GameObject BagelPrefab;
    public GameObject MarkPrefab;
    public GameObject ExplosionVFX;

    public Vector3 BagelGenerationPos;
    public Vector3 BagelGenerationPosLeft;
    public Vector3 BagelGenerationPosRight;

    private BrawlModeReforgedModeData Data;

    private CanonInfo Info;
    private GameObject Bagel;

    // Start is called before the first frame update
    void Start()
    {
        if (Utility.GetPlayerNumber() <= 2)
        {
            Data = Data_2Player;
        }
        else
        {
            Data = Data_MorePlayer;
        }

        Info = new CanonInfo(PipeObject, LeverObject);

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
        CheckCanon();
    }


    private int GetPlayerNumber()
    {
        int Count = 0;
        for(int i=0;i < Services.GameStateManager.CameraTargets.Count; i++)
        {
            if (Services.GameStateManager.CameraTargets[i].GetComponent<PlayerController>())
            {
                Count++;
            }
        }

        return Count;

    }

    private void CheckCanon()
    {
        Info.SetCanon(FeelData.MaxPipeAngle, FeelData.MaxLeverAngle, FeelData.PipeRotateSpeed, FeelData.LeverRotateSpeed);
        switch (Info.State)
        {
            case CanonState.Swtiching:
                if (Info.ReadyToFire && LockPlayer(Info))
                {
                    StopAllCoroutines();
                    Destroy(Info.Rocket);
                    StartCoroutine(CanonFire());

                    Info.TransitionTo(CanonState.Firing);
                }
                break;
            case CanonState.Firing:
                CanonFiring(Info);
                break;
            case CanonState.Cooldown:
                CanonCooldown(Info);
                break;
        }
    }

    private void CanonCooldown(CanonInfo Info)
    {
        Info.Timer += Time.deltaTime;

        if(Info.Timer >= Data.CanonCooldown)
        {
            if (LockPlayer(Info))
            {
                StopAllCoroutines();
                Destroy(Info.Rocket);
                StartCoroutine(CanonFire());

                Info.TransitionTo(CanonState.Firing);
            }
            else
            {
                Destroy(Info.Mark);
            }
        }

    }

    private IEnumerator CanonFire()
    {
        float Timer = 0;

        PipeEndObject.transform.localPosition = FeelData.PipeEndStartLocalPos;

        StartCoroutine(RocketFire());

        while(Timer < FeelData.PipeEndFireTime)
        {
            Timer += Time.deltaTime;
            PipeEndObject.transform.localPosition = Vector3.Lerp(FeelData.PipeEndStartLocalPos, FeelData.PipeEndStartLocalPos + Vector3.down * FeelData.PipeEndShakeDis, Timer / FeelData.PipeEndFireTime);
            yield return null;
        }

        Timer = 0;

        while (Timer < FeelData.PipeEndRecoverTime)
        {
            Timer += Time.deltaTime;
            PipeEndObject.transform.localPosition = Vector3.Lerp(FeelData.PipeEndStartLocalPos + Vector3.down * FeelData.PipeEndShakeDis, FeelData.PipeEndStartLocalPos, Timer / FeelData.PipeEndRecoverTime);
            yield return null;
        }
    }

    private IEnumerator RocketFire()
    {
        Info.Rocket = GameObject.Instantiate(FeelData.BombPrefab);
        Info.Rocket.transform.position = PipeEndObject.transform.position;

        float Timer = 0;

        Vector3 Start = Info.Rocket.transform.position;

        Vector3 Target = Info.Rocket.transform.position + (FeelData.BombInvisibleHeight - Info.Rocket.transform.position.y) / Info.Rocket.transform.up.y * Info.Rocket.transform.up;

        while (Timer < FeelData.BombRiseTime)
        {
            Timer += Time.deltaTime;

            Info.Rocket.transform.position = Vector3.Lerp(Start, Target, Timer / FeelData.BombRiseTime);

            yield return null;
        }
    }

    private void CanonFiring(CanonInfo Info)
    {
        Info.Timer += Time.deltaTime;
        if(Info.Timer >= Data.CanonFireTime)
        {
            BombFall(Info);
            Info.FireCount++;
            if (Info.FireCount >= Data.MaxCanonFireCount)
            {
                Info.TransitionTo(CanonState.Unactivated);
                Info.LastSide = Info.CurrentSide;
                Info.CurrentSide = CanonSide.Neutral;
            }
            else
            {
                Info.TransitionTo(CanonState.Cooldown);
            }

            return;
        }
        else if(Info.Timer >= Data.CanonFireTime - FeelData.BombFallTime)
        {
            Info.Rocket.transform.up = Vector3.down;
            Info.Rocket.transform.position += Vector3.down * (FeelData.BombInvisibleHeight - Info.Mark.transform.position.y)/FeelData.BombFallTime*Time.deltaTime;
        }
        else if(Info.Timer >= Data.CanonFireTime - Data.CanonFireAlertTime)
        {
            Info.Mark.GetComponent<SpriteRenderer>().color = FeelData.MarkAlertColor;
        }
        else
        {
            Info.Mark.GetComponent<SpriteRenderer>().color = new Color(FeelData.MarkDefaultColor.r, FeelData.MarkDefaultColor.g, FeelData.MarkDefaultColor.b, Info.Timer / FeelData.MarkAppearTime);
        }

        MarkFollow(Info);
    }

    private void MarkFollow(CanonInfo Info)
    {
        if(Info.LockedPlayer == null)
        {
            return;
        }

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

        Info.Rocket.transform.position = new Vector3(Info.Mark.transform.position.x, Info.Rocket.transform.position.y, Info.Mark.transform.position.z);
    }

    private bool LockPlayer(CanonInfo Info)
    {
        List<GameObject> AvailablePlayer = new List<GameObject>();
        List<Vector3> AvailablePoint = new List<Vector3>();

        foreach (Transform child in Players.transform)
        {
            if (child.GetComponent<PlayerController>().enabled && (child.tag.Contains("1") && Info.CurrentSide == CanonSide.Blue || child.tag.Contains("2") && Info.CurrentSide == CanonSide.Red))
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
            Mark.GetComponent<SpriteRenderer>().color = FeelData.MarkDefaultColor;
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

    private void BombFall(CanonInfo Info)
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
        Destroy(Info.Rocket);
        Destroy(Info.Mark);
    }

    private void GenerateBagel()
    {
        Vector3 Pos = Data.BagelGenerationPos;

        BrawlModeReforgedObjectiveManager Manager = (BrawlModeReforgedObjectiveManager)Services.GameObjectiveManager;

        if (Manager.TeamAScore > Manager.TeamBScore)
        {
            Pos = Data.BagelGenerationPosRight;
        }
        else if(Manager.TeamBScore > Manager.TeamAScore)
        {
            Pos = Data.BagelGenerationPosLeft;
        }
        /*switch (Info.CurrentSide)
        {
            case CanonSide.Neutral:
                Pos = BagelGenerationPos;
                break;
            case CanonSide.Blue:
                Pos = BagelGenerationPosLeft;
                break;
            case CanonSide.Red:
                Pos = BagelGenerationPosRight;
                break;
        }*/

        Bagel = GameObject.Instantiate(BagelPrefab, Pos, new Quaternion(0, 0, 0, 0));
    }

    private void OnBagelDespawn(BagelDespawn e)
    {
        GenerateBagel();
    }

    private void OnBagelSent(BagelSent e)
    {
        Bagel = null;

        if(e.Basket == Team1Basket)
        {
            if (Info.CurrentSide != CanonSide.Red)
            {
                Info.LastSide = Info.CurrentSide;
                Info.CurrentSide = CanonSide.Red;

                Info.LockedPlayer = null;
                Destroy(Info.Mark);

                Info.TransitionTo(CanonState.Swtiching);

            }
        }
        else
        {
            if (Info.CurrentSide != CanonSide.Blue)
            {
                Info.LastSide = Info.CurrentSide;
                Info.CurrentSide = CanonSide.Blue;

                Info.LockedPlayer = null;
                Destroy(Info.Mark);

                Info.TransitionTo(CanonState.Swtiching);

            }
        }
    }



    private void OnPlayerDied(PlayerDied e)
    {
        if(e.Player == Info.LockedPlayer && Info.State == CanonState.Firing)
        {
            Info.LockedPlayer = null;
        }
    }
}
