using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CanonState
{
    Idle,
    Cooldown,
    Aiming,
    Firing

}

public enum CabelState
{
    Idle,
    Switching
}

public enum CanonSide
{
    Neutral,
    Red,
    Blue
}

public abstract class CabelAction : FSM<BrawlModeReforgedArenaManager>.State
{
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log(this.GetType().Name);
    }

    protected float CountDistance(List<GameObject> Waypoints)
    {
        float dis = 0;

        for (int i = 0; i < Waypoints.Count - 1; i++)
        {
            dis += (Waypoints[i + 1].transform.position - Waypoints[i].transform.position).magnitude;
        }

        return dis;
    }

    protected void MoveAlongWaypoints(List<GameObject> Waypoints, ref int TargetWaypoint, ref int CurrentWaypoint, GameObject Cart, float Speed, Vector3 Target, Vector3 Start, Vector3 LastDirection)
    {
        if (TargetWaypoint >= Waypoints.Count)
        {
            return;
        }

        Vector3 Direction = Target - Start;
        Direction.Normalize();

        Cart.transform.position += Direction * Speed * Time.deltaTime;

        Direction.y = 0;
        Direction.Normalize();
        LastDirection.y = 0;
        LastDirection.Normalize();



        if (Vector3.Dot(Direction, Cart.transform.position - Waypoints[TargetWaypoint].transform.position) > 0)
        {
            TargetWaypoint++;
            CurrentWaypoint++;

            if (TargetWaypoint >= Waypoints.Count)
            {
                return;
            }

            Vector3 Dir = Waypoints[TargetWaypoint].transform.position - Waypoints[CurrentWaypoint].transform.position;
            Vector3 Forward = Dir;
            Forward.y = 0;

            Cart.transform.forward = Forward.normalized;

            Cart.transform.position = Dir.normalized * (Cart.transform.position - Waypoints[CurrentWaypoint].transform.position).magnitude + Waypoints[CurrentWaypoint].transform.position;
        }
        else
        {
            float T = (Start - Cart.transform.position).magnitude / (Target - Start).magnitude;
            Cart.transform.forward = Vector3.Lerp(LastDirection, Direction, T);
        }
    }
}

public class CabelIdle : CabelAction
{
    public override void OnEnter()
    {
        base.OnEnter();
        EventManager.Instance.AddHandler<BagelSent>(OnDelivery);

    }


    public override void OnExit()
    {
        base.OnExit();
        EventManager.Instance.RemoveHandler<BagelSent>(OnDelivery);
    }

    private void OnDelivery(BagelSent e)
    {
        if (e.Basket == Context.Basket1)
        {
            Context.Info.LastSide = Context.Info.CurrentSide;
            Context.Info.CurrentLockSide = Context.Info.CurrentSide;
            Context.Info.CurrentSide = CanonSide.Red;

            TransitionTo<CabelSwtiching_DeliverMove>();

            if (Context.Info.CurrentSide != CanonSide.Red)
            {


            }
        }
        else
        {
            Context.Info.LastSide = Context.Info.CurrentSide;
            Context.Info.CurrentLockSide = Context.Info.CurrentSide;
            Context.Info.CurrentSide = CanonSide.Blue;

            TransitionTo<CabelSwtiching_DeliverMove>();

            if (Context.Info.CurrentSide != CanonSide.Blue)
            {

            }
        }
    }
}

public class CabelSwtiching_DeliverMove : CabelAction
{
    private Vector3 Target;
    private Vector3 DeliverEnd;

    private Vector3 Dir;
    private GameObject Cart;

    private float MaxSpeed;
    private float Ac;
    private float Dc;

    private float Timer;
    private float Speed;

    public override void OnEnter()
    {
        base.OnEnter();
        if (Context.Info.CurrentSide == CanonSide.Red)
        {
            Target = Context.Team1DeliverEdgePoint;
            DeliverEnd = Context.Team1DeliverCartPoint;
            Cart = Context.Team1Cart;
        }
        else
        {
            Target = Context.Team2DeliverEdgePoint;
            DeliverEnd = Context.Team2DeliverCartPoint;
            Cart = Context.Team2Cart;
        }

        Dir = (Target - Context.Bagel.transform.position).normalized;

        Vector3 Offset = DeliverEnd - Target;
        Offset.y = 0;
        float DeliverEndSpeed = Offset.magnitude / Context.FeelData.DeliverFallTime;

        float Dis = (Target - Context.Bagel.transform.position).magnitude;
        MaxSpeed = Mathf.Abs((Dis - DeliverEndSpeed * Context.FeelData.DeliverMoveDcTime / 2) / (Context.FeelData.DeliverMoveStableTime + Context.FeelData.DeliverMoveAcTime / 2 + Context.FeelData.DeliverMoveDcTime / 2));
        Ac = MaxSpeed / Context.FeelData.DeliverMoveAcTime;
        Dc = (MaxSpeed - DeliverEndSpeed) / Context.FeelData.DeliverMoveDcTime;

        Timer = 0;
        Speed = 0;

        EventManager.Instance.TriggerEvent(new OnAddCameraTargets(Cart, 1));
    }

