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

public abstract class CanonAction : FSM<BrawlModeReforgedArenaManager>.State
{

}

public class CanonIdle: CanonAction
{
    public override void Update()
    {
        base.Update();
        CheckDelivery();
    }

    private void CheckDelivery()
    {
        if (Context.DeliveryEvent != null)
        {
            Context.Info.LastSide = Context.Info.CurrentSide;

            if(Context.DeliveryEvent.Basket == BrawlModeReforgedArenaManager.Team1Basket)
            {
                Context.Info.CurrentSide = CanonSide.Red;
            }
            else
            {
                Context.Info.CurrentSide = CanonSide.Blue;
            }

            TransitionTo<CanonSwtich>();

            Context.DeliveryEvent = null;

            return;
        }
    }
}

public class CanonSwtich : CanonAction
{
    private float Timer;

    private Material RedCabelMat;
    private Material BlueCabelMat;

    public override void OnEnter()
    {
        base.OnEnter();


    }


    public override void Update()
    {
        base.Update();

        SetCanonPipe(Context.FeelData.MaxPipeAngle, Context.FeelData.PipeRotateSpeed);
    }


    /*private void CabelChange(GameObject Cabel,bool Shine)
    {
        foreach (Transform child in Cabel.transform)
        {
            Material mat = child.GetComponent<Renderer>().material;
            mat.EnableKeyword("_EMISSION");

            float Emission;

            if (Shine)
            {
                Emission = Mathf.Lerp(FeelData.CabelStartEmission, FeelData.CabelEndEmission, Timer / FeelData.CabelShineTime);
            }
            else
            {
                Emission = Mathf.Lerp(FeelData.CabelEndEmission, FeelData.CabelStartEmission, Timer / FeelData.CabelShineTime);
            }


            mat.SetColor("_EmissionColor", color * Emission);
        }
    }*/

    private void SetCabel(GameObject RedCabel, GameObject BlueCabel)
    {


        Timer += Time.deltaTime;

        switch (Context.Info.CurrentSide)
        {
            case CanonSide.Neutral:
                switch (Context.Info.LastSide)
                {
                    case CanonSide.Red:


                        break;
                    case CanonSide.Blue:
                        break;
                }
                break;
            case CanonSide.Red:
                break;
            case CanonSide.Blue:
                break;
        }
    }

    private void SetCanonPipe(float MaxPipeAngle, float PipeRotateSpeed)
    {
        switch (Context.Info.CurrentSide)
        {
            case CanonSide.Neutral:

                switch (Context.Info.LastSide)
                {
                    case CanonSide.Blue:
                        if (Context.Info.PipeAngle < 0)
                        {
                            Context.Info.PipeAngle += PipeRotateSpeed * Time.deltaTime;
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, PipeRotateSpeed * Time.deltaTime, 0));
                        }
                        else
                        {
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, -Context.Info.PipeAngle, 0));
                            Context.Info.PipeAngle = 0;

