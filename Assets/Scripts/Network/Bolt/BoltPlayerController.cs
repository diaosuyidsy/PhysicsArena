using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using DG.Tweening;
using Bolt;

public class BoltPlayerController : Bolt.EntityEventListener<IBirfiaPlayerState>, IHittable, IVFXHolder, IBodyConfiguration
{
    [Header("Player Data Section")]
    public CharacterData CharacterDataStore;
    public PlayerIdentification PlayerID;
    [Header("Player Body Setting Section")]
    public GameObject Chest;
    public GameObject Head;
    [Tooltip("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] LeftArms;
    [Tooltip("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] RightArms;
    public GameObject LeftHand;
    public GameObject RightHand;
    public GameObject LeftFoot;
    public GameObject RightFoot;
    public GameObject[] OnDeathHidden;
    public GameObject BlockUIVFXHolder;
    public ShieldController BlockShield;

    public int PlayerNumber;

    [HideInInspector] public GameObject HandObject { get; set; }
    [HideInInspector] public GameObject EquipmentObject { get; set; }
    [HideInInspector] public GameObject MeleeVFXHolder { get; set; }
    [HideInInspector] public GameObject MeleeVFXHolder2 { get; set; }
    [HideInInspector] public GameObject BlockVFXHolder { get; set; }
    [HideInInspector] public GameObject StunVFXHolder { get; set; }
    [HideInInspector] public GameObject SlowVFXHolder { get; set; }
    [HideInInspector] public GameObject FoodTraverseVFXHolder { get; set; }
    public Transform PlayerFeet { get { return OnDeathHidden[1].transform; } }
    public Transform PlayerUITransform;

    #region Private Variables
    private Player _player;
    private Rigidbody _rb;
    private float _distToGround;
    private float _currentStamina;
    private float _lastTimeUseStamina;
    // private float _lastTimeUSeStaminaUnimportant;
    private Vector2 _staminaUISize;
    private float _sideStepTimer;
    private float _jumpTimer;
    private Vector3 _freezeBody;
    private ImpactMarker _impactMarker;
    private Animator _animator;
    // private IEnumerator StaminaUIDecrease;

    private FSM<BoltPlayerController> _movementFSM;
    private FSM<BoltPlayerController> _actionFSM;
    #endregion

    #region Status Variables
    private float _stunTimer;
    private float _slowTimer;
    private float _hitUncontrollableTimer;
    private float _walkSpeedMultiplier = 1f;
    private float _permaSlowWalkSpeedMultiplierSub = 1f;
    private float _permaSlowWalkSpeedMultiplier
    {
        get
        {
            return _permaSlowWalkSpeedMultiplierSub;
        }
        set
        {
            _permaSlowWalkSpeedMultiplierSub = value;
            if (entity.IsOwner)
                state.RunningSpeed = _walkSpeed;
            // _animator.SetFloat("RunningSpeed", _walkSpeed);
        }
    }
    private int _permaSlow;
    private float _walkSpeed
    {
        get
        {
            if (_permaSlow > 0 && Time.time > _slowTimer)
                return _permaSlowWalkSpeedMultiplier;
            else if (_permaSlow > 0 && Time.time < _slowTimer)
                return Mathf.Max(_permaSlowWalkSpeedMultiplier, _walkSpeedMultiplier);
            else if (_permaSlow == 0 && Time.time > _slowTimer)
                return 1f;
            else return _walkSpeedMultiplier;
        }
    }
    private float _rotationSpeedMultiplier = 1f;
    private List<Rigidbody> _playerBodies;
    private Rigidbody[] _allPlayerRBs;
    private IEnumerator _deadInvincible;
    private int _playerBodiesLayer;
    private Vector3 _storedVelocity;
    private int _hitStopFrames;
    public bool IsIdle { get { return _movementFSM.CurrentState.GetType().Equals(typeof(IdleState)); } }

    GameObject IBodyConfiguration.Chest => this.Chest;

    GameObject IBodyConfiguration.Head => this.Head;

    GameObject[] IBodyConfiguration.LeftArms => this.LeftArms;

    GameObject[] IBodyConfiguration.RightArms => this.RightArms;

    GameObject IBodyConfiguration.LeftHand => this.LeftHand;

    GameObject IBodyConfiguration.RightHand => this.RightHand;

    GameObject IBodyConfiguration.LeftFoot => this.LeftFoot;

    GameObject IBodyConfiguration.RightFoot => this.RightFoot;

    Transform IBodyConfiguration.PlayerUITransform => this.PlayerUITransform;

    #region  Controller Variable
    float HA;
    float VA;
    float HAR;
    float VAR;
    bool JUM;
    bool RT;
    bool RTD;
    bool RTU;
    bool BB;
    bool BD;
    bool BU;
    bool QD;
    #endregion

    private ForceTuple _hitForceTuple;
    #endregion

    public override void Attached()
    {
        state.SetTransforms(state.MainTransform, transform);
        state.SetAnimator(GetComponent<Animator>());
        if (entity.IsOwner)
        {
            if (BoltNetwork.IsServer)
            {
                entity.TakeControl();
            }
        }
        else
        {
            state.AddCallback("MovementStateIndex", _onMovementStateIndexChange);
            state.AddCallback("ActionStateIndex", _onActionStateIndexChange);
        }
        if (BoltNetwork.IsServer) _rb.isKinematic = false;
    }

    private void Awake()
    {
        _movementFSM = new FSM<BoltPlayerController>(this);
        _actionFSM = new FSM<BoltPlayerController>(this);
        _rb = GetComponent<Rigidbody>();
        _player = ReInput.players.GetPlayer(PlayerNumber);
        _distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        _freezeBody = new Vector3(0, transform.localEulerAngles.y, 0);
        _movementFSM.TransitionTo<IdleState>();
        _actionFSM.TransitionTo<IdleActionState>();
        _impactMarker = new ImpactMarker(null, Time.time, ImpactType.Self);
        _animator = GetComponent<Animator>();
        _playerBodies = new List<Rigidbody>();
        _currentStamina = CharacterDataStore.MaxStamina;
        _staminaUISize = BlockUIVFXHolder.transform.GetChild(0).GetComponent<SpriteRenderer>().size;
        _playerBodiesLayer = gameObject.layer;
        _allPlayerRBs = GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(true))
        {
            if (rb.gameObject.layer != LayerMask.NameToLayer("NoCollision"))
            {
                _playerBodies.Add(rb);
            }
        }
    }

    public void Init(int controllernumber)
    {
        PlayerNumber = controllernumber;
        _player = ReInput.players.GetPlayer(controllernumber);
    }

    // public override void SimulateOwner()
    // {
    //     _movementFSM.Update();
    //     _actionFSM.Update();
    //     _movementFSM.FixedUpdate();
    //     _actionFSM.FixedUpdate();
    //     _movementFSM.LateUpdate();
    //     _actionFSM.LateUpdate();
    // }

    public override void SimulateController()
    {
        _pollKeys();
        IBirfiaPlayerCommandInput input = BirfiaPlayerCommand.Create();

        input.HorizontalAxis = HA;
        input.VerticalAxis = VA;
        input.HorizontalAxisRaw = HAR;
        input.VerticalAxisRaw = VAR;
        input.Jump = JUM;
        input.RightTrigger = RT;
        input.RightTriggerDown = RTD;
        input.RightTriggerUp = RTU;
        input.B = BB;
        input.BDown = BD;
        input.BUp = BU;
        input.QDown = QD;
        entity.QueueInput(input);
    }

    private void _pollKeys()
    {
        HA = _player.GetAxis("Move Horizontal");
        VA = _player.GetAxis("Move Vertical");
        HAR = _player.GetAxisRaw("Move Horizontal");
        VAR = _player.GetAxisRaw("Move Vertical");
        JUM = _player.GetButtonDown("Jump");
        RT = _player.GetButton("Attack");
        RTD = _player.GetButtonDown("Attack");
        RTU = _player.GetButtonUp("Attack");
        BB = _player.GetButton("Block");
        BD = _player.GetButtonDown("Block");
        BU = _player.GetButtonUp("Block");
        QD = _player.GetButtonDown("QuestionMark");
    }

    public override void ExecuteCommand(Command command, bool resetState)
    {
        if (!BoltNetwork.IsServer) return;
        BirfiaPlayerCommand cmd = (BirfiaPlayerCommand)command;
        if (resetState)
        {
            // transform.position = cmd.Result.Position;
            // _rb.velocity = cmd.Result.Velocity;
        }
        else
        {
            ((MovementState)_movementFSM.CurrentState).ExecuteCommand(command, resetState);
            ((ActionState)_actionFSM.CurrentState).ExecuteCommand(command, resetState);

            // cmd.Result.Position = transform.position;
            // cmd.Result.Velocity = _rb.velocity;
        }

    }

    public override void OnEvent(PunchEvent ev)
    {
        OnImpact(ev.PunchForce, ForceMode.Impulse, gameObject, ImpactType.Melee);
    }

    /// <summary>
    /// Movement State Index
    /// 0: IdleState
    /// 1: JumpState
    /// 2: RunState
    /// 3: HitUncontrollableState
    /// 4: PunchHitStopMovementState
    /// 5: PunchHittedStopMovmentState
    /// 6: StunMovmentState
    /// 7: BazookaMovementAimState
    /// 8: BazookaMovmentLauchState
    /// 9: HookGUnStaticMovementState
    /// 10: DeadState
    /// </summary>
    private void _onMovementStateIndexChange()
    {
        switch (state.MovementStateIndex)
        {
            case 0:
                _movementFSM.TransitionTo<IdleState>();
                break;
            case 1:
                _movementFSM.TransitionTo<JumpState>();
                break;
            case 2:
                _movementFSM.TransitionTo<RunState>();
                break;
            case 3:
                _movementFSM.TransitionTo<HitUncontrollableState>();
                break;
            case 4:
                _movementFSM.TransitionTo<PunchHitStopMovementState>();
                break;
            case 5:
                _movementFSM.TransitionTo<PunchHittedStopMovementState>();
                break;
            case 6:
                _movementFSM.TransitionTo<StunMovementState>();
                break;
            case 7:
                _movementFSM.TransitionTo<BazookaMovmentAimState>();
                break;
            case 8:
                _movementFSM.TransitionTo<BazookaMovementLaunchState>();
                break;
            case 9:
                _movementFSM.TransitionTo<HookGunStaticMovementState>();
                break;
            case 10:
                _movementFSM.TransitionTo<DeadState>();
                break;
            default:
                _movementFSM.TransitionTo<IdleState>();
                break;
        }
    }

    /// <summary>
    /// Action State Index:
    /// 0: IdleActionState
    /// 1: HoldingState
    /// 2: DroppedRecoveryState
    /// 3: PunchHoldingState
    /// 4: PunchReleaseingState
    /// 5: HitUnControllableActionState
    /// 6: PunchHitStopActionState
    /// 7: PunchHittedStopAcitonState
    /// 8: BlockingState
    /// 9: BazookaActionState
    /// 10: WaterGunActionState
    /// 11: StunActionState
    /// 12: ActionDeadState
    /// </summary>
    private void _onActionStateIndexChange()
    {
        switch (state.ActionStateIndex)
        {
            case 0:
                _actionFSM.TransitionTo<IdleActionState>();
                break;
            case 1:
                _actionFSM.TransitionTo<HoldingState>();
                break;
            case 2:
                _actionFSM.TransitionTo<DroppedRecoveryState>();
                break;
            case 3:
                _actionFSM.TransitionTo<PunchHoldingState>();
                break;
            case 4:
                _actionFSM.TransitionTo<PunchReleasingState>();
                break;
            case 5:
                _actionFSM.TransitionTo<HitUnControllableActionState>();
                break;
            case 6:
                _actionFSM.TransitionTo<PunchHitStopActionState>();
                break;
            case 7:
                _actionFSM.TransitionTo<PunchHittedStopActionState>();
                break;
            case 8:
                _actionFSM.TransitionTo<BlockingState>();
                break;
            case 9:
                _actionFSM.TransitionTo<BazookaActionState>();
                break;
            case 10:
                _actionFSM.TransitionTo<WaterGunActionState>();
                break;
            case 11:
                _actionFSM.TransitionTo<StunActionState>();
                break;
            case 12:
                _actionFSM.TransitionTo<ActionDeadState>();
                break;
            default:
                _actionFSM.TransitionTo<IdleActionState>();
                break;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        _movementFSM.Update();
        _actionFSM.Update();

        if (entity.IsControllerOrOwner)
            _pollKeys();
    }

    private void FixedUpdate()
    {
        _movementFSM.FixedUpdate();
        _actionFSM.FixedUpdate();
    }

    private void LateUpdate()
    {
        _movementFSM.LateUpdate();
        _actionFSM.LateUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone") || other.CompareTag("DeathModeTrapZone"))
        {
            ((MovementState)_movementFSM.CurrentState).OnEnterDeathZone();
            ((ActionState)_actionFSM.CurrentState).OnEnterDeathZone();
            EventManager.Instance.TriggerEvent(new PlayerDied(gameObject, PlayerNumber, _impactMarker, other.gameObject));
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (_movementFSM.CurrentState != null)
            ((MovementState)_movementFSM.CurrentState).OnCollisionEnter(other);
    }

    public bool CanBeBlockPushed()
    {
        return _actionFSM.CurrentState.GetType().Equals(typeof(BlockingState)) ||
        _actionFSM.CurrentState.GetType().Equals(typeof(IdleActionState)) ||
        _actionFSM.CurrentState.GetType().Equals(typeof(PunchHoldingState)) ||
        _actionFSM.CurrentState.GetType().Equals(typeof(HoldingState));
    }
    /// <summary>
    /// Can Block The attack or not
    /// Used by hook and hit
    /// </summary>
    /// <param name="forwardAngle"></param>
    /// <returns></returns>
    public bool CanBlock(Vector3 forwardAngle)
    {
        // if (_actionFSM.CurrentState.GetType().Equals(typeof(BlockingState)) &&
        //     _angleWithin(transform.forward, forwardAngle, 180f - CharacterDataStore.BlockAngle))
        //     return true;
        // return false;
        return state.Blocking && _angleWithin(state.MainTransform.Transform.forward, forwardAngle, 180f - CharacterDataStore.BlockAngle);
    }

    /// <summary>
    /// This function is called when enemies want to impact the player
    /// </summary>
    /// <param name="force">The amount of force</param>
    /// <param name="forcemode">Force mode</param>
    /// <param name="enforcer">who is the impactor</param>
    public void OnImpact(Vector3 force, ForceMode forcemode, GameObject enforcer, ImpactType impactType)
    {
        OnImpact(enforcer, impactType);

        if (force.magnitude > CharacterDataStore.HitSmallThreshold)
        {
            _hitForceTuple = new ForceTuple(force, forcemode);
            _hitUncontrollableTimer = CharacterDataStore.HitUncontrollableTimeSmall;
            _hitStopFrames = CharacterDataStore.HitStopFramesSmall;
            if (force.magnitude > CharacterDataStore.HitBigThreshold)
            {
                _hitUncontrollableTimer = CharacterDataStore.HitUncontrollableTimeBig;
                _hitStopFrames = CharacterDataStore.HitStopFramesBig;
            }

            if (_movementFSM.CurrentState != null && (_movementFSM.CurrentState as MovementState).ShouldOnHitTransitToUncontrollableState)
            {
                // if (impactType == ImpactType.Melee || impactType == ImpactType.Block)
                //     _movementFSM.TransitionTo<PunchHittedStopMovementState>();
                // else
                // _isHit = true;
                // Debug.Log("On Impact actually impacted");
                _movementFSM.TransitionTo<HitUncontrollableState>();
            }

            if (_actionFSM.CurrentState != null && (_actionFSM.CurrentState as ActionState).ShouldOnHitTransitToUncontrollableState)
            {
                // if (impactType == ImpactType.Melee || impactType == ImpactType.Block)
                //     _actionFSM.TransitionTo<PunchHittedStopActionState>();
                // else
                // _isHit = true;
                _actionFSM.TransitionTo<HitUnControllableActionState>();
            }
        }
        else
        {
            _rb.AddForce(force, forcemode);
        }
    }

    public void OnImpact(GameObject enforcer, ImpactType impactType)
    {
        _impactMarker.SetValue(enforcer, Time.time, impactType);
    }

    public void OnImpact(Status status)
    {
        if (status.GetType().Equals(typeof(StunEffect)))
        {
            if (status.Duration < _stunTimer - Time.time) return;
            _stunTimer = Time.time + status.Duration;
            _movementFSM.TransitionTo<StunMovementState>();
            _actionFSM.TransitionTo<StunActionState>();
        }
        else if (status.GetType().Equals(typeof(SlowEffect)))
        {
            if (1f - _walkSpeedMultiplier > status.Potency) return;
            /// If Slow Potency are similar
            /// Refresh the timer if duration is longer
            /// If Potency is bigger, then refresh everything
            if (Mathf.Approximately(1f - _walkSpeedMultiplier, status.Potency))
            {
                _slowTimer = _slowTimer - Time.time > status.Duration ? _slowTimer : Time.time + status.Duration;
            }
            else
            {
                _slowTimer = Time.time + status.Duration;
                _walkSpeedMultiplier = 1f - status.Potency;
            }
        }
        else if (status.GetType().Equals(typeof(PermaSlowEffect)))
        {
            _permaSlow++;
            EventManager.Instance.TriggerEvent(new PlayerSlowed(gameObject, PlayerNumber, OnDeathHidden[1]));
            if (1f - _permaSlowWalkSpeedMultiplier > status.Potency) return;
            else
            {
                _permaSlowWalkSpeedMultiplier = 1f - status.Potency;
            }
        }
        else if (status.GetType().Equals(typeof(RemovePermaSlowEffect)))
        {
            _permaSlow--;
            if (_permaSlow == 0) EventManager.Instance.TriggerEvent(new PlayerUnslowed(gameObject, PlayerNumber, OnDeathHidden[1]));
        }
    }

    public void ForceDropHandObject()
    {
        if ((_actionFSM.CurrentState as ActionState).ShouldDropHandObjectWhenForced)
        {
            _actionFSM.TransitionTo<DroppedRecoveryState>();
            _dropHandObject();
        }
    }

    public void ForceDropHandObject(Vector3 force)
    {
        if ((_actionFSM.CurrentState as ActionState).ShouldDropHandObjectWhenForced)
        {
            _actionFSM.TransitionTo<DroppedRecoveryState>();
            _dropHandObject(true, force);
        }
    }

    /// <summary>
    /// This function is called from FootSteps on LegSwingRefernece
    /// </summary>
    /// <param name="foot">0 is right foot, 1 is left foot</param>
    public void FootStep(int foot = 0)
    {
        if (_isGrounded())
        {
            EventManager.Instance.TriggerEvent(new FootStep(OnDeathHidden[1], foot == 0 ? RightFoot : LeftFoot, _getGroundTag(), gameObject, foot));
        }
    }

    public void HookedStatic(bool start)
    {
        if (start)
        {
            _movementFSM.TransitionTo<HookGunStaticMovementState>();
        }
        else
        {
            _movementFSM.TransitionTo<IdleState>();
        }
    }

    public void SetVelocity(Vector3 vel)
    {
        foreach (Rigidbody rb in _allPlayerRBs)
        {
            rb.velocity = vel;
        }
    }

    #region Helper Method
    private bool _frontIsCliff()
    {
        RaycastHit hit;
        float colliderRadius = GetComponent<CapsuleCollider>().radius;
        Physics.Raycast(transform.position + transform.forward * (colliderRadius + CharacterDataStore.FrontIsCliff),
                        Vector3.down,
                        out hit,
                        50f,
                        CharacterDataStore.JumpMask);
        if (hit.collider == null) return true;
        return false;
    }
    private void _setToSpawn(float yOffset)
    {
        int colorindex = Utility.GetColorIndexFromPlayer(gameObject);
        if (PlayerID.PlayerTeamNumber == 0)
        {
            Vector3 pos = Services.Config.Team1RespawnPoints[colorindex - 2];
            pos.y += yOffset;
            transform.position = pos;
        }
        else
        {
            Vector3 pos = Services.Config.Team2RespawnPoints[colorindex];
            pos.y += yOffset;
            transform.position = pos;
        }
    }

    private string _getGroundTag()
    {
        RaycastHit hit;
        Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, _distToGround, CharacterDataStore.JumpMask);
        if (hit.collider == null) return "";
        return hit.collider.tag;
    }

    private bool _angleWithin(Vector3 A, Vector3 B, float degree)
    {
        return Vector3.Angle(A, B) > degree;
    }

    private void _dropHandObject(bool customDrop = false, Vector3 customDropForce = default(Vector3))
    {
        if (HandObject == null) return;
        // Drop the thing
        HandObject.GetComponent<WeaponBase>().OnDrop(customDrop, customDropForce);

        EventManager.Instance.TriggerEvent(new ObjectDropped(gameObject, PlayerNumber, HandObject));
        // Nullify the holder
        HandObject = null;
    }

    private IEnumerator _deadInvincibleIenumerator(float time)
    {
        foreach (Rigidbody rb in _playerBodies)
        {
            rb.gameObject.layer = LayerMask.NameToLayer("ReviveInvincible");
        }

        yield return new WaitForSeconds(time);

        foreach (Rigidbody rb in _playerBodies)
        {
            rb.gameObject.layer = _playerBodiesLayer;
        }
    }

    private void _helpAim(float maxangle, float maxRange)
    {
        GameObject target = null;
        float minAngle = 360f;
        GameObject[] enemies = PlayerID.PlayerTeamNumber == 0 ? GameObject.FindGameObjectsWithTag("Team2") : GameObject.FindGameObjectsWithTag("Team1");
        foreach (GameObject otherPlayer in enemies)
        {
            if (otherPlayer.activeSelf)
            {
                // If other player are within max Distance, then check for the smalliest angle player
                if (Vector3.Distance(otherPlayer.transform.position, gameObject.transform.position) <= maxRange)
                {
                    Vector3 targetDir = otherPlayer.transform.position - transform.position;
                    float angle = Vector3.Angle(targetDir, transform.forward);
                    if (angle <= maxangle && angle < minAngle)
                    {
                        minAngle = angle;
                        target = otherPlayer;
                    }
                }
            }
        }
        // Now we got the target Player, time to auxillary against it
        if (target != null)
        {
            transform.LookAt(target.transform);
            if (HandObject != null)
            {
                HandObject.transform.eulerAngles = transform.eulerAngles + new Vector3(0f, 90f, 0f);
            }
        }
    }

    private bool _isGrounded()
    {
        RaycastHit hit;
        return Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, _distToGround, CharacterDataStore.JumpMask);
    }

    private bool _canDrainStamina(float drain)
    {
        if (_currentStamina > drain)
            return true;
        else
        {
            // BlockUIVFXHolder.SetActive(true);
            // BlockUIVFXHolder.GetComponent<DOTweenAnimation>().DORestart();
            return false;
        }
    }

    private void _drainStamina(float drain)
    {
        // if (drain <= 0f) return;
        if (_currentStamina - drain < 0f)
            _currentStamina = 0f;
        else if (_currentStamina - drain > CharacterDataStore.MaxStamina)
        {
            _currentStamina = CharacterDataStore.MaxStamina;
        }
        else
        {
            _currentStamina -= drain;
        }

        if (drain > 0f)
        {
            _lastTimeUseStamina = Time.timeSinceLevelLoad;
        }
        BlockShield.SetEnergy(_currentStamina / CharacterDataStore.MaxStamina);

        // BlockUIVFXHolder.SetActive(true);
        // Vector2 _nextStaminaUISize = _staminaUISize;
        // _nextStaminaUISize.x *= _currentStamina / CharacterDataStore.MaxStamina;
        // BlockUIVFXHolder.transform.GetChild(0).GetComponent<SpriteRenderer>().size = _nextStaminaUISize;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
    #endregion

    #region Movment States
    private class MovementState : FSM<BoltPlayerController>.State
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
                Parent.TransitionTo<DeadState>();
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

        public override void OnEnter()
        {
            base.OnEnter();
            print(GetType().Name);
            if (Context.entity.IsOwner)
                Context.state.MovementStateIndex = _stateIndex;
        }
    }

    private class ControllableMovementState : MovementState
    {
        public override void LateUpdate()
        {
            base.LateUpdate();
            Context._freezeBody.y = Context.transform.localEulerAngles.y;
            Context.transform.localEulerAngles = Context._freezeBody;
        }
    }

    private class IdleState : ControllableMovementState
    {
        protected override int _stateIndex { get { return 0; } }

        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
                Context.state.IdleDowner = true;
            // Context._animator.SetBool("IdleDowner", true);
        }

        public override void Update()
        {
            base.Update();
            if (_HLAxisRaw != 0f || _VLAxisRaw != 0f)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<RunState>();
                    return;
                }

            }

            if (_jump && Context._isGrounded() && Context._jumpTimer < Time.timeSinceLevelLoad)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<JumpState>();
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
            // Context._animator.SetBool("IdleDowner", false);
        }
    }

