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
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log(this.GetType().Name);
    }

    protected bool CheckDelivery()
    {
        if (Context.DeliveryEvent != null)
        {
            if (Context.DeliveryEvent.Basket == BrawlModeReforgedArenaManager.Team1Basket && Context.Info.CurrentSide != CanonSide.Red)
            {
                Context.Info.LastSide = Context.Info.CurrentSide;
                Context.Info.CurrentSide = CanonSide.Red;

                Context.DeliveryEvent = null;

                TransitionTo<CanonSwtich>();

                return true;
            }
            else if (Context.DeliveryEvent.Basket == BrawlModeReforgedArenaManager.Team2Basket && Context.Info.CurrentSide != CanonSide.Blue)
            {
                Context.Info.LastSide = Context.Info.CurrentSide;
                Context.Info.CurrentSide = CanonSide.Blue;

                Context.DeliveryEvent = null;

                TransitionTo<CanonSwtich>();

                return true;
            }
        }

        return false;
    }

    protected void MarkFollow(bool Normal)
    {
        if (Context.Info.LockedPlayer == null)
        {
            return;
        }

        Vector3 Offset = Context.Info.LockedPlayer.transform.position - Context.Info.Mark.transform.position;
        Offset.y = 0;

        float FollowSpeed;
        if (Normal)
        {
            FollowSpeed = Context.Data.NormalFollowSpeed;
        }
        else
        {
            FollowSpeed = Context.Data.AlertFollowSpeed;
        }

        float Dis = Offset.magnitude;

        if (Dis >= 0.01f)
        {
            if (Dis >= FollowSpeed * Time.deltaTime)
            {
                Context.Info.Mark.transform.position += FollowSpeed * Time.deltaTime * Offset.normalized;
            }
            else
            {
                Context.Info.Mark.transform.position += Dis * Offset.normalized;
            }
        }
    }

    protected bool LockPlayer()
    {
        List<GameObject> AvailablePlayer = new List<GameObject>();
        List<Vector3> AvailablePoint = new List<Vector3>();

        foreach (Transform child in Context.Players.transform)
        {
            if (child.GetComponent<PlayerController>().enabled && (child.tag.Contains("1") && Context.Info.CurrentSide == CanonSide.Blue || child.tag.Contains("2") && Context.Info.CurrentSide == CanonSide.Red))
            {
                RaycastHit hit;
                if (Physics.Raycast(child.position, Vector3.down, out hit, 5, Context.GroundLayer))
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

            GameObject Mark = GameObject.Instantiate(Context.MarkPrefab);
            Mark.GetComponent<SpriteRenderer>().color = Context.FeelData.MarkDefaultColor;
            Mark.transform.position = AvailablePoint[index] + Vector3.up * 0.01f;

            Context.Info.Mark = Mark;
            Context.Info.LockedPlayer = AvailablePlayer[index];

            return true;
        }
        else
        {
            return false;
        }
    }

    protected void AimingSetCanon()
    {
        Vector3 Offset = Context.Info.Mark.transform.position - Context.Info.Entity.transform.position;
        Offset.y = 0;

        float TargetPercentage;
        float TargetAngle;

        TargetAngle = Vector3.SignedAngle(Offset, Vector3.back, Vector3.up);

        float Dis = Mathf.Clamp(Offset.magnitude, Context.FeelData.MinShootDisShape, Context.FeelData.MaxShootDisShape);

        TargetPercentage = (Dis - Context.FeelData.MinShootDisShape) / (Context.FeelData.MaxShootDisShape - Context.FeelData.MinShootDisShape);
        TargetPercentage = Mathf.Lerp(Context.FeelData.MinPercentage, Context.FeelData.MaxPercentage,TargetPercentage);

        SetCanon(TargetPercentage, TargetAngle);

    }

    protected void FireSetCanon()
    {
        Vector3 Offset = Context.Info.Mark.transform.position - Context.Info.Entity.transform.position;
        Offset.y = 0;
        float TargetAngle;
        TargetAngle = Vector3.SignedAngle(Offset, Vector3.back, Vector3.up);

        SetCanon(0, TargetAngle);
    }

    protected void SetCanon(float TargetPercentage, float TargetAngle)
    {


        Context.Info.CurrentPercentage = Mathf.Lerp(Context.Info.CurrentPercentage, TargetPercentage, Context.FeelData.PercentageFollowSpeed*Time.deltaTime);
        if (Mathf.Abs(Context.Info.CurrentPercentage - TargetPercentage) <= Context.FeelData.PercentageIgnoreError)
        {
            Context.Info.CurrentPercentage = TargetPercentage;
        }

        Context.Info.Pad.transform.localPosition = Vector3.forward * Mathf.Lerp(Context.FeelData.MinPadDis, Context.FeelData.MaxPadDis, Context.Info.CurrentPercentage);

        SoftJointLimit JointLimit = new SoftJointLimit();
        JointLimit.limit = Mathf.Lerp(Context.FeelData.MinLinearLimit, Context.FeelData.MaxLinearLimit, Context.Info.CurrentPercentage);

        Debug.Log(JointLimit.limit);

        Context.Info.LeftJoint.GetComponent<ConfigurableJoint>().linearLimit = JointLimit;
        Context.Info.RightJoint.GetComponent<ConfigurableJoint>().linearLimit = JointLimit;

        if (Mathf.Abs(TargetAngle - Context.Info.CurrentAngle) <= Context.FeelData.RotateSpeed * Time.deltaTime)
        {
            Context.Info.Entity.transform.Rotate(Vector3.up * (TargetAngle - Context.Info.CurrentAngle), Space.Self);
            Context.Info.CurrentAngle = TargetAngle;
        }
        else
        {
            if (TargetAngle > Context.Info.CurrentAngle)
            {
                Context.Info.Entity.transform.Rotate(Vector3.down * Context.FeelData.RotateSpeed * Time.deltaTime, Space.Self);
                Context.Info.CurrentAngle += Context.FeelData.RotateSpeed * Time.deltaTime;
            }
            else
            {
                Context.Info.Entity.transform.Rotate(Vector3.up * Context.FeelData.RotateSpeed*Time.deltaTime, Space.Self);
                Context.Info.CurrentAngle -= Context.FeelData.RotateSpeed * Time.deltaTime;
            }
        }
    }
}