                            TransitionTo<CanonIdle>();
                        }
                        break;
                    case CanonSide.Red:

                        if (Context.Info.PipeAngle > 0)
                        {
                            Context.Info.PipeAngle += -PipeRotateSpeed * Time.deltaTime;
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, -PipeRotateSpeed * Time.deltaTime, 0));
                        }
                        else
                        {
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, -Context.Info.PipeAngle, 0));
                            Context.Info.PipeAngle = 0;

                            TransitionTo<CanonIdle>();
                        }
                        break;
                }
                break;
            case CanonSide.Blue:
                switch (Context.Info.LastSide)
                {
                    case CanonSide.Neutral:

                        if (Context.Info.PipeAngle > -MaxPipeAngle)
                        {
                            Context.Info.PipeAngle += -PipeRotateSpeed * Time.deltaTime;
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, -PipeRotateSpeed * Time.deltaTime, 0));
                        }
                        else
                        {
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, -MaxPipeAngle - Context.Info.PipeAngle, 0));
                            Context.Info.PipeAngle = -MaxPipeAngle;

                            TransitionTo<CanonFiring_Normal>();
                        }
                        break;
                    case CanonSide.Red:
                        if (Context.Info.PipeAngle > -MaxPipeAngle)
                        {
                            Context.Info.PipeAngle += -PipeRotateSpeed * Time.deltaTime;
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, -PipeRotateSpeed * Time.deltaTime, 0));
                        }
                        else
                        {
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, -MaxPipeAngle - Context.Info.PipeAngle, 0));
                            Context.Info.PipeAngle = -MaxPipeAngle;

                            TransitionTo<CanonFiring_Normal>();
                        }
                        break;
                }
                break;
            case CanonSide.Red:
                switch (Context.Info.LastSide)
                {
                    case CanonSide.Neutral:

                        if (Context.Info.PipeAngle < MaxPipeAngle)
                        {
                            Context.Info.PipeAngle += PipeRotateSpeed * Time.deltaTime;
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, PipeRotateSpeed * Time.deltaTime, 0));
                        }
                        else
                        {
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, MaxPipeAngle - Context.Info.PipeAngle, 0));
                            Context.Info.PipeAngle = MaxPipeAngle;

                            TransitionTo<CanonFiring_Normal>();
                        }
                        break;
                    case CanonSide.Blue:

                        if (Context.Info.PipeAngle < MaxPipeAngle)
                        {
                            Context.Info.PipeAngle += PipeRotateSpeed * Time.deltaTime;
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, PipeRotateSpeed * Time.deltaTime, 0));
                        }
                        else
                        {
                            Context.Info.Pipe.transform.Rotate(new Vector3(0, MaxPipeAngle - Context.Info.PipeAngle, 0));
                            Context.Info.PipeAngle = MaxPipeAngle;

                            TransitionTo<CanonFiring_Normal>();
                        }
                        break;
                }
                break;
        }
    }
}

public class CanonCooldown : CanonAction
{
    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;
    }

    public override void Update()
    {
        base.Update();

        CheckDelivery();
        CheckTimer();

    }

    private void CheckDelivery()
    {
        if (Context.DeliveryEvent != null)
        {
            if (Context.DeliveryEvent.Basket == BrawlModeReforgedArenaManager.Team1Basket && Context.Info.CurrentSide != CanonSide.Red)
            {
                Context.Info.LastSide = Context.Info.CurrentSide;
                Context.Info.CurrentSide = CanonSide.Red;

                TransitionTo<CanonSwtich>();
                return;
            }
            else if (Context.DeliveryEvent.Basket == BrawlModeReforgedArenaManager.Team2Basket && Context.Info.CurrentSide != CanonSide.Blue)
            {
                Context.Info.LastSide = Context.Info.CurrentSide;
                Context.Info.CurrentSide = CanonSide.Blue;

                TransitionTo<CanonSwtich>();
                return;
            }

            Context.DeliveryEvent = null;
        }
    }

    private void CheckTimer()
    {
        Timer += Time.deltaTime;
        if (Timer >= Context.Data.CanonCooldown)
        {
            TransitionTo<CanonFiring_Normal>();
        }
    }
}

public class CanonFiring_Normal : CanonAction
{

}

public class CanonFiring_Alert : CanonAction
{

}

public class BrawlModeReforgedArenaManager : MonoBehaviour
{
    public class CanonInfo
    {
        public GameObject Pipe;

        public CanonState State;
        public CanonSide CurrentSide;
        public CanonSide LastSide;
        public float Timer;
        public int FireCount;

        public GameObject Rocket;
        public GameObject Mark;
        public GameObject LockedPlayer;

        public float PipeAngle;

        public bool ReadyToFire;

        public Vector3 MarkDirection;


