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
    void Start ()
    {
        rb = GetComponent<Rigidbody> ();
    }

    public void Throw ()
    {
        SpringJoint thisSJ = GetComponent<SpringJoint> ();
        if (thisSJ != null)
        {
            thisSJ.connectedBody = null;
            Destroy (thisSJ);
        }
    }

    void OnCollisionEnter (Collision col)
    {
        if (col.collider.CompareTag ("Ground"))
        {
            return;
        }
        if (!taken)
        {
            // Tell other necessary components that it has taken something
            OtherHand.GetComponent<Fingers> ().taken = true;
            Hip.GetComponent<PlayerController> ().HandTaken = true;

            // If it's a weapon, apply it to specific position
            if (col.collider.CompareTag ("Weapon"))
            {
                // Tell the collected weapon who picked it up
                col.collider.GetComponent<GunPositionControl> ().Owner = Hip;
                Destroy (col.collider.GetComponent<Rigidbody> ());
                col.gameObject.layer = 9;
            }
            else
            {
                // If picked up a stone or something, add a spring joint to it
                SpringJoint sp = gameObject.AddComponent<SpringJoint> ();
                sp.connectedBody = col.rigidbody;
                sp.spring = 12000;
                sp.breakForce = Force;
            }
            PickUpItem (col.collider.tag);
            taken = true;

        }
    }

    void OnJointBreak ()
    {
        taken = false;

    }

    void PickUpItem (string _tag)
    {
        GetComponentInParent<PlayerController> ().OnPickUpItem (_tag);
    }

}