public class CanonIdle: CanonAction
{
    public override void Update()
    {
        base.Update();
        SetCanon(0, 0);
        CheckDelivery();


    }

}

public class CanonSwtich : CanonAction
{
    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;

        if (!Services.GameStateManager.CameraTargets.Contains(Context.Info.CameraFocus.transform))
        {
            Services.GameStateManager.CameraTargets.Add(Context.Info.CameraFocus.transform);
        }

    }


    public override void Update()
    {
        base.Update();
        Timer += Time.deltaTime;
        SetCanon(0,0);
        SetCabel();
        CheckTimer();

    }

    private void CheckTimer()
    {
        if (Timer >= Context.Data.CanonSwitchTime)
        {
            TransitionTo<CanonFiring_Normal>();
        }
    }

    private void SetCabel()
    {
        switch (Context.Info.CurrentSide)
        {
            case CanonSide.Neutral:
                if(Context.Info.LastSide == CanonSide.Red)
                {
                    CabelChange(Context.Team1Cabel, false, true);
                }
                else
                {
                    CabelChange(Context.Team2Cabel, false, false);
                }
                break;
            case CanonSide.Red:
                CabelChange(Context.Team1Cabel, true, true);
                if (Context.Info.LastSide == CanonSide.Blue)
                {
                    CabelChange(Context.Team2Cabel, false, false);
                }
                break;
            case CanonSide.Blue:
                CabelChange(Context.Team2Cabel, true, false);
                if (Context.Info.LastSide == CanonSide.Red)
                {
                    CabelChange(Context.Team1Cabel, false, true);
                }
                break;
        }
    }


    private void CabelChange(GameObject Cabel,bool Shine,bool Team1)
    {

        foreach (Transform child in Cabel.transform)
        {
            Material mat = child.GetComponent<Renderer>().material;
            mat.EnableKeyword("_EMISSION");

            Color color;

            float Emission;

            if (Shine)
            {
                Emission = Mathf.Lerp(Context.FeelData.CabelStartEmission, Context.FeelData.CabelEndEmission, Timer / Context.FeelData.CabelShineTime);
            }
            else
            {
                Emission = Mathf.Lerp(Context.FeelData.CabelEndEmission, Context.FeelData.CabelStartEmission, Timer / Context.FeelData.CabelShineTime);
            }

            if (Team1)
            {
                color = Context.FeelData.RedCabelColor;
            }
            else
            {
                color = Context.FeelData.BlueCabelColor;
            }

            mat.SetColor("_EmissionColor", color * Emission);
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
        Services.GameStateManager.CameraTargets.Remove(Context.Info.CameraFocus.transform);
    }

    public override void Update()
    {
        base.Update();

        SetCanon(0,0);

        if (CheckDelivery())
        {
            return;
        }
        CheckTimer();

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
    private float Timer;
    private bool PlayerLocked;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;

        if (!Services.GameStateManager.CameraTargets.Contains(Context.Info.CameraFocus.transform))
        {
            Services.GameStateManager.CameraTargets.Add(Context.Info.CameraFocus.transform);
        }
    }

    public override void Update()
    {
        base.Update();

        if (CheckDelivery())
        {
            return;
        }

        if (PlayerLocked)
        {
            AimingSetCanon();

            Debug.Log(Context.Info.CurrentPercentage);
            Debug.Log(Context.Info.CurrentAngle);

            Color color = Context.Info.Mark.GetComponent<SpriteRenderer>().color;
            Context.Info.Mark.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, Timer / Context.FeelData.MarkAppearTime);


            CheckTimer();
        }
        else
        {
            SetCanon(0, 0);
            if (LockPlayer())
            {
                PlayerLocked = true;
            }
        }



    }

    private void CheckTimer()
    {
        Timer += Time.deltaTime;
        if (Timer >= Context.Data.CanonFireTime-Context.Data.CanonFireAlertTime)
        {
            TransitionTo<CanonFiring_Alert>();
        }
    }
}

