using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fingers : MonoBehaviour
{

    public float Force = 4000;
    Rigidbody rb;

    [HideInInspector]
    public bool taken = false;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Ground"))
        {
            return;
        }
        if (!taken)
        {
            SpringJoint sp = gameObject.AddComponent<SpringJoint>();
            sp.connectedBody = col.rigidbody;
            sp.spring = 12000;
            sp.breakForce = Force;
            taken = true;
        }
    }

    void OnJointBreak()
    {
        taken = false;

    }

}