    public override void Update()
    {
        base.Update();
        Timer += Time.deltaTime;
        if (Timer <= Context.FeelData.DeliverMoveAcTime)
        {
            Speed += Ac * Time.deltaTime;
        }
        else if (Timer <= Context.FeelData.DeliverMoveAcTime + Context.FeelData.DeliverMoveStableTime)
        {
            Speed = MaxSpeed;
        }
        else if (Timer <= Context.FeelData.DeliverMoveAcTime + Context.FeelData.DeliverMoveStableTime + Context.FeelData.DeliverMoveDcTime)
        {
            Speed -= Dc * Time.deltaTime;
        }
        else
        {
            Context.Bagel.transform.position = Target;
            TransitionTo<CabelSwtiching_DeliverFall>();
            return;
        }

        Context.Bagel.transform.position += Dir * Speed * Time.deltaTime;

    }

}

public class CabelSwtiching_DeliverFall : CabelAction
{
    private Vector3 Target;

    private Vector3 HoriDir;
    private float HoriSpeed;
    private float VerSpeed;

    private float HoriDc;
    private float VerAc;

    private float MaxVerSpeed;

    private GameObject Cart;

    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();
        if (Context.Info.CurrentSide == CanonSide.Red)
        {
            Target = Context.Team1DeliverCartPoint;
            Cart = Context.Team1Cart;
        }
        else
        {
            Target = Context.Team2DeliverCartPoint;
            Cart = Context.Team2Cart;
        }

        HoriDir = Target - Context.Bagel.transform.position;
        HoriDir.y = 0;
        HoriDir.Normalize();

        MaxVerSpeed = Mathf.Abs(2 * (Target - Context.Bagel.transform.position).y / Context.FeelData.DeliverFallTime);

        Vector3 Offset = Target - Context.Bagel.transform.position;
        Offset.y = 0;
        float DeliverEndSpeed = Offset.magnitude / Context.FeelData.DeliverFallTime;

        HoriDc = DeliverEndSpeed / Context.FeelData.DeliverFallTime;
        VerAc = MaxVerSpeed / Context.FeelData.DeliverFallTime;

        HoriSpeed = DeliverEndSpeed;
        VerSpeed = 0;

        Timer = 0;

    }

    public override void Update()
    {
        base.Update();
        Timer += Time.deltaTime;
        if (Timer <= Context.FeelData.DeliverFallTime)
        {
            HoriSpeed -= HoriDc * Time.deltaTime;
            VerSpeed += VerAc * Time.deltaTime;
            Context.Bagel.transform.position += HoriSpeed * HoriDir * Time.deltaTime;
            Context.Bagel.transform.position += VerSpeed * Vector3.down * Time.deltaTime;
        }
        else
        {
            Context.Bagel.transform.position = Target;
            TransitionTo<CabelSwtiching_FirstSegment>();
        }

    }

    public override void OnExit()
    {
        base.OnExit();
        Context.Bagel.transform.parent = Cart.transform;
    }

}

public class CabelSwtiching_FirstSegment : CabelAction
{
    private List<GameObject> Waypoints;
    private List<GameObject> SecondSegWaypoints;
    private GameObject Cart;
    private Vector3 Start;

    private int TargetWaypoint;
    private int CurrentWaypoint;

    private float SecondSegSpeed;
    private float MaxSpeed;
    private float Ac;
    private float Dc;

    private float Timer;
    private float Speed;
    private float PauseTimer;



    public override void OnEnter()
    {
        base.OnEnter();
        if (Context.Info.CurrentSide == CanonSide.Red)
        {
            Waypoints = Context.Team1FirstSegmentWaypoints;
            SecondSegWaypoints = Context.Team1SecondSegmentWaypoints;
            Cart = Context.Team1Cart;
            Start = Context.Team1CartStart;
        }
        else
        {
            Waypoints = Context.Team2FirstSegmentWaypoints;
            SecondSegWaypoints = Context.Team2SecondSegmentWaypoints;
            Cart = Context.Team2Cart;
            Start = Context.Team2CartStart;
        }

        Cart.transform.position = Start;
        TargetWaypoint = 0;
        CurrentWaypoint = -1;

        float Dis = CountDistance(Waypoints);
        Dis += (Waypoints[TargetWaypoint].transform.position - Start).magnitude;

        SecondSegSpeed = CountDistance(SecondSegWaypoints) / Context.FeelData.CabelSecondSegTime;

        MaxSpeed = (Dis - SecondSegSpeed * Context.FeelData.CabelFirstSegDcTime / 2) / (Context.FeelData.CabelFirstSegStableTime + Context.FeelData.CabelFirstSegAcTime / 2 + Context.FeelData.CabelFirstSegDcTime / 2);
        Ac = MaxSpeed / Context.FeelData.CabelFirstSegAcTime;
        Dc = (MaxSpeed - SecondSegSpeed) / Context.FeelData.CabelFirstSegDcTime;

        Timer = 0;
        Speed = 0;

        Vector3 offset = Waypoints[TargetWaypoint].transform.position - Start;
        offset.y = 0;

        Cart.transform.forward = offset.normalized;
    }

