using System.Collections;
using UnityEngine;
using Rewired;
using System;
using Mirror;

public class PlayerControllerNetworking : NetworkBehaviour
{
    [Header("Player Data Section")]
    public CharacterData CharacterDataStore;
    [Header("Player Body Setting Section")]
    public GameObject Chest;
    public GameObject Head;
    public GameObject RightHand;
    public GameObject[] OnDeathHidden;

    public int PlayerNumber;

    [HideInInspector] public GameObject HandObject;
    [HideInInspector] public GameObject MeleeVFXHolder;
    [HideInInspector] public GameObject BlockVFXHolder;
    [HideInInspector] public GameObject BlockUIVFXHolder;
    [HideInInspector] public GameObject StunVFXHolder;
    [HideInInspector] public GameObject SlowVFXHolder;
    [HideInInspector] public GameObject FoodTraverseVFXHolder;
    public Transform PlayerFeet { get { return OnDeathHidden[1].transform; } }

    #region Private Variables
    private Player _player;
    private Rigidbody _rb;
    private float _distToGround;
    private float _meleeCharge;
    private float _blockCharge;
    private float _lastTimeUseBlock;
    private Vector3 _freezeBody;
    private ImpactMarker _impactMarker;
    IEnumerator _startSlow;
    private bool _isJumping;
    private Animator _animator;

    private FSM<PlayerControllerNetworking> _movementFSM;
    private FSM<PlayerControllerNetworking> _actionFSM;
    #endregion

    #region Status Variables
    private float _stunTimer;
    private float _slowTimer;
    private float _walkSpeedMultiplier = 1f;
    private float _permaSlowWalkSpeedMultiplier = 1f;
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

    #endregion

    private void Awake()
    {
        _movementFSM = new FSM<PlayerControllerNetworking>(this);
        _actionFSM = new FSM<PlayerControllerNetworking>(this);
        _player = ReInput.players.GetPlayer(PlayerNumber);
        _rb = GetComponent<Rigidbody>();
        _distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        _freezeBody = new Vector3(0, transform.localEulerAngles.y, 0);
        _movementFSM.TransitionTo<IdleState>();
        _actionFSM.TransitionTo<IdleActionState>();
        _impactMarker = new ImpactMarker(null, Time.time, ImpactType.Self);
        _animator = GetComponent<Animator>();
    }

