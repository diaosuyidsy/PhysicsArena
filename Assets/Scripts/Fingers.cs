using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fingers : MonoBehaviour
{

    public float Force = 4000;
    public GameObject OtherHand;
    public GameObject Hip;
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
            OtherHand.GetComponent<Fingers>().taken = true;
            Hip.GetComponent<PlayerController>().HandTaken = true;
            // If it's a weapon, do something else
            if (col.collider.CompareTag("Weapon"))
            {
                //col.transform.parent = GetComponentInParent<PlayerController>().HeadGunPos.transform;
                col.collider.GetComponent<GunPositionControl>().Owner = Hip;
                Destroy(col.collider.GetComponent<Rigidbody>());
                col.gameObject.layer = 9;
                taken = true;
                PickUpItem(col.collider.tag);
                return;
            }
            else
            {
                SpringJoint sp = gameObject.AddComponent<SpringJoint>();
                sp.connectedBody = col.rigidbody;
                sp.spring = 12000;
                sp.breakForce = Force;
                taken = true;
            }

        }
    }

    void OnJointBreak()
    {
        taken = false;

    }

    void PickUpItem(string _tag)
    {
        GetComponentInParent<PlayerController>().OnPickUpItem(_tag);
    }

}