    private class JumpState : ControllableMovementState
    {
        protected override int _stateIndex { get { return 1; } }
        public override void OnEnter()
        {
            base.OnEnter();
            Context._rb.AddForce(new Vector3(0, Context.CharacterDataStore.JumpForce, 0), ForceMode.Impulse);
            EventManager.Instance.TriggerEvent(new PlayerJump(Context.gameObject, Context.OnDeathHidden[1], Context.PlayerNumber, Context._getGroundTag()));
        }

        public override void OnCollisionEnter(Collision other)
        {
            if (Context.CharacterDataStore.JumpMask == (Context.CharacterDataStore.JumpMask | (1 << other.gameObject.layer)))
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleState>();
                }
                EventManager.Instance.TriggerEvent(new PlayerLand(Context.gameObject, Context.OnDeathHidden[1], Context.PlayerNumber, Context._getGroundTag()));
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (_HLAxis != 0f || _VLAxis != 0f)
            {
                Vector3 targetVelocity = Context.transform.forward * Context.CharacterDataStore.WalkSpeed * Context._walkSpeed;
                Vector3 velocityChange = targetVelocity - Context._rb.velocity;
                velocityChange.x = Mathf.Clamp(velocityChange.x, -Context.CharacterDataStore.MaxVelocityChange, Context.CharacterDataStore.MaxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -Context.CharacterDataStore.MaxVelocityChange, Context.CharacterDataStore.MaxVelocityChange);
                velocityChange.y = 0f;

                Context._rb.AddForce(velocityChange * Context.CharacterDataStore.InAirSpeedMultiplier, ForceMode.VelocityChange);

                Vector3 relPos = Quaternion.AngleAxis(Mathf.Atan2(_HLAxis, _VLAxis * -1f) * Mathf.Rad2Deg, Context.transform.up) * Vector3.forward;
                Quaternion rotation = Quaternion.LookRotation(relPos, Vector3.up);
                Quaternion tr = Quaternion.Slerp(Context.transform.rotation, rotation, Time.deltaTime * Context.CharacterDataStore.MinRotationSpeed * Context._rotationSpeedMultiplier);
                Context.transform.rotation = tr;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._jumpTimer = Time.timeSinceLevelLoad + Context.CharacterDataStore.JumpCD;
        }
    }

