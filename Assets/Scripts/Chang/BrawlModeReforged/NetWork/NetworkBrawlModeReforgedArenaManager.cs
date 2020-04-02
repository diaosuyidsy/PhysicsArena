using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public abstract class NetworkCanonAction : FSM<NetworkBrawlModeReforgedArenaManager>.State
{
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log(this.GetType().Name);
    }

    protected bool CheckDelivery() // Check if there is an available delivery for switch
    {
        if (Context.DeliveryEvent != null)
        {
            if (Context.DeliveryEvent.Basket == NetworkBrawlModeReforgedArenaManager.Team1Basket && Context.Info.CurrentSide != CanonSide.Red)
            {
                Context.Info.LastSide = Context.Info.CurrentSide;
                Context.Info.CurrentSide = CanonSide.Red;
                Context.Info.LockedPlayer = null;

                Context.DeliveryEvent = null;


                TransitionTo<NetworkCanonSwtich>();

                return true;
            }
            else if (Context.DeliveryEvent.Basket == NetworkBrawlModeReforgedArenaManager.Team2Basket && Context.Info.CurrentSide != CanonSide.Blue)
            {
                Context.Info.LastSide = Context.Info.CurrentSide;
                Context.Info.CurrentSide = CanonSide.Blue;
                Context.Info.LockedPlayer = null;

                Context.DeliveryEvent = null;

                TransitionTo<NetworkCanonSwtich>();

                return true;
            }
        }

        return false;
    }

    protected void MarkFollow(bool Normal) // Bomb area follows target
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

    protected bool LockPlayer() // Try to lock a target for bomb
    {
        List<GameObject> AvailablePlayer = new List<GameObject>();
        List<Vector3> AvailablePoint = new List<Vector3>();

        foreach (Transform child in Context.Players.transform)
        {
            if (child.GetComponent<PlayerControllerMirror>().enabled && (child.tag.Contains("1") && Context.Info.CurrentSide == CanonSide.Blue || child.tag.Contains("2") && Context.Info.CurrentSide == CanonSide.Red))
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

            NetworkServer.Spawn(Mark);
            Context.RpcSetMark(Mark);

            return true;
        }
        else
        {
            return false;
        }
    }

    protected void AimingSetCanon() // Set the shape and transform of canon object and ammo 
    {
        Vector3 Offset = Context.Info.Mark.transform.position - Context.Info.Entity.transform.position;
        Offset.y = 0;

        float TargetPercentage;
        float TargetAngle;

        TargetAngle = Vector3.SignedAngle(Offset, Vector3.back, Vector3.up);

        float Dis = Mathf.Clamp(Offset.magnitude, Context.FeelData.MinShootDisShape, Context.FeelData.MaxShootDisShape);

        TargetPercentage = Mathf.Lerp(0, Context.FeelData.MaxPercentage, Dis / Context.FeelData.MaxShootDisShape);

        SetCanon(TargetPercentage, TargetAngle, Context.FeelData.AimingPercentageFollowSpeed);
        Context.RpcSetCanon(TargetPercentage, TargetAngle, Context.FeelData.AimingPercentageFollowSpeed);
    }

    protected void FireSetCanon() // Set the shape and transform of canon object and ammo 
    {
        Vector3 Offset = Context.Info.Mark.transform.position - Context.Info.Entity.transform.position;
        Offset.y = 0;
        float TargetAngle;
        TargetAngle = Vector3.SignedAngle(Offset, Vector3.back, Vector3.up);

        SetCanon(0, TargetAngle, Context.FeelData.ShootPercentageFollowSpeed);
        Context.RpcSetCanon(0, TargetAngle, Context.FeelData.ShootPercentageFollowSpeed);
    }

    protected void SetCanon(float TargetPercentage, float TargetAngle, float PercentageFollowSpeed) // Set Canon transform and shape
    {

        Context.Info.CurrentPercentage = Mathf.Lerp(Context.Info.CurrentPercentage, TargetPercentage, PercentageFollowSpeed * Time.deltaTime);
        if (Mathf.Abs(Context.Info.CurrentPercentage - TargetPercentage) <= Context.FeelData.PercentageIgnoreError)
        {
            Context.Info.CurrentPercentage = TargetPercentage;
        }

        Context.Info.Pad.transform.localPosition = Vector3.forward * Mathf.Lerp(Context.FeelData.MinPadDis, Context.FeelData.MaxPadDis, Context.Info.CurrentPercentage);

        SoftJointLimit JointLimit = new SoftJointLimit();
        JointLimit.limit = Mathf.Lerp(Context.FeelData.MinLinearLimit, Context.FeelData.MaxLinearLimit, Context.Info.CurrentPercentage);

        Context.Info.LJoint0.GetComponent<ConfigurableJoint>().linearLimit = JointLimit;
        Context.Info.RJoint0.GetComponent<ConfigurableJoint>().linearLimit = JointLimit;

        Context.Info.LeftJoint.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(0, -Mathf.Lerp(Context.FeelData.MinJointRotation, Context.FeelData.MaxJointRotation, Context.Info.CurrentPercentage), 0, 1);
        Context.Info.RightJoint.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(0, Mathf.Lerp(Context.FeelData.MinJointRotation, Context.FeelData.MaxJointRotation, Context.Info.CurrentPercentage), 0, 1);

        JointDrive Joint1Drive = new JointDrive();
        Joint1Drive.positionSpring = Mathf.Lerp(Context.FeelData.MinJoint1AngularYZ, Context.FeelData.MaxJoint1AngularYZ, Context.Info.CurrentPercentage);
        Joint1Drive.maximumForce = float.PositiveInfinity;

        Context.Info.LeftJoint.GetComponent<ConfigurableJoint>().angularYZDrive = Joint1Drive;
        Context.Info.RightJoint.GetComponent<ConfigurableJoint>().angularYZDrive = Joint1Drive;

        JointDrive Joint0Drive = new JointDrive();
        Joint0Drive.positionSpring = Mathf.Lerp(Context.FeelData.MinJoint0AngularYZ, Context.FeelData.MaxJoint0AngularYZ, Context.Info.CurrentPercentage);
        Joint0Drive.maximumForce = float.PositiveInfinity;

        Context.Info.LJoint0.GetComponent<ConfigurableJoint>().angularYZDrive = Joint0Drive;
        Context.Info.RJoint0.GetComponent<ConfigurableJoint>().angularYZDrive = Joint0Drive;

        if (Mathf.Abs(TargetAngle - Context.Info.CurrentAngle) <= Context.FeelData.RotateSpeed * Time.deltaTime)
        {
            Context.Info.Entity.transform.Rotate(Vector3.down * (TargetAngle - Context.Info.CurrentAngle), Space.Self);
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
                Context.Info.Entity.transform.Rotate(Vector3.up * Context.FeelData.RotateSpeed * Time.deltaTime, Space.Self);
                Context.Info.CurrentAngle -= Context.FeelData.RotateSpeed * Time.deltaTime;
            }
        }
    }


}

