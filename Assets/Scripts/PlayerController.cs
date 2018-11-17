using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float Thrust = 300f;
    public float JumpForce = 300f;
    public GameObject LegSwingReference;
    public GameObject Chest;
    public GameObject LeftArm;
    public GameObject RightArm;
    public GameObject LeftHand;
    public GameObject RightHand;

    private Rigidbody _rb;
    private float _distToGround;
    private HingeJoint _chesthj;
    private HingeJoint _leftArmhj;
    private HingeJoint _rightArmhj;
    private HingeJoint _leftHandhj;
    private HingeJoint _rightHandhj;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        _chesthj = Chest.GetComponent<HingeJoint>();
        _leftArmhj = LeftArm.GetComponent<HingeJoint>();
        _rightArmhj = RightArm.GetComponent<HingeJoint>();
        _leftHandhj = LeftHand.GetComponent<HingeJoint>();
        _rightHandhj = RightHand.GetComponent<HingeJoint>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckMovement();
        CheckJump();
        CheckBend();
        CheckArm();
    }

    private void CheckArm()
    {
        float LeftTrigger = Input.GetAxis("XboxLT");
        LeftTrigger = Mathf.Approximately(LeftTrigger, 0f) || Mathf.Approximately(LeftTrigger, -1f) ? 0f : 1f;

        JointSpring js = _leftArmhj.spring;
        js.targetPosition = 100f * LeftTrigger;
        js.targetPosition = Mathf.Clamp(js.targetPosition, _leftArmhj.limits.min + 5, _leftArmhj.limits.max - 5);
        _leftArmhj.spring = js;

        float RightTrigger = Input.GetAxis("XboxRT");
        RightTrigger = Mathf.Approximately(RightTrigger, 0f) || Mathf.Approximately(RightTrigger, -1f) ? 0f : 1f;

        JointSpring js1 = _rightArmhj.spring;
        js1.targetPosition = 100f * RightTrigger;
        js1.targetPosition = Mathf.Clamp(js1.targetPosition, _rightArmhj.limits.min + 5, _rightArmhj.limits.max - 5);
        _rightArmhj.spring = js;

        //JointLimits limits = _leftHandhj.limits;
        //limits.max = 90f * (1f - LeftTrigger);
        //_leftHandhj.limits = limits;
    }

    private void CheckBend()
    {
        //float HRAxis = Input.GetAxis("XboxHorizontalRight");
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