    public override void Update()
    {
        base.Update();
        Timer += Time.deltaTime;

        float TrueSpeed = Speed;

        if (Timer <= Context.FeelData.CabelFirstSegAcTime)
        {
            Speed += Ac * Time.deltaTime;
        }
        else if (Timer <= Context.FeelData.CabelFirstSegStableTime + Context.FeelData.CabelFirstSegAcTime)
        {
            Speed = MaxSpeed;

        }
        else if (Timer <= Context.FeelData.CabelFirstSegStableTime + Context.FeelData.CabelFirstSegAcTime + Context.FeelData.CabelFirstSegDcTime)
        {
            Speed -= Dc * Time.deltaTime;
        }
        else
        {
            Cart.transform.position = Waypoints[Waypoints.Count - 1].transform.position;
            TransitionTo<CabelSwtiching_SecondSegment>();
        }

        TrueSpeed = (TrueSpeed + Speed) / 2;

        if (CurrentWaypoint < 0)
        {
            MoveAlongWaypoints(Waypoints, ref TargetWaypoint, ref CurrentWaypoint, Cart, TrueSpeed, Waypoints[TargetWaypoint].transform.position, Start, (Waypoints[TargetWaypoint].transform.position - Start).normalized);
        }
        else if (TargetWaypoint < Waypoints.Count)
        {
            Vector3 LastDir;
            if (CurrentWaypoint < 1)
            {
                LastDir = (Waypoints[CurrentWaypoint].transform.position - Start).normalized;
            }
            else
            {
                LastDir = (Waypoints[CurrentWaypoint].transform.position - Waypoints[CurrentWaypoint - 1].transform.position).normalized;
            }

            MoveAlongWaypoints(Waypoints, ref TargetWaypoint, ref CurrentWaypoint, Cart, TrueSpeed, Waypoints[TargetWaypoint].transform.position, Waypoints[CurrentWaypoint].transform.position, LastDir);
        }
    }

}

public class CabelSwtiching_SecondSegment : CabelAction
{
    private List<GameObject> Waypoints;
    private GameObject Cart;
    private int TargetWaypoint;
    private int CurrentWaypoint;

    private float Speed;

    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();
        if (Context.Info.CurrentSide == CanonSide.Red)
        {
            Waypoints = Context.Team1SecondSegmentWaypoints;
            Cart = Context.Team1Cart;
        }
        else
        {
            Waypoints = Context.Team2SecondSegmentWaypoints;
            Cart = Context.Team2Cart;
        }

        Cart.transform.position = Waypoints[0].transform.position;
        TargetWaypoint = 1;
        CurrentWaypoint = 0;

        Speed = CountDistance(Waypoints) / Context.FeelData.CabelSecondSegTime;
        Timer = 0;

        Vector3 offset = Waypoints[TargetWaypoint].transform.position - Waypoints[CurrentWaypoint].transform.position;
        offset.y = 0;

        Cart.transform.forward = offset.normalized;

        Move();
    }

    public override void Update()
    {
        base.Update();

        Move();
    }

    private void Move()
    {
        Timer += Time.deltaTime;

        if (Timer <= Context.FeelData.CabelSecondSegTime)
        {
            if (TargetWaypoint < Waypoints.Count)
            {
                Vector3 LastDir;
                if (CurrentWaypoint < 1)
                {
                    LastDir = (Waypoints[TargetWaypoint].transform.position - Waypoints[CurrentWaypoint].transform.position).normalized;
                }
                else
                {
                    LastDir = (Waypoints[CurrentWaypoint].transform.position - Waypoints[CurrentWaypoint - 1].transform.position).normalized;
                }

                MoveAlongWaypoints(Waypoints, ref TargetWaypoint, ref CurrentWaypoint, Cart, Speed, Waypoints[TargetWaypoint].transform.position, Waypoints[CurrentWaypoint].transform.position, LastDir);
            }
        }
        else
        {
            Cart.transform.position = Waypoints[Waypoints.Count - 1].transform.position;
            TransitionTo<CabelSwtiching_ThirdSegment>();
        }
    }
}

public class CabelSwtiching_ThirdSegment : CabelAction
{
    private Vector3 StartPoint;
    private List<GameObject> Waypoints;
    private List<GameObject> SecondSegWaypoints;
    private GameObject Cart;

    private float SecondSegSpeed;
    private int TargetWaypoint;
    private int CurrentWaypoint;
    private float MaxSpeed;
    private float Ac;

    private float Timer;
    private float Speed;

    private float DeliveryJumpHoriDc;
    private float DeliveryJumpHoriSpeed;
    private float DeliveryJumpVerDc;
    private float DeliveryJumpVerSpeed;
    private Vector3 DeliveryJumpHoriDir;
    private Vector3 DeliveryJumpRotateSpeed;