public class NetworkCanonIdle : NetworkCanonAction
{
    public override void Update()
    {
        base.Update();
        //SetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);
        //Context.RpcSetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);
        CheckDelivery();
    }

}

public class NetworkCanonSwtich : NetworkCanonAction // Canon switches side
{
    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;

        Context.RpcCameraAddRemove(true);

    }


    public override void Update()
    {
        base.Update();
        Timer += Time.deltaTime;
        SetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);
        Context.RpcSetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);

        SetCabel();
        CheckTimer();

    }

    private void CheckTimer()
    {
        if (Timer >= Context.Data.CanonSwitchTime)
        {
            TransitionTo<NetworkCanonFiring_Normal>();
        }
    }

    private void SetCabel() // Set cabel appearance
    {
        switch (Context.Info.CurrentSide)
        {
            case CanonSide.Neutral:
                if (Context.Info.LastSide == CanonSide.Red)
                {
                    CabelChange(Context.Team1Cabel, false, true);
                    Context.RpcCabelChange(Context.Team1Cabel, false, true,Timer);
                }
                else
                {
                    CabelChange(Context.Team2Cabel, false, false);
                    Context.RpcCabelChange(Context.Team2Cabel, false, false, Timer);
                }
                break;
            case CanonSide.Red:
                CabelChange(Context.Team1Cabel, true, true);
                Context.RpcCabelChange(Context.Team1Cabel, true, true, Timer);
                if (Context.Info.LastSide == CanonSide.Blue)
                {
                    CabelChange(Context.Team2Cabel, false, false);
                    Context.RpcCabelChange(Context.Team2Cabel, false, false, Timer);
                }
                break;
            case CanonSide.Blue:
                CabelChange(Context.Team2Cabel, true, false);
                Context.RpcCabelChange(Context.Team2Cabel, true, false, Timer);
                if (Context.Info.LastSide == CanonSide.Red)
                {
                    CabelChange(Context.Team1Cabel, false, true);
                    Context.RpcCabelChange(Context.Team1Cabel, false, true, Timer);
                }
                break;
        }
    }


    private void CabelChange(GameObject Cabel, bool Shine, bool Team1)
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


