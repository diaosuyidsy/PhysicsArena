using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using DG.Tweening;
using Bolt;

/// <summary>
/// This is 风神鹤 class's controller
/// </summary>
public class BoltPlayerWindController : BoltPlayerControllerBase
{
    public PlayerClassData_Wind PlayerClassData_Wind;
    private FSM<BoltPlayerWindController> _movementFSM;
    private FSM<BoltPlayerWindController> _actionFSM;

    protected override void SetupStateMachine()
    {
        _movementFSM = new FSM<BoltPlayerWindController>(this);
        _actionFSM = new FSM<BoltPlayerWindController>(this);
        _movementFSM.TransitionTo<MovementIdleState>();
        _actionFSM.TransitionTo<ActionIdleState>();
    }

    protected override void _onMovementStateIndexChange()
    {
        switch (state.MovementStateIndex)
        {
            case 0:
                _movementFSM.TransitionTo<MovementIdleState>();
                break;
            case 1:
                _movementFSM.TransitionTo<MovementRunState>();
                break;
            case 2:
                _movementFSM.TransitionTo<MovementHitUncontrollableState>();
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            default:
                _movementFSM.TransitionTo<MovementIdleState>();
                break;
        }
    }

    protected override void _onActionStateIndexChange()
    {
        switch (state.ActionStateIndex)
        {
            case 0:
                _actionFSM.TransitionTo<ActionIdleState>();
                break;
            case 1:
                _actionFSM.TransitionTo<ActionDeadState>();
                break;
            case 2:
                _actionFSM.TransitionTo<ActionHitUnControllableState>();
                break;
            default:
                _actionFSM.TransitionTo<ActionIdleState>();
                break;
        }
    }

    public override void ExecuteCommand(Command command, bool resetState)
    {
        base.ExecuteCommand(command, resetState);
        if (!resetState)
        {
            ((MovementState)_movementFSM.CurrentState).ExecuteCommand(command, resetState);
            ((ActionState)_actionFSM.CurrentState).ExecuteCommand(command, resetState);
        }
    }

