using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float Thrust = 300f;
    public float JumpForce = 300f;
    public GameObject LegSwingReference;
    public GameObject Chest;
    [Tooltip("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] LeftArms;
    [Tooltip("Index 0 is Arm2, 1 is Arm, 2 is Hand")]
    public GameObject[] RightArms;

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
    }

    // Update is called once per frame
    void Update()
    {
        CheckMovement();
        CheckJump();
        CheckArm();
    }


    private void CheckArm()
    {
        // For the left side
        float LeftTrigger = Input.GetAxis("XboxLT");
        LeftTrigger = Mathf.Approximately(LeftTrigger, 0f) || Mathf.Approximately(LeftTrigger, -1f) ? 0f : 1f;

        // Arm2: Min -90 --> 89
        // Arm: Connected Mass Scale 1 --> 0
        //      Target Position: 0 --> 180
        //      Limits: -90, -90 --> 81, 120
        // Hand: Limit Max: 90 --> 0

        CheckArmHelper(LeftTrigger, _leftArm2hj, _leftArmhj, _leftHandhj, true);

        // Same for the right side
        float RightTrigger = Input.GetAxis("XboxRT");
        RightTrigger = Mathf.Approximately(RightTrigger, 0f) || Mathf.Approximately(RightTrigger, -1f) ? 0f : 1f;

        CheckArmHelper(RightTrigger, _rightArm2hj, _rightArmhj, _rightHandhj, false);

        // Bend the body all together
        JointSpring tempjs = _chesthj.spring;
        tempjs.targetPosition = (Mathf.Approximately(LeftTrigger, 1f) || Mathf.Approximately(RightTrigger, 1f) ? 1f : 0f) * 90f;
        tempjs.targetPosition = Mathf.Clamp(tempjs.targetPosition, _chesthj.limits.min + 5, _chesthj.limits.max - 5);
        _chesthj.spring = tempjs;
    }

    private void CheckArmHelper(float TriggerValue, HingeJoint Arm2hj, HingeJoint Armhj, HingeJoint Handhj, bool IsLeftHand)
    {
        // Arm2: Min -90 --> 89
        JointLimits lm1 = Arm2hj.limits;
        if (IsLeftHand)
            lm1.min = Mathf.Approximately(TriggerValue, 1f) ? 89f : -90f;
        else
            lm1.max = Mathf.Approximately(TriggerValue, 1f) ? -89f : 90f;
        Arm2hj.limits = lm1;

        // Arm: Connected Mass Scale 1 --> 0
        Armhj.connectedMassScale = Mathf.Approximately(TriggerValue, 1f) ? 0f : 1f;

        //  Arm: Limits: -90, 90 --> 81, 120
        JointLimits lm = Armhj.limits;
        lm.max = Mathf.Approximately(TriggerValue, 1f) ? 120f : 90f;
        lm.min = Mathf.Approximately(TriggerValue, 1f) ? 81 : -90f;
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


    private void CheckBend()
    {
        float VRAxis = Input.GetAxis("XboxVerticalRight") * -1f;

        JointSpring js = _chesthj.spring;
        js.targetPosition = VRAxis * 90f;
        js.targetPosition = Mathf.Clamp(js.targetPosition, _chesthj.limits.min + 5, _chesthj.limits.max - 5);
        _chesthj.spring = js;
    }

    private void CheckJump()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton16) && IsGrounded())
        {
            _rb.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
        }
    }

    private void CheckMovement()
    {
        float HLAxis = Input.GetAxis("XboxHorizontal");
        float VLAxis = Input.GetAxis("XboxVertical");

        if (!Mathf.Approximately(HLAxis, 0f) || !Mathf.Approximately(VLAxis, 0f))
        {
            // Turn on the animator of the Leg Swing Preference
            LegSwingReference.GetComponent<Animator>().enabled = true;
            // Get the percent of input force player put in
            float normalizedInputVal = Mathf.Sqrt(Mathf.Pow(HLAxis, 2f) + Mathf.Pow(VLAxis, 2f)) / Mathf.Sqrt(2);
            // Add force based on that percentage
            _rb.GetComponent<Rigidbody>().AddForce(transform.forward * Thrust * normalizedInputVal);
            // Turn player according to the rotation of the joystick
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.Atan2(HLAxis, VLAxis * -1f) * Mathf.Rad2Deg, transform.eulerAngles.z);
        }
        else
        {
            LegSwingReference.GetComponent<Animator>().enabled = false;
            LegSwingReference.transform.eulerAngles = Vector3.zero;
        }
    }

    #region Helper Functions
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, _distToGround + 0.1f);
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