public class NetworkCanonCooldown : NetworkCanonAction
{
    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;

        Context.RpcCameraAddRemove(false);
    }

    public override void Update()
    {
        base.Update();

        SetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);
        Context.RpcSetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);

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
            TransitionTo<NetworkCanonFiring_Normal>();
        }
    }
}

public class NetworkCanonFiring_Normal : NetworkCanonAction // Lock and follow player (white mark)
{
    private float Timer;
    private bool PlayerLocked;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;
        PlayerLocked = false;

        Context.RpcCameraAddRemove(true);
    }

    public override void Update()
    {
        base.Update();

        if (CheckDelivery())
        {
            NetworkServer.UnSpawn(Context.Info.Bomb);
            GameObject.Destroy(Context.Info.Bomb);

            NetworkServer.UnSpawn(Context.Info.Mark);
            GameObject.Destroy(Context.Info.Mark);

            return;
        }

        if (PlayerLocked)
        {
            AimingSetCanon();
            MarkFollow(true);

            Color color = Context.Info.Mark.GetComponent<SpriteRenderer>().color;
            Context.Info.Mark.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, Timer / Context.FeelData.MarkAppearTime);

            Context.RpcSetMarkColor(new Vector3(color.r, color.g, color.b), Timer / Context.FeelData.MarkAppearTime);

            if (Context.Info.LockedPlayer == null)
            {
                TransitionTo<NetworkCanonFiring_Fall>();
                return;
            }

            CheckTimer();
        }
        else
        {
            SetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);
            Context.RpcSetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);

            if (LockPlayer())
            {
                GameObject Bomb = GameObject.Instantiate(Context.BombPrefab);

                Context.Info.Bomb = Bomb;
                Context.Info.Bomb.transform.parent = Context.CanonPad.transform;
                Context.Info.Bomb.transform.localPosition = Vector3.back * Context.FeelData.AmmoOffset;

                NetworkServer.Spawn(Bomb);
                Context.RpcSetAmmo(Bomb);

                AimingSetCanon();

                PlayerLocked = true;
            }
        }
    }

    private void CheckTimer()
    {
        Timer += Time.deltaTime;
        if (Timer >= Context.Data.CanonFireTime)
        {
            TransitionTo<NetworkCanonFiring_Alert>();
        }
    }
}

public class NetworkCanonFiring_Alert : NetworkCanonAction // Purple mark follows target
{
    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;
        Context.Info.Mark.GetComponent<SpriteRenderer>().color = Context.FeelData.MarkAlertColor;
        Context.RpcSetMarkColor(new Vector3(Context.FeelData.MarkAlertColor.r, Context.FeelData.MarkAlertColor.g, Context.FeelData.MarkAlertColor.b), Context.FeelData.MarkAlertColor.a);

