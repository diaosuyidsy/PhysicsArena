using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System;
using DG.Tweening;

public class PlayerController : MonoBehaviour, IHittable
{
    [Header("Player Data Section")]
    public CharacterData CharacterDataStore;
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

    public int PlayerNumber;

    [HideInInspector] public GameObject HandObject;
    [HideInInspector] public GameObject EquipmentObject;
    [HideInInspector] public GameObject MeleeVFXHolder;
    [HideInInspector] public GameObject MeleeVFXHolder2;
    [HideInInspector] public GameObject BlockVFXHolder;
    [HideInInspector] public GameObject StunVFXHolder;
    [HideInInspector] public GameObject SlowVFXHolder;
    [HideInInspector] public GameObject FoodTraverseVFXHolder;
    public Transform PlayerFeet { get { return OnDeathHidden[1].transform; } }

    #region Private Variables
    private Player _player;
    private Rigidbody _rb;
    private float _distToGround;
    private float _meleeCharge;
    private float _currentStamina;
    private float _lastTimeUseStamina;
    private float _lastTimeUSeStaminaUnimportant;
    private Vector2 _staminaUISize;
    private float _sideStepTimer;
    private float _jumpTimer;
    private Vector3 _freezeBody;
    private ImpactMarker _impactMarker;
    private Animator _animator;
    // private IEnumerator StaminaUIDecrease;

    private FSM<PlayerController> _movementFSM;
    private FSM<PlayerController> _actionFSM;
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
            _animator.SetFloat("RunningSpeed", _walkSpeed);
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
    private IEnumerator _deadInvincible;
    private int _playerBodiesLayer;
    #endregion

    private void Awake()
    {
        _movementFSM = new FSM<PlayerController>(this);
        _actionFSM = new FSM<PlayerController>(this);
        _player = ReInput.players.GetPlayer(PlayerNumber);
        _rb = GetComponent<Rigidbody>();
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

    // Update is called once per frame
    private void Update()
    {
        _movementFSM.Update();
        _actionFSM.Update();
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
        ((MovementState)_movementFSM.CurrentState).OnCollisionEnter(other);
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
            _angleWithin(transform.forward, forwardAngle, 180f - CharacterDataStore.BlockAngle))
            return true;
        return false;
    }

