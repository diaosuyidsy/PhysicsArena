using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //public float AccelerationSpeed = 50f;
    //public float DeccelerationSpeed;
    //public float RotateSpeed;
    public float Thrust = 20f;
    public float MaxSpeed = 10f;
    public GameObject[] Legs;
    public GameObject[] Feet;

    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float HLAxis = Input.GetAxis("XboxHorizontal");
        float VLAxis = Input.GetAxis("XboxVertical");
        float HRAxis = Input.GetAxis("XboxHorizontalRight");
        float VRAxis = Input.GetAxis("XboxVerticalRight");

        //_rb.velocity = new Vector3(HLAxis * WalkSpeed, _rb.velocity.y, -1f * VLAxis * WalkSpeed);
        //transform.Rotate(Vector3.up, Time.deltaTime * RotateSpeed * HRAxis);
        // Player's velocity is determined by accleration
        //_rb.velocity += new Vector3(HLAxis * AccelerationSpeed * Time.deltaTime, 0f, VLAxis * AccelerationSpeed * Time.deltaTime);
        //foreach (GameObject leg in Legs)
        //{
        //    leg.GetComponent<HingeJoint>().axis = new Vector3(leg.GetComponent<HingeJoint>().axis.x, leg.GetComponent<HingeJoint>().axis.y, VLAxis);
        //}
        //foreach (GameObject foot in Feet)
        //{
        //    foot.GetComponent<HingeJoint>().axis = new Vector3(foot.GetComponent<HingeJoint>().axis.x, VLAxis >= 0f ? 1f : -1f, foot.GetComponent<HingeJoint>().axis.z);
        //}
        _rb.GetComponent<Rigidbody>().AddForce(transform.forward * Thrust);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.Atan2(HLAxis, VLAxis * -1f) * Mathf.Rad2Deg, transform.eulerAngles.z);
        //if (_rb.velocity.x >= MaxSpeed)
        //{
        //    _rb.velocity = new Vector3(MaxSpeed, _rb.velocity.y, _rb.velocity.z);
        //}
        //if (_rb.velocity.x <= -MaxSpeed)
        //{
        //    _rb.velocity = new Vector3(-MaxSpeed, _rb.velocity.y, _rb.velocity.z);
        //}
        //if (_rb.velocity.z >= MaxSpeed)
        //{
        //    _rb.velocity = new Vector3(_rb.velocity.x, _rb.velocity.y, MaxSpeed);
        //}
        //if (_rb.velocity.z <= -MaxSpeed)
        //{
        //    _rb.velocity = new Vector3(_rb.velocity.x, _rb.velocity.y, -MaxSpeed);
        //}
        //if (Mathf.Approximately(HLAxis, 0f))
        //{
        //    _rb.velocity = new Vector3(0f, _rb.velocity.y, _rb.velocity.z);
        //}
        //if (Mathf.Approximately(VLAxis, 0f))
        //{
        //    _rb.velocity = new Vector3(_rb.velocity.x, _rb.velocity.y, 0f);
        //}
    }

}