    public override void OnEnter()
    {
        base.OnEnter();
        if (Context.Info.CurrentSide == CanonSide.Red)
        {
            StartPoint = Context.Team1CartStart;
            Waypoints = Context.Team1ThirdSegmentWaypoints;
            SecondSegWaypoints = Context.Team1SecondSegmentWaypoints;
            Cart = Context.Team1Cart;
        }
        else
        {
            StartPoint = Context.Team2CartStart;
            Waypoints = Context.Team2ThirdSegmentWaypoints;
            SecondSegWaypoints = Context.Team2SecondSegmentWaypoints;
            Cart = Context.Team2Cart;
        }

        Cart.transform.position = Waypoints[0].transform.position;
        TargetWaypoint = 1;
        CurrentWaypoint = 0;

        SecondSegSpeed = CountDistance(SecondSegWaypoints) / Context.FeelData.CabelSecondSegTime;

        float Dis = CountDistance(Waypoints);
        MaxSpeed = (Dis - SecondSegSpeed * Context.FeelData.CabelThirdSegAcTime / 2) / (Context.FeelData.CabelThirdSegStableTime + Context.FeelData.CabelThirdSegAcTime / 2);
        Ac = (MaxSpeed - SecondSegSpeed) / Context.FeelData.CabelThirdSegAcTime;

        Timer = 0;
        Speed = SecondSegSpeed;

        Vector3 offset = Waypoints[TargetWaypoint].transform.position - Waypoints[CurrentWaypoint].transform.position;
        offset.y = 0;

        Cart.transform.forward = offset.normalized;

        Move();

    }

    public override void Update()
    {
        base.Update();

        Move();

    }

    private void Move()
    {
        Timer += Time.deltaTime;

        float TrueSpeed = Speed;

        if (Timer <= Context.FeelData.CabelThirdSegAcTime)
        {
            Speed += Ac * Time.deltaTime;
        }
        else if (Timer <= Context.FeelData.CabelThirdSegAcTime + Context.FeelData.CabelThirdSegStableTime)
        {
            Speed = MaxSpeed;
        }
        else
        {
            Context.Info.CurrentLockSide = Context.Info.CurrentSide;

            if (Context.Info.LastSide == Context.Info.CurrentSide)
            {

                Context.Bagel.transform.eulerAngles = Vector3.zero;
                Cart.transform.position = StartPoint;
                Cart.transform.eulerAngles = Vector3.zero;

                Context.Bagel.transform.position = Context.DeliveryPlacePoint;
                GameObject.Destroy(Context.Info.Delivery);
                Context.Info.Delivery = Context.Bagel;

                Context.Bagel = null;

                TransitionTo<CabelIdle>();

                return;
            }

            if (Context.Info.State != CanonState.Firing)
            {
                Context.Bagel.transform.eulerAngles = Vector3.zero;
                Context.Bagel.transform.position = Context.DeliveryPlacePoint;
                GameObject.Destroy(Context.Info.Delivery);
                Context.Info.Delivery = Context.Bagel;
                Context.Bagel = null;

                Cart.transform.position = StartPoint;
                Cart.transform.eulerAngles = Vector3.zero;

                TransitionTo<CabelIdle>();
                Context.CanonFSM.TransitionTo<CanonFiring_Normal>();

                if (Context.Info.Bomb != null)
                {
                    EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(Context.Info.Bomb));
                }

                Context.Info.LockedPlayer = null;
                GameObject.Destroy(Context.Info.AimedMark);
                GameObject.Destroy(Context.Info.Mark);
                GameObject.Destroy(Context.Info.Bomb);


                Context.SetWrap();

                Context.CanonEntity.GetComponent<AudioSource>().Play();
            }

            return;
        }

        TrueSpeed = (TrueSpeed + Speed) / 2;

        if (Timer >= Context.FeelData.CabelThirdSegAcTime + Context.FeelData.CabelThirdSegStableTime - Context.FeelData.DeliverJumpTime)
        {
            if (Timer - Time.deltaTime < Context.FeelData.CabelThirdSegAcTime + Context.FeelData.CabelThirdSegStableTime - Context.FeelData.DeliverJumpTime)
            {
                GetDeliverJumpInfo();
            }

            DeliverJump();
        }

        Vector3 LastDir;
        if (CurrentWaypoint < 1)
        {
            LastDir = (Waypoints[TargetWaypoint].transform.position - Waypoints[CurrentWaypoint].transform.position).normalized;
        }
        else
        {
            LastDir = (Waypoints[CurrentWaypoint].transform.position - Waypoints[CurrentWaypoint - 1].transform.position).normalized;
        }

