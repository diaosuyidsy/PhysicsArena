﻿using System.Collections;
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
    public GameObject[] OnDeathHidden;

    public int PlayerNumber;

    [HideInInspector] public GameObject HandObject;
    [HideInInspector] public GameObject MeleeVFXHolder;
    [HideInInspector] public GameObject MeleeVFXHolder2;
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
    private float _sideStepTimer;
    private float _jumpTimer;
    private Vector3 _freezeBody;
    private ImpactMarker _impactMarker;
    private Animator _animator;

    private FSM<PlayerController> _movementFSM;
    private FSM<PlayerController> _actionFSM;
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
        if (other.CompareTag("DeathZone"))
        {
            ((MovementState)_movementFSM.CurrentState).OnEnterDeathZone();
            ((ActionState)_actionFSM.CurrentState).OnEnterDeathZone();
            EventManager.Instance.TriggerEvent(new PlayerDied(gameObject, PlayerNumber, _impactMarker));
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (CharacterDataStore.JumpMask == (CharacterDataStore.JumpMask | (1 << other.gameObject.layer)) &&
        _movementFSM.CurrentState.GetType().Equals(typeof(JumpState)))
        {
            _movementFSM.TransitionTo<IdleState>();
            EventManager.Instance.TriggerEvent(new PlayerLand(gameObject, OnDeathHidden[1], PlayerNumber, _getGroundTag()));
        }
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
        // Check if is side stepping
        if (_movementFSM.CurrentState.GetType().Equals(typeof(SideSteppingState)))
            return;
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
        public override void OnEnter()
        {
            base.OnEnter();
            print(GetType().Name);
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
            if (_jump && Context._isGrounded() && Context._jumpTimer < Time.timeSinceLevelLoad)
            {
                TransitionTo<JumpState>();
                return;
            }
            if (_B && Context.CharacterDataStore.IsSideStepping && Context._sideStepTimer < Time.timeSinceLevelLoad)
            {
                TransitionTo<SideSteppingState>();
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
            Context._rb.AddForce(new Vector3(0, Context.CharacterDataStore.JumpForce, 0), ForceMode.Impulse);
            EventManager.Instance.TriggerEvent(new PlayerJump(Context.gameObject, Context.OnDeathHidden[1], Context.PlayerNumber, Context._getGroundTag()));
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
                Quaternion tr = Quaternion.Slerp(Context.transform.rotation, rotation, Time.deltaTime * Context.CharacterDataStore.MinRotationSpeed);
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
            if (_jump && Context._isGrounded())
            {
                TransitionTo<JumpState>();
                return;
            }

            if (_B && Context.CharacterDataStore.IsSideStepping && Context._sideStepTimer < Time.timeSinceLevelLoad)
            {
                TransitionTo<SideSteppingState>();
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
            Quaternion tr = Quaternion.Slerp(Context.transform.rotation, rotation, Time.deltaTime * Context.CharacterDataStore.MinRotationSpeed);
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

    private class SideSteppingState : ControllableMovementState
    {
        private float _timer;
        private Vector3 _originalForward;
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = Time.timeSinceLevelLoad + Context.CharacterDataStore.SideSteppingDuration;
            _originalForward = Context.transform.forward;
            Context._rb.AddForce(_originalForward * Context.CharacterDataStore.SideSteppingInitForce, ForceMode.VelocityChange);
            Context._animator.SetBool("SideStep", true);
            Context._actionFSM.TransitionTo<SideSteppingActionState>();
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

        public override void OnExit()
        {
            base.OnExit();
            Context._sideStepTimer = Time.timeSinceLevelLoad + Context.CharacterDataStore.SideSteppingCD;
            Context._animator.SetBool("SideStep", false);
            Context._actionFSM.TransitionTo<IdleActionState>();
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
            /// Regen when past 3 seconds after block
            if (Time.time > Context._lastTimeUseBlock + Context.CharacterDataStore.BlockRegenInterval)
            {
                if (Context._blockCharge > 0f) Context._blockCharge -= (Time.deltaTime * Context.CharacterDataStore.BlockRegenRate);
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
            if (_RightTrigger && !Context.CharacterDataStore.IsButtHitting)
            {
                TransitionTo<PunchHoldingState>();
                return;
            }
            if (_RightTriggerDown && Context.CharacterDataStore.IsButtHitting)
            {
                TransitionTo<ButtAnticipationState>();
                return;
            }
            if (_B && Context._blockCharge <= Context.CharacterDataStore.MaxBlockCD && !Context.CharacterDataStore.IsSideStepping)
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

    private class SideSteppingActionState : ActionState
    {

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
            if (_RightTriggerUp || Time.time > _startHoldingTime + Context.CharacterDataStore.MeleeHoldTime)
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
            Context._animator.SetFloat("FistReleaseTime", 1f / Context.CharacterDataStore.FistReleaseTime);
            Context._animator.SetBool("PunchReleased", true);
            _time = Time.time + Context.CharacterDataStore.FistReleaseTime;
            _hitOnce = false;
            if (Context._meleeCharge < Context.CharacterDataStore.MeleeChargeThreshold) Context._meleeCharge = 0f;
            Context._rb.AddForce(Context.transform.forward * Context._meleeCharge * Context.CharacterDataStore.SelfPushForce, ForceMode.Impulse);
            EventManager.Instance.TriggerEvent(new PunchReleased(Context.gameObject, Context.PlayerNumber));
        }

        public override void Update()
        {
            base.Update();
            if (Time.time < _time)
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
                }
            }
            else
            {
                EventManager.Instance.TriggerEvent(new PunchDone(Context.gameObject, Context.PlayerNumber, Context.RightHand.transform));
                TransitionTo<IdleActionState>();
                return;
            }

            // if (_RightTriggerDown)
            // {
            //     TransitionTo<PunchHoldingState>();
            //     return;
            // }
        }

        public override void OnExit()
        {
            base.OnExit();
            Context._animator.SetBool("PunchReleased", false);
            Context._meleeCharge = 0f;
        }
    }

    private class ButtAnticipationState : ActionState
    {
        private float _timer;
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = Context.CharacterDataStore.ButtAnticipationDuration + Time.timeSinceLevelLoad;
            Context._rb.velocity = Vector3.zero;
            Context._movementFSM.TransitionTo<ButtStrikeMovementState>();
            Context._animator.SetBool("ButtAnticipation", true);
        }

        public override void Update()
        {
            base.Update();
            if (_timer < Time.timeSinceLevelLoad)
            {
                RaycastHit hit;
                if (Physics.SphereCast(Context.transform.position, Context.CharacterDataStore.ButtStrikeRaidus, Context.transform.forward, out hit, Context.CharacterDataStore.ButtStrikeStopDistance, Context.CharacterDataStore.ButtHitStopLayer))
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

    private class ButtStrikeState : ActionState
    {
        private float _timer;
        private bool _hitOnce;
        private Tweener _hitTween;
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = Context.CharacterDataStore.ButtStrikeDuration + Time.timeSinceLevelLoad;
            _hitOnce = false;
            _hitTween = Context.transform.DOMove(Context.transform.forward * Context.CharacterDataStore.ButtStrikeForwardPush, Context.CharacterDataStore.ButtStrikeDuration)
            .SetRelative(true)
            .SetEase(Context.CharacterDataStore.ButtStrikePushEase);
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
                if (!_hitOnce && Physics.SphereCast(Context.transform.position, Context.CharacterDataStore.ButtStrikeRaidus, Context.transform.forward, out hit, Context.CharacterDataStore.ButtStrikeDistance, layermask))
                {
                    if (hit.transform.GetComponentInParent<IHittable>() == null) return;
                    _hitOnce = true;
                    foreach (var rb in hit.transform.GetComponentInParent<PlayerController>().gameObject.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.velocity = Vector3.zero;
                    }
                    Vector3 force = Context.transform.forward * Context.CharacterDataStore.ButtStrikeStrength;
                    hit.transform.GetComponentInParent<IHittable>().OnImpact(force, 1f, Context.gameObject, true);
                    TransitionTo<ButtRecoveryState>();
                    return;
                }
                if (Physics.SphereCast(Context.transform.position, Context.CharacterDataStore.ButtStrikeRaidus, Context.transform.forward, out hit, Context.CharacterDataStore.ButtStrikeStopDistance, Context.CharacterDataStore.ButtHitStopLayer))
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

    private class ButtRecoveryState : ActionState
    {
        private float _timer;
        public override void OnEnter()
        {
            base.OnEnter();
            _timer = Context.CharacterDataStore.ButtRecoveryDuration + Time.timeSinceLevelLoad;
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
        private Vector2 _shieldUISize;
        public override void OnEnter()
        {
            base.OnEnter();
            EventManager.Instance.TriggerEvent(new BlockStart(Context.gameObject, Context.PlayerNumber));
            _shieldUISize = Services.VisualEffectManager.VFXDataStore.ChickenBlockUIVFX.transform.GetChild(0).GetComponent<SpriteRenderer>().size;
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
                _nextShieldUISize.x *= (Context.CharacterDataStore.MaxBlockCD - Context._blockCharge) / Context.CharacterDataStore.MaxBlockCD;
                Context.BlockUIVFXHolder.transform.GetChild(0).GetComponent<SpriteRenderer>().size = _nextShieldUISize;
            }
            if (Context._blockCharge > Context.CharacterDataStore.MaxBlockCD)
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
        }
    }
    #endregion
}