        public CanonInfo(GameObject pipe)
        {
            Pipe = pipe;
            State = CanonState.Unactivated;
            CurrentSide = LastSide = CanonSide.Neutral;

            FireCount = 0;
            Timer = 0;

            Mark = null;
            LockedPlayer = null;

            PipeAngle = 0;

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



        public void SetCanon(float MaxPipeAngle, float PipeRotateSpeed)
        {
            ReadyToFire = false;

            switch (CurrentSide)
            {
                case CanonSide.Neutral:

                    switch (LastSide)
                    {
                        case CanonSide.Neutral:
                            Pipe.transform.Rotate(new Vector3(0, -PipeAngle, 0));

                            PipeAngle = 0;
                            break;
                        case CanonSide.Blue:
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
                        case CanonSide.Red:

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
                    }
                    break;
                case CanonSide.Blue:
                    switch (LastSide)
                    {
                        case CanonSide.Neutral:

                            if(PipeAngle > -MaxPipeAngle)
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
                            Pipe.transform.Rotate(new Vector3(0, -MaxPipeAngle - PipeAngle, 0));

                            PipeAngle = -MaxPipeAngle;

                            ReadyToFire = true;
                            break;
                        case CanonSide.Red:
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
                    }
                    break;
                case CanonSide.Red:
                    switch (LastSide)
                    {
                        case CanonSide.Neutral:

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
                        case CanonSide.Blue:

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
                        case CanonSide.Red:
                            Pipe.transform.Rotate(new Vector3(0, MaxPipeAngle - PipeAngle, 0));

                            PipeAngle = MaxPipeAngle;

                            ReadyToFire = true;
                            break;
                    }
                    break;
            }
        }
    }

    public static GameObject Team1Basket;
    public static GameObject Team2Basket;

    public BrawlModeReforgedModeData Data_2Player;
    public BrawlModeReforgedModeData Data_MorePlayer;

    public CanonFeelData FeelData;

    public GameObject CanonObject;
    public GameObject PipeObject;
    public GameObject PipeEndObject;
    public GameObject CameraPoint;

    public GameObject Team1Cabel;
    public GameObject Team2Cabel;

    public LayerMask PlayerLayer;
    public LayerMask GroundLayer;
    public GameObject Players;

    public GameObject BagelPrefab;
    public GameObject MarkPrefab;
    public GameObject ExplosionVFX;

    public BrawlModeReforgedModeData Data;

    public CanonInfo Info;

    public BagelSent DeliveryEvent;
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

        Team1Basket = CanonObject.transform.Find("LForceField").gameObject;
        Team2Basket = CanonObject.transform.Find("RForceField").gameObject;


        Info = new CanonInfo(PipeObject);

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
        Info.SetCanon(FeelData.MaxPipeAngle, FeelData.PipeRotateSpeed);
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
        else if(Info.Timer >= Data.CanonCooldown - 2)
        {
            if (!Services.GameStateManager.CameraTargets.Contains(CameraPoint.transform))
            {
                Services.GameStateManager.CameraTargets.Add(CameraPoint.transform);
            }
        }

    }