        Context.RpcCameraAddRemove(true);
    }

    public override void Update()
    {
        base.Update();

        AimingSetCanon();
        MarkFollow(false);

        if (Context.Info.LockedPlayer == null)
        {

            TransitionTo<NetworkCanonFiring_Fall>();
            return;
        }

        if (CheckDelivery())
        {
            NetworkServer.UnSpawn(Context.Info.Bomb);
            GameObject.Destroy(Context.Info.Bomb);

            NetworkServer.UnSpawn(Context.Info.Mark);
            GameObject.Destroy(Context.Info.Mark);

            return;
        }
        CheckTimer();
    }

    private void CheckTimer()
    {
        Timer += Time.deltaTime;
        if (Timer >= Context.Data.CanonFireAlertTime)
        {
            TransitionTo<NetworkCanonFiring_Fall>();
        }
    }
}

public class NetworkCanonFiring_Fall : NetworkCanonAction // Shoot ammo
{
    private float Timer;
    private float HorizontalSpeed;
    private Vector3 HorizontalDirection;
    private float VerticalSpeed;
    private float VerticalSpeedAcceleration;

    private bool InfoGot;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;
        InfoGot = false;

        

        GetBombFlyInfo();


        Context.Info.Mark.GetComponent<SpriteRenderer>().color = Context.FeelData.MarkFallColor;
        Context.RpcSetMarkColor(new Vector3(Context.FeelData.MarkFallColor.r, Context.FeelData.MarkFallColor.g, Context.FeelData.MarkFallColor.b), Context.FeelData.MarkFallColor.a);

        Context.RpcCameraAddRemove(true);
    }

    public override void Update()
    {
        base.Update();

        FireSetCanon();
        if (Context.Info.CurrentPercentage == 0)
        {
            Context.Info.Bomb.transform.parent = null;

            Context.RpcAmmoRelease();

            if (!InfoGot)
            {
                InfoGot = true;
                GetBombFlyInfo();
            }
            BombFly();
        }


        CheckTimer();
    }

    private void CheckTimer()
    {
        Timer += Time.deltaTime;
        if (Timer >= Context.Data.CanonFireFinalTime)
        {
            BombFall();

            if (CheckDelivery())
            {
                return;
            }

            TransitionTo<NetworkCanonCooldown>();
        }
    }

    private void GetBombFlyInfo() // Calculate how ammo should fly
    {
        float time = Context.Data.CanonFireFinalTime - Timer;

        Vector3 Offset = Context.Info.Mark.transform.position - Context.Info.Bomb.transform.position;

        HorizontalDirection = Offset;
        HorizontalDirection.y = 0;

        HorizontalSpeed = HorizontalDirection.magnitude / time;
        HorizontalDirection.Normalize();

        VerticalSpeed = Context.FeelData.BombVerticalSpeedPercentage * HorizontalSpeed;
        VerticalSpeedAcceleration = 2 * (Mathf.Abs(Offset.y) + VerticalSpeed * time) / Mathf.Pow(time, 2);

    }

    private void BombFly()
    {
        Context.Info.Bomb.transform.position += HorizontalDirection * HorizontalSpeed * Time.deltaTime + Vector3.up * VerticalSpeed * Time.deltaTime;
        VerticalSpeed -= VerticalSpeedAcceleration * Time.deltaTime;
    }

    private void BombFall() //Impact
    {

        RaycastHit[] AllHits = Physics.SphereCastAll(Context.Info.Mark.transform.position, Context.Data.CanonRadius, Vector3.up, 0, Context.PlayerLayer);

        List<GameObject> HitPlayer = new List<GameObject>();

        for (int i = 0; i < AllHits.Length; i++)
        {
            GameObject Player;

            if (AllHits[i].collider.gameObject.GetComponent<PlayerControllerMirror>())
            {
                Player = AllHits[i].collider.gameObject;
            }
            else if (AllHits[i].collider.gameObject.GetComponentInParent<PlayerControllerMirror>())
            {
                Player = AllHits[i].collider.gameObject.GetComponentInParent<PlayerControllerMirror>().gameObject;
            }
            else
            {
                Player = AllHits[i].collider.gameObject.GetComponent<NetworkWeaponBase>().Owner;
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

                Context.RpcGetImpact(Player, Offset);
            }
        }

        Context.Info.LockedPlayer = null;

        GameObject VFX = GameObject.Instantiate(Context.ExplosionVFX, Context.Info.Mark.transform.position, Context.ExplosionVFX.transform.rotation);
        NetworkServer.UnSpawn(Context.Info.Bomb);
        NetworkServer.UnSpawn(Context.Info.Mark);
        GameObject.Destroy(Context.Info.Bomb);
        GameObject.Destroy(Context.Info.Mark);

        NetworkServer.Spawn(VFX);


    }
}