        if (TargetWaypoint < Waypoints.Count)
        {
            MoveAlongWaypoints(Waypoints, ref TargetWaypoint, ref CurrentWaypoint, Cart, TrueSpeed, Waypoints[TargetWaypoint].transform.position, Waypoints[CurrentWaypoint].transform.position, LastDir);
        }

    }

    public override void OnExit()
    {
        base.OnExit();
        Context.GenerateBagel();
        EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(Cart));
    }

    private void GetDeliverJumpInfo()
    {
        Context.Bagel.transform.parent = null;

        Vector3 Offset = Context.DeliveryPlacePoint - Context.Bagel.transform.position;

        DeliveryJumpHoriDir = Offset;


        float HoriDis = DeliveryJumpHoriDir.magnitude;
        float VerDis = DeliveryJumpHoriDir.y;

        DeliveryJumpHoriDir.y = 0;
        DeliveryJumpHoriDir.Normalize();

        DeliveryJumpHoriSpeed = (2 * HoriDis - Context.FeelData.DeliverJumpEndHoriSpeed * Context.FeelData.DeliverJumpTime) / Context.FeelData.DeliverJumpTime;
        DeliveryJumpHoriDc = (DeliveryJumpHoriSpeed - Context.FeelData.DeliverJumpEndHoriSpeed) / Context.FeelData.DeliverJumpTime;

        float DeliveryEndJumpSpeed = (2 * VerDis - Context.FeelData.DeliverJumpVerSpeed * Context.FeelData.DeliverJumpTime) / Context.FeelData.DeliverJumpTime;

        DeliveryJumpVerSpeed = Context.FeelData.DeliverJumpVerSpeed;
        DeliveryJumpVerDc = (DeliveryJumpVerSpeed - DeliveryEndJumpSpeed) / Context.FeelData.DeliverJumpTime;

        Vector3 AngleOffset = Context.Bagel.transform.eulerAngles;
        if (AngleOffset.y > 180)
        {
            AngleOffset.y -= 360;
        }

        if (AngleOffset.y > 0)
        {
            AngleOffset.y += Context.FeelData.DeliverJumpEndExtraSpin * 360f;
        }
        else
        {
            AngleOffset.y -= Context.FeelData.DeliverJumpEndExtraSpin * 360f;
        }

        DeliveryJumpRotateSpeed = AngleOffset / Context.FeelData.DeliverJumpTime;


    }

    private void DeliverJump()
    {
        DeliveryJumpHoriSpeed -= DeliveryJumpHoriDc * Time.deltaTime;
        DeliveryJumpVerSpeed -= DeliveryJumpVerDc * Time.deltaTime;


        Context.Bagel.transform.position += DeliveryJumpHoriSpeed * DeliveryJumpHoriDir * Time.deltaTime + DeliveryJumpVerSpeed * Vector3.up * Time.deltaTime;

        Context.Bagel.transform.eulerAngles -= DeliveryJumpRotateSpeed * Time.deltaTime;
    }
}

public abstract class CanonAction : FSM<BrawlModeReforgedArenaManager>.State
{
    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log(this.GetType().Name);
    }

    /*protected bool CheckDelivery() // Check if there is an available delivery for switch
    {
        if (Context.DeliveryEvent != null)
        {
            if (Context.DeliveryEvent.Basket == BrawlModeReforgedArenaManager.Team1Basket && Context.Info.CurrentSide != CanonSide.Red)
            {
                Context.Info.LastSide = Context.Info.CurrentSide;
                Context.Info.CurrentSide = CanonSide.Red;
                Context.Info.LockedPlayer = null;

                Context.DeliveryEvent = null;

                Context.SetWrap();

                TransitionTo<CanonSwtich>();

                return true;
            }
            else if (Context.DeliveryEvent.Basket == BrawlModeReforgedArenaManager.Team2Basket && Context.Info.CurrentSide != CanonSide.Blue)
            {
                Context.Info.LastSide = Context.Info.CurrentSide;
                Context.Info.CurrentSide = CanonSide.Blue;
                Context.Info.LockedPlayer = null;

                Context.DeliveryEvent = null;

                Context.SetWrap();

                TransitionTo<CanonSwtich>();

                return true;
            }
        }

        return false;
    }*/

    protected void MarkFollow(bool Normal) // Bomb area follows target
    {
        if (Context.Info.LockedPlayer == null)
        {
            return;
        }

        Context.Info.AimedMark.transform.position = Context.Info.LockedPlayer.transform.position + Context.FeelData.AimedMarkOffset;
        Context.Info.AimedMark.transform.LookAt(Camera.main.transform);

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
            if (child.GetComponent<PlayerController>().enabled && (child.tag.Contains("1") && Context.Info.CurrentLockSide== CanonSide.Blue || child.tag.Contains("2") && Context.Info.CurrentLockSide == CanonSide.Red))
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

            GameObject AimedMark = GameObject.Instantiate(Context.AimedMarkPrefab);
            Color color = AimedMark.GetComponent<SpriteRenderer>().color;
            AimedMark.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0);
            AimedMark.transform.position = AvailablePlayer[index].transform.position + Context.FeelData.AimedMarkOffset;

            Context.Info.Mark = Mark;
            Context.Info.AimedMark = AimedMark;
            Context.Info.LockedPlayer = AvailablePlayer[index];

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
    }

    protected void FireSetCanon() // Set the shape and transform of canon object and ammo 
    {
        Vector3 Offset = Context.Info.Mark.transform.position - Context.Info.Entity.transform.position;
        Offset.y = 0;
        float TargetAngle;
        TargetAngle = Vector3.SignedAngle(Offset, Vector3.back, Vector3.up);

        SetCanon(0, TargetAngle, Context.FeelData.ShootPercentageFollowSpeed);
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

        Context.Info.LJoint1.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(0, -Mathf.Lerp(Context.FeelData.MinJointRotation, Context.FeelData.MaxJointRotation, Context.Info.CurrentPercentage), 0, 1);
        Context.Info.RJoint1.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(0, Mathf.Lerp(Context.FeelData.MinJointRotation, Context.FeelData.MaxJointRotation, Context.Info.CurrentPercentage), 0, 1);

        JointDrive Joint1Drive = new JointDrive();
        Joint1Drive.positionSpring = Mathf.Lerp(Context.FeelData.MinJoint1AngularYZ, Context.FeelData.MaxJoint1AngularYZ, Context.Info.CurrentPercentage);
        Joint1Drive.maximumForce = float.PositiveInfinity;

        Context.Info.LJoint1.GetComponent<ConfigurableJoint>().angularYZDrive = Joint1Drive;
        Context.Info.RJoint1.GetComponent<ConfigurableJoint>().angularYZDrive = Joint1Drive;

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

    protected void SetBombRotation()
    {
        Context.Info.Bomb.transform.forward = Context.Info.Mark.transform.position - Context.Info.Bomb.transform.position;
    }
}

