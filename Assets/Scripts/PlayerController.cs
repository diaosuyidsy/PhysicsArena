using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float AccelerationSpeed;
    public float RotateSpeed;
    public float MaxSpeed;

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
        _rb.velocity += new Vector3(HLAxis * AccelerationSpeed * Time.deltaTime, 0f, -1f * VLAxis * AccelerationSpeed * Time.deltaTime);
        if (_rb.velocity.x >= MaxSpeed)
        {
            _rb.velocity = new Vector3(MaxSpeed, _rb.velocity.y, _rb.velocity.z);
        }
        if (_rb.velocity.x <= -MaxSpeed)
        {
            _rb.velocity = new Vector3(-MaxSpeed, _rb.velocity.y, _rb.velocity.z);
        }
        if (_rb.velocity.z >= MaxSpeed)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, _rb.velocity.y, MaxSpeed);
        }
        if (_rb.velocity.z <= -MaxSpeed)
        {
            _rb.velocity = new Vector3(_rb.velocity.x, _rb.velocity.y, -MaxSpeed);
        }
    }
}
