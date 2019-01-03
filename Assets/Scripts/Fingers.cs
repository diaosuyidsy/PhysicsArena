using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Fingers : MonoBehaviour
{

    public float Force = 4000;
    public GameObject OtherHand;
    public GameObject Hip;
    public float GunVerticalOffset = 0.087f;
    Rigidbody rb;

    [HideInInspector]
    public bool taken = true;
    private string[] _playercanpickuptags;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _playercanpickuptags = GameManager.GM.PlayerCanPickupTags;
        taken = true;
    }

    public void Throw()
    {
        SpringJoint thisSJ = GetComponent<SpringJoint>();
        if (thisSJ != null)
        {
            thisSJ.connectedBody = null;
            Destroy(thisSJ);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        // Make it so that you cannot stick to the ground, payload or any other players
        if (GameManager.GM.AllPlayers == (GameManager.GM.AllPlayers | (1 << col.collider.gameObject.layer)) || !_playercanpickuptags.Contains(col.collider.tag))
            return;

        if (!taken)
        {
            // Tell other necessary components that it has taken something
            OtherHand.GetComponent<Fingers>().taken = true;
            Hip.GetComponent<PlayerController>().HandTaken = true;
            GetComponentInParent<PlayerController>().HandObject = col.gameObject;

            // If it's a weapon, apply it to specific position
            if (col.collider.CompareTag("Weapon") || col.collider.CompareTag("Team1Resource") || col.collider.CompareTag("Team2Resource")
                || col.collider.CompareTag("WoodStamp"))
            {
                // Tell the collected weapon who picked it up
                col.collider.GetComponent<GunPositionControl>().Owner = Hip;
                col.collider.GetComponent<Rigidbody>().isKinematic = true;
                col.collider.gameObject.layer = gameObject.layer;
                // Change the Gun's vertical offset according to duck or chicken
                col.collider.GetComponent<GunPositionControl>().VerticalOffset = GunVerticalOffset;
            }
            else
            {
                // If picked up a stone or something, add a spring joint to it
                SpringJoint sp = gameObject.AddComponent<SpringJoint>();
                sp.connectedBody = col.rigidbody;
                sp.spring = 12000;
                sp.breakForce = Force;
            }
            PickUpItem(col.collider.tag);
            taken = true;
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

    public void SetTaken(bool _taken)
    {
        taken = _taken;
    }

}