public class CanonIdle : CanonAction
{
    public override void OnEnter()
    {
        base.OnEnter();

        Context.Info.State = CanonState.Idle;
    }

    public override void Update()
    {
        base.Update();
        SetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);
        //CheckDelivery();
    }

}


public class CanonCooldown : CanonAction
{
    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();

        Context.Info.State = CanonState.Cooldown;

        Timer = 0;

        //EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(Context.Info.CameraFocus));
    }

    public override void Update()
    {
        base.Update();

        SetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);

        /*if (CheckDelivery())
        {
            return;
        }*/
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

public class CanonFiring_Normal : CanonAction // Lock and follow player (white mark)
{
    private float Timer;
    private bool PlayerLocked;

    public override void OnEnter()
    {
        base.OnEnter();

        Context.Info.State = CanonState.Aiming;

        Timer = 0;
        PlayerLocked = false;

        
    }

    public override void Update()
    {
        base.Update();

        /*if (CheckDelivery())
        {
            GameObject.Destroy(Context.Info.Bomb);
            GameObject.Destroy(Context.Info.Mark);
            return;
        }*/

        if (PlayerLocked)
        {
            AimingSetCanon();
            MarkFollow(true);

            if (Context.Info.LockedPlayer == null)
            {
                EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(Context.Info.Bomb));

                GameObject.Destroy(Context.Info.Bomb);
                GameObject.Destroy(Context.Info.Mark);
                GameObject.Destroy(Context.Info.AimedMark);

                TransitionTo<CanonCooldown>();
                return;
            }

            Color color = Context.Info.Mark.GetComponent<SpriteRenderer>().color;
            Context.Info.Mark.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, Timer / Context.FeelData.MarkAppearTime);

            Color Acolor = Context.Info.AimedMark.GetComponent<SpriteRenderer>().color;
            Context.Info.AimedMark.GetComponent<SpriteRenderer>().color = new Color(Acolor.r, Acolor.g, Acolor.b, Timer / Context.FeelData.MarkAppearTime);

            CheckTimer();

            SetBombRotation();
        }
        else
        {
            SetCanon(0, 0, Context.FeelData.AimingPercentageFollowSpeed);
            if (LockPlayer())
            {


                Context.Info.Bomb = GameObject.Instantiate(Context.BombPrefab);
                Context.Info.Bomb.transform.parent = Context.CanonPad.transform;
                Context.Info.Bomb.transform.localPosition = Vector3.back * Context.FeelData.AmmoOffset;

                AimingSetCanon();

                PlayerLocked = true;

                EventManager.Instance.TriggerEvent(new OnAddCameraTargets(Context.Info.Bomb, Context.FeelData.AimingCameraWeight));
            }
        }



    }

    private void CheckTimer()
    {
        Timer += Time.deltaTime;
        if (Timer >= Context.Data.CanonFireTime)
        {
            TransitionTo<CanonFiring_Alert>();
        }
    }
}

public class CanonFiring_Alert : CanonAction // Purple mark follows target
{
    private float Timer;

    public override void OnEnter()
    {
        base.OnEnter();
        Timer = 0;
        Context.Info.Mark.GetComponent<SpriteRenderer>().color = Context.FeelData.MarkAlertColor;
    }

    public override void Update()
    {
        base.Update();

        AimingSetCanon();
        MarkFollow(false);
        SetBombRotation();

        if (Context.Info.LockedPlayer == null)
        {
            EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(Context.Info.Bomb));

            GameObject.Destroy(Context.Info.Bomb);
            GameObject.Destroy(Context.Info.Mark);
            GameObject.Destroy(Context.Info.AimedMark);

            TransitionTo<CanonCooldown>();
            return;
        }