    public override void OnImpact(Vector3 force, ForceMode forcemode, GameObject enforcer, ImpactType impactType)
    {
        base.OnImpact(force, forcemode, enforcer, impactType);
        if (force.magnitude > CharacterDataStore.HitSmallThreshold)
        {
            if (_movementFSM.CurrentState != null && (_movementFSM.CurrentState as MovementState).ShouldOnHitTransitToUncontrollableState)
            {
                _movementFSM.TransitionTo<MovementHitUncontrollableState>();
            }

            if (_actionFSM.CurrentState != null && (_actionFSM.CurrentState as ActionState).ShouldOnHitTransitToUncontrollableState)
            {
                _actionFSM.TransitionTo<ActionHitUnControllableState>();
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        _movementFSM.Update();
        _actionFSM.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        _movementFSM.FixedUpdate();
        _actionFSM.FixedUpdate();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        _movementFSM.LateUpdate();
        _actionFSM.LateUpdate();
    }
    #region General Movement States
    protected class MovementState : FSM<BoltPlayerWindController>.State
    {
        protected float _HLAxis;
        protected float _VLAxis;
        protected float _HLAxisRaw;
        protected float _VLAxisRaw;
        protected bool _jump;
        protected bool _RightTriggerUp;
        public virtual bool ShouldOnHitTransitToUncontrollableState { get { return true; } }
        protected virtual int _stateIndex { get { return -1; } }
        public void OnEnterDeathZone()
        {
            if (Context.entity.IsOwner)
            {
                Parent.TransitionTo<MovementDeadState>();
            }
        }

        public virtual void OnCollisionEnter(Collision other)
        {

        }

        public virtual void ExecuteCommand(Command command, bool resetState)
        {
            BirfiaPlayerCommand cmd = (BirfiaPlayerCommand)command;
            _HLAxis = cmd.Input.HorizontalAxis;
            _VLAxis = cmd.Input.VerticalAxis;
            _HLAxisRaw = cmd.Input.HorizontalAxisRaw;
            _VLAxisRaw = cmd.Input.VerticalAxisRaw;
            _jump = cmd.Input.Jump;
            _RightTriggerUp = cmd.Input.RightTriggerUp;
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            Context._freezeBody.y = Context.transform.localEulerAngles.y;
            Context.transform.localEulerAngles = Context._freezeBody;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            print(GetType().Name);
            if (Context.entity.IsOwner)
                Context.state.MovementStateIndex = _stateIndex;
        }
    }
    protected class MovementIdleState : MovementState
    {
        protected override int _stateIndex { get { return 0; } }

        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
                Context.state.IdleDowner = true;
        }

        public override void Update()
        {
            base.Update();
            if (_HLAxisRaw != 0f || _VLAxisRaw != 0f)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<MovementRunState>();
                    return;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (Context._frontIsCliff() && Context._isGrounded())
            {
                Context._rb.AddForce(-Context.transform.forward * Context.CharacterDataStore.CliffPreventionForce, ForceMode.Acceleration);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
                Context.state.IdleDowner = false;
        }
    }
    protected class MovementRunState : MovementState
    {
        protected override int _stateIndex { get { return 1; } }

        private float _runTowardsCliffTime;
        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
                Context.state.Running = true;
            _runTowardsCliffTime = 0f;
        }

        public override void Update()
        {
            base.Update();
            if (_HLAxisRaw == 0f && _VLAxisRaw == 0f)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<MovementIdleState>();
                    return;
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool isonground = Context._isGrounded();
            Vector3 targetVelocity = Context.transform.forward * Context.CharacterDataStore.WalkSpeed * Context.WalkSpeedMultiplier;
            Vector3 velocityChange = targetVelocity - Context._rb.velocity;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -Context.CharacterDataStore.MaxVelocityChange, Context.CharacterDataStore.MaxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -Context.CharacterDataStore.MaxVelocityChange, Context.CharacterDataStore.MaxVelocityChange);
            velocityChange.y = 0f;

            if (isonground)
                Context._rb.AddForce(velocityChange, ForceMode.VelocityChange);
            else
                Context._rb.AddForce(velocityChange * Context.CharacterDataStore.InAirSpeedMultiplier, ForceMode.VelocityChange);

            Vector3 relPos = Quaternion.AngleAxis(Mathf.Atan2(_HLAxis, _VLAxis * -1f) * Mathf.Rad2Deg, Context.transform.up) * Vector3.forward;
            Quaternion rotation = Quaternion.LookRotation(relPos, Vector3.up);
            Quaternion tr = Quaternion.Slerp(Context.transform.rotation, rotation, Time.deltaTime * Context.CharacterDataStore.MinRotationSpeed * Context.RotationSpeedMultiplier);
            Context.transform.rotation = tr;
            if (Context._frontIsCliff() && isonground)
            {
                _runTowardsCliffTime += Time.fixedDeltaTime;
                if (_runTowardsCliffTime < Context.CharacterDataStore.CliffPreventionTimer)
                    Context._rb.AddForce(-Context.transform.forward * Context.CharacterDataStore.CliffPreventionForce, ForceMode.Acceleration);
            }
            else
                _runTowardsCliffTime = 0f;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
                Context.state.Running = false;
        }
    }
    protected class MovementHitUncontrollableState : MovementState
    {
        protected override int _stateIndex { get { return 2; } }

        private float _timer;
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = Time.timeSinceLevelLoad + Context._hitUncontrollableTimer;
        }