    public void OnImpact(Vector3 force, float _meleeCharge, GameObject sender, bool _blockable)
    {
        // First check if the player could block the attack
        if (_blockable &&
            CanBlock(sender.transform.forward))
        {
            sender.GetComponentInParent<IHittable>().OnImpact(-force * CharacterDataStore.BlockMultiplier, _meleeCharge, gameObject, false);
        }
        else // Player is hit cause he could not block
        {
            EventManager.Instance.TriggerEvent(new PlayerHit(sender, gameObject, force, sender.GetComponent<PlayerController>().PlayerNumber, PlayerNumber, _meleeCharge, !_blockable));
            OnImpact(force, ForceMode.Impulse, sender, _blockable ? ImpactType.Melee : ImpactType.Block);
        }
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
        if (force.magnitude > CharacterDataStore.DropWeaponForceThreshold &&
            _actionFSM.CurrentState.GetType().Equals(typeof(HoldingState)))
        {
            _actionFSM.TransitionTo<IdleActionState>();
        }
        if (_actionFSM.CurrentState.GetType().Equals(typeof(ButtStrikeState)))
        {
            _actionFSM.TransitionTo<IdleActionState>();
        }

        if (force.magnitude > CharacterDataStore.HitSmallThreshold)
        {
            _hitUncontrollableTimer = CharacterDataStore.HitUncontrollableTimeSmall;
            if (force.magnitude > CharacterDataStore.HitBigThreshold)
            {
                _hitUncontrollableTimer = CharacterDataStore.HitUncontrollableTimeBig;
            }
            _movementFSM.TransitionTo<HitUncontrollableState>();
            if (_actionFSM.CurrentState.GetType().Equals(typeof(BlockingState)) ||
                _actionFSM.CurrentState.GetType().Equals(typeof(PunchHoldingState)) ||
                _actionFSM.CurrentState.GetType().Equals(typeof(IdleActionState)) ||
                _actionFSM.CurrentState.GetType().Equals(typeof(PickingState)))
            {
                _actionFSM.TransitionTo<HitUnControllableActionState>();
            }
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
        if (_actionFSM.CurrentState.GetType().Equals(typeof(HoldingState)))
            _actionFSM.TransitionTo<IdleActionState>();
    }

    public void ForceDropEquipment(EquipmentPositionType posType)
    {
        if (posType == EquipmentPositionType.OnBack && _movementFSM.CurrentState.GetType().Equals(typeof(JetPackState)))
        {
            _movementFSM.TransitionTo<IdleState>();
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

    #region Helper Method
    private void _setVelocity(Vector3 vel)
    {
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>(true))
        {
            rb.velocity = vel;
        }
    }

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
        int colorindex = 0;
        for (int j = 0; j < Services.GameStateManager.PlayersInformation.RewiredID.Length; j++)
        {
            if (PlayerNumber == Services.GameStateManager.PlayersInformation.RewiredID[j]) colorindex = Services.GameStateManager.PlayersInformation.ColorIndex[j];
        }
        if (CompareTag("Team1"))
        {
            Vector3 pos = Services.Config.Team1RespawnPoints[colorindex - 3];
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

    private void _dropHandObject()
    {
        if (HandObject == null) return;
        // Drop the thing
        HandObject.GetComponent<WeaponBase>().OnDrop();
        HandObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Vector3 forceToAdd = transform.right * CharacterDataStore.DropForce.x +
            transform.forward * CharacterDataStore.DropForce.z +
            transform.up * CharacterDataStore.DropForce.y;
        HandObject.GetComponent<Rigidbody>().AddForce(forceToAdd, ForceMode.VelocityChange);

        EventManager.Instance.TriggerEvent(new ObjectDropped(gameObject, PlayerNumber, HandObject));
        // Return the body to normal position
        // _resetBodyAnimation();
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
        return Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, _distToGround, CharacterDataStore.JumpMask);
    }

    private bool _canDrainStamina(float drain)
    {
        if (_currentStamina > drain)
            return true;
        else
        {
            _lastTimeUSeStaminaUnimportant = Time.timeSinceLevelLoad;
            BlockUIVFXHolder.SetActive(true);
            BlockUIVFXHolder.GetComponent<DOTweenAnimation>().DORestart();
            return false;
        }
    }

    private void _drainStamina(float drain)
    {
        if (drain <= 0f) return;
        if (_currentStamina - drain < 0f)
            _currentStamina = 0f;
        else
            _currentStamina -= drain;
        _lastTimeUseStamina = Time.timeSinceLevelLoad;
        _lastTimeUSeStaminaUnimportant = Time.timeSinceLevelLoad;

        BlockUIVFXHolder.SetActive(true);
        Vector2 _nextStaminaUISize = _staminaUISize;
        _nextStaminaUISize.x *= _currentStamina / CharacterDataStore.MaxStamina;
        BlockUIVFXHolder.transform.GetChild(0).GetComponent<SpriteRenderer>().size = _nextStaminaUISize;
    }
    #endregion

    #region Movment States
    private class MovementState : FSM<PlayerController>.State
    {
        protected float _HLAxis { get { return Context._player.GetAxis("Move Horizontal"); } }
        protected float _VLAxis { get { return Context._player.GetAxis("Move Vertical"); } }
        protected float _HLAxisRaw { get { return Context._player.GetAxisRaw("Move Horizontal"); } }
        protected float _VLAxisRaw { get { return Context._player.GetAxisRaw("Move Vertical"); } }
        protected bool _jump { get { return Context._player.GetButtonDown("Jump"); } }
        protected bool _RightTriggerUp { get { return Context._player.GetButtonUp("Right Trigger"); } }
        protected bool _B { get { return Context._player.GetButton("Block"); } }

        public void OnEnterDeathZone()
        {
            Parent.TransitionTo<DeadState>();
        }

        public virtual void OnCollisionEnter(Collision other)
        {

        }

        public override void OnEnter()
        {
            base.OnEnter();
            // print(GetType().Name);
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
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("IdleDowner", true);
        }

        public override void Update()
        {
            base.Update();
            if (_HLAxisRaw != 0f || _VLAxisRaw != 0f)
            {
                TransitionTo<RunState>();
                return;
            }

            if (_jump && Context._isGrounded() &&
            Context._jumpTimer < Time.timeSinceLevelLoad &&
            Context.EquipmentObject != null &&
            Context.EquipmentObject.GetComponent<rtJet>() != null &&
            Context._canDrainStamina(Context.EquipmentObject.GetComponent<rtJet>().m_JetData.JumpStaminaDrain))
            {
                Context.EquipmentObject.GetComponent<EquipmentBase>().OnUse();
                TransitionTo<JetPackState>();
                return;
            }
            else if (_jump && Context._isGrounded() && Context._jumpTimer < Time.timeSinceLevelLoad)
            {
                TransitionTo<JumpState>();
                return;
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
            Context._animator.SetBool("IdleDowner", false);
        }
    }

    private class JumpState : ControllableMovementState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context.OnDeathHidden[2].SetActive(false);
            Context._rb.AddForce(new Vector3(0, Context.CharacterDataStore.JumpForce, 0), ForceMode.Impulse);
            EventManager.Instance.TriggerEvent(new PlayerJump(Context.gameObject, Context.OnDeathHidden[1], Context.PlayerNumber, Context._getGroundTag()));
        }

        public override void OnCollisionEnter(Collision other)
        {
            if (Context.CharacterDataStore.JumpMask == (Context.CharacterDataStore.JumpMask | (1 << other.gameObject.layer)))
            {
                TransitionTo<IdleState>();
                EventManager.Instance.TriggerEvent(new PlayerLand(Context.gameObject, Context.OnDeathHidden[1], Context.PlayerNumber, Context._getGroundTag()));
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Vector3 targetVelocity = Context.transform.forward * Context.CharacterDataStore.WalkSpeed * Context._walkSpeed;
            Vector3 velocityChange = targetVelocity - Context._rb.velocity;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -Context.CharacterDataStore.MaxVelocityChange, Context.CharacterDataStore.MaxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -Context.CharacterDataStore.MaxVelocityChange, Context.CharacterDataStore.MaxVelocityChange);
            velocityChange.y = 0f;

            Context._rb.AddForce(velocityChange * Context.CharacterDataStore.InAirSpeedMultiplier, ForceMode.VelocityChange);

            if (_HLAxis != 0f && _VLAxis != 0f)
            {
                Vector3 relPos = Quaternion.AngleAxis(Mathf.Atan2(_HLAxis, _VLAxis * -1f) * Mathf.Rad2Deg, Context.transform.up) * Vector3.forward;
                Quaternion rotation = Quaternion.LookRotation(relPos, Vector3.up);
                Quaternion tr = Quaternion.Slerp(Context.transform.rotation, rotation, Time.deltaTime * Context.CharacterDataStore.MinRotationSpeed * Context._rotationSpeedMultiplier);
                Context.transform.rotation = tr;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context.OnDeathHidden[2].SetActive(true);
            Context._jumpTimer = Time.timeSinceLevelLoad + Context.CharacterDataStore.JumpCD;
        }
    }

    private class RunState : ControllableMovementState
    {
        private float _runTowardsCliffTime;
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("Running", true);
            _runTowardsCliffTime = 0f;
        }

        public override void Update()
        {
            base.Update();
            if (_HLAxisRaw == 0f && _VLAxisRaw == 0f)
            {
                TransitionTo<IdleState>();
                return;
            }
            if (_jump && Context._isGrounded() &&
            Context._jumpTimer < Time.timeSinceLevelLoad &&
            Context.EquipmentObject != null &&
            Context.EquipmentObject.GetComponent<rtJet>() != null &&
            Context._canDrainStamina(Context.EquipmentObject.GetComponent<rtJet>().m_JetData.JumpStaminaDrain))
            {
                Context.EquipmentObject.GetComponent<EquipmentBase>().OnUse();
                TransitionTo<JetPackState>();
                return;
            }
            else if (_jump && Context._isGrounded() && Context._jumpTimer < Time.timeSinceLevelLoad)
            {
                TransitionTo<JumpState>();
                return;
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
            Context._animator.SetBool("Running", false);
        }
    }
    private class HitUncontrollableState : ControllableMovementState
    {
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
                TransitionTo<IdleState>();
                return;
            }
        }
    }