    public void Init(int controllernumber)
    {
        PlayerNumber = controllernumber;
        _player = ReInput.players.GetPlayer(controllernumber);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer) return;
        _movementFSM.Update();
        _actionFSM.Update();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        _movementFSM.FixedUpdate();
        _actionFSM.FixedUpdate();
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;
        _movementFSM.LateUpdate();
        _actionFSM.LateUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            ((MovementState)_movementFSM.CurrentState).OnEnterDeathZone();
            ((ActionState)_actionFSM.CurrentState).OnEnterDeathZone();
            EventManager.Instance.TriggerEvent(new PlayerDied(gameObject, PlayerNumber, _impactMarker));
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (CharacterDataStore.CharacterMovementDataStore.JumpMask == (CharacterDataStore.CharacterMovementDataStore.JumpMask | (1 << other.gameObject.layer)) && _isJumping)
        {
            _isJumping = false;
            EventManager.Instance.TriggerEvent(new PlayerLand(gameObject, OnDeathHidden[1], PlayerNumber, _getGroundTag()));
        }
    }

    // If is blockable, meaning the hit could be blocked
    public void OnMeleeHit(Vector3 force, float _meleeCharge, GameObject sender, bool _blockable)
    {
        // First check if the player could block the attack
        // if (_blockable &&
        //     CanBlock(sender.transform.forward))
        // {
        //     sender.GetComponentInParent<PlayerControllerNetworking>().OnMeleeHit(-force * CharacterDataStore.CharacterBlockDataStore.BlockMultiplier, _meleeCharge, gameObject, false);
        // }
        // else // Player is hit cause he could not block
        // {
        EventManager.Instance.TriggerEvent(new PlayerHit(sender, gameObject, force, sender.GetComponent<PlayerControllerNetworking>().PlayerNumber, PlayerNumber, _meleeCharge, !_blockable));
        // OnImpact(force, ForceMode.Impulse, sender, _blockable ? ImpactType.Melee : ImpactType.Block);
        _rb.AddForce(force, ForceMode.Impulse);

        // CmdImpact(force, gameObject);
        // }
    }

    [Command]
    void CmdMeleeHit(GameObject target, Vector3 force, float _meleeCharge, GameObject sender, bool _blockable)
    {
        Debug.Assert(target.GetComponent<PlayerControllerNetworking>() != null);
        // target.GetComponent<PlayerControllerNetworking>().OnMeleeHit(force, _meleeCharge, sender, _blockable);
        RpcMeleeHit(target, force, _meleeCharge, sender, _blockable);
    }

    [ClientRpc]
    void RpcMeleeHit(GameObject target, Vector3 force, float _meleeCharge, GameObject sender, bool _blockable)
    {
        Debug.Assert(target.GetComponent<PlayerControllerNetworking>() != null);
        target.GetComponent<PlayerControllerNetworking>().OnMeleeHit(force, _meleeCharge, sender, _blockable);
    }

    /// <summary>
    /// Can Block The attack or not
    /// Used by hook and hit
    /// </summary>
    /// <param name="forwardAngle"></param>
    /// <returns></returns>
    public bool CanBlock(Vector3 forwardAngle)
    {
        if (_actionFSM.CurrentState.GetType().Equals(typeof(BlockingState)) &&
            _angleWithin(transform.forward, forwardAngle, 180f - CharacterDataStore.CharacterBlockDataStore.BlockAngle))
            return true;
        return false;
    }

    /// <summary>
    /// This function is called when enemies want to impact the player
    /// </summary>
    /// <param name="force">The amount of force</param>
    /// <param name="forcemode">Force mode</param>
    /// <param name="enforcer">who is the impactor</param>
    public void OnImpact(Vector3 force, ForceMode forcemode, GameObject enforcer, ImpactType impactType)
    {
        _rb.AddForce(force, forcemode);
        OnImpact(enforcer, impactType);
        if (force.magnitude > CharacterDataStore.CharacterMovementDataStore.DropWeaponForceThreshold &&
            _actionFSM.CurrentState.GetType().Equals(typeof(HoldingState)))
        {
            _actionFSM.TransitionTo<IdleActionState>();
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
        if (_actionFSM.CurrentState.GetType().Equals(typeof(HoldingState))
            || _actionFSM.CurrentState.GetType().Equals(typeof(HammerActionState)))
            _actionFSM.TransitionTo<IdleActionState>();
        if (_movementFSM.CurrentState.GetType().Equals(typeof(HammerMovementOutState)))
            _movementFSM.TransitionTo<IdleState>();
    }

    /// <summary>
    /// This function is called from FootSteps on LegSwingRefernece
    /// </summary>
    public void FootStep()
    {
        if (_isGrounded())
        {
            EventManager.Instance.TriggerEvent(new FootStep(OnDeathHidden[1], _getGroundTag()));
        }
    }

    #region Helper Method
    private void _setToSpawn(float yOffset)
    {
        int colorindex = 0;
        for (int j = 0; j < ServicesNetwork.GameStateManager.PlayersInformation.RewiredID.Length; j++)
        {
            if (PlayerNumber == ServicesNetwork.GameStateManager.PlayersInformation.RewiredID[j]) colorindex = ServicesNetwork.GameStateManager.PlayersInformation.ColorIndex[j];
        }
        if (CompareTag("Team1"))
        {
            Vector3 pos = ServicesNetwork.Config.GameMapData.Team1RespawnPoints[colorindex - 3];
            pos.y += yOffset;
            transform.position = pos;
        }
        else
        {
            Vector3 pos = ServicesNetwork.Config.GameMapData.Team2RespawnPoints[colorindex];
            pos.y += yOffset;
            transform.position = pos;
        }
    }

    private string _getGroundTag()
    {
        RaycastHit hit;
        Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, _distToGround, CharacterDataStore.CharacterMovementDataStore.JumpMask);
        if (hit.collider == null) return "";
        return hit.collider.tag;
    }

    private bool _angleWithin(Vector3 A, Vector3 B, float degree)
    {
        return Vector3.Angle(A, B) > degree;
    }

    private void _dropHandObject()
    {
        if (HandObject == null) return;
        // Drop the thing
        HandObject.GetComponent<WeaponBase>().OnDrop();
        EventManager.Instance.TriggerEvent(new ObjectDropped(gameObject, PlayerNumber, HandObject));
        // Return the body to normal position
        // _resetBodyAnimation();
        // Nullify the holder
        HandObject = null;
    }

    private void _helpAim(float maxangle, float maxRange)
    {
        GameObject target = null;
        float minAngle = 360f;
        GameObject[] enemies = tag == "Team1" ? GameObject.FindGameObjectsWithTag("Team2") : GameObject.FindGameObjectsWithTag("Team1");
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
        return Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, _distToGround, CharacterDataStore.CharacterMovementDataStore.JumpMask);
    }
    #endregion

    #region Movment States
    private class MovementState : FSM<PlayerControllerNetworking>.State
    {
        protected float _HLAxis { get { return Context._player.GetAxis("Move Horizontal"); } }
        protected float _VLAxis { get { return Context._player.GetAxis("Move Vertical"); } }
        protected bool _jump { get { return Context._player.GetButtonDown("Jump"); } }
        protected bool _RightTriggerUp { get { return Context._player.GetButtonUp("Right Trigger"); } }
        protected CharacterMovementData _charMovData { get { return Context.CharacterDataStore.CharacterMovementDataStore; } }

        public void OnEnterDeathZone()
        {
            Parent.TransitionTo<DeadState>();
        }
    }

    private class ControllableMovementState : MovementState
    {
        public override void Update()
        {
            if (_jump && Context._isGrounded())
            {
                Context._isJumping = true;
                Context._rb.AddForce(new Vector3(0, _charMovData.JumpForce, 0), ForceMode.Impulse);
                EventManager.Instance.TriggerEvent(new PlayerJump(Context.gameObject, Context.OnDeathHidden[1], Context.PlayerNumber, Context._getGroundTag()));
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            Context._freezeBody.y = Context.transform.localEulerAngles.y;
            Context.transform.localEulerAngles = Context._freezeBody;
        }
    }

    private class IdleState : ControllableMovementState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("IdleDowner", true);
        }

        public override void Update()
        {
            base.Update();
            if (!Mathf.Approximately(_HLAxis, 0f) || !Mathf.Approximately(0f, _VLAxis))
            {
                TransitionTo<RunState>();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("IdleDowner", false);
        }
    }

    private class RunState : ControllableMovementState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("Running", true);
        }

        public override void Update()
        {
            base.Update();
            if (Mathf.Approximately(_HLAxis, 0f) && Mathf.Approximately(_VLAxis, 0f)) TransitionTo<IdleState>();
        }

        public override void FixedUpdate()
        {
            bool isonground = Context._isGrounded();
            Vector3 targetVelocity = Context.transform.forward * _charMovData.WalkSpeed * Context._walkSpeed;
            Vector3 velocityChange = targetVelocity - Context._rb.velocity;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -_charMovData.MaxVelocityChange, _charMovData.MaxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -_charMovData.MaxVelocityChange, _charMovData.MaxVelocityChange);
            velocityChange.y = 0f;

            if (isonground)
                Context._rb.AddForce(velocityChange, ForceMode.VelocityChange);
            else
                Context._rb.AddForce(velocityChange * _charMovData.InAirSpeedMultiplier, ForceMode.VelocityChange);

            Vector3 relPos = Quaternion.AngleAxis(Mathf.Atan2(_HLAxis, _VLAxis * -1f) * Mathf.Rad2Deg, Context.transform.up) * Vector3.forward;
            Quaternion rotation = Quaternion.LookRotation(relPos, Vector3.up);
            Quaternion tr = Quaternion.Slerp(Context.transform.rotation, rotation, Time.deltaTime * _charMovData.MinRotationSpeed);
            Context.transform.rotation = tr;
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("Running", false);
        }
    }

    private class StunMovementState : MovementState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("IdleDowner", true);
            EventManager.Instance.TriggerEvent(new PlayerStunned(Context.gameObject, Context.PlayerNumber, Context.Head.transform, Context._stunTimer - Time.time));
        }

        public override void Update()
        {
            base.Update();
            if (Time.time > Context._stunTimer)
            {
                TransitionTo<IdleState>();
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
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("IdleDowner", true);
        }
        public override void Update()
        {
            base.Update();
            Vector3 lookpos = Context.HandObject.GetComponent<rtBazooka>().BazookaShadowTransformPosition;
            lookpos.y = Context.transform.position.y;
            Context.transform.LookAt(lookpos);
            if (_RightTriggerUp)
                TransitionTo<BazookaMovementLaunchState>();
        }
        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("IdleDowner", false);
        }
    }

    private class BazookaMovementLaunchState : MovementState
    {
        private Vector3 _diff;

        public override void OnEnter()
        {
            base.OnEnter();
            _diff = Context.HandObject.transform.position - Context.transform.position;
        }

        public override void Update()
        {
            base.Update();
            bool isrunning = (!Mathf.Approximately(_HLAxis, 0f) || !Mathf.Approximately(0f, _VLAxis));
            Context._animator.SetBool("Running", isrunning);
            Context._animator.SetBool("IdleDowner", !isrunning);
            Context.transform.position = Context.HandObject.transform.position - _diff;
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("Running", false);
        }
    }

    private class HammerMovementOutState : MovementState
    {
        private Vector3 _diff;
        private Vector3 _rotDiff;

        public override void OnEnter()
        {
            base.OnEnter();
            _diff = Context.HandObject.transform.position - Context.transform.position;
            _rotDiff = Context.HandObject.transform.eulerAngles - Context.transform.eulerAngles;
        }

        public override void Update()
        {
            base.Update();
            Context.transform.position = Context.HandObject.transform.position + -Context.HandObject.transform.forward * _diff.magnitude;
            Context.transform.eulerAngles = Context.HandObject.transform.eulerAngles + _rotDiff;
        }
    }


    private class DeadState : FSM<PlayerControllerNetworking>.State
    {
        private float _startTime;
        private float _respawnTime { get { return Context.CharacterDataStore.CharacterMovementDataStore.RespawnTime; } }

        public override void OnEnter()
        {
            base.OnEnter();
            _startTime = Time.time;
            Context._rb.isKinematic = true;
            Context._setToSpawn(10f);
            Context._animator.SetBool("IdleDowner", true);
            foreach (GameObject go in Context.OnDeathHidden) { go.SetActive(false); }
        }

        public override void Update()
        {
            base.Update();
            if (Time.time >= _startTime + _respawnTime)
            {
                EventManager.Instance.TriggerEvent(new PlayerRespawned(Context.gameObject));
                TransitionTo<IdleState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._rb.isKinematic = false;
            Context._setToSpawn(0f);
            foreach (GameObject go in Context.OnDeathHidden) { go.SetActive(true); }

        }
    }
    #endregion

    #region Action States
    private class ActionState : FSM<PlayerControllerNetworking>.State
    {
        protected bool _LeftTrigger { get { return Context._player.GetButton("Left Trigger"); } }
        protected bool _LeftTriggerDown { get { return Context._player.GetButtonDown("Left Trigger"); } }
        protected bool _LeftTriggerUp { get { return Context._player.GetButtonUp("Left Trigger"); } }

        protected bool _RightTrigger { get { return Context._player.GetButton("Right Trigger"); } }
        protected bool _RightTriggerDown { get { return Context._player.GetButtonDown("Right Trigger"); } }
        protected bool _RightTriggerUp { get { return Context._player.GetButtonUp("Right Trigger"); } }

        protected bool _B { get { return Context._player.GetButton("Block"); } }
        protected bool _BDown { get { return Context._player.GetButtonDown("Block"); } }
        protected bool _BUp { get { return Context._player.GetButtonUp("Block"); } }

        protected CharacterMeleeData _charMeleeData { get { return Context.CharacterDataStore.CharacterMeleeDataStore; } }
        protected CharacterBlockData _charBlockData { get { return Context.CharacterDataStore.CharacterBlockDataStore; } }

        public override void Update()
        {
            /// Regen when past 3 seconds after block
            if (Time.time > Context._lastTimeUseBlock + _charBlockData.BlockRegenInterval)
            {
                if (Context._blockCharge > 0f) Context._blockCharge -= (Time.deltaTime * _charBlockData.BlockRegenRate);
            }
        }

        public virtual void OnEnterDeathZone()
        {
            TransitionTo<ActionDeadState>();
            return;
        }
    }

    private class IdleActionState : ActionState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("IdleUpper", true);
            Context._dropHandObject();
        }

        public override void Update()
        {
            base.Update();
            if (_LeftTrigger)
            {
                TransitionTo<PickingState>();
                return;
            }
            if (_RightTriggerDown)
            {
                TransitionTo<PunchHoldingState>();
                return;
            }
            if (_B && Context._blockCharge <= _charBlockData.MaxBlockCD)
            {
                TransitionTo<BlockingState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("IdleUpper", false);
        }
    }

    private class PickingState : ActionState
    {
        private CharacterPickUpData _characterPickUpData { get { return Context.CharacterDataStore.CharacterPickUpDataStore; } }
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("Picking", true);
        }

        public override void Update()
        {
            base.Update();
            if (_LeftTriggerUp)
            {
                TransitionTo<IdleActionState>();
                return;
            }
            _pickupcheck();
        }

        private void _pickupcheck()
        {
            RaycastHit hit;
            if (Physics.SphereCast(Context.Chest.transform.position,
                _characterPickUpData.Radius,
                Vector3.down,
                out hit,
                Context._distToGround,
                _characterPickUpData.PickUpLayer))
            {
                if (Context.HandObject == null && hit.collider.GetComponent<WeaponBase>().CanBePickedUp)
                {
                    EventManager.Instance.TriggerEvent(new ObjectPickedUp(Context.gameObject, Context.PlayerNumber, hit.collider.gameObject));
                    // Tell other necessary components that it has taken something
                    Context.HandObject = hit.collider.gameObject;

                    // Tell the collected weapon who picked it up
                    hit.collider.GetComponent<WeaponBase>().OnPickUp(Context.gameObject);
                    TransitionTo<HoldingState>();
                    return;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("Picking", false);
        }
    }

    private class HoldingState : ActionState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Assert(Context.HandObject != null);
            switch (Context.HandObject.tag)
            {
                case "Hook":
                case "FistGun":
                case "Hammer":
                case "Weapon":
                    Context._animator.SetBool("PickUpHalf", true);
                    break;
                default:
                    Context._animator.SetBool("PickUpFull", true);
                    break;
            }
        }

        public override void Update()
        {
            base.Update();
            if (_LeftTriggerDown)
            {
                TransitionTo<DroppingState>();
                return;
            }
            if (_RightTriggerDown)
            {
                WeaponBase wb = Context.HandObject.GetComponent<WeaponBase>();
                Context._helpAim(wb.HelpAimAngle, wb.HelpAimDistance);
                Context.HandObject.GetComponent<WeaponBase>().Fire(true);
                switch (Context.HandObject.tag)
                {
                    case "Bazooka":
                        Context._movementFSM.TransitionTo<BazookaMovmentAimState>();
                        TransitionTo<BazookaActionState>();
                        break;
                    case "Hammer":
                        Context._movementFSM.TransitionTo<HammerMovementOutState>();
                        TransitionTo<HammerActionState>();
                        break;
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
            Context._animator.SetBool("PickUpFull", false);
            Context._animator.SetBool("PickUpHalf", false);
        }
    }

    private class DroppingState : ActionState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("Dropping", true);
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("Dropping", false);
        }

        public override void Update()
        {
            base.Update();
            if (_LeftTriggerUp)
            {
                TransitionTo<IdleActionState>();
                return;
            }
        }
    }

    private class PunchHoldingState : ActionState
    {
        private float _startHoldingTime;
        private bool _holding;

        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetFloat("ClockFistTime", 1f / _charMeleeData.ClockFistTime);
            Context._animator.SetBool("PunchHolding", true);
            EventManager.Instance.TriggerEvent(new PunchStart(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
            _holding = false;
            _startHoldingTime = Time.time;
        }

        public override void Update()
        {
            base.Update();
            if (!_holding && Time.time > _startHoldingTime + _charMeleeData.ClockFistTime)
            {
                _holding = true;
                Context._meleeCharge = 1f;
                EventManager.Instance.TriggerEvent(new PunchHolding(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
            }
            else if (Time.time <= _startHoldingTime + _charMeleeData.ClockFistTime)
            {
                Context._meleeCharge = (Time.time - _startHoldingTime) / _charMeleeData.ClockFistTime;
            }
            if (_RightTriggerUp || Time.time > _startHoldingTime + _charMeleeData.MeleeHoldTime)
            {
                TransitionTo<PunchReleasingState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("PunchHolding", false);
        }
    }

    private class PunchReleasingState : ActionState
    {
        private float _time;
        private bool _hitOnce;

        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetFloat("FistReleaseTime", 1f / _charMeleeData.FistReleaseTime);
            Context._animator.SetBool("PunchReleased", true);
            _time = Time.time + _charMeleeData.FistReleaseTime + 0.1f;
            _hitOnce = false;
            if (Context._meleeCharge < _charMeleeData.MeleeChargeThreshold) Context._meleeCharge = 0f;
            Context._rb.AddForce(Context.transform.forward * Context._meleeCharge * _charMeleeData.SelfPushForce, ForceMode.Impulse);
            EventManager.Instance.TriggerEvent(new PunchReleased(Context.gameObject, Context.PlayerNumber));
        }

        public override void Update()
        {
            base.Update();
            if (Time.time < _time)
            {
                RaycastHit hit;
                // This Layermask get all player's layer except this player's
                int layermask = ServicesNetwork.Config.ConfigData.AllPlayerLayer ^ (1 << Context.gameObject.layer);
                if (!_hitOnce && Physics.SphereCast(Context.transform.position, _charMeleeData.PunchRadius, Context.transform.forward, out hit, _charMeleeData.PunchDistance, layermask))
                {
                    if (hit.transform.GetComponentInParent<PlayerControllerNetworking>() == null) return;
                    _hitOnce = true;
                    foreach (var rb in hit.transform.GetComponentInParent<PlayerControllerNetworking>().gameObject.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.velocity = Vector3.zero;
                    }
                    Vector3 force = Context.transform.forward * _charMeleeData.PunchForce * Context._meleeCharge;
                    // hit.transform.GetComponentInParent<PlayerControllerNetworking>().OnMeleeHit(force, Context._meleeCharge, Context.gameObject, true);
                    Context.CmdMeleeHit(hit.transform.GetComponentInParent<PlayerControllerNetworking>().gameObject, force, Context._meleeCharge, Context.gameObject, true);
                    // Context.CmdImpact(force, hit.transform.GetComponentInParent<PlayerControllerNetworking>().gameObject);
                }
            }
            else
            {
                EventManager.Instance.TriggerEvent(new PunchDone(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
                TransitionTo<IdleActionState>();
                return;
            }

            if (_RightTriggerDown)
            {
                TransitionTo<PunchHoldingState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("PunchReleased", false);
            Context._meleeCharge = 0f;
        }
    }

    private class BlockingState : ActionState
    {
        private Vector2 _shieldUISize;
        public override void OnEnter()
        {
            base.OnEnter();
            EventManager.Instance.TriggerEvent(new BlockStart(Context.gameObject, Context.PlayerNumber));
            _shieldUISize = ServicesNetwork.VisualEffectManager.VFXDataStore.ChickenBlockUIVFX.transform.GetChild(0).GetComponent<SpriteRenderer>().size;
            Context._animator.SetBool("Blocking", true);
        }

        public override void Update()
        {
            base.Update();
            if (_BUp)
            {
                TransitionTo<IdleActionState>();
                return;
            }
            Context._lastTimeUseBlock = Time.time;
            Context._blockCharge += Time.deltaTime;
            if (Context.BlockUIVFXHolder != null)
            {
                Vector2 _nextShieldUISize = _shieldUISize;
                _nextShieldUISize.x *= (_charBlockData.MaxBlockCD - Context._blockCharge) / _charBlockData.MaxBlockCD;
                Context.BlockUIVFXHolder.transform.GetChild(0).GetComponent<SpriteRenderer>().size = _nextShieldUISize;
            }
            if (Context._blockCharge > _charBlockData.MaxBlockCD)
            {
                TransitionTo<IdleActionState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("Blocking", false);
            EventManager.Instance.TriggerEvent(new BlockEnd(Context.gameObject, Context.PlayerNumber));
        }
    }

    /// <summary>
    /// A Base state class for any weapon that is being used
    /// </summary>
    private class WeaponActionState : ActionState { }

    private class BazookaActionState : WeaponActionState
    {
        public override void Update()
        {
            base.Update();
            if (_RightTriggerUp)
            {
                Context.HandObject.GetComponent<WeaponBase>().Fire(false);
            }
        }
    }

    private class HammerActionState : WeaponActionState
    {

    }

    private class StunActionState : ActionState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._dropHandObject();
            Context._animator.SetBool("IdleUpper", true);
        }

        public override void Update()
        {
            base.Update();
            if (Time.time > Context._stunTimer)
            {
                TransitionTo<IdleActionState>();
            }
        }
    }

    private class ActionDeadState : ActionState
    {
        private float _startTime;
        private float _respawnTime { get { return Context.CharacterDataStore.CharacterMovementDataStore.RespawnTime; } }

        public override void OnEnter()
        {
            base.OnEnter();
            _startTime = Time.time;
            Context._animator.SetBool("IdleUpper", true);
            Context._dropHandObject();
            if (Context.MeleeVFXHolder != null) Destroy(Context.MeleeVFXHolder);
        }

        public override void Update()
        {
            base.Update();
            if (Time.time >= _startTime + _respawnTime)
            {
                TransitionTo<IdleActionState>();
                return;
            }
        }
    }
    #endregion
}
