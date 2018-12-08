﻿using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header ("Player Basic Control Section")]
    [Tooltip ("Enter 1 Digit 1-4")]
    public string PlayerControllerNumber;
    public float Thrust = 300f;
    public float JumpForce = 300f;
    public LayerMask JumpMask;
    public float RotationSpeed = 200f;
    public float RunSpeedMultiplier = 1f;
    [Header ("Player Body Setting Section")]
    public GameObject HeadGunPos;
    public GameObject LegSwingReference;
    public GameObject Chest;
    [Tooltip ("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] LeftArms;
    [Tooltip ("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] RightArms;
    public GameObject LeftHand;
    public GameObject RightHand;
    public GameObject TurnReference;
    public GameObject[] OnDeathHidden;

    [Header ("Auxillary Aiming Section")]
    public bool EnableAuxillaryAiming = true;
    public float MaxWeaponCD = 0.3f;

    [HideInInspector]
    public GameObject HandObject;
    [HideInInspector]
    public float MeleeCharge = 0f;
    [HideInInspector]
    public bool IsPunching = false;
    [HideInInspector]
    public bool HandTaken = false;

    #region States
    private enum State
    {
        Empty,
        Walking,
        Jumping,
        Shooting,
        Meleeing,
        Picking,
        Holding,
        Dead,
    }
    // Normal State should include: Walking, Jumping, Picking, Holding, Dead
    private State normalState;
    // Attack State Should include: Shooting, Meleeing
    private State attackState;
    #endregion

    #region Private Variable
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
    private bool _startMelee = false;
    private bool _canControl = true;
    private Vector3 _freezeBody;
    private float _previousFrameVel = 0f;
    private float _weaponCD;
    [SerializeField]
    private float _axuillaryMaxDistance = 30f;
    [SerializeField]
    private float _auxillaryMaxAngle = 5f;
    private bool _auxillaryRotationLock = false;
    #endregion

    #region Controller Variables
    private KeyCode JumpCode;
    private KeyCode RunCode;
    private KeyCode YButton;
    private string RTStr;
    private string LTStr;
    //private bool CanDrop = false;
    private float RightTrigger;
    private float LeftTrigger;
    #endregion

    [Header ("Debug Section: Don't Ever Touch")]
    #region Debug Toggle
    public bool debugT_CheckArm = true;
    #endregion

    private void Start ()
    {
        _rb = GetComponent<Rigidbody> ();
        _distToGround = GetComponent<CapsuleCollider> ().bounds.extents.y;
        _chesthj = Chest.GetComponent<HingeJoint> ();
        _leftArm2hj = LeftArms[0].GetComponent<HingeJoint> ();
        _rightArm2hj = RightArms[0].GetComponent<HingeJoint> ();
        _leftArmhj = LeftArms[1].GetComponent<HingeJoint> ();
        _rightArmhj = RightArms[1].GetComponent<HingeJoint> ();
        _leftHandhj = LeftArms[2].GetComponent<HingeJoint> ();
        _rightHandhj = RightArms[2].GetComponent<HingeJoint> ();
        _freezeBody = new Vector3 (0, transform.localEulerAngles.y, 0);
    }

    // Update is called once per frame
    void Update ()
    {
        if (!_canControl)
            return;

        CheckAllInput ();
        CheckMovement ();
        CheckJump ();
        if (_checkArm && debugT_CheckArm)
            CheckArm ();
        CheckFire ();
        // CheckRun ();
        CheckDrop ();
    }

    // This is primarily for dropping item when velocity change too much 
    private void FixedUpdate ()
    {
        if (Mathf.Abs (_rb.velocity.magnitude - _previousFrameVel) >= GameManager.GM.DropWeaponVelocityThreshold)
        {
            DropHelper ();
        }
        _previousFrameVel = _rb.velocity.magnitude;

    }

    // Late Update is for standing the character
    private void LateUpdate ()
    {
        _freezeBody.y = transform.localEulerAngles.y;
        transform.localEulerAngles = _freezeBody;
    }

    // OnEnterDeathZone controls the behavior how player reacts when it dies
    public void OnEnterDeathZone ()
    {
        _canControl = false;
        foreach (GameObject go in OnDeathHidden)
        {
            go.SetActive (false);
        }
        DropHelper ();
        GameManager.GM.SetToRespawn (gameObject);
        StartCoroutine (respawn (GameManager.GM.RespawnTime));
    }

    IEnumerator respawn (float time)
    {
        yield return new WaitForSeconds (time);
        foreach (GameObject go in OnDeathHidden)
        {
            go.SetActive (true);
        }
        _canControl = true;
    }

    private void CheckDrop ()
    {
        // Drop Only happens when player is holding something
        if (HandObject == null || normalState != State.Holding || attackState == State.Shooting)
            return;

        // If taken something, and pushed Y, drop the thing
        if (Mathf.Approximately (1f, LeftTrigger))
        {
            normalState = State.Empty;
            DropHelper ();
        }
    }

    public void DropHelper ()
    {
        if (HandObject == null) return;
        // Drop the thing
        HandObject.SendMessage ("Drop");
        HandTaken = false;
        // Return the body to normal position
        _checkArm = true;
        StopAllCoroutines ();
        ResetBody ();
        // Specialized checking
        if (HandObject.CompareTag ("Weapon"))
        {
            HandObject.GetComponent<Rigidbody> ().isKinematic = false;
            HandObject.layer = 0;
            // Stop the shooting
            HandObject.SendMessage ("Shoot", 0f);
        }
        if (HandObject.CompareTag ("Team1Resource") || HandObject.CompareTag ("Team2Resource"))
        {
            HandObject.GetComponent<Rigidbody> ().isKinematic = false;
            HandObject.layer = 0;
        }
        // Nullify the holder
        HandObject = null;
        // Clear the right trigger register
        _rightTriggerRegister = "";
    }

    public void CheckFire ()
    {
        if (Mathf.Approximately (RightTrigger, 1f))
        {
            // Means we want to fire
            switch (_rightTriggerRegister)
            {
                case "Throwable":
                    LeftHand.GetComponent<Fingers> ().Throw ();
                    RightHand.GetComponent<Fingers> ().Throw ();
                    break;
                case "Weapon":
                    // Add weapon right trigger action
                    if (HandObject != null)
                    {
                        attackState = State.Shooting;
                        HandObject.SendMessage ("Shoot", 1f);
                        if (EnableAuxillaryAiming)
                            AuxillaryAim ();
                    }
                    break;
                default:
                    //If we don't have anything on hand, we are applying melee action
                    if (attackState != State.Meleeing && normalState != State.Picking)
                    {
                        attackState = State.Meleeing;
                        _checkArm = false;
                        StopAllCoroutines ();
                        StartCoroutine (MeleeClockFistHelper (_rightArm2hj, _rightArmhj, _rightHandhj, 1f));
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
                        HandObject.SendMessage ("Shoot", 0f);
                    // Auxillary Aiming
                    _auxillaryRotationLock = false;
                    _weaponCD = 0f;
                    break;
                default:
                    // If we previously started melee and released the trigger, then release the fist
                    if (attackState == State.Meleeing)
                    {
                        attackState = State.Empty;
                        StopAllCoroutines ();
                        StartCoroutine (MeleePunchHelper (_rightArmhj, _rightHandhj, 0.2f));
                    }
                    break;
            }
        }
    }

    private void AuxillaryAim ()
    {
        // Auxillary Aiming
        _weaponCD += Time.deltaTime;
        if (_weaponCD <= MaxWeaponCD)
        {
            GameObject target = null;
            float minAngle = 360f;
            foreach (GameObject otherPlayer in GameManager.GM.Players)
            {
                if (!otherPlayer.CompareTag (tag))
                {
                    // If other player are within max Distance, then check for the smalliest angle player
                    if (Vector3.Distance (otherPlayer.transform.position, gameObject.transform.position) <= _axuillaryMaxDistance)
                    {
                        Vector3 targetDir = otherPlayer.transform.position - transform.position;
                        float angle = Vector3.Angle (targetDir, transform.forward);
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
                transform.LookAt (target.transform);
            }
        }
        else
        {
            _auxillaryRotationLock = false;
        }
    }

    public void OnPickUpItem (string tag)
    {
        // Actual Logic Below
        _rightTriggerRegister = tag;
        switch (tag)
        {
            case "Team1Resource":
            case "Team2Resource":
            case "Weapon":
                _checkArm = false;

                // Bend the body back
                JointSpring tempjs = _chesthj.spring;
                tempjs.targetPosition = -5f;
                _chesthj.spring = tempjs;

                StartCoroutine (PickUpWeaponHelper (_leftArm2hj, _leftArmhj, true, 2f));
                StartCoroutine (PickUpWeaponHelper (_rightArm2hj, _rightArmhj, false, 2f));
                break;
            case "Throwable":

                break;
            default:
                break;
        }
    }

    private void CheckArm ()
    {
        // Arm2: right max: 90 --> -74
        //       left min: -75 --> 69
        // Arm: Connected Mass Scale 1 --> 0
        //      Target Position: 0 --> 180
        //      Limits: max 90 --> 121
        // Hand: Limit Max: 90 --> 0
        if (attackState == State.Meleeing || normalState == State.Holding) return;

        LeftHand.GetComponent<Fingers> ().SetTaken (Mathf.Approximately (0f, LeftTrigger));
        RightHand.GetComponent<Fingers> ().SetTaken (Mathf.Approximately (0f, LeftTrigger));

        if (Mathf.Approximately (1f, LeftTrigger))
            normalState = State.Picking;
        else
            normalState = State.Empty;

        CheckArmHelper (LeftTrigger, _leftArm2hj, _leftArmhj, _leftHandhj, true);
        CheckArmHelper (LeftTrigger, _rightArm2hj, _rightArmhj, _rightHandhj, false);

        // Bend the body all together
        JointSpring tempjs = _chesthj.spring;
        tempjs.targetPosition = (Mathf.Approximately (LeftTrigger, 1f) ? 1f : 0f) * 90f;
        tempjs.targetPosition = Mathf.Clamp (tempjs.targetPosition, _chesthj.limits.min + 5, _chesthj.limits.max - 5);
        _chesthj.spring = tempjs;
    }

    private void CheckRun ()
    {
        if (Input.GetKey (RunCode) && IsGrounded ())
        {
            LegSwingReference.GetComponent<Animator> ().speed = 2f;
            _rb.AddForce (transform.forward * Thrust * RunSpeedMultiplier);
        }
        else
        {
            LegSwingReference.GetComponent<Animator> ().speed = 1.6f;
        }
    }

    public void OnMeleeHit (Vector3 force)
    {
        _rb.AddForce (force, ForceMode.Impulse);
    }

    private void CheckJump ()
    {
        if (Input.GetKeyDown (JumpCode) && IsGrounded ())
        {
            _rb.AddForce (new Vector3 (0, JumpForce, 0), ForceMode.Impulse);
        }
    }

    public void ApplyWalkForce (float _force)
    {
        string HLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis1";
        string VLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis2";
        float HLAxis = Input.GetAxis (HLcontrollerStr);
        float VLAxis = Input.GetAxis (VLcontrollerStr);

        if (IsGrounded () && (!Mathf.Approximately (HLAxis, 0f) || !Mathf.Approximately (VLAxis, 0f)))
            _rb.AddForce (transform.forward * _force, ForceMode.Impulse);
    }

    private void CheckMovement ()
    {
        string HLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis1";
        string VLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis2";
        float HLAxis = Input.GetAxis (HLcontrollerStr);
        float VLAxis = Input.GetAxis (VLcontrollerStr);

        if (!Mathf.Approximately (HLAxis, 0f) || !Mathf.Approximately (VLAxis, 0f))
        {
            // Get the percent of input force player put in
            float normalizedInputVal = Mathf.Sqrt (Mathf.Pow (HLAxis, 2f) + Mathf.Pow (VLAxis, 2f)) / Mathf.Sqrt (2);
            // Add force based on that percentage
            if (IsGrounded ())
                _rb.AddForce (transform.forward * Thrust * normalizedInputVal);
            // Turn player according to the rotation of the joystick
            //transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.Atan2(HLAxis, VLAxis * -1f) * Mathf.Rad2Deg, transform.eulerAngles.z);
            float playerRot = transform.rotation.eulerAngles.y > 180f ? (transform.rotation.eulerAngles.y - 360f) : transform.rotation.eulerAngles.y;
            float controllerRot = Mathf.Atan2 (HLAxis, VLAxis * -1f) * Mathf.Rad2Deg;
            if (!(Mathf.Abs (playerRot - controllerRot) < 20f))
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
            float playerVelRot = Mathf.Atan2 (_rb.velocity.x, _rb.velocity.z) * Mathf.Rad2Deg;

            LegSwingReference.GetComponent<Animator> ().enabled = IsGrounded () && (Mathf.Abs (playerVelRot - playerRot) < 90f);

            RotationSpeed = Mathf.Clamp (RotationSpeed, 4f, 15f);
            Transform target = TurnReference.transform.GetChild (0);
            Vector3 relativePos = target.position - transform.position;

            TurnReference.transform.eulerAngles = new Vector3 (transform.eulerAngles.x, Mathf.Atan2 (HLAxis, VLAxis * -1f) * Mathf.Rad2Deg, transform.eulerAngles.z);
            Quaternion rotation = Quaternion.LookRotation (relativePos, Vector3.up);
            Quaternion tr = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * RotationSpeed);
            if (!_auxillaryRotationLock)
                transform.rotation = tr;
        }
        else
        {
            LegSwingReference.GetComponent<Animator> ().enabled = false;
            LegSwingReference.transform.eulerAngles = Vector3.zero;
        }
    }

    #region Helper Functions

    private void CheckAllInput ()
    {
        // For L&R Triggers
#if UNITY_EDITOR_OSX
        LTStr = "Joy" + PlayerControllerNumber + "Axis5";
        RTStr = "Joy" + PlayerControllerNumber + "Axis6";
#endif

#if UNITY_EDITOR_WIN
        LTStr = "Joy" + PlayerControllerNumber + "Axis9";
        RTStr = "Joy" + PlayerControllerNumber + "Axis10";
#endif
        RightTrigger = Input.GetAxis (RTStr);
        RightTrigger = Mathf.Approximately (RightTrigger, 0f) || Mathf.Approximately (RightTrigger, -1f) ? 0f : 1f;

        LeftTrigger = Input.GetAxis (LTStr);
        LeftTrigger = Mathf.Approximately (LeftTrigger, 0f) || Mathf.Approximately (LeftTrigger, -1f) ? 0f : 1f;

        // Only change to holding state when player released the LT button and holding something
        if (Mathf.Approximately (0f, LeftTrigger) && HandObject != null)
            normalState = State.Holding;

        // For A, B, X, Y Buttons
        // RunCode is Button L Axis
        // JumpCode is Button A
        switch (PlayerControllerNumber)
        {
            case "1":
#if UNITY_EDITOR_OSX
                JumpCode = KeyCode.Joystick1Button16;
                RunCode = KeyCode.Joystick1Button11;
                YButton = KeyCode.Joystick1Button19;
#endif
#if UNITY_EDITOR_WIN
                JumpCode = KeyCode.Joystick1Button0;
                RunCode = KeyCode.Joystick1Button8;
                YButton = KeyCode.Joystick1Button3;

#endif
                break;
            case "2":
#if UNITY_EDITOR_OSX
                JumpCode = KeyCode.Joystick2Button16;
                RunCode = KeyCode.Joystick2Button11;
                YButton = KeyCode.Joystick2Button19;

#endif
#if UNITY_EDITOR_WIN
                JumpCode = KeyCode.Joystick2Button0;
                RunCode = KeyCode.Joystick2Button8;
                YButton = KeyCode.Joystick2Button3;
#endif
                break;
            case "3":
#if UNITY_EDITOR_OSX
                JumpCode = KeyCode.Joystick3Button16;
                RunCode = KeyCode.Joystick3Button11;
                YButton = KeyCode.Joystick3Button19;

#endif
#if UNITY_EDITOR_WIN
                JumpCode = KeyCode.Joystick3Button0;
                RunCode = KeyCode.Joystick3Button8;
                YButton = KeyCode.Joystick3Button3;
#endif
                break;
            case "4":
#if UNITY_EDITOR_OSX
                JumpCode = KeyCode.Joystick4Button16;
                RunCode = KeyCode.Joystick4Button11;
                YButton = KeyCode.Joystick4Button19;

#endif
#if UNITY_EDITOR_WIN
                JumpCode = KeyCode.Joystick4Button0;
                RunCode = KeyCode.Joystick4Button8;
                YButton = KeyCode.Joystick4Button3;
#endif
                break;
        }

    }

    private bool IsGrounded ()
    {
        return Physics.Raycast (transform.position, -Vector3.up, _distToGround + 0.2f, JumpMask);
    }

    private void CheckArmHelper (float TriggerValue, HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, bool IsLeftHand)
    {
        // Arm2: right max: 90 --> -74
        //       left min: -75 --> 69        
        JointLimits lm1 = Arm2hj.limits;
        if (IsLeftHand)
            lm1.min = Mathf.Approximately (TriggerValue, 1f) ? 69f : -75f;
        else
            lm1.max = Mathf.Approximately (TriggerValue, 1f) ? -74f : 90f;
        Arm2hj.limits = lm1;

        // Arm: Connected Mass Scale 1 --> 0
        Armhj.connectedMassScale = Mathf.Approximately (TriggerValue, 1f) ? 0f : 1f;

        //  Arm: Limits: -90, 90 --> 0, 121
        JointLimits lm = Armhj.limits;
        lm.max = Mathf.Approximately (TriggerValue, 1f) ? 121f : 90f;
        lm.min = Mathf.Approximately (TriggerValue, 1f) ? 0f : -90f;
        Armhj.limits = lm;

        // Arm: Target Position: 0 --> 180
        JointSpring js = Armhj.spring;
        js.targetPosition = 180f * TriggerValue;
        Armhj.spring = js;

        // Hand: Limit Max: 90 --> 0
        JointLimits tlm = Handhj.limits;
        tlm.max = 90f * (1f - TriggerValue);
        Handhj.limits = tlm;

    }

    void ResetBody ()
    {
        ResetBodyHelper (_leftArm2hj, _leftArmhj, true);
        ResetBodyHelper (_rightArm2hj, _rightArmhj, false);
    }

    void ResetBodyHelper (HingeJoint Arm2hj, HingeJoint Armhj, bool IsLeftHand)
    {
        JointLimits lm2 = Arm2hj.limits;

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
        Arm2hj.limits = lm2;
    }

    IEnumerator MeleeClockFistHelper (HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, float time)
    {
        IsPunching = false;
        float elapesdTime = 0f;
        JointLimits lm2 = Arm2hj.limits;
        JointLimits hl = Handhj.limits;
        JointSpring js = Armhj.spring;

        float initLm2Max = lm2.max;
        float initLm2Min = lm2.min;
        float initLmTargetPosition = js.targetPosition;
        float inithlMax = hl.max;
        float inithlMin = hl.min;

        while (elapesdTime < time)
        {
            elapesdTime += Time.deltaTime;
            MeleeCharge = elapesdTime / time;
            lm2.max = Mathf.Lerp (initLm2Max, 4f, elapesdTime / time);
            lm2.min = Mathf.Lerp (initLm2Min, -17f, elapesdTime / time);

            js.targetPosition = Mathf.Lerp (initLmTargetPosition, -85f, elapesdTime / time);
            hl.max = Mathf.Lerp (inithlMax, 130f, elapesdTime / time);
            hl.min = Mathf.Lerp (inithlMin, 128f, elapesdTime / time);
            Arm2hj.limits = lm2;
            Armhj.spring = js;
            Handhj.limits = hl;
            yield return new WaitForEndOfFrame ();
        }
    }

    IEnumerator MeleePunchHelper (HingeJoint Armhj, HingeJoint Handhj, float time)
    {
        float elapesdTime = 0f;
        IsPunching = true;
        JointLimits hl = Handhj.limits;
        JointSpring js = Armhj.spring;

        float initLmTargetPosition = js.targetPosition;
        float inithlMax = hl.max;
        float inithlMin = hl.min;
        Armhj.connectedMassScale = 0.2f;

        while (elapesdTime < time)
        {
            elapesdTime += Time.deltaTime;

            js.targetPosition = Mathf.Lerp (initLmTargetPosition, 180f, elapesdTime / time);
            hl.max = Mathf.Lerp (inithlMax, 12f, elapesdTime / time);
            hl.min = Mathf.Lerp (inithlMin, -2.8f, elapesdTime / time);
            Armhj.spring = js;
            Handhj.limits = hl;
            yield return new WaitForEndOfFrame ();
        }
        yield return new WaitForSeconds (0.1f);
        _checkArm = true;
        Armhj.connectedMassScale = 1f;
        ResetBody ();
        IsPunching = false;
        MeleeCharge = 0f;
    }

    IEnumerator PickUpWeaponHelper (HingeJoint Arm2hj, HingeJoint Armhj, bool IsLeftHand, float time)
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
                lm2.max = Mathf.Lerp (initLm2LeftMax, 103f, elapesdTime / time);
                lm2.min = Mathf.Lerp (initLm2LeftMin, 95f, elapesdTime / time);
            }
            else
            {
                lm2.max = Mathf.Lerp (initLm2LeftMax, -88f, elapesdTime / time);
                lm2.min = Mathf.Lerp (initLm2LeftMin, -98f, elapesdTime / time);
            }
            lm.max = Mathf.Lerp (initLmMax, 180f, elapesdTime / time);
            Arm2hj.limits = lm2;
            Armhj.limits = lm;
            yield return new WaitForEndOfFrame ();
        }
        Armhj.connectedMassScale = 1f;
    }
    #endregion
}