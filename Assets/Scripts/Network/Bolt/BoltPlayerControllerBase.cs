using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using DG.Tweening;
using Bolt;

public abstract class BoltPlayerControllerBase : Bolt.EntityEventListener<IBirfiaPlayerBaseState>,
IHittable,
IVFXHolder,
IBodyConfiguration,
IEffectable
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
    [HideInInspector] public GameObject MeleeVFXHolder { get; set; }
    [HideInInspector] public GameObject MeleeVFXHolder2 { get; set; }
    [HideInInspector] public GameObject BlockVFXHolder { get; set; }
    [HideInInspector] public GameObject StunVFXHolder { get; set; }
    [HideInInspector] public GameObject SlowVFXHolder { get; set; }
    [HideInInspector] public GameObject FoodTraverseVFXHolder { get; set; }
    public Transform PlayerFeet { get { return OnDeathHidden[1].transform; } }
    public Transform PlayerUITransform;

    #region Inherient Variables
    protected Player _player;
    protected Rigidbody _rb;
    protected float _distToGround;
    protected float _currentStamina;
    protected float _lastTimeUseStamina;
    protected Vector2 _staminaUISize;
    protected Vector3 _freezeBody;
    protected ImpactMarker _impactMarker;
    protected Animator _animator;
    protected IEnumerator _deadInvincible;
    protected int _playerBodiesLayer;

    #endregion

    #region Status Variables
    protected float _hitUncontrollableTimer;

    protected ForceTuple _hitForceTuple;

    private float _ws = 1f;
    public float WalkSpeedMultiplier
    {
        get => _ws; set
        {
            _ws = value;
            if (entity.IsOwner)
                state.RunningSpeed = value;
        }
    }
    private float _rs = 1f;
    public float RotationSpeedMultiplier { get => _rs; set => _rs = value; }

    #endregion


    GameObject IBodyConfiguration.Chest => this.Chest;

    GameObject IBodyConfiguration.Head => this.Head;

    GameObject[] IBodyConfiguration.LeftArms => this.LeftArms;

    GameObject[] IBodyConfiguration.RightArms => this.RightArms;

    GameObject IBodyConfiguration.LeftHand => this.LeftHand;

    GameObject IBodyConfiguration.RightHand => this.RightHand;

    GameObject IBodyConfiguration.LeftFoot => this.LeftFoot;

    GameObject IBodyConfiguration.RightFoot => this.RightFoot;

    Transform IBodyConfiguration.PlayerUITransform => this.PlayerUITransform;

    ShieldController IBodyConfiguration.BlockShield => this.BlockShield;

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

    [HideInInspector] public BoltPlayerView _playerview;
    protected EffectController _effectController;
    #region setup
    public override void Attached()
    {
        state.SetTransforms(state.MainTransform, transform);
        state.SetAnimator(GetComponentInChildren<Animator>());
        if (entity.IsOwner)
        {
            entity.TakeControl();
            _rb.isKinematic = false;
            state.RunningSpeed = 1f;
        }
        else
        {
            state.AddCallback("MovementStateIndex", _onMovementStateIndexChange);
            state.AddCallback("ActionStateIndex", _onActionStateIndexChange);
        }
        _playerview.transform.SetParent(null);
    }

    protected virtual void Awake()
    {
        _playerview = GetComponentInChildren<BoltPlayerView>();
        _effectController = GetComponent<EffectController>();
        SetupStateMachine();
        _rb = GetComponent<Rigidbody>();
        _player = ReInput.players.GetPlayer(PlayerNumber);
        _distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        _freezeBody = new Vector3(0, transform.localEulerAngles.y, 0);
        _impactMarker = new ImpactMarker(gameObject, Time.time, ImpactType.Self);
        _animator = GetComponentInChildren<Animator>();
        _currentStamina = CharacterDataStore.MaxStamina;
        _staminaUISize = BlockUIVFXHolder.transform.GetChild(0).GetComponent<SpriteRenderer>().size;
        _playerBodiesLayer = gameObject.layer;
        // WalkSpeedMultiplier = 1f;
    }


    protected abstract void SetupStateMachine();
    #endregion

    /// <summary>
    /// Movement State Index
    /// 0: IdleState
    /// 1: DeadState
    /// 2: RunState
    /// 3: HitUncontrollableState
    /// </summary>
    protected abstract void _onMovementStateIndexChange();

    /// <summary>
    /// Action State Index:
    /// 0: IdleActionState
    /// 1: HitUncontrollableActionState
    /// 2: DroppedRecoveryState
    /// </summary>
    protected abstract void _onActionStateIndexChange();

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
        BirfiaPlayerCommand cmd = (BirfiaPlayerCommand)command;
        if (resetState)
        {
            transform.position = cmd.Result.Position;
            _rb.velocity = cmd.Result.Velocity;
        }
        else
        {
            cmd.Result.Position = transform.position;
            cmd.Result.Velocity = _rb.velocity;
        }
    }

    public override void OnEvent(PunchEvent ev)
    {
        OnImpact(ev.PunchForce, ForceMode.Impulse, gameObject, ImpactType.Melee);
    }

    protected virtual void Update()
    {
        if (entity.IsControllerOrOwner)
            _pollKeys();
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual void LateUpdate()
    {
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone") || other.CompareTag("DeathModeTrapZone"))
        {
            // ((MovementState)_movementFSM.CurrentState).OnEnterDeathZone();
            // ((ActionState)_actionFSM.CurrentState).OnEnterDeathZone();
            if (entity.IsOwner)
                Services.BoltEventBroadcaster.OnPlayerDied(new PlayerDied(gameObject, PlayerNumber, _impactMarker, other.gameObject));
        }
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        // if (_movementFSM.CurrentState != null)
        //     ((MovementState)_movementFSM.CurrentState).OnCollisionEnter(other);
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public void SetVelocity(Vector3 vel)
    {
        _rb.velocity = vel;
    }

    public bool CanBeBlockPushed()
    {
        return false;
    }

    public virtual bool CanBlock(Vector3 forwardAngle)
    {
        return true;
    }

    public virtual void OnImpact(Vector3 force, ForceMode forcemode, GameObject enforcer, ImpactType impactType)
    {
        OnImpact(enforcer, impactType);

        if (force.magnitude > CharacterDataStore.HitSmallThreshold)
        {
            _hitForceTuple = new ForceTuple(force, forcemode);
            _hitUncontrollableTimer = CharacterDataStore.HitUncontrollableTimeSmall;
            if (force.magnitude > CharacterDataStore.HitBigThreshold)
            {
                _hitUncontrollableTimer = CharacterDataStore.HitUncontrollableTimeBig;
            }
        }
        else
        {
            _rb.AddForce(force, forcemode);
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

    public void OnImpact(GameObject enforcer, ImpactType impactType)
    {
        _impactMarker.SetValue(enforcer, Time.time, impactType);
    }

    public void OnImpact(Status status)
    {
        _effectController.OnApplyEffect(status);
    }

    #region Helper Methods
    protected bool _canDrainStamina(float drain)
    {
        if (_currentStamina > drain)
            return true;
        else
        {
            return false;
        }
    }

    protected void _drainStamina(float drain)
    {
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
    }

    protected bool _frontIsCliff()
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
    protected void _setToSpawn(float yOffset)
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

    protected string _getGroundTag()
    {
        RaycastHit hit;
        Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, _distToGround, CharacterDataStore.JumpMask);
        if (hit.collider == null) return "";
        return hit.collider.tag;
    }

    protected bool _angleWithin(Vector3 A, Vector3 B, float degree)
    {
        return Vector3.Angle(A, B) > degree;
    }

    protected IEnumerator _deadInvincibleIenumerator(float time)
    {
        _rb.gameObject.layer = LayerMask.NameToLayer("ReviveInvincible");

        yield return new WaitForSeconds(time);

        _rb.gameObject.layer = _playerBodiesLayer;

    }

    protected void _helpAim(float maxangle, float maxRange)
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

    protected bool _isGrounded()
    {
        RaycastHit hit;
        return Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, _distToGround, CharacterDataStore.JumpMask);
    }

    public void OnStun(StunEffect effect)
    {
        throw new System.NotImplementedException();
    }

    public void OnRemove(Status effect)
    {
        _effectController.OnRemoveEffect(effect);
    }

    public virtual bool CanDefend(Vector3 forwardAngle)
    {
        return false;
    }
    #endregion
}