        /*if (CheckDelivery())
        {
            GameObject.Destroy(Context.Info.Bomb);
            GameObject.Destroy(Context.Info.Mark);
            return;
        }*/
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

public class CanonFiring_Fall : CanonAction // Shoot ammo
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

        Context.Info.State = CanonState.Firing;

        Timer = 0;
        InfoGot = false;



        GetBombFlyInfo();


        Context.Info.Mark.GetComponent<SpriteRenderer>().color = Context.FeelData.MarkFallColor;
    }

    public override void Update()
    {
        base.Update();

        if (Context.Info.LockedPlayer != null)
        {
            Context.Info.AimedMark.transform.position = Context.Info.LockedPlayer.transform.position + Context.FeelData.AimedMarkOffset;
            Context.Info.AimedMark.transform.LookAt(Camera.main.transform);
        }
        else
        {
            GameObject.Destroy(Context.Info.AimedMark);
        }

        FireSetCanon();
        if (Context.Info.CurrentPercentage == 0)
        {
            Context.Info.Bomb.transform.parent = null;
            if (!InfoGot)
            {
                InfoGot = true;
                GetBombFlyInfo();
            }
            BombFly();
            SetBombRotation();
        }


        CheckTimer();
    }

    private void CheckTimer()
    {
        Timer += Time.deltaTime;
        if (Timer >= Context.Data.CanonFireFinalTime)
        {
            EventManager.Instance.TriggerEvent(new OnRemoveCameraTargets(Context.Info.Bomb));
            BombFall();

            /*if (CheckDelivery())
            {
                return;
            }*/

            TransitionTo<CanonCooldown>();
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

                Player.GetComponent<IHittable>().OnImpact(Context.Data.CanonPower * Offset.normalized, ForceMode.Impulse, Context.Info.Entity, ImpactType.BazookaGun);
            }
        }

        Context.Info.LockedPlayer = null;

        EventManager.Instance.TriggerEvent(new AmmoExplode(Context.Info.Bomb.transform.position));
        GameObject.Destroy(Context.Info.Bomb);
        GameObject.Destroy(Context.Info.Mark);
        GameObject.Destroy(Context.Info.AimedMark);
    }
}

public class BrawlModeReforgedArenaManager : MonoBehaviour
{
    public class CanonInfo
    {
        public GameObject Entity;
        public GameObject Pad;
        public GameObject LJoint0;
        public GameObject RJoint0;
        public GameObject LJoint1;
        public GameObject RJoint1;
        public GameObject CameraFocus;

        public GameObject Delivery;

        public float CurrentPercentage;
        public float CurrentAngle;

        public CanonState State;
        public CanonSide CurrentSide;
        public CanonSide LastSide;
        public CanonSide CurrentLockSide;
        public float Timer;
        public int FireCount;

        public GameObject Bomb;

        public GameObject Mark;
        public GameObject AimedMark;
        public GameObject LockedPlayer;