public class CanonFiring_Alert : CanonAction
{
    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;
        Context.Info.Mark.GetComponent<SpriteRenderer>().color = Context.FeelData.MarkAlertColor;

        if (!Services.GameStateManager.CameraTargets.Contains(Context.Info.CameraFocus.transform))
        {
            Services.GameStateManager.CameraTargets.Add(Context.Info.CameraFocus.transform);
        }
    }

    public override void Update()
    {
        base.Update();

        AimingSetCanon();

        if (CheckDelivery())
        {
            return;
        }
        CheckTimer();
    }

    private void CheckTimer()
    {
        Timer += Time.deltaTime;
        if (Timer >= Context.Data.CanonFireAlertTime)
        {
            TransitionTo<CanonFiring_Fall>();
        }
    }
}

public class CanonFiring_Fall : CanonAction
{
    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;
        Context.Info.Mark.GetComponent<SpriteRenderer>().color = Color.red;

        if (!Services.GameStateManager.CameraTargets.Contains(Context.Info.CameraFocus.transform))
        {
            Services.GameStateManager.CameraTargets.Add(Context.Info.CameraFocus.transform);
        }
    }

    public override void Update()
    {
        base.Update();

        FireSetCanon();
        CheckTimer();
    }

    private void CheckTimer()
    {
        Timer += Time.deltaTime;
        if (Timer >= Context.Data.CanonFireFinalTime)
        {
            BombFall();
            TransitionTo<CanonCooldown>();
        }
    }

    private void BombFall()
    {

        RaycastHit[] AllHits = Physics.SphereCastAll(Context.Info.Mark.transform.position, Context.Data.CanonRadius, Vector3.up, 0, Context.PlayerLayer);

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
                Vector3 Offset = Player.transform.position - Context.Info.Mark.transform.position;
                Offset.y = 0;

                if (Mathf.Abs(Offset.x) < 0.01f && Mathf.Abs(Offset.z) < 0.01f)
                {
                    float angle = Random.Range(0, 2 * Mathf.PI);

                    Offset.x = Mathf.Sin(angle);
                    Offset.z = Mathf.Cos(angle);
                }

                Player.GetComponent<IHittable>().OnImpact(Context.Data.CanonPower * Offset.normalized, ForceMode.Impulse, Player, ImpactType.BazookaGun);
            }
        }

        GameObject.Instantiate(Context.ExplosionVFX, Context.Info.Mark.transform.position, Context.ExplosionVFX.transform.rotation);
        GameObject.Destroy(Context.Info.Mark);
    }
}