    private class RunState : ControllableMovementState
    {
        protected override int _stateIndex { get { return 2; } }

        private float _runTowardsCliffTime;
        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
                Context.state.Running = true;
            // Context._animator.SetBool("Running", true);
            _runTowardsCliffTime = 0f;
        }

        public override void Update()
        {
            base.Update();
            if (_HLAxisRaw == 0f && _VLAxisRaw == 0f)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleState>();
                    return;
                }

            }
            if (_jump && Context._isGrounded() && Context._jumpTimer < Time.timeSinceLevelLoad)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<JumpState>();
                    return;
                }

            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            bool isonground = Context._isGrounded();
            Vector3 targetVelocity = Context.transform.forward * Context.CharacterDataStore.WalkSpeed * Context._walkSpeed;
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
            Quaternion tr = Quaternion.Slerp(Context.transform.rotation, rotation, Time.deltaTime * Context.CharacterDataStore.MinRotationSpeed * Context._rotationSpeedMultiplier);
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
            // Context._animator.SetBool("Running", false);
        }
    }
    private class HitUncontrollableState : ControllableMovementState
    {
        protected override int _stateIndex { get { return 3; } }

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
                    TransitionTo<IdleState>();
                    return;
                }

            }
        }
    }

    private class PunchHitStopMovementState : ControllableMovementState
    {
        protected override int _stateIndex { get { return 4; } }

        private int _counter;
        private int _firstPhysicsFrame;

        public override void OnEnter()
        {
            base.OnEnter();
            _counter = 0;
            _firstPhysicsFrame = 2;

        }

        public override void Update()
        {
            base.Update();
            if (_firstPhysicsFrame > 0) return;
            _counter++;
            if (_counter > Context._hitStopFrames)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleState>();
                    return;
                }

            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _firstPhysicsFrame--;
        }

    }

    private class PunchHittedStopMovementState : ControllableMovementState
    {
        protected override int _stateIndex { get { return 5; } }

        private int _counter;
        private int _firstPhysicsFrame;
        private Tweener _shakeTween;

        public override void OnEnter()
        {
            base.OnEnter();
            _counter = 0;
            _firstPhysicsFrame = 2;
        }

        public override void Update()
        {
            base.Update();
            if (_firstPhysicsFrame > 0) return;
            if (_shakeTween == null || !_shakeTween.IsPlaying()) _shakeTween = Context.transform.DOShakePosition(Time.unscaledDeltaTime * Context._hitStopFrames, Context.CharacterDataStore.HitStopViberation, Context.CharacterDataStore.HitStopViberato,
        Context.CharacterDataStore.HitStopRandomness).SetEase(Context.CharacterDataStore.HitStopViberationEase);
            _counter++;
            if (_counter > Context._hitStopFrames)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<HitUncontrollableState>();
                    return;
                }

            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _firstPhysicsFrame--;
        }

        public override void OnExit()
        {
            base.OnExit();
            _shakeTween.Kill();
        }
    }

    private class StunMovementState : MovementState
    {
        protected override int _stateIndex { get { return 6; } }

        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
                Context.state.IdleDowner = true;
            // Context._animator.SetBool("IdleDowner", true);
            EventManager.Instance.TriggerEvent(new PlayerStunned(Context.gameObject, Context.PlayerNumber, Context.Head.transform, Context._stunTimer - Time.time));
        }

        public override void Update()
        {
            base.Update();
            if (Time.time > Context._stunTimer)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleState>();
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            EventManager.Instance.TriggerEvent(new PlayerUnStunned(Context.gameObject, Context.PlayerNumber));
        }
    }

    private class BazookaMovmentAimState : MovementState
    {
        protected override int _stateIndex { get { return 7; } }

        public override bool ShouldOnHitTransitToUncontrollableState { get { return false; } }

        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
                Context.state.IdleDowner = true;
            // Context._animator.SetBool("IdleDowner", true);
        }
        public override void Update()
        {
            base.Update();
            Vector3 lookpos = Context.HandObject.GetComponent<rtBazooka>().BazookaShadowTransformPosition;
            lookpos.y = Context.transform.position.y;
            Context.transform.LookAt(lookpos);
            if (_RightTriggerUp)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<BazookaMovementLaunchState>();
                }
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
                Context.state.IdleDowner = false;
            // Context._animator.SetBool("IdleDowner", false);
        }
    }

    private class BazookaMovementLaunchState : MovementState
    {
        protected override int _stateIndex { get { return 8; } }

        private Vector3 _diff;
        public override bool ShouldOnHitTransitToUncontrollableState { get { return false; } }
        public override void OnEnter()
        {
            base.OnEnter();
            _diff = Context.HandObject.transform.position - Context.transform.position;
        }

        public override void Update()
        {
            base.Update();
            bool isrunning = (!Mathf.Approximately(_HLAxis, 0f) || !Mathf.Approximately(0f, _VLAxis));
            if (Context.entity.IsOwner)
            {
                Context.state.Running = isrunning;
                Context.state.IdleDowner = !isrunning;
            }

            // Context._animator.SetBool("Running", isrunning);
            // Context._animator.SetBool("IdleDowner", !isrunning);
            Context.transform.position = Context.HandObject.transform.position - _diff;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
                Context.state.Running = false;
            // Context._animator.SetBool("Running", false);
        }
    }

    private class HookGunStaticMovementState : MovementState
    {
        protected override int _stateIndex { get { return 9; } }
        private Vector3 _diff;

        public override void OnEnter()
        {
            base.OnEnter();
            foreach (Rigidbody rb in Context.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = true;
            }
            _diff = Context.HandObject.transform.position - Context.transform.position;
        }

        public override void Update()
        {
            base.Update();
            Context.transform.position = Context.HandObject.transform.position - _diff;
        }

        public override void OnExit()
        {
            base.OnExit();
            foreach (Rigidbody rb in Context.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = false;
            }
        }
    }

    private class DeadState : FSM<BoltPlayerController>.State
    {
        private float _startTime;
        private float _respawnTime { get { return Services.Config.GameMapData.RespawnTime; } }

        public override void OnEnter()
        {
            base.OnEnter();
            _startTime = Time.time;
            Context._rb.isKinematic = true;
            Context._setToSpawn(10f);
            if (Context.entity.IsOwner)
            {
                Context.state.IdleDowner = true;
                Context.state.MovementStateIndex = 10;
            }
            // Context._animator.SetBool("IdleDowner", true);
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
                    EventManager.Instance.TriggerEvent(new PlayerRespawned(Context.gameObject));
                    TransitionTo<IdleState>();
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

    #region Action States
    private class ActionState : FSM<BoltPlayerController>.State
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

    private class IdleActionState : ActionState
    {
        protected override int _stateIndex { get { return 0; } }

        private float _pickUpTimer;
        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }
        private float _emojiTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            Context._dropHandObject();
            if (Context.entity.IsOwner)
                Context.state.IdleUpper = true;
            // Context._animator.SetBool("IdleUpper", true);
            Context._permaSlow = 0;
            Context._permaSlowWalkSpeedMultiplier = 1f;
        }

        public override void Update()
        {
            base.Update();
            if (_RightTrigger)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<PunchHoldingState>();
                    return;
                }

            }
            if (_B && Context._canDrainStamina(0.1f))
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<BlockingState>();
                    return;
                }

            }

            if (_QDown && _emojiTimer < Time.time)
            {
                _emojiTimer = Time.time + 0.3f;
                EventManager.Instance.TriggerEvent(new TriggerEmoji(0, Context.gameObject));
            }
            _pickupcheck();
        }

        private void _pickupcheck()
        {
            Collider[] colliders = Physics.OverlapSphere(Context.transform.position, Context.CharacterDataStore.PickupRadius,
            Context.CharacterDataStore.PickUpLayer);
            GameObject target = null;
            float minDistance = Mathf.Infinity;
            foreach (Collider coll in colliders)
            {
                float dist = Vector3.Distance(coll.transform.position, Context.transform.position);
                if (dist < minDistance)
                {
                    target = coll.gameObject;
                    minDistance = dist;
                }
            }
            if (target == null) return;
            if (Context.HandObject == null && target.GetComponent<WeaponBase>() != null && target.GetComponent<WeaponBase>().CanBePickedUp)
            {
                if (Context.entity.IsOwner)
                {
                    EventManager.Instance.TriggerEvent(new ObjectPickedUp(Context.gameObject, Context.PlayerNumber, target.gameObject));
                    // Tell other necessary components that it has taken something
                    Context.HandObject = target.gameObject;

                    // Tell the collected weapon who picked it up
                    target.GetComponent<WeaponBase>().OnPickUp(Context.gameObject);
                    TransitionTo<HoldingState>();
                    return;
                }

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
                Context.state.IdleUpper = false;
            // Context._animator.SetBool("IdleUpper", false);
        }
    }

    private class HoldingState : ActionState
    {
        protected override int _stateIndex { get { return 1; } }

        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }
        public override bool ShouldDropHandObjectWhenForced { get { return true; } }

        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Assert(Context.HandObject != null);
            switch (Context.HandObject.tag)
            {
                case "Weapon_OnChest":
                    if (Context.entity.IsOwner)
                        Context.state.PickUpHalf = true;
                    // Context._animator.SetBool("PickUpHalf", true);
                    break;
                case "Team1Resource":
                case "Team2Resource":
                case "Weapon_OnHead":
                    if (Context.entity.IsOwner)
                        Context.state.PickUpFull = true;
                    // Context._animator.SetBool("PickUpFull", true);
                    break;
                default:
                    if (Context.entity.IsOwner)
                        Context.state.IdleUpper = true;
                    // Context._animator.SetBool("IdleUpper", true);
                    break;
            }
            Context._permaSlow++;
            Context._permaSlowWalkSpeedMultiplier = Context.HandObject.GetComponent<WeaponBase>().WeaponDataBase.PickupSlowMultiplier;
        }

        public override void Update()
        {
            base.Update();
            if (_BDown)
            {
                if (Context.entity.IsOwner)
                {
                    Context._dropHandObject();
                    TransitionTo<DroppedRecoveryState>();
                    return;
                }

            }
            if (_RightTriggerDown)
            {
                WeaponBase wb = Context.HandObject.GetComponent<WeaponBase>();
                Context._helpAim(wb.HelpAimAngle, wb.HelpAimDistance);
                Context.HandObject.GetComponent<WeaponBase>().Fire(true);
                if (Context.HandObject == null) return;

                if (Context.HandObject.GetComponent<WeaponBase>().GetType().Equals(typeof(rtBazooka)))
                {
                    if (Context.entity.IsOwner)
                    {
                        Context._movementFSM.TransitionTo<BazookaMovmentAimState>();
                        TransitionTo<BazookaActionState>();
                        return;
                    }

                }
                else if (Context.HandObject.GetComponent<WeaponBase>().GetType().Equals(typeof(rtSmallBaz)))
                {
                    if (Context.entity.IsOwner)
                    {
                        TransitionTo<IdleActionState>();
                        return;
                    }

                }
                else if (Context.HandObject.GetComponent<WeaponBase>().GetType().Equals(typeof(rtEmit)))
                {
                    if (Context.entity.IsOwner)
                    {
                        TransitionTo<WaterGunActionState>();
                        return;
                    }

                }
            }
            if (_RightTriggerUp)
            {
                Context.HandObject.GetComponent<WeaponBase>().Fire(false);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
            {
                Context.state.PickUpFull = false;
                Context.state.PickUpHalf = false;
            }
            // Context._animator.SetBool("PickUpFull", false);
            // Context._animator.SetBool("PickUpHalf", false);
        }
    }

    private class DroppedRecoveryState : ActionState
    {
        protected override int _stateIndex { get { return 2; } }

        private float _timer;
        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }

        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
                Context.state.IdleUpper = true;
            // Context._animator.SetBool("IdleUpper", true);
            _timer = Time.timeSinceLevelLoad + Context.CharacterDataStore.DropRecoveryTime;
        }

        public override void Update()
        {
            base.Update();
            if (Time.timeSinceLevelLoad > _timer)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleActionState>();
                    return;
                }

            }
        }
    }

    private class PunchHoldingState : ActionState
    {
        protected override int _stateIndex { get { return 3; } }

        private float _startHoldingTime;
        private bool _holding;
        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }
        private bool _triggerdHoldingEvent;

        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
            {
                Context.state.ClockFistTime = 1f / Context.CharacterDataStore.ClockFistTime;
                Context.state.PunchHolding = true;
            }
            // Context._animator.SetFloat("ClockFistTime", 1f / Context.CharacterDataStore.ClockFistTime);
            // Context._animator.SetBool("PunchHolding", true);
            Context._permaSlow++;
            Context._permaSlowWalkSpeedMultiplier = Context.CharacterDataStore.FistHoldSpeedMultiplier;
            Context._rotationSpeedMultiplier = Context.CharacterDataStore.FistHoldRotationMutiplier;
            Services.BoltEventBroadcaster.OnPunchStart(new PunchStart(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
            _holding = false;
            _triggerdHoldingEvent = false;
            _startHoldingTime = Time.time;
        }

        public override void Update()
        {
            base.Update();
            if (!_triggerdHoldingEvent && Time.time > _startHoldingTime + Context.CharacterDataStore.HoldEventTriggerDuration)
            {
                _triggerdHoldingEvent = true;
                Services.BoltEventBroadcaster.OnPunchHolding(new PunchHolding(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
            }
            if (!_holding && Time.time > _startHoldingTime + Context.CharacterDataStore.ClockFistTime)
            {
                _holding = true;
            }

            if (_holding && _BDown)
            {
                if (Context.entity.IsOwner)
                {
                    Services.BoltEventBroadcaster.OnPunchDone(new PunchDone(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
                    TransitionTo<BlockingState>();
                    return;
                }

            }
            if (_holding && !_RightTrigger)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<PunchReleasingState>();
                    return;
                }

            }
            if (_RightTriggerUp && _holding)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<PunchReleasingState>();
                    return;
                }

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context.entity.IsOwner)
                Context.state.PunchHolding = false;
            // Context._animator.SetBool("PunchHolding", false);
            Context._permaSlow--;
            Context._permaSlowWalkSpeedMultiplier = 1f;
            Context._rotationSpeedMultiplier = 1f;
        }
    }

    private class PunchReleasingState : ActionState
    {
        protected override int _stateIndex { get { return 4; } }

        private float _time;
        private bool _hitOnce;
        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }
        // private BirfiaPlayerCommand _cmd;

        public override void ExecuteCommand(Command command, bool resetState)
        {
            base.ExecuteCommand(command, resetState);
            if (Time.time < _time + Context.CharacterDataStore.PunchActivateTime)
            {
                _checkHitBolt((BirfiaPlayerCommand)command);
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (Context.entity.IsOwner)
            {
                Context.state.FistReleaseTime = 1f / Context.CharacterDataStore.FistReleaseTime;
                Context.state.PunchReleased = true;
            }
            // Context._animator.SetFloat("FistReleaseTime", 1f / Context.CharacterDataStore.FistReleaseTime);
            // Context._animator.SetBool("PunchReleased", true);
            _time = Time.time;
            _hitOnce = false;
            Context._helpAim(Context.CharacterDataStore.PunchHelpAimAngle, Context.CharacterDataStore.PunchHelpAimDistance);
            if (Context._movementFSM.CurrentState.GetType().Equals(typeof(IdleState)))
                Context._rb.AddForce(Context.transform.forward * Context.CharacterDataStore.IdleSelfPushForce, ForceMode.VelocityChange);
            else
                Context._rb.AddForce(Context.transform.forward * Context.CharacterDataStore.SelfPushForce, ForceMode.VelocityChange);
            Services.BoltEventBroadcaster.OnPunchReleased(new PunchReleased(Context.gameObject, Context.PlayerNumber));
            Context._rotationSpeedMultiplier = Context.CharacterDataStore.PunchReleaseRotationMultiplier;
        }

        public override void Update()
        {
            base.Update();
            if (Time.time > _time + Context.CharacterDataStore.FistReleaseTime)
            {
                if (Context.entity.IsOwner)
                {
                    Services.BoltEventBroadcaster.OnPunchDone(new PunchDone(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
                    TransitionTo<IdleActionState>();
                    return;
                }

            }
            // if (Time.time < _time + Context.CharacterDataStore.PunchActivateTime)
            // {
            //     // _checkHit();
            //     _checkHitBolt();
            // }
        }

        public override void OnExit()
        {
            base.OnExit();
            // _checkHit();
            // Context._animator.SetBool("PunchReleased", false);
            if (Context.entity.IsOwner)
                Context.state.PunchReleased = false;
            Context._rotationSpeedMultiplier = 1f;
        }

        private void _checkHitBolt(BirfiaPlayerCommand cmd)
        {
            if (_hitOnce) return;
            // Bolt Hitscan
            using (var hits = BoltNetwork.OverlapSphereAll(Context.entity.transform.position, Context.CharacterDataStore.PunchRadius, cmd.ServerFrame))
            {
                for (int i = 0; i < hits.count; i++)
                {
                    var hit = hits.GetHit(i);
                    if (Vector3.Angle(Context.entity.transform.forward, hit.body.transform.position - Context.entity.transform.position) < 30f)
                    {
                        // Hit,  TODO: convert to single hit TODO: detect teammate
                        Vector3 force = Context.entity.transform.forward * Context.CharacterDataStore.PunchForce;

                        var serializer = hit.body.GetComponent<BoltPlayerController>();
                        if (serializer != null && serializer != Context.entity.GetComponent<BoltPlayerController>())
                        // if (serializer != null)
                        {
                            Context.SetVelocity(Vector3.zero);
                            PunchEvent.Post(serializer.entity, force, Context.entity);
                            Services.BoltEventBroadcaster.OnPlayerHit(new PlayerHit(Context.entity.gameObject, serializer.gameObject, force, Context.entity.gameObject.GetComponent<BoltPlayerController>().PlayerNumber, serializer.PlayerNumber, 1f, false));
                            _hitOnce = true;
                        }
                    }
                }
            }
        }

        private void _checkHit()
        {
            RaycastHit hit;
            // This Layermask get all player's layer except this player's
            int layermask = 0;
            if (Context.gameObject.layer == LayerMask.NameToLayer("ReviveInvincible")) layermask = Context.CharacterDataStore.CanHitLayer;
            else layermask = Context.CharacterDataStore.CanHitLayer ^ (1 << Context.gameObject.layer);
            if (!_hitOnce && Physics.SphereCast(Context.transform.position - Context.transform.forward * Context.CharacterDataStore.PunchBackwardCastDistance, Context.CharacterDataStore.PunchRadius, Context.transform.forward, out hit, Context.CharacterDataStore.PunchDistance, layermask))
            {
                GameObject target = null;
                if (hit.transform.GetComponent<WeaponBase>() != null)
                {
                    target = hit.transform.GetComponent<WeaponBase>().Owner;
                }
                else if (hit.transform.GetComponentInParent<IHittable>() != null)
                {
                    target = hit.transform.GetComponentInParent<IHittable>().GetGameObject();
                }
                else return;
                if (target == null) return;
                _hitOnce = true;
                Vector3 force = Context.transform.forward * Context.CharacterDataStore.PunchForce;
                Context._hitStopFrames = Context.CharacterDataStore.HitStopFramesSmall;
                if (target.GetComponent<IHittable>().CanBlock(Context.transform.forward))
                {
                    force *= (-Context.CharacterDataStore.BlockMultiplier);
                    // EventManager.Instance.TriggerEvent(new PlayerHit(target, Context.gameObject, force, target.GetComponent<PlayerController>().PlayerNumber, Context.PlayerNumber, 1f, true));
                    // Context.OnImpact(force, ForceMode.Impulse, target, ImpactType.Block);
                    Context.GetComponent<IHittable>().SetVelocity(Vector3.zero);
                    Services.ActionManager.RegisterAction(Time.frameCount, new MeleeHitAction(Context.gameObject, target, force, ForceMode.Impulse, ImpactType.Block));
                }
                else
                {
                    // EventManager.Instance.TriggerEvent(new PlayerHit(Context.gameObject, target, force, Context.PlayerNumber, target.GetComponent<PlayerController>().PlayerNumber, 1f, false));
                    // if (Time.time > Context._impactMarker.PlayerMarkedTime + Context.CharacterDataStore.PunchResetVelocityBeforeHitDuration)
                    //     Context.SetVelocity(Vector3.zero);
                    // target.GetComponent<IHittable>().OnImpact(force, ForceMode.Impulse, Context.gameObject, ImpactType.Melee);
                    target.GetComponent<IHittable>().SetVelocity(Vector3.zero);
                    Services.ActionManager.RegisterAction(Time.frameCount, new MeleeHitAction(target, Context.gameObject, force, ForceMode.Impulse, ImpactType.Melee));
                }
                // TransitionTo<PunchHitStopActionState>();
                // Context._movementFSM.TransitionTo<PunchHitStopMovementState>();
                return;
            }
        }
    }

    private class HitUnControllableActionState : ActionState
    {
        protected override int _stateIndex { get { return 5; } }

        private float _timer;
        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }
        private int myLayer;
        private HashSet<GameObject> _sweepedObjects;
        public override void OnEnter()
        {
            base.OnEnter();
            Context._dropHandObject();
            _sweepedObjects = new HashSet<GameObject>();
            _timer = Time.timeSinceLevelLoad + Context._hitUncontrollableTimer;
            Services.BoltEventBroadcaster.OnPunchInterrepted(new PunchInterruptted(Context.gameObject, Context.PlayerNumber));
            myLayer = Context.gameObject.layer;
            Context.gameObject.layer = 19;
            Context._rb.AddForce(Context._hitForceTuple.Force, Context._hitForceTuple.ForceMode);
            // _sweepInHitDirection();
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
                    TransitionTo<IdleActionState>();
                    return;
                }

            }
            // _sweepInHitDirection();
        }

        private void _sweepInHitDirection()
        {
            RaycastHit[] hits = Physics.SphereCastAll(Context.transform.position, Context.CharacterDataStore.HitSweepRadius, Context._rb.velocity.normalized,
            Context.CharacterDataStore.HitSweepBackwardDistance, Context.CharacterDataStore.CanHitLayer);
            for (int i = 0; i < hits.Length; i++)
            {
                Transform hit = hits[i].transform;
                GameObject target = null;
                // if (hit.GetComponent<WeaponBase>() != null && hit.transform.GetComponent<WeaponBase>().Owner != null)
                //     target = hit.transform.GetComponent<WeaponBase>().Owner;
                if (hit.transform.GetComponentInParent<IHittable>() != null)
                    target = hit.transform.GetComponentInParent<IHittable>().GetGameObject();
                else continue;
                if (target == Context.gameObject) continue;
                if (_sweepedObjects.Contains(target)) continue;
                _sweepedObjects.Add(target);
                Vector3 targetToPlayerDirection = (target.transform.position - Context.transform.position).normalized;
                Vector3 playerMovingDirection = Context._rb.velocity.normalized;
                Vector3 projected = Vector3.Project(targetToPlayerDirection, playerMovingDirection);
                Vector3 temp = (targetToPlayerDirection - projected);
                temp.y = 0f;
                Vector3 result = temp.normalized * Context.CharacterDataStore.HitSweepPushBackForce;
                // target.GetComponent<IHittable>().OnImpact(result, ForceMode.VelocityChange, Context.gameObject, ImpactType.Melee);
                Services.ActionManager.RegisterAction(Time.frameCount, new HitAction(target, Context.gameObject, result, ForceMode.VelocityChange, ImpactType.Melee));
            }
        }
    }

    private class PunchHitStopActionState : ActionState
    {
        protected override int _stateIndex { get { return 6; } }

        private int _counter;
        private Vector3[] _storedVelocity;
        private int _firstPhysicsFrame;
        public override void OnEnter()
        {
            base.OnEnter();
            _counter = 0;
            _firstPhysicsFrame = 2;
            _storedVelocity = new Vector3[Context._allPlayerRBs.Length];

        }

        public override void Update()
        {
            base.Update();
            if (_firstPhysicsFrame > 0) return;
            _counter++;
            if (_counter > Context._hitStopFrames)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleActionState>();
                    return;
                }

            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _firstPhysicsFrame--;
            if (_firstPhysicsFrame == 0)
            {
                for (int i = 0; i < _storedVelocity.Length; i++)
                {
                    _storedVelocity[i] = Context._allPlayerRBs[i].velocity;
                    Context._allPlayerRBs[i].isKinematic = true;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            for (int i = 0; i < _storedVelocity.Length; i++)
            {
                Context._allPlayerRBs[i].velocity = _storedVelocity[i];
                Context._allPlayerRBs[i].isKinematic = false;
            }
            Services.BoltEventBroadcaster.OnPunchDone(new PunchDone(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
        }
    }

    private class PunchHittedStopActionState : ActionState
    {
        protected override int _stateIndex { get { return 7; } }
        private int _counter;
        private Vector3[] _storedVelocity;
        private int _firstPhysicsFrame;
        public override void OnEnter()
        {
            base.OnEnter();
            _counter = 0;
            _firstPhysicsFrame = 2;
            _storedVelocity = new Vector3[Context._allPlayerRBs.Length];

        }

        public override void Update()
        {
            base.Update();
            if (_firstPhysicsFrame > 0) return;
            _counter++;
            if (_counter > Context._hitStopFrames)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<HitUnControllableActionState>();
                    return;
                }

            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _firstPhysicsFrame--;
            if (_firstPhysicsFrame == 0)
            {
                for (int i = 0; i < _storedVelocity.Length; i++)
                {
                    _storedVelocity[i] = Context._allPlayerRBs[i].velocity;
                    Context._allPlayerRBs[i].isKinematic = true;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            for (int i = 0; i < _storedVelocity.Length; i++)
            {
                Context._allPlayerRBs[i].velocity = _storedVelocity[i];
                Context._allPlayerRBs[i].isKinematic = false;
            }
        }
    }

    private class BlockingState : ActionState
    {
        protected override int _stateIndex { get { return 8; } }

        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }
        private float _blockPutDownTimer;

        public override void OnEnter()
        {
            base.OnEnter();
            EventManager.Instance.TriggerEvent(new BlockStart(Context.gameObject, Context.PlayerNumber));
            // Context._animator.SetBool("Blocking", true);
            if (Context.entity.IsOwner)
                Context.state.Blocking = true;
            Context.BlockShield.SetShield(true);
        }

        public override void Update()
        {
            base.Update();
            if (_BUp)
            {
                _blockPutDownTimer = Time.timeSinceLevelLoad + Context.CharacterDataStore.BlockLingerDuration;
            }

            if (!_B && _blockPutDownTimer < Time.timeSinceLevelLoad)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleActionState>();
                    return;
                }
            }
            Context._drainStamina(Time.deltaTime * Context.CharacterDataStore.BlockStaminaDrain);

            if (Context._currentStamina <= 0f)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleActionState>();
                    return;
                }

            }
            if (_RightTriggerDown)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<PunchHoldingState>();
                    return;
                }
            }
            _blockPush();
        }

        private void _blockPush()
        {
            RaycastHit hit;
            // This Layermask get all player's layer except this player's
            int layermask = 0;
            if (Context.gameObject.layer == LayerMask.NameToLayer("ReviveInvincible")) layermask = Context.CharacterDataStore.CanHitLayer;
            else layermask = Context.CharacterDataStore.CanHitLayer ^ (1 << Context.gameObject.layer);
            if (Physics.SphereCast(Context.transform.position, Context.CharacterDataStore.PunchRadius, Context.transform.forward, out hit, Context.CharacterDataStore.PunchDistance, layermask))
            {
                IHittable ihit = hit.transform.GetComponentInParent<IHittable>();
                if (ihit == null) return;
                if (!ihit.CanBeBlockPushed()) return;

                foreach (var rb in hit.transform.GetComponentInParent<PlayerController>().gameObject.GetComponentsInChildren<Rigidbody>())
                {
                    rb.velocity = Vector3.zero;
                }
                Vector3 force = Context.transform.forward * Context.CharacterDataStore.BlockPushForce;
                // ihit.OnImpact(force, ForceMode.VelocityChange, Context.gameObject, ImpactType.Block);
                Services.ActionManager.RegisterAction(Time.frameCount, new HitAction(ihit.GetGameObject(), Context.gameObject, force, ForceMode.VelocityChange, ImpactType.Block));
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            // Context._animator.SetBool("Blocking", false);
            if (Context.entity.IsOwner)
                Context.state.Blocking = false;
            Context.BlockShield.SetShield(false);
            EventManager.Instance.TriggerEvent(new BlockEnd(Context.gameObject, Context.PlayerNumber));
        }
    }

    /// <summary>
    /// A Base state class for any weapon that is being used
    /// </summary>
    private class WeaponActionState : ActionState { }

    private class BazookaActionState : WeaponActionState
    {
        protected override int _stateIndex { get { return 9; } }

        public override void Update()
        {
            base.Update();
            if (_RightTriggerUp)
            {
                Context.HandObject.GetComponent<WeaponBase>().Fire(false);
            }
        }
    }
    private class WaterGunActionState : WeaponActionState
    {
        protected override int _stateIndex { get { return 10; } }
        public override bool ShouldDropHandObjectWhenForced { get { return true; } }
        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }


        public override void OnEnter()
        {
            base.OnEnter();
            // Context._animator.SetBool("PickUpHalf", true);
            if (Context.entity.IsOwner)
                Context.state.PickUpHalf = true;
            WaterGunData data = Context.HandObject.GetComponent<WeaponBase>().WeaponDataBase as WaterGunData;
            Context._permaSlow++;
            Context._permaSlowWalkSpeedMultiplier = data.FireWalkSpeedMultiplier;
            Context._rotationSpeedMultiplier = data.FireRotationMultiplier;
        }

        public override void Update()
        {
            base.Update();
            if (_RightTriggerUp)
            {
                if (Context.entity.IsOwner)
                {
                    Context.HandObject.GetComponent<WeaponBase>().Fire(false);
                    TransitionTo<HoldingState>();
                    return;
                }

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._permaSlow = 1;
            Context._walkSpeedMultiplier = 1f;
            Context._rotationSpeedMultiplier = 1f;
            // Context._animator.SetBool("PickUpHalf", false);
            if (Context.entity.IsOwner)
                Context.state.PickUpHalf = false;
        }
    }
    private class StunActionState : ActionState
    {
        protected override int _stateIndex { get { return 11; } }

        public override bool ShouldOnHitTransitToUncontrollableState { get { return true; } }

        public override void OnEnter()
        {
            base.OnEnter();
            Context._dropHandObject();
            // Context._animator.SetBool("IdleUpper", true);
            if (Context.entity.IsOwner)
                Context.state.IdleUpper = true;
            Services.BoltEventBroadcaster.OnPunchInterrepted(new PunchInterruptted(Context.gameObject, Context.PlayerNumber));
        }

        public override void Update()
        {
            base.Update();
            if (Time.time > Context._stunTimer)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleActionState>();
                }
            }
        }
    }

    private class ActionDeadState : ActionState
    {
        protected override int _stateIndex { get { return 12; } }
        private float _startTime;
        private float _respawnTime { get { return Services.Config.GameMapData.RespawnTime + Services.Config.GameMapData.InvincibleTime; } }

        public override void OnEnter()
        {
            base.OnEnter();
            _startTime = Time.time;
            Context._drainStamina(-5f);
            // Context._animator.SetBool("IdleUpper", true);
            if (Context.entity.IsOwner)
                Context.state.IdleUpper = true;
            Services.BoltEventBroadcaster.OnPunchInterrepted(new PunchInterruptted(Context.gameObject, Context.PlayerNumber));
            Context._dropHandObject();
            if (Context.MeleeVFXHolder != null) Destroy(Context.MeleeVFXHolder);
        }

        public override void Update()
        {
            base.Update();
            if (Time.time >= _startTime + _respawnTime)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleActionState>();
                    return;
                }

            }
            if (Time.time >= _startTime + Services.Config.GameMapData.RespawnTime && (_B || _RightTrigger))
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleActionState>();
                    return;
                }

            }
            if (Time.time > _startTime + Services.Config.GameMapData.RespawnTime)
            {
                _pickupcheck();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (Context._deadInvincible != null)
            {
                Context.StopCoroutine(Context._deadInvincible);

                foreach (Rigidbody rb in Context._playerBodies)
                {
                    rb.gameObject.layer = Context._playerBodiesLayer;
                }
            }
            Context._permaSlow = 0;
        }
        private void _pickupcheck()
        {
            Collider[] colliders = Physics.OverlapSphere(Context.transform.position, Context.CharacterDataStore.PickupRadius,
            Context.CharacterDataStore.PickUpLayer);
            GameObject target = null;
            float minDistance = Mathf.Infinity;
            foreach (Collider coll in colliders)
            {
                float dist = Vector3.Distance(coll.transform.position, Context.transform.position);
                if (dist < minDistance)
                {
                    target = coll.gameObject;
                    minDistance = dist;
                }
            }
            if (target == null) return;
            if (Context.HandObject == null && target.GetComponent<WeaponBase>() != null && target.GetComponent<WeaponBase>().CanBePickedUp)
            {
                if (Context.entity.IsOwner)
                {
                    TransitionTo<IdleActionState>();
                    return;
                }

            }
        }
    }
    #endregion
}