public class NetworkBrawlModeReforgedArenaManager : NetworkBehaviour
{
    public class CanonInfo
    {
        public GameObject Entity;
        public GameObject Pad;
        public GameObject LeftJoint; //Joint1
        public GameObject RightJoint; //Joint1
        public GameObject CameraFocus;

        public GameObject LJoint0;
        public GameObject RJoint0;

        public float CurrentPercentage;
        public float CurrentAngle;

        public CanonSide CurrentSide;
        public CanonSide LastSide;
        public float Timer;
        public int FireCount;

        public GameObject Bomb;

        public GameObject Mark;
        public GameObject LockedPlayer;

        public bool CameraFollowed;


        public CanonInfo(GameObject entity, GameObject pad, GameObject LJoint, GameObject RJoint, GameObject L0, GameObject R0, GameObject Focus)
        {
            Entity = entity;
            Pad = pad;
            LeftJoint = LJoint;
            RightJoint = RJoint;
            CameraFocus = Focus;
            LJoint0 = L0;
            RJoint0 = R0;

            CurrentSide = LastSide = CanonSide.Neutral;

            FireCount = 0;
            Timer = 0;

            Mark = null;
            LockedPlayer = null;

            CameraFollowed = false;

        }

    }

    public static GameObject Team1Basket;
    public static GameObject Team2Basket;

    public GameObject Basket1;
    public GameObject Basket2;

    public BrawlModeReforgedModeData Data_2Player;
    public BrawlModeReforgedModeData Data_MorePlayer;

    public NetworkBrawlModeReforgedObjectiveManager ObjectiveManager;

    public CanonFeelData FeelData;

    public GameObject CanonEntity;
    public GameObject CanonPad;
    public GameObject CanonLJoint;
    public GameObject CanonRJoint;

    public GameObject CanonFinalLJoint;
    public GameObject CanonFinalRJoint;
    public GameObject CanonLJoint0;
    public GameObject CanonRJoint0;

    public GameObject CameraFocus;

    public GameObject Team1Cabel;
    public GameObject Team2Cabel;

    public LayerMask PlayerLayer;
    public LayerMask GroundLayer;
    public GameObject Players;

    public GameObject BagelPrefab;
    public GameObject MarkPrefab;
    public GameObject BombPrefab;
    public GameObject ExplosionVFX;

    public BrawlModeReforgedModeData Data;

    public CanonInfo Info;

    public BagelSent DeliveryEvent;
    private GameObject Bagel;

    private FSM<NetworkBrawlModeReforgedArenaManager> CanonFSM;