        public override void Update()
        {
            base.Update();
            if (_timer < Time.timeSinceLevelLoad)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<MovementIdleState>();
                    return;
                }

            }
        }
    }
    protected class MovementDeadState : MovementState
    {
        private float _startTime;
        private float _respawnTime { get { return Services.Config.GameMapData.RespawnTime; } }
        protected override int _stateIndex { get { return 10; } }

        public override void OnEnter()
        {
            base.OnEnter();
            _startTime = Time.time;
            Context._rb.isKinematic = true;
            Context._setToSpawn(10f);
            if (Context.entity.IsOwner)
            {
                Context.state.IdleDowner = true;
            }
            foreach (GameObject go in Context.OnDeathHidden) { go.SetActive(false); }
            if (Context._deadInvincible != null)
                Context.StopCoroutine(Context._deadInvincible);
        }

        public override void Update()
        {
            base.Update();
            if (Time.time >= _startTime + _respawnTime)
            {
                if (Context.entity.IsOwner)
                {
                    Services.BoltEventBroadcaster.OnPlayerRespawned(new PlayerRespawned(Context.gameObject));
                    TransitionTo<MovementIdleState>();
                    return;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._rb.isKinematic = false;
            Context._setToSpawn(0f);
            foreach (GameObject go in Context.OnDeathHidden) { go.SetActive(true); }
            Context._deadInvincible = Context._deadInvincibleIenumerator(Services.Config.GameMapData.InvincibleTime);
            Context.StartCoroutine(Context._deadInvincible);
        }
    }
    #endregion

    #region Wind Class Specific Movement States
    protected class MovementFanStrikeRecoveryState : MovementState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
                Context.state.IdleDowner = true;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
                Context.state.IdleDowner = false;
        }
    }
    #endregion

    #region General Action State
    protected class ActionState : FSM<BoltPlayerWindController>.State
    {
        protected bool _RightTrigger;
        protected bool _RightTriggerDown;
        protected bool _RightTriggerUp;

        protected bool _B;
        protected bool _BDown;
        protected bool _BUp;

        protected bool _QDown;

        public virtual bool ShouldOnHitTransitToUncontrollableState { get { return false; } }
        public virtual bool ShouldDropHandObjectWhenForced { get { return false; } }
        protected virtual int _stateIndex { get { return -1; } }

        public override void OnEnter()
        {
            base.OnEnter();
            print(GetType().Name);
            if (Context.entity.IsOwner)
            {
                Context.state.ActionStateIndex = _stateIndex;
            }
        }
        public virtual void ExecuteCommand(Command command, bool resetState)
        {
            BirfiaPlayerCommand cmd = (BirfiaPlayerCommand)command;
            _RightTrigger = cmd.Input.RightTrigger;
            _RightTriggerDown = cmd.Input.RightTriggerDown;
            _RightTriggerUp = cmd.Input.RightTriggerUp;
            _B = cmd.Input.B;
            _BDown = cmd.Input.BDown;
            _BUp = cmd.Input.BUp;
            _QDown = cmd.Input.QDown;
        }
        public override void Update()
        {
            base.Update();
            /// Regen when past 3 seconds after block
            if (Time.timeSinceLevelLoad > Context._lastTimeUseStamina + Context.CharacterDataStore.StaminaRegenInterval)
            {
                // if (Context._currentStamina < Context.CharacterDataStore.MaxStamina) Context._currentStamina += (Time.deltaTime * Context.CharacterDataStore.StaminaRegenRate);
                if (Context._currentStamina < Context.CharacterDataStore.MaxStamina) Context._drainStamina(-Time.deltaTime * Context.CharacterDataStore.StaminaRegenRate);
            }
        }

        public virtual void OnEnterDeathZone()
        {
            if (Context.entity.IsOwner)
            {
                TransitionTo<ActionDeadState>();
                return;
            }

        }
    }
    protected class ActionIdleState : ActionState
    {
        protected override int _stateIndex { get { return 0; } }

        private float _pickUpTimer;
        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }
        private float _emojiTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
                Context.state.IdleUpper = true;
        }

        public override void Update()
        {
            base.Update();

            if (_QDown && _emojiTimer < Time.time)
            {
                _emojiTimer = Time.time + 0.3f;
                EventManager.Instance.TriggerEvent(new TriggerEmoji(0, Context.gameObject));
            }

            if (_RightTriggerDown)
            {
                TransitionTo<ActionFanStrikeAnticipationState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
                Context.state.IdleUpper = false;
        }
    }
    protected class ActionHitUnControllableState : ActionState
    {
        protected override int _stateIndex { get { return 2; } }

        private float _timer;
        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }
        private int myLayer;
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = Time.timeSinceLevelLoad + Context._hitUncontrollableTimer;
            if (Context.entity.IsOwner)
                Services.BoltEventBroadcaster.OnPunchInterrepted(new PunchInterruptted(Context.gameObject, Context.PlayerNumber));
            myLayer = Context.gameObject.layer;
            Context.gameObject.layer = 19;
            Context._rb.AddForce(Context._hitForceTuple.Force, Context._hitForceTuple.ForceMode);
        }

        public override void OnExit()
        {
            base.OnExit();
            Context.gameObject.layer = myLayer;
        }

        public override void Update()
        {
            base.Update();
            if (_timer < Time.timeSinceLevelLoad)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<ActionIdleState>();
                    return;
                }

            }
        }
    }
    protected class ActionDeadState : ActionState
    {
        protected override int _stateIndex { get { return 1; } }
        private float _startTime;
        private float _respawnTime { get { return Services.Config.GameMapData.RespawnTime + Services.Config.GameMapData.InvincibleTime; } }

        public override void OnEnter()
        {
            base.OnEnter();
            _startTime = Time.time;
            Context._drainStamina(-5f);
            if (Context.entity.IsOwner)
                Context.state.IdleUpper = true;
            if (Context.entity.IsOwner)
                Services.BoltEventBroadcaster.OnPunchInterrepted(new PunchInterruptted(Context.gameObject, Context.PlayerNumber));
            if (Context.MeleeVFXHolder != null) Destroy(Context.MeleeVFXHolder);
        }

        public override void Update()
        {
            base.Update();
            if (Time.time >= _startTime + _respawnTime)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<ActionIdleState>();
                    return;
                }

            }
            if (Time.time >= _startTime + Services.Config.GameMapData.RespawnTime && (_B || _RightTrigger))
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<ActionIdleState>();
                    return;
                }

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context._deadInvincible != null)
            {
                Context.StopCoroutine(Context._deadInvincible);

                Context._rb.gameObject.layer = Context._playerBodiesLayer;

            }
        }
    }
    #endregion

    #region Wind Class Specific Action States
    protected class ActionFanStrikeAnticipationState : ActionState
    {
        private float _timer;
        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
            {
                (Context.state as IWindPlayerState).FanStrikeAnticipationMul = 1f / Context.PlayerClassData_Wind.FanStrikeAnticipationDuration;
                (Context.state as IWindPlayerState).FanStrikeAnticipation = true;
            }
            _timer = Time.timeSinceLevelLoad + Context.PlayerClassData_Wind.FanStrikeAnticipationDuration;
        }

        public override void Update()
        {
            base.Update();
            if (_timer < Time.timeSinceLevelLoad)
            {
                TransitionTo<ActionFanStrikeState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
                (Context.state as IWindPlayerState).FanStrikeAnticipation = false;
        }
    }

    protected class ActionFanStrikeState : ActionState
    {
        private float _timer;
        private SlowEffect _strikeSlowEffect;
        private RotationSlowEffect _strikeRotationSlowEffect;
        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
            {
                (Context.state as IWindPlayerState).FanStrikeMul = 1f / Context.PlayerClassData_Wind.FanStrikeDuration;
                (Context.state as IWindPlayerState).FanStrike = true;
                _strikeSlowEffect = new SlowEffect(0f, Context.PlayerClassData_Wind.FanStrikeSlowPercent, true);
                _strikeRotationSlowEffect = new RotationSlowEffect(0f, Context.PlayerClassData_Wind.FanStrikeSlowRotatePercent, true);
                Context.OnImpact(_strikeSlowEffect);
                Context.OnImpact(_strikeRotationSlowEffect);
            }
            _timer = Time.timeSinceLevelLoad + Context.PlayerClassData_Wind.FanStrikeDuration;
        }

        public override void Update()
        {
            base.Update();
            if (_timer < Time.timeSinceLevelLoad)
            {
                TransitionTo<ActionFanStrikeRecoveryState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
            {
                (Context.state as IWindPlayerState).FanStrike = false;
                Context.OnRemove(_strikeSlowEffect);
                Context.OnRemove(_strikeRotationSlowEffect);
            }
        }
    }

    protected class ActionFanStrikeRecoveryState : ActionState
    {
        private float _timer;
        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
            {
                (Context.state as IWindPlayerState).FanStrikeRecoveryMul = 1f / Context.PlayerClassData_Wind.FanStrikeRecoveryDuration;
                (Context.state as IWindPlayerState).FanRecovery = true;
            }
            Context._movementFSM.TransitionTo<MovementFanStrikeRecoveryState>();
            _timer = Time.timeSinceLevelLoad + Context.PlayerClassData_Wind.FanStrikeRecoveryDuration;
        }

        public override void Update()
        {
            base.Update();
            if (_timer < Time.timeSinceLevelLoad)
            {
                TransitionTo<ActionIdleState>();
                return;
            }
        }

        public override void OnExit()
        {
            if (Context.entity.IsOwner)
                (Context.state as IWindPlayerState).FanRecovery = false;
            Context._movementFSM.TransitionTo<MovementIdleState>();
            base.OnExit();
        }
    }
    #endregion
}
