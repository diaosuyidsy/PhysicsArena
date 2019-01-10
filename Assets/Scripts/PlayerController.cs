using System.Collections;
using UnityEngine;
using Rewired;

public class PlayerController : MonoBehaviour
{
    [Header("Player Basic Control Section")]
    //[Tooltip("Enter 1 Digit 1-4")]
    //public int PlayerControllerNumber;
    public float Thrust = 300f;
    public float JumpForce = 300f;
    public LayerMask JumpMask;
    public float RotationSpeed = 200f;
    [Tooltip("For Chicken, it should be 2, Duck, could be 1.9")]
    public float WalkSpeed = 2f;
    [Tooltip("It should be the same speed, little less than Duck's normal WalkSpeed")]
    public float PickUpSpeed = 1.8f;
    [HideInInspector]
    public float MaxBlockCD = 1f;
    [HideInInspector]
    public float BlockRegenInterval = 3f;
    // How many regen per time.deltatime
    [HideInInspector]
    public float BlockRegen = 1 / 120f;
    [Header("Player Body Setting Section")]
    public GameObject LegSwingReference;
    public GameObject Chest;
    [Tooltip("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] LeftArms;
    [Tooltip("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] RightArms;
    public GameObject LeftHand;
    public GameObject RightHand;
    public GameObject TurnReference;
    public GameObject[] OnDeathHidden;
    public GameObject MeleeChargingVFX;
    public GameObject MeleeUltimateVFX;

    [Header("Auxillary Aiming Section")]
    public bool EnableAuxillaryAiming = true;
    public float MaxWeaponCD = 0.3f;

    [HideInInspector]
    public GameObject HandObject;
    [HideInInspector]
    public float MeleeCharge = 0f;
    [HideInInspector]
    public float DropCharge = 0f;
    [HideInInspector]
    public bool IsPunching = false;
    [HideInInspector]
    public bool HandTaken = false;
    [HideInInspector]
    public bool IsOnGround = false;

    #region States
    private enum State
    {
        Empty,
        Walking,
        Jumping,
        Picking,
        Holding,
        MachineGuning,
        Dead,
        Shooting,
        Meleeing,
        Blocking,
    }
    // Normal State should include: Walking, Jumping, Picking, Holding, Dead
    private State normalState;
    // Attack State Should include: Shooting, Meleeing, Blocking
    private State attackState;
    #endregion

    #region Private Variable
    private Player _player;
    private Rigidbody _rb;
    private float _distToGround;
    private HingeJoint _chesthj;
    private HingeJoint _leftArm2hj;
    private HingeJoint _rightArm2hj;
    private HingeJoint _leftArmhj;
    private HingeJoint _rightArmhj;
    private HingeJoint _leftHandhj;
    private HingeJoint _rightHandhj;
    private bool _checkArm = true;
    private string _rightTriggerRegister = "";
    private bool _canControl = true;
    private Vector3 _freezeBody;
    private float _previousFrameVel = 0f;
    private float _weaponCD;
    [SerializeField]
    private float _axuillaryMaxDistance = 30f;
    [SerializeField]
    private float _auxillaryMaxAngle = 5f;
    private bool _auxillaryRotationLock = false;
    private bool _dropping = false;
    private float _blockCharge = 0f;
    private bool _blockCanRegen = false;
    #endregion

    [Header("Debug Section: Don't Ever Touch")]
    #region Debug Toggle
    public bool debugT_CheckArm = true;
    #endregion

    public void Init(int controllerNumber)
    {
        _player = ReInput.players.GetPlayer(controllerNumber);
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        _chesthj = Chest.GetComponent<HingeJoint>();
        _leftArm2hj = LeftArms[0].GetComponent<HingeJoint>();
        _rightArm2hj = RightArms[0].GetComponent<HingeJoint>();
        _leftArmhj = LeftArms[1].GetComponent<HingeJoint>();
        _rightArmhj = RightArms[1].GetComponent<HingeJoint>();
        _leftHandhj = LeftArms[2].GetComponent<HingeJoint>();
        _rightHandhj = RightArms[2].GetComponent<HingeJoint>();
        _freezeBody = new Vector3(0, transform.localEulerAngles.y, 0);
        LegSwingReference.GetComponent<Animator>().SetFloat("WalkSpeedMultiplier", WalkSpeed / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_canControl)
            return;

        CheckRewiredInput();
        CheckMovement();
        CheckJump();
        if (_checkArm && debugT_CheckArm)
            CheckArm();
        CheckFire();
        CheckDrop();
        CheckBlock();
    }

    // This is primarily for dropping item when velocity change too much 
    private void FixedUpdate()
    {
        //if (Mathf.Abs (_rb.velocity.magnitude - _previousFrameVel) >= GameManager.GM.DropWeaponVelocityThreshold)
        //{
        //    DropHelper ();
        //}
        if (_rb.velocity.magnitude >= GameManager.GM.DropWeaponVelocityThreshold)

            DropHelper();

        //_previousFrameVel = _rb.velocity.magnitude;

    }

    // Late Update is for standing the character
    private void LateUpdate()
    {
        _freezeBody.y = transform.localEulerAngles.y;
        transform.localEulerAngles = _freezeBody;
    }

    // OnEnterDeathZone controls the behavior how player reacts when it dies
    public void OnEnterDeathZone()
    {
        _canControl = false;
        foreach (GameObject go in OnDeathHidden)
        {
            go.SetActive(false);
        }
        DropHelper();
        StartCoroutine(Respawn(GameManager.GM.RespawnTime));
    }

    IEnumerator Respawn(float time)
    {
        _rb.isKinematic = true;
        GameManager.GM.SetToRespawn(gameObject, 5f);
        yield return new WaitForSeconds(time);
        _rb.isKinematic = false;
        GameManager.GM.SetToRespawn(gameObject, 0f);
        foreach (GameObject go in OnDeathHidden)
        {
            go.SetActive(true);
        }
        _canControl = true;
    }

    private void CheckDrop()
    {
        // Drop Only happens when player is holding something
        if (HandObject == null || normalState != State.Holding || attackState == State.Shooting)
            return;

        // If taken something, and pushed LT, drop the thing
        if (_player.GetButton("Left Trigger"))
        {
            _dropping = true;
            //DropHelper ();
            StartCoroutine(PowerDrop(2f));
        }
        else
        {
            if (_dropping)
            {
                StopAllCoroutines();
                JointSpring js = _chesthj.spring;
                js.targetPosition = 10f * DropCharge;
                DropCharge = 0f;
                _chesthj.spring = js;
                normalState = State.Empty;
                _dropping = false;
                DropHelper();
            }
        }
    }

    IEnumerator PowerDrop(float time)
    {
        float elapsedTime = 0f;
        JointSpring js = _chesthj.spring;

        float initspringtagetPosition = js.targetPosition;

        while (elapsedTime <= time)
        {
            DropCharge = elapsedTime / time;
            elapsedTime += Time.deltaTime;
            js.targetPosition = Mathf.Lerp(initspringtagetPosition, -100f, elapsedTime / time);
            _chesthj.spring = js;
            yield return new WaitForEndOfFrame();
        }

    }

    public void DropHelper()
    {
        if (HandObject == null) return;
        // Drop the thing
        HandObject.SendMessage("Drop");
        _auxillaryRotationLock = false;
        normalState = State.Empty;
        _dropping = false;
        // Change to non-dropping state after a while
        HandTaken = false;
        // Set Auxillary Aim to false
        _auxillaryRotationLock = false;
        // Return the body to normal position
        _checkArm = true;
        StopAllCoroutines();
        ResetBody();
        // These two are necessary for all objects
        HandObject.GetComponent<Rigidbody>().isKinematic = false;
        HandObject.layer = 0;
        // Specialized checking
        if (HandObject.CompareTag("Weapon"))
        {
            // Stop the shooting
            HandObject.SendMessage("Shoot", 0f);
        }
        if (HandObject.CompareTag("SuckGun"))
        {

        }
        // Nullify the holder
        HandObject = null;
        // Change the speed back to normal
        LegSwingReference.GetComponent<Animator>().SetFloat("WalkSpeedMultiplier", WalkSpeed / 2f);
        // Clear the right trigger register
        _rightTriggerRegister = "";
    }

    public void CheckFire()
    {
        //if (Mathf.Approximately(RightTrigger, 1f))
        if (_player.GetButton("Right Trigger"))
        {
            // Means we want to fire
            switch (_rightTriggerRegister)
            {
                case "Throwable":
                    LeftHand.GetComponent<Fingers>().Throw();
                    RightHand.GetComponent<Fingers>().Throw();
                    break;
                case "Weapon":
                    // Add weapon right trigger action
                    if (HandObject != null && !_dropping)
                    {
                        attackState = State.Shooting;
                        HandObject.SendMessage("Shoot", 1f);
                        if (EnableAuxillaryAiming)
                            AuxillaryAim();
                    }
                    break;
                case "WoodStamp":
                    if (HandObject != null && !_dropping && attackState != State.Shooting)
                    {
                        attackState = State.Shooting;
                        HandObject.SendMessage("Stamp", true);
                        if (EnableAuxillaryAiming) AuxillaryAim();
                        // Bend the body
                        JointSpring tempjs = _chesthj.spring;
                        tempjs.targetPosition = 40f;
                        _chesthj.spring = tempjs;
                    }
                    break;
                case "SuckGun":
                    if (HandObject != null && !_dropping)
                    {
                        HandObject.SendMessage("Suck", true);
                    }
                    break;
                case "Team1Resource":
                case "Team2Resource":
                    if (!_dropping)
                        DropHelper();
                    break;
                default:
                    //If we don't have anything on hand, we are applying melee action
                    if (attackState != State.Meleeing && normalState != State.Picking && normalState != State.Holding)
                    {
                        attackState = State.Meleeing;
                        _checkArm = false;
                        StopAllCoroutines();
                        StartCoroutine(MeleeClockFistHelper(_rightArm2hj, _rightArmhj, _rightHandhj, 1f, _leftArmhj));
                    }
                    break;
            }
        }
        else
        {
            switch (_rightTriggerRegister)
            {
                // Means we release the button
                case "Throwable":
                    break;
                case "Weapon":
                    attackState = State.Empty;
                    // Add weapon right trigger action
                    if (HandObject != null)
                        HandObject.SendMessage("Shoot", 0f);
                    // Auxillary Aiming
                    _auxillaryRotationLock = false;
                    _weaponCD = 0f;
                    break;
                case "WoodStamp":
                    attackState = State.Empty;
                    HandObject.SendMessage("Stamp", false);
                    _auxillaryRotationLock = false;

                    // Bend the body back
                    JointSpring tempjs = _chesthj.spring;
                    tempjs.targetPosition = -5f;
                    _chesthj.spring = tempjs;
                    break;
                case "SuckGun":
                    if (HandObject != null)
                        HandObject.SendMessage("Suck", false);
                    break;
                default:
                    // If we previously started melee and released the trigger, then release the fist
                    if (attackState == State.Meleeing)
                    {
                        attackState = State.Empty;
                        // This is add a push force when melee
                        //_rb.AddForce (transform.forward * Thrust * MeleeCharge * 3f, ForceMode.Impulse);
                        StopAllCoroutines();
                        StartCoroutine(MeleePunchHelper(_rightArmhj, _rightHandhj, 0.2f, _leftArmhj));
                    }
                    break;
            }
        }
    }

    public void CheckBlock()
    {
        print(_blockCharge);
        // When player released the button, should skip blockregeninterval seconds before it can regen
        if (_player.GetButtonUp("Block"))
        {
            StopCoroutine("blockRegen");
            StartCoroutine("blockRegen");
            ResetBody();
            attackState = State.Empty;
        }

        if (!_player.GetButton("Block") && _blockCanRegen)
        {
            _blockCharge -= Time.deltaTime;
            if (_blockCharge <= 0f) _blockCanRegen = false;
        }

        if (_blockCharge > MaxBlockCD)
        {
            attackState = State.Empty;
            ResetBody();
            return;
        }

        // if Player hold the button, check if player could block then block
        if (_player.GetButton("Block"))
        {
            attackState = State.Blocking;
            _blockCanRegen = false;
            BlockHelper(_leftArmhj, _leftHandhj);
            BlockHelper(_rightArmhj, _rightHandhj);
            _blockCharge += Time.deltaTime;
        }
    }

    IEnumerator blockRegen()
    {
        _blockCanRegen = false;
        yield return new WaitForSeconds(BlockRegenInterval);
        print("Block can now regen");
        _blockCanRegen = true;
    }

    private void AuxillaryAim()
    {
        // Auxillary Aiming
        _weaponCD += Time.deltaTime;
        if (_weaponCD <= MaxWeaponCD)
        {
            GameObject target = null;
            float minAngle = 360f;
            foreach (GameObject otherPlayer in GameManager.GM.Players)
            {
                if (otherPlayer != null && !otherPlayer.CompareTag(tag))
                {
                    // If other player are within max Distance, then check for the smalliest angle player
                    if (Vector3.Distance(otherPlayer.transform.position, gameObject.transform.position) <= _axuillaryMaxDistance)
                    {
                        Vector3 targetDir = otherPlayer.transform.position - transform.position;
                        float angle = Vector3.Angle(targetDir, transform.forward);
                        if (angle <= _auxillaryMaxAngle && angle < minAngle)
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
                _auxillaryRotationLock = true;
                transform.LookAt(target.transform);
            }
        }
        else
        {
            _auxillaryRotationLock = false;
        }
    }

    public void OnPickUpItem(string tag)
    {
        // Actual Logic Below
        _rightTriggerRegister = tag;
        // if pick up resource, then slow down
        if (tag == "Team1Resource" || tag == "Team2Resource")
            LegSwingReference.GetComponent<Animator>().SetFloat("WalkSpeedMultiplier", PickUpSpeed / 2f);

        switch (tag)
        {
            case "Team1Resource":
            case "Team2Resource":
            case "WoodStamp":
            case "SuckGun":
            case "Weapon":
                _checkArm = false;

                // Bend the body back
                JointSpring tempjs = _chesthj.spring;
                tempjs.targetPosition = -5f;
                _chesthj.spring = tempjs;

                StartCoroutine(PickUpWeaponHelper(_leftArm2hj, _leftArmhj, true, 0.1f));
                StartCoroutine(PickUpWeaponHelper(_rightArm2hj, _rightArmhj, false, 0.1f));
                break;
            case "Throwable":
                break;
            default:
                break;
        }
    }

    private void CheckArm()
    {
        // Arm2: right max: 90 --> -74
        //       left min: -75 --> 69
        // Arm: Connected Mass Scale 1 --> 0
        //      Target Position: 0 --> 180
        //      Limits: max 90 --> 121
        // Hand: Limit Max: 90 --> 0
        if (attackState == State.Meleeing || normalState == State.Holding) return;

        bool LTPushDown = _player.GetButton("Left Trigger");
        bool LTLiftUp = _player.GetButtonUp("Left Trigger");
        //LeftHand.GetComponent<Fingers>().SetTaken(Mathf.Approximately(0f, LeftTrigger) || _dropping);
        //RightHand.GetComponent<Fingers>().SetTaken(Mathf.Approximately(0f, LeftTrigger) || _dropping);
        LeftHand.GetComponent<Fingers>().SetTaken(!LTPushDown || _dropping);
        RightHand.GetComponent<Fingers>().SetTaken(!LTPushDown || _dropping);

        if (LTPushDown)
            normalState = State.Picking;
        else
            normalState = State.Empty;

        CheckArmHelper(LTPushDown, _leftArm2hj, _leftArmhj, _leftHandhj, true);
        CheckArmHelper(LTPushDown, _rightArm2hj, _rightArmhj, _rightHandhj, false);

        // Bend the body all together
        JointSpring tempjs = _chesthj.spring;
        tempjs.targetPosition = (LTPushDown ? 1f : 0f) * 90f;
        tempjs.targetPosition = Mathf.Clamp(tempjs.targetPosition, _chesthj.limits.min + 5, _chesthj.limits.max - 5);
        _chesthj.spring = tempjs;
    }

    // If sender is not null, meaning the hit could be blocked
    public void OnMeleeHit(Vector3 force, GameObject sender = null)
    {
        // Add HIT VFX
        GameObject par = Instantiate(VisualEffectManager.VEM.HitVFX, transform.position, Quaternion.Euler(0f, 180f + Vector3.SignedAngle(Vector3.forward, new Vector3(force.x, 0f, force.z), Vector3.up), 0f));
        // END VFX
        ParticleSystem.MainModule psmain = par.GetComponent<ParticleSystem>().main;
        ParticleSystem.MainModule psmain2 = par.transform.GetChild(0).GetComponent<ParticleSystem>().main;
        psmain.maxParticles = (int)Mathf.Round((9f / 51005f) * force.magnitude * force.magnitude);
        psmain2.maxParticles = (int)Mathf.Round(12f / 255025f * force.magnitude * force.magnitude);

        if (sender != null && attackState == State.Blocking)
        {
            sender.GetComponentInParent<PlayerController>().OnMeleeHit(force * -3f);
        }
        else
        {
            _rb.AddForce(force, ForceMode.Impulse);
        }

    }

    private void CheckJump()
    {
        if (_player.GetButtonDown("Jump") && IsGrounded())
        {
            _rb.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
        }
    }

    public void ApplyWalkForce(float _force)
    {
        //string HLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis1";
        //string VLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis2";
        float HLAxis = _player.GetAxis("Move Horizontal");
        float VLAxis = _player.GetAxis("Move Vertical");

        if (IsGrounded() && (!Mathf.Approximately(HLAxis, 0f) || !Mathf.Approximately(VLAxis, 0f)))
            _rb.AddForce(transform.forward * _force, ForceMode.Impulse);
    }

    private void CheckMovement()
    {
        //string HLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis1";
        //string VLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis2";
        float HLAxis = _player.GetAxis("Move Horizontal");
        float VLAxis = _player.GetAxis("Move Vertical");

        if (!Mathf.Approximately(HLAxis, 0f) || !Mathf.Approximately(VLAxis, 0f))
        {
            // Get the percent of input force player put in
            float normalizedInputVal = Mathf.Sqrt(Mathf.Pow(HLAxis, 2f) + Mathf.Pow(VLAxis, 2f)) / Mathf.Sqrt(2);
            // Add force based on that percentage
            if (IsGrounded())
                _rb.AddForce(transform.forward * Thrust * normalizedInputVal);
            else
                _rb.AddForce(transform.forward * Thrust * normalizedInputVal * 0.5f);
            // Turn player according to the rotation of the joystick
            //transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.Atan2(HLAxis, VLAxis * -1f) * Mathf.Rad2Deg, transform.eulerAngles.z);
            float playerRot = transform.rotation.eulerAngles.y > 180f ? (transform.rotation.eulerAngles.y - 360f) : transform.rotation.eulerAngles.y;
            float controllerRot = Mathf.Atan2(HLAxis, VLAxis * -1f) * Mathf.Rad2Deg;
            if (!(Mathf.Abs(playerRot - controllerRot) < 20f))
            {
                //float mirrorAngle = playerRot > 0f ? (playerRot - 180f) : (playerRot + 180f);
                //float rotation = controllerRot - playerRot > 0f ? 1f : -1f;
                //Debug.Log("Controller Rotation: " + controllerRot);
                //Debug.Log("Plauer Rotation: " + playerRot);
                //transform.Rotate(new Vector3(0f, rotation * RotationSpeed, 0f) * Time.deltaTime);
                //_rb.GetComponent<Rigidbody>().AddForce(transform.forward * Thrust * normalizedInputVal * 3f);
                RotationSpeed += Time.deltaTime;
            }
            else
            {
                RotationSpeed = 4f;
            }
            // Check if player's speed is not within x degree of the controller angle
            // Then disable the animator if so
            // Turn on the animator of the Leg Swing Preference
            float playerVelRot = Mathf.Atan2(_rb.velocity.x, _rb.velocity.z) * Mathf.Rad2Deg;



            //LegSwingReference.GetComponent<Animator>().enabled = IsGrounded();
            LegSwingReference.GetComponent<Animator>().enabled = true;

            RotationSpeed = Mathf.Clamp(RotationSpeed, 4f, 15f);
            Transform target = TurnReference.transform.GetChild(0);
            Vector3 relativePos = target.position - transform.position;

            TurnReference.transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.Atan2(HLAxis, VLAxis * -1f) * Mathf.Rad2Deg, transform.eulerAngles.z);
            Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
            Quaternion tr = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * RotationSpeed);
            if (!_auxillaryRotationLock)
                transform.rotation = tr;
        }
        else
        {
            LegSwingReference.GetComponent<Animator>().enabled = false;
            LegSwingReference.transform.eulerAngles = Vector3.zero;
        }
    }

    #region Helper Functions

    private void CheckRewiredInput()
    {
        if (_player == null) return;
        //LeftTrigger = _player.GetAxis("Left Trigger");
        //RightTrigger = _player.GetAxis("Right Trigger");

        //LeftTrigger = Mathf.Approximately(LeftTrigger, 0f) || Mathf.Approximately(LeftTrigger, -1f) ? 0f : 1f;
        //RightTrigger = Mathf.Approximately(RightTrigger, 0f) || Mathf.Approximately(RightTrigger, -1f) ? 0f : 1f;

        if (!_player.GetButton("Left Trigger") && HandObject != null)
            normalState = State.Holding;
    }

    private bool IsGrounded()
    {
        RaycastHit hit;
        return Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, _distToGround, JumpMask);
    }

    //private void CheckArmHelper(float TriggerValue, HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, bool IsLeftHand)
    //{
    //    // Arm2: right max: 90 --> -74
    //    //       left min: -75 --> 69        
    //    JointLimits lm1 = Arm2hj.limits;
    //    if (IsLeftHand)
    //        lm1.min = Mathf.Approximately(TriggerValue, 1f) ? 69f : -75f;
    //    else
    //        lm1.max = Mathf.Approximately(TriggerValue, 1f) ? -74f : 90f;
    //    Arm2hj.limits = lm1;

    //    //  Arm: Limits: -90, 90 --> 0, 121
    //    JointLimits lm = Armhj.limits;
    //    lm.max = Mathf.Approximately(TriggerValue, 1f) ? 121f : 90f;
    //    lm.min = Mathf.Approximately(TriggerValue, 1f) ? 0f : -90f;
    //    Armhj.limits = lm;

    //    // Arm: Target Position: 0 --> 180
    //    JointSpring js = Armhj.spring;
    //    js.targetPosition = 180f * TriggerValue;
    //    Armhj.spring = js;

    //    // Hand: Limit Max: 90 --> 0
    //    JointLimits tlm = Handhj.limits;
    //    tlm.max = 90f * (1f - TriggerValue);
    //    Handhj.limits = tlm;

    //}

    private void CheckArmHelper(bool down, HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, bool IsLeftHand)
    {
        // Arm2: right max: 90 --> -74
        //       left min: -75 --> 69        
        JointLimits lm1 = Arm2hj.limits;
        if (IsLeftHand)
            lm1.min = down ? 69f : -75f;
        else
            lm1.max = down ? -74f : 90f;
        Arm2hj.limits = lm1;

        //  Arm: Limits: -90, 90 --> 0, 121
        JointLimits lm = Armhj.limits;
        lm.max = down ? 121f : 90f;
        lm.min = down ? 0f : -90f;
        Armhj.limits = lm;

        // Arm: Target Position: 0 --> 180
        JointSpring js = Armhj.spring;
        js.targetPosition = down ? 180f : 0f;
        Armhj.spring = js;

        // Hand: Limit Max: 90 --> 0
        JointLimits tlm = Handhj.limits;
        tlm.max = down ? 0f : 90f;
        Handhj.limits = tlm;

    }

    void ResetBody()
    {
        ResetBodyHelper(_leftArm2hj, _leftArmhj, _leftHandhj, true);
        ResetBodyHelper(_rightArm2hj, _rightArmhj, _rightHandhj, false);
        JointSpring js = _chesthj.spring;
        js.targetPosition = 0f;
        _chesthj.spring = js;
    }

    // Reset All Body
    void ResetBodyHelper(HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, bool IsLeftHand)
    {
        JointLimits lm2 = Arm2hj.limits;
        JointLimits lm = Armhj.limits;
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
        lm.min = -90f;
        lm.max = 90f;
        js.targetPosition = 0f;
        hjs.targetPosition = 0f;
        Handhj.spring = hjs;
        Armhj.spring = js;
        Armhj.limits = lm;
        Arm2hj.limits = lm2;
    }

    private void BlockHelper(HingeJoint Armhj, HingeJoint Handhj)
    {
        JointSpring ajs = Armhj.spring;
        ajs.targetPosition = 100f;
        Armhj.spring = ajs;

        JointSpring hjs = Handhj.spring;
        hjs.targetPosition = 120f;
        Handhj.spring = hjs;
    }

    IEnumerator MeleeClockFistHelper(HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, float time, HingeJoint LeftArmhj)
    {
        IsPunching = false;
        float elapesdTime = 0f;
        JointLimits lm2 = Arm2hj.limits;
        JointLimits hl = Handhj.limits;
        JointSpring js = Armhj.spring;
        JointSpring ljs = LeftArmhj.spring;

        float initLm2Max = lm2.max;
        float initLm2Min = lm2.min;
        float initLmTargetPosition = js.targetPosition;
        float inithlMax = hl.max;
        float inithlMin = hl.min;
        float initLATargetPosition = ljs.targetPosition;
        // VFX section
        MeleeChargingVFX.GetComponent<ParticleSystem>().Play();
        // VFX END

        while (elapesdTime < time)
        {
            elapesdTime += Time.deltaTime;
            MeleeCharge = elapesdTime / time;

            // VFX Section
            MeleeChargingVFX.transform.localScale = new Vector3(0.8f * MeleeCharge, 0.8f * MeleeCharge, 0.8f * MeleeCharge);
            // VFX END
            lm2.max = Mathf.Lerp(initLm2Max, 4f, MeleeCharge);
            lm2.min = Mathf.Lerp(initLm2Min, -17f, MeleeCharge);

            ljs.targetPosition = Mathf.Lerp(initLATargetPosition, 80f, MeleeCharge);
            js.targetPosition = Mathf.Lerp(initLmTargetPosition, -85f, MeleeCharge);

            hl.max = Mathf.Lerp(inithlMax, 130f, MeleeCharge);
            hl.min = Mathf.Lerp(inithlMin, 128f, MeleeCharge);

            Arm2hj.limits = lm2;
            Armhj.spring = js;
            LeftArmhj.spring = ljs;
            Handhj.limits = hl;
            yield return new WaitForEndOfFrame();
        }
        // VFX Section: Charged, Ult now
        MeleeChargingVFX.GetComponent<ParticleSystem>().Stop();
        MeleeUltimateVFX.GetComponent<ParticleSystem>().Play();
        // END
    }

    IEnumerator MeleePunchHelper(HingeJoint Armhj, HingeJoint Handhj, float time, HingeJoint LeftHandhj)
    {
        float elapesdTime = 0f;
        IsPunching = true;
        JointLimits hl = Handhj.limits;
        JointSpring js = Armhj.spring;
        JointSpring ljs = LeftHandhj.spring;
        // VFX Section
        MeleeChargingVFX.GetComponent<ParticleSystem>().Stop();
        // END
        float initLmTargetPosition = js.targetPosition;
        float initLATargetPosition = ljs.targetPosition;
        float inithlMax = hl.max;
        float inithlMin = hl.min;
        //Armhj.connectedMassScale = 0.2f;

        while (elapesdTime < time)
        {
            elapesdTime += Time.deltaTime;
            ljs.targetPosition = Mathf.Lerp(initLATargetPosition, -120f, elapesdTime / time);
            js.targetPosition = Mathf.Lerp(initLmTargetPosition, 180f, elapesdTime / time);
            hl.max = Mathf.Lerp(inithlMax, 12f, elapesdTime / time);
            hl.min = Mathf.Lerp(inithlMin, -2.8f, elapesdTime / time);
            LeftHandhj.spring = ljs;
            Armhj.spring = js;
            Handhj.limits = hl;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.1f);
        // VFX Section
        MeleeUltimateVFX.GetComponent<ParticleSystem>().Stop();
        //VFX END
        _checkArm = true;
        Armhj.connectedMassScale = 1f;
        ResetBody();
        IsPunching = false;
        MeleeCharge = 0f;
    }

    IEnumerator PickUpWeaponHelper(HingeJoint Arm2hj, HingeJoint Armhj, bool IsLeftHand, float time)
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
        Armhj.connectedMassScale = 1f;
    }
    #endregion
}

//private void CheckRun ()
//{
//    if (Input.GetKey (RunCode) && IsGrounded ())
//    {
//        LegSwingReference.GetComponent<Animator> ().speed = 2f;
//        _rb.AddForce (transform.forward * Thrust * RunSpeedMultiplier);
//    }
//    else
//    {
//        LegSwingReference.GetComponent<Animator> ().speed = 1.6f;
//    }
//}