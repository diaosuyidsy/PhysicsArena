using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Tooltip ("Enter 1 Digit 1-4")]
    public string PlayerControllerNumber;
    public float Thrust = 300f;
    public float JumpForce = 300f;
    public float RotationSpeed = 200f;
    public GameObject LegSwingReference;
    public GameObject Chest;
    [Tooltip ("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] LeftArms;
    [Tooltip ("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] RightArms;

    public GameObject TurnReference;
    public LayerMask JumpMask;

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
    }

    // Update is called once per frame
    void Update ()
    {
        CheckMovement ();
        CheckJump ();
        CheckArm ();
    }


    private void CheckArm ()
    {
        // For the left side
#if UNITY_EDITOR_OSX
        string LTStr = "Joy" + PlayerControllerNumber + "Axis5";
        string RTStr = "Joy" + PlayerControllerNumber + "Axis6";
#endif

#if UNITY_EDITOR_WIN
        string LTStr = "Joy" + PlayerControllerNumber + "Axis9";
        string RTStr = "Joy" + PlayerControllerNumber + "Axis10";
#endif
        float LeftTrigger = Input.GetAxis (LTStr);
        LeftTrigger = Mathf.Approximately (LeftTrigger, 0f) || Mathf.Approximately (LeftTrigger, -1f) ? 0f : 1f;

        // Arm2: Min -90 --> 89
        // Arm: Connected Mass Scale 1 --> 0
        //      Target Position: 0 --> 180
        //      Limits: -90, -90 --> 81, 120
        // Hand: Limit Max: 90 --> 0

        CheckArmHelper (LeftTrigger, _leftArm2hj, _leftArmhj, _leftHandhj, true);
        CheckArmHelper (LeftTrigger, _rightArm2hj, _rightArmhj, _rightHandhj, false);

        // Same for the right side
        float RightTrigger = Input.GetAxis (RTStr);
        RightTrigger = Mathf.Approximately (RightTrigger, 0f) || Mathf.Approximately (RightTrigger, -1f) ? 0f : 1f;


        // Bend the body all together
        JointSpring tempjs = _chesthj.spring;
        tempjs.targetPosition = (Mathf.Approximately (LeftTrigger, 1f) || Mathf.Approximately (RightTrigger, 1f) ? 1f : 0f) * 90f;
        tempjs.targetPosition = Mathf.Clamp (tempjs.targetPosition, _chesthj.limits.min + 5, _chesthj.limits.max - 5);
        _chesthj.spring = tempjs;
    }

    private void CheckArmHelper (float TriggerValue, HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, bool IsLeftHand)
    {
        // Arm2: Min -90 --> 89
        JointLimits lm1 = Arm2hj.limits;
        if (IsLeftHand)
            lm1.min = Mathf.Approximately (TriggerValue, 1f) ? 89f : -90f;
        else
            lm1.max = Mathf.Approximately (TriggerValue, 1f) ? -89f : 90f;
        Arm2hj.limits = lm1;

        // Arm: Connected Mass Scale 1 --> 0
        Armhj.connectedMassScale = Mathf.Approximately (TriggerValue, 1f) ? 0f : 1f;

        //  Arm: Limits: -90, 90 --> 81, 120
        JointLimits lm = Armhj.limits;
        lm.max = Mathf.Approximately (TriggerValue, 1f) ? 120f : 90f;
        lm.min = Mathf.Approximately (TriggerValue, 1f) ? 81 : -90f;
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


    private void CheckBend ()
    {
        float VRAxis = Input.GetAxis ("XboxVerticalRight") * -1f;

        JointSpring js = _chesthj.spring;
        js.targetPosition = VRAxis * 90f;
        js.targetPosition = Mathf.Clamp (js.targetPosition, _chesthj.limits.min + 5, _chesthj.limits.max - 5);
        _chesthj.spring = js;
    }

    private void CheckJump ()
    {
        KeyCode JumpCode = KeyCode.A;
        switch (PlayerControllerNumber)
        {
            case "1":
#if UNITY_EDITOR_OSX
                JumpCode = KeyCode.Joystick1Button16;
#endif
#if UNITY_EDITOR_WIN
                JumpCode = KeyCode.Joystick1Button0;
#endif
                break;
            case "2":
#if UNITY_EDITOR_OSX
                JumpCode = KeyCode.Joystick2Button16;
#endif
#if UNITY_EDITOR_WIN
                JumpCode = KeyCode.Joystick2Button0;
#endif
                break;
            case "3":
#if UNITY_EDITOR_OSX
                JumpCode = KeyCode.Joystick3Button16;
#endif
#if UNITY_EDITOR_WIN
                JumpCode = KeyCode.Joystick3Button0;
#endif
                break;
            case "4":
#if UNITY_EDITOR_OSX
                JumpCode = KeyCode.Joystick4Button16;
#endif
#if UNITY_EDITOR_WIN
                JumpCode = KeyCode.Joystick4Button0;
#endif
                break;
        }

        if (Input.GetKeyDown (JumpCode) && IsGrounded ())
        {
            _rb.AddForce (new Vector3 (0, JumpForce, 0), ForceMode.Impulse);
        }
    }

    private void CheckMovement ()
    {
        string HLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis1";
        string VLcontrollerStr = "Joy" + PlayerControllerNumber + "Axis2";
        float HLAxis = Input.GetAxis (HLcontrollerStr);
        float VLAxis = Input.GetAxis (VLcontrollerStr);

        if (!Mathf.Approximately (HLAxis, 0f) || !Mathf.Approximately (VLAxis, 0f))
        {
            // Turn on the animator of the Leg Swing Preference
            LegSwingReference.GetComponent<Animator> ().enabled = IsGrounded ();
            // Get the percent of input force player put in
            float normalizedInputVal = Mathf.Sqrt (Mathf.Pow (HLAxis, 2f) + Mathf.Pow (VLAxis, 2f)) / Mathf.Sqrt (2);
            // Add force based on that percentage
            _rb.GetComponent<Rigidbody> ().AddForce (transform.forward * Thrust * normalizedInputVal);
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
                RotationSpeed += Time.deltaTime * 2f;
            }
            else
            {
                RotationSpeed = 4f;
            }
            RotationSpeed = Mathf.Clamp (RotationSpeed, 4f, 15f);
            Transform target = TurnReference.transform.GetChild (0);
            Vector3 relativePos = target.position - transform.position;

            TurnReference.transform.eulerAngles = new Vector3 (transform.eulerAngles.x, Mathf.Atan2 (HLAxis, VLAxis * -1f) * Mathf.Rad2Deg, transform.eulerAngles.z);
            Quaternion rotation = Quaternion.LookRotation (relativePos, Vector3.up);
            Quaternion tr = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * RotationSpeed);
            transform.rotation = tr;
        }
        else
        {
            LegSwingReference.GetComponent<Animator> ().enabled = false;
            LegSwingReference.transform.eulerAngles = Vector3.zero;
        }
    }

    #region Helper Functions
    private bool IsGrounded ()
    {
        return Physics.Raycast (transform.position, -Vector3.up, _distToGround + 0.2f, JumpMask);
    }
    #endregion
}


//// Arm2: Min -90 --> 89
//JointLimits lm1 = _leftArm2hj.limits;
//lm1.min = Mathf.Approximately(LeftTrigger, 1f) ? 89f : -90f;
//_leftArm2hj.limits = lm1;

//// Arm: Connected Mass Scale 1 --> 0
//_leftArmhj.connectedMassScale = Mathf.Approximately(LeftTrigger, 1f) ? 0f : 1f;

////  Arm: Limits: -90, 90 --> 81, 120
//JointLimits lm = _leftArmhj.limits;
//lm.max = Mathf.Approximately(LeftTrigger, 1f) ? 120f : 90f;
//lm.min = Mathf.Approximately(LeftTrigger, 1f) ? 81 : -90f;
//_leftArmhj.limits = lm;

//// Arm: Target Position: 0 --> 180
//JointSpring js = _leftArmhj.spring;
//js.targetPosition = 180f * LeftTrigger;
//_leftArmhj.spring = js;

//// Hand: Limit Max: 90 --> 0
//JointLimits tlm = _leftHandhj.limits;
//tlm.max = 90f * (1f - LeftTrigger);
//_leftHandhj.limits = tlm;