public class BrawlModeReforgedArenaManager : MonoBehaviour
{
    public class CanonInfo
    {
        public GameObject Entity;
        public GameObject Pad;
        public GameObject LeftJoint;
        public GameObject RightJoint;
        public GameObject CameraFocus;

        public float CurrentPercentage;
        public float CurrentAngle;

        public CanonState State;
        public CanonSide CurrentSide;
        public CanonSide LastSide;
        public float Timer;
        public int FireCount;

        public GameObject Mark;
        public GameObject LockedPlayer;

        public float PipeAngle;

        public bool ReadyToFire;

        public Vector3 MarkDirection;


        public CanonInfo(GameObject entity, GameObject pad, GameObject LJoint, GameObject RJoint, GameObject Focus)
        {
            Entity = entity;
            Pad = pad;
            LeftJoint = LJoint;
            RightJoint = RJoint;
            CameraFocus = Focus;

            State = CanonState.Unactivated;
            CurrentSide = LastSide = CanonSide.Neutral;

            FireCount = 0;
            Timer = 0;

            Mark = null;
            LockedPlayer = null;

            PipeAngle = 0;

            ReadyToFire = false;
        }
        
    }

    public static GameObject Team1Basket;
    public static GameObject Team2Basket;

    public GameObject Basket1;
    public GameObject Basket2;

    public BrawlModeReforgedModeData Data_2Player;
    public BrawlModeReforgedModeData Data_MorePlayer;

    public CanonFeelData FeelData;

    public GameObject CanonEntity;
    public GameObject CanonPad;
    public GameObject CanonLJoint;
    public GameObject CanonRJoint;

    public GameObject CameraFocus;

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

    private FSM<BrawlModeReforgedArenaManager> CanonFSM;

    // Start is called before the first frame update
    void Start()
    {
        CanonFSM = new FSM<BrawlModeReforgedArenaManager>(this);
        CanonFSM.TransitionTo<CanonIdle>();

        if (Utility.GetPlayerNumber() <= 2)
        {
            Data = Data_2Player;
        }
        else
        {
            Data = Data_MorePlayer;
        }

        Team1Basket = Basket1;
        Team2Basket = Basket2;

        Info = new CanonInfo(CanonEntity,CanonPad,CanonLJoint,CanonRJoint,CameraFocus);

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
        CanonFSM.Update();
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

                Info.LockedPlayer = null;
                Destroy(Info.Mark);

                DeliveryEvent = e;
            }
        }
        else
        {
            if (Info.CurrentSide != CanonSide.Blue)
            {

                Info.LockedPlayer = null;
                Destroy(Info.Mark);

                DeliveryEvent = e;

            }
        }
    }



    private void OnPlayerDied(PlayerDied e)
    {
        if(e.Player == Info.LockedPlayer)
        {
            Info.LockedPlayer = null;
        }

        if (Bagel == null)
        {
            GenerateBagel();
        }
    }
}