        public CanonInfo(GameObject entity, GameObject pad, GameObject L0, GameObject R0, GameObject L1, GameObject R1, GameObject Focus)
        {
            Entity = entity;
            Pad = pad;

            CameraFocus = Focus;
            LJoint0 = L0;
            RJoint0 = R0;
            LJoint1 = L1;
            RJoint1 = R1;

            State = CanonState.Idle;
            CurrentSide = LastSide = CanonSide.Neutral;

            FireCount = 0;
            Timer = 0;

            Mark = null;
            LockedPlayer = null;

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
    public GameObject CanonLJoint0;
    public GameObject CanonRJoint0;
    public GameObject CanonLJoint1;
    public GameObject CanonRJoint1;

    public GameObject CanonLJoint2;
    public GameObject CanonRJoint2;


    public GameObject CameraFocus;

    public GameObject Wrap;

    public GameObject Team1Cabel;
    public GameObject Team2Cabel;

    public LayerMask PlayerLayer;
    public LayerMask GroundLayer;
    public GameObject Players;

    public GameObject BagelPrefab;
    public GameObject MarkPrefab;
    public GameObject AimedMarkPrefab;
    public GameObject BombPrefab;
    public GameObject ExplosionVFX;

    public GameObject Team1FirstSeg;
    public GameObject Team1SecondSeg;
    public GameObject Team1ThirdSeg;

    public List<GameObject> Team1FirstSegmentWaypoints;
    public List<GameObject> Team1SecondSegmentWaypoints;
    public List<GameObject> Team1ThirdSegmentWaypoints;

    public GameObject Team1Cart;

    public Vector3 Team1CartStart;
    public Vector3 Team1DeliverEdgePoint;
    public Vector3 Team1DeliverCartPoint;

    public GameObject Team2FirstSeg;
    public GameObject Team2SecondSeg;
    public GameObject Team2ThirdSeg;

    public List<GameObject> Team2FirstSegmentWaypoints;
    public List<GameObject> Team2SecondSegmentWaypoints;
    public List<GameObject> Team2ThirdSegmentWaypoints;

    public GameObject Team2Cart;

    public Vector3 Team2CartStart;
    public Vector3 Team2DeliverEdgePoint;
    public Vector3 Team2DeliverCartPoint;

    public Vector3 DeliveryPlacePoint;

    public BrawlModeReforgedModeData Data;

    public CanonInfo Info;

    public GameObject Bagel;

    public FSM<BrawlModeReforgedArenaManager> CanonFSM;
    public FSM<BrawlModeReforgedArenaManager> CabelFSM;

    // Start is called before the first frame update
    void Start()
    {
        Team1FirstSegmentWaypoints = new List<GameObject>();
        Team1SecondSegmentWaypoints = new List<GameObject>();
        Team1ThirdSegmentWaypoints = new List<GameObject>();
        Team2FirstSegmentWaypoints = new List<GameObject>();
        Team2SecondSegmentWaypoints = new List<GameObject>();
        Team2ThirdSegmentWaypoints = new List<GameObject>();

        GetSegWaypoints(Team1FirstSeg, Team1FirstSegmentWaypoints);
        GetSegWaypoints(Team1SecondSeg, Team1SecondSegmentWaypoints);
        GetSegWaypoints(Team1ThirdSeg, Team1ThirdSegmentWaypoints);
        GetSegWaypoints(Team2FirstSeg, Team2FirstSegmentWaypoints);
        GetSegWaypoints(Team2SecondSeg, Team2SecondSegmentWaypoints);
        GetSegWaypoints(Team2ThirdSeg, Team2ThirdSegmentWaypoints);

        CanonFSM = new FSM<BrawlModeReforgedArenaManager>(this);
        CanonFSM.TransitionTo<CanonIdle>();

        CabelFSM = new FSM<BrawlModeReforgedArenaManager>(this);
        CabelFSM.TransitionTo<CabelIdle>();

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



        Info = new CanonInfo(CanonEntity, CanonPad, CanonLJoint0, CanonRJoint0, CanonLJoint1, CanonRJoint1, CameraFocus);

        SetWrap();

        GenerateBagel();

        EventManager.Instance.AddHandler<PlayerDied>(OnPlayerDied);
        EventManager.Instance.AddHandler<BagelDespawn>(OnBagelDespawn);
        //EventManager.Instance.AddHandler<BagelSent>(OnBagelSent);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveHandler<PlayerDied>(OnPlayerDied);
        EventManager.Instance.RemoveHandler<BagelDespawn>(OnBagelDespawn);
        //EventManager.Instance.RemoveHandler<BagelSent>(OnBagelSent);
    }

    // Update is called once per frame
    void Update()
    {
        CanonFSM.Update();
        CabelFSM.Update();
    }

    private void GetSegWaypoints(GameObject Seg, List<GameObject> SegWaypoints)
    {
        foreach (Transform child in Seg.transform)
        {
            SegWaypoints.Add(child.gameObject);
        }


    }

    private int GetPlayerNumber()
    {
        return Players.transform.childCount;

    }



    public void GenerateBagel()
    {
        Vector3 Pos = Data.BagelGenerationPos;

        BrawlModeReforgedObjectiveManager Manager = (BrawlModeReforgedObjectiveManager)Services.GameObjectiveManager;

        if (Manager.Team1Score - Manager.Team2Score >= Data.DeliveryPoint)
        {
            float Ran = Random.Range(0.0f, 1.0f);
            Pos = Vector3.Lerp(Data.BagelGenerationPos, Data.BagelGenerationPosRight, Ran);
            //Pos = Data.BagelGenerationPosRight;
        }
        else if (Manager.Team2Score - Manager.Team1Score >= Data.DeliveryPoint)
        {
            float Ran = Random.Range(0.0f, 1.0f);
            Pos = Vector3.Lerp(Data.BagelGenerationPos, Data.BagelGenerationPosLeft, Ran);
            //Pos = Data.BagelGenerationPosLeft;
        }



        Bagel = GameObject.Instantiate(BagelPrefab, Pos, new Quaternion(0, 0, 0, 0));
    }

    private void OnBagelDespawn(BagelDespawn e)
    {
        GenerateBagel();
    }


    private void OnPlayerDied(PlayerDied e)
    {
        if (e.Player == Info.LockedPlayer)
        {
            Info.LockedPlayer = null;
        }

        if (Bagel == null && GetPlayerNumber() > 2)
        {
            GenerateBagel();
        }
    }

    public void SetWrap()
    {
        Material mat = FeelData.WrapNMat;
        switch (Info.CurrentSide)
        {
            case CanonSide.Neutral:
                mat = FeelData.WrapNMat;
                break;
            case CanonSide.Red:
                mat = FeelData.WrapRedMat;
                break;
            case CanonSide.Blue:
                mat = FeelData.WrapBlueMat;
                break;
        }

        Wrap.GetComponent<MeshRenderer>().material = mat;
    }
}