    // Start is called before the first frame update
    void Start()
    {
        Info = new CanonInfo(CanonEntity, CanonPad, CanonLJoint, CanonRJoint, CanonLJoint0, CanonRJoint0, CameraFocus);

        CanonFSM = new FSM<NetworkBrawlModeReforgedArenaManager>(this);
        CanonFSM.TransitionTo<NetworkCanonIdle>();

        if (Players.transform.childCount <= 2)
        {
            Data = Data_2Player;
        }
        else
        {
            Data = Data_MorePlayer;
        }

        Team1Basket = Basket1;
        Team2Basket = Basket2;



        if (isServer)
        {
            GenerateBagel();
        }

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
        if (!isServer)
        {
            return;
        }

        CanonFSM.Update();
    }



    private void GenerateBagel()
    {
        Vector3 Pos = Data.BagelGenerationPos;

        NetworkBrawlModeReforgedObjectiveManager Manager = ObjectiveManager;

        if (Manager.TeamAScore - Manager.TeamBScore >= Data.DeliveryPoint)
        {
            Pos = Data.BagelGenerationPosRight;
        }
        else if (Manager.TeamBScore - Manager.TeamAScore >= Data.DeliveryPoint)
        {
            Pos = Data.BagelGenerationPosLeft;
        }

        Bagel = GameObject.Instantiate(BagelPrefab, Pos, new Quaternion(0, 0, 0, 0));

        NetworkServer.Spawn(Bagel);
    }

    private void OnBagelDespawn(BagelDespawn e)
    {
        if (!isServer)
        {
            return;
        }
        GenerateBagel();
    }

    private void OnBagelSent(BagelSent e)
    {

        if (!isServer)
        {
            return;
        }

        Bagel = null;


        if (e.Basket == Team1Basket)
        {
            if (Info.CurrentSide != CanonSide.Red)
            {
                DeliveryEvent = e;
            }
        }
        else
        {
            if (Info.CurrentSide != CanonSide.Blue)
            {
                DeliveryEvent = e;

            }
        }
    }

    private void OnPlayerDied(PlayerDied e)
    {
        if (!isServer)
        {
            return;
        }

        if (e.Player == Info.LockedPlayer)
        {
            Info.LockedPlayer = null;
        }

        if (Bagel == null)
        {
            GenerateBagel();
        }
    }

    [ClientRpc]
    public void RpcSetCanon(float TargetPercentage, float TargetAngle, float PercentageFollowSpeed)
    {
        Info.CurrentPercentage = Mathf.Lerp(Info.CurrentPercentage, TargetPercentage, PercentageFollowSpeed * Time.deltaTime);
        if (Mathf.Abs(Info.CurrentPercentage - TargetPercentage) <= FeelData.PercentageIgnoreError)
        {
            Info.CurrentPercentage = TargetPercentage;
        }

        Info.Pad.transform.localPosition = Vector3.forward * Mathf.Lerp(FeelData.MinPadDis, FeelData.MaxPadDis, Info.CurrentPercentage);

        SoftJointLimit JointLimit = new SoftJointLimit();
        JointLimit.limit = Mathf.Lerp(FeelData.MinLinearLimit, FeelData.MaxLinearLimit, Info.CurrentPercentage);

        Info.LJoint0.GetComponent<ConfigurableJoint>().linearLimit = JointLimit;
        Info.RJoint0.GetComponent<ConfigurableJoint>().linearLimit = JointLimit;

        Info.LeftJoint.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(0, -Mathf.Lerp(FeelData.MinJointRotation, FeelData.MaxJointRotation, Info.CurrentPercentage), 0, 1);
        Info.RightJoint.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(0, Mathf.Lerp(FeelData.MinJointRotation, FeelData.MaxJointRotation, Info.CurrentPercentage), 0, 1);

        JointDrive Joint1Drive = new JointDrive();
        Joint1Drive.positionSpring = Mathf.Lerp(FeelData.MinJoint1AngularYZ, FeelData.MaxJoint1AngularYZ, Info.CurrentPercentage);
        Joint1Drive.maximumForce = float.PositiveInfinity;

        Info.LeftJoint.GetComponent<ConfigurableJoint>().angularYZDrive = Joint1Drive;
        Info.RightJoint.GetComponent<ConfigurableJoint>().angularYZDrive = Joint1Drive;

        JointDrive Joint0Drive = new JointDrive();
        Joint0Drive.positionSpring = Mathf.Lerp(FeelData.MinJoint0AngularYZ, FeelData.MaxJoint0AngularYZ, Info.CurrentPercentage);
        Joint0Drive.maximumForce = float.PositiveInfinity;

        Info.LJoint0.GetComponent<ConfigurableJoint>().angularYZDrive = Joint0Drive;
        Info.RJoint0.GetComponent<ConfigurableJoint>().angularYZDrive = Joint0Drive;

        if (Mathf.Abs(TargetAngle - Info.CurrentAngle) <= FeelData.RotateSpeed * Time.deltaTime)
        {
            Info.Entity.transform.Rotate(Vector3.down * (TargetAngle - Info.CurrentAngle), Space.Self);
            Info.CurrentAngle = TargetAngle;
        }
        else
        {
            if (TargetAngle > Info.CurrentAngle)
            {
                Info.Entity.transform.Rotate(Vector3.down * FeelData.RotateSpeed * Time.deltaTime, Space.Self);
                Info.CurrentAngle += FeelData.RotateSpeed * Time.deltaTime;
            }
            else
            {
                Info.Entity.transform.Rotate(Vector3.up * FeelData.RotateSpeed * Time.deltaTime, Space.Self);
                Info.CurrentAngle -= FeelData.RotateSpeed * Time.deltaTime;
            }
        }
    }