    private class PunchReleasingMovementState : ControllableMovementState
    {

    }

    private class JetPackState : ControllableMovementState
    {
        private JetData _jetData;
        private float _InAirJumpTimer;
        public override void OnEnter()
        {
            base.OnEnter();
            _jetData = Context.EquipmentObject.GetComponent<EquipmentBase>().EquipmentDataBase as JetData;
            Context._rb.AddForce(_jetData.JumpForce * Vector3.up, ForceMode.Impulse);
            Context._drainStamina(_jetData.JumpStaminaDrain);
            _InAirJumpTimer = Time.timeSinceLevelLoad + _jetData.InAirJumpCD;
        }

        public override void Update()
        {
            base.Update();
            if (_jump && _InAirJumpTimer < Time.timeSinceLevelLoad && Context._canDrainStamina(_jetData.InAirStaminaDrain))
            {
                _InAirJumpTimer = Time.timeSinceLevelLoad + _jetData.InAirJumpCD;
                Context._drainStamina(_jetData.InAirStaminaDrain);
                Context._rb.AddForce(_jetData.InAirForce.y * Vector3.up + _jetData.InAirForce.z * Context.transform.forward, ForceMode.VelocityChange);
            }
        }

        public override void OnCollisionEnter(Collision other)
        {
            if (Context.CharacterDataStore.JumpMask == (Context.CharacterDataStore.JumpMask | (1 << other.gameObject.layer)))
            {
                TransitionTo<IdleState>();
                EventManager.Instance.TriggerEvent(new PlayerLand(Context.gameObject, Context.OnDeathHidden[1], Context.PlayerNumber, Context._getGroundTag()));
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Vector3 targetVelocity = Context.transform.forward * Context.CharacterDataStore.WalkSpeed * Context._walkSpeed;
            Vector3 velocityChange = targetVelocity - Context._rb.velocity;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -Context.CharacterDataStore.MaxVelocityChange, Context.CharacterDataStore.MaxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -Context.CharacterDataStore.MaxVelocityChange, Context.CharacterDataStore.MaxVelocityChange);
            velocityChange.y = 0f;
            Context._rb.AddForce(_jetData.FloatAuxilaryForce * Vector3.up, ForceMode.Acceleration);
            Context._rb.AddForce(velocityChange, ForceMode.VelocityChange);

            if (_HLAxis != 0f && _VLAxis != 0f)
            {
                Vector3 relPos = Quaternion.AngleAxis(Mathf.Atan2(_HLAxis, _VLAxis * -1f) * Mathf.Rad2Deg, Context.transform.up) * Vector3.forward;
                Quaternion rotation = Quaternion.LookRotation(relPos, Vector3.up);
                Quaternion tr = Quaternion.Slerp(Context.transform.rotation, rotation, Time.deltaTime * Context.CharacterDataStore.MinRotationSpeed);
                Context.transform.rotation = tr;
            }
        }
    }