    private IEnumerator CanonFire()
    {
        float Timer = 0;

        Services.GameStateManager.CameraTargets.Remove(CameraPoint.transform);

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

    private IEnumerator CabelChange(GameObject Cabel,Color color,bool Shine)
    {
        foreach (Transform child in Cabel.transform)
        {
            Material mat = child.GetComponent<Renderer>().material;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * FeelData.CabelStartEmission);
        }


        float Timer = 0;

        while (Timer < FeelData.CabelShineTime)
        {
            Timer += Time.deltaTime;
            foreach (Transform child in Cabel.transform)
            {
                Material mat = child.GetComponent<Renderer>().material;
                mat.EnableKeyword("_EMISSION");

                float Emission;

                if (Shine)
                {
                    Emission = Mathf.Lerp(FeelData.CabelStartEmission, FeelData.CabelEndEmission, Timer / FeelData.CabelShineTime);
                }
                else
                {
                    Emission = Mathf.Lerp(FeelData.CabelEndEmission, FeelData.CabelStartEmission, Timer / FeelData.CabelShineTime);
                }


                mat.SetColor("_EmissionColor", color * Emission);
            }
            yield return null;
        }

        /*while(Timer < FeelData.CabelShineTime / 2)
        {
            Timer += Time.deltaTime;
            foreach (Transform child in Cabel.transform)
            {
                Material mat = child.GetComponent<Renderer>().material;
                mat.EnableKeyword("_EMISSION");

                float Emission = Mathf.Lerp(FeelData.CabelStartEmission, FeelData.CabelEndEmission, Timer / (FeelData.CabelShineTime / 2));

                mat.SetColor("_EmissionColor", color * Emission);
            }
            yield return null;
        }

        while (Timer < FeelData.CabelShineTime)
        {
            Timer += Time.deltaTime;
            foreach (Transform child in Cabel.transform)
            {
                Material mat = child.GetComponent<Renderer>().material;
                mat.EnableKeyword("_EMISSION");

                float Emission = Mathf.Lerp(FeelData.CabelEndEmission, FeelData.CabelStartEmission, (Timer - FeelData.CabelShineTime / 2) / (FeelData.CabelShineTime / 2));

                mat.SetColor("_EmissionColor", color * Emission);
            }
            yield return null;
        }*/
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
                StopAllCoroutines();
                if(Info.CurrentSide == CanonSide.Red)
                {
                    StartCoroutine(CabelChange(Team1Cabel, FeelData.RedCabelColor, false));
                }
                else
                {
                    StartCoroutine(CabelChange(Team2Cabel, FeelData.BlueCabelColor, false));
                }

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
            //MarkFollow(Info,false);
            //MarkDash(Info);
            Info.Mark.GetComponent<SpriteRenderer>().color = Color.red;

        }
        else if(Info.Timer >= Data.CanonFireTime - Data.CanonFireAlertTime)
        {
            Info.Mark.GetComponent<SpriteRenderer>().color = FeelData.MarkAlertColor;
            MarkFollow(Info,false);
            //MarkDash(Info);
        }
        else
        {
            Info.Mark.GetComponent<SpriteRenderer>().color = new Color(FeelData.MarkDefaultColor.r, FeelData.MarkDefaultColor.g, FeelData.MarkDefaultColor.b, Info.Timer / FeelData.MarkAppearTime);
            MarkFollow(Info,true);
        }


    }

    private void MarkDash(CanonInfo Info)
    {
        Info.Mark.transform.position += Data.AlertFollowSpeed * Time.deltaTime * Info.MarkDirection;


    }

    private void MarkFollow(CanonInfo Info, bool Normal)
    {
        if(Info.LockedPlayer == null)
        {
            return;
        }

        Vector3 Offset = Info.LockedPlayer.transform.position - Info.Mark.transform.position;
        Offset.y = 0;

        float FollowSpeed;
        if (Normal)
        {
            FollowSpeed = Data.NormalFollowSpeed;
        }
        else
        {
            FollowSpeed = Data.AlertFollowSpeed;
        }

        float Dis = Offset.magnitude;

        if (Dis >= 0.01f)
        {
            if (Dis >= FollowSpeed* Time.deltaTime)
            {
                Info.Mark.transform.position += FollowSpeed * Time.deltaTime * Offset.normalized;
            }
            else
            {
                Info.Mark.transform.position += Dis * Offset.normalized;
            }
        }

        Info.MarkDirection = Offset.normalized;

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

        GameObject.Instantiate(ExplosionVFX, Info.Mark.transform.position, ExplosionVFX.transform.rotation);
        Destroy(Info.Rocket);
        Destroy(Info.Mark);
    }

    private void GenerateBagel()
    {
        Vector3 Pos = Data.BagelGenerationPos;

        BrawlModeReforgedObjectiveManager Manager = (BrawlModeReforgedObjectiveManager)Services.GameObjectiveManager;

        if (Manager.TeamAScore - Manager.TeamBScore >= Data.DeliveryPoint)
        {
            Pos = Data.BagelGenerationPosRight;
        }
        else if(Manager.TeamBScore - Manager.TeamAScore >= Data.DeliveryPoint)
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

                StartCoroutine(CabelChange(Team1Cabel, FeelData.RedCabelColor,true));

                if (Info.LastSide != CanonSide.Neutral)
                {
                    StartCoroutine(CabelChange(Team2Cabel, FeelData.BlueCabelColor, false));
                }
                if (!Services.GameStateManager.CameraTargets.Contains(CameraPoint.transform))
                {
                    Services.GameStateManager.CameraTargets.Add(CameraPoint.transform);
                }


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

                if(Info.LastSide != CanonSide.Neutral)
                {
                    StartCoroutine(CabelChange(Team1Cabel, FeelData.RedCabelColor, false));
                }

                StartCoroutine(CabelChange(Team2Cabel, FeelData.BlueCabelColor, true));

                if (!Services.GameStateManager.CameraTargets.Contains(CameraPoint.transform))
                {
                    Services.GameStateManager.CameraTargets.Add(CameraPoint.transform);
                }

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

        if (Bagel == null)
        {
            GenerateBagel();
        }
    }
}