    [ClientRpc]
    public void RpcCameraAddRemove(bool Add)
    {

        if (Add)
        {
            if (!Info.CameraFollowed)
            {
                Info.CameraFollowed = true;
                EventManager.Instance.TriggerEvent(new OnAddCameraTargets(Info.CameraFocus, 1));
            }
        }
        else
        {
            if (Info.CameraFollowed)
            {
                Info.CameraFollowed = false;
                EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(Info.CameraFocus));
            }

        }
    }

    [ClientRpc]
    public void RpcSetMark(GameObject obj)
    {
        Info.Mark = obj;
    }

    [ClientRpc]
    public void RpcSetAmmo(GameObject obj)
    {
        Info.Bomb = obj;
        Info.Bomb.transform.localScale = Vector3.one * 0.5f;
        //Info.Bomb.transform.parent = CanonPad.transform;
    }

    [ClientRpc]
    public void RpcAmmoRelease()
    {
        Info.Bomb.transform.parent = null;
    }

    [ClientRpc]
    public void RpcSetMarkColor(Vector3 color, float alpha)
    {
        Info.Mark.GetComponent<SpriteRenderer>().color = new Color(color.x, color.y, color.z, alpha);
    }

    [ClientRpc]
    public void RpcCabelChange(GameObject Cabel, bool Shine, bool Team1,float Timer)
    {
        foreach (Transform child in Cabel.transform)
        {
            Material mat = child.GetComponent<Renderer>().material;
            mat.EnableKeyword("_EMISSION");

            Color color;

            float Emission;

            if (Shine)
            {
                Emission = Mathf.Lerp(FeelData.CabelStartEmission, FeelData.CabelEndEmission, Timer / FeelData.CabelShineTime);
            }
            else
            {
                Emission = Mathf.Lerp(FeelData.CabelEndEmission, FeelData.CabelStartEmission, Timer / FeelData.CabelShineTime);
            }

            if (Team1)
            {
                color = FeelData.RedCabelColor;
            }
            else
            {
                color = FeelData.BlueCabelColor;
            }

            mat.SetColor("_EmissionColor", color * Emission);
        }
    }

    [ClientRpc]
    public void RpcGetImpact(GameObject Player, Vector3 Offset)
    {
        Player.GetComponent<IHittableNetwork>().OnImpact(Data.CanonPower * Offset.normalized, ForceMode.Impulse, Player, ImpactType.BazookaGun);
    }
}