    private class ButtStrikeMovementState : MovementState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("IdleDowner", true);
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("IdleDowner", false);
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

    private class HookGunStaticMovementState : MovementState
    {
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

    private class DeadState : FSM<PlayerController>.State
    {
        private float _startTime;
        private float _respawnTime { get { return Services.Config.GameMapData.RespawnTime; } }

        public override void OnEnter()
        {
            base.OnEnter();
            _startTime = Time.time;
            Context._rb.isKinematic = true;
            Context._setToSpawn(10f);
            Context._animator.SetBool("IdleDowner", true);
            foreach (GameObject go in Context.OnDeathHidden) { go.SetActive(false); }
            if (Context._deadInvincible != null)
                Context.StopCoroutine(Context._deadInvincible);
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
            Context._deadInvincible = Context._deadInvincibleIenumerator(Services.Config.GameMapData.InvincibleTime);
            Context.StartCoroutine(Context._deadInvincible);
        }
    }
    #endregion

    #region Action States
    private class ActionState : FSM<PlayerController>.State
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

        public override void Update()
        {
            base.Update();
            /// Regen when past 3 seconds after block
            if (Time.timeSinceLevelLoad > Context._lastTimeUseStamina + Context.CharacterDataStore.StaminaRegenInterval)
            {
                if (Context._currentStamina < Context.CharacterDataStore.MaxStamina) Context._currentStamina += (Time.deltaTime * Context.CharacterDataStore.StaminaRegenRate);
            }
            /// Stamina Regen UI Linger Time
            if (Context.BlockUIVFXHolder != null && Context.BlockUIVFXHolder.activeSelf && Time.timeSinceLevelLoad > Context._lastTimeUSeStaminaUnimportant + Context.CharacterDataStore.BlockUILingerDuration)
            {
                Context.BlockUIVFXHolder.SetActive(false);
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
            Context._dropHandObject();
            Context._animator.SetBool("IdleUpper", true);
            Context._permaSlow = 0;
            Context._permaSlowWalkSpeedMultiplier = 1f;
        }

        public override void Update()
        {
            base.Update();
            if (_LeftTrigger)
            {
                TransitionTo<PickingState>();
                return;
            }
            if (_RightTriggerDown && Context.EquipmentObject != null &&
            Context.EquipmentObject.GetComponent<rtJet>() != null &&
            Context._canDrainStamina(Context.EquipmentObject.GetComponent<rtJet>().m_JetData.ButtStaminaDrain))
            {
                Context.EquipmentObject.GetComponent<EquipmentBase>().OnUse();
                TransitionTo<ButtAnticipationState>();
                return;
            }
            else if (_RightTrigger && (Context.EquipmentObject == null || Context.EquipmentObject.GetComponent<rtJet>() == null))
            {
                TransitionTo<PunchHoldingState>();
                return;
            }
            if (_B && Context._canDrainStamina(0.1f))
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
                Context.CharacterDataStore.Radius,
                Vector3.down,
                out hit,
                Context._distToGround,
                Context.CharacterDataStore.PickUpLayer))
            {

                if (Context.HandObject == null && hit.collider.GetComponent<WeaponBase>() != null && hit.collider.GetComponent<WeaponBase>().CanBePickedUp)
                {
                    EventManager.Instance.TriggerEvent(new ObjectPickedUp(Context.gameObject, Context.PlayerNumber, hit.collider.gameObject));
                    // Tell other necessary components that it has taken something
                    Context.HandObject = hit.collider.gameObject;

                    // Tell the collected weapon who picked it up
                    hit.collider.GetComponent<WeaponBase>().OnPickUp(Context.gameObject);
                    TransitionTo<HoldingState>();
                    return;
                }
                else if (Context.EquipmentObject == null &&
                        hit.collider.GetComponent<EquipmentBase>() != null &&
                        hit.collider.GetComponent<EquipmentBase>().CanBePickedUp)
                {
                    EventManager.Instance.TriggerEvent(new ObjectPickedUp(Context.gameObject, Context.PlayerNumber, hit.collider.gameObject));
                    Context.EquipmentObject = hit.collider.gameObject;
                    hit.collider.GetComponent<EquipmentBase>().OnPickUp(Context.gameObject);
                    TransitionTo<IdleActionState>();
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
                case "Weapon_OnChest":
                    Context._animator.SetBool("PickUpHalf", true);
                    break;
                case "Team1Resource":
                case "Team2Resource":
                case "Weapon_OnHead":
                    Context._animator.SetBool("PickUpFull", true);
                    break;
                default:
                    Context._animator.SetBool("IdleUpper", true);
                    break;
            }
            Context._permaSlow++;
            Context._permaSlowWalkSpeedMultiplier = Context.HandObject.GetComponent<WeaponBase>().WeaponDataBase.PickupSlowMultiplier;
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
                if (Context.HandObject.GetComponent<WeaponBase>().GetType().Equals(typeof(rtBazooka)))
                {
                    Context._movementFSM.TransitionTo<BazookaMovmentAimState>();
                    TransitionTo<BazookaActionState>();
                }
                else if (Context.HandObject.GetComponent<WeaponBase>().GetType().Equals(typeof(rtBoomerang)))
                {
                    TransitionTo<BoomerangActionState>();
                }
                else if (Context.HandObject.GetComponent<WeaponBase>().GetType().Equals(typeof(rtSmallBaz)))
                {
                    TransitionTo<IdleActionState>();
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
            Context._dropHandObject();
            Context._animator.SetBool("Dropping", false);
        }

        public override void Update()
        {
            base.Update();
            if (_LeftTriggerUp)
            {
                TransitionTo<DroppedRecoveryState>();
                return;
            }
        }
    }

