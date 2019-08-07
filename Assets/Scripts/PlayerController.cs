using System.Collections;
using UnityEngine;
using Rewired;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Player Data Section")]
    public CharacterData CharacterDataStore;
    [Header("Player Body Setting Section")]
    public GameObject LegSwingReference;
    public GameObject Chest;
    public GameObject Head;
    [Tooltip("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] LeftArms;
    [Tooltip("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] RightArms;
    public GameObject LeftHand;
    public GameObject RightHand;
    public GameObject TurnReference;
    public GameObject[] OnDeathHidden;

    public int PlayerNumber;

    [HideInInspector] public GameObject HandObject;
    [HideInInspector] public GameObject MeleeVFXHolder;
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
    private float _blockCharge;
    private float _lastTimeUseBlock;
    private Vector3 _freezeBody;
    private HingeJoint _chesthj;
    private HingeJoint _leftArm2hj;
    private HingeJoint _rightArm2hj;
    private HingeJoint _leftArmhj;
    private HingeJoint _rightArmhj;
    private HingeJoint _leftHandhj;
    private HingeJoint _rightHandhj;
    private ImpactMarker _impactMarker;
    IEnumerator _startSlow;
    private bool _isJumping;

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

    #endregion

    private void Awake()
    {
        _movementFSM = new FSM<PlayerController>(this);
        _actionFSM = new FSM<PlayerController>(this);
        _player = ReInput.players.GetPlayer(PlayerNumber);
        _rb = GetComponent<Rigidbody>();
        _distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        _freezeBody = new Vector3(0, transform.localEulerAngles.y, 0);
        _chesthj = Chest.GetComponent<HingeJoint>();
        _leftArm2hj = LeftArms[0].GetComponent<HingeJoint>();
        _rightArm2hj = RightArms[0].GetComponent<HingeJoint>();
        _leftArmhj = LeftArms[1].GetComponent<HingeJoint>();
        _rightArmhj = RightArms[1].GetComponent<HingeJoint>();
        _leftHandhj = LeftArms[2].GetComponent<HingeJoint>();
        _rightHandhj = RightArms[2].GetComponent<HingeJoint>();
        _movementFSM.TransitionTo<IdleState>();
        _actionFSM.TransitionTo<IdleActionState>();
        _impactMarker = new ImpactMarker(null, Time.time, ImpactType.Self);
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
        if (_blockable &&
            CanBlock(sender.transform.forward))
        {
            sender.GetComponentInParent<PlayerController>().OnMeleeHit(-force * CharacterDataStore.CharacterBlockDataStore.BlockMultiplier, _meleeCharge, gameObject, false);
        }
        else // Player is hit cause he could not block
        {
            EventManager.Instance.TriggerEvent(new PlayerHit(sender, gameObject, force, sender.GetComponent<PlayerController>().PlayerNumber, PlayerNumber, _meleeCharge, !_blockable));
            OnImpact(force, ForceMode.Impulse, sender, _blockable ? ImpactType.Melee : ImpactType.Block);
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
        for (int j = 0; j < Services.GameStateManager.PlayersInformation.RewiredID.Length; j++)
        {
            if (PlayerNumber == Services.GameStateManager.PlayersInformation.RewiredID[j]) colorindex = Services.GameStateManager.PlayersInformation.ColorIndex[j];
        }
        if (CompareTag("Team1"))
        {
            Vector3 pos = Services.Config.GameMapData.Team1RespawnPoints[colorindex - 3];
            pos.y += yOffset;
            transform.position = pos;
        }
        else
        {
            Vector3 pos = Services.Config.GameMapData.Team2RespawnPoints[colorindex];
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
        HandObject.SendMessage("Drop");
        EventManager.Instance.TriggerEvent(new ObjectDropped(gameObject, PlayerNumber, HandObject));
        // Return the body to normal position
        _resetBodyAnimation();
        // These two are necessary for all objects
        HandObject.GetComponent<Rigidbody>().isKinematic = false;
        HandObject.layer = LayerMask.NameToLayer("Pickup");
        // Specialized checking
        if (HandObject.CompareTag("Weapon"))
        {
            // Stop the shooting
            HandObject.GetComponent<WeaponBase>().Fire(false);
            // Disable the UI
            HandObject.SendMessage("KillUI");
        }
        if (HandObject.CompareTag("Team1Resource") || HandObject.CompareTag("Team2Resource"))
        {
            HandObject.SendMessage("RegisterLastHolder", PlayerNumber);
        }
        // Nullify the holder
        HandObject = null;
    }

    private void _helpAim(float maxangle)
    {
        GameObject target = null;
        float minAngle = 360f;
        GameObject[] enemies = tag == "Team1" ? GameObject.FindGameObjectsWithTag("Team2") : GameObject.FindGameObjectsWithTag("Team1");
        foreach (GameObject otherPlayer in enemies)
        {
            if (otherPlayer.activeSelf)
            {
                // If other player are within max Distance, then check for the smalliest angle player
                if (Vector3.Distance(otherPlayer.transform.position, gameObject.transform.position) <= CharacterDataStore.HelpAimMaxRange)
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

    #region Animations
    private void _pickAnimation()
    {
        // Arm2: right max: 90 --> -74
        //       left min: -75 --> 69
        JointLimits la2l = _leftArm2hj.limits;
        JointLimits ra2l = _rightArm2hj.limits;
        la2l.min = 69f;
        ra2l.max = -74f;
        _leftArm2hj.limits = la2l;
        _rightArm2hj.limits = ra2l;

        JointLimits lal = _leftArmhj.limits;
        JointLimits ral = _rightArmhj.limits;
        lal.max = 121f;
        ral.max = 121f;
        lal.min = 0f;
        ral.min = 0f;
        _leftArmhj.limits = lal;
        _rightArmhj.limits = ral;

        JointSpring ljs = _leftArmhj.spring;
        JointSpring rjs = _rightArmhj.spring;
        ljs.targetPosition = 180f;
        rjs.targetPosition = 180f;
        _leftArmhj.spring = ljs;
        _rightArmhj.spring = rjs;

        JointLimits lhl = _leftHandhj.limits;
        JointLimits rhl = _rightHandhj.limits;
        lhl.max = 0f;
        rhl.max = 0f;
        _leftHandhj.limits = lhl;
        _rightHandhj.limits = rhl;

        JointSpring tempjs = _chesthj.spring;
        tempjs.targetPosition = 90f;
        tempjs.targetPosition = Mathf.Clamp(tempjs.targetPosition, _chesthj.limits.min + 5, _chesthj.limits.max - 5);
        _chesthj.spring = tempjs;
    }

    private void _resetBodyAnimation()
    {
        _resetArmHandAnimation(_leftArm2hj, _leftArmhj, _leftHandhj, true);
        _resetArmHandAnimation(_rightArm2hj, _rightArmhj, _rightHandhj, false);
        _resetSpineAnimation();
    }

    private void _resetSpineAnimation()
    {
        JointSpring js = _chesthj.spring;
        js.targetPosition = 0f;
        _chesthj.spring = js;
    }

    private void _resetArmHandAnimation(HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, bool IsLeftHand)
    {
        JointLimits lm2 = Arm2hj.limits;
        JointLimits lm = Armhj.limits;
        JointLimits hl = Handhj.limits;

        JointSpring js = Armhj.spring;
        JointSpring hjs = Handhj.spring;

        if (IsLeftHand)
        {
            lm2.max = 70f;
            lm2.min = -75f;
        }
        else
        {
            lm2.max = 90f;
            lm2.min = -75f;
        }
        hl.min = 0f;
        hl.max = 90f;
        lm.min = -90f;
        lm.max = 90f;
        js.targetPosition = 0f;
        hjs.targetPosition = 0f;
        Handhj.spring = hjs;
        Handhj.limits = hl;
        Armhj.spring = js;
        Armhj.limits = lm;
        Arm2hj.limits = lm2;
    }

    private void _hammerOutAnimation()
    {
        JointSpring js = _chesthj.spring;
        js.targetPosition = 30f;
        _chesthj.spring = js;
    }

    IEnumerator _pickUpAnimation(HingeJoint Arm2hj, HingeJoint Armhj, bool IsLeftHand, float time)
    {
        float elapesdTime = 0f;
        JointLimits lm2 = Arm2hj.limits;
        JointLimits lm = Armhj.limits;

        float initLm2LeftMax = lm2.max;
        float initLm2LeftMin = lm2.min;

        float initLmMax = lm.max;
        while (elapesdTime < time)
        {
            elapesdTime += Time.deltaTime;
            if (IsLeftHand)
            {
                lm2.max = Mathf.Lerp(initLm2LeftMax, 103f, elapesdTime / time);
                lm2.min = Mathf.Lerp(initLm2LeftMin, 95f, elapesdTime / time);
            }
            else
            {
                lm2.max = Mathf.Lerp(initLm2LeftMax, -88f, elapesdTime / time);
                lm2.min = Mathf.Lerp(initLm2LeftMin, -98f, elapesdTime / time);
            }
            lm.max = Mathf.Lerp(initLmMax, 180f, elapesdTime / time);
            Arm2hj.limits = lm2;
            Armhj.limits = lm;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator _pickUpHalfAnimation(HingeJoint Armhj, float time)
    {
        float elapesdTime = 0f;
        JointSpring js = Armhj.spring;
        float initArmTargetPosition = js.targetPosition;

        while (elapesdTime < time)
        {
            elapesdTime += Time.deltaTime;
            js.targetPosition = Mathf.Lerp(initArmTargetPosition, 90f, elapesdTime / time);
            Armhj.spring = js;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator _powerDropAnimation(float time)
    {
        float elapsedTime = 0f;
        JointSpring js = _chesthj.spring;

        float initspringtagetPosition = js.targetPosition;

        while (elapsedTime <= time)
        {
            //DropCharge = elapsedTime / time;
            elapsedTime += Time.deltaTime;
            js.targetPosition = Mathf.Lerp(initspringtagetPosition, -100f, elapsedTime / time);
            _chesthj.spring = js;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator _meleeHoldingLeftAnimation(HingeJoint LeftArmhj, float time)
    {
        float elapsedTime = 0f;
        JointSpring ljs = LeftArmhj.spring;
        float initLATargetPosition = ljs.targetPosition;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            ljs.targetPosition = Mathf.Lerp(initLATargetPosition, 80f, _meleeCharge);
            LeftArmhj.spring = ljs;
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator _meleeHoldingRightAnimation(HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, float time)
    {
        float elapesdTime = 0f;
        JointLimits lm2 = Arm2hj.limits;
        JointLimits hl = Handhj.limits;
        JointSpring js = Armhj.spring;

        float initLm2Max = lm2.max;
        float initLm2Min = lm2.min;
        float initLmTargetPosition = js.targetPosition;
        float inithlMax = hl.max;
        float inithlMin = hl.min;

        EventManager.Instance.TriggerEvent(new PunchStart(gameObject, PlayerNumber, RightHand.transform));

        while (elapesdTime < time)
        {
            elapesdTime += Time.deltaTime;
            _meleeCharge = elapesdTime / time;
            lm2.max = Mathf.Lerp(initLm2Max, 4f, _meleeCharge);
            lm2.min = Mathf.Lerp(initLm2Min, -17f, _meleeCharge);

            js.targetPosition = Mathf.Lerp(initLmTargetPosition, -85f, _meleeCharge);

            hl.max = Mathf.Lerp(inithlMax, 130f, _meleeCharge);
            hl.min = Mathf.Lerp(inithlMin, 110f, _meleeCharge);

            Arm2hj.limits = lm2;
            Armhj.spring = js;
            Handhj.limits = hl;
            yield return new WaitForEndOfFrame();
        }
        EventManager.Instance.TriggerEvent(new PunchHolding(gameObject, PlayerNumber, RightHand.transform));
    }

    IEnumerator _meleePunchLeftHandAnimation(HingeJoint LeftHandhj, float time)
    {
        float elapsedTime = 0f;
        JointSpring ljs = LeftHandhj.spring;
        float initLATargetPosition = ljs.targetPosition;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            ljs.targetPosition = Mathf.Lerp(initLATargetPosition, -120f, elapsedTime / time);
            LeftHandhj.spring = ljs;

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator _meleePunchRightHandAnimation(HingeJoint Armhj, HingeJoint Handhj, float time)
    {
        float elapesdTime = 0f;
        JointLimits hl = Handhj.limits;
        JointSpring js = Armhj.spring;
        float initLmTargetPosition = js.targetPosition;
        float inithlMax = hl.max;
        float inithlMin = hl.min;

        while (elapesdTime < time)
        {
            elapesdTime += Time.deltaTime;
            js.targetPosition = Mathf.Lerp(initLmTargetPosition, 180f, elapesdTime / time);
            hl.max = Mathf.Lerp(inithlMax, 12f, elapesdTime / time);
            hl.min = Mathf.Lerp(inithlMin, -2.8f, elapesdTime / time);
            Armhj.spring = js;
            Handhj.limits = hl;

            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator _blockAnimation(HingeJoint Armhj, HingeJoint Handhj, float time)
    {
        JointSpring ajs = Armhj.spring;
        float initaTargetPosition = ajs.targetPosition;
        JointSpring hjs = Handhj.spring;
        float inithTargetPosition = hjs.targetPosition;
        float elapsedTime = 0f;
        float armtargetpos = CharacterDataStore.CharacterBlockDataStore.ArmTargetPosition;
        float handtartgetpos = CharacterDataStore.CharacterBlockDataStore.HandTargetPosition;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;

            ajs.targetPosition = Mathf.Lerp(initaTargetPosition, armtargetpos, elapsedTime / time);
            hjs.targetPosition = Mathf.Lerp(inithTargetPosition, handtartgetpos, elapsedTime / time);
            Armhj.spring = ajs;
            Handhj.spring = hjs;

            yield return new WaitForEndOfFrame();
        }
    }

    #endregion

    #region Movment States
    private class MovementState : FSM<PlayerController>.State
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
            Context.LegSwingReference.GetComponent<Animator>().SetFloat("WalkSpeedMultiplier", Context._walkSpeed);
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
            Context.LegSwingReference.GetComponent<Animator>().enabled = false;
            Context.LegSwingReference.transform.eulerAngles = Vector3.zero;
        }

        public override void Update()
        {
            base.Update();
            if (!Mathf.Approximately(_HLAxis, 0f) || !Mathf.Approximately(0f, _VLAxis))
            {
                TransitionTo<RunState>();
            }
        }
    }

    private class RunState : ControllableMovementState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context.LegSwingReference.GetComponent<Animator>().enabled = true;
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

            Transform target = Context.TurnReference.transform.GetChild(0);
            Vector3 relativePos = target.position - Context.transform.position;

            Context.TurnReference.transform.eulerAngles = new Vector3(Context.transform.eulerAngles.x, Mathf.Atan2(_HLAxis, _VLAxis * -1f) * Mathf.Rad2Deg, Context.transform.eulerAngles.z);
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            Quaternion tr = Quaternion.Slerp(Context.transform.rotation, rotation, Time.deltaTime * _charMovData.MinRotationSpeed);
            Context.transform.rotation = tr;
        }
    }

    private class StunMovementState : MovementState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context.LegSwingReference.GetComponent<Animator>().enabled = false;
            Context.LegSwingReference.transform.eulerAngles = Vector3.zero;
            EventManager.Instance.TriggerEvent(new PlayerStunned(Context.gameObject, Context.PlayerNumber, Context.Head.transform));
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
            Context.LegSwingReference.GetComponent<Animator>().enabled = false;
            Context.LegSwingReference.transform.eulerAngles = Vector3.zero;
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
            Context.LegSwingReference.GetComponent<Animator>().enabled = (!Mathf.Approximately(_HLAxis, 0f) || !Mathf.Approximately(0f, _VLAxis));
            Context.transform.position = Context.HandObject.transform.position - _diff;
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
            Context.LegSwingReference.GetComponent<Animator>().enabled = false;
            Context.LegSwingReference.transform.rotation = Quaternion.identity;
            Context._hammerOutAnimation();
        }

        public override void Update()
        {
            base.Update();
            Context.transform.position = Context.HandObject.transform.position + -Context.HandObject.transform.forward * _diff.magnitude;
            Context.transform.eulerAngles = Context.HandObject.transform.eulerAngles + _rotDiff;
        }

        public override void OnExit()
        {
            base.OnExit();
            Context.LegSwingReference.GetComponent<Animator>().enabled = true;
            Context._resetSpineAnimation();
        }
    }


    private class DeadState : FSM<PlayerController>.State
    {
        private float _startTime;
        private float _respawnTime { get { return Context.CharacterDataStore.CharacterMovementDataStore.RespawnTime; } }

        public override void OnEnter()
        {
            base.OnEnter();
            _startTime = Time.time;
            Context._rb.isKinematic = true;
            Context._setToSpawn(10f);
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
            Context._resetBodyAnimation();
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

    }

    private class PickingState : ActionState
    {
        private CharacterPickUpData _characterPickUpData { get { return Context.CharacterDataStore.CharacterPickUpDataStore; } }
        public override void OnEnter()
        {
            Context._pickAnimation();
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
                if (Context.HandObject == null && hit.collider.GetComponent<GunPositionControl>().CanBePickedUp)
                {
                    EventManager.Instance.TriggerEvent(new ObjectPickedUp(Context.gameObject, Context.PlayerNumber, hit.collider.gameObject));
                    // Tell other necessary components that it has taken something
                    Context.HandObject = hit.collider.gameObject;

                    // Tell the collected weapon who picked it up
                    hit.collider.GetComponent<GunPositionControl>().Owner = Context.gameObject;
                    hit.collider.GetComponent<Rigidbody>().isKinematic = true;
                    hit.collider.gameObject.layer = Context.gameObject.layer;
                    TransitionTo<HoldingState>();
                    return;
                }
            }
        }
    }

    private class HoldingState : ActionState
    {
        private IEnumerator _lefthandcoroutine;
        private IEnumerator _righthandcoroutine;

        public override void OnEnter()
        {
            Context._resetSpineAnimation();
            Debug.Assert(Context.HandObject != null);
            switch (Context.HandObject.tag)
            {
                case "Hook":
                case "FistGun":
                case "Hammer":
                    _lefthandcoroutine = Context._pickUpHalfAnimation(Context._leftArmhj, 0.1f);
                    _righthandcoroutine = Context._pickUpHalfAnimation(Context._rightArmhj, 0.1f);
                    break;
                default:
                    _lefthandcoroutine = Context._pickUpAnimation(Context._leftArm2hj, Context._leftArmhj, true, 0.1f);
                    _righthandcoroutine = Context._pickUpAnimation(Context._rightArm2hj, Context._rightArmhj, false, 0.1f);
                    break;
            }
            Context.StartCoroutine(_lefthandcoroutine);
            Context.StartCoroutine(_righthandcoroutine);
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
                switch (Context.HandObject.tag)
                {
                    case "Weapon":
                    case "Hook":
                    case "FistGun":
                        Context._helpAim(Context.CharacterDataStore.HelpAimMaxRange);
                        break;
                }
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

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //if (Context._rb.velocity.magnitude >= Context.CharacterDataStore.CharacterMovementDataStore.DropWeaponVelocityThreshold)
            //	TransitionTo<IdleActionState>();
        }

        public override void OnExit()
        {
            Context.StopCoroutine(_lefthandcoroutine);
            Context.StopCoroutine(_righthandcoroutine);
        }
    }

    private class DroppingState : ActionState
    {
        private IEnumerator _dropcoroutine;

        public override void OnEnter()
        {
            _dropcoroutine = Context._powerDropAnimation(2f);
            Context.StartCoroutine(_dropcoroutine);
        }

        public override void OnExit()
        {
            Context.StopCoroutine(_dropcoroutine);
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
        private IEnumerator _punchleftcoroutine;
        private IEnumerator _punchrightcoroutine;

        private float _startHoldingTime;

        public override void OnEnter()
        {
            _punchrightcoroutine = Context._meleeHoldingRightAnimation(Context._rightArm2hj, Context._rightArmhj, Context._rightHandhj, _charMeleeData.ClockFistTime);
            _punchleftcoroutine = Context._meleeHoldingLeftAnimation(Context._leftArmhj, _charMeleeData.ClockFistTime);
            Context.StartCoroutine(_punchrightcoroutine);
            Context.StartCoroutine(_punchleftcoroutine);
            _startHoldingTime = Time.time;
        }

        public override void Update()
        {
            base.Update();
            if (_RightTriggerUp || Time.time > _startHoldingTime + _charMeleeData.MeleeHoldTime)
            {
                TransitionTo<PunchReleasingState>();
                return;
            }
        }

        public override void OnExit()
        {
            Context.StopCoroutine(_punchrightcoroutine);
            Context.StopCoroutine(_punchleftcoroutine);
        }
    }

    private class PunchReleasingState : ActionState
    {
        private IEnumerator _punchleftcoroutine;
        private IEnumerator _punchrightcoroutine;
        private float _time;
        private bool _hitOnce;

        public override void OnEnter()
        {
            _punchrightcoroutine = Context._meleePunchRightHandAnimation(Context._rightArmhj, Context._rightHandhj, _charMeleeData.FistReleaseTime);
            _punchleftcoroutine = Context._meleePunchLeftHandAnimation(Context._leftArmhj, _charMeleeData.FistReleaseTime);
            Context.StartCoroutine(_punchleftcoroutine);
            Context.StartCoroutine(_punchrightcoroutine);
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
                int layermask = Services.Config.ConfigData.AllPlayerLayer ^ (1 << Context.gameObject.layer);
                if (!_hitOnce && Physics.SphereCast(Context.transform.position, _charMeleeData.PunchRadius, Context.transform.forward, out hit, _charMeleeData.PunchDistance, layermask))
                {
                    if (hit.transform.GetComponentInParent<PlayerController>() == null) return;
                    _hitOnce = true;
                    foreach (var rb in hit.transform.GetComponentInParent<PlayerController>().gameObject.GetComponentsInChildren<Rigidbody>())
                    {
                        rb.velocity = Vector3.zero;
                    }
                    Vector3 force = Context.transform.forward * _charMeleeData.PunchForce * Context._meleeCharge;
                    hit.transform.GetComponentInParent<PlayerController>().OnMeleeHit(force, Context._meleeCharge, Context.gameObject, true);
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
            Context.StopCoroutine(_punchleftcoroutine);
            Context.StopCoroutine(_punchrightcoroutine);
            Context._resetBodyAnimation();
            Context._meleeCharge = 0f;
        }
    }

    private class BlockingState : ActionState
    {
        IEnumerator _leftblockcoroutine;
        IEnumerator _rightblockcoroutine;

        public override void OnEnter()
        {
            EventManager.Instance.TriggerEvent(new BlockStart(Context.gameObject, Context.PlayerNumber));
            _leftblockcoroutine = Context._blockAnimation(Context._leftArmhj, Context._leftHandhj, 0.1f);
            _rightblockcoroutine = Context._blockAnimation(Context._rightArmhj, Context._rightHandhj, 0.1f);
            Context.StartCoroutine(_leftblockcoroutine);
            Context.StartCoroutine(_rightblockcoroutine);
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
            if (Context._blockCharge > _charBlockData.MaxBlockCD)
            {
                TransitionTo<IdleActionState>();
                return;
            }
        }

        public override void OnExit()
        {
            Context.StopCoroutine(_leftblockcoroutine);
            Context.StopCoroutine(_rightblockcoroutine);
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
            Context._resetBodyAnimation();
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
            Context._resetBodyAnimation();
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

        public override void OnExit()
        {
            base.OnExit();
        }
    }
    #endregion
}