    private class DroppedRecoveryState : ActionState
    {
        private float _timer;
        public override void OnEnter()
        {
            base.OnEnter();
            Context._animator.SetBool("IdleUpper", true);
            _timer = Time.timeSinceLevelLoad + Context.CharacterDataStore.DropRecoveryTime;
        }

        public override void Update()
        {
            base.Update();
            if (Time.timeSinceLevelLoad > _timer)
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
            Context._animator.SetFloat("ClockFistTime", 1f / Context.CharacterDataStore.ClockFistTime);
            Context._animator.SetBool("PunchHolding", true);
            EventManager.Instance.TriggerEvent(new PunchStart(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
            _holding = false;
            _startHoldingTime = Time.time;
        }

        public override void Update()
        {
            base.Update();
            if (!_holding && Time.time > _startHoldingTime + Context.CharacterDataStore.ClockFistTime)
            {
                _holding = true;
                Context._meleeCharge = 1f;
                EventManager.Instance.TriggerEvent(new PunchHolding(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
            }
            else if (Time.time <= _startHoldingTime + Context.CharacterDataStore.ClockFistTime)
            {
                Context._meleeCharge = (Time.time - _startHoldingTime) / Context.CharacterDataStore.ClockFistTime;
            }
            if (_RightTriggerUp && _holding)
            {
                TransitionTo<PunchReleasingState>();
                return;
            }
            if (_holding && Time.time > _startHoldingTime + Context.CharacterDataStore.ClockFistTime && !_RightTrigger)
            {
                TransitionTo<PunchReleasingState>();
                return;
            }
            if (_holding && _BDown)
            {
                EventManager.Instance.TriggerEvent(new PunchDone(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
                TransitionTo<BlockingState>();
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
            Context._animator.SetFloat("FistReleaseTime", 1f / Context.CharacterDataStore.FistReleaseTime);
            Context._animator.SetBool("PunchReleased", true);
            _time = Time.time;
            _hitOnce = false;
            if (Context._meleeCharge < Context.CharacterDataStore.MeleeChargeThreshold) Context._meleeCharge = 0f;
            if (Context._movementFSM.CurrentState.GetType().Equals(typeof(IdleState)))
                Context._rb.AddForce(Context.transform.forward * Context._meleeCharge * Context.CharacterDataStore.IdleSelfPushForce, ForceMode.VelocityChange);
            else
                Context._rb.AddForce(Context.transform.forward * Context._meleeCharge * Context.CharacterDataStore.SelfPushForce, ForceMode.VelocityChange);
            EventManager.Instance.TriggerEvent(new PunchReleased(Context.gameObject, Context.PlayerNumber));
            Context._rotationSpeedMultiplier = Context.CharacterDataStore.PunchReleaseRotationMultiplier;
            // Context._movementFSM.TransitionTo<PunchReleasingMovementState>();
        }

        public override void Update()
        {
            base.Update();
            if (Time.time < _time + Context.CharacterDataStore.PunchActivateTime)
            {
                RaycastHit hit;
                // This Layermask get all player's layer except this player's
                int layermask = 0;
                if (Context.gameObject.layer == LayerMask.NameToLayer("ReviveInvincible")) layermask = Context.CharacterDataStore.CanHitLayer;
                else layermask = Context.CharacterDataStore.CanHitLayer ^ (1 << Context.gameObject.layer);
                if (!_hitOnce && Physics.SphereCast(Context.transform.position, Context.CharacterDataStore.PunchRadius, Context.transform.forward, out hit, Context.CharacterDataStore.PunchDistance, layermask))
                {
                    if (hit.transform.GetComponentInParent<IHittable>() == null) return;
                    _hitOnce = true;
                    foreach (var rb in hit.transform.GetComponentInParent<PlayerController>().gameObject.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.velocity = Vector3.zero;
                    }
                    Vector3 force = Context.transform.forward * Context.CharacterDataStore.PunchForce * Context._meleeCharge;
                    hit.transform.GetComponentInParent<IHittable>().OnImpact(force, Context._meleeCharge, Context.gameObject, true);
                    Context._setVelocity(Vector3.zero);
                }
            }
            if (Time.time > _time + Context.CharacterDataStore.FistReleaseTime)
            {
                EventManager.Instance.TriggerEvent(new PunchDone(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
                TransitionTo<IdleActionState>();
                return;
            }
            if (_LeftTrigger)
            {
                EventManager.Instance.TriggerEvent(new PunchDone(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
                TransitionTo<PickingState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("PunchReleased", false);
            Context._meleeCharge = 0f;
            Context._rotationSpeedMultiplier = 1f;
            // Context._movementFSM.TransitionTo<IdleState>();
        }
    }

    private class HitUnControllableActionState : ActionState
    {
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
                TransitionTo<IdleActionState>();
                return;
            }
        }
    }

    private class ButtState : ActionState
    {
        protected JetData _jetData;
        public override void OnEnter()
        {
            base.OnEnter();
            _jetData = Context.EquipmentObject.GetComponent<rtJet>().m_JetData;
        }
    }
    private class ButtAnticipationState : ButtState
    {
        private float _timer;
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = _jetData.ButtAnticipationDuration + Time.timeSinceLevelLoad;
            Context._rb.velocity = Vector3.zero;
            Context._movementFSM.TransitionTo<ButtStrikeMovementState>();
            Context._drainStamina(_jetData.ButtStaminaDrain);
            Context._animator.SetBool("ButtAnticipation", true);
        }

        public override void Update()
        {
            base.Update();
            if (_timer < Time.timeSinceLevelLoad)
            {
                RaycastHit hit;
                if (Physics.SphereCast(Context.transform.position, _jetData.ButtStrikeRaidus, Context.transform.forward, out hit, _jetData.ButtStrikeStopDistance, _jetData.ButtHitStopLayer))
                {
                    TransitionTo<ButtRecoveryState>();
                    return;
                }
                TransitionTo<ButtStrikeState>();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("ButtAnticipation", false);
        }
    }

    private class ButtStrikeState : ButtState
    {
        private float _timer;
        private bool _hitOnce;
        private Tweener _hitTween;
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = _jetData.ButtStrikeDuration + Time.timeSinceLevelLoad;
            _hitOnce = false;
            _hitTween = Context.transform.DOMove(Context.transform.forward * _jetData.ButtStrikeForwardPush, _jetData.ButtStrikeDuration)
            .SetRelative(true)
            .SetEase(Ease.OutCirc);
            Context._animator.SetBool("ButtStrike", true);

        }

        public override void Update()
        {
            base.Update();

            if (_timer < Time.timeSinceLevelLoad)
            {
                TransitionTo<ButtRecoveryState>();
                return;
            }
            else
            {
                RaycastHit hit;
                // This Layermask get all player's layer except this player's
                int layermask = 0;
                if (Context.gameObject.layer == LayerMask.NameToLayer("ReviveInvincible")) layermask = Context.CharacterDataStore.CanHitLayer;
                else layermask = Context.CharacterDataStore.CanHitLayer ^ (1 << Context.gameObject.layer);
                if (!_hitOnce && Physics.SphereCast(Context.transform.position, _jetData.ButtStrikeRaidus, Context.transform.forward, out hit, _jetData.ButtStrikeDistance, layermask))
                {
                    if (hit.transform.GetComponentInParent<IHittable>() == null) return;
                    _hitOnce = true;
                    foreach (var rb in hit.transform.GetComponentInParent<PlayerController>().gameObject.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.velocity = Vector3.zero;
                    }
                    Vector3 force = Context.transform.forward * _jetData.ButtStrikeStrength;
                    hit.transform.GetComponentInParent<IHittable>().OnImpact(force, 1f, Context.gameObject, true);
                    TransitionTo<ButtRecoveryState>();
                    return;
                }
                if (Physics.SphereCast(Context.transform.position, _jetData.ButtStrikeRaidus, Context.transform.forward, out hit, _jetData.ButtStrikeStopDistance, _jetData.ButtHitStopLayer))
                {
                    print("On Hit Obstacles");
                    TransitionTo<ButtRecoveryState>();
                    return;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            _hitTween.Kill();
            Context._animator.SetBool("ButtStrike", false);
        }
    }

    private class ButtRecoveryState : ButtState
    {
        private float _timer;
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = _jetData.ButtRecoveryDuration + Time.timeSinceLevelLoad;
        }

        public override void Update()
        {
            base.Update();
            if (_timer < Time.timeSinceLevelLoad)
            {
                TransitionTo<IdleActionState>();
                return;
            }
        }


        public override void OnExit()
        {
            base.OnExit();
            Context._movementFSM.TransitionTo<IdleState>();
        }
    }

    private class BlockingState : ActionState
    {
        private float _timer;
        public override void OnEnter()
        {
            base.OnEnter();
            EventManager.Instance.TriggerEvent(new BlockStart(Context.gameObject, Context.PlayerNumber));
            Context._animator.SetBool("Blocking", true);
            Context._permaSlow++;
            Context._permaSlowWalkSpeedMultiplier = Context.CharacterDataStore.BlockSpeedMultiplier;
            _timer = Time.timeSinceLevelLoad + Context.CharacterDataStore.MinBlockUpTime;
        }

        public override void Update()
        {
            base.Update();
            if (!_B && _timer < Time.timeSinceLevelLoad)
            {
                TransitionTo<IdleActionState>();
                return;
            }
            Context._drainStamina(Time.deltaTime * Context.CharacterDataStore.BlockStaminaDrain);

            if (Context._currentStamina <= 0f)
            {
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
            Context._animator.SetBool("Blocking", false);
            Context._permaSlow--;
            Context._permaSlowWalkSpeedMultiplier = 1f;
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

    private class BoomerangActionState : WeaponActionState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context.OnImpact(new PermaSlowEffect(0f, 0.5f));
        }
        public override void Update()
        {
            base.Update();
            if (_RightTriggerUp)
            {
                Context.HandObject.GetComponent<WeaponBase>().Fire(false);
                TransitionTo<IdleActionState>();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context.OnImpact(new RemovePermaSlowEffect(0f, 0.5f));
        }
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
        private float _respawnTime { get { return Services.Config.GameMapData.RespawnTime + Services.Config.GameMapData.InvincibleTime; } }

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
            if (_B || _RightTrigger || _LeftTrigger)
            {
                TransitionTo<IdleActionState>();
                return;
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
    }
    #endregion
